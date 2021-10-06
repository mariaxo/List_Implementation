using System;
using System.Collections.Generic;
using System.Text;

namespace List_Implementation
{
    class Program
    {
        static void Main(string[] args)
        {
			var arr = new[] { 4, 5, 6, 9, 10 };

			MyList<StringBuilder> listl = new MyList<StringBuilder>();
			listl.Add(new StringBuilder("1"));
			listl.Add(new StringBuilder("2"));
			listl.Add(new StringBuilder("3"));
			listl.Add(new StringBuilder("4"));
			listl.Add(new StringBuilder("5"));
			listl.Add(new StringBuilder("6"));
			listl.Add(new StringBuilder("7"));
			listl.Add(new StringBuilder("8"));
			listl.Add(new StringBuilder("9"));
			listl.Add(new StringBuilder("10"));
			listl.Add(new StringBuilder("11"));
			listl.Clear();

			int a = listl.Count;

		}

		//----Big O's of List<T> methods----

		//Add - O(1) or O(n) in worst case

		//Insert - O(n)
		//Remove - O(n)
		//RemoveAt - O(n) 
		//RemoveAll - O(n) 
		//IndexOf - O(n) 
		//Clear - O(n)        NOTE: it's only for value types when the values are not being removed
		//Contains - O(n) 
		//ConvertAll - O(n) 
		//CopyTo - O(n) 
		//Find - O(n) 
		//FindAll - O(n) 
		//FindIndex - O(n) 
		//FindLast - O(n) 
		//FindLastIndex - O(n) 
		//Foreach - O(n) 
		//GetRange - O(n) 
		//LastIndexOf - O(n) 
		//Reverse - O(n) 
		//ToArray - O(n)
		//TrimExcess - O(n)
		//TrueForAll - O(n)

		//AddRange - O(n + m) where M = Count, N = number of elements to be added
		//InsertRange - O(n + m) where M = Count, N = number of elements to be added

		//BinarySearch - O(log(n))
		//Sort - O(n * log(n))

	}
}

