using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RM.RM_ArcMesh
{
    public class RMArcMesh
    {
        
        /// <summary>
        /// The center of the mesh. Generally this is the startpoint of the iteration.
        /// </summary>
        public Vector3 Center { get; set; }
        
        /// <summary>
        /// the vertices of the mesh.
        /// </summary>
        public Vector3[] Vertices { get; set; }
        
        /// <summary>
        /// Construct a default sphere.
        /// </summary>
        public RMArcMesh()
        {
            Center = Vector3.zero;
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="center"></param>
        /// <param name="Vertices"></param>
        public RMArcMesh(Vector3 center, Vector3[] vertices)
        {
            Center = center;
            Vertices = vertices;
        }

    }
}