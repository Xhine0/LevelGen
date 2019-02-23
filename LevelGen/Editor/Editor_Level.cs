#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUILayout;

[CustomEditor(typeof(Level))]
public class Editor_Level : AbstractEditor<Level> {
	private LevelGenerator2D gen = null;
	private LevelGenerator2D LG {
		get {
			if (gen == null) gen = GameObject.Find("_Level")?.GetComponent<LevelGenerator2D>();
			return gen;
		}
	}

	public override bool AutoSetDirty => false;

	public override void DrawGUI() {
		EditorGUI.BeginDisabledGroup(LG == null);
		if (GUILayout.Button("Set as active")) LG.level = t;
		EditorGUI.EndDisabledGroup();
		if (LG == null) LabelField("Scene not setup correctly for level generation.\nFix this by going to Level Generator > Show Window", EditorStyles.helpBox);
	}
}
#endif