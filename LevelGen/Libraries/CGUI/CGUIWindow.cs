#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static UnityEditor.EditorGUILayout;

public abstract class CGUIWindow : EditorWindow {
	public Color GuiColor { get; private set; }
	public Color GuiBgColor { get; private set; }
	public Color GuiContentColor { get; private set; }

	private int maxParamLength = 0;
	private Dictionary<string, Vector2> scrollViews = new Dictionary<string, Vector2>();

	public abstract string Title { get; }

	/// <summary>
	/// Setup that must be performed before GUI can be drawn
	/// </summary>
	protected abstract void Setup();
	/// <summary>
	/// If this invariant doesn't hold, setup must be performed
	/// </summary>
	protected abstract bool SetupInvariant { get; }
	/// <summary>
	/// If this invariant doesn't hold, GUI will not be drawn (and setup will not be performed)
	/// </summary>
	protected abstract bool DrawGUIInvariant { get; }
	/// <summary>
	/// GUI that is drawn when all invariatns are upheld
	/// </summary>
	protected abstract void DrawGUI();
	/// <summary>
	/// GUI that is drawn when DrawGUIInvariant isn't upheld
	/// </summary>
	protected abstract void DrawErrorGUI();

	protected virtual void Awake() {
		GuiColor = GUI.color;
		GuiBgColor = GUI.backgroundColor;
		GuiContentColor = GUI.contentColor;
	}

	protected virtual void OnEnable() {
		string name = this.Path().Slice('/', -1, 0).Slice('_', -1, 0).Slice('.', -1);
		Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(this.Path().Slice('/', -1) + "/Icon_" + name + ".psd");
		if (icon != null)
			titleContent = new GUIContent(Title, icon);
	}
	protected virtual void OnGUI() {
		if (!DrawGUIInvariant) {
			DrawErrorGUI();
			return;
		}
		
		if (!SetupInvariant) {
			Setup();
			if (!SetupInvariant) throw new System.Exception("Setup method doesn't uphold setup invariant");
			// Debug.Log("Window " + Title + " has been setup correctly");
		}

		if (Application.isEditor) Repaint();

		
		DrawGUI();
	}

	public void ResetGuiColor() {
		GUI.color = GuiColor;
	}
	public void ResetGuiBgColor() {
		GUI.backgroundColor = GuiBgColor;
	}
	public void ResetGuiContentColor() {
		GUI.contentColor = GuiContentColor;
	}

	protected void ParameterField<T>(string key, T val, GUIStyle style) {
		maxParamLength = Mathf.Max(maxParamLength, key.Length);
		LabelField(key + ":   " + ToolBox.Utility.Aesthetic.Indent(maxParamLength - key.Length) + val, style);
	}

	protected void ParameterField<T>(string key, T val) {
		ParameterField(key, val, new GUIStyle(EditorStyles.label));
	}
	
	public void Scroll(string key, System.Action action, params GUILayoutOption[] options) {
		if (!scrollViews.ContainsKey(key)) scrollViews.Add(key, default);

		scrollViews[key] = BeginScrollView(scrollViews[key], options);
		action();
		EndScrollView();
	}
}
#endif