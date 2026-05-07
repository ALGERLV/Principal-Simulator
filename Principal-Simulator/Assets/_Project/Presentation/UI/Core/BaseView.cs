using System;
using System.Collections.Generic;
using UnityEngine;

namespace TBS.Presentation.UI
{
    /// <summary>
    /// View基类 - MVP组合的入口点，挂载在Prefab根节点
    /// </summary>
    public abstract class BaseView<TViewModel> : MonoBehaviour, IUIView
        where TViewModel : IViewModel, new()
    {
        [SerializeField] private string _uiId;

        public string UIId => string.IsNullOrEmpty(_uiId) ? GetType().Name : _uiId;
        public bool IsVisible => gameObject != null && gameObject.activeInHierarchy;
        public TViewModel ViewModel { get; private set; }

        private readonly Dictionary<string, List<Action>> _bindings = new();
        private bool _isInitialized;

        /// <summary>
        /// 获取关联的Presenter（由子类创建）
        /// </summary>
        protected IPresenter Presenter { get; private set; }

        protected virtual void Awake()
        {
            if (_isInitialized) return;

            // 创建ViewModel
            ViewModel = new TViewModel();

            // 订阅ViewModel属性变更
            ViewModel.PropertyChanged += OnViewModelPropertyChanged;

            // 创建Presenter（子类实现）
            Presenter = CreatePresenter();
            Presenter?.Initialize(this, ViewModel);

            _isInitialized = true;
        }

        protected virtual void Start()
        {
            OnBind();
        }

        protected virtual void OnDestroy()
        {
            OnBeforeDestroy();

            // 清理Presenter
            Presenter?.OnDestroy();
            Presenter = null;

            // 清理ViewModel订阅
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }

            // 清理绑定
            _bindings.Clear();
        }

        /// <summary>
        /// 创建Presenter（子类必须实现）
        /// </summary>
        protected abstract IPresenter CreatePresenter();

        /// <summary>
        /// 绑定UI组件和ViewModel属性（子类实现具体绑定逻辑）
        /// </summary>
        protected abstract void OnBind();

        /// <summary>
        /// 绑定ViewModel属性到回调（当属性变化时执行回调）
        /// </summary>
        protected void Bind(string propertyName, Action callback)
        {
            if (!_bindings.TryGetValue(propertyName, out var list))
            {
                list = new List<Action>();
                _bindings[propertyName] = list;
            }
            list.Add(callback);
        }

        /// <summary>
        /// ViewModel属性变更回调
        /// </summary>
        private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_bindings.TryGetValue(e.PropertyName, out var callbacks))
            {
                foreach (var callback in callbacks)
                {
                    callback?.Invoke();
                }
            }
        }

        /// <summary>
        /// 显示UI（UIManager调用）
        /// </summary>
        public virtual void OnShow()
        {
            gameObject.SetActive(true);
            Presenter?.OnShow();
        }

        /// <summary>
        /// 隐藏UI（UIManager调用）
        /// </summary>
        public virtual void OnHide()
        {
            Presenter?.OnHide();
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 销毁前的清理（子类可重写）
        /// </summary>
        public virtual void OnBeforeDestroy()
        {
            // 子类清理UI事件订阅等
        }

        /// <summary>
        /// 设置UI在场景中的父节点
        /// </summary>
        public void SetParent(Transform parent, bool worldPositionStays = false)
        {
            transform.SetParent(parent, worldPositionStays);
        }

        /// <summary>
        /// 设置UI的Sorting Order（用于层级管理）
        /// </summary>
        public void SetSortingOrder(int order)
        {
            var canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }
            canvas.overrideSorting = true;
            canvas.sortingOrder = order;
        }

        /// <summary>
        /// 设置UIId（通常用于编辑器生成Prefab时调用）
        /// </summary>
        public void SetUIId(string uiId)
        {
            _uiId = uiId;
        }
    }
}
