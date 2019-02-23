#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static UnityEditorInternal.InternalEditorUtility;
using static UnityEditor.EditorGUILayout;

namespace CGUI {
	public static class Constants {
		public const float SCROLL_BAR_WIDTH = 44, SCROLLVIEW_BAR_WIDTH = 24;

		public enum LineStyle { Solid, Dashed, Bezier };

		public enum W { Null, Margin, Padding, Toggle, Enum, Object, SmallButton, Header };
		public static Dictionary<W, float> rawWidth = new Dictionary<W, float> {
			{ W.Null, 0 }, { W.Margin, 4 }, { W.Padding, 4 },
			{ W.Header, 140 },
			{ W.Toggle, 14 }, { W.Enum, 74 }, { W.Object, 100 },
			{ W.SmallButton, 20 }
		};

		public static Color WarningColor { get { return Color.red.Blend(GUI.backgroundColor, 0.675f); } }
	}

	public static class Text {
		public static void Symbol(string symbol, int width) {
			LabelField(ToolBox.Data.Resources.Symbols[symbol].ToString(), GUILayout.Width(width));
		}
		public static void DrawSymbol(string symbol) {
			Symbol(symbol, 20);
		}

		public static void BoldLabel(string text, float width) {
			if (width > 0) {
				LabelField(text, EditorStyles.boldLabel, GUILayout.Width(width));
			}
			else {
				LabelField(text, EditorStyles.boldLabel);
			}
		}
		public static void BoldLabel(string text) {
			BoldLabel(text, 0);
		}

		public static void Header(string text, float width) {
			Space();
			BoldLabel(text, width);
		}
		public static void Header(string text) {
			Header(text, 0);
		}
		public static void SectionHeader(string text) {
			Space();
			Layout.Line();
			Header(text);
		}
	}

	public static class Layout {
		public static void HorizontalSpace(float width) {
			Space(width, 0);
		}
		public static void HorizontalSpace(params Constants.W[] w) {
			HorizontalSpace(RawWidth(w));
		}
		public static void VerticalSpace(float height) {
			Space(0, height);
		}
		public static void VerticalSpace(params Constants.W[] w) {
			VerticalSpace(RawWidth(w));
		}
		public static void Space(float width, float height) {
			width -= 2; height -= 2;
			if (width >= 0 || height >= 0)
				LabelField("", GUILayout.Width(width), GUILayout.Height(height));
		}

		public static void HorizontalMargin(float leftMargin, float rightMargin, System.Action action) {
			Horizontal(() => {
				HorizontalSpace(leftMargin);
				Vertical(action);
				HorizontalSpace(rightMargin);
			});
		}
		public static void HorizontalMargin(float margin, System.Action action) {
			HorizontalMargin(margin, margin, action);
		}

		public static void VerticalMargin(float topMargin, float bottomMargin, System.Action action) {
			Vertical(() => {
				VerticalSpace(topMargin);
				action();
				VerticalSpace(bottomMargin);
			});
		}
		public static void VerticalMargin(float margin, System.Action action) {
			VerticalMargin(margin, margin, action);
		}

		public static void Margin(float topMargin, float bottomMargin, float leftMargin, float rightMargin, System.Action action) {
			VerticalMargin(topMargin, bottomMargin, () => { HorizontalMargin(leftMargin, rightMargin, action); });
		}
		public static void Margin(float verticalMargin, float horizontalMargin, System.Action action) {
			Margin(verticalMargin, verticalMargin, horizontalMargin, horizontalMargin, action);
		}
		public static void Margin(float margin, System.Action action) {
			Margin(margin, margin, margin, margin, action);
		}

		public static T Field<T>(string text, float width, System.Func<T> field) {
			T result = default;
			Horizontal(() => {
				LabelField(text, Width(width));
				result = field();
			});
			return result;
		}

		#region Custom Scopes
		public static void Horizontal(System.Action action, params GUILayoutOption[] options) {
			BeginHorizontal(options);
			action();
			EndHorizontal();
		}
		public static EditorAction Horizontal(System.Func<EditorAction> func, params GUILayoutOption[] options) {
			BeginHorizontal(options);
			EditorAction editorAction = func();
			EndHorizontal();
			return editorAction;
		}
		public static void Vertical(System.Action action, params GUILayoutOption[] options) {
			BeginVertical(options);
			action();
			EndVertical();
		}
		public static EditorAction Vertical(System.Func<EditorAction> func, params GUILayoutOption[] options) {
			BeginVertical(options);
			EditorAction editorAction = func();
			EndVertical();
			return editorAction;
		}
		
