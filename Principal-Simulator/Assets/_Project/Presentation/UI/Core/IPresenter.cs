namespace TBS.Presentation.UI
{
    /// <summary>
    /// Presenter接口 - 定义调度者的契约
    /// </summary>
    public interface IPresenter
    {
        /// <summary>
        /// 初始化Presenter（由View在创建时调用）
        /// </summary>
        void Initialize(IUIView view, IViewModel viewModel);

        /// <summary>
        /// UI显示时调用
        /// </summary>
        void OnShow();

        /// <summary>
        /// UI隐藏时调用
        /// </summary>
        void OnHide();

        /// <summary>
        /// UI销毁时调用（清理订阅和资源）
        /// </summary>
        void OnDestroy();
    }

    /// <summary>
    /// 泛型Presenter接口 - 强类型关联View和ViewModel
    /// </summary>
    public interface IPresenter<out TView, out TViewModel> : IPresenter
        where TView : IUIView
        where TViewModel : IViewModel
    {
        TView View { get; }
        TViewModel ViewModel { get; }
    }
}
