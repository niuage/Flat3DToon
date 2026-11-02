using UnityEngine;

namespace Inkwood
{
    public enum CharacterMotionMode { Idle, Walk }

    [RequireComponent(typeof(Animator))]
    public sealed class CharacterMotion : MonoBehaviour
    {
        public string displayName;
        public int rigVersion = 1;
        [SerializeField] CharacterMotionMode motion = CharacterMotionMode.Idle;
        [SerializeField, Range(.25f, 2)] float playbackSpeed = 1;
        [SerializeField] bool paused;
        Animator animator;
        static readonly int WalkParameter = UnityEngine.Animator.StringToHash("Walk");

        public CharacterMotionMode Motion => motion;
        public bool Paused => paused;
        public float PlaybackSpeed => playbackSpeed;
        public Animator Animator => animator ? animator : GetComponent<Animator>();

        void OnEnable()
        {
            if (!Application.IsPlaying(gameObject)) return;
            animator = GetComponent<Animator>();
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            Apply();
        }
        void Start()
        {
            // Stable phase offsets keep groups of characters from moving in unison.
            float phase = Mathf.Repeat(transform.position.x * .173f + transform.position.z * .117f, 1);
            Animator.Play(motion == CharacterMotionMode.Walk ? "Walk" : "Idle", 0, phase);
            if (Animator.layerCount > 1) Animator.Play("Blink", 1, Mathf.Repeat(phase * .731f, 1));
            Animator.Update(0);
        }
        public void SetMotion(CharacterMotionMode value) { motion = value; Apply(); }
        public void SetPaused(bool value) { paused = value; Apply(); }
        public void SetSpeed(float value) { playbackSpeed = Mathf.Clamp(value, .25f, 2); Apply(); }
        void Apply()
        {
            if (!Application.IsPlaying(gameObject)) return;
            if (!animator) animator = GetComponent<Animator>();
            if (!animator.runtimeAnimatorController) return;
            animator.SetBool(WalkParameter, motion == CharacterMotionMode.Walk);
            animator.speed = paused ? 0 : playbackSpeed;
        }
    }
}
