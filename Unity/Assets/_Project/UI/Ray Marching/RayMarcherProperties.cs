using System.Collections;
using _Project.Ray_Tracer.Scripts;
using _Project.Ray_Tracer.Scripts.Ray_Marching;
using _Project.Scripts;
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
            rtSceneManager = RTSceneManager.Get();
            
            renderShadowsEdit.IsOn = rayMarcher.RenderShadows;
            enablePointLightsEdit.IsOn = rtSceneManager.Scene.EnablePointLights;
            enableSpotLightsEdit.IsOn = false; // not the prettiest way
            enableAreaLightsEdit.IsOn = false;
            recursionDepthEdit.Value = rayMarcher.MaxDepth;
            backgroundColorEdit.Color = rayMarcher.BackgroundColor;

            showRaysEdit.IsOn = rayMarchingManager.ShowRays;
            hideNoHitRaysEdit.IsOn = rayMarchingManager.HideNoHitRays;
            hideNegligibleRaysEdit.IsOn = rayMarchingManager.HideNegligibleRays;
            rayHideThresholdEdit.Value = rayMarchingManager.RayHideThreshold;
            rayTransparencyEnabled.IsOn = rayMarchingManager.RayTransparencyEnabled;
            rayDynamicRadiusEnabled.IsOn = rayMarchingManager.RayDynamicRadiusEnabled;
            rayColorContributionEnabled.IsOn = rayMarchingManager.RayColorContributionEnabled;
            rayTransExponentEdit.Value = rayMarchingManager.RayTransExponent;
            rayRadiusEdit.Value = rayMarchingManager.RayRadius;
            rayMinRadiusEdit.Value = rayMarchingManager.RayMinRadius;
            rayMaxRadiusEdit.Value = rayMarchingManager.RayMaxRadius;

            animateEdit.IsOn = rayMarchingManager.Animate;
            animateSequentiallyEdit.IsOn = rayMarchingManager.AnimateSequentially;
            loopEdit.IsOn = rayMarchingManager.Loop;
            speedEdit.Value = rayMarchingManager.Speed;

            superSamplingFactorEdit.Value = rayMarcher.SuperSamplingFactor;
            superSamplingVisualEdit.IsOn = rayMarcher.SuperSamplingVisual;
            
            //RM specific
            showCollisionIndicatorsEdit.IsOn = rayMarchingManager.ShowCollisionIndicators;
            showRMRaysEdit.IsOn = rayMarchingManager.ShowRMRays;
            showRMArcsEdit.IsOn = rayMarchingManager.ShowRMArcs;
            showRMSpheres.IsOn = rayMarchingManager.ShowRMSpheres;
        }
        
        private void Start()
        {
             renderShadowsEdit.OnValueChanged.AddListener((value) => { RTSceneManager.Get().SetShadows(value); });
        }
        
        protected override IEnumerator RunRenderImage()
        {
            yield return new WaitForFixedUpdate();
            yield return rayMarcher.RenderImage();
            //Texture2D render = rayMarcher.RenderImage();
            uiManager.RenderedImageWindow.SetImageTexture(rayMarcher.Image);
            //uiManager.RenderedImageWindow.SetImageTexture(render);
            yield return null;
        }

        private void Awake()
        {
            renderShadowsEdit.OnValueChanged.AddListener((value) => { rayMarcher.RenderShadows = value; });
            enablePointLightsEdit.OnValueChanged.AddListener((value) => { rtSceneManager.Scene.EnablePointLights = value; });
            enableSpotLightsEdit.OnValueChanged.AddListener((value) => { rtSceneManager.Scene.EnableSpotLights = value; });
            enableAreaLightsEdit.OnValueChanged.AddListener((value) => { rtSceneManager.Scene.EnableAreaLights = value; });
            recursionDepthEdit.OnValueChanged.AddListener((value) => { rayMarcher.MaxDepth = (int)value; });
            backgroundColorEdit.OnValueChanged.AddListener((value) => { rayMarcher.BackgroundColor = value; });

            hideNoHitRaysEdit.OnValueChanged.AddListener((value) => { rayMarchingManager.HideNoHitRays = value; });
            showRaysEdit.OnValueChanged.AddListener((value) => { rayMarchingManager.ShowRays = value; });
            hideNegligibleRaysEdit.OnValueChanged.AddListener((value) => { rayMarchingManager.HideNegligibleRays = value; });
            rayHideThresholdEdit.OnValueChanged.AddListener((value) => { rayMarchingManager.RayHideThreshold = value; });
            rayTransparencyEnabled.OnValueChanged.AddListener((value) => { rayMarchingManager.RayTransparencyEnabled = value; });
            rayDynamicRadiusEnabled.OnValueChanged.AddListener((value) => { rayMarchingManager.RayDynamicRadiusEnabled = value; });
            rayColorContributionEnabled.OnValueChanged.AddListener((value) => { rayMarchingManager.RayColorContributionEnabled = value; });
            rayTransExponentEdit.OnValueChanged.AddListener((value) => { rayMarchingManager.RayTransExponent = value; });
            rayRadiusEdit.OnValueChanged.AddListener((value) => { rayMarchingManager.RayRadius = value; });
            rayMinRadiusEdit.OnValueChanged.AddListener((value) => { rayMarchingManager.RayMinRadius = value; });
            rayMaxRadiusEdit.OnValueChanged.AddListener((value) => { rayMarchingManager.RayMaxRadius = value; });

            animateEdit.OnValueChanged.AddListener((value) => { rayMarchingManager.Animate = value; });
            animateSequentiallyEdit.OnValueChanged.AddListener((value) => { rayMarchingManager.AnimateSequentially = value; });
            loopEdit.OnValueChanged.AddListener((value) => { rayMarchingManager.Loop = value; });
            speedEdit.OnValueChanged.AddListener((value) => { rayMarchingManager.Speed = value; });

            superSamplingFactorEdit.OnValueChanged.AddListener((value) => { rayMarcher.SuperSamplingFactor = (int)value; });
            superSamplingVisualEdit.OnValueChanged.AddListener((value) => { rayMarcher.SuperSamplingVisual = value; });
            renderImageButton.onClick.AddListener(RenderImage);
            openImageButton.onClick.AddListener(ToggleImage);
            flyRoRTCameraButton.onClick.AddListener(() =>
            { 
                showRaysEdit.IsOn = false; // This invokes the OnValueChanged event as well.
                FindObjectOfType<CameraController>().FlyToRTCamera(); // There should only be 1 CameraController.
            });
            
            // RM specific
            showCollisionIndicatorsEdit.OnValueChanged.AddListener((value) => { rayMarchingManager.ShowCollisionIndicators = value; }); 
            showRMRaysEdit.OnValueChanged.AddListener((value) => { rayMarchingManager.ShowRMRays = value; }) ;
            showRMArcsEdit.OnValueChanged.AddListener((value) => { rayMarchingManager.ShowRMArcs = value; }) ;
            showRMSpheres.OnValueChanged.AddListener((value) => { rayMarchingManager.ShowRMSpheres = value; });
        }
    }
}