using TBS.Core.Events;

namespace TBS.Presentation.UI.Panels
{
    /// <summary>
    /// UnitPanel的Presenter - 处理单位面板业务逻辑
    /// </summary>
    public class UnitPanelPresenter : BasePresenter<UnitPanelView, UnitPanelViewModel>
    {
        protected override void OnInitialize()
        {
            // 订阅单位状态变更事件
            EventBus.On<UnitStateChangedEvent>(OnUnitStateChanged);
            EventBus.On<UnitStatsChangedEvent>(OnUnitStatsChanged);
        }

        /// <summary>
        /// 显示指定单位的信息
        /// </summary>
        public void ShowUnitInfo(TBS.Unit.Unit unit)
        {
            if (unit == null)
            {
                // 清空显示
                ViewModel.Clear();
                return;
            }

            ViewModel.UpdateFromUnit(unit);
            View.OnShow();
        }

        /// <summary>
        /// 刷新当前显示的单位信息
        /// </summary>
        public void RefreshCurrentUnit()
        {
            ViewModel.UpdateDynamicProperties();
        }

        /// <summary>
        /// 关闭面板
        /// </summary>
        public void OnCloseClicked()
        {
            ViewModel.ClosePanel();
        }

        #region 事件处理

        private void OnUnitStateChanged(UnitStateChangedEvent evt)
        {
            // 如果事件中的单位是当前显示的单位，更新状态
            if (ViewModel.CurrentUnit != null && evt.UnitId == ViewModel.CurrentUnit.UnitId)
            {
                ViewModel.State = evt.NewState;
            }
        }

        private void OnUnitStatsChanged(UnitStatsChangedEvent evt)
        {
            // 如果事件中的单位是当前显示的单位，更新属性
            if (ViewModel.CurrentUnit != null && evt.UnitId == ViewModel.CurrentUnit.UnitId)
            {
                ViewModel.UpdateDynamicProperties();
            }
        }

        public override void OnDestroy()
        {
            // 取消事件订阅
            EventBus.Off<UnitStateChangedEvent>(OnUnitStateChanged);
            EventBus.Off<UnitStatsChangedEvent>(OnUnitStatsChanged);
            base.OnDestroy();
        }

        #endregion
    }

    #region 事件定义

    /// <summary>
    /// 单位状态变更事件
    /// </summary>
    public struct UnitStateChangedEvent
    {
        public string UnitId;
        public TBS.Unit.UnitState NewState;
    }

    /// <summary>
    /// 单位属性变更事件
    /// </summary>
    public struct UnitStatsChangedEvent
    {
        public string UnitId;
    }

    #endregion
}
