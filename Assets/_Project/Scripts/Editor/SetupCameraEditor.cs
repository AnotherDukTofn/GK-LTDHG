using UnityEngine;
using UnityEditor;
using SpellStrike.Camera;
using SpellStrike.Player;

namespace SpellStrike.Editor
{
    public class SetupCameraEditor
    {
        [MenuItem("SpellStrike/Setup Isometric Camera")]
        public static void SetupCamera()
        {
            var cam = UnityEngine.Camera.main;
            if (cam == null)
            {
                Debug.LogError("No Main Camera found!");
                return;
            }

            var player = Object.FindFirstObjectByType<PlayerController>();
            if (player == null)
            {
                Debug.LogError("No PlayerController found in scene!");
                return;
            }

            var follow = cam.gameObject.GetComponent<CameraFollow>();
            if (follow == null)
            {
                follow = cam.gameObject.AddComponent<CameraFollow>();
            }

            // Dùng reflection hoặc public method
            var serializedObject = new SerializedObject(follow);
            serializedObject.FindProperty("m_Target").objectReferenceValue = player.transform;
            serializedObject.FindProperty("m_UseFixedRotation").boolValue = true;
            serializedObject.FindProperty("m_FixedRotation").vector3Value = new Vector3(30f, 45f, 0f);
            serializedObject.FindProperty("m_IsOrthographic").boolValue = true;
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(cam.gameObject);
            Debug.Log($"Camera '{cam.name}' đã được cấu hình Isometric Top-down và follow '{player.name}'!");
        }
    }
}
