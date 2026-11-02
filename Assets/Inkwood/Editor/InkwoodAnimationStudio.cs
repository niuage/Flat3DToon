using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Inkwood.Editor
{
    public static partial class InkwoodAnimationAuthoring
    {
        [MenuItem("Inkwood/Build Animation Studio")]
        public static void BuildStudio()
        {
            RequireSavedScenes();
            foreach (string key in CharacterKeys)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Inkwood/Prefabs/" + key + ".prefab");
                if (!prefab || !prefab.GetComponent<CharacterMotion>()) throw new System.InvalidOperationException("Install character animation before building the studio.");
            }
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var ground = new GameObject("Meadow backdrop", typeof(MeshFilter), typeof(MeshRenderer));
            ground.GetComponent<MeshFilter>().sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Inkwood/Meshes/box.asset");
            ground.GetComponent<MeshRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Inkwood/Materials/Meadow.mat");
            ground.transform.position = new Vector3(0, -.17f, 0); ground.transform.localScale = new Vector3(100, .3f, 100);
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(DioramaCamera)); cameraObject.tag = "MainCamera";
            var camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.82f, .86f, .49f);
            camera.nearClipPlane = .1f; camera.farClipPlane = 100; camera.allowHDR = false; camera.allowMSAA = true;
            var data = camera.GetUniversalAdditionalCameraData(); data.renderPostProcessing = true;
            data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing; data.antialiasingQuality = AntialiasingQuality.High;
            var orbit = cameraObject.GetComponent<DioramaCamera>();
            orbit.target = new Vector3(0, 1.2f, 0); orbit.elevation = 25; orbit.homeYaw = -12;
            orbit.homeSize = 1.9f; orbit.referenceAspect = 1.6f; orbit.zoomLimits = new Vector2(1.2f, 5);
            orbit.showCameraControls = false; orbit.ResetView();
            var volume = new GameObject("Palette preservation", typeof(Volume)).GetComponent<Volume>();
            volume.isGlobal = true; volume.priority = 100;
            volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>("Assets/Inkwood/Settings/PaletteProfile.asset");
            var lighting = new GameObject("Toon lighting", typeof(DioramaLighting)).GetComponent<DioramaLighting>();
            var sun = new GameObject("Sun", typeof(Light)).GetComponent<Light>(); sun.transform.SetParent(lighting.transform);
            sun.type = LightType.Directional; sun.intensity = 1; sun.transform.rotation = Quaternion.Euler(50, -40, 0);
            lighting.sun = sun; RenderSettings.sun = sun; RenderSettings.fog = false;
            RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = Color.white;
            var director = new GameObject("Animation controls", typeof(DioramaAnimation)).GetComponent<DioramaAnimation>();
            director.showControls = false;
            var studio = director.gameObject.AddComponent<CharacterAnimationStudio>();
            studio.characters = new GameObject[CharacterKeys.Length];
            for (int i = 0; i < CharacterKeys.Length; i++) studio.characters[i] = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Inkwood/Prefabs/" + CharacterKeys[i] + ".prefab");
            studio.stage = new GameObject("Character stage").transform;
            studio.initialActor = ((GameObject)PrefabUtility.InstantiatePrefab(studio.characters[0], studio.stage)).GetComponent<CharacterMotion>();
            studio.cameraRig = orbit; studio.lighting = lighting; lighting.SetCastShadows(false);
            EditorSceneManager.SaveScene(scene, "Assets/Inkwood/Scenes/AnimationStudio.unity"); AssetDatabase.SaveAssets();
            Selection.activeGameObject = director.gameObject;
        }
    }
}
