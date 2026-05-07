using UnityEngine;

namespace TBS.Contracts.Events
{
    /// <summary>
    /// 输入系统相关事件 - 由 InputRouter 发布
    /// </summary>

    public struct MouseButtonDownEvent
    {
        public Vector2 ScreenPos;
        public int Button;
        public bool IsOverUI;
    }

    public struct MouseButtonUpEvent
    {
        public Vector2 ScreenPos;
        public int Button;
    }

    public struct MouseDragEvent
    {
        public Vector2 Delta;
        public int Button;
    }

    public struct ScrollEvent
    {
        public float Delta;
    }

    public struct KeyHeldEvent
    {
        public KeyCode Key;
    }
}
