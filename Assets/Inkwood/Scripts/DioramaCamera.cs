using UnityEngine;
using UnityEngine.InputSystem;

namespace Inkwood
{
    [RequireComponent(typeof(Camera))]
    public sealed class DioramaCamera : MonoBehaviour
    {
        public Vector3 target = new Vector3(0, 1.2f, 0);
        [Range(20, 75)] public float elevation = 43;
        public float homeYaw = 20;
        public float homeSize = 9.4f;
        public float rotationSpeed = 0.22f;
        public Vector2 zoomLimits = new Vector2(5, 18);
        public bool showHelp = true;
        float yaw;
        Camera view;

        void OnEnable() { view = GetComponent<Camera>(); ResetView(); }
        public void ResetView()
        {
            if (!view) view = GetComponent<Camera>();
            yaw = homeYaw;
            view.orthographic = true;
            view.orthographicSize = homeSize;
            Apply();
        }
        void Update()
        {
            var mouse = Mouse.current;
            var keyboard = Keyboard.current;
            if (mouse != null)
            {
                if (mouse.rightButton.isPressed)
                    yaw += mouse.delta.ReadValue().x * rotationSpeed;
                float scroll = mouse.scroll.ReadValue().y;
                view.orthographicSize = Mathf.Clamp(view.orthographicSize - scroll * 0.008f, zoomLimits.x, zoomLimits.y);
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
        }
        void OnGUI()
        {
            if (!showHelp) return;
            var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 13 };
            style.normal.textColor = new Color(0.23f, 0.18f, 0.13f, 0.8f);
            GUI.Label(new Rect(0, Screen.height - 34, Screen.width, 26), "DRAG RIGHT MOUSE / A D  ·  ROTATE       SCROLL  ·  ZOOM       R  ·  RESET       H  ·  HIDE HELP", style);
        }
    }
}
