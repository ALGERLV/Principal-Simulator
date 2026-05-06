using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TBS.Presentation.UI
{
    /// <summary>
    /// ViewModel基类 - 实现属性通知机制
    /// </summary>
    public abstract class ViewModelBase : IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly Dictionary<string, object> _propertyValues = new();

        /// <summary>
        /// 获取属性值（带默认值）
        /// </summary>
        protected T GetProperty<T>([CallerMemberName] string propertyName = null, T defaultValue = default)
        {
            if (_propertyValues.TryGetValue(propertyName, out var value))
            {
                return (T)value;
            }
            return defaultValue;
        }

        /// <summary>
        /// 设置属性值并触发变更通知
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// 设置属性值（使用内部存储）
        /// </summary>
        protected bool SetProperty<T>(T value, [CallerMemberName] string propertyName = null)
        {
            if (_propertyValues.TryGetValue(propertyName, out var existingValue) &&
                EqualityComparer<T>.Default.Equals((T)existingValue, value))
                return false;

            _propertyValues[propertyName] = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// 手动触发属性变更通知
        /// </summary>
        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 批量触发多个属性变更通知
        /// </summary>
        public void RaisePropertiesChanged(params string[] propertyNames)
        {
            foreach (var name in propertyNames)
            {
                RaisePropertyChanged(name);
            }
        }

        /// <summary>
        /// 清空所有数据状态
        /// </summary>
        public virtual void Clear()
        {
            _propertyValues.Clear();
        }

        /// <summary>
        /// 订阅属性变更事件（方便View层使用）
        /// </summary>
        public void Subscribe(string propertyName, Action callback)
        {
            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == propertyName)
                {
                    callback?.Invoke();
                }
            };
        }
    }
}
