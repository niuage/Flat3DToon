using UnityEngine;
using UnityEngine.InputSystem;

namespace Inkwood
{
    [RequireComponent(typeof(DioramaAnimation))]
    public sealed class CharacterAnimationStudio : MonoBehaviour
    {
        public GameObject[] characters;
        public Transform stage;
        public CharacterMotion initialActor;
        public DioramaCamera cameraRig;
        public DioramaLighting lighting;
        [SerializeField] int selected;
        CharacterMotion actor;
        DioramaAnimation director;
        public int Selected => selected;
        public CharacterMotion Actor => actor;

        void Start()
        {
            director = GetComponent<DioramaAnimation>();
            actor = initialActor;
            if (!actor) Select(selected);
            else { director.RefreshActors(); director.Apply(); }
        }
        public void Select(int index)
        {
            if (characters == null || characters.Length == 0) return;
            if (!director) director = GetComponent<DioramaAnimation>();
            selected = (index % characters.Length + characters.Length) % characters.Length;
            if (actor) { actor.gameObject.SetActive(false); Destroy(actor.gameObject); }
            actor = Instantiate(characters[selected], stage, false).GetComponent<CharacterMotion>();
            actor.transform.localPosition = Vector3.zero;
            actor.transform.localRotation = Quaternion.identity;
            director.RefreshActors(); director.Apply();
            if (lighting) lighting.SetCastShadows(lighting.CastShadows);
        }
        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.nKey.wasPressedThisFrame) Select(selected + 1);
            if (keyboard.pKey.wasPressedThisFrame) Select(selected - 1);
        }
        void OnGUI()
        {
            if (!director || !actor || (cameraRig && !cameraRig.showHelp)) return;
            // Keep the complete control panel usable in smaller Game windows.
            float scale = Mathf.Min(Screen.width / 1000f, Screen.height / 720f);
            var previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(Vector3.one * scale);
            float width = Screen.width / scale, height = Screen.height / scale;
            MotionPreviewGui.Panel(new Rect(width / 2 - 228, 16, 456, 68));
            MotionPreviewGui.Label(new Rect(width / 2 - 206, 22, 412, 20), "INKWOOD · CHARACTER ANIMATION", true);
            if (MotionPreviewGui.Button(new Rect(width / 2 - 214, 46, 72, 27), "Previous")) Select(selected - 1);
            MotionPreviewGui.Label(new Rect(width / 2 - 126, 46, 280, 27), $"{selected + 1:00} / {characters.Length:00}   {actor.displayName}");
            if (MotionPreviewGui.Button(new Rect(width / 2 + 142, 46, 72, 27), "Next")) Select(selected + 1);

            float x = width / 2 - 354, y = height - 126;
            MotionPreviewGui.Panel(new Rect(x, y, 708, 110));
            if (MotionPreviewGui.Button(new Rect(x + 12, y + 12, 70, 30), "Idle", director.mode == CharacterMotionMode.Idle)) director.SetMode(CharacterMotionMode.Idle);
            if (MotionPreviewGui.Button(new Rect(x + 88, y + 12, 118, 30), "Walk in place", director.mode == CharacterMotionMode.Walk)) director.SetMode(CharacterMotionMode.Walk);
            if (MotionPreviewGui.Button(new Rect(x + 212, y + 12, 78, 30), director.paused ? "Resume" : "Pause", director.paused)) director.SetPaused(!director.paused);
            MotionPreviewGui.Label(new Rect(x + 310, y + 12, 90, 30), $"Speed {director.speed:0.00}×");
            float speed = GUI.HorizontalSlider(new Rect(x + 408, y + 23, 112, 20), director.speed, .25f, 2);
            if (Mathf.Abs(speed - director.speed) > .001f) director.SetSpeed(speed);
            if (MotionPreviewGui.Button(new Rect(x + 540, y + 12, 156, 30), lighting && lighting.CastShadows ? "Cast shadows: on" : "Cast shadows: off", lighting && lighting.CastShadows))
                if (lighting) lighting.SetCastShadows(!lighting.CastShadows);
            MotionPreviewGui.Label(new Rect(x + 14, y + 51, 688, 25), "P / N · CHARACTER    1 / 2 · MOTION    SPACE · PAUSE    RIGHT DRAG · ROTATE    SCROLL · ZOOM");
            MotionPreviewGui.Label(new Rect(x + 14, y + 75, 688, 23), "R · RESET VIEW    S · CAST SHADOWS    H · HIDE CONTROLS");
            GUI.matrix = previousMatrix;
        }
    }
}
