using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Fred68.TreeItem
{
	public class TreeItem<T> where T : class
	{
		T						_data;		/// Data reference
		TreeItem<T>				_root;		// Root node, can be 'this'
		int						_depth;		// Node depth (0 if root)
		TreeItem<T>?			_prev;		// Previous node: 'father' node or null (if root)
		List<TreeItem<T>>?		_items;		// First level child items (can be null)

		/// <summary>
		/// Data
		/// </summary>
		public T Data
		{
			get { return _data; }
			set { _data = value; }
		}
		
		/// <summary>
		/// Depth (readonly)
		/// </summary>
		public int Dept
		{
			get {return _depth; }
		}

		/// <summary>
		/// Root node (readonly)
		/// </summary>
		public TreeItem<T>	Root
		{
			get { return _root; }
		}

		/// <summary>
		/// IsRoot
		/// </summary>
		public bool IsRoot
		{
			get {return (_prev == null);}
		}

		/// <summary>
		/// Previous node (or null)
		/// </summary>
		public TreeItem<T>? Previous
		{
			get { return _prev;}
		}

		/// <summary>
		/// CTOR
		/// </summary>
		/// <param name="data">class T object</param>
		/// <param name="prev">previous tree node</param>
		public TreeItem(T data, TreeItem<T>? prev)
		{
			_data = data;					// Set data
			if(prev == null)				// 'this' is a root node (no previous node)
			{
				_root = this;
				_prev = null;
				_depth = 0;
			}
			else							// 'this' is a child node
			{
				_prev = prev;				// Set previous, root and depth
				_root = prev._root;
				_depth = prev._depth + 1;
				if(_prev._items == null)	// Add this to previous node child items: create list if already null...
				{
					_prev._items = new List<TreeItem<T>>();
				}
				_prev._items.Add(this);		// ...and add this to the list
			}
		}

		/// <summary>
		/// ToString() override
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			string prv = (this._prev != null) ? " <- "+this._prev.Data.ToString() : "<root>";
			return $"[{_depth}] {_data.ToString()} ({prv})";
		}

		/// <summary>
		/// Enumerate first level child items
		/// </summary>
		/// <returns></returns>
		public IEnumerable<TreeItem<T>> Items()
		{
			if(_items != null)
			{
				foreach(TreeItem<T> item in _items)
				{
					yield return item;
				}
			}
			yield break;
		}
		
		/// <summary>
		/// Enumerate child items
		/// </summary>
		/// <param name="max_rel_depth">max relative depth</param>
		/// <returns></returns>
		public IEnumerable<TreeItem<T>> TreeItems(int max_rel_depth = int.MaxValue)
		{
			Queue<TreeItem<T>> collect = new Queue<TreeItem<T>>();	// Queue, for breadth-first search (if depth-first search, use a Stack)
			collect.Enqueue(this);
			while(collect.Count > 0)								// Search...
			{
				TreeItem<T> item = collect.Dequeue();
				if(item._depth - this._depth < max_rel_depth)
				{
					foreach(TreeItem<T> chItem in item.Items())
					{
						collect.Enqueue(chItem);
					}
				}
				yield return item;
			}
			yield break;
		}

		/// <summary>
		/// Child items to list
		/// </summary>
		/// <param name="max_rel_depth">max relative depth</param>
		/// <returns></returns>
		public List<TreeItem<T>> TreeItemsToList(int max_rel_depth = int.MaxValue)
		{
			List<TreeItem<T>> il = new List<TreeItem<T>>();
			foreach(TreeItem<T> item in Items()) { il.Add(item); }
			return il;
		}

		/// <summary>
		/// Enumerate child items data
		/// </summary>
		/// <param name="max_rel_depth">max relative depth</param>
		/// <returns></returns>
		public IEnumerable<T> ItemsData(int max_rel_depth = int.MaxValue)
		{
			foreach(TreeItem<T> t in this.TreeItems(max_rel_depth))
			{
				yield return t.Data;
			}
			yield break;
		}

		#warning Rimozione di un nodo: restituisce il nodo, aggiornare _depth e _root in tutti i nodi figlio
		
		#warning CREARE FUNZIONE DI AGGIORNAMENTO PARTENDO DA UN NODO (ROOT O NON NODO PADRE)
		#warning DA PROVARE (inseriment ciclico con errore, inserimento normale, controllo depth e root)
		/// <summary>
		/// Add item to 'this' first level child items
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public TreeItem<T>? Add(TreeItem<T> item)
		{
			TreeItem<T>? ret = null;
			if(!item.TreeItemsToList().Contains(this))		// If 'this' is not a child of item... 
			{
				if (_items == null)							// Create _items, if not done yet
				{
					_items = new List<TreeItem<T>>();
				}
				_items.Add(item);							// Add item to 'this'
				item._prev = this;

				#warning CORREGGERE con chiamata a funzione di aggiornamento !
				item._depth = this._depth + 1;
				item._root = this._root;

				ret = item;
			}
			return ret;
		}

		/// <summary>
		/// Remove item from 'this' first level child items
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public TreeItem<T>? Remove(TreeItem<T> item)
		{
			TreeItem<T>? ret = null;
			if(_items != null)
			{
				if(_items.Contains(item))
				{
					_items.Remove(item);
					item._prev = null;
				}
			}
			return ret;
		}
	}
}
