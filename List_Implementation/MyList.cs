using System;
using System.Collections;
using System.Collections.Generic;

namespace List_Implementation
{
	class MyList<T>
	{
		#region Private members

		private const int _defaultCapacity = 4;

		private T[] _items;
		private int _size;
		private int _version;
		[NonSerialized]
		private Object _syncRoot;

		static readonly T[] _emptyArray = new T[0];


		private static bool IsCompatibleObject(object value)
		{
			// Non-null values are fine.  Only accept nulls if T is a class or Nullable<U>.
			// Note that default(T) is not equal to null for value types except when T is Nullable<U>. 
			return ((value is T) || (value == null && default(T) == null));
		}
		private void EnsureCapacity(int min)
		{
			if (_items.Length < min)
			{
				int newCapacity = _items.Length == 0 ? _defaultCapacity : _items.Length * 2;
				// Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
				// Note that this check works even when _items.Length overflowed thanks to the (uint) cast
				if (newCapacity < min) newCapacity = min;
				Capacity = newCapacity;
			}
		}
		private static int GetMedian(int low, int hi)
		{
			return low + ((hi - low) >> 1);
		}
		private int BinarySearchRecursion<T>(T[] array, int startIndex, int lastIndex, T value, IComparer<T>? comparer)
		{
			//Input validations
			if (array == null)
				throw new ArgumentNullException(nameof(array));
			if (comparer == null)
				comparer = Comparer<T>.Default;

			//Binary search won't search for null values
			if (value == null)
				return -1;

			//Searching algorithm starts here
			if (lastIndex >= startIndex)
			{
				int midIndex = GetMedian(startIndex, lastIndex);

				int comparisonResult = comparer.Compare(array[midIndex], value);

				if (comparisonResult == 0)
				{
					return midIndex;
				}
				else if (comparisonResult < 0)
				{
					return BinarySearchRecursion<T>(array, midIndex + 1, lastIndex, value, comparer); //Take the right part
				}
				else
				{
					return BinarySearchRecursion<T>(array, startIndex, midIndex - 1, value, comparer); //Take the left part
				}
			}

			return -1;
		}
		private int BinarySearchIteration<T>(T[] array, int startIndex, int lastIndex, T value, IComparer<T>? comparer)
		{
			//Input validations
			if (array == null)
				throw new ArgumentNullException(nameof(array));
			if (lastIndex - startIndex < 0)
				throw new ArgumentException($"Invalid offset length. ({nameof(startIndex)} - {nameof(lastIndex)} < 0)");
			if (comparer == null)
				comparer = Comparer<T>.Default;

			//Binary search won't search for null values
			if (value == null)
				return -1;

			//Searching algorithm starts here
			while (lastIndex >= startIndex)
			{
				int midIndex = GetMedian(startIndex, lastIndex);
				int comparisonResult = comparer.Compare(array[midIndex], value);

				if (comparisonResult == 0)
				{
					return midIndex;
				}
				else if (comparisonResult < 0)
				{
					startIndex = midIndex + 1; // Take the 'right' part
				}
				else
				{
					lastIndex = midIndex - 1; //Take the 'left' part
				}
			}

			return -1;

		}
		#endregion

		public MyList()
		{
			_items = _emptyArray;
		}
		public MyList(int capacity)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be equal or greater than zero.");

