using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Block {
	public enum Type { Simple, SlicedConvex, SlicedConcave };
	public enum Shape { Unitary, Horizontal, Vertical, Square };

	public Color color;
	public GameObject[] convexSlice = new GameObject[0];//9
	public GameObject[] concaveSlice = new GameObject[0];//4
	public GameObject obj;
	public bool inStasis = true;
	public Type type = Type.Simple;
	public Shape shape = Shape.Unitary;

	private Block(Block block) {
		color = block.color;
		obj = block.obj;
		inStasis = block.inStasis;
		type = block.type;
		shape = block.shape;
	}
	public Block(Color color, GameObject obj, Type type) {
		this.color = color;
		this.obj = obj;
		this.type = type;
	}
    public Block(Color color, GameObject obj) {
        this.color = color;
        this.obj = obj;
	}
	public Block() {
		color = Color.grey;
	}

    public Block Clone() {
        return new Block(this);
    }

	public override bool Equals(object obj) {
		var block = obj as Block;
		return block != null &&
			   color.Equals(block.color);
	}

	public override int GetHashCode() {
		return 790427672 + EqualityComparer<Color>.Default.GetHashCode(color);
	}

	public static bool operator ==(Block block1, Block block2) {
		return EqualityComparer<Block>.Default.Equals(block1, block2);
	}

	public static bool operator !=(Block block1, Block block2) {
		return !(block1 == block2);
	}
}
