using System;
using System.Windows.Input;

namespace DicomViewer.DotNetExtensions
{
    public class BindableCommand<T> : ICommand
    {
        private Action<T> _execute;
        private Predicate<T> _canExecute;
        private bool _isEnabled = true;
        private event EventHandler CanExecutedChanged;

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CanExecutedChanged += value;
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CanExecutedChanged -= value;
                CommandManager.RequerySuggested -= value;
            }
        }

        public delegate bool Predicate();

        public BindableCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        public BindableCommand(Action<T> execute, Predicate<T> canExecute)
        {
            Type type = typeof(T);
            if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
            {
                // If you get this exception, use a reference type (class) or a nullable type, like 'bool?'
                // as template argument. Unfortunately, this cannot be checked statically.
                throw new InvalidOperationException(
                    "Only types with a default value of null can be assigned to the RelayCommand<T> type.");
            }

            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            _execute = execute;
            _canExecute = canExecute;
        }
        protected BindableCommand()
        {
        }

        protected Action<T> ExecuteAction
        {
            get { return _execute; }
            set { _execute = value; }
        }
        protected Predicate<T> CanExecutePredicate
        {
            get { return _canExecute; }
            set { _canExecute = value; }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    RaiseCanExecuteChanged();
                }
            }
        }

        public bool CanExecute(object parameter)
        {
            return (_canExecute == null || _canExecute((T)parameter)) && IsEnabled;
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecutedChanged?.Invoke(this, new EventArgs());
        }
    }

    public class BindableCommand : BindableCommand<object>
    {
        public BindableCommand(Action execute)
            : base(args => execute())
        {
        }

        public BindableCommand(Action execute, Predicate canExecute)
            : base(args => execute(), args => canExecute())
        {
        }
        protected BindableCommand()
        {
        }
    }
}
