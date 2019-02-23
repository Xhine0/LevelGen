using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Framework for a directional graph with node values
/// Nodes are physical (has a position and size in local space)
/// </summary>
[Serializable]
public class Graph : IList<Graph.Node> {
	[Serializable]
	public class Node {
		public string value;
		public Vector2 pos, size;
		public (string, Vector2, Vector2) Tuple => (value, pos, size);

		public Node(string value, Vector2 pos, Vector2 size) {
			this.value = value;
			this.pos = pos;
			this.size = size;
		}
		public Node(string value) {
			this.value = value;
			pos = Vector2.zero;
			size = Vector2.zero;
		}

		public override bool Equals(object obj) {
			var node = obj as Node;
			return node != null &&
				   value == node.value &&
				   pos.Equals(node.pos) &&
				   size.Equals(node.size);
		}

		public override int GetHashCode() {
			var hashCode = -2132632183;
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(value);
			hashCode = hashCode * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(pos);
			hashCode = hashCode * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(size);
			return hashCode;
		}

		public override string ToString() {
			return pos + ", " + value.ToString();
		}

		public static bool operator ==(Node node1, Node node2) {
			return EqualityComparer<Node>.Default.Equals(node1, node2);
		}

		public static bool operator !=(Node node1, Node node2) {
			return !(node1 == node2);
		}
	}
	[Serializable]
	public class Edge {
		public int ai, bi;
		public Edge(int ai, int bi) {
			this.ai = ai;
			this.bi = bi;
		}
		public (int, int) Tuple => (ai, bi);
	}

	[SerializeField]
	private List<Node> nodes = new List<Node>();
	[SerializeField]
	private List<Edge> edges = new List<Edge>();
	[SerializeField]
	private int root = 0;

	public Graph(params Node[] nodes) {
		this.nodes.AddAll(nodes);
	}
	private Graph(Graph other) {
		nodes = new List<Node>(other.nodes);
		edges = new List<Edge>(other.edges);
		root = other.root;
	}
	public Graph Clone() => new Graph(this);

	public Node this[int index] {
		get { return index >= 0 && index < Count ? nodes[index] : default; }
		set { if (index >= 0 && index < Count) nodes[index] = value; }
	}

	public Node Root {
		get { return this[root]; }
		set {
			int i = IndexOf(value);
			if (i >= 0 && i < Count)
				root = i;
		}
	}
	public int RootI => root;

	public bool IsReadOnly => false;

	public string[] NodeValues {
		get {
			List<string> values = new List<string>();
			foreach (Node node in nodes)
				values.Add(node.value);
			return values.ToArray();
		}
	}
	public int IndexOf(Node node) => nodes.IndexOf(node);

	public (string, Vector2, Vector2)[] Nodes => nodes.Perform(e => { return e.Tuple; });
	public (int, int)[] Edges => edges.Perform(e => { return e.Tuple; });
	public (Node, Node)[] NodeEdges {
		get {
			(Node, Node)[] result = new (Node, Node)[edges.Count];
			foreach (var (ai, bi) in Edges)
				result.Add((this[ai], this[bi]));
			return result;
		}
	}
	/// <summary>
	/// Return all nodes directly connected to a given node
	/// </summary>
	/// <param name="i">Origin node</param>
	/// <returns>Direct connections</returns>
	public virtual Node[] Connections(int i) {
		List<Node> result = new List<Node>();
		foreach (var (ai, bi) in Edges) {
			if (ai == i) result.Add(this[bi]);
		}
		return result.ToArray();
	}
	public int[] ConnectionsI(int i) => Connections(i).Perform(IndexOf);
	public Node[] Connections(Node node) => Connections(IndexOf(node));
	public Node[] Connections(string value) => FindNodesByValue(value).Perform(Connections).Concat();


	public bool IsRoot(int i) => root == i;
	public bool IsRoot(Node node) => IsRoot(IndexOf(node));

	public int Count => nodes.Count;
	public int EdgeCount => edges.Count;

