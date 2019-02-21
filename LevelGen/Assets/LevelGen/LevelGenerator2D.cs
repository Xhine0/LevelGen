using static Block;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Vector2Int;

/// <summary>
/// Manager for generating 
/// </summary>
[ExecuteInEditMode]
[System.Serializable]
public class LevelGenerator2D : MonoBehaviour {
	public const int COLOR_TOLERANCE = 1;

	public Level level;
	
	public List<MapGroup> Groups { get => level?.groups; set { if (level != null) level.groups = value; } }

	private GameObject exitTemplate = null;
	private GameObject ExitTemplate {
		get {
			if (exitTemplate == null) {
				string path = this.DirectoryPath() + "/Prefabs/RoomTrigger.prefab";
				Debug.Log("Loading exit template from path " + path);
				exitTemplate = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
			}
			return exitTemplate;
		}
	}

	#region Window Data
	// Temporary storage (used when generating new data)
	public string tempId = "default";
	public Texture2D tempTexture = null;
	public Color tempExitColor = Color.white;

	public Transform tempHandler = null;
	public string tempGroup = "default";

	public bool autoFreshId = true, createMap = false;
	#endregion

	#region Map Info
	public string GetBlockName(Transform obj) {
		string str = obj.transform.name;
		return str.Substring(str.IndexOf("_") + 1);
	}

	/// <param name="id">Map id to check</param>
	/// <returns>All blocks with given map id</returns>
	public Block[] GetBlocksById(string id) => GetGroupWithId(id).Blocks;

	/// <param name="id">Map id to check</param>
	/// <param name="color">Block color to check</param>
	/// <returns>Block with given color in map with id, if any</returns>
	public Block GetBlockByColor(string id, Color color) => GetBlocksById(id).FirstMatch((b) => b.color == color);

	/// <param name="color">Color to check</param>
	/// <param name="ids">Ids to check</param>
	/// <returns>Whether or not the id contains a block entry that matches the given color</returns>
	public bool ColorDefinedWithId(Color color, params string[] ids) {
		return ids.Perform(GetGroupWithId).Any((g) => g != null && g.ContainsColor(color));
	}

	public MapGroup GetGroupWithId(string id) => Groups.FirstMatch((g) => g.Maps.FirstMatch((m) => m.Id == id) != null);
	/// <summary>
	/// Get the map with a given id
	/// </summary>
	/// <param name="id">Id to check</param>
	/// <returns>Block map</returns>
	public BlockMap GetMapById(string id) {
		foreach (MapGroup group in Groups)
			foreach (BlockMap map in group.maps)
				if (map.Id == id) return map;
		return null;
	}

	/// <summary>
	/// Checks if a given id is not taken by any block container
	/// </summary>
	/// <param name="id">Id to check</param>
	/// <returns>If id is available</returns>
	public bool IdAvailable(string id) => GetMapById(id) == null;
	#endregion

	#region Native Methods
	private void Awake() {
		
	}
	#endregion

	#region Room Management
	public bool AddRoom(BlockMap map) {
		if (map?.Id == null) throw new System.ArgumentNullException("BlockMap cannot be null");
		if (level.roomGraph.NodeValues.Contains(map.Id)) return false;

		level.groups.Add(new MapGroup(map));
		level.roomGraph.Add(new Graph.Node(map.Id, Vector2.one * level.roomGraph.Count * 20, map.Size));

		for (int y = 0; y < map.Height; y++) {
			for (int x = 0; x < map.Width; x++) {
				if (map.GetPixel(x, y) == map.ExitColor) {
					var (start, end, isBeginning) = GetColorShape(map, new Vector2Int(x, y), true, true);
					if (!isBeginning) continue;

					level.exitGraph.Add(new Graph.Node(map.Id, (new Vector2(x, map.Height - y - 1) - end + start) / new Vector2(map.Width, map.Height), end - start + one));
				}
			}
		}
		return true;
	}
	public void RemoveRoom(string id) {
		level.roomGraph.RemoveByValue(id);
		level.exitGraph.RemoveByValue(id);
		UngroupRoom(id);
	}
	public void RemoveRoom(Graph.Node node) {
		RemoveRoom(node.value);
	}

	public void GroupRooms(string moveId, string targetId) {
		BlockMap toMove = GetMapById(moveId).Clone();
		GetGroupWithId(moveId).Remove(toMove);
		GetGroupWithId(targetId).Add(toMove);
		RemoveEmptyGroups();
	}
	public void UngroupRoom(string id) {
		MapGroup group = GetGroupWithId(id);
		group.Remove(id);
		RemoveEmptyGroups();
	}
	public void SeparateRoomFromGroup(string id) {
		MapGroup group = GetGroupWithId(id).Clone();
		if (group.maps.Count <= 1) return;
		BlockMap toMove = GetMapById(id).Clone();
		UngroupRoom(id);
		level.groups.Add(new MapGroup(toMove));
		// Move current block definitions to new group
		GetGroupWithId(id).Sync(group);
	}
	
