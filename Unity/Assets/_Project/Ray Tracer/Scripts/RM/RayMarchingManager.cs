using System;
using System.Collections.Generic;
using _Project.Ray_Tracer.Scripts.RM.RM_ArcMesh;
using _Project.Ray_Tracer.Scripts.RM.RM_Sphere;
using _Project.Ray_Tracer.Scripts.RT_Ray;
using _Project.Ray_Tracer.Scripts.Utility;
using UnityEngine;
using SphereObjectPool = _Project.Ray_Tracer.Scripts.RM.RM_Sphere.SphereObjectPool;

namespace _Project.Ray_Tracer.Scripts.RM
{
    public class RayMarchingManager:RayManager
    {
        public bool HideSpheres; // in case there will be too much clutter
        
        [SerializeField] private SphereObject spherePrefab;
        [SerializeField] private ArcMeshObject arcMeshPrefab;
        
        
        [SerializeField] private Material sphereRayMaterial;
        [SerializeField] private Material sphereCollisionMaterial;
        [SerializeField] private Material sphereExpandingMaterial;
        [SerializeField] private Material sphereErrorMaterial;
        [SerializeField] private Material rmCollisionMaterial;
        [SerializeField] private Material rmArcMaterial;
        [SerializeField] private Material rmArcMeshMaterial;
        
        private static RayMarchingManager instance = null;
        private UnityRayMarcher rayMarcher;
        private TreeNode<List<(float, Vector3)>> selectedcollisionDistance;
        private List<TreeNode<List<(float, Vector3)>>> collisionDistances;
        private SphereObjectPool sphereObjectPool;
        private ArcMeshObjectPool arcMeshObjectPool;

        // showing/hiding Ray Marching visualizations
        [SerializeField]
        protected bool showCollisionIndicators = false;
        /// <summary>
        /// Whether this ray manager animates the collision indicators.
        /// </summary>
        public bool ShowCollisionIndicators
        {
            get { return showCollisionIndicators; }
            set
            {
                Reset = showCollisionIndicators != value; // Reset the animation if we changed the value.
                showCollisionIndicators = value;
            }
        }
        
        [SerializeField]
        protected bool showRMRays = false;
        /// <summary>
        /// Whether this ray manager animates the collision indicators.
        /// </summary>
        public bool ShowRMRays
        {
            get { return showRMRays; }
            set
            {
                Reset = showRMRays != value; // Reset the animation if we changed the value.
                showRMRays = value;
            }
        }
        
        [SerializeField]
        protected bool showRMSpheres = false;
        /// <summary>
        /// Whether this ray manager animates the collision indicators.
        /// </summary>
        public bool ShowRMSpheres
        {
            get { return showRMSpheres; }
            set
            {
                Reset = showRMSpheres != value; // Reset the animation if we changed the value.
                showRMSpheres = value;
            }
        }

        public static RayMarchingManager RMGet()
        {
            return instance;
        }

        public Material GetArcMeshMaterial()
        {
            return rmArcMeshMaterial;
        }

        public Material GetSphereMaterial(RMSphere.SphereType type)
        {
            switch (type)
            {
                case RMSphere.SphereType.RayIndicator:
                    return sphereRayMaterial;
                case RMSphere.SphereType.CollisionIndicator:
                    return sphereCollisionMaterial;
                case RMSphere.SphereType.ExpandingVisualization:
                    return sphereExpandingMaterial;
                default:
                    Debug.LogError("Unrecognized sphere type!");
                    return sphereErrorMaterial;
            }
        }
        

        
        private void FixedUpdate()
        {
            rayObjectPool.SetAllUnused();
            sphereObjectPool.SetAllUnused();
            arcMeshObjectPool.SetAllUnused();
            
            if(shouldUpdateRays)
                UpdateRays();

            // Determine the selected ray.
            if (hasSelectedRay)
            {
                int width = rtSceneManager.Scene.Camera.ScreenWidth;
                int index = selectedRayCoordinates.x + width * selectedRayCoordinates.y;
                selectedRay = rays[index];
                selectedcollisionDistance = collisionDistances[index];
            }

            if (Animate)
                DrawRaysAnimated();
            else
                DrawRays();
            
            rayObjectPool.DeactivateUnused();
            sphereObjectPool.DeactivateUnused();
            arcMeshObjectPool.DeactivateUnused();
        }
        
