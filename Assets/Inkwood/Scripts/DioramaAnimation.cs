using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Inkwood
{
    public sealed class DioramaAnimation : MonoBehaviour
    {
        public CharacterMotionMode mode = CharacterMotionMode.Idle;
        [Range(.25f, 2)] public float speed = 1;
        public bool paused;
        public bool showControls = true;
        CharacterMotion[] actors;
        DioramaCamera cameraRig;

        void Start() { RefreshActors(); Apply(); }
        public void RefreshActors()
        {
            actors = FindObjectsByType<CharacterMotion>().Where(a => a.gameObject.scene == gameObject.scene).ToArray();
        }
        public void SetMode(CharacterMotionMode value) { mode = value; Apply(); }
        public void SetPaused(bool value) { paused = value; Apply(); }
        public void SetSpeed(float value) { speed = Mathf.Clamp(value, .25f, 2); Apply(); }
        public void Apply()
        {
            if (actors == null) RefreshActors();
            foreach (var actor in actors)
                if (actor) { actor.SetMotion(mode); actor.SetSpeed(speed); actor.SetPaused(paused); }
        }
        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.digit1Key.wasPressedThisFrame) SetMode(CharacterMotionMode.Idle);
            if (keyboard.digit2Key.wasPressedThisFrame) SetMode(CharacterMotionMode.Walk);
            if (keyboard.spaceKey.wasPressedThisFrame) SetPaused(!paused);
        }
        void OnGUI()
        {
            if (!showControls) return;
            if (!cameraRig && Camera.main) cameraRig = Camera.main.GetComponent<DioramaCamera>();
            if (cameraRig && !cameraRig.showHelp) return;
            MotionPreviewGui.Panel(new Rect(16, 16, 348, 84));
            MotionPreviewGui.Label(new Rect(28, 23, 320, 20), "CHARACTER MOTION  ·  1 / 2 / SPACE", true);
            if (MotionPreviewGui.Button(new Rect(28, 49, 82, 34), "Idle", mode == CharacterMotionMode.Idle)) SetMode(CharacterMotionMode.Idle);
            if (MotionPreviewGui.Button(new Rect(116, 49, 132, 34), "Walk in place", mode == CharacterMotionMode.Walk)) SetMode(CharacterMotionMode.Walk);
            if (MotionPreviewGui.Button(new Rect(254, 49, 98, 34), paused ? "Resume" : "Pause", paused)) SetPaused(!paused);
        }
    }
}
