namespace Abu
{
    using UnityEngine;

    public class RectRenderHole : HoleCore
    {
        public RectRenderHole(Rect worldRect, bool isAutoUpdate = true) : base(isAutoUpdate)
        {
            WorldRect = worldRect;
        }

        public Rect WorldRect { get; private set; }

        public void SetWorldRect(Rect worldRect)
        {
            if (worldRect == WorldRect)
                return;

            WorldRect = worldRect;
            InvokeRectChanged();
        }

        public override void UpdateRect()
        {
        }

        public override Rect GetWorldRect() => WorldRect;

    }
}
