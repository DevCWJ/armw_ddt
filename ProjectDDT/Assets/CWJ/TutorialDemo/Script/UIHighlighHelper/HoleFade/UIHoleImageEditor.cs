#if UNITY_EDITOR
namespace Abu
{
    using UnityEditor;
    using UnityEditor.UI;

    [CustomEditor(typeof(UIHoleFadeImage))]
    public class UIHoleImageEditor : ImageEditor
    {
        SerializedProperty holeSizeProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            holeSizeProperty = serializedObject.FindProperty("smoothness");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SpriteGUI();
            AppearanceControlsGUI();
            EditorGUILayout.PropertyField(holeSizeProperty);
            EditorGUILayout.PropertyField(m_RaycastTarget);

            serializedObject.ApplyModifiedProperties();
        }
    }

}
#endif
