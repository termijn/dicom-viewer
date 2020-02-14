using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Entities
{
    public abstract class Bindable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent<T>(Expression<Func<T>> expression)
        {
            var property = PropertyOf(expression);
            OnPropertyChanged(property.Name);
        }

        protected void RaiseAllPropertiesChangedEvent()
        {
            OnPropertyChanged(string.Empty);
        }

        protected bool SetProperty<T>(ref T oldValue, T newValue, [CallerMemberName] string property = "")
        {
            if (Equals(oldValue, newValue))
            {
                return false;
            }

            oldValue = newValue;
            OnPropertyChanged(property);
            return true;
        }

        protected bool SetProperty(ref double oldValue, double newValue, [CallerMemberName] string property = "")
        {
            if (oldValue.Equals(newValue))
            {
                return false;
            }

            oldValue = newValue;
            OnPropertyChanged(property);
            return true;
        }

        protected void RaisePropertyChangedEvent([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(propertyName);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static PropertyInfo PropertyOf<T>(Expression<Func<T>> expression)
        {
            var memberExpr = expression.Body as MemberExpression;

            if (memberExpr == null)
            {
                throw new ArgumentException("Expression \"" + expression + "\" is not a valid member expression.");
            }

            var property = memberExpr.Member as PropertyInfo;
            if (property == null)
            {
                throw new ArgumentException("Expression \"" + expression + "\" does not reference a property.");
            }
            return property;
        }
    }
}
