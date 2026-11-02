using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Inkwood.Editor
{
    public static partial class InkwoodAnimationAuthoring
    {
        public const string AnimationRoot = "Assets/Inkwood/Animations";
        public static readonly string[] CharacterKeys = { "Wizard", "BucketKnight", "Ranger", "Pilgrim", "Monk", "SkeletonGuard", "Goblinswordsman", "Goblinshieldbearer", "Goblinarcher", "Goblinbannershaman" };
        public static readonly string[] CharacterNames = { "Wizard", "Bucket knight", "Ranger", "Pilgrim", "Monk", "Skeleton guard", "Goblin swordsman", "Goblin shieldbearer", "Goblin archer", "Goblin banner shaman" };

        static Transform Pivot(string name, Transform parent, Vector3 localPosition)
        {
            var result = new GameObject(name).transform; result.SetParent(parent, false); result.localPosition = localPosition; return result;
        }
        static string Normal(string value) => new string(value.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
        static bool Any(string text, params string[] terms) => terms.Any(text.Contains);
        static Vector3 Center(Transform part, Transform root)
        {
            var renderers = part.GetComponentsInChildren<MeshRenderer>(true);
            if (renderers.Length == 0) return root.InverseTransformPoint(part.position);
            var bounds = renderers[0].bounds;
            foreach (var r in renderers.Skip(1)) bounds.Encapsulate(r.bounds);
            return root.InverseTransformPoint(bounds.center);
        }
        static string Section(Transform part, Transform root, string key)
        {
            string n = Normal(part.name); bool goblin = key.StartsWith("Goblin", StringComparison.Ordinal);
            var center = Center(part, root);
            if (Any(n, "leftleg", "leftboneleg", "bentleftleg")) return "LeftLeg";
            if (Any(n, "rightleg", "rightboneleg", "bentrightleg")) return "RightLeg";
            if (key == "Goblinbannershaman" && n == "ivoryskull") return "RightArm";
            if (Any(n, "quiver", "arrowshaft", "fletching")) return "BackGear";
            if (Any(n, "strap", "belt", "cowl", "robe", "tunic", "surcoat", "habit", "shirt", "spine"))
            {
                if (!Any(n, "staffhandsleeve")) return "Body";
            }
            if (Any(n, "book", "satchel", "hangingrope", "ropeknot")) return "Gear";
            if (Any(n, "shield", "buckler", "flower")) return goblin ? "RightArm" : "LeftArm";
            if (Any(n, "sword", "blade", "guard") && !Any(n, "beard", "beardedaxe")) return goblin ? "LeftArm" : "RightArm";
            if (Any(n, "staff", "axe", "spear", "banner")) return "RightArm";
            if (Any(n, "bow", "nockedarrow")) return goblin ? "RightArm" : "LeftArm";
            if (Any(n, "helmet", "helm", "head", "face", "nose", "ear", "tooth", "mustache", "beard", "hair", "fringe", "turban", "cap", "feather", "socket", "doteye", "hat", "ivoryskull")) return "Head";
            if (Any(n, "leftsleeve", "leftarm", "raisedwidesleeve")) return "LeftArm";
            if (Any(n, "rightsleeve", "rightarm")) return "RightArm";
            if (key == "BucketKnight" && center.y > 1.1f && center.z < -.24f) return "Head";
            if (goblin && center.y > 1.05f && Mathf.Abs(center.x) < .28f && center.z < -.19f) return "Head";
            if (key == "Wizard" && center.y > 1 && center.z < -.32f && Mathf.Abs(center.x) < .15f) return "Head";
            if (center.y > 1.24f && Mathf.Abs(center.x) < .42f) return "Head";
            if (Mathf.Abs(center.x) > .30f && center.y > .6f) return center.x < 0 ? "LeftArm" : "RightArm";
            return "Body";
        }
        static void AddMonkLegs(GameObject root)
        {
            if (root.transform.Cast<Transform>().Any(t => Normal(t.name).Contains("leftleg"))) return;
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Inkwood/Meshes/cylinder.asset");
            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Inkwood/Materials/WoodDark.mat");
            for (int i = 0; i < 2; i++)
            {
                var leg = new GameObject(i == 0 ? "Little left leg" : "Little right leg", typeof(MeshFilter), typeof(MeshRenderer));
                leg.transform.SetParent(root.transform, false); leg.transform.localPosition = new Vector3(i == 0 ? -.14f : .14f, .15f, 0);
                leg.transform.localScale = new Vector3(.075f, .25f, .075f); leg.GetComponent<MeshFilter>().sharedMesh = mesh;
                leg.GetComponent<MeshRenderer>().sharedMaterial = material; leg.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            }
        }
        public static void RigCharacter(GameObject root, string key)
        {
            if (root.GetComponent<CharacterMotion>()) return;
            if (!CharacterKeys.Contains(key)) throw new ArgumentException("Unknown Inkwood character: " + key);
            if (key == "Monk") AddMonkLegs(root);
            var parts = root.transform.Cast<Transform>().Where(t => t.name != "Quiet ground shadow").ToArray();
            var sections = parts.ToDictionary(t => t, t => Section(t, root.transform, key));
            var rig = Pivot("Motion Rig", root.transform, Vector3.zero);
            var body = Pivot("Body", rig, new Vector3(0, .68f, 0));
            var bones = new Dictionary<string, Transform>
            {
                { "Body", body }, { "Head", Pivot("Head", body, new Vector3(0, .64f, 0)) },
                { "LeftArm", Pivot("LeftArm", body, new Vector3(-.18f, .35f, 0)) },
                { "RightArm", Pivot("RightArm", body, new Vector3(.18f, .35f, 0)) },
                { "Gear", Pivot("Gear", body, new Vector3(0, -.02f, .05f)) },
                { "BackGear", Pivot("BackGear", body, new Vector3(.25f, .28f, .20f)) },
                { "LeftLeg", Pivot("LeftLeg", rig, new Vector3(-.13f, .53f, 0)) },
                { "RightLeg", Pivot("RightLeg", rig, new Vector3(.13f, .53f, 0)) }
            };
            foreach (var part in parts) part.SetParent(bones[sections[part]], true);
            EnsureFolder(AnimationRoot);
            var animator = root.AddComponent<Animator>(); animator.applyRootMotion = false; animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.runtimeAnimatorController = CreateController(root, key, rig, bones);
            var motion = root.AddComponent<CharacterMotion>(); motion.displayName = CharacterNames[Array.IndexOf(CharacterKeys, key)];
        }
        static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            int slash = folder.LastIndexOf('/'); EnsureFolder(folder.Substring(0, slash)); AssetDatabase.CreateFolder(folder.Substring(0, slash), folder.Substring(slash + 1));
        }
        static AnimationClip Clip(string key, string action, float length)
        {
            string path = AnimationRoot + "/" + key + "_" + action + ".anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (!clip) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, path); }
            clip.ClearCurves(); clip.name = key + "_" + action; clip.frameRate = 30; clip.wrapMode = WrapMode.Loop;
            var settings = AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime = true; settings.loopBlend = true; AnimationUtility.SetAnimationClipSettings(clip, settings);
            return clip;
        }
        static void Curve(AnimationClip clip, GameObject root, Transform target, string property, float length, params float[] values)
        {
            var curve = new AnimationCurve();
            for (int i = 0; i < values.Length; i++) curve.AddKey(length * i / (values.Length - 1), values[i]);
            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
            }
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(AnimationUtility.CalculateTransformPath(target, root.transform), typeof(Transform), property), curve);
        }
        static void Angle(AnimationClip clip, GameObject root, Transform bone, float length, float x, float y, float z)
        {
            Curve(clip, root, bone, "localEulerAnglesRaw.x", length, x, 0, -x, 0, x);
            Curve(clip, root, bone, "localEulerAnglesRaw.y", length, y, 0, -y, 0, y);
            Curve(clip, root, bone, "localEulerAnglesRaw.z", length, z, 0, -z, 0, z);
        }
        static AnimatorController CreateController(GameObject root, string key, Transform rig, Dictionary<string, Transform> b)
        {
            string controllerPath = AnimationRoot + "/" + key + ".controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            // Rebuilding geometry preserves any artist edits to the existing clips.
            if (controller) return controller;
            bool goblin = key.StartsWith("Goblin", StringComparison.Ordinal);
            float duration = goblin ? .78f : key == "Wizard" ? 1.08f : .96f;
            var idle = Clip(key, "Idle", 4); var walk = Clip(key, "Walk", duration);
            Curve(idle, root, rig, "m_LocalPosition.y", 4, 0, 0, 0, 0, 0);
            Curve(idle, root, b["Body"], "m_LocalPosition.y", 4, .68f, .686f, .68f, .674f, .68f);
            Curve(idle, root, b["Body"], "m_LocalScale.y", 4, 1, 1.012f, 1, .994f, 1);
            Angle(idle, root, b["Body"], 4, 0, .35f, .35f); Angle(idle, root, b["Head"], 4, .6f, 2.5f, 1.3f);
            Angle(idle, root, b["LeftArm"], 4, 1.2f, 0, .8f); Angle(idle, root, b["RightArm"], 4, -1.2f, 0, -.8f);
            Angle(idle, root, b["Gear"], 4, 0, 0, 1); Angle(idle, root, b["BackGear"], 4, .4f, 0, -.5f);
            foreach (string leg in new[] { "LeftLeg", "RightLeg" })
            {
                Angle(idle, root, b[leg], 4, 0, 0, 0); Curve(idle, root, b[leg], "m_LocalPosition.y", 4, .53f, .53f);
            }
            Curve(walk, root, rig, "m_LocalPosition.y", duration, 0, .024f, 0, .024f, 0);
            Curve(walk, root, b["Body"], "m_LocalPosition.y", duration, .68f, .68f);
            Curve(walk, root, b["Body"], "m_LocalScale.y", duration, 1, 1);
            Angle(walk, root, b["Body"], duration, .7f, 2.5f, 1.8f); Angle(walk, root, b["Head"], duration, -1.2f, -1.8f, -1.1f);
            Angle(walk, root, b["LeftLeg"], duration, -24, 0, 0); Angle(walk, root, b["RightLeg"], duration, 24, 0, 0);
            Curve(walk, root, b["LeftLeg"], "m_LocalPosition.y", duration, .53f, .57f, .53f, .53f, .53f);
            Curve(walk, root, b["RightLeg"], "m_LocalPosition.y", duration, .53f, .53f, .53f, .57f, .53f);
            float left = key == "Ranger" ? 5 : key == "Wizard" ? 5 : 10;
            float right = key == "Wizard" || key == "Goblinbannershaman" || key == "SkeletonGuard" ? 5 : 9;
            Angle(walk, root, b["LeftArm"], duration, left, 0, 1.3f); Angle(walk, root, b["RightArm"], duration, -right, 0, -1.3f);
            Angle(walk, root, b["Gear"], duration, 0, 0, -5); Angle(walk, root, b["BackGear"], duration, 1, 0, 2);
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath); controller.AddParameter("Walk", AnimatorControllerParameterType.Bool);
            var sm = controller.layers[0].stateMachine;
            var idleState = sm.AddState("Idle"); idleState.motion = idle; idleState.writeDefaultValues = false;
            var walkState = sm.AddState("Walk"); walkState.motion = walk; walkState.writeDefaultValues = false; sm.defaultState = idleState;
            var go = idleState.AddTransition(walkState); go.hasExitTime = false; go.duration = .18f; go.hasFixedDuration = true; go.AddCondition(AnimatorConditionMode.If, 0, "Walk");
            var back = walkState.AddTransition(idleState); back.hasExitTime = false; back.duration = .18f; back.hasFixedDuration = true; back.AddCondition(AnimatorConditionMode.IfNot, 0, "Walk");
            var eyes = b["Head"].GetComponentsInChildren<Transform>(true).Where(t => t.name == "Left dot eye" || t.name == "Right dot eye").ToArray();
            if (eyes.Length > 0)
            {
                var blink = Clip(key, "Blink", 4);
                foreach (var eye in eyes)
                {
                    float y = eye.localScale.y;
                    var curve = new AnimationCurve(new Keyframe(0, y), new Keyframe(2.35f, y), new Keyframe(2.42f, y * .1f), new Keyframe(2.49f, y * .1f), new Keyframe(2.57f, y), new Keyframe(4, y));
                    AnimationUtility.SetEditorCurve(blink, EditorCurveBinding.FloatCurve(AnimationUtility.CalculateTransformPath(eye, root.transform), typeof(Transform), "m_LocalScale.y"), curve);
                }
                var blinkMachine = new AnimatorStateMachine { name = "Blink" }; AssetDatabase.AddObjectToAsset(blinkMachine, controller);
                var blinkState = blinkMachine.AddState("Blink"); blinkState.motion = blink; blinkState.writeDefaultValues = false; blinkMachine.defaultState = blinkState;
                controller.AddLayer(new AnimatorControllerLayer { name = "Blink", defaultWeight = 1, stateMachine = blinkMachine });
            }
            EditorUtility.SetDirty(idle); EditorUtility.SetDirty(walk); EditorUtility.SetDirty(controller); return controller;
        }
        [MenuItem("Inkwood/Add Character Animation")]
        public static void InstallAnimations()
        {
            RequireSavedScenes();
            foreach (var key in CharacterKeys)
            {
                string path = "Assets/Inkwood/Prefabs/" + key + ".prefab";
                var root = PrefabUtility.LoadPrefabContents(path);
                try { RigCharacter(root, key); PrefabUtility.SaveAsPrefabAsset(root, path); }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            EnsureDirector(SceneManager.GetActiveScene());
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
        public static void RequireSavedScenes()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save every open scene before changing animation assets.");
        }
        public static DioramaAnimation EnsureDirector(Scene scene)
        {
            var director = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<DioramaAnimation>(true)).FirstOrDefault();
            if (!director) director = new GameObject("09 · Character motion", typeof(DioramaAnimation)).GetComponent<DioramaAnimation>();
            return director;
        }
    }
}
