namespace Abu
{
    using System;
    using UnityEngine;

    public abstract class HoleCore
    {
        protected HoleCore(bool isAutoUpdate = true)
        {
            IsAutoUpdateEnabled = isAutoUpdate;
        }

        public bool IsAutoUpdateEnabled { get; set; }

        public abstract void UpdateRect();

        public event Action RectChanged;

        public abstract Rect GetWorldRect();

        protected void InvokeRectChanged()
        {
            RectChanged?.Invoke();
        }
    }
}
