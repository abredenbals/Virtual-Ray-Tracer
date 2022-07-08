using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.Ray_Marching.RM_ArcMesh
{
    /// <summary>
    /// Renders a ArcMesh as a 2D Mesh.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ArcMeshRenderer:MonoBehaviour
    {
        private Vector3 center;
        private Vector3[] vertices;
        private int[] triangles;
        private Mesh mesh;

        /// <summary>
        /// The center is the first vertex added, currently unused.
        /// </summary>
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

        /// <summary>
        /// The Vertices to render.
        /// </summary>
        public Vector3[] Vertices
        {
            get => vertices;
            set => vertices = value;
        }

        /// <summary>
        /// The corresponding Mesh that is created.
        /// </summary>
        public Mesh AMesh
        {
            get => mesh;
            set
            {
                if (mesh == value)
                {
                    return;
                }

                mesh = value;
                meshFilter.mesh = mesh;
            }
        }
        
        
        private Material material;
        /// <summary>
        /// The material used to draw the mesh.
        /// </summary>
        public Material Material
        {
            get => material;
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
        private MeshFilter meshFilter;

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            meshFilter = GetComponent<MeshFilter>();
        }
    }
}