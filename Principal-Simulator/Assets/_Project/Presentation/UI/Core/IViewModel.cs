using System;
using System.ComponentModel;

namespace TBS.Presentation.UI
{
    /// <summary>
    /// ViewModel接口 - 定义UI数据状态的契约
    /// </summary>
    public interface IViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 手动触发属性变更通知
        /// </summary>
        void RaisePropertyChanged(string propertyName);

        /// <summary>
        /// 批量触发多个属性变更通知
        /// </summary>
        void RaisePropertiesChanged(params string[] propertyNames);

        /// <summary>
        /// 清空所有数据状态
        /// </summary>
        void Clear();
    }
}
