namespace TBS.Presentation.UI
{
    /// <summary>
    /// Presenter基类 - 协调View和ViewModel之间的交互
    /// </summary>
    public abstract class BasePresenter<TView, TViewModel> : IPresenter<TView, TViewModel>
        where TView : IUIView
        where TViewModel : IViewModel
    {
        public TView View { get; private set; }
        public TViewModel ViewModel { get; private set; }

        private bool _isInitialized;

        /// <summary>
        /// IPresenter接口的显式实现（接收非泛型参数并转换为泛型）
        /// </summary>
        void IPresenter.Initialize(IUIView view, IViewModel viewModel)
        {
            if (view is TView typedView && viewModel is TViewModel typedViewModel)
            {
                Initialize(typedView, typedViewModel);
            }
        }

        /// <summary>
        /// 初始化Presenter，关联View和ViewModel（泛型版本）
        /// </summary>
        public virtual void Initialize(TView view, TViewModel viewModel)
        {
            if (_isInitialized) return;

            View = view;
            ViewModel = viewModel;
            _isInitialized = true;

            OnInitialize();
        }

        /// <summary>
        /// 初始化时调用（子类重写此方法进行事件订阅等）
        /// </summary>
        protected abstract void OnInitialize();

        /// <summary>
        /// UI显示时调用
        /// </summary>
        public virtual void OnShow() { }

        /// <summary>
        /// UI隐藏时调用
        /// </summary>
        public virtual void OnHide() { }

        /// <summary>
        /// UI销毁时调用（清理订阅和资源）
        /// </summary>
        public virtual void OnDestroy()
        {
            View = default;
            ViewModel = default;
            _isInitialized = false;
        }

        /// <summary>
        /// 安全地更新ViewModel属性（空检查）
        /// </summary>
        protected void SetViewModelProperty<T>(string propertyName, T value)
        {
            ViewModel?.RaisePropertyChanged(propertyName);
        }
    }
}
