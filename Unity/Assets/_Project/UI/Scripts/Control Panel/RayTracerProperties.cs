using System.Collections;
using _Project.Ray_Tracer.Scripts;
using _Project.Ray_Tracer.Scripts.RM;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Project.UI.Scripts.Control_Panel
{
    /// <summary>
    /// A UI class that provides access to the properties of the current <see cref="UnityRayTracer"/> and
    /// <see cref="RayManager"/>. Any changes made to the shown properties will be applied to the ray tracer and ray
    /// manager.
    /// </summary>
    public class RayTracerProperties : MonoBehaviour
    {
        private UnityRayTracer rayTracer;
        private RayManager rayManager;
        protected UIManager uiManager;

        [SerializeField]
        protected BoolEdit renderShadowsEdit;
        [SerializeField]
        protected FloatEdit recursionDepthEdit;
        [SerializeField]
        protected ColorEdit backgroundColorEdit;

        [SerializeField]
        protected BoolEdit hideNoHitRaysEdit;
        [SerializeField]
        protected BoolEdit showRaysEdit;
        [SerializeField]
        protected FloatEdit rayRadiusEdit;

        [SerializeField]
        protected BoolEdit animateEdit;
        [SerializeField]
        protected BoolEdit animateSequentiallyEdit;
        [SerializeField]
        protected BoolEdit loopEdit;
        [SerializeField]
        protected FloatEdit speedEdit;

        [SerializeField]
        protected FloatEdit superSamplingFactorEdit;
        [SerializeField]
        protected Button renderImageButton;
        [SerializeField]
        protected Button openImageButton;

        /// <summary>
        /// Show the ray tracer properties for the current <see cref="UnityRayTracer"/> and <see cref="RayManager"/>.
        /// These properties can be changed via the shown UI.
        /// </summary>
        public virtual void Show()
        {
            gameObject.SetActive(true);
            uiManager = UIManager.Get();
            if (SceneManager.GetActiveScene().name == "Ray Marching")
            {
                rayManager = RayMarchingManager.RMGet();
                rayTracer = UnityRayMarcher.RMGet();
            }
            else
            {
                rayManager = RayManager.Get();
                rayTracer = UnityRayTracer.Get();
            }
            renderShadowsEdit.IsOn = rayTracer.RenderShadows;
            recursionDepthEdit.Value = rayTracer.MaxDepth;
            backgroundColorEdit.Color = rayTracer.BackgroundColor;

            hideNoHitRaysEdit.IsOn = rayManager.HideNoHitRays;
            showRaysEdit.IsOn = rayManager.ShowRays;
            rayRadiusEdit.Value = rayManager.RayRadius;

            animateEdit.IsOn = rayManager.Animate;
            animateSequentiallyEdit.IsOn = rayManager.AnimateSequentially;
            loopEdit.IsOn = rayManager.Loop;
            speedEdit.Value = rayManager.Speed;

            superSamplingFactorEdit.Value = rayTracer.SuperSamplingFactor;
        }

        /// <summary>
        /// Hide the shown ray tracer properties.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        protected IEnumerator RunRenderImage()
        {
            yield return new WaitForFixedUpdate();
            Texture2D render = rayTracer.RenderImage();
            uiManager.RenderedImageWindow.SetImageTexture(render);
            yield return null;
        }

        protected void RenderImage()
        {
            uiManager.RenderedImageWindow.Show();
            uiManager.RenderedImageWindow.SetLoading();
            StartCoroutine(RunRenderImage());
        }

        protected void ToggleImage()
        {
            uiManager.RenderedImageWindow.Toggle();
        }

        // TODO overhaul object order in levels and dependencies. It's becoming a bit difficult to get the right order 
        // TODO code wise. Objects should ideally set there own values on awake and do everything else on start.
        private void Start()
        {
            renderShadowsEdit.OnValueChanged += (value) => { RTSceneManager.Get().SetShadows(value); };
        }
        
        private void Awake()
        {
            renderShadowsEdit.OnValueChanged += (value) => { rayTracer.RenderShadows = value; };
            recursionDepthEdit.OnValueChanged += (value) => { rayTracer.MaxDepth = (int)value; };
            backgroundColorEdit.OnValueChanged += (value) => { rayTracer.BackgroundColor = value; };

            hideNoHitRaysEdit.OnValueChanged += (value) => { rayManager.HideNoHitRays = value; };
            showRaysEdit.OnValueChanged += (value) => { rayManager.ShowRays = value; };
            rayRadiusEdit.OnValueChanged += (value) => { rayManager.RayRadius = value; };

            animateEdit.OnValueChanged += (value) => { rayManager.Animate = value; };
            animateSequentiallyEdit.OnValueChanged += (value) => { rayManager.AnimateSequentially = value; };
            loopEdit.OnValueChanged += (value) => { rayManager.Loop = value; };
            speedEdit.OnValueChanged += (value) => { rayManager.Speed = value; };

            superSamplingFactorEdit.OnValueChanged += (value) => { rayTracer.SuperSamplingFactor = (int)value; };
            renderImageButton.onClick.AddListener(RenderImage);
            openImageButton.onClick.AddListener(ToggleImage);
        }
    }
}
