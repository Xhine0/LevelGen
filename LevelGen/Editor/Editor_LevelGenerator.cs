#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelGenerator2D))]
public class Editor_LevelGenerator : AbstractEditor<LevelGenerator2D> {
	public override bool AutoSetDirty => false;

	public override void DrawGUI() {
		if (GUILayout.Button("Edit Level")) {
			LevelGen.Editor.Window_Inspector.ShowAll();
		}
	}
}
#endif
