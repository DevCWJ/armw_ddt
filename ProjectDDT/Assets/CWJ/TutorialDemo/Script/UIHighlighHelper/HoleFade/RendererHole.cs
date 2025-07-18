namespace Abu
{
    using UnityEngine;

    public class RendererHole : HoleCore
    {
        public RendererHole(Renderer renderer, UIHoleFadeImage image, bool isAutoUpdate = true) : base(
            isAutoUpdate)
        {
            Renderer = renderer;
            Image = image;
            WorldRect = CalculateRendererRect();
        }

        public Renderer Renderer { get; }

        public UIHoleFadeImage Image { get; }

        Rect WorldRect { get; set; }

        public override Rect GetWorldRect() => WorldRect;

        public override void UpdateRect()
        {
            Rect rect = CalculateRendererRect();

            if (WorldRect == rect)
                return;

            WorldRect = rect;
            InvokeRectChanged();
        }

        Rect CalculateRendererRect()
        {
            if (Image == null || Image.canvas == null || Image.canvas.worldCamera == null)
                return Rect.zero;

            Bounds bounds = Renderer.bounds;

            Vector2 min = bounds.min;
            Vector2 max = bounds.max;

            if (Image.canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                min = RectTransformUtility.WorldToScreenPoint(Image.canvas.worldCamera, bounds.min);
                max = RectTransformUtility.WorldToScreenPoint(Image.canvas.worldCamera, bounds.max);
            }

            return new Rect(min, new Vector2(max.x - min.x, max.y - min.y));
        }
    }
}
