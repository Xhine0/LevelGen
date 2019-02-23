#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using static UnityEditor.EditorGUILayout;

public abstract class CGUIEditor : Editor {
	private int maxParamLength = 0;

	public void FormattedLabelField<T>(string key, T val, GUIStyle style) {
		maxParamLength = Mathf.Max(maxParamLength, key.Length);
		LabelField(key + ":   " + ToolBox.Utility.Aesthetic.Indent(maxParamLength - key.Length) + val, style);
	}

	public void FormattedLabelField<T>(string key, T val) {
		FormattedLabelField(key, val, new GUIStyle(EditorStyles.label));
	}
}
#endif