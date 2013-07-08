﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Vixen.IO;
using Vixen.Module.SequenceFilter;
using Vixen.Sys;
using Vixen.Module.Property;
using System.Drawing;

namespace Vixen.Sys
{
	/// <summary>
	/// A logical node that encapsulates a single Element or a branch/group of other ElementNodes.
	/// </summary>
	[Serializable]
	public class ElementNode : GroupNode<Element>, IEqualityComparer<ElementNode>
	{
		// Making this static so there doesn't have to be potentially thousands of
		// subscriptions from the node manager.
		public static event EventHandler Changed;

		#region Constructors

		internal ElementNode(Guid id, string name, Element element, IEnumerable<ElementNode> content)
			: base(name, content)
		{
			if (VixenSystem.Nodes.ElementNodeExists(id)) {
				throw new InvalidOperationException("Trying to create a ElementNode that already exists.");
			}
			else {
				VixenSystem.Nodes.SetElementNode(id, this);
			}
			Id = id;
			Element = element;
			Properties = new PropertyManager(this);
		}

		internal ElementNode(string name, Element element, IEnumerable<ElementNode> content)
			: this(Guid.NewGuid(), name, element, content)
		{
		}

		internal ElementNode(string name, IEnumerable<ElementNode> content)
			: this(name, null, content)
		{
		}

		private ElementNode(Guid id, string name, Element element, params ElementNode[] content)
			: this(id, name, element, content as IEnumerable<ElementNode>)
		{
		}

		internal ElementNode(string name, Element element, params ElementNode[] content)
			: this(name, element, content as IEnumerable<ElementNode>)
		{
		}

		internal ElementNode(string name, params ElementNode[] content)
			: this(name, null, content)
		{
		}

		#endregion

		private Element _element;

		public Element Element
		{
			get { return _element; }
			set
			{
				if (_element != null) {
					VixenSystem.Elements.RemoveElement(_element);
				}

				_element = value;

				if (_element != null) {
					// this Element should be unique to this ElementNode. If it already exists in the element -> ElementNode
					// mapping in the Element Manager, something Very Bad (tm) has happened.
					if (VixenSystem.Elements.GetElementNodeForElement(value) != null) {
						VixenSystem.Logging.Error("ElementNode: assigning element (id: " + value.Id + ") to this ElementNode (id: " + Id +
						                          "), but it already exists in another ElementNode! (id: " +
						                          VixenSystem.Elements.GetElementNodeForElement(value).Id + ")");
					}

					VixenSystem.Elements.SetElementNodeForElement(_element, this);
				}
			}
		}

		public Guid Id { get; private set; }

		public override string Name
		{
			get { return base.Name; }
			set
			{
				base.Name = value;
				if (_element != null) {
					_element.Name = value;
				}
			}
		}

		public new ElementNode Find(string childName)
		{
			return base.Find(childName) as ElementNode;
		}

		public new IEnumerable<ElementNode> Children
		{
			get { return base.Children.Cast<ElementNode>(); }
		}

		public new IEnumerable<ElementNode> Parents
		{
			get { return base.Parents.Cast<ElementNode>(); }
		}

		public bool Masked
		{
			get { return this.All(x => x.Masked); }
			set
			{
				foreach (Element element in this) {
					element.Masked = value;
				}
			}
		}

		public bool IsLeaf
		{
			get { return !base.Children.Any(); }
		}

		/// <summary>
		/// Finds all nodes that would be considered invalid children for this node. This is effectively the
		/// node itself, and any parent nodes it has. It also includes any immediate child nodes.
		/// </summary>
		/// <returns>An enumeration of invalid child nodes for this node.</returns>
		public IEnumerable<ElementNode> InvalidChildren()
		{
			HashSet<ElementNode> result = new HashSet<ElementNode>();

			// the node itself is an invalid child for itself!
			result.Add(this);

			// any children it already has are invalid.
			result.AddRange(Children);

			// any parents it has (all the way back to root) are invalid,
			// otherwise that will create loops.
			result.AddRange(GetAllParentNodes());

			return result;
		}

		public PropertyManager Properties { get; private set; }

		#region Overrides

		public override void AddChild(GroupNode<Element> node)
		{
			base.AddChild(node);
			OnChanged(this);
		}

		public override void InsertChild(int index, GroupNode<Element> node)
		{
			base.InsertChild(index, node);
			OnChanged(this);
		}

		public override bool RemoveFromParent(GroupNode<Element> parent, bool cleanupIfFloating)
		{
			bool result = base.RemoveFromParent(parent, cleanupIfFloating);

			// if we're cleaning up if we're a floating node (eg. being deleted), and we're actually
			// floating, then remove the associated element (if any)
			if (cleanupIfFloating && Parents.Count() == 0) {
				Element = null;
				VixenSystem.Nodes.ClearElementNode(Id);
			}

			OnChanged(this);
			return result;
		}

		public override bool RemoveChild(GroupNode<Element> node)
		{
			bool result = base.RemoveChild(node);
			OnChanged(this);
			return result;
		}

		public override GroupNode<Element> Get(int index)
		{
			if (IsLeaf) throw new InvalidOperationException("Cannot get child nodes from a leaf.");
			return base.Get(index);
		}

		public override IEnumerator<Element> GetEnumerator()
		{
			return GetElementEnumerator().GetEnumerator();
		}

		#endregion

		#region Enumerators

		public IEnumerable<Element> GetElementEnumerator()
		{
			if (IsLeaf) {
				// Element is already an enumerable, so AsEnumerable<> won't work.
				return (new[] {Element});
			}
			else {
				return this.Children.SelectMany(x => x.GetElementEnumerator());
			}
		}

		public IEnumerable<ElementNode> GetNodeEnumerator()
		{
			// "this" is already an enumerable, so AsEnumerable<> won't work.
			return (new[] {this}).Concat(Children.SelectMany(x => x.GetNodeEnumerator()));
		}

		public IEnumerable<ElementNode> GetLeafEnumerator()
		{
			if (IsLeaf) {
				// Element is already an enumerable, so AsEnumerable<> won't work.
				return (new[] {this});
			}
			else {
				return Children.SelectMany(x => x.GetLeafEnumerator());
			}
		}

		public IEnumerable<ElementNode> GetNonLeafEnumerator()
		{
			if (IsLeaf) {
				return Enumerable.Empty<ElementNode>();
			}
			else {
				// "this" is already an enumerable, so AsEnumerable<> won't work.
				return (new[] {this}).Concat(Children.SelectMany(x => x.GetNonLeafEnumerator()));
			}
		}

		public IEnumerable<ElementNode> GetAllParentNodes()
		{
			return Parents.Concat(Parents.SelectMany(x => x.GetAllParentNodes()));
		}

		#endregion

		#region Static members

		protected static void OnChanged(ElementNode value)
		{
			if (Changed != null) {
				Changed(value, EventArgs.Empty);
			}
		}

		#endregion

		public bool Equals(ElementNode x, ElementNode y)
		{
			return x.Id == y.Id;
		}

		public int GetHashCode(ElementNode obj)
		{
			return Id.GetHashCode();
		}
	}
}