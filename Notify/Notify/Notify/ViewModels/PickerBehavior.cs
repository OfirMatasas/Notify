using System.Collections;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class PickerBehavior : IList<Behavior>
    {
        public IEnumerator<Behavior> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Behavior item)
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }

        public bool Contains(Behavior item)
        {
            throw new System.NotImplementedException();
        }

        public void CopyTo(Behavior[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(Behavior item)
        {
            throw new System.NotImplementedException();
        }

        public int Count { get; }
        public bool IsReadOnly { get; }
        public int IndexOf(Behavior item)
        {
            throw new System.NotImplementedException();
        }

        public void Insert(int index, Behavior item)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new System.NotImplementedException();
        }

        public Behavior this[int index]
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }
    }
}