        public override void UpdateRays()
        {
            rays = rayMarcher.Render(out collisionDistances);
            rtSceneManager.UpdateImage(GetRayColors());
            shouldUpdateRays = false;
        }
        
        
        /// <summary>
        /// Draw <see cref="rays"/> in full.
        /// </summary>
        protected override void DrawRays()
        {
            if (!ShowRays)
                return;

            // If we have selected a ray we only draw its ray tree.
            if (hasSelectedRay)
            {
                DrawRayTree(selectedRay, selectedcollisionDistance);
            }
            // Otherwise we draw all ray trees.
            else
            {
                for (int i = 0; i < rays.Count; i++)
                {
                    DrawRayTree(rays[i], collisionDistances[i]);
                }
            }

        }
        
        protected void DrawRayTree(TreeNode<RTRay> rayTree, TreeNode<List<(float, Vector3)>> collisionDistance)
        {
            if (HideNoHitRays && rayTree.Data.Type == RTRay.RayType.NoHit) // TODO: do similar for the indicators below (add checkboxes in gui)
                return;

            RayObject rayObject = rayObjectPool.GetRayObject();
            rayObject.Ray = rayTree.Data;
            rayObject.Draw(RayRadius);
            
            // Ray marching specific visualization
            Vector3 origin = rayObject.Ray.Origin;
            Vector3 direction = rayObject.Ray.Direction;
            for (int i = 0; i < collisionDistance.Data.Count; i++)
                //foreach (var collisionPair in collisionDistance.Data)
            {
                var collisionPair = collisionDistance.Data[i]; // Item1 = Distance from the camera, Item2 = collision point
                Vector3 center = origin + direction * collisionPair.Item1;
                if (ShowCollisionIndicators)
                {
                    // The indicator on the ray
                    SphereObject sphereObject = sphereObjectPool.GetSphereObject();
                    sphereObject.Sphere = new RMSphere(center, RMSphere.SphereType.RayIndicator);
                    sphereObject.Draw();
                
                    // The indicator on the nearest object
                    sphereObject = sphereObjectPool.GetSphereObject();
                    sphereObject.Sphere = new RMSphere(collisionPair.Item2, RMSphere.SphereType.CollisionIndicator);
                    sphereObject.Draw();
                }

                if (ShowRMRays)
                {
                    // Slim rays connecting the two
                    RayObject connection = rayObjectPool.GetRayObject();
                    connection.Ray = new RTRay(center, collisionPair.Item2 - center, Vector3.Distance(collisionPair.Item2, center), new Color(0,0,0),
                        RTRay.RayType.RMCollision); 
                    connection.Draw(RayRadius * 0.5f);
                }

                if (i < collisionDistance.Data.Count - 1)
                {
                    DrawArc(center, collisionPair.Item2, origin + direction *collisionDistance.Data[i+1].Item1, 0.05f);
                }
            }
            
            if (!rayTree.IsLeaf())
            {
                foreach (var child in rayTree.Children)
                    DrawRayTree(child);
            }
        }
        