		public static void PaddedSection(System.Action content, float padding) {
			Horizontal(() => {
				HorizontalSpace(padding);
				Vertical(() => {
					VerticalSpace(padding);
					content();
					VerticalSpace(padding);
				});
				HorizontalSpace(padding);
			});
		}

		public static void Area(Rect rect, System.Action action, params GUILayoutOption[] options) {
			GUILayout.BeginArea(rect);
			action();
			GUILayout.EndArea();
		}

		public static void Disable(bool condition, System.Action action) {
			EditorGUI.BeginDisabledGroup(condition);
			action();
			EditorGUI.EndDisabledGroup();
		}
		public static EditorAction Disable(bool condition, System.Func<EditorAction> func) {
			EditorGUI.BeginDisabledGroup(condition);
			EditorAction editorAction = func();
			EditorGUI.EndDisabledGroup();
			return editorAction;
		}
		#endregion

		public static void ConstructLine(Color color, char c) {
			GUIStyle style = new GUIStyle(EditorStyles.centeredGreyMiniLabel) {
				fontSize = 3
			};
			style.normal.textColor = color;

			LabelField("".Generate(1000, () => { return c; }), style, GUILayout.ExpandWidth(true), GUILayout.Height(6));
		}

		public static void TopLine(Color color) { ConstructLine(color, '¯'); }
		public static void TopLine() {
			TopLine(Color.black);
		}
		public static void Line(Color color) { ConstructLine(color, '-'); }
		public static void Line() {
			Line(Color.black);
		}
		public static void BottomLine(Color color) { ConstructLine(color, '_'); }
		public static void BottomLine() {
			BottomLine(Color.black);
		}

		#region GUILayoutOption Management
		public static float RawWidth(params Constants.W[] keys) {
			float width = 0;
			keys.Perform((key) => { width += Constants.rawWidth[key]; });
			return width;
		}
		
		public static GUILayoutOption Width(params Constants.W[] keys) {
			return GUILayout.Width(RawWidth(keys));
		}
		public static GUILayoutOption Width(float value) {
			return GUILayout.Width(value);
		}

		public static GUILayoutOption Height(params Constants.W[] keys) {
			return GUILayout.Height(RawWidth(keys));
		}
		public static GUILayoutOption Height(float value) {
			return GUILayout.Height(value);
		}

		public static GUILayoutOption MinWidth(params Constants.W[] keys) {
			return GUILayout.MinWidth(RawWidth(keys));
		}
		public static GUILayoutOption MinWidth(float value) {
			return GUILayout.MinWidth(value);
		}

		public static GUILayoutOption MaxWidth(params Constants.W[] keys) {
			return GUILayout.MaxWidth(RawWidth(keys));
		}
		public static GUILayoutOption MaxWidth(float value) {
			return GUILayout.MaxWidth(value);
		}
		#endregion
	}

	public static class Drawers {
		public static void DrawPoint(Vector2 p, Color color, float size, float lineWidth, float angle, int lineCount) {
			Handles.color = color;
			for (int i = 0; i < lineCount; i++) {
				Vector2 d = ToolBox.Calculations.Math.AngleToVector(angle + i * (180 / lineCount)) * size / 2;
				Handles.DrawAAPolyLine(lineWidth, p - d, p + d);
			}
		}
		public static void DrawPoint(Vector2 p, Color color, float size, float lineWidth) {
			DrawPoint(p, color, size, lineWidth, 0, 2);
		}

		public static void DrawDashedLine(Vector2 p0, Vector2 p1, Color color, float width, float length) {
			Handles.color = color;
			Vector2 delta = p1 - p0;
			length = Mathf.Min(length / delta.magnitude, 1);
			for (float i = 0; i < 1; i += length * 2)
				Handles.DrawAAPolyLine(width, p0 + delta * i, p0 + delta * Mathf.Min(i + length, 1));
		}

		public static void ColoredBox(Color color, params GUILayoutOption[] options) {
			GUILayout.Box("", options);
			EditorGUI.DrawRect(GUILayoutUtility.GetLastRect(), color);
		}

