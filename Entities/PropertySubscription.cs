using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Entities
{
    public class PropertySubscription : Disposable
    {
        private readonly INotifyPropertyChanged _viewModel;
        private readonly string _property;
        private readonly PropertyChangedEventHandler _handler;
        private readonly Action<object> _action;

        public PropertySubscription(INotifyPropertyChanged viewModel, PropertyChangedEventHandler handler)
        {
            _viewModel = viewModel;
            _handler = handler;

            viewModel.PropertyChanged += Invoker;
        }

        public PropertySubscription(Expression<Func<object>> propertyLambda, Action action)
            : this(propertyLambda, o => action())
        {

        }

        public PropertySubscription(Expression<Func<object>> propertyLambda, Action<object> action)
        {
            var body = propertyLambda.Body as MemberExpression ??
                       ((UnaryExpression)propertyLambda.Body).Operand as MemberExpression;
            if (body == null)
            {
                throw new ArgumentException("Expression \"" + propertyLambda + "\" is not a valid member expression.");
            }
            _property = body.Member.Name;

            _viewModel = Expression.Lambda(body.Expression).Compile().DynamicInvoke() as INotifyPropertyChanged;
            if (_viewModel == null)
            {
                throw new ArgumentException("Expression \"" + propertyLambda + "\" is not calling a INotifyPropertyChanged.");
            }

            _action = action;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        public Func<bool> IsEnabled { get; set; }

        public void Invoke()
        {
            if (IsEnabled == null || IsEnabled())
            {
                if (_property == null)
                {
                    _handler(null, new PropertyChangedEventArgs(string.Empty));
                }
                else
                {
                    _action(_viewModel);
                }
            }
        }

        private void Invoker(object sender, PropertyChangedEventArgs e)
        {
            Invoke();
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _property || e.PropertyName == string.Empty)
            {
                Invoke();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (_property == null)
                    {
                        _viewModel.PropertyChanged -= Invoker;
                    }
                    else
                    {
                        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                    }
                }
            }
            base.Dispose(disposing);
        }
    }
}
