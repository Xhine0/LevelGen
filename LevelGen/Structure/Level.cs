using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "Level", order = 1)]
[System.Serializable]
public class Level : ScriptableObject {
	public UndirectedEventGraph roomGraph = new UndirectedEventGraph();
	public UndirectedGraph exitGraph = new UndirectedGraph();
	public List<MapGroup> groups = new List<MapGroup>();
}