        /// <summary>
        /// Draw a part of <see cref="rays"/> up to <see cref="distanceToDraw"/>. The part drawn grows each frame.
        /// </summary>
        protected override void DrawRaysAnimated()
        {
            if (!ShowRays)
                return;

            // Reset the animation if we are looping or if a reset was requested.
            if (animationDone && Loop || Reset)
            {
                distanceToDraw = 0.0f;
                rayTreeToDraw = 0;
                animationDone = false;
                Reset = false;
            }

            // Animate all ray trees if we are not done animating already.
            if (!animationDone)
            {
                distanceToDraw += Speed * Time.deltaTime;
                animationDone = true; // Will be reset to false if one tree is not finished animating.

                // If we have selected a ray we only draw its ray tree.
                if (hasSelectedRay)
                {
                    animationDone = DrawRayTreeAnimated(selectedRay, distanceToDraw, selectedcollisionDistance);
                }
                // If specified we animate the ray trees sequentially (pixel by pixel).
                else if (animateSequentially)
                {
                    // Draw all previous ray trees in full.
                    for (int i = 0; i < rayTreeToDraw; ++i)
                        DrawRayTree(rays[i]);

                    // Animate the current ray tree. If it is now fully drawn we move on to the next one.
                    bool treeDone = DrawRayTreeAnimated(rays[rayTreeToDraw], distanceToDraw, collisionDistances[rayTreeToDraw]);
                    if (treeDone)
                    {
                        distanceToDraw = 0.0f;
                        ++rayTreeToDraw;
                    }

                    animationDone = treeDone && rayTreeToDraw >= rays.Count;
                }
                // Otherwise we animate all ray trees.
                else
                {
                    for (int i = 0; i < rays.Count; i++)
                    {
                        animationDone &= DrawRayTreeAnimated(rays[i], distanceToDraw, collisionDistances[i]);
                    }
                    //foreach (var rayTree in rays)
                    //    animationDone &= DrawRayTreeAnimated(rayTree, distanceToDraw);
                }
            }
            // Otherwise we can just draw all rays in full.
            else
                DrawRays();
        }

        protected bool DrawRayTreeAnimated(TreeNode<RTRay> rayTree, float distance,  TreeNode<List<(float, Vector3)>> collisionDistance)
        {
            if (HideNoHitRays && rayTree.Data.Type == RTRay.RayType.NoHit)
                return true;

            RayObject rayObject = rayObjectPool.GetRayObject();
            rayObject.Ray = rayTree.Data;
            rayObject.Draw(RayRadius, distance);
            
            float leftover = distance - rayObject.DrawLength;

            // Ray Marching specific
            Vector3 origin = rayObject.Ray.Origin;
            Vector3 direction = rayObject.Ray.Direction;
            
            for (int i = 0; i < collisionDistance.Data.Count; i++)
            //foreach (var collisionPair in collisionDistance.Data)
            {
                var collisionPair = collisionDistance.Data[i];
                // the starting point of this iteration (on the  ray)
                Vector3 center = origin + direction * collisionPair.Item1;
                
                // The expanding sphere visualization
                if (showRMSpheres && i < collisionDistance.Data.Count - 1 && collisionDistance.Data[i+1].Item1 >= distance)
                {
                    SphereObject sphereObject = sphereObjectPool.GetSphereObject();
                    sphereObject.Sphere = new RMSphere(center, 2 * (distance - collisionDistance.Data[i].Item1), RMSphere.SphereType.ExpandingVisualization);
                    sphereObject.Draw();
                }

                if (showCollisionIndicators)
                {
                    // The indicator on the ray
                    SphereObject sphereObject = sphereObjectPool.GetSphereObject();
                    sphereObject.Sphere = new RMSphere(center, RMSphere.SphereType.RayIndicator);
                    sphereObject.Draw();
                }

                if (ShowRMRays)
                {
                    // Slim rays connecting the two
                    RayObject connection = rayObjectPool.GetRayObject();
                    connection.Ray = new RTRay(center, collisionPair.Item2 - center, Vector3.Distance(collisionPair.Item2, center), new Color(0,0,0),
                        RTRay.RayType.RMCollision);
                    connection.Draw(RayRadius * 0.5f, distance - collisionPair.Item1);
                }
                
                // check, if the collision is near enough to be drawn
                if (i < collisionDistance.Data.Count - 1 && collisionDistance.Data[i+1].Item1 >= distance)
                {
                    break;
                }

                if (showCollisionIndicators)
                {
                    // The indicator on the nearest object
                    SphereObject sphereObject = sphereObjectPool.GetSphereObject();
                    sphereObject.Sphere = new RMSphere(collisionPair.Item2, RMSphere.SphereType.CollisionIndicator);
                    sphereObject.Draw();
                }
                
                if (i < collisionDistance.Data.Count - 1)
                {
                    DrawArc(center, collisionPair.Item2, origin + direction *collisionDistance.Data[i+1].Item1, 0.05f);
                }

            }
            
            // If this ray is not at its full length we are not done animating.
            if (leftover <= 0.0f)
                return false;
            // If this ray is at its full length and has no children we are done animating.
            if (rayTree.IsLeaf())
                return true;

            // Otherwise we start animating the children.
            bool done = true;
            foreach (var child in rayTree.Children)
                done &= DrawRayTreeAnimated(child, leftover);
            return done;
        }

