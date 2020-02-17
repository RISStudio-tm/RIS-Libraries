using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RIS.Collections.NestableCollections
{
    public class NestableListL<T> : INestableCollection<T>, ICollection, IEnumerable<T>, IEnumerable
    {
        public event RMessageHandler ShowMessage;
        public event RErrorHandler ShowError;

        public NestedElement<T> this[int index]
        {
            get
            {
                return Get(index);
            }
            set
            {
                Set(index, value);
            }
        }

        private List<NestedElement<T>> ValuesCollection { get; }
        public int Length { get; private set; }
        public object SyncRoot { get; }
        public bool IsSynchronized { get; }

        public NestableListL()
        {
            SyncRoot = new object();
            IsSynchronized = false;

            Length = 0;
            ValuesCollection = new List<NestedElement<T>>();
        }
        public NestableListL(int length)
        {
            if (length < 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(length), "Длина массива не может быть меньше 0");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            SyncRoot = new object();
            IsSynchronized = false;

            Length = length;
            ValuesCollection = new List<NestedElement<T>>(length);
        }

        public NestedElement<T> Get(int index)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > Length - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            
            return ValuesCollection[index];
        }
        
        public void Set(int index, NestedElement<T> value)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > Length - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            ValuesCollection[index] = value;
        }
        public void Set(int index, T value)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > Length - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            ValuesCollection[index].Set(value);
        }
        public void Set(int index, T[] value)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > Length - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            ValuesCollection[index].Set(value);
        }
        public void Set(int index, INestableCollection<T> value)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > Length - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            ValuesCollection[index].Set(value);
        }

        public string ToStringRepresent()
        {
            return NestableCollectionHelper.ToStringRepresent<T>(this);
        }

        public void FromStringRepresent(string represent)
        {
            NestableCollectionHelper.FromStringRepresent<T, NestableListL<T>>(represent, this);
        }
        public void FromStringRepresent<TC>(string represent)
            where TC : INestableCollection<T>, new()
        {
            NestableCollectionHelper.FromStringRepresent<T, TC>(represent, this);
        }

        public IEnumerable<T> Enumerate()
        {
            return NestableCollectionHelper.Enumerate<T>(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            IEnumerable<T> value = NestableCollectionHelper.Enumerate<T>(this);

            foreach (var element in value)
            {
                yield return element;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            IEnumerable<T> value = NestableCollectionHelper.Enumerate<T>(this);

            foreach (var element in value)
            {
                yield return element;
            }
        }

        int ICollection.Count
        {
            get
            {
                return Length;
            }
        }

        public bool Add(NestedElement<T> value)
        {
            if (Length == int.MaxValue)
            {
                var exception = new Exception("Нельзя добавить элемент, так как коллекция уже содержит максимальное их количество");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            ValuesCollection.Add(value);
            ++Length;

            return true;
        }
        public bool Add(T value)
        {
            if (Length == int.MaxValue)
            {
                var exception = new Exception("Нельзя добавить элемент, так как коллекция уже содержит максимальное их количество");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            ValuesCollection.Add(new NestedElement<T>(value));
            ++Length;

            return true;
        }
        public bool Add(T[] value)
        {
            if (Length == int.MaxValue)
            {
                var exception = new Exception("Нельзя добавить элемент, так как коллекция уже содержит максимальное их количество");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            ValuesCollection.Add(new NestedElement<T>(value));
            ++Length;

            return true;
        }
        public bool Add(INestableCollection<T> value)
        {
            if (Length == int.MaxValue)
            {
                var exception = new Exception("Нельзя добавить элемент, так как коллекция уже содержит максимальное их количество");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            ValuesCollection.Add(new NestedElement<T>(value));
            ++Length;

            return true;
        }

        public bool Insert(NestedElement<T> value, int index)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > Length - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (Length == int.MaxValue)
            {
                var exception = new Exception("Нельзя добавить элемент, так как коллекция уже содержит максимальное их количество");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            ValuesCollection.Insert(index, value);
            ++Length;

            return true;
        }
        public bool Insert(T value, int index)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > Length - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (Length == int.MaxValue)
            {
                var exception = new Exception("Нельзя добавить элемент, так как коллекция уже содержит максимальное их количество");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            ValuesCollection.Insert(index, new NestedElement<T>(value));
            ++Length;

            return true;
        }
        public bool Insert(T[] value, int index)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > Length - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (Length == int.MaxValue)
            {
                var exception = new Exception("Нельзя добавить элемент, так как коллекция уже содержит максимальное их количество");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            ValuesCollection.Insert(index, new NestedElement<T>(value));
            ++Length;

            return true;
        }
        public bool Insert(INestableCollection<T> value, int index)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > Length - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (Length == int.MaxValue)
            {
                var exception = new Exception("Нельзя вставить элемент, так как коллекция уже содержит максимальное их количество");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            ValuesCollection.Insert(index, new NestedElement<T>(value));
            ++Length;

            return true;
        }

        public bool Remove()
        {
            if (Length < 1)
            {
                var exception = new Exception("Нельзя удалить элемент, так как коллекция уже пустая");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            ValuesCollection.RemoveAt(ValuesCollection.Count - 1);
            --Length;

            return true;
        }

        public bool RemoveAt(int index)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > Length - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (Length < 1)
            {
                var exception = new Exception("Нельзя удалить элемент, так как коллекция уже пустая");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            ValuesCollection.RemoveAt(index);
            --Length;

            return true;
        }

        public void Clear()
        {
            Length = 0;
            ValuesCollection.Clear();
        }

        public void CopyTo(Array array, int index)
        {
            if (array != null && array.Rank != 1)
            {
                var exception = new RankException("Копирование в многомерные массивы не поддерживается");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            if (array.GetValue(0).GetType() != typeof(T))
            {
                var exception = new ArrayTypeMismatchException("Для копирования тип целевого массива не может отличаться от типа текущей коллекции");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            List<T> thisCollection = Enumerate().ToList();

            if (thisCollection.Count < 1)
            {
                var exception = new Exception("Нельзя скопировать пустую коллекцию");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            if (array.Length - index < thisCollection.Count)
            {
                var exception = new Exception("Для копирования длина целевого массива, начиная с указанного индекса, не может быть меньше длины текущей коллекции");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            int arrayIndex = index;
            for (int i = 0; i < thisCollection.Count; ++i)
            {
                array.SetValue(thisCollection[i], arrayIndex);
                ++arrayIndex;
            }
        }

        public void CopyTo(IList<T> collection, bool clearBeforeCopy)
        {
            if (collection == null)
            {
                var exception = new ArgumentNullException(nameof(collection), "Целевая коллекция не может быть равна null");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            List<T> thisCollection = Enumerate().ToList();

            if (thisCollection.Count < 1)
            {
                var exception = new Exception("Нельзя скопировать пустую коллекцию");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (clearBeforeCopy)
                collection.Clear();

            for (int i = 0; i < thisCollection.Count; ++i)
            {
                collection.Add(thisCollection[i]);
            }
        }
    }
}
