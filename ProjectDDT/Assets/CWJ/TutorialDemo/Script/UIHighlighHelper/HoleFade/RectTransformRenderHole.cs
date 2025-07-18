namespace Abu
{
    using UnityEngine;

    public static class Extension
    {
        public static Rect TransformRect(this Transform transform, Rect rect)
        {
            Vector3 lossyScale = transform.lossyScale;
            Vector3 position = transform.position;

            return new Rect(
                rect.x * lossyScale.x + position.x,
                rect.y * lossyScale.y + position.y,
                rect.width * lossyScale.x,
                rect.height * lossyScale.y
            );
        }
    }

    public class RectTransformRenderHole : HoleCore
    {
        public RectTransformRenderHole(RectTransform rectTransform, bool isAutoUpdate = true) : base(isAutoUpdate)
        {
            RectTransform = rectTransform;
            WorldRect = RectTransform.TransformRect(RectTransform.rect);
        }

        public RectTransform RectTransform { get; }

        Rect WorldRect { get; set; }

        public override Rect GetWorldRect() => WorldRect;

        public override void UpdateRect()
        {
            Rect rect = RectTransform.TransformRect(RectTransform.rect);

            if (WorldRect == rect)
                return;

            WorldRect = rect;
            InvokeRectChanged();
        }
    }
}
