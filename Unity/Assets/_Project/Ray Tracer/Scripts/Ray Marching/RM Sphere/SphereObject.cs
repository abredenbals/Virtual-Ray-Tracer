using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RM_Sphere
{
    public class SphereObject : MonoBehaviour
    {
        private RMSphere sphere;

        public RMSphere Sphere
        {
            get { return sphere; }
            set { sphere = value; }  //Reset(); }
        }
        
        public float DrawRadius { get; private set; }
    }
}