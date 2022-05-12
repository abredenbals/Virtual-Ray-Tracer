using System.Collections.Generic;
using _Project.Ray_Tracer.Scripts.RT_Ray;
using _Project.Ray_Tracer.Scripts.Utility;
using _Project.Ray_Tracer.Scripts.Ray_Marching.RM_Ray;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Ray_Tracer.Scripts.Ray_Marching
{
    /// <summary>
    /// Manages the visible rays in the Unity scene. Gets new rays from the ray tracer each frame and draws them.
    /// </summary>
    public class RayMarchingManager : MonoBehaviour
    {
        [Header("Render Settings")]

        [SerializeField, Range(0.0f, 0.1f)]
        private float rayRadius = 0.01f;
        /// <summary>
        /// The radius of the rays this ray manager draws.
        /// </summary>
        public float RayRadius
        {
            get { return rayRadius; }
            set { rayRadius = value; }
        }

        [SerializeField, Range(0.0f, 25.0f)]
        private float infiniteRayDrawLength = 10.0f;
        /// <summary>
        /// The length with which this ray manager draws rays that do not intersect an object.
        /// </summary>
        public float InfiniteRayDrawLength
        {
            get { return infiniteRayDrawLength; }
            set { infiniteRayDrawLength = value; }
        }

        [SerializeField]
        private RMRayObject rayPrefab;
        [Range(0, 1024)]
        private int initialRayPoolSize = 64;

        /// <summary>
        /// Whether this ray manager hides rays that do not intersect an object.
        /// </summary>
        public bool HideNoHitRays;
        
        /// <summary>
        /// Whether this ray manager hides all rays it would normally draw. When this is <c>false</c>, all ray drawing
        /// and animation code will be skipped.
        /// </summary>
        public bool ShowRays;

        [SerializeField] private Material noHitMaterial;
        [SerializeField] private Material reflectMaterial;
        [SerializeField] private Material refractMaterial;
        [SerializeField] private Material normalMaterial;
        [SerializeField] private Material shadowMaterial;
        [SerializeField] private Material lightMaterial;
        [SerializeField] private Material errorMaterial;

        [Header("Animation Settings")]

        [SerializeField]
        private bool animate = false;
        /// <summary>
        /// Whether this ray manager animates the rays it draws.
        /// </summary>
        public bool Animate
        {
            get { return animate; }
            set
            {
                Reset = animate != value; // Reset the animation if we changed the value.
                animate = value;
            }
        }

        [SerializeField]
        private bool animateSequentially = false;
        /// <summary>
        /// Whether this ray manager animates the rays sequentially. Does nothing if <see cref="Animate"/> is not set.
        /// </summary>
        public bool AnimateSequentially
        {
            get { return animateSequentially; }
            set
            {
                Reset = animateSequentially != value; // Reset the animation if we changed the value.
                animateSequentially = value;
            }
        }

        [SerializeField]
        private bool loop = false;
        /// <summary>
        /// Whether this ray manager loops its animation. Does nothing if <see cref="Animate"/> is not set.
        /// </summary>
        public bool Loop
        {
            get { return loop; }
            set { loop = value; }
        }

        [SerializeField]
        private bool reset = false;
        /// <summary>
        /// Whether this ray manager will reset its animation on the next frame. After resetting this variable will be
        /// set to <c>false</c> again. Does nothing if <see cref="Animate"/> is not set.
        /// </summary>
        public bool Reset
        {
            get { return reset; }
            set { reset = value; }
        }

        [SerializeField, Range(0.0f, 10.0f)]
        private float speed = 1.0f;
        /// <summary>
        /// The speed of this ray manager's animation. Does nothing if <see cref="Animate"/> is not set.
        /// </summary>
        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        private static RayMarchingManager instance = null;

        private List<TreeNode<RMRay>> rays;
        private RMRayObjectPool rayObjectPool;

        private TreeNode<RMRay> selectedRay;
        private Vector2Int selectedRayCoordinates;
        private bool hasSelectedRay = false;

        private RMSceneManager rmSceneManager;
        private UnityRayMarcher rayMarcher;
        
        private float distanceToDraw = 0.0f;
        private int rayTreeToDraw = 0; // Used when animating sequentially.
        private bool animationDone = false;

        private bool shouldUpdateRays = true;

        /// <summary>
        /// Get the current <see cref="RayMarchingManager"/> instance.
        /// </summary>
        /// <returns> The current <see cref="RayMarchingManager"/> instance. </returns>
        public static RayMarchingManager Get()
        {
            return instance;
        }

        /// <summary>
        /// Get the colors of the root rays managed by this <see cref="RayMarchingManager"/>.
        /// </summary>
        /// <returns> A <see cref="Color"/><c>[]</c> of from rays at the root of their ray tree. </returns>
        public Color[] GetRayColors()
        {
            Color[] colors = new Color[rays.Count];
            for (int i = 0; i < rays.Count; i++)
            {
                colors[i] = rays[i].Data.Color;
                colors[i].a = 1.0f; // The ray tracer messes up the alpha channel, but it should always be 1.
            }
            return colors;
        }

        /// <summary>
        /// Get the material used to render rays of the given <see cref="RMRay.RayType"/>.
        /// </summary>
        /// <param name="type"> What type of ray to get the material for. </param>
        /// <returns> 
        /// The <see cref="Material"/> for <paramref name="type"/>. The material for <see cref="RMRay.RayType.NoHit"/>
        /// is returned if <paramref name="type"/> is not a recognized <see cref="RMRay.RayType"/>.
        /// </returns>
        public Material GetRayTypeMaterial(RMRay.RayType type)
        {
            switch (type)
            {
                case RMRay.RayType.NoHit:
                    return noHitMaterial;
                case RMRay.RayType.Reflect:
                    return reflectMaterial;
                case RMRay.RayType.Refract:
                    return refractMaterial;
                case RMRay.RayType.Normal:
                    return normalMaterial;
                case RMRay.RayType.Shadow:
                    return shadowMaterial;
                case RMRay.RayType.Light:
                    return lightMaterial;
                default:
                    Debug.LogError("Unrecognized ray type " + type + "!");
                    return errorMaterial;
            }
        }

        public void SelectRay(Vector2Int rayCoordinates)
        {
            selectedRayCoordinates = rayCoordinates;
            hasSelectedRay = true;
            Reset = true;
        }

        public void DeselectRay()
        {
            hasSelectedRay = false;
            Reset = true;
        }

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            rays = new List<TreeNode<RMRay>>();
            rayObjectPool = new RMRayObjectPool(rayPrefab, initialRayPoolSize, transform);
            Reset = true;

            rmSceneManager = RMSceneManager.Get();
            rayMarcher = UnityRayMarcher.Get();

            rmSceneManager.Scene.OnSceneChanged += () => { shouldUpdateRays = true; };
            rayMarcher.OnRayTracerChanged += () => { shouldUpdateRays = true; };
        }

        private void FixedUpdate()
        {
            rayObjectPool.SetAllUnused();
            
            if(shouldUpdateRays)
                UpdateRays();

            // Determine the selected ray.
            if (hasSelectedRay)
            {
                int width = rmSceneManager.Scene.Camera.ScreenWidth;
                int index = selectedRayCoordinates.x + width * selectedRayCoordinates.y;
                selectedRay = rays[index];
            }

            if (Animate)
                DrawRaysAnimated();
            else
                DrawRays();
            
            rayObjectPool.DeactivateUnused();
        }

        /// <summary>
        /// Get new ray trees from the ray tracer.
        /// </summary>
        public void UpdateRays()
        {
            rays = rayMarcher.Render();
            rmSceneManager.UpdateImage(GetRayColors());
            shouldUpdateRays = false;
        }

        /// <summary>
        /// Draw <see cref="rays"/> in full.
        /// </summary>
        private void DrawRays()
        {
            if (!ShowRays)
                return;

            // If we have selected a ray we only draw its ray tree.
            if (hasSelectedRay)
            {
                DrawRayTree(selectedRay);
            }
            // Otherwise we draw all ray trees.
            else
            {
                foreach (var rayTree in rays)
                    DrawRayTree(rayTree);
            }

        }

        private void DrawRayTree(TreeNode<RMRay> rayTree)
        {
            if (HideNoHitRays && rayTree.Data.Type == RMRay.RayType.NoHit)
                return;

            RMRayObject rayObject = rayObjectPool.GetRayObject();
            rayObject.Ray = rayTree.Data;
            rayObject.Draw(RayRadius);

            if (!rayTree.IsLeaf())
            {
                foreach (var child in rayTree.Children)
                    DrawRayTree(child);
            }
        }

        /// <summary>
        /// Draw a part of <see cref="rays"/> up to <see cref="distanceToDraw"/>. The part drawn grows each frame.
        /// </summary>
        private void DrawRaysAnimated()
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
                    animationDone = DrawRayTreeAnimated(selectedRay, distanceToDraw);
                }
                // If specified we animate the ray trees sequentially (pixel by pixel).
                else if (animateSequentially)
                {
                    // Draw all previous ray trees in full.
                    for (int i = 0; i < rayTreeToDraw; ++i)
                        DrawRayTree(rays[i]);

                    // Animate the current ray tree. If it is now fully drawn we move on to the next one.
                    bool treeDone = DrawRayTreeAnimated(rays[rayTreeToDraw], distanceToDraw);
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
                    foreach (var rayTree in rays)
                        animationDone &= DrawRayTreeAnimated(rayTree, distanceToDraw);
                }
            }
            // Otherwise we can just draw all rays in full.
            else
                DrawRays();
        }

        private bool DrawRayTreeAnimated(TreeNode<RMRay> rayTree, float distance)
        {
            if (HideNoHitRays && rayTree.Data.Type == RMRay.RayType.NoHit)
                return true;

            RMRayObject rayObject = rayObjectPool.GetRayObject();
            rayObject.Ray = rayTree.Data;
            rayObject.Draw(RayRadius, distance);
            
            float leftover = distance - rayObject.DrawLength;

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
    }
}
