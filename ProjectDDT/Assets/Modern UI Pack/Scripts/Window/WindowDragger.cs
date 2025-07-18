using UnityEngine;
using UnityEngine.EventSystems;

namespace Michsky.MUIP
{
    public class WindowDragger : UIBehaviour,
                                 IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Resources")]
        public RectTransform dragArea;     // 창이 벗어나지 못할 영역
        public RectTransform dragObject;   // 실제로 움직일 창
        public RectTransform dragHandle;   // ⬅️ 드래그를 시작할 수 있는 영역(타이틀바 등)

        [Header("Settings")]
        public bool topOnDrag = true;

        // 내부 상태
        Vector2  originalLocalPointerPosition;
        Vector3  originalPanelLocalPosition;
        bool     isDragging;   // 핸들 안에서 시작했는지 여부

        /* ---------- SETUP ---------- */

        public new void Start()
        {
            if (dragArea == null)
            {
#if UNITY_2023_2_OR_NEWER
                var canvas = FindObjectsByType<Canvas>(FindObjectsSortMode.None)[0];
#else
                var canvas = (Canvas)FindObjectsOfType(typeof(Canvas))[0];
#endif
                dragArea = canvas.GetComponent<RectTransform>();
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

        /* ---------- DRAG EVENTS ---------- */

        public void OnBeginDrag(PointerEventData data)
        {
            // 핸들이 지정됐으면, 포인터가 핸들 안에 있는지 검사
            isDragging = dragHandle == null ||
                         RectTransformUtility.RectangleContainsScreenPoint(
                             dragHandle, data.pressPosition, data.pressEventCamera);

            if (!isDragging) return;   // 핸들 밖이면 드래그 무시

            originalPanelLocalPosition = DragObjectInternal.localPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                DragAreaInternal, data.position, data.pressEventCamera, out originalLocalPointerPosition);

            gameObject.transform.SetAsLastSibling();
            if (topOnDrag) dragObject.transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData data)
        {
            if (!isDragging) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    DragAreaInternal, data.position, data.pressEventCamera, out Vector2 localPointerPosition))
            {
                Vector3 offset = localPointerPosition - originalLocalPointerPosition;
                DragObjectInternal.localPosition = originalPanelLocalPosition + offset;
            }

            ClampToArea();
        }

        public void OnEndDrag(PointerEventData data) => isDragging = false;

        /* ---------- HELPERS ---------- */

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
