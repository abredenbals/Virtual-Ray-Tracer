using System;
using _Project.Ray_Tracer.Scripts;
using _Project.Ray_Tracer.Scripts.RT_Scene;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Camera;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Light;
using _Project.UI.Scripts.Control_Panel;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.UI.Scripts.Ray_Marching.RM_Control_Panel
{
    /// <summary>
    /// A UI class that provides a basic panel for ray tracer, camera, light and mesh properties to be shown on.
    /// </summary>
    public class RMControlPanel : MonoBehaviour
    {
        
        public enum SignalType
        {
            RayTracer,
            Camera,
            Object
        }

        public void Subscribe(Action<SignalType> function)
        {
            rayTracerButton.onClick.AddListener(() => function(SignalType.RayTracer));
            cameraButton.onClick.AddListener(() => function(SignalType.Camera));
            objectButton.onClick.AddListener(() => function(SignalType.Object));
        }
        
        [FormerlySerializedAs("rayTracerProperties")] [SerializeField]
        private RMRayTracerProperties rmRayTracerProperties;
        [FormerlySerializedAs("cameraProperties")] [SerializeField]
        private RMCameraProperties rmCameraProperties;
        [FormerlySerializedAs("lightProperties")] [SerializeField]
        private RMLightProperties rmLightProperties;
        [FormerlySerializedAs("meshProperties")] [SerializeField]
        private RMMeshProperties rmMeshProperties;
        [SerializeField]
        private TextMeshProUGUI emptyProperties;
        [SerializeField]
        private FolderButton rayTracerButton;
        [SerializeField]
        private FolderButton cameraButton;
        [SerializeField]
        private FolderButton objectButton;

        /// <summary>
        /// Show the control panel. By default this will not show any properties, just the panel background.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Show the ray tracer properties for the current <see cref="UnityRayTracer"/> and
        /// <see cref="RayManager"/>. These properties can be changed via the shown UI.
        /// </summary>
        public void ShowRayTracerProperties()
        {
            rmCameraProperties.Hide();
            rmLightProperties.Hide();
            rmMeshProperties.Hide();
            emptyProperties.gameObject.SetActive(false);
            cameraButton.Conceal();
            objectButton.Conceal();

            Show();
            rayTracerButton.Highlight();
            rmRayTracerProperties.Show();
        }

        /// <summary>
        /// Show the camera properties for <paramref name="camera"/>. These properties can be changed via the shown UI.
        /// </summary>
        /// <param name="camera"> The <see cref="RTCamera"/> whose properties will be shown. </param>
        public void ShowCameraProperties(RTCamera camera)
        {
            rmRayTracerProperties.Hide();
            rmLightProperties.Hide();
            rmMeshProperties.Hide();
            emptyProperties.gameObject.SetActive(false);
            objectButton.Conceal();
            rayTracerButton.Conceal();

            Show();
            cameraButton.Highlight();
            rmCameraProperties.Show(camera);
        }

        /// <summary>
        /// Show the light properties for <paramref name="light"/>. These properties can be changed via the shown UI.
        /// </summary>
        /// <param name="light"> The <see cref="RTLight"/> whose properties will be shown. </param>
        public void ShowLightProperties(RTLight light)
        {
            rmRayTracerProperties.Hide();
            rmCameraProperties.Hide();
            rmMeshProperties.Hide();
            emptyProperties.gameObject.SetActive(false);
            rayTracerButton.Conceal();
            cameraButton.Conceal();
            
            Show();
            objectButton.Highlight();
            rmLightProperties.Show(light);
        }

        /// <summary>
        /// Show the mesh properties for <paramref name="mesh"/>. These properties can be changed via the shown UI.
        /// </summary>
        /// <param name="mesh"> The <see cref="RTMesh"/> whose properties will be shown. </param>
        public void ShowMeshProperties(RTMesh mesh)
        {
            rmRayTracerProperties.Hide();
            rmCameraProperties.Hide();
            rmLightProperties.Hide();
            emptyProperties.gameObject.SetActive(false);
            cameraButton.Conceal();
            rayTracerButton.Conceal();
            
            Show();
            objectButton.Highlight();
            rmMeshProperties.Show(mesh);
        }

        public void ShowEmptyProperties()
        {
            rmRayTracerProperties.Hide();
            rmCameraProperties.Hide();
            rmLightProperties.Hide();
            cameraButton.Conceal();
            rayTracerButton.Conceal();
            
            Show();
            objectButton.Highlight();
            emptyProperties.gameObject.SetActive(true);
        }

        /// <summary>
        /// Hide the control panel and any properties shown on it.
        /// </summary>
        public void Hide()
        {
            rmRayTracerProperties.Hide();
            rmCameraProperties.Hide();
            rmLightProperties.Hide();
            rmMeshProperties.Hide();
            emptyProperties.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        private void Start()
        {
            ShowRayTracerProperties();
        }
    }
}
