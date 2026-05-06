using System;

namespace TBS.Presentation.UI
{
    /// <summary>
    /// 命令接口 - 封装UI用户操作
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// 是否可以执行
        /// </summary>
        bool CanExecute { get; }

        /// <summary>
        /// 执行命令
        /// </summary>
        void Execute();

        /// <summary>
        /// 执行状态变更事件
        /// </summary>
        event Action<bool> OnCanExecuteChanged;
    }

    /// <summary>
    /// 带参数的泛型命令接口
    /// </summary>
    public interface ICommand<T>
    {
        /// <summary>
        /// 是否可以执行
        /// </summary>
        bool CanExecute(T parameter);

        /// <summary>
        /// 执行命令
        /// </summary>
        void Execute(T parameter);

        /// <summary>
        /// 执行状态变更事件
        /// </summary>
        event Action<bool> OnCanExecuteChanged;
    }

    /// <summary>
    /// 简单命令实现
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public bool CanExecute => _canExecute?.Invoke() ?? true;
        public event Action<bool> OnCanExecuteChanged;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public void Execute()
        {
            if (CanExecute)
            {
                _execute.Invoke();
            }
        }

        /// <summary>
        /// 手动触发CanExecuteChanged事件
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged?.Invoke(CanExecute);
        }
    }

    /// <summary>
    /// 带参数的泛型命令实现
    /// </summary>
    public class RelayCommand<T> : ICommand<T>
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public event Action<bool> OnCanExecuteChanged;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(T parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(T parameter)
        {
            if (CanExecute(parameter))
            {
                _execute.Invoke(parameter);
            }
        }

        /// <summary>
        /// 手动触发CanExecuteChanged事件
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged?.Invoke(true);
        }
    }
}
