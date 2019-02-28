using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static ToolBox.Syntax.Syntax;
using static CGUI.Constants;

public abstract class GraphWindow : CGUIWindow {
	protected class ConnectionManager {
		public Graph graph { get; private set; }
		public Graph.Node from { get; private set; }

		public ConnectionManager(Graph graph) {
			this.graph = graph;
			if (graph == null) throw new System.ArgumentNullException("Graph cannot be null");
		}

		public void Begin(Graph.Node graph) {
			if (Ongoing) return;
			from = graph;
		}
		public void End() => from = null;

		public bool Ongoing => from != null;

		public void Toggle(Graph.Node node) {
			if (!Ongoing) Begin(node);
			else {
				graph.ToggleConnection(from, node);
				End();
			}
		}
	}

	public override string Title => "Graph";

	#region Data
	public const float GRAPH_OFFSET = 20,
					   SCROLL_SENSITIVITY = 0.03f,
					   MIN_DRAG_DISTANCE = 3;
	public const int SNAP_GRID_SIZE = 10;

	public static Color DefaultBgColor => new Color(71.0f / 255, 71.0f / 255, 71.0f / 255);
	public static Color DefaultGridColor => Color.white.Fade(0.3f);

	[SerializeField]
	protected float zoom = 1;
	public float Zoom {
		get { return zoom; }
		set { if (value >= 0.5f && value <= 50) zoom = value; }
	}
	public Vector2 panPos = new Vector2(Screen.width, Screen.height) / 2;
	public bool createNode = false, showGrid = true, showDebugInfo = false;
	private List<(System.Func<bool>, System.Action)> funcQueue = new List<(System.Func<bool>, System.Action)>();
	
	protected abstract UndirectedEventGraph EventGraph { get; }

	public Vector2 StartClickPos { get; private set; } = default;
	public bool SelectActive { get; private set; } = false;
	#endregion

	#region Event Handling
	/// <summary>
	/// Get index of the node clicked in a given graph
	/// </summary>
	/// <param name="e">Current event</param>
	/// <param name="graph">Graph to check</param>
	/// <param name="posFunc">Local to world space conversion</param>
	/// <returns>Index of clicked node in graph</returns>
	protected virtual int ClickedNodeI(Event e, Graph graph, System.Func<Graph.Node, Vector2> posFunc) {
		int index = -1;
		for (int i = 0; i < graph.Count; i++) {
			if (index != -1) {
				Vector2 a = graph[index].size, c = graph[i].size;
				if (c.x > a.x || c.y > a.y) continue;
			}
			Vector2 p = ScreenPos(posFunc == null ? graph[i].pos : posFunc(graph[i])), s = ScreenSize(graph[i].size);
			if (e.mousePosition.InInterval(p, p + s))
				index = i;
		}
		return index;
	}
	protected virtual int ClickedNodeI(Event e, Graph graph) {
		return ClickedNodeI(e, graph, null);
	}

	protected (Vector2, Vector2) SelectedGroupShape(UndirectedEventGraph graph, System.Func<Graph.Node, Vector2> toWorldPos) {
		Graph.Node[] selected = graph.Selected;
		if (selected.Length == 0) return (default, default);

		// Convert to world space
		Vector2 WorldPos(Graph.Node node) => toWorldPos == null ? node.pos : toWorldPos(node);

		Graph.Node first = selected.Head();
		Vector2 start = WorldPos(first), end = WorldPos(first);
		int i = 0;
		foreach (Graph.Node node in selected) {
			Vector2 p0 = WorldPos(node), p1 = p0 + node.size;
			if (i++ == 0) { start = p0; end = p1; }
			else {
				start = Vector2.Min(start, p0);
				end = Vector2.Max(end, p1);
			}
		}
		// Convert to screen space
		return (ScreenPos(start), ScreenPos(end));
	}
	protected Rect SelectedGroupRect(UndirectedEventGraph graph, System.Func<Graph.Node, Vector2> toWorldPos) {
		var (start, end) = SelectedGroupShape(graph, toWorldPos);
		return new Rect(start, end - start);
	}

	protected bool ClickedSelectGroup(Event e, UndirectedEventGraph graph, System.Func<Graph.Node, Vector2> toWorldPos) {
		var (min, max) = SelectedGroupShape(graph, toWorldPos);
		return e.mousePosition.InInterval(min, max);
	}

