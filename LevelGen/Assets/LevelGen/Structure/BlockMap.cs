using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlockMap {
	public bool minimized;
	public List<Block> blockOverrides = new List<Block>();

	#region Treated as readonly
	[SerializeField]
	protected string id;
	public string Id => id;
	[SerializeField]
	private Texture2D texture;
	public Texture2D Texture => texture;
	[SerializeField]
	private Color exitColor;
	public Color ExitColor => exitColor;
	#endregion

	#region Constructor & Public Methods
	private BlockMap(BlockMap other) {
		id = other.id;
		minimized = other.minimized;
		texture = other.texture;
		exitColor = other.exitColor;
		//unshareableColors = new List<Color>(other.unshareableColors);
	}
	public BlockMap(string id, Texture2D texture, Color exitColor) {
		this.id = id;
		minimized = false;
		this.texture = texture ?? throw new System.ArgumentNullException("BlockMap texture must be assigned");
		this.exitColor = exitColor;
    }
	#endregion

	public BlockMap Clone() => new BlockMap(this);

	public Color[] Pixels => texture.GetPixels();
    public Color GetPixel(int x, int y) { return texture.GetPixel(x, y); }
	public Color GetPixel(Vector2Int p) { return WithinBounds(p) ? GetPixel(p.x, p.y) : Color.clear; }
	
	public bool HasColor(Color c) => blockOverrides.Any((b) => b.color == c);

	public int Width => texture.width;
    public int Height => texture.height;
	public Vector2 Size => new Vector2(Width, Height);

	public bool WithinBounds(Vector2Int p) {
		return p.InInterval(Vector2Int.zero, new Vector2Int(Width, Height) - Vector2Int.one);
	}

	#region Equality Management
	public override bool Equals(object obj) {
		var map = obj as BlockMap;
		return map != null &&
			   id == map.id;
	}

	public override int GetHashCode() => 1877310944 + EqualityComparer<string>.Default.GetHashCode(id);

	public static bool operator ==(BlockMap map1, BlockMap map2) {
		return EqualityComparer<BlockMap>.Default.Equals(map1, map2);
	}

	public static bool operator !=(BlockMap library1, BlockMap library2) {
		return !(library1 == library2);
	}
	#endregion
}
