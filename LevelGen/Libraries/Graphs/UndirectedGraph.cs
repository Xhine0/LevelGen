using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Framework for a non-directional graph with node values
/// Nodes are physical (has a position and size in local space)
/// </summary>
[Serializable]
public class UndirectedGraph : Graph {
	public UndirectedGraph() : base() { }
	public UndirectedGraph(params Node[] nodes) : base(nodes) { }

	public sealed override Node[] Connections(int i) {
		List<Node> result = new List<Node>();
		foreach (var (ai, bi) in Edges) {
			if (ai == i) result.Add(this[bi]);
			else if (bi == i) result.Add(this[ai]);
		}
		return result.ToArray();
	}

	public sealed override bool Connected(int ai, int bi) => Edges.ContainsAny((ai, bi), (bi, ai));

	public sealed override bool Disconnect(int ai, int bi) {
		bool result = base.Disconnect(ai, bi);
		return !result ? base.Disconnect(bi, ai) : result;
	}
}
