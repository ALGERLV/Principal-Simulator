using UnityEngine;

namespace TBS.Presentation.UI
{
    /// <summary>
    /// View接口 - 所有UI组件的统一契约
    /// </summary>
    public interface IUIView
    {
        /// <summary>
        /// UI唯一标识
        /// </summary>
        string UIId { get; }

        /// <summary>
        /// 是否处于显示状态
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// 对应的GameObject
        /// </summary>
        GameObject gameObject { get; }

        /// <summary>
        /// Transform组件
        /// </summary>
        Transform transform { get; }

        /// <summary>
        /// 显示UI
        /// </summary>
        void OnShow();

        /// <summary>
        /// 隐藏UI
        /// </summary>
        void OnHide();

        /// <summary>
        /// 销毁UI前的清理
        /// </summary>
        void OnBeforeDestroy();
    }
}
