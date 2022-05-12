using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.Ray_Marching.RM_Ray
{
    /// <summary>
    /// A Unity object that visually represent a ray traced by the ray tracer.
    /// </summary>
    [RequireComponent(typeof(RMRayRenderer))]
    public class RMRayObject : MonoBehaviour
    {
        private RMRay ray;
        /// <summary>
        /// The <see cref="RMRay"/> produced by the ray tracer that this ray object represents.
        /// </summary>
        public RMRay Ray 
        { 
            get { return ray; }
            set { ray = value; Reset(); }
        }

        /// <summary>
        /// The length to which this ray object is drawn. Generally this is the same as the length of <see cref="Ray"/>,
        /// but if the ray is infinitely long the drawn length will be set to
        /// <see cref="RayManager.InfiniteRayDrawLength"/>.
        /// </summary> 
        public float DrawLength { get; private set; }

        private RMRayRenderer rmRayRenderer;
        private RayMarchingManager rayManager;

        /// <summary>
        /// Draw the ray as a cylinder where <paramref name="radius"/> determines the drawn radius of the cylinder. The
        /// length of the cylinder is the same as the ray's true length unless that length is infinity, then the length is
        /// clamped to <see cref="RayManager.InfiniteRayDrawLength"/>.
        /// </summary>
        /// <param name="radius"> The drawn radius of the cylinder. </param>
        public void Draw(float radius)
        {
            rmRayRenderer.Radius = radius;
            rmRayRenderer.Length = DrawLength;
        }

        /// <summary>
        /// Draw the ray as a cylinder where <paramref name="radius"/> determines the drawn radius of the cylinder. The
        /// length of the cylinder is given by <paramref name="length"/>, but it is clamped between 0 and
        /// <see cref="DrawLength"/>.
        /// </summary>
        /// <param name="radius"> The drawn radius of the cylinder. </param>
        /// <param name="length"> The drawn length of the cylinder. Clamped between 0 and <see cref="DrawLength"/> </param>
        public void Draw(float radius, float length)
        {
            rmRayRenderer.Radius = radius;
            rmRayRenderer.Length = Mathf.Clamp(length, 0.0f, DrawLength);
        }

        private void Reset()
        {
            DetermineDrawLength();

            rmRayRenderer.Origin = Ray.Origin;
            rmRayRenderer.Direction = Ray.Direction;
            rmRayRenderer.Length = 0.0f;
            rmRayRenderer.Material = rayManager.GetRayTypeMaterial(Ray.Type);
        }

        private void DetermineDrawLength()
        {
            DrawLength = float.IsInfinity(Ray.Length) ? rayManager.InfiniteRayDrawLength : Ray.Length;
        }

        private void Awake()
        {
            rmRayRenderer = GetComponent<RMRayRenderer>();
        }

        private void Start()
        {
            rayManager = RayMarchingManager.Get();
        }

        private void OnEnable()
        {
            rayManager = RayMarchingManager.Get();
        }
    }
}