		public static void ColoredLabel(string text, Color color, params GUILayoutOption[] options) {
			GUIStyle style = new GUIStyle(GUI.skin.label);
			style.normal.textColor = color;
			LabelField(text, style, options);
		}
		public static void ColoredLabel(GUIContent content, Color color, params GUILayoutOption[] options) {
			GUIStyle style = new GUIStyle(GUI.skin.label);
			style.normal.textColor = color;
			LabelField(content, options);
		}

		public static void DrawRow<T>(T[] array, int fromIndex, int length, float paddingLeft, float paddingRight) where T : Object {
			if (paddingLeft > 0) LabelField("", Layout.Width(paddingLeft));
			for (int i = fromIndex; i < Mathf.Min(fromIndex + length, array.Length); i++)
				array[i] = Input.Fields.ObjField(array[i]);
			if (paddingRight > 0) LabelField("", Layout.Width(paddingRight));
		}
		public static void DrawRow<T>(T[] array, int fromIndex, int length) where T : Object {
			DrawRow(array, fromIndex, length, 0, 0);
		}

		public static void DrawTable<T>(T[] array, int width, float paddingLeft, float paddingRight, float paddingTop, float paddingBottom) where T : Object {
			BeginVertical();
			Layout.VerticalSpace(paddingTop);
			for (int i = 0; i < array.Length; i += width) {
				BeginHorizontal();
				DrawRow(array, i, width, paddingLeft, paddingRight);
				EndHorizontal();
			}
			Layout.VerticalSpace(paddingBottom);
			EndVertical();
		}
		public static void DrawTable<T>(T[] array, int width) where T : Object {
			DrawTable(array, width, 0, 0, 0, 0);
		}

		public static void DrawGrid(Vector2 pos, float width, float height, float sizeFactor, Color gridColor) {
			for (float x = 0; x <= width; x += sizeFactor) {
				Rect rect = new Rect(pos + Vector2.right * x, new Vector2(1, height));
				EditorGUI.DrawRect(rect, gridColor);
			}
			for (float x = 0; x <= height; x += sizeFactor) {
				Rect rect = new Rect(pos + Vector2.up * x, new Vector2(width, 1));
				EditorGUI.DrawRect(rect, gridColor);
			}
		}

		public static void DrawImageSpace(float width, float height) {
			GUILayout.Box("", GUILayout.Width(width), GUILayout.Height(height));
		}
		public static void DrawImage(Rect rect, Texture2D texture) {
			if (texture != null) GUI.DrawTexture(rect, texture);
		}
		public static void DrawImage(Texture2D texture, int width, int height) {
			DrawImageSpace(width, height);
			DrawImage(GUILayoutUtility.GetLastRect(), texture);
		}
		public static void DrawImage(Texture2D texture, int size) {
			DrawImage(texture, size, size);
		}

		public static void DrawGallery(Texture2D[] textures, int width, int pixelsPerCell) {
			if (width <= 0) return;

			if (textures.Length > width) {
				Layout.Vertical(() => {
					for (int i = 0; i < textures.Length; i += width) {
						Layout.Horizontal(() => DrawGallery(textures.SubArray(i, width), width, pixelsPerCell));
					}
				});
			}
			else
				foreach (Texture2D texture in textures)
					DrawImage(texture, pixelsPerCell);
		}
	}
}
namespace CGUI.Styles {
	public static class BoxStyles {
		public static GUIStyle Flat(Color color) {
			GUIStyle style = new GUIStyle();
			style.normal.background = ToolBox.Utility.Conversions.ColorToTexture(1, color);
			return style;
		}
		public static GUIStyle Colored(Color color) {
			GUIStyle style = new GUIStyle(GUI.skin.box);
			style.normal.background = ToolBox.Utility.Conversions.ColorToTexture(1, color);
			return style;
		}
	}
	public static class LabelStyles {
		public static GUIStyle centeredBoldMiniLabel {
			get {
				GUIStyle style = new GUIStyle(EditorStyles.centeredGreyMiniLabel) { fontStyle = FontStyle.Bold };
				style.normal.textColor = Color.black;
				return style;
			}
		}
		public static GUIStyle Colored(Color color, FontStyle fontStyle, TextAnchor alignment) {
			GUIStyle style = new GUIStyle() { fontStyle = fontStyle, alignment = alignment };
			style.normal.textColor = color;
			return style;
		}
		public static GUIStyle Colored(Color color, FontStyle fontStyle) => Colored(color, fontStyle, default);
		public static GUIStyle ColoredBg(Color textColor, Color bgColor, FontStyle fontStyle) {
			GUIStyle style = new GUIStyle(BoxStyles.Flat(bgColor)) { fontStyle = fontStyle };
			style.normal.textColor = textColor;
			return style;
		}
	}
	public static class ButtonStyles {
		public static GUIStyle Standard {
			get {
				GUIStyle style = new GUIStyle(GUI.skin.button) {
					fontSize = 14
				};
				style.normal.textColor = Color.white;
				style.active.textColor = Color.white;
				style.fixedHeight = 26;
				return style;
			}
		}
		public static GUIStyle Dangerous {
			get {
				GUIStyle style = Standard;
				style.stretchWidth = true;
				style.normal.textColor = Color.red;
				style.active.textColor = Color.red;
				return style;
			}
		}
	}
	
