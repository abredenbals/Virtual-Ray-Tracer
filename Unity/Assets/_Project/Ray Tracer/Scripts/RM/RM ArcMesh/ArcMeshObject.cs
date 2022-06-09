using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RM.RM_ArcMesh
{
    public class ArcMeshObject:MonoBehaviour
    {
        private Vector3[] vertices;
        private Vector2[] uv;
        private int[] triangles;
        private Mesh arcMesh;
        
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