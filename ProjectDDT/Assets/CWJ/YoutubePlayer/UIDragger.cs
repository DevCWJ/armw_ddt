using UnityEngine;
using UnityEngine.EventSystems;

namespace CWJ
{
    public class UIDragger : UIBehaviour,
                             IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Resources")]
        [Tooltip("드래그 영역")]
        public RectTransform dragArea;
        [Tooltip("이동될 UI")]
        public RectTransform dragObject;
        [Tooltip("드래그 시작을위한 클릭 가능한 영역")]
        public RectTransform dragHandle;

        [Header("Settings")]
        [Tooltip("드래그할때 상단으로 올려줄건지")]
        public bool topOnDrag = true;

        public bool isDragEnabled = true;

        Vector2  originalLocalPointerPosition;
        Vector3  originalPanelLocalPosition;
        bool     isDragging;

        private void Reset()
        {
            if (dragObject == null)
            {
                dragObject = GetComponent<RectTransform>();
                dragArea = dragObject.transform.parent.GetComponent<RectTransform>();
            }

        }

        public new void Start()
        {
            if (dragArea == null)
            {
// #if UNITY_2023_2_OR_NEWER
//                 var canvas = FindObjectsByType<Canvas>(FindObjectsSortMode.None)[0];
// #else
//                 var canvas = (Canvas)FindObjectsOfType(typeof(Canvas))[0];
// #endif

                dragArea = transform.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            }
        }

        RectTransform DragObjectInternal  => dragObject ? dragObject : (RectTransform)transform;
        RectTransform DragAreaInternal
        {
            get
            {
                if (dragArea) return dragArea;
                RectTransform canvas = transform as RectTransform;
                while (canvas.parent is RectTransform) canvas = canvas.parent as RectTransform;
                return canvas;
            }
        }

        public void OnBeginDrag(PointerEventData data)
        {
            // 핸들이 지정됐으면, 포인터가 핸들 안에 있는지 검사
            isDragging = dragHandle == null ||
                         RectTransformUtility.RectangleContainsScreenPoint(
                             dragHandle, data.pressPosition, data.pressEventCamera);

            if (!isDragging) return;

            if (isDragEnabled)
            {
                originalPanelLocalPosition = DragObjectInternal.localPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    DragAreaInternal, data.position, data.pressEventCamera, out originalLocalPointerPosition);
            }
            else
            {
                isDragging = false;
            }

            // gameObject.transform.SetAsLastSibling();
            if (topOnDrag) dragObject.transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData data)
        {
            if (!isDragEnabled)
                isDragging = false;
            if (!isDragging)
                return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    DragAreaInternal, data.position, data.pressEventCamera, out Vector2 localPointerPosition))
            {
                Vector3 offset = localPointerPosition - originalLocalPointerPosition;
                DragObjectInternal.localPosition = originalPanelLocalPosition + offset;
            }

            ClampToArea();
        }

        public void OnEndDrag(PointerEventData data) => isDragging = false;


        void ClampToArea()
        {
            Vector3 pos          = DragObjectInternal.localPosition;
            Vector3 minPosition  = DragAreaInternal.rect.min - DragObjectInternal.rect.min;
            Vector3 maxPosition  = DragAreaInternal.rect.max - DragObjectInternal.rect.max;

            pos.x = Mathf.Clamp(pos.x, minPosition.x, maxPosition.x);
            pos.y = Mathf.Clamp(pos.y, minPosition.y, maxPosition.y);

            DragObjectInternal.localPosition = pos;
        }
    }
}
