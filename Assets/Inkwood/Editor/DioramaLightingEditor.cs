using UnityEditor;
using UnityEngine;

namespace Inkwood.Editor
{
    [CustomEditor(typeof(DioramaLighting))]
    public sealed class DioramaLightingEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Both modes use the scene light and separate lit/shadow colors. Rotate the Sun to move the bands. In Play Mode, press S to compare cast shadows.", MessageType.Info);
            var lighting = (DioramaLighting)target;
            int mode = GUILayout.Toolbar(lighting.CastShadows ? 1 : 0, new[] { "No cast shadows", "Cast shadows" });
            if ((mode == 1) != lighting.CastShadows)
            {
                Undo.RecordObject(lighting, "Change toon shadow mode");
                lighting.SetCastShadows(mode == 1);
                EditorUtility.SetDirty(lighting);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(lighting.gameObject.scene);
                SceneView.RepaintAll();
            }
            EditorGUILayout.Space();
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject,"m_Script","castShadows");
            serializedObject.ApplyModifiedProperties();
        }
    }
}
