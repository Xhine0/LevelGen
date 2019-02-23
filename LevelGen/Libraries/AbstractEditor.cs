#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public abstract class AbstractEditor<O> : CGUIEditor where O : Object {
	protected O t { get; private set; }

	protected virtual void Awake() {
		if (target == null) throw new System.NullReferenceException("GUI target cannot be null");
		t = (O)target;
	}

	public abstract bool AutoSetDirty { get; }
	public abstract void DrawGUI();

	public override void OnInspectorGUI() {
		DrawGUI();
		if (AutoSetDirty && GUI.changed) {
			EditorUtility.SetDirty(t);
		}
	}
}
#endif