			if (capacity == 0)
				_items = _emptyArray;
			else
				_items = new T[capacity];

		}
		public MyList(IEnumerable<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));

			ICollection<T> c = collection as ICollection<T>;

			if (c != null)
			{
				int count = c.Count;

				if (count == 0)
				{
					_items = _emptyArray;
				}
				else
				{
					_items = new T[count];
					c.CopyTo(_items, 0);
					_size = count;
				}
			}
			else
			{
				_size = 0;
				_items = _emptyArray;

				foreach (var item in collection)
				{
					this.Add(item);
				}
			}
		}

		public int Capacity
		{
			get
			{
				return _items.Length;
			}
			set
			{
				if (value < _size)
					throw new ArgumentOutOfRangeException(nameof(value), "Capacity must be equal or greater than count of elements in the list");

				if (value != _items.Length)
				{
					if (value > 0)
					{
						T[] newItems = new T[value];
						if (_size > 0)
						{
							Array.Copy(_items, newItems, _size);
						}
						_items = newItems;
					}
					else
					{
						_items = _emptyArray;
					}
				}
			}
		}

		public int Count
		{
			get
			{
				return _size;
			}
		}

		public T this[int index]
		{
			get
			{
				if ((uint)index >= (uint)_size)
					throw new ArgumentOutOfRangeException();

				return _items[index];
			}
			set
			{
				if ((uint)index >= (uint)_size)
					throw new ArgumentOutOfRangeException();

				_items[index] = value;
				_version++;
			}
		}

		public void Add(T item)
		{
			if (_size == _items.Length)
				EnsureCapacity(_size + 1);

			_items[_size++] = item;
			_version++;
		}
		public void AddRange(IEnumerable<T> collection)
		{
			this.InsertRange(_size, collection);
		}

		public void Insert(int index, T item)
		{
			// Note that insertions at the end are legal.
			if ((uint)index > (uint)_size)
			{
				throw new ArgumentOutOfRangeException(nameof(index), "Argument was outside of bounds of the array.");
			}

			if (_size == _items.Length)
				EnsureCapacity(_size + 1);

			if (index < _size)
			{
				Array.Copy(_items, index, _items, index + 1, _size - index);
			}
			_items[index] = item;
			_size++;
			_version++;
		}
		public void InsertRange(int index, IEnumerable<T> collection)
		{
			if (collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}

			if ((uint)index > (uint)_size)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			ICollection<T> c = collection as ICollection<T>;
			if (c != null)
			{    // if collection is ICollection<T>
				int count = c.Count;
				if (count > 0)
				{
					EnsureCapacity(_size + count);
					if (index < _size)
					{
						Array.Copy(_items, index, _items, index + count, _size - index);
					}

					// If we're inserting a List into itself, we want to be able to deal with that.
					if (this == c)
					{
						// Copy first part of _items to insert location
						Array.Copy(_items, 0, _items, index, index);
						// Copy last part of _items back to inserted location
						Array.Copy(_items, index + count, _items, index * 2, _size - index);
					}
					else
					{
						T[] itemsToInsert = new T[count];
						c.CopyTo(itemsToInsert, 0);
						itemsToInsert.CopyTo(_items, index);
					}
					_size += count;
				}
			}
			else
			{
				using (IEnumerator<T> en = collection.GetEnumerator())
				{
					while (en.MoveNext())
					{
						Insert(index++, en.Current);
					}
				}
			}
			_version++;
		}

		public bool Remove(T item)
		{
			int index = IndexOf(item);
			if (index >= 0)
			{
				RemoveAt(index);
				return true;
			}

			return false;
		}
		public void RemoveAt(int index)
		{
			if ((uint)index >= (uint)_size)
			{
				throw new ArgumentOutOfRangeException();
			}

			_size--;

			if (index < _size)
			{
				Array.Copy(_items, index + 1, _items, index, _size - index);
			}
			_items[_size] = default(T);
			_version++;
		}
		public void RemoveRange(int index, int count)
		{
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(index), "The parameter 'index' must be non-negative.");
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), "The parameter 'count' must be non-negative.");
			}

			if (_size - index < count)
				throw new ArgumentException("Invalid offset length");

			if (count > 0)
			{
				int i = _size;
				_size -= count;
				if (index < _size)
				{
					Array.Copy(_items, index + count, _items, index, _size - index);
				}
				Array.Clear(_items, _size, count);
				_version++;
			}
		}
		public int RemoveAll(Predicate<T> match)
		{
			if (match == null)
			{
				throw new ArgumentNullException(nameof(match));
			}

			int freeIndex = 0;   // the first free slot in items array

			// Find the first item which needs to be removed.
			while (freeIndex < _size && !match(_items[freeIndex])) freeIndex++;
			if (freeIndex >= _size) return 0;

			int current = freeIndex + 1;
			while (current < _size)
			{
				// Find the first item which needs to be kept.
				while (current < _size && match(_items[current])) current++;

				if (current < _size)
				{
					// copy item to the free slot.
					_items[freeIndex++] = _items[current++];
				}
			}

			Array.Clear(_items, freeIndex, _size - freeIndex);
			int result = _size - freeIndex;
			_size = freeIndex;
			_version++;
			return result;
		}

		public int IndexOf(T item)
		{
			return Array.IndexOf(_items, item, 0, _size);
		}
		public int IndexOf(T item, int index)
		{
			if (index > _size)
				throw new ArgumentOutOfRangeException(nameof(index));

			return Array.IndexOf(_items, item, index, _size - index);
		}
		public int IndexOf(T item, int index, int count)
		{
			if (index > _size)
				throw new ArgumentOutOfRangeException(nameof(index));

			if (count < 0 || index > _size - count) throw new ArgumentOutOfRangeException(nameof(count));

			return Array.IndexOf(_items, item, index, count);
		}

		public void Clear()
		{
			if (_size > 0)
			{
				Array.Clear(_items, 0, _size); // Don't need to doc this but we clear the elements so that the gc can reclaim the references.
				_size = 0;
			}
			_version++;
		}
		public bool Contains(T item)
		{
			if ((Object)item == null)
			{
				for (int i = 0; i < _size; i++)
					if ((Object)_items[i] == null)
						return true;
				return false;
			}
			else
			{
				EqualityComparer<T> c = EqualityComparer<T>.Default;
				for (int i = 0; i < _size; i++)
				{
					if (c.Equals(_items[i], item))
						return true;
				}
				return false;
			}
		}

		public int BinarySearch(T item)
		{
			return BinarySearch(0, Count, item, null);
		}
		public int BinarySearch(T item, IComparer<T> comparer)
		{
			return BinarySearch(0, Count, item, comparer);
		}
		public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException(nameof(index));
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count));
			if (_size - index < count)
				throw new ArgumentException("Invalid offset length.");

			return BinarySearchRecursion<T>(_items, index, count + index - 1, item, comparer);
		}

		public MyList<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
		{
			if (converter == null)
			{
				throw new ArgumentNullException(nameof(converter));
			}

			MyList<TOutput> list = new MyList<TOutput>(_size);
			for (int i = 0; i < _size; i++)
			{
				list._items[i] = converter(_items[i]);
			}
			list._size = _size;
			return list;
		}

		public void CopyTo(T[] array)
		{
			CopyTo(array, 0);
		}
		public void CopyTo(int index, T[] array, int arrayIndex, int count)
		{
			if (_size - index < count)
			{
				throw new ArgumentException("Invalid offset length");
			}

			// Delegate rest of error checking to Array.Copy.
			Array.Copy(_items, index, array, arrayIndex, count);
		}
		public void CopyTo(T[] array, int arrayIndex)
		{
			// Delegate rest of error checking to Array.Copy.
			Array.Copy(_items, 0, array, arrayIndex, _size);
		}

		public T Find(Predicate<T> match)
		{
			if (match == null)
			{
				throw new ArgumentNullException(nameof(match));
			}

			for (int i = 0; i < _size; i++)
			{
				if (match(_items[i]))
				{
					return _items[i];
				}
			}
			return default(T);
		}
		public MyList<T> FindAll(Predicate<T> match)
		{
			if (match == null)
			{
				throw new ArgumentNullException(nameof(match));
			}

			MyList<T> list = new MyList<T>();
			for (int i = 0; i < _size; i++)
			{
				if (match(_items[i]))
				{
					list.Add(_items[i]);
				}
			}
			return list;
		}

		public int FindIndex(Predicate<T> match)
		{
			return FindIndex(0, _size, match);
		}
		public int FindIndex(int startIndex, Predicate<T> match)
		{
			return FindIndex(startIndex, _size - startIndex, match);
		}
		public int FindIndex(int startIndex, int count, Predicate<T> match)
		{
			if ((uint)startIndex > (uint)_size)
			{
				throw new ArgumentOutOfRangeException(nameof(startIndex));
			}

			if (count < 0 || startIndex > _size - count)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			if (match == null)
			{
				throw new ArgumentNullException(nameof(match));
			}

			int endIndex = startIndex + count;
			for (int i = startIndex; i < endIndex; i++)
			{
				if (match(_items[i])) return i;
			}
			return -1;
		}

		public T FindLast(Predicate<T> match)
		{
			if (match == null)
			{
				throw new ArgumentNullException(nameof(match));
			}

			for (int i = _size - 1; i >= 0; i--)
			{
				if (match(_items[i]))
				{
					return _items[i];
				}
			}
			return default(T);
		}
		public int FindLastIndex(Predicate<T> match)
		{
			return FindLastIndex(_size - 1, _size, match);
		}
		public int FindLastIndex(int startIndex, Predicate<T> match)
		{
			return FindLastIndex(startIndex, startIndex + 1, match);
		}
		public int FindLastIndex(int startIndex, int count, Predicate<T> match)
		{
			if (match == null)
			{
				throw new ArgumentNullException(nameof(match));
			}

			if (_size == 0)
			{
				// Special case for 0 length List
				if (startIndex != -1)
				{
					throw new ArgumentOutOfRangeException(nameof(startIndex));
				}
			}
			else
			{
				// Make sure we're not out of range            
				if ((uint)startIndex >= (uint)_size)
				{
					throw new ArgumentOutOfRangeException(nameof(startIndex));
				}
			}

			// 2nd have of this also catches when startIndex == MAXINT, so MAXINT - 0 + 1 == -1, which is < 0.
			if (count < 0 || startIndex - count + 1 < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			int endIndex = startIndex - count;
			for (int i = startIndex; i > endIndex; i--)
			{
				if (match(_items[i]))
				{
					return i;
				}
			}
			return -1;
		}

		public void ForEach(Action<T> action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			int version = _version;

			for (int i = 0; i < _size; i++)
			{
				if (version != _version)
				{
					break;
				}
				action(_items[i]);
			}

			if (version != _version)
				throw new InvalidOperationException("ExceptionResource.InvalidOperation_EnumFailedVersion");
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		public MyList<T> GetRange(int index, int count)
		{
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			if (_size - index < count)
			{
				throw new InvalidOperationException("Invalid offset length.");
			}

			MyList<T> list = new MyList<T>(count);
			Array.Copy(_items, index, list._items, 0, count);
			list._size = count;
			return list;
		}

		public int LastIndexOf(T item)
		{
			if (_size == 0)
			{  // Special case for empty list
				return -1;
			}
			else
			{
				return LastIndexOf(item, _size - 1, _size);
			}
		}
		public int LastIndexOf(T item, int index)
		{
			if (index >= _size)
				throw new ArgumentOutOfRangeException(nameof(index));

			return LastIndexOf(item, index, index + 1);
		}
		public int LastIndexOf(T item, int index, int count)
		{
			if ((Count != 0) && (index < 0))
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			if ((Count != 0) && (count < 0))
			{
				throw new ArgumentOutOfRangeException(nameof(count), "The parameter 'count' must be non-negative");
			}

			if (_size == 0)
			{  // Special case for empty list
				return -1;
			}

			if (index >= _size)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			if (count > index + 1)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			return Array.LastIndexOf(_items, item, index, count);
		}

		public void Reverse()
		{
			Reverse(0, Count);
		}
		public void Reverse(int index, int count)
		{
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			if (_size - index < count)
				throw new InvalidOperationException("Invalid offset length.");

			Array.Reverse(_items, index, count);
			_version++;
		}

		public void Sort()
		{
			Sort(0, Count, null);
		}
		public void Sort(IComparer<T> comparer)
		{
			Sort(0, Count, comparer);
		}
		public void Sort(int index, int count, IComparer<T> comparer)
		{
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			if (_size - index < count)
				throw new ArgumentException("Invalid offset length");

			Array.Sort<T>(_items, index, count, comparer);
			_version++;
		}
		public void Sort(Comparison<T> comparison)
		{
			if (comparison == null)
			{
				throw new ArgumentNullException(nameof(comparison));
			}

			if (_size > 0)
			{
				IComparer<T> comparer = new FunctorComparer<T>(comparison);
				Array.Sort(_items, 0, _size, comparer);
			}
		}

		public T[] ToArray()
		{
			T[] array = new T[_size];
			Array.Copy(_items, 0, array, 0, _size);
			return array;
		}

		public void TrimExcess()
		{
			int threshold = (int)(((double)_items.Length) * 0.9);
			if (_size < threshold)
			{
				Capacity = _size;
			}
		}

		public bool TrueForAll(Predicate<T> match)
		{
			if (match == null)
			{
				throw new ArgumentNullException(nameof(match));
			}

			for (int i = 0; i < _size; i++)
			{
				if (!match(_items[i]))
				{
					return false;
				}
			}
			return true;
		}


		public struct Enumerator : IEnumerator<T>, IEnumerator
		{
			private MyList<T> list;
			private int index;
			private int version;
			private T current;

			internal Enumerator(MyList<T> list)
			{
				this.list = list;
				index = 0;
				version = list._version;
				current = default(T);
			}

			public void Dispose()
			{
			}

			public bool MoveNext()
			{

				MyList<T> localList = list;

				if (version == localList._version && ((uint)index < (uint)localList._size))
				{
					current = localList._items[index];
					index++;
					return true;
				}
				return MoveNextRare();
			}

			private bool MoveNextRare()
			{
				if (version != list._version)
				{
					throw new InvalidOperationException("ExceptionResource.InvalidOperation_EnumFailedVersion");
				}

				index = list._size + 1;
				current = default(T);
				return false;
			}

			public T Current
			{
				get
				{
					return current;
				}
			}

			Object System.Collections.IEnumerator.Current
			{
				get
				{
					if (index == 0 || index == list._size + 1)
					{
						throw new InvalidOperationException("ExceptionResource.InvalidOperation_EnumFailedVersion");
					}
					return Current;
				}
			}

			void System.Collections.IEnumerator.Reset()
			{
				if (version != list._version)
				{
					throw new InvalidOperationException("ExceptionResource.InvalidOperation_EnumFailedVersion");
				}

				index = 0;
				current = default(T);
			}
		}

		internal sealed class FunctorComparer<T> : IComparer<T>
		{
			Comparison<T> comparison;

			public FunctorComparer(Comparison<T> comparison)
			{
				this.comparison = comparison;
			}

			public int Compare(T x, T y)
			{
				return comparison(x, y);
			}
		}

	}
}