	public static class Tools {
		public static GUIStyle Bold(GUIStyle style) {
			style.fontStyle = FontStyle.Bold;
			return style;
		}
	}
}
namespace CGUI.Input {
	public static class Compound {
		public static List<T> ObjectListPanel<T>(List<T> list, string elemText) where T : Object {
			for (int i = 0; i < list.Count; i++) {
				if (Layout.Horizontal(() => {
					list[i] = (T)ObjectField(list[i], typeof(T), true);
					return Buttons.ButtonPanel(list, i);
				}) != EditorAction.Null) break;
			}
			list = Buttons.AddButton(elemText, list, default(T));
			return list;
		}
		public static bool ListPanel<T>(IList<T> list, bool open, string title, System.Func<T, T> func, bool move, bool duplicate, bool remove) {
			open = Collapsible(open, title, -1, () => {
				if (open) list = Buttons.AddButton("", list, default(T), Layout.Width(Constants.W.SmallButton));
			}, () => {
				for (int i = 0; i < list.Count; i++) {
					if (Layout.Horizontal(() => {
						list[i] = func(list[i]);
						return Buttons.ButtonPanel(list, i, move, duplicate, remove);
					}) != EditorAction.Null) break;
				}
				Space();
			});

			return open;
		}
		public static bool ListPanel<T>(List<T> list, bool open, string title, System.Func<T, T> func) => 
			ListPanel(list, open, title, func, true, false, true);

		public static bool Collapsible(bool open, string title, float width, System.Action menu, System.Action content, GUIStyle style) {
			Layout.Horizontal(() => {
				GUILayoutOption[] options = new GUILayoutOption[0];
				if (width >= 0) options = options.Append(Layout.Width(width));
				Layout.Horizontal(() => { open = Foldout(open, title, true, style); }, options);
				menu?.Invoke();
			});

			if (open) content();

			return open;
		}
		public static bool Collapsible(bool open, string title, float width, System.Action menu, System.Action content) =>
			Collapsible(open, title, width, menu, content, EditorStyles.foldout);
		public static bool Collapsible(bool open, string title, float width, System.Action content, GUIStyle style) => 
			Collapsible(open, title, width, null, content, style);
		public static bool Collapsible(bool open, string title, float width, System.Action content) => 
			Collapsible(open, title, width, null, content, EditorStyles.foldout);
		public static bool Collapsible(bool open, string title, System.Action content) =>
			Collapsible(open, title, -1, content);
	}

