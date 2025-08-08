using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Pipes;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Tree;



namespace Fred68.TreeItem
{
	public enum TreeSearchType {broadth_first, depth_first};

	public class TreeItem<T> where T : class, IFormattable
	{
		T						_data;		/// Data reference
		TreeItem<T>				_root;		// Root node, can be 'this'
		int						_depth;		// Node depth (0 if root)
		TreeItem<T>?			_prev;		// Previous node: 'father' node or null (if root)
		List<TreeItem<T>>?		_items;     // First level child items (can be null)

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
		public int Depth
		{
			get {return _depth; }
		}

		/// <summary>
		/// Root node (readonly)
		/// </summary>
		public TreeItem<T> Root
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
		/// IsLeaf (node with no sons)
		/// </summary>
		public bool IsLeaf
		{
			get
			{
				bool leaf = false;
				if(_items == null)
				{
					leaf = true;
				}
				else if (_items.Count == 0)
				{
					leaf = true;
				}
				return  leaf;
			}
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
				_prev._items.Insert(0, this);	// ...and add this to the list, at the beginning, to preserve creation order
			}
		}

		/// <summary>
		/// ToString() override
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			string prv = (this._prev != null) ? " <- "+this._prev.Data.ToString() : "<root>";
			string indent = new string('.', _depth);
			return $"{indent}{_data.ToString()} [{_depth}] ({prv})";
		}

		/// <summary>
		/// TreeItems to string
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public string ToString(TreeSearchType s)
		{
			StringBuilder sb = new StringBuilder();
			foreach(TreeItem<T> x in this.TreeItems(s))
			{
				sb.AppendLine(x.ToString());
			}
			return sb.ToString();
		}

		/// <summary>
		/// Classe di supporto per ToString()
		/// </summary>
		/// <typeparam name="TD"></typeparam>
		protected class DiagramItem<TD> where TD : class, IFormattable
		{
			public TreeItem<TD>		_treeIt;
			public int				_depth;
			public StringBuilder	_indent;
			public bool				_isLastChild;

			public DiagramItem(TreeItem<TD> itm, int iniDepth, bool isLastChild = false)
			{
				_treeIt = itm;
				_depth = itm._depth;
				_indent = new StringBuilder();
				_isLastChild = isLastChild;
			}

		}

		/// <summary>
		/// TreeItems to string, with tree links
		/// </summary>
		/// <param name="max_rel_depth"></param>
		/// <returns></returns>
		public string ToTreeString(int max_rel_depth = int.MaxValue)
		{
			const string _incrocio =	" |-";
			const string _terminale =	" L_";
			const string _vuoto =		"   ";
			const string _continua =	" | ";
			
			List<DiagramItem<T>> lst = new List<DiagramItem<T>>();
			
			Stack<DiagramItem<T>> stack = new Stack<DiagramItem<T>>();    // Stack, for depth-first search
			int depth_max = 0;
			stack.Push(new DiagramItem<T>(this, this._depth));
			while(stack.Count > 0)                                  // Search...
			{
				DiagramItem<T> item = stack.Pop();
				if(item._depth - this._depth < max_rel_depth)
				{
					if(item._treeIt._items != null)
					{
						for(int i = 0; i < item._treeIt._items.Count; i++)
						{
							bool last = (i == 0);
							TreeItem<T> chItem = item._treeIt._items[i];
							stack.Push(new DiagramItem<T>(chItem, this._depth, last));
						}
					}
				}
				if(item._depth > depth_max)	depth_max = item._depth;
				lst.Add(item);
			}

			StringBuilder sb = new StringBuilder();
			for(int dpth = 1; dpth <= depth_max; dpth++)
			{
				bool cont_level = false;			// Continue level connection
				foreach(DiagramItem<T> item in lst)
				{
					string ch = _vuoto;
					if(item._depth >= dpth)			// Take only nodes of enough dept into account...
					{
						if(item._depth == dpth)		// If current tree depth
						{
							cont_level = true;
							if(item._isLastChild)
							{
								
								cont_level = false;		// Last child
								ch = _terminale;
							}
							else
							{
								ch = _incrocio;			// Child, but not the last one
							}
						}
						else
						{
							if(cont_level)			// Different depth
							{
								ch = _continua;
							}
						}
						item._indent.Append(ch);
					}
				}
			}

			foreach(DiagramItem<T> item in lst)
			{
				sb.AppendLine($"{item._indent}{item._treeIt.Data.ToString()}");
			}
			

			return sb.ToString();
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
		/// First level child items count
		/// </summary>
		/// <returns></returns>
		public int ItemsCount()
		{
			int count = 0;
			if(_items != null)	count = _items.Count;
			return count;
		}

		/// <summary>
		/// Enumerate child items
		/// </summary>
		/// <param name="max_rel_depth">max relative depth</param>
		/// <returns></returns>
		public IEnumerable<TreeItem<T>> TreeItems(TreeSearchType s = TreeSearchType.broadth_first, int max_rel_depth = int.MaxValue)
		{
			#warning Eventualmente implementare una Deque<T> come array (che non parte da zero...) copiando da list.cs (non usare list<T>, InsertAt(0) è lento)

			if( s == TreeSearchType.broadth_first)
			{
				Queue<TreeItem<T>> queue = new Queue<TreeItem<T>>();	// Queue, for breadth-first search
				queue.Enqueue(this);
				while(queue.Count > 0)									// Search...
				{
					TreeItem<T> item = queue.Dequeue();
					if(item._depth - this._depth < max_rel_depth)
					{
						foreach(TreeItem<T> chItem in item.Items())
						{
							queue.Enqueue(chItem);
						}
					}
					yield return item;
				}
			}
			else if( s == TreeSearchType.depth_first)
			{
				Stack<TreeItem<T>> stack = new Stack<TreeItem<T>>();	// Stack, for depth-first search
				stack.Push(this);
				while(stack.Count > 0)									// Search...
				{
					TreeItem<T> item = stack.Pop();
					if(item._depth - this._depth < max_rel_depth)
					{
						foreach(TreeItem<T> chItem in item.Items())
						{
							stack.Push(chItem);
						}
					}
					yield return item;
				}
			}
			yield break;
		}

		/// <summary>
		/// Child items to list
		/// </summary>
		/// <param name="max_rel_depth">max relative depth</param>
		/// <returns></returns>
		public List<TreeItem<T>> TreeItemsToList(TreeSearchType s = TreeSearchType.broadth_first, int max_rel_depth = int.MaxValue)
		{
			List<TreeItem<T>> il = new List<TreeItem<T>>();
			foreach(TreeItem<T> item in TreeItems(s,max_rel_depth))
				{
				il.Add(item);
				}
			return il;
		}

		/// <summary>
		/// Enumerate child items data
		/// </summary>
		/// <param name="max_rel_depth">max relative depth</param>
		/// <returns></returns>
		public IEnumerable<T> TreeItemsData(TreeSearchType s = TreeSearchType.broadth_first, int max_rel_depth = int.MaxValue)
		{
			foreach(TreeItem<T> t in this.TreeItems(s,max_rel_depth))
			{
				yield return t.Data;
			}
			yield break;
		}

		/// <summary>
		/// Add item to 'this' first level child items
		/// </summary>
		/// <param name="item">TreeItem to be added as a child node of 'this'</param>
		/// <returns></returns>
		public TreeItem<T>? Add(TreeItem<T>? item)
		{
			TreeItem<T>? ret = null;
			if(item != null)
			{
				if(!item.TreeItemsToList().Contains(this))		// If 'this' is not a child of item... 
				{
					if (_items == null)							// Create _items, if not done yet
					{
						_items = new List<TreeItem<T>>();
					}
					item._prev = this;							// Add item to 'this'
					_items.Add(item);
					UpdateTree(item);							// Update tree under item
				
					ret = item;
				}
			}
			return ret;
		}

		/// <summary>
		/// Remove item from 'this' first level child items
		/// </summary>
		/// <param name="item">TreeItem to be removed from child nodes of 'this'</param>
		/// <returns></returns>
		public TreeItem<T>? Remove(TreeItem<T>? item)
		{
			TreeItem<T>? ret = null;
			if((_items != null) && (item!=null))
			{
				if(_items.Contains(item))
				{
					item._prev = null;					// Remove item form 'this'
					_items.Remove(item);
					UpdateTree(item);					// Update tree under item
					ret = item;
				}
			}
			return ret;
		}

		/// <summary>
		/// Update root and depths in item sub-tree, using item previous node (null if item is root)...
		/// ...using TreeItems() broadth-first search and skipping first element of the search (item).
		/// </summary>
		/// <param name="item">item must have</param>
		protected void UpdateTree(TreeItem<T> item)
		{
			TreeItem<T> root;

			if(item._prev != null)
			{
				root = item._prev._root;
				item._depth = item._prev._depth+1;
			}
			else
			{
				root = item;
				item._depth = 0;
			}
			bool keep = false;
			foreach(TreeItem<T> i in item.TreeItems())
			{
				if(keep && (i._prev != null))		// Skip first element (item)
				{
					i._root = root;
					i._depth = i._prev._depth+1;
				}
				keep = true;
			}
		}

		/// <summary>
		/// Clear the node and its tree
		/// </summary>
		public void Clear()
		{
			TreeItem<T>? prev = this._prev;
			if(prev != null)
			{
				prev.Remove(this);
			}

			List<TreeItem<T>> list = TreeItemsToList();
			
			list.RemoveAt(0);						// Remove first node (this)
			foreach(TreeItem<T> itm in list)
			{
				itm._data = null;					// Set null pointer, easing GC to dealloc data
				itm._depth = 0;
				itm._prev = null;
				if(itm._items != null)
				{
					itm._items.Clear();
					itm._items = null;
				}
			}

			if(this._items != null)					// Clear this items				
			{
				this._items.Clear();
				this._items = null;
			}

		}
	}
}