	protected void HandleEvents(Event e, Graph.Node dragging, System.Func<Graph.Node, Vector2> fromPos, System.Func<Graph.Node, Vector2, Vector2> toPos) {
		if (e.isKey && e.type == EventType.KeyDown) OnKeyDown(e);
		if (e.isScrollWheel) OnScroll(e);
		if (e.type == EventType.MouseDrag) OnDrag(e, dragging, fromPos, toPos);
		if (e.button == 0) {
			if (e.type == EventType.MouseDown) OnLeftMouseDown(e);
			if (e.type == EventType.MouseUp) OnLeftMouseUp(e);
		}
		if (e.button == 1) {
			if (e.type == EventType.MouseDown) OnRightMouseDown(e);
			if (e.type == EventType.MouseUp) OnRightMouseUp(e);
		}
	}
	protected void HandleEvents(Event e, Graph.Node dragging) {
		HandleEvents(e, dragging, null, null);
	}

	protected void HandleSelection(Event e, UndirectedEventGraph graph, int i) {
		// Don't handle selection if a valid drag was performed
		if (!graph.IsDragging || graph.DragDelta.magnitude < MIN_DRAG_DISTANCE / Zoom) {
			graph.Select(i);
		}
		graph.EndDrag();
	}
	protected void HandleSelection(Event e, UndirectedEventGraph graph, Graph.Node node) => HandleSelection(e, graph, graph.IndexOf(node));

	protected virtual void OnKeyDown(Event e) {
		if (e.alt || e.control) SelectActive = false;
	}
	protected virtual void OnScroll(Event e) {
		float prevZoom = Zoom;
		Zoom -= e.delta.y * Zoom * SCROLL_SENSITIVITY;
		panPos -= (e.mousePosition - panPos) * ((Zoom / prevZoom) - 1);
	}
	protected virtual void OnDrag(Event e, Graph.Node dragging, System.Func<Graph.Node, Vector2> fromPos, System.Func<Graph.Node, Vector2, Vector2> toPos) {
		if (e.button == 2 || (e.button == 0 && e.alt)) panPos += e.delta;

		// When control is held down, drag all selected nodes 
		if (e.control) {
			EventGraph.Selected.Perform((r) => {
				r.pos += e.delta / Zoom;
			});
		}
		else {
			if (!e.alt && e.button == 0) {
				EventGraph.Drag(WorldPos(e.mousePosition), null);
				if (!EventGraph.IsDragging) {
					EditorGUI.DrawRect(new Rect(StartClickPos, e.mousePosition - StartClickPos), Color.white.Fade(0.4f));
				}
			}
			if (e.shift) SnapPos(dragging, null, null);
		}
	}
	protected virtual void OnLeftMouseDown(Event e) {
		StartClickPos = e.mousePosition;

		Vector2 worldMousePos = WorldPos(e.mousePosition);
		EventGraph.EndDrag();
		EventGraph.StartDrag(worldMousePos, ClickedNodeI(e, EventGraph, null), null);

		if (!(e.control || e.alt || EventGraph.IsDragging)) SelectActive = true;
	}
	protected virtual void OnLeftMouseUp(Event e) {
		if (!e.shift) EventGraph.ClearSelection();
		HandleSelection(e, EventGraph, ClickedNodeI(e, EventGraph, null));

		if (SelectActive) {	
			foreach (Graph.Node node in EventGraph) {
				Vector2 s = ScreenPos(node.pos);
				if (s.InInterval(StartClickPos, e.mousePosition)) {
					HandleSelection(e, EventGraph, node);
				}
			}

			SelectActive = false;
		}
	}
	protected virtual void OnRightMouseDown(Event e) { }
	protected virtual void OnRightMouseUp(Event e) { }
	#endregion

	#region Coordinate Management
	public void ResetView() {
		zoom = 1;
		panPos = new Vector2(Screen.width, Screen.height) / 2;
	}

	public Vector2 ScreenPos(Vector2 pos) => pos * Zoom + panPos;
	public Vector2 ScreenPos(Graph.Node node) => ScreenPos(node.pos);
	public Vector2 WorldPos(Vector2 screenPos) => (screenPos - panPos) / Zoom; // Inverse of ScreenPos

	public Vector2 ScreenSize(Vector2 size) => size * Zoom;
	public Vector2 WorldSize(Vector2 size) => size / Zoom; // Inverse of ScreenSize

	public Rect ScreenRect(Rect rect) => new Rect(ScreenPos(rect.position), ScreenSize(rect.size));
	public Rect WorldRect(Rect rect) => new Rect(WorldPos(rect.position), WorldSize(rect.size));

