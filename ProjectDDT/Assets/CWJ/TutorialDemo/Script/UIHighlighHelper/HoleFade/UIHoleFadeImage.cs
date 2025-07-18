namespace Abu
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Component which will render added holes in UI.
    /// </summary>
    public class UIHoleFadeImage : Image
    {

        /// <summary>
        /// Id for holes rect array property in UIHoleFade.shared.
        /// </summary>
        private static readonly int HolesID = Shader.PropertyToID("_Holes");

        /// <summary>
        /// Id for FadeImage's aspect ration property in UIHoleFade.shared.
        /// </summary>
        private static readonly int AspectID = Shader.PropertyToID("_Aspect");

        /// <summary>
        /// Id for count of active holes property in UIHoleFade.shared.
        /// </summary>
        private static readonly int HolesLengthID = Shader.PropertyToID("_HolesLength");

        /// <summary>
        /// Id for hole's edge smoothness (i.e. size of the hole) property in UIHoleFade.shared.
        /// </summary>
        private static readonly int SmoothnessID = Shader.PropertyToID("_Smoothness");

        /// <summary>
        /// Max hole size. Could be changed, however you should change the _Holes[7]; value in UIHoleFade.shared file also.
        /// </summary>
        public const int MaxHolesCount = 7;

        private readonly List<HoleCore> Holes = new(MaxHolesCount);

        private const string ShaderName = "UI/HoleFade";

        public override Material material
        {
            get
            {
                if (!m_Material)
                {
#if UNITY_EDITOR
                    //we should be sure that shader added to always included list
                    AddAlwaysIncludedShader();
#endif
                    Shader shader = Shader.Find(ShaderName);

                    //this error could happen in runtime if shader was not added to always include list. The other thing is that shader could be renamed.
                    if (!shader)
                        Debug.LogError($"[UIHoleFadeImage] Shader '{ShaderName}' doesn't exist. Probably it's not added to always include shaders list");

                    m_Material = new Material(shader);
                }

                return m_Material;
            }
        }

        bool isDirty;

        readonly Vector4[] holesBuffer = new Vector4[MaxHolesCount];

        [SerializeField, Range(0, 1), Tooltip("size of the hole")] float smoothness = 0.01f;


        public float Smoothness
        {
            get => smoothness;
            set
            {
                if (Mathf.Approximately(smoothness, value))
                    return;

                smoothness = value;
                SetDirtyMaterial();
            }
        }

        void LateUpdate()
        {
            foreach (HoleCore hole in Holes.Where(hole => hole.IsAutoUpdateEnabled))
                hole.UpdateRect();

            UpdateMaterialData();
        }

        protected override void OnDidApplyAnimationProperties()
        {
            base.OnDidApplyAnimationProperties();

            SetDirtyMaterial();
        }


        public void AddHole(HoleCore hole)
        {
            if(hole == null || Holes.Contains(hole))
                return;

            Holes.Add(hole);
            hole.RectChanged += SetDirtyMaterial;
            SetDirtyMaterial();
        }


        public void RemoveHole(HoleCore hole)
        {
            if(hole == null || !Holes.Contains(hole))
                return;

            hole.RectChanged -= SetDirtyMaterial;
            Holes.Remove(hole);
            SetDirtyMaterial();
        }


        public override bool IsRaycastLocationValid(Vector2 eventPosition, Camera eventCamera)
        {
            Vector3 worldEventPosition;

            if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventPosition, eventCamera, out worldEventPosition))
                return false;

            return !Holes.Any(hole =>
                                  hole.GetWorldRect().Contains(worldEventPosition));
        }


        void SetDirtyMaterial()
        {
            isDirty = true;
        }


        void UpdateMaterialData()
        {
            if (!isDirty)
                return;

            if (Holes.Count > MaxHolesCount)
                Debug.LogError($"[UIHoleFadeImage] Max holes size is {MaxHolesCount}");

            material.SetInt(HolesLengthID, Holes.Count);
            material.SetFloat(SmoothnessID, smoothness);

            Rect worldRect = rectTransform.TransformRect(rectTransform.rect);

            for (int i = 0; i < MaxHolesCount; i++)
            {
                if (i < Holes.Count)
                    holesBuffer[i] = GetRectVectorRelative(Holes[i].GetWorldRect(), worldRect);
                else
                    holesBuffer[i] = Vector4.zero;
            }

            float aspect = worldRect.width / worldRect.height;

            material.SetFloat(AspectID, aspect);
            material.SetVectorArray(HolesID, holesBuffer);

            isDirty = false;
        }


        Vector4 GetRectVectorRelative(Rect holeRect, Rect worldRect)
        {
            float xMin = Remap(holeRect.x, worldRect.x, worldRect.x + worldRect.width, 0, 1);
            float yMin = Remap(holeRect.y, worldRect.y, worldRect.y + worldRect.height, 0, 1);
            float xMax = Remap(holeRect.x + holeRect.width, worldRect.x, worldRect.x + worldRect.width, 0, 1);
            float yMax = Remap(holeRect.y + holeRect.height, worldRect.y, worldRect.y + worldRect.height, 0, 1);

            return new Vector4(xMin, yMin, xMax, yMax);
        }


        static float Remap(float value, float from1, float to1, float from2, float to2)
            => (value - from1) / (to1 - from1) * (to2 - from2) + from2;


#if UNITY_EDITOR

        protected override void OnValidate()
        {
            base.OnValidate();

            SetDirtyMaterial();
        }

        [UnityEditor.MenuItem("GameObject/UI/UIHoleFadeImage", false)]
        static void Create()
        {
            GameObject gameObject = new GameObject(nameof(UIHoleFadeImage), typeof(UIHoleFadeImage));
            gameObject.transform.SetParent(UnityEditor.Selection.activeTransform);

            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            UIHoleFadeImage image = gameObject.GetComponent<UIHoleFadeImage>();
            image.color = new Color(0f, 0f, 0f, 0.39f);
        }

        static void AddAlwaysIncludedShader()
        {
            Shader shader = Shader.Find(ShaderName);

            if (!shader)
                return;

            UnityEngine.Rendering.GraphicsSettings graphicsSettingsObj =
                UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
            UnityEditor.SerializedObject serializedObject = new UnityEditor.SerializedObject(graphicsSettingsObj);
            UnityEditor.SerializedProperty arrayProp = serializedObject.FindProperty("m_AlwaysIncludedShaders");

            bool hasShader = false;

            for (int i = 0; i < arrayProp.arraySize; ++i)
            {
                UnityEditor.SerializedProperty arrayElem = arrayProp.GetArrayElementAtIndex(i);
                if (shader == arrayElem.objectReferenceValue)
                {
                    hasShader = true;
                    break;
                }
            }

            if (!hasShader)
            {
                int arrayIndex = arrayProp.arraySize;
                arrayProp.InsertArrayElementAtIndex(arrayIndex);
                UnityEditor.SerializedProperty arrayElem = arrayProp.GetArrayElementAtIndex(arrayIndex);
                arrayElem.objectReferenceValue = shader;

                serializedObject.ApplyModifiedProperties();

                UnityEditor.AssetDatabase.SaveAssets();

                Debug.Log($"[UIHoleFadeImage] Shader '{ShaderName}' has been added to always include shaders list. It's important. Don't delete it.");
            }
        }

#endif

    }
}

