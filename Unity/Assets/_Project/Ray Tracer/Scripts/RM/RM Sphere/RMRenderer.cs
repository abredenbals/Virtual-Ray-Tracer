using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RM.RM_Sphere
{
    
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class RMRenderer:MonoBehaviour
    {
        private Vector3 center;
        private float radius;

        public Vector3 Center
        {
            get => center;
            set
            {
                // Because we often reset the origin to the same value this check improves performance.
                if (center == value)
                    return;

                center = value;
                transform.position = center;
            }
        }

        public float Radius
        {
            get => radius;
            set
            {
                // Because we often reset the origin to the same value this check improves performance.
                if (radius == value)
                    return;

                radius = value;
                transform.localScale = new Vector3(radius,radius,radius);
            }
        }
        
        
        private Material material;
        /// <summary>
        /// The material used to draw the sphere.
        /// </summary>
        public Material Material
        {
            get { return material; }
            set
            {
                // Because we often reset the material to the same value this check improves performance.
                if (material == value)
                    return;

                material = value;
                meshRenderer.material = material;
            }
        }

        private MeshRenderer meshRenderer;

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }
    }
}