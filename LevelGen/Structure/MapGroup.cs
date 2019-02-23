using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ToolBox.Data.Resources;

[System.Serializable]
public class MapGroup : ICollection<BlockMap> {
	#region Treated as readonly
	[SerializeField]
	private List<Block> blocks = new List<Block>();
	public Block[] Blocks => blocks.Perform((b) => b.Clone());
	#endregion

	public List<BlockMap> maps = new List<BlockMap>();
	public BlockMap[] Maps => maps.Perform((m) => m.Clone());

	public Color[] Colors => Blocks.Perform((b) => b.color);

	public int Count => maps.Count;
	public bool IsReadOnly => false;

	// Constructor ensures that a non-empty group is made
	public MapGroup(BlockMap map) {
		Add(map);
		if (maps.Count == 0) throw new System.ArgumentException("The given block map is invalid");
		SetupBlocks();
	}
	private MapGroup(MapGroup other) {
		blocks = new List<Block>(other.Blocks);
		maps = new List<BlockMap>(other.Maps);
	}
	public MapGroup Clone() => new MapGroup(this);

	public BlockMap this[string id] {
		get => maps.FirstMatch((m) => m.Id == id);
		set => maps.SafeSet(value);
	}

	public Block GetBlock(string id, Color? color) {
		if (!color.HasValue) return null;

		bool Predicate(Block b) => b.color == color;
		Block block = this[id]?.blockOverrides.FirstMatch(Predicate);
		if (block == null) block = blocks.FirstMatch(Predicate);
		return block?.Clone();
	}
	public Block GetBlock(string id, Vector2Int pos) => GetBlock(id, this[id]?.GetPixel(pos));

	private void SetupBlocks() {
		HashSet<Color> colors = Colors.ToSet();
		HashSet<Color> pixels = maps.Perform((m) => m.Pixels).Concat().ToSet();
		pixels.Perform((p) => {
			if (p.a > 0 && !colors.Contains(p)) blocks.Add(new Block(p, null));
		});
		blocks = blocks.Filter((b) => pixels.Contains(b.color));
	}

	public void Sync(MapGroup other) {
		other.blocks.Perform(Set);
	}
	
	public bool IdAvailable(string id) => !maps.Any((m) => m.Id == id);
	public bool ContainsColor(Color color) => Colors.Contains(color);


	#region Block Management
	public GameObject GetObj(string id, Vector2Int pos) {
		Block block = GetBlock(id, pos);
		if (block == null) return null;

		int objVal = GetObjValue(id, pos);
		if (objVal == -1) return block.obj;
		else if (objVal < 9) return block.convexSlice[objVal];
		else return block.concaveSlice[objVal - 9];
	}

	public bool IsConnected(string id, Vector2Int a, Vector2Int b) {
		BlockMap map = this[id];
		return map.GetPixel(a) == map.GetPixel(b) && GetObjValue(id, a) == GetObjValue(id, b);
	}

	public int GetObjValue(string id, Vector2Int pos) {
		BlockMap map = this[id];
		if (map == null) return -1;
		Block block = GetBlock(id, map?.GetPixel(pos));
		if (block == null || block.type == Block.Type.Simple) return -1;
		
		Vector2Int move = Vector2Int.zero;
		HashSet<string> adj = new HashSet<string>();
		foreach (string dir in Compass9.Keys) {
			Vector2Int v = Compass9[dir];
			Color? c = map.GetPixel(pos + v);
			if (!c.HasValue || v == Vector2Int.zero) continue;

			if (c == block.color) {
				adj.Add(dir);
				if (dir.Length == 1) move += v;
			}
		}
		move.x = -move.x;

		if (adj.ContainsAll("n", "s") || adj.ContainsAll("w", "e")) return 4;

		if (block.type == Block.Type.Concave) {
			if (move.x != 0 && move.y != 0 && map.GetPixel(pos + move) == block.color && map.GetPixel(pos - move) == block.color) {
				if (move.x != -1) move.x--;
				if (move.y != -1) move.y--;

				return (move.y + 1) * 2 + move.x + 10;
			}
		}

		return (move.y + 1) * 3 + move.x + 1;
	}
	#endregion

	#region Collection Management
	public void Set(Block block) => blocks.SafeSet(block.Clone());
	public void Set(ICollection<Block> blocks) => blocks.Perform(Set);

	public void Add(BlockMap item) {
		if (item != null && IdAvailable(item.Id)) {
			maps.Add(item.Clone());
			SetupBlocks();
		}
	}

	public void Clear() {
		blocks.Clear();
		maps.Clear();
	}

	public bool Contains(BlockMap item) => maps.Contains(item);

	public void CopyTo(BlockMap[] array, int arrayIndex) => maps.CopyTo(array, arrayIndex);

	public bool Remove(BlockMap item) {
		if (item == null || !maps.Remove(item)) return false;
		SetupBlocks();
		return true;
	}
	public bool Remove(string id) => Remove(this[id]);

	public IEnumerator<BlockMap> GetEnumerator() => maps.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => maps.GetEnumerator();
	#endregion
}
