using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Inkwood
{
    [ExecuteAlways]
    public sealed class DioramaLighting : MonoBehaviour
    {
        [Tooltip("Rotate this directional light to move the toon bands across the models.")]
        public Light sun;
        [SerializeField, Tooltip("Real directional shadows. Surface lighting stays enabled in both modes.")]
        bool castShadows;
        [Tooltip("Use the simple grounding ovals when real cast shadows are off.")]
        public bool groundingOvals = true;
        MeshRenderer[] surfaces;
        bool pending;
        public bool CastShadows => castShadows;

        void OnEnable() { pending = true; }
        void OnValidate() { pending = true; }
        void Update()
        {
            if (Application.isPlaying && Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame)
                SetCastShadows(!castShadows);
            if (pending) { pending = false; RefreshSurfaces(); Apply(); }
        }
        public void SetCastShadows(bool value) { castShadows = value; RefreshSurfaces(); Apply(); }
        public void RefreshSurfaces()
        {
            if (!gameObject.scene.IsValid() || !gameObject.scene.isLoaded) return;
            var result = new System.Collections.Generic.List<MeshRenderer>();
            foreach (var root in gameObject.scene.GetRootGameObjects())
                result.AddRange(root.GetComponentsInChildren<MeshRenderer>(true));
            surfaces = result.ToArray();
        }
        public void Apply()
        {
            if (!gameObject.scene.IsValid() || !gameObject.scene.isLoaded) { pending = true; return; }
            if (sun)
            {
                sun.shadows = castShadows ? LightShadows.Hard : LightShadows.None;
                sun.shadowStrength = 1;
                sun.shadowBias = 0.035f;
                sun.shadowNormalBias = 0.12f;
            }
            if (surfaces == null) RefreshSurfaces();
            if (surfaces == null) return;
            foreach (var surface in surfaces)
            {
                if (!surface) continue;
                var material = surface.sharedMaterial;
                bool toon = material && material.HasProperty("_LightingEnabled");
                bool lit = toon && material.GetFloat("_LightingEnabled") > 0.5f;
                surface.shadowCastingMode = castShadows && lit ? ShadowCastingMode.On : ShadowCastingMode.Off;
                surface.receiveShadows = toon && material.GetFloat("_ReceiveShadows") > 0.5f;
                if (surface.name == "Quiet ground shadow") surface.enabled = !castShadows && groundingOvals;
            }
        }
    }
}
