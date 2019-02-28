using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Framework for a non-directional graph with node values that handles user events
/// All translations occur in local space
/// </summary>
[System.Serializable]
public class UndirectedEventGraph : UndirectedGraph {
	private List<int> selectI = new List<int>(); public int[] SelectI => selectI.ToArray();
	public int DragI { get; private set; }
	public Vector2 StartDragPos { get; private set; }
	/// <summary>
	/// Offset between node position and drag anchor position in local space
	/// </summary>
	public Vector2 DragOffset { get; private set; } = Vector2.zero;
	/// <summary>
	/// Difference between start of drag and current drag position
	/// </summary>
	public Vector2 DragDelta => !this.Safe(DragI) ? Vector2.zero : this[DragI].pos - StartDragPos + DragOffset;

	public bool IsDragging => DragI != -1;
	public Node DragTarget => !IsDragging ? null : this[DragI];
	public Node[] Selected => selectI.Perform((i) => this[i]);

	/// <summary>
	/// Toggle node selection
	/// </summary>
	/// <param name="i">Target index of selection</param>
	public virtual void Select(int i) {
		if (i == -1) return;
		else if (selectI.Contains(i)) DeSelect(i);
		else selectI.Add(i);
	}
	public virtual void DeSelect(int i) { selectI.Remove(i); }
	public virtual void ClearSelection() => selectI.Clear();
	public virtual bool IsSelected(Node node) => selectI.Contains(IndexOf(node));
	public virtual bool AnySelected(params Node[] nodes) => selectI.ContainsAny(nodes.Perform(IndexOf));
	public virtual bool AnySelected(params string[] values) => AnySelected(values.Perform(FindNodesByValue).Concat());

	/// <summary>
	/// End all active events
	/// </summary>
	public virtual void ClearEvents() {
		EndDrag();
		selectI.Clear();
	}

	/// <summary>
	/// Starts dragging a physical node in world space
	/// </summary>
	/// <param name="startWorldPos">Start position in world space</param>
	/// <param name="i">Index of the drag target</param>
	/// <param name="toLocalSpace">Base change from world to local space</param>
	public virtual void StartDrag(Vector2 startWorldPos, int i, System.Func<Node, Vector2, Vector2> toLocalSpace) {
		if (DragI == -1 && i != -1) {
			DragI = i;
			StartDragPos = toLocalSpace == null ? startWorldPos : toLocalSpace(this[DragI], startWorldPos);
			DragOffset = StartDragPos - this[DragI].pos;
		}
	}
	/// <summary>
	/// Continue dragging a physical node in world space
	/// </summary>
	/// <param name="targetWorldPos">Target position in world space</param>
	/// <param name="toLocalSpace">Base change from world to local space</param>
	public virtual void Drag(Vector2 targetWorldPos, System.Func<Node, Vector2, Vector2> toLocalSpace) {
		if (DragI != -1)
			this[DragI].pos = (toLocalSpace == null ? targetWorldPos : toLocalSpace(this[DragI], targetWorldPos)) - DragOffset;
	}
	/// <summary>
	/// Stop dragging a physical node in local space
	/// </summary>
	public virtual void EndDrag() {
		DragI = -1;
		DragOffset = Vector2.zero;
	}
	
	public UndirectedEventGraph(params Node[] nodes) : base(nodes) {}

	public sealed override void Add(Node item) { base.Add(item); }
	/// <summary>
	/// Remove a node from the graph.
	/// Ends all active events
	/// </summary>
	/// <param name="item">Node to remove</param>
	public sealed override bool Remove(Node item) {
		ClearEvents();
		return base.Remove(item);
	}
	
	/// <summary>
	/// Remove currently selected node(s) from graph and end all active events
	/// </summary>
	public void RemoveSelected() {
		Selected.Perform(Remove);
	}
}
