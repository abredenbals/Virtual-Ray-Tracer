using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RM.RM_ArcMesh
{
    /// <summary>
    /// A Unity object that visually represents the arcs visualisation in ray marching.
    /// </summary>
    public class ArcMeshObject:MonoBehaviour
    {
        private Vector3[] vertices;
        private Vector2[] uv;
        private int[] triangles;
        private Mesh arcMesh;
        
        /// <summary>
        /// The ArcMesh that this Object represents.
        /// </summary>
        public Mesh ArcMesh
        {
            get { return arcMesh; }
            set 
            { 
                arcMesh = value; 
                Reset();
                
            }
        }

        private ArcMeshRenderer arcMeshRenderer;
        private RayMarchingManager rayMarchingManager;


        /// <summary>
        /// Set the vertices that this Mesh consists of.
        /// </summary>
        /// <param name="vertices">Array of the vertices</param>
        public void Draw(Vector3[] vertices)
        {
            arcMeshRenderer.Vertices = vertices;
        }
        
        private void Reset()
        {
            //arcMeshRenderer.Center = ArcMesh.Vertices[0];
            //arcMeshRenderer.Vertices = ArcMesh.Vertices;
            arcMeshRenderer.AMesh = arcMesh;
            arcMeshRenderer.Material = rayMarchingManager.GetArcMeshMaterial();
        }

        private void Awake()
        {
            arcMeshRenderer = GetComponent<ArcMeshRenderer>();
            //GameObject arcMeshObject = new GameObject("arcMesh", typeof(MeshFilter), typeof(MeshRenderer)); //seems unnecessary (copied from the other custom objects)
        }

        private void Start()
        {
            rayMarchingManager = RayMarchingManager.RMGet();
        }

        private void OnEnable()
        {
            rayMarchingManager = RayMarchingManager.RMGet();
        }
    }
}