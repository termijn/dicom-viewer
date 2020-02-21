using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Entities
{
    public class CollectionObserver<T> : Disposable
    {
        private readonly List<T> _cachedItems;

        private INotifyCollectionChanged _inputCollectionChanged;
        private IEnumerable _inputCollection;

        private Action<T, int> _itemAddedCallback;
        private Action<T, int> _itemRemovedCallback;

        public CollectionObserver(ObservableCollection<T> observableCollection, Action<T> itemAddedCallback, Action<T> itemRemovedCallback)
            : this(observableCollection, observableCollection, itemAddedCallback, itemRemovedCallback)
        {
        }

        public CollectionObserver(ReadOnlyObservableCollection<T> readOnlyObservableCollection, Action<T> itemAddedCallback, Action<T> itemRemovedCallback)
            : this(readOnlyObservableCollection, readOnlyObservableCollection, itemAddedCallback, itemRemovedCallback)
        {
        }

        public CollectionObserver(ObservableCollection<T> observableCollection, Action<T, int> itemAddedCallback, Action<T, int> itemRemovedCallback)
            : this(observableCollection, observableCollection, itemAddedCallback, itemRemovedCallback)
        {
        }

        public CollectionObserver(ReadOnlyObservableCollection<T> readOnlyObservableCollection, Action<T, int> itemAddedCallback, Action<T, int> itemRemovedCallback)
            : this(readOnlyObservableCollection, readOnlyObservableCollection, itemAddedCallback, itemRemovedCallback)
        {
        }

        public CollectionObserver(INotifyCollectionChanged notifyCollectionChanged, IEnumerable inputCollection, Action<T> itemAddedCallback, Action<T> itemRemovedCallback)
            : this(notifyCollectionChanged, inputCollection, (item, index) => itemAddedCallback(item), (item, index) => itemRemovedCallback(item))
        {

        }

        public CollectionObserver(INotifyCollectionChanged notifyCollectionChanged, IEnumerable inputCollection, Action<T, int> itemAddedCallback, Action<T, int> itemRemovedCallback)
        {
            _inputCollection = inputCollection;
            _cachedItems = new List<T>();
            _itemAddedCallback = itemAddedCallback;
            _itemRemovedCallback = itemRemovedCallback;
            _inputCollectionChanged = notifyCollectionChanged;
            Add(_inputCollection, 0);

            _inputCollectionChanged.CollectionChanged += OnInputCollectionChanged;
        }

        private void OnInputCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Add(e.NewItems, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Remove(e.OldItems, e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    Remove(e.OldItems, e.OldStartingIndex);
                    Add(e.NewItems, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Remove(_cachedItems, e.OldStartingIndex);
                    Add(_inputCollection, e.NewStartingIndex);
                    break;
                default:
                    // Not supported
                    break;
            }
        }

        private void Remove(IEnumerable items, int index)
        {
            List<T> itemsToRemove = (from object item in items select (T)item).ToList();

            var itemIndex = index;
            // If index is -1, then entire collection is being removed by repeatedly
            // removing the first element
            if (itemIndex == -1)
            {
                itemIndex = 0;
            }

            foreach (T itemToRemove in itemsToRemove)
            {
                _itemRemovedCallback(itemToRemove, itemIndex);
                _cachedItems.Remove(itemToRemove);
            }
        }

        private void Add(IEnumerable items, int index)
        {
            int i = index;
            foreach (T item in items)
            {
                _itemAddedCallback(item, i);
                _cachedItems.Add(item);
                i++;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                _inputCollectionChanged.CollectionChanged -= OnInputCollectionChanged;

                Remove(_inputCollection, 0);

                _cachedItems.Clear();

                _itemAddedCallback = null;
                _itemRemovedCallback = null;
                _inputCollection = null;
                _inputCollectionChanged = null;
            }

            base.Dispose(disposing);
        }
    }
}
