using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.Ray_Marching.RM_Sphere
{
    public class RMSphere
    {
        
        /// <summary>
        /// The types of spheres.
        /// </summary>
        public enum SphereType
        {
            RayIndicator,
            CollisionIndicator,
            ExpandingVisualization
        }
        /// <summary>
        /// The type of this sphere.
        /// </summary>
        public SphereType Type { get; set; }
        
        /// <summary>
        /// The center of the Sphere. Generally this is the camera position.
        /// </summary>
        public Vector3 Center { get; set; }

        /// <summary>
        /// The radius of this sphere. Will represent the distance to the nearest object from the center
        /// </summary>
        public float Radius { get; set; }
        
        /// <summary>
        /// Construct a default sphere.
        /// </summary>
        public RMSphere()
        {
            Center = Vector3.zero;
            Radius = 0.0f;
            Type = SphereType.RayIndicator;
        }
        
        /// <summary>
        /// Construction of a new sphere with radius 0.1
        /// </summary>
        /// <param name="center"></param> Center of the sphere.
        /// <param name="type"></param> type of sphere.
        public RMSphere(Vector3 center, SphereType type)
        {
            Center = center;
            Radius = 0.05f;
            Type = type;
        }

        /// <summary>
        /// Construction of a new sphere
        /// </summary>
        /// <param name="center"></param> Center of the sphere.
        /// <param name="radius"></param> Radius of the sphere.
        /// <param name="type"></param> type of sphere.
        public RMSphere(Vector3 center, float radius, SphereType type)
        {
            Center = center;
            Radius = radius;
            Type = type;
        }
    }
}