	public static class Buttons {
		public static EditorAction ButtonPanel<T>(IList<T> list, int index, string up, string down, string duplicate, string remove, float buttonWidth, GUIStyle style) {
			if (index < 0 || index >= list.Count) return EditorAction.Null;
			EditorAction action = EditorAction.Null;

			int buttonCount = new string[] { up, down, duplicate, remove }.Filter((s) => s != null).Count;
			buttonWidth -= buttonCount * 2 + 2;
			buttonWidth = Mathf.Max(buttonCount * Layout.RawWidth(Constants.W.SmallButton), buttonWidth) / buttonCount;

			if (up != null && GUILayout.Button(up, Layout.Width(buttonWidth))) {
				list = list.Move(index, -1);
				action = EditorAction.MoveUp;
			}
			if (down != null && GUILayout.Button(down, Layout.Width(buttonWidth))) {
				list = list.Move(index, 1);
				action = EditorAction.MoveDown;
			}
			if (duplicate != null && GUILayout.Button(duplicate, Layout.Width(buttonWidth))) {
				list.Add(list[index]);
				action = EditorAction.Duplicate;
			}
			if (remove != null && GUILayout.Button(remove, Layout.Width(buttonWidth))) {
				list.RemoveAt(index);
				action = EditorAction.Remove;
			}

			return action;
		}
		public static EditorAction ButtonPanel<T>(IList<T> list, int index, string up, string down, string duplicate, string remove) {
			return ButtonPanel(list, index, up, down, duplicate, remove, 0, GUI.skin.button);
		}
		public static EditorAction ButtonPanel<T>(IList<T> list, int index, bool move, bool duplicate, bool remove, float panelWidth, GUIStyle style) {
			return ButtonPanel(list, index, move ? "/\\" : null, move ? "\\/" : null, duplicate ? "*" : null, remove ? "X" : null, panelWidth, style);
		}
		public static EditorAction ButtonPanel<T>(IList<T> list, int index, bool move, bool duplicate, bool remove, GUIStyle style) {
			return ButtonPanel(list, index, move, duplicate, remove, 0, style);
		}
		public static EditorAction ButtonPanel<T>(IList<T> list, int index, bool move, bool duplicate, bool remove) {
			return ButtonPanel(list, index, move, duplicate, remove, GUI.skin.button);
		}
		public static EditorAction ButtonPanel<T>(IList<T> list, T elem, bool move, bool duplicate, bool remove) {
			return ButtonPanel(list, list.IndexOf(elem), move, duplicate, remove);
		}
		public static EditorAction ButtonPanel<T>(IList<T> list, int index) {
			return ButtonPanel(list, index, true, false, true);
		}
		public static EditorAction ButtonPanel<T>(IList<T> list, T elem) {
			return ButtonPanel(list, list.IndexOf(elem));
		}

		public static List<T> ListButtonPanel<T>(List<T> list, int minLength) {
			minLength = Mathf.Max(0, minLength);

			while (list.Count < minLength) {
				list.Add(default);
			}

			if (GUILayout.Button("+", Layout.Width(Constants.W.SmallButton))) {
				list.Add(default);
			}
			EditorGUI.BeginDisabledGroup(list.Count <= minLength);
			if (list.Count > 0 && GUILayout.Button(list.Count > minLength || minLength <= 0 ? "-" : "X", Layout.Width(Constants.W.SmallButton))) {
				list.RemoveAt(list.Count - 1);
			}
			EditorGUI.EndDisabledGroup();
			return list;
		}
		public static T[] ListButtonPanel<T>(T[] array, int minLength) {
			return ListButtonPanel(new List<T>(array), minLength).ToArray();
		}

		public static (List<T>, bool) TryButtonPanel<T>(List<T> list, int index) {
			List<T> oldList = new List<T>(list);
			ButtonPanel(list, index);
			return (list, !list.Equals(oldList));
		}

		public static (C, bool) TryAddButton<E, C>(string text, C list, E elemToAdd, params GUILayoutOption[] options) where C : ICollection<E> {
			bool changed = false;
			text = text.Trim();
			if (GUILayout.Button(text.Length == 0 ? "+" : "Add " + text, options)) {
				list.Add(elemToAdd);
				changed = true;
			}
			return (list, changed);
		}

		public static C AddButton<E, C>(string text, C list, E elemToAdd, params GUILayoutOption[] options) where C : ICollection<E> {
			return TryAddButton(text, list, elemToAdd, options).Item1;
		}
		public static C AddButton<E, C>(string text, C list, params GUILayoutOption[] options) where C : ICollection<E> {
			return TryAddButton(text, list, default(E), options).Item1;
		}

		public static bool ToggleButton(bool val, string onText, string offText, params GUILayoutOption[] options) {
			if (GUILayout.Button(val ? onText : offText, options)) val = !val;
			return val;
		}
	}

	public static class Fields {
		public static bool ToggleField(string text, bool value, float labelWidth) {
			Layout.Horizontal(() => {
				if (labelWidth >= 0) LabelField(text, Layout.Width(labelWidth));
				value = Toggle(value, Layout.Width(Constants.W.Toggle));
			});
			return value;
		}
		public static bool ToggleField(bool value) {
			return ToggleField("", value, -1);
		}

		public static Vector2 RangeField(string text, float min, float max) {
			Vector2 v = Vector2Field(text, new Vector2(min, max));
			if (v.x != min && v.x > v.y) v.x = v.y;
			if (v.y != max && v.y < v.x) v.y = v.x;
			return v;
		}

