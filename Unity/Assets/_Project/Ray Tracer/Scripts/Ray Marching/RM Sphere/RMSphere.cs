using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RM_Sphere
{
    public class RMSphere
    {
        public Vector3 Origin { get; set;}
        
        public float Radius { get; set;}
        
        public Color Color { get; set;}

        public RMSphere()
        {
            Origin = Vector3.zero;
            Radius = 0.0f;
            Color = Color.black;
        }

        
        public RMSphere(Vector3 origin, float radius, Color color)
        {
            Origin = origin;
            Radius = radius;
            Color = color;
        }
    }
}