	protected void SnapPos(Graph.Node node, System.Func<Graph.Node, Vector2> fromPos, System.Func<Graph.Node, Vector2, Vector2> toPos) {
		if (node == null) return;
		Vector2 p = (fromPos == null ? node.pos : fromPos(node)).RoundByFactor(SNAP_GRID_SIZE);
		node.pos = toPos == null ? p : toPos(node, p);
	}
	#endregion

	#region Queued Functions
	protected void Button(Rect rect, string text, System.Action action) {
		funcQueue.Add((() => { return GUI.Button(rect, text); }, action));
	}
	protected void RunFuncQueue() {
		foreach (var (Validate, Func) in funcQueue)
			if (Validate()) Func();
		funcQueue.Clear();
	}
	#endregion

	#region Drawers
	protected void DrawBackground(Color backgroundColor, Color gridColor) {
		EditorGUI.DrawRect(new Rect(0, 0, Screen.width, Screen.height), backgroundColor);

		float gridScale = 1;
		if (showGrid) gridScale = DrawAdaptiveGrid(6, gridColor);
	}
	protected void DrawBackground() {
		DrawBackground(DefaultBgColor, DefaultGridColor);
	}

	protected float DrawAdaptiveGrid(float minSize, Color color) {
		float scale = 1;
		while (scale * Zoom < minSize) scale *= 10;
		float cellSize = scale * Zoom;

		DrawGrid(cellSize, color.Fade(Mathf.Clamp01((cellSize - minSize) / (10 - minSize))) * 0.7f);
		DrawGrid(scale * 10 * Zoom, color);

		return scale;
	}
	private void DrawGrid(float cellSize, Color color) {
		float w = Screen.width, h = Screen.height;
		Vector2 p = panPos;

		Handles.color = color;
		for (float x = p.x % cellSize; x < Screen.width; x += cellSize)
			DrawAxisY(x, false);
		for (float y = p.y % cellSize; y < Screen.height; y += cellSize)
			DrawAxisX(y, false);
	}

	protected void DrawAxisX(float y, bool relative) {
		Vector2 p = relative ? panPos : Vector2.zero;
		Handles.DrawLine(new Vector2(0, p.y + y), new Vector2(Screen.width, p.y + y));
	}
	protected void DrawAxisY(float x, bool relative) {
		Vector2 p = relative ? panPos : Vector2.zero;
		Handles.DrawLine(new Vector2(p.x + x, 0), new Vector2(p.x + x, Screen.height));
	}

	/// <summary>
	/// Draw edge in screen space
	/// </summary>
	/// <param name="p0">First position in screen space</param>
	/// <param name="p1">Second position in screen space</param>
	/// <param name="color">Edge color</param>
	/// <param name="width">Edge width</param>
	/// <param name="directed">If edge is directed</param>
	protected void DrawEdge(Vector2 p0, Vector2 p1, Color color, float width, LineStyle style, bool directed) {
		Vector2 dir = (p1 - p0).Sign().ToFloatVector() * 20 * Zoom,
				t0 = dir + p0, t1 = dir + p1;

		Switch(style,
			(LineStyle.Solid,  () => { Handles.color = color; Handles.DrawAAPolyLine(width, p0, p1); }), 
			(LineStyle.Dashed, () => CGUI.Drawers.DrawDashedLine(p0, p1, color, width, 10)), 
			(LineStyle.Bezier, () => Handles.DrawBezier(p0, p1, t0, t1, color, BlankTexture, width))
		);

		//if (directed) CGUI.Drawers.DrawPoint(p1, color, 20, width);
	}

	protected void DrawOutline(Vector2 a, Vector2 size, float lineWidth, Color color, LineStyle style, bool padded) {
		if (padded) {
			a -= Vector2.one * lineWidth;
			size += Vector2.one * lineWidth * 2;
		}

		Vector2 b = a + Vector2.right * size.x,
				c = b + Vector2.up * size.y,
				d = c + Vector2.left * size.x;
		void Draw(Vector2 x, Vector2 y) => DrawEdge(x, y, color, lineWidth, style, false);
		Draw(a, b);
		Draw(b, c);
		Draw(c, d);
		Draw(d, a);
	}
	protected void DrawOutline(Vector2 a, Vector2 size, float lineWidth, Color color, LineStyle style) {
		DrawOutline(a, size, lineWidth, color, style, false);
	}

	protected void DrawSelectionPanel() {
		Vector2 mousePos = Event.current.mousePosition;
		if (SelectActive && StartClickPos != mousePos) {
			EditorGUI.DrawRect(new Rect(Vector2.Min(StartClickPos, mousePos), (mousePos - StartClickPos).Abs()), Color.white.Fade(0.2f));
		}
	}
	#endregion
}
