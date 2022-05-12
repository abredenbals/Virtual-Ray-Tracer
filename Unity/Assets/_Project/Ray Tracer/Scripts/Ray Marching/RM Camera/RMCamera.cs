using UnityEngine;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Camera;

namespace _Project.Ray_Tracer.Scripts.Ray_Marching.RM_Camera
{
    /// <summary>
    /// Represents the camera used by the ray tracer. On <see cref="Start"/> it will instantiate a visual
    /// representation of the camera and the projected screen.
    /// </summary>
    [RequireComponent(typeof(CameraCollisionMesh))]
    public class RMCamera : RTCamera
    {
        public RMCamera():base(){}
    }
}
