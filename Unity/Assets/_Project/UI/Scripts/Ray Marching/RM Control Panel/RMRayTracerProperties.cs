using System.Collections;
using _Project.Ray_Tracer.Scripts;
using _Project.Ray_Tracer.Scripts.Ray_Marching;
using _Project.UI.Scripts.Control_Panel;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Project.UI.Scripts.Ray_Marching.RM_Control_Panel
{
    /// <summary>
    /// A UI class that provides access to the properties of the current <see cref="UnityRayTracer"/> and
    /// <see cref="RayManager"/>. Any changes made to the shown properties will be applied to the ray tracer and ray
    /// manager.
    /// </summary>
    public class RMRayTracerProperties : MonoBehaviour
    {
        private UnityRayMarcher rayMarcher;
        private RayMarchingManager rayManager;
        private UIManager uiManager;

        [SerializeField]
        private Control_Panel.BoolEdit renderShadowsEdit;
        [SerializeField]
        private Control_Panel.FloatEdit recursionDepthEdit;
        [FormerlySerializedAs("backgroundColorEdit")] [SerializeField]
        private Control_Panel.ColorEdit backgroundRmColorEdit;

        [SerializeField]
        private Control_Panel.BoolEdit hideNoHitRaysEdit;
        [SerializeField]
        private Control_Panel.BoolEdit showRaysEdit;
        [SerializeField]
        private Control_Panel.FloatEdit rayRadiusEdit;

        [SerializeField]
        private BoolEdit animateEdit;
        [SerializeField]
        private BoolEdit animateSequentiallyEdit;
        [SerializeField]
        private BoolEdit loopEdit;
        [SerializeField]
        private FloatEdit speedEdit;

        [SerializeField]
        private FloatEdit superSamplingFactorEdit;
        [SerializeField]
        private Button renderImageButton;
        [SerializeField]
        private Button openImageButton;

        /// <summary>
        /// Show the ray tracer properties for the current <see cref="UnityRayTracer"/> and <see cref="RayManager"/>.
        /// These properties can be changed via the shown UI.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            rayMarcher = UnityRayMarcher.Get();
            rayManager = RayMarchingManager.Get();
            uiManager = UIManager.Get();

            renderShadowsEdit.IsOn = rayMarcher.RenderShadows;
            recursionDepthEdit.Value = rayMarcher.MaxDepth;
            backgroundRmColorEdit.Color = rayMarcher.BackgroundColor;

            hideNoHitRaysEdit.IsOn = rayManager.HideNoHitRays;
            showRaysEdit.IsOn = rayManager.ShowRays;
            rayRadiusEdit.Value = rayManager.RayRadius;

            animateEdit.IsOn = rayManager.Animate;
            animateSequentiallyEdit.IsOn = rayManager.AnimateSequentially;
            loopEdit.IsOn = rayManager.Loop;
            speedEdit.Value = rayManager.Speed;

            superSamplingFactorEdit.Value = rayMarcher.SuperSamplingFactor;
        }

        /// <summary>
        /// Hide the shown ray tracer properties.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private IEnumerator RunRenderImage()
        {
            yield return new WaitForFixedUpdate();
            Texture2D render = rayMarcher.RenderImage();
            uiManager.RenderedImageWindow.SetImageTexture(render);
            yield return null;
        }

        private void RenderImage()
        {
            uiManager.RenderedImageWindow.Show();
            uiManager.RenderedImageWindow.SetLoading();
            StartCoroutine(RunRenderImage());
        }

        private void ToggleImage()
        {
            uiManager.RenderedImageWindow.Toggle();
        }

        // TODO overhaul object order in levels and dependencies. It's becoming a bit difficult to get the right order 
        // TODO code wise. Objects should ideally set there own values on awake and do everything else on start.
        private void Start()
        {
            renderShadowsEdit.OnValueChanged += (value) => { RMSceneManager.Get().SetShadows(value); };
        }
        
        private void Awake()
        {
            renderShadowsEdit.OnValueChanged += (value) => { rayMarcher.RenderShadows = value; };
            recursionDepthEdit.OnValueChanged += (value) => { rayMarcher.MaxDepth = (int)value; };
            backgroundRmColorEdit.OnValueChanged += (value) => { rayMarcher.BackgroundColor = value; };

            hideNoHitRaysEdit.OnValueChanged += (value) => { rayManager.HideNoHitRays = value; };
            showRaysEdit.OnValueChanged += (value) => { rayManager.ShowRays = value; };
            rayRadiusEdit.OnValueChanged += (value) => { rayManager.RayRadius = value; };

            animateEdit.OnValueChanged += (value) => { rayManager.Animate = value; };
            animateSequentiallyEdit.OnValueChanged += (value) => { rayManager.AnimateSequentially = value; };
            loopEdit.OnValueChanged += (value) => { rayManager.Loop = value; };
            speedEdit.OnValueChanged += (value) => { rayManager.Speed = value; };

            superSamplingFactorEdit.OnValueChanged += (value) => { rayMarcher.SuperSamplingFactor = (int)value; };
            renderImageButton.onClick.AddListener(RenderImage);
            openImageButton.onClick.AddListener(ToggleImage);
        }
    }
}