	/// <summary>
	/// Update edge index references on change in node order
	/// </summary>
	/// <param name="ri">Removed index</param>
	private void UpdateIndices(int ri) {
		for (int i = edges.Count - 1; i >= 0; --i) { // Safe range removal
			var (ai, bi) = Edges[i];
			if (ai == ri || bi == ri) {
				edges.RemoveAt(i);
				continue;
			}
			if (ai > ri) --ai;
			if (bi > ri) --bi;
			edges[i] = new Edge(ai, bi);
		}
		if (root == ri) root = 0;
		else if (root > ri) --root;
	}

	/// <summary>
	/// Check if two nodes are directly connected
	/// </summary>
	/// <param name="ai">First node index</param>
	/// <param name="bi">Second node index</param>
	/// <returns>If edge exists</returns>
	public virtual bool Connected(int ai, int bi) => Edges.Contains((ai, bi));
	public bool Connected(Node a, Node b) => Connected(IndexOf(a), IndexOf(b));

	/// <summary>
	/// Create a direct connection between two nodes
	/// </summary>
	/// <param name="ai">First node index</param>
	/// <param name="bi">Second node index</param>
	public virtual bool Connect(int ai, int bi) {
		Node a = this[ai], b = this[bi];
		if (a == null || b == null || Connected(ai, bi)) return false;

		//notifier.Notify("connect", a, b);
		edges.Add(new Edge(ai, bi));
		return true;
	}

	/// <summary>
	/// Remove edge between two directly connected nodes
	/// </summary>
	/// <param name="ai">First node index</param>
	/// <param name="disconnectFrom">Second node index</param>
	public virtual bool Disconnect(int ai, int bi) {
		Edge match = edges.FirstMatch((e) => e.Tuple.Equals((ai, bi)));
		if (match == null) return false;
		
		edges.Remove(match);
		return true;
	}

	/// <summary>
	/// Toggle direct connection between two nodes
	/// </summary>
	/// <param name="a">First node index</param>
	/// <param name="b">Second node index</param>
	public void ToggleConnection(int a, int b) {
		if (Connected(a, b)) Disconnect(a, b);
		else Connect(a, b);
	}
	public void ToggleConnection(Node a, Node b) {
		ToggleConnection(IndexOf(a), IndexOf(b));
	}

	public Node FindNodeByValue(string value) {
		foreach (Node node in nodes)
			if (node.value.Equals(value))
				return node;
		return null;
	}
	public Node FindNodeByValue(object value) => FindNodeByValue(value.ToString());
	public Node[] FindNodesByValue(string value) {
		List<Node> match = new List<Node>();
		foreach (Node node in nodes)
			if (node.value.Equals(value))
				match.Add(node);
		return match.ToArray();
	}

	/// <summary>
	/// Add a range of nodes to the graph
	/// </summary>
	/// <param name="input">Nodes to add</param>
	public virtual void Add(Node item) {
		nodes.Add(item);
	}

	public bool Contains(Node item) => nodes.Contains(item);

	public void CopyTo(Node[] array, int arrayIndex) => nodes.CopyTo(array, arrayIndex);

	public void Insert(int index, Node item) => nodes.Insert(index, item);

	/// <summary>
	/// Remove a node from the graph
	/// </summary>
	/// <param name="item">Node to remove</param>
	public virtual bool Remove(Node item) {
		int index = IndexOf(item);
		if (!this.Safe(index)) return false;

		ConnectionsI(index).Perform((otherIndex) => Disconnect(index, otherIndex));
		UpdateIndices(index);
		nodes.Remove(item);
		return true;
	}

	public void RemoveAt(int index) => Remove(this[index]);

	/// <summary>
	/// Remove all nodes with a given value
	/// </summary>
	/// <param name="val">Node value to match</param>
	public void RemoveByValue(string val) {
		FindNodesByValue(val).Perform(Remove);
	}

	/// <summary>
	/// Remove all nodes and edges from graph
	/// </summary>
	public void Clear() { nodes.Clear(); edges.Clear(); }

	public IEnumerator<Node> GetEnumerator() {
		return nodes.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator() {
		return nodes.GetEnumerator();
	}
}
