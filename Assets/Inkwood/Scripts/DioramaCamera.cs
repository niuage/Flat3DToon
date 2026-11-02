using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Inkwood
{
    [RequireComponent(typeof(Camera))]
    [ExecuteAlways]
    public sealed class DioramaCamera : MonoBehaviour
    {
        public Vector3 target = new Vector3(0, 1.2f, 0);
        [Range(20, 75)] public float elevation = 43;
        public float homeYaw = 20;
        public float homeSize = 8;
        [Tooltip("Keep the wide reference composition visible in narrower Game views.")]
        public float referenceAspect = 2.4f;
        public float rotationSpeed = 0.22f;
        public Vector2 zoomLimits = new Vector2(5, 18);
        public bool showHelp = true;
        public bool showCameraControls = true;
        float yaw;
        float zoomSize;
        Camera view;
        DioramaLighting lighting;

        void OnEnable()
        {
            view = GetComponent<Camera>(); ResetView();
            RenderPipelineManager.beginCameraRendering += BeforeRender;
        }
        void OnDisable() { RenderPipelineManager.beginCameraRendering -= BeforeRender; }
        void BeforeRender(ScriptableRenderContext context, Camera camera) { if(camera == view) FitProjection(); }
        void FitProjection() { view.orthographicSize = zoomSize * Mathf.Max(1, referenceAspect / Mathf.Max(view.aspect,.1f)); }
        public void ResetView()
        {
            if (!view) view = GetComponent<Camera>();
            yaw = homeYaw;
            view.orthographic = true;
            zoomSize = homeSize;
            Apply();
        }
        void Update()
        {
            if (!Application.isPlaying) { if(view) FitProjection(); return; }
            var mouse = Mouse.current;
            var keyboard = Keyboard.current;
            if (mouse != null)
            {
                if (mouse.rightButton.isPressed)
                    yaw += mouse.delta.ReadValue().x * rotationSpeed;
                float scroll = mouse.scroll.ReadValue().y;
                zoomSize = Mathf.Clamp(zoomSize - scroll * 0.008f, zoomLimits.x, zoomLimits.y);
            }
            if (keyboard != null)
            {
                if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) yaw -= Time.unscaledDeltaTime * 45;
                if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) yaw += Time.unscaledDeltaTime * 45;
                if (keyboard.rKey.wasPressedThisFrame) ResetView();
                if (keyboard.hKey.wasPressedThisFrame) showHelp = !showHelp;
            }
            Apply();
        }
        void Apply()
        {
            var rotation = Quaternion.Euler(elevation, yaw, 0);
            transform.SetPositionAndRotation(target - rotation * Vector3.forward * 55, rotation);
            FitProjection();
        }
        void OnGUI()
        {
            if (!Application.isPlaying || !showHelp || !showCameraControls) return;
            if(!lighting) lighting=Object.FindAnyObjectByType<DioramaLighting>();
            string shadowState=lighting&&lighting.CastShadows?"ON":"OFF";
            var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 13 };
            style.normal.textColor = new Color(0.23f, 0.18f, 0.13f, 0.8f);
            GUI.Label(new Rect(0, Screen.height - 34, Screen.width, 26), $"RIGHT DRAG / A D · ROTATE     SCROLL · ZOOM     R · RESET     S · SHADOWS {shadowState}     H · HIDE HELP", style);
        }
    }
}
