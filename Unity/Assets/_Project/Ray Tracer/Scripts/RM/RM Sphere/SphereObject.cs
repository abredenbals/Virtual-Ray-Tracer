using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RM.RM_Sphere
{
    public class SphereObject:MonoBehaviour
    {
        private RMSphere sphere;

        public RMSphere Sphere
        {
            get { return sphere; }
            set { sphere = value; Reset();}
        }
        
        /// <summary>
        /// The radius the sphere will be drawn to (can be different to the actual radius)
        /// </summary> 
        public float DrawRadius { get; private set; }

        private RMRenderer rmRenderer;
        private RayMarchingManager rayMarchingManager;

        public void Draw()
        {
            rmRenderer.Radius = DrawRadius;
        }

        public void Draw(float radius)
        {
            rmRenderer.Radius = Mathf.Clamp(radius, 0.0f, DrawRadius);
        }
        
        private void Reset()
        {
            DetermineDrawRadius();

            rmRenderer.Center = Sphere.Center;
            rmRenderer.Radius = 0.0f;
            rmRenderer.Material = rayMarchingManager.GetSphereMaterial(Sphere.Type);
        }
        
        private void DetermineDrawRadius()
        {
            DrawRadius = float.IsInfinity(Sphere.Radius) ? rayMarchingManager.InfiniteRayDrawLength : Sphere.Radius;
        }
        
        private void Awake()
        {
            rmRenderer = GetComponent<RMRenderer>();
        }

        private void Start()
        {
            rayMarchingManager = RayMarchingManager.RMGet();
            Debug.Log(rayMarchingManager.name);
        }

        private void OnEnable()
        {
            rayMarchingManager = RayMarchingManager.RMGet();
        }
    }
}