        private void DrawArc(Vector3 center, Vector3 start, Vector3 finish, float segmentThreshold)
        {
            float distance = Vector3.Distance(start,finish);
            Vector3 arcVector = finish - start;
            if (distance > segmentThreshold)
            {
                Vector3 halfPoint = start + arcVector * 0.5f;
                float halfPointDistance = Vector3.Distance(halfPoint, center);
                halfPoint = center + Vector3.Normalize(halfPoint - center) * Vector3.Distance(start, center);
                
                // to not make too many arc pieces
                if (segmentThreshold * 100f <= distance)
                {
                    segmentThreshold = distance / 100f;
                }
                DrawArc(center ,start,halfPoint,segmentThreshold * 2);
                DrawArc(center ,halfPoint, finish, segmentThreshold * 2);
            }
            else
            {
                RayObject arc = rayObjectPool.GetRayObject();
                arc.Ray = new RTRay(start, arcVector, Vector3.Magnitude(arcVector), new Color(0, 0, 0),
                    RTRay.RayType.RMArc);
                arc.Draw(RayRadius * 0.5f);
                ArcMeshObject arcMeshObject = arcMeshObjectPool.GetArcMeshObject();
                Vector3[] vertices = new Vector3[3]{center, finish, start};
                Mesh arcMesh = new Mesh();
                arcMesh.vertices = vertices;
                arcMesh.triangles = new []{0,1,2};
                arcMeshObject.ArcMesh = arcMesh;
            }
        }

        public override Material GetRayTypeMaterial(RTRay.RayType type)
        {
            switch (type)
            {
                case RTRay.RayType.NoHit:
                    return noHitMaterial;
                case RTRay.RayType.Reflect:
                    return reflectMaterial;
                case RTRay.RayType.Refract:
                    return refractMaterial;
                case RTRay.RayType.Normal:
                    return normalMaterial;
                case RTRay.RayType.Shadow:
                    return shadowMaterial;
                case RTRay.RayType.Light:
                    return lightMaterial;
                case RTRay.RayType.RMCollision:
                    return rmCollisionMaterial;
                case RTRay.RayType.RMArc:
                    return rmArcMaterial;
                default:
                    Debug.LogError("Unrecognized ray type " + type + "!");
                    return errorMaterial;
            }
        }

        private void Awake()
        {
            instance = this;
        }
        
        private void Start()
        {
            rays = new List<TreeNode<RTRay>>();
            rayObjectPool = new RayObjectPool(rayPrefab, initialRayPoolSize, transform);
            sphereObjectPool = new SphereObjectPool(spherePrefab, initialRayPoolSize);
            arcMeshObjectPool = new ArcMeshObjectPool(arcMeshPrefab, initialRayPoolSize);
            Reset = true;

            rtSceneManager = RTSceneManager.Get();
            rayMarcher = UnityRayMarcher.RMGet();
            
            rtSceneManager.Scene.OnSceneChanged += () => { shouldUpdateRays = true; };
            rayMarcher.OnRayTracerChanged += () => { shouldUpdateRays = true; };
        }
        
    }
}