	private void RemoveEmptyGroups() {
		level.groups = level.groups.Filter((g) => g.maps.Count > 0);
	}
	#endregion

	#region Generation
	/// <summary>
	/// Generates level according to specified parameters
	/// </summary>
	public void GenerateLevel() {
		if (level == null) { Debug.LogError("No level has been specified"); return; }

		ClearLevel();

		GenerateRoom(level.roomGraph.Root);
	}
	/// <summary>
	/// Clears the level and updates generated-state
	/// </summary>
	public void DeleteLevel() {
		if (level == null) { Debug.LogError("No level has been specified"); return; }

		ClearLevel();
	}

	/// <summary>
	/// Destroys all spawned scene objects
	/// </summary>
	private void ClearLevel() {
		for (int i = transform.childCount - 1; i >= 0; i--) {
			GameObject obj = transform.GetChild(i).gameObject;
			if (!Application.isPlaying) DestroyImmediate(obj);
			else if(obj.tag != "Player") Destroy(obj);
		}
	}

	/// <summary>
	/// Explicitly saves changes to a level
	/// </summary>
	public void SaveLevel() {
		if (level == null) {
			Debug.Log("Couldn't save (no level assigned)");
			return;
		}
		UnityEditor.EditorUtility.SetDirty(level);
		UnityEditor.AssetDatabase.SaveAssets();
		Debug.Log("Level " + level.name + " saved");
	}

	/// <summary>
	/// Spawns a map's content into the scene
	/// </summary>
	/// <param name="room">Graph node corresponding to the map</param>
	/// <returns>Tuple with level parent and spawned exits</returns>
	public (Transform Parent, List<Transform> Exits) GenerateRoom(Graph.Node room) {
		MapGroup group = GetGroupWithId(room.value);

		#region Block Spawning
		// Prepare level parent
		Transform levelParent = new GameObject().transform;
		levelParent.name = room.value;
		levelParent.transform.parent = transform;

		SpawnRoom(group, room.value, levelParent);
		#endregion

		#region Exit Spawning
		// Prepare exit parent
		Transform exitParent = new GameObject().transform;
		exitParent.name = "_Exits";
		exitParent.transform.parent = transform;
		
		return (levelParent, SpawnExits(room, group[room.value], exitParent));
		#endregion
	}

	internal void SpawnRoom(MapGroup group, string id, Transform parent) {
		BlockMap map = group[id];
		
		// The current map's instance of the group's block collection
		Dictionary<Color, Block> blocksInstance = map.blockOverrides.Append(group.Blocks).ValuesToDictionary((b) => b.color);
		
		for (int y = 0; y < map.Height; y++) {
			for (int x = 0; x < map.Width; x++) {
				Block b = blocksInstance.SafeGet(map.GetPixel(x, y));
				if (b == null || b.inStasis) continue;
				
				void Spawn(bool goRight, bool goUp) {
					Vector2Int pos = new Vector2Int(x, y);
					var (Start, End, IsBeginning) = GetBlockShape(group, id, pos, goRight, goUp);
					if (IsBeginning) SpawnBlock(group.GetObj(id, pos), (Start, End), parent);
				}
				if (b.shape == Shape.Square) Spawn(true, true);
				else Spawn(b.shape == Shape.Horizontal, b.shape == Shape.Vertical);
			}
		}
	}
	internal List<Transform> SpawnExits(Graph.Node room, BlockMap map, Transform parent) {
		List<Transform> spawnedExits = new List<Transform>();
		int ei = 0;
		foreach (Graph.Node exit in level.exitGraph.FindNodesByValue(room.value)) {
			GameObject obj = Instantiate(ExitTemplate, 
				parent.position.ToXY() + map.Size * new Vector2(exit.pos.x, 1 - exit.pos.y) - (exit.size + new Vector2(-1, 1)) / 2, 
				new Quaternion(0, 0, 0, 0)
			) as GameObject;
			obj.transform.localScale = exit.size;
			obj.transform.parent = parent;
			obj.transform.name = "Exit" + ei++;
			obj.GetComponent<RoomTrigger>().exit = exit;
			spawnedExits.Add(obj.transform);
		}
		return spawnedExits;
	}