		public static T ObjField<T>(string txt, T obj, params GUILayoutOption[] options) where T : Object {
			return ObjectField(txt, obj, typeof(T), false, options) as T;
		}
		public static T ObjField<T>(GUIContent content, T obj, params GUILayoutOption[] options) where T : Object {
			return ObjectField(content, obj, typeof(T), false, options) as T;
		}
		public static T ObjField<T>(T obj, params GUILayoutOption[] options) where T : Object {
			return ObjectField(obj, typeof(T), false, options) as T;
		}

		public static T RecursiveComponentField<T>(GameObject parent, T obj, params GUILayoutOption[] options) where T : Component {
			T[] objs = parent.GetComponentsRecursive<T>().ToArray();
			string[] names = objs.Perform((o) => o.BaseName());
			return objs[names.IndexOf(DropdownList(names, obj.BaseName(), options))];
		}

		public static List<T> ListField<T>(List<T> list, params GUILayoutOption[] options) where T : Object {
			for (int i = 0; i < list.Count; i++)
				list[i] = ObjField(list[i], options);
			return list;
		}
		public static T[] ArrayField<T>(T[] array, params GUILayoutOption[] options) where T : Object {
			for (int i = 0; i < array.Length; i++)
				array[i] = ObjField(array[i], options);
			return array;
		}
		public static float[] ArrayField(float[] array, params GUILayoutOption[] options) {
			for (int i = 0; i < array.Length; i++)
				array[i] = FloatField(array[i], options);
			return array;
		}
		public static int[] ArrayField(int[] array, params GUILayoutOption[] options) {
			for (int i = 0; i < array.Length; i++)
				array[i] = IntField(array[i], options);
			return array;
		}
		public static string[] ArrayField(string[] array, params GUILayoutOption[] options) {
			for (int i = 0; i < array.Length; i++)
				array[i] = TextField(array[i], options);
			return array;
		}

		public static LayerMask LayerMaskField(string name, LayerMask mask) {
			return ConcatenatedLayersMaskToLayerMask(
				MaskField(name, LayerMaskToConcatenatedLayersMask(mask), layers));
		}
		public static LayerMask LayerMaskField(LayerMask mask) {
			return ConcatenatedLayersMaskToLayerMask(
				MaskField(LayerMaskToConcatenatedLayersMask(mask), layers));
		}

		public static T DropdownList<T>(T[] array, int index, params GUILayoutOption[] options) {
			if (index < 0 || index >= array?.Length) index = 0;
			if (array == null || array.Length == 0) {
				Layout.Disable(true, () => LabelField(array == null ? "No Dropdown" : "Empty Dropdown", EditorStyles.helpBox, options));
				return default;
			} else {
				return array[Popup(index, array.ToStringArray(), options)];
			}
		}
		public static T DropdownList<T>(T[] array, T elem, params GUILayoutOption[] options) {
			return DropdownList(array, elem == null ? 0 : array.IndexOf(elem), options);
		}
	}
}
namespace CGUI.Utility {
	public class Aesthetic {
		public static Texture2D[] AssetsPreview(GameObject[] objs) {
			return objs.Perform((o) => o == null ? null : AssetPreview.GetAssetPreview(o));
		}
		public static Texture2D[] AssetsPreview(Component[] objs) {
			return AssetsPreview(objs.Perform((o) => o?.gameObject));
		}
	}

	[System.Serializable]
	public class ErrorHandler<T> {
		[SerializeField]
		private string[] messages;
		[SerializeField]
		private System.Func<T, bool>[] funcs;

		private (string Message, System.Func<T, bool> Validate)[] Cases {
			get => (messages, funcs).Pack();
			set {
				(messages, funcs) = value.Unpack();
			}
		}
		[SerializeField]
		private System.Func<T, T> resolveFunc;
		
		public ErrorHandler(System.Func<T, T> resolveFunc, params (string, System.Func<T, bool>)[] cases) {
			Cases = cases;
			this.resolveFunc = resolveFunc;
		}

		public ErrorHandler(System.Action<T> resolveFunc, params (string, System.Func<T, bool>)[] cases) {
			Cases = cases;
			this.resolveFunc = (o) => { resolveFunc(o); return o; };
		}

		public bool Correct(T obj) {
			foreach (var (Message, Validate) in Cases)
				if (!Validate(obj)) return false;
			return true;
		}

		public void Draw(T obj) {
			Cases.Perform((c) => { if (!c.Validate(obj)) LabelField(c.Message, EditorStyles.helpBox); });
		}
		public T Resolve(T obj) { return resolveFunc == null ? default : resolveFunc(obj); }
	}
}
#endif