using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Light;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Ray_Tracer.Scripts.RT_Scene.RT_Spot_Light
{
    /// <summary>
    /// Represents a light in the ray tracer scene. Requires that the attached game object has a 
    /// <see cref="UnityEngine.Light"/> component. Should be considered something like a tag to indicate to the scene
    /// manager that this light should be sent to the ray tracer. All actual information for the ray tracer is stored
    /// in the transform and light components.
    ///
    /// Because of changes made to the render engine the light color "does not" represent the actual color in the scene.
    /// The color of the light symbol in the editor can be ignored and for accurate light changes the light should
    /// be changed with the color settings in the RT Light tab instead.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Light))]
    public class RTSpotLight : RTLight
    {
        [SerializeField, Range(0, 170)]
        private float spotAngle;
        public override float SpotAngle
        {
            get => spotAngle;
            set
            {
                if (value < 0 || value > 170) return;
                light.spotAngle = value; // Do this always for Editor purposes.
                if (value == spotAngle) return;

                spotAngle = value;
                UpdateLightData();
                OnLightChangedInvoke();
            }
        }

        public override void UpdateLightData()
        {
            Color lightData;
            lightData.r = Mathf.Floor(color.r * 256)  + color.g / 2;
            lightData.g = Mathf.Floor(color.b * 256)  + (intensity / intensityDivisor);
            lightData.b = Mathf.Floor(ambient * 256)  + diffuse / 2;
            lightData.a = Mathf.Floor(specular * 256) + Mathf.Cos(spotAngle * Mathf.PI / 360f) / 2f + (lightDistanceAttenuation ? 512 : 0);
            light.color = lightData;
        }

        /// <summary>
        /// The underlying Spot<see cref="UnityEngine.Light"/> used by the spotlight.
        /// </summary>
        [SerializeField]
        private new Light light;
        
        public override LightShadows Shadows { get => light.shadows; set => light.shadows = value; }

        protected override void Awake()
        {
            Type = RTLightType.Spot;
            base.Awake();
        }

        private void LateUpdate()
        {
#if UNITY_EDITOR
                if(!Application.isPlaying) return;
#endif
            // Make the label face the camera. We do this in LateUpdate to make sure the camera has finished its moving.
            // Only rotate the canvas towards the camera in the z rotation.
            canvas.transform.up = (Camera.main.transform.position - Position).normalized;
            canvas.transform.localEulerAngles = new Vector3(0, 0, canvas.transform.localEulerAngles.z);
        }

#if UNITY_EDITOR
        private void OnRenderObject()
        {
            // Fix maximize window errors
            if (UnityEditor.SceneView.lastActiveSceneView == null) 
                return;
            // Only rotate the canvas towards the camera in the z rotation.
            canvas.transform.up = (UnityEditor.SceneView.lastActiveSceneView.camera.transform.position - Position).normalized;
            canvas.transform.localEulerAngles = new Vector3(0, 0, canvas.transform.localEulerAngles.z);
        }
#endif
    }
}