	private GameObject SpawnBlock(GameObject obj, Vector2 spawnPoint, Transform parent) {
		// Don't respawn objects that carry over between levels
		if (obj == null || (LevelGen.Data.PersistentTags.Contains(obj.tag) && obj.InScene())) return null;

		GameObject sceneObj = Instantiate(obj, spawnPoint, new Quaternion(0, 0, 0, 0)) as GameObject;
		sceneObj.transform.parent = parent;

		return sceneObj;
	}
	private GameObject SpawnBlock(GameObject obj, (Vector2Int Start, Vector2Int End) shape, Vector2 posOffset, Transform parent) {
		Vector2 start = shape.Start, end = shape.End;
		if (shape.Start == shape.End) return SpawnBlock(obj, start, parent); // Treat 1x1 shapes as regular blocks

		GameObject sceneObj = SpawnBlock(obj, (end - start) / 2 + start + posOffset, parent);
		if (sceneObj != null) {
			Vector3 scale = sceneObj.transform.localScale.Mult((end - start).Abs() + Vector2.one);
			sceneObj.transform.localScale = new Vector3(scale.x, scale.y, 1);
		}
		return sceneObj;
	}
	private GameObject SpawnBlock(GameObject obj, (Vector2Int, Vector2Int) shape, Transform parent) {
		return SpawnBlock(obj, shape, Vector2.zero, parent);
	}

	/// <summary>
	/// Move an object from a given exit, to one of it's connected exits (chosen randomly).
	/// Currently loaded room will be replaced with the parent room of the transition destination
	/// </summary>
	/// <param name="fromExit">Exit to transition from</param>
	/// <param name="target">Object to move</param>
	/// <returns>Transition destination</returns>
	public Vector3 Transition(Graph.Node fromExit, GameObject target) {
		// Remove target from current level, so that it can be moved to the next
		if (target == null) throw new System.ArgumentNullException("Transition target cannot be null");
		target.transform.parent = null;

		Graph.Node[] connections = level.exitGraph.Connections(fromExit);
		if (connections.Length == 0) {
			Debug.LogError("Exit has no connections. Transition failed");
			return target.transform.position;
		}
		Graph.Node toExit = connections[Random.Range(0, connections.Length - 1)];

		// Remove existing level content
		ClearLevel();
		// Spawn connected room
		var (LevelParent, Exits) = GenerateRoom(level.roomGraph.FindNodeByValue(toExit.value));
		// Move target to new level
		target.transform.parent = LevelParent;

		Debug.Log("Target exit: " + toExit);
		return Exits[level.exitGraph.FindNodesByValue(toExit.value).IndexOf(toExit)].transform.position;
	}

	/// <summary>
	/// Returns path from the given position (if said position is the path's starting point).
	/// This instance compares blocks
	/// </summary>
	/// <param name="map">Map containing shape</param>
	/// <param name="start">Start position of shape</param>
	/// <param name="goRight">Check for connected blocks to the right</param>
	/// <param name="goUp">Check for connected blocks upwards</param>
	/// <returns>Path from given position</returns>
	internal (Vector2Int, Vector2Int, bool) GetShape(BlockMap map, Vector2Int start, bool goRight, bool goUp, System.Func<Vector2Int, Vector2Int, bool> Connected) {
		Vector2Int end = start;
		bool isBeginning = !(Connected(end, left) || Connected(end, down));

		Vector2Int Next(ref bool go, Vector2Int dir) {
			if (go && Connected(end, dir)) return dir;
			else { go = false; return zero; }
		}
		while (goRight || goUp)
			end += Next(ref goRight, right) + Next(ref goUp, up);

		return (start, end, isBeginning);
	}

	/// <summary>
	/// Returns shape connected by block type
	/// </summary>
	/// <returns>Shape from the given start position, if any</returns>
	public (Vector2Int, Vector2Int, bool) GetBlockShape(MapGroup group, string id, Vector2Int start, bool goRight, bool goUp) {
		bool isBeginning = !((goRight && group.IsConnected(id, start, start + left)) ||
			(goUp && group.IsConnected(id, start, start + down)));

		Vector2Int end = start;
		while (goRight || goUp) {
			if (goRight) {
				if (group.IsConnected(id, end, end + right)) end.x++;
				else goRight = false;
			}
			if (goUp) {
				if (group.IsConnected(id, end, end + up)) end.y++;
				else goUp = false;
			}
		}
		return (start, end, isBeginning);
	}
	/// <summary>
	/// Returns shape connected by pixel values
	/// </summary>
	/// <returns>Shape from the given start position, if any</returns>
	public (Vector2Int, Vector2Int, bool) GetColorShape(BlockMap map, Vector2Int start, bool goRight, bool goUp) {
		return GetShape(map, start, goRight, goUp, (end, dir) => map.GetPixel(end + dir) == map.GetPixel(end));
	}
	#endregion
}
