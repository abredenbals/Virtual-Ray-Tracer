using System.Collections;
using _Project.Ray_Tracer.Scripts;
using _Project.Ray_Tracer.Scripts.RM;
using _Project.UI.Scripts;
using _Project.UI.Scripts.Control_Panel;
using UnityEngine;

namespace _Project.UI.Ray_Marching
{
    public class RayMarcherProperties:RayTracerProperties
    {
        private UnityRayMarcher rayMarcher;
        private RayMarchingManager rayMarchingManager;
        
        [SerializeField] protected BoolEdit showCollisionIndicatorsEdit;
        [SerializeField] protected BoolEdit showRMRaysEdit;
        [SerializeField] protected BoolEdit showRMArcsEdit;
        [SerializeField] protected BoolEdit showRMSpheres;
        
        public override void Show()
        {
            gameObject.SetActive(true);
            uiManager = UIManager.Get();
            rayMarchingManager = RayMarchingManager.RMGet();
            rayMarcher = UnityRayMarcher.RMGet();
            renderShadowsEdit.IsOn = rayMarcher.RenderShadows;
            recursionDepthEdit.Value = rayMarcher.MaxDepth;
            backgroundColorEdit.Color = rayMarcher.BackgroundColor;

            hideNoHitRaysEdit.IsOn = rayMarchingManager.HideNoHitRays;
            showRaysEdit.IsOn = rayMarchingManager.ShowRays;
            rayRadiusEdit.Value = rayMarchingManager.RayRadius;

            animateEdit.IsOn = rayMarchingManager.Animate;
            animateSequentiallyEdit.IsOn = rayMarchingManager.AnimateSequentially;
            loopEdit.IsOn = rayMarchingManager.Loop;
            speedEdit.Value = rayMarchingManager.Speed;

            superSamplingFactorEdit.Value = rayMarcher.SuperSamplingFactor;
            
            //RM specific
            showCollisionIndicatorsEdit.IsOn = rayMarchingManager.ShowCollisionIndicators;
            showRMRaysEdit.IsOn = rayMarchingManager.ShowRMRays;
            showRMArcsEdit.IsOn = rayMarchingManager.ShowRMArcs;
            showRMSpheres.IsOn = rayMarchingManager.ShowRMSpheres;
        }
        
        private void Start()
        {
            uiManager = UIManager.Get();
            rayMarchingManager = RayMarchingManager.RMGet();
            rayMarcher = UnityRayMarcher.RMGet();
            renderShadowsEdit.OnValueChanged += (value) => { RTSceneManager.Get().SetShadows(value); };
            renderShadowsEdit.OnValueChanged += (value) => { rayMarcher.RenderShadows = value; };
            recursionDepthEdit.OnValueChanged += (value) => { rayMarcher.MaxDepth = (int)value; };
            backgroundColorEdit.OnValueChanged += (value) => { rayMarcher.BackgroundColor = value; };

            hideNoHitRaysEdit.OnValueChanged += (value) => { rayMarchingManager.HideNoHitRays = value; };
            showRaysEdit.OnValueChanged += (value) => { rayMarchingManager.ShowRays = value; };
            rayRadiusEdit.OnValueChanged += (value) => { rayMarchingManager.RayRadius = value; };

            animateEdit.OnValueChanged += (value) => { rayMarchingManager.Animate = value; };
            animateSequentiallyEdit.OnValueChanged += (value) => { rayMarchingManager.AnimateSequentially = value; };
            loopEdit.OnValueChanged += (value) => { rayMarchingManager.Loop = value; };
            speedEdit.OnValueChanged += (value) => { rayMarchingManager.Speed = value; };

            superSamplingFactorEdit.OnValueChanged += (value) => { rayMarcher.SuperSamplingFactor = (int)value; };
            renderImageButton.onClick.AddListener(RenderImage);
            openImageButton.onClick.AddListener(ToggleImage);
            
            // RM specific
            showCollisionIndicatorsEdit.OnValueChanged += (value) => { rayMarchingManager.ShowCollisionIndicators = value; };
            showRMRaysEdit.OnValueChanged += (value) => { rayMarchingManager.ShowRMRays = value; };
            showRMArcsEdit.OnValueChanged += (value) => { rayMarchingManager.ShowRMArcs = value; };
            showRMSpheres.OnValueChanged += (value) => { rayMarchingManager.ShowRMSpheres = value; };
        }
        
        protected override IEnumerator RunRenderImage()
        {
            yield return new WaitForFixedUpdate();
            Texture2D render = rayMarcher.RenderImage();
            uiManager.RenderedImageWindow.SetImageTexture(render);
            yield return null;
        }

        private void Awake()
        {
            
        }
    }
}