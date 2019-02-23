#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using CGUI.Styles;
using static LevelGen.Data;
using static UnityEditor.EditorGUILayout;
using static CGUI.Drawers;
using static CGUI.Layout;
using static CGUI.Input.Buttons;
using static ToolBox.Syntax.Syntax;

namespace LevelGen.Editor {
	public class Window_Graph : GraphWindow {
		private Vector2 ROOM_MENU_POS => new Vector2(0, -22);
		private Vector2 EXIT_MENU_POS => new Vector2(0, 0);
		private Vector2 ROOT_POS => ROOM_MENU_POS + Vector2.right * 22;

		#region References
		protected static LevelGenerator2D gen => GameObject.Find("_Level")?.GetComponent<LevelGenerator2D>();
		protected LevelGenerator2D t;

		private UndirectedEventGraph Rooms {
			get { return t?.level?.roomGraph; }
			set { t.level.roomGraph = value; }
		}
		private UndirectedGraph Exits {
			get { return t?.level?.exitGraph; }
			set { t.level.exitGraph = value; }
		}

		protected override UndirectedEventGraph EventGraph => Rooms;
		#endregion

		#region Exits Coordinate Management
		public Vector2 WorldToExitPos(Graph.Node exit, Vector2 worldPos) {
			Graph.Node parent = Rooms.FindNodeByValue(exit.value);

			return ((worldPos - parent.pos).Round() / parent.size).Clamp(Vector2.zero, Vector2.one);
		}
		public Vector2 ExitToWorldPos(Graph.Node exit) {
			if (exit == null) return Vector2.one * -1;
			Graph.Node parent = Rooms.FindNodeByValue(exit.value);
			return parent.pos + exit.pos.Clamp01() * parent.size;
		}
		public Vector2 ExitToScreenPos(Graph.Node exit) {
			return ScreenPos(ExitToWorldPos(exit));
		}

		public Vector2 FixExitPos(Vector2 p0, Vector2 p1, Vector2 size) {
			if (size.y > size.x) p1.x = p0.x;
			else p1.y = p0.y;
			return p1;
		}
		#endregion

		#region Event Data Management
		private ConnectionManager connectExits;
		#endregion

		protected override bool DrawGUIInvariant => !(t?.level == null || Rooms == null || Exits == null);
		protected override bool SetupInvariant => !(connectExits == null);

		protected override void OnEnable() {
			base.OnEnable();
			t = gen;
		}

		protected sealed override void OnGUI() {
			if (t == null && gen != null) {
				// Debug.Log("Auto-locating level generator");
				OnEnable();
			}

			if (t?.level != null) Undo.RecordObject(t.level, t.level.name);
			base.OnGUI();
			if (t?.level != null) EditorUtility.SetDirty(t.level);
		}

		protected override void Setup() {
			connectExits = new ConnectionManager(Exits);
		}

		protected override void DrawErrorGUI() {
			LabelField("No level assigned (Go to Level Generator > Show Room Inspector)", EditorStyles.helpBox);
		}
		protected override void DrawGUI() {
			DrawBackground();

			Rooms.Perform(DrawRoom);
			int exitN = Exits.Count;
			for (int i = 0; i < Exits.Count; i++) {
				DrawExit(Exits[i], Color.white);
				if (Exits.Count != exitN) return;
			}
			t.level.groups.Perform(DrawGroup);
			DrawConnections();

			DrawSelectionPanel();

			Horizontal(() => {
				if (GUILayout.Button("Reset View", Width(100))) ResetView();
				showGrid = ToggleButton(showGrid, "Hide Grid", "Show Grid", Width(100));
				showDebugInfo = ToggleButton(showDebugInfo, "Hide Debug", "Show Debug", Width(100));
				if (showDebugInfo) {
					HorizontalSpace(10);
					if (GUILayout.Button("Print paths", Width(100)))
						Debug.Log(ToolBox.Algorithms.Pathfinding.GetPaths(Rooms.Edges, Rooms.RootI).Perform((e) => e.ArrayToString() + "\n").ArrayToString());
				}
			});
			if (showDebugInfo) LabelField("Mouse Pos: " + Event.current.mousePosition + "  |  Pan Pos: " + panPos + "  |  Window Size: " + new Vector2(Screen.width, Screen.height)
				+ "        Dragging: " + Rooms.DragI + "  |  Selected: " + Rooms.SelectI
				+ "        Zoom: " + Zoom + "        Room Count: " + Rooms.Count + "  |  Exit Count: " + Exits.Count + "  |  Edge Count: " + Exits.Edges.Length
				+ "        Group Count: " + t.level.groups.Count
				+ "        Room Connections: " + Rooms.Edges.Length + "  |  Exit Connections: " + Exits.Edges.Length, EditorStyles.helpBox);


			RunFuncQueue();
			HandleEvents(Event.current, Rooms.DragTarget);
		}

		#region Event Handling
		private int ClickedRoomI(Event e) => ClickedNodeI(e, Rooms);
		protected override void OnKeyDown(Event e) {
			base.OnKeyDown(e);
		
			Switch(e.keyCode, 
				(KeyCode.Delete, () => {
					foreach (Graph.Node room in Rooms.Selected)
					t.RemoveRoom(room);
				})
			);
		}

		protected override void OnLeftMouseUp(Event e) {
			base.OnLeftMouseUp(e);
			connectExits.End();
		}
		#endregion

		#region Drawers
		private void DrawSelectionPanel() {
			Graph.Node[] selectedRooms = Rooms.Selected;
			if (selectedRooms.Length == 0) return;

			Vector2? min = null, max = null;
			foreach (Graph.Node node in selectedRooms) {
				min = min.HasValue ? Vector2.Min(node.pos, min.Value) : node.pos;
				max = max.HasValue ? Vector2.Max(node.pos + node.size, max.Value) : node.pos + node.size;
			}
			Vector2 p = ScreenPos(min.Value), s = ScreenSize(max.Value - min.Value);

			DrawOutline(p, s, 4, SelectColor, CGUI.Constants.LineStyle.Solid);

			if (GUI.Button(new Rect(p + ROOM_MENU_POS, Vector2.one * 20), ToolBox.Data.Resources.Symbols["down"].ToString())) {
				t.GroupRooms(selectedRooms.Perform((e) => e.value));
				Rooms.ClearSelection();
			}
		}

		private void DrawGroup(MapGroup group) {
			if (group.maps.Count == 0) return;
			var (head, tail) = group.maps.Perform((m) => Rooms.FindNodeByValue(m.Id)).Step();
			(Vector2 start, Vector2 end) boundary = (head.pos, head.pos + head.size);
			tail.Perform((n) => boundary = (Vector2.Min(n.pos, boundary.start), Vector2.Max(n.pos + n.size, boundary.end)));
			var (p0, p1) = (ScreenPos(boundary.start), ScreenPos(boundary.end));
			Vector2 s = p1 - p0;
			DrawOutline(p0, s, 4, Color.white, CGUI.Constants.LineStyle.Dashed, true);
		}
		private void DrawRoom(Graph.Node room) {
			if (room?.value == null) {
				Debug.LogError("Room node cannot be null");
				return;
			}
			Vector2 p = ScreenPos(room.pos), s = ScreenSize(room.size);
		
			if (t.IdAvailable(room.value)) throw new ArgumentException("The room " + room.value + " has no corresponding block container");
			#region Draw Node (with menu)
			DrawImage(new Rect(p, s), t.GetMapById(room.value).Texture);
			DrawOutline(p, s, 2, Color.black, default);

			// Only draw node menu if it is selected
			if (Rooms.AnySelected(room)) {
				if (!Rooms.IsRoot(room) && GUI.Button(new Rect(p + ROOT_POS, new Vector2(50, 14)), "Root")) {
					Rooms.Root = room;
				}
			}
			#endregion
			
			Handles.Label(p, room.value, LabelStyles.ColoredBg(Color.white.Fade(0.75f), Color.black, FontStyle.Bold));
			if (showDebugInfo) Handles.Label(p + Vector2.right * s + Vector2.down * 16, Rooms.IndexOf(room).ToString(), LabelStyles.Colored(Color.white, FontStyle.Normal));

			if (Rooms.IsRoot(room)) {
				Handles.Label(p + ROOT_POS + Vector2.up * 4, "Root", LabelStyles.Colored(HighlightColor, FontStyle.Bold));
			}
		}

		private void DrawExit(Graph.Node exit, Color color) {
			Vector2 p = ExitToScreenPos(exit), s = ScreenSize(exit.size);
			bool isConnectFrom = exit.Equals(connectExits.from);

			EditorGUI.DrawRect(new Rect(p, s), isConnectFrom ? HighlightColor : color);
			DrawOutline(ExitToScreenPos(exit), s, 2, Color.black, default);

			DrawConnectionPanel(connectExits, exit, ExitToScreenPos, "o", EXIT_MENU_POS);
		}

		private bool ConnectedWithOngoing(ConnectionManager connect, Graph.Node node) => connect != null && connect.graph.Connected(connect.from, node);
		private void DrawConnectionPanel(ConnectionManager connect, Graph.Node node, Func<Graph.Node, Vector2> toScreenPosFunc, string symbol, Vector2 posOffset) {
			if (ConnectedWithOngoing(connect, node)) symbol = "x";

			Vector2 p = toScreenPosFunc == null ? node.pos : toScreenPosFunc(node);
			if (!connect.Ongoing ? Rooms.AnySelected(node.value) : (connect.from.value != node.value)) {
				if (GUI.Button(new Rect(p + posOffset, Vector2.one * 20), symbol)) {
					CommitConnection(connect, node);
				}
			}
		}
		private void CommitConnection(ConnectionManager connect, Graph.Node node) {
			int ai = connect.graph.IndexOf(connect.from), bi = connect.graph.IndexOf(node);
			connect.Toggle(node);
		}

		private void DrawConnections() {
			foreach (var (ai, bi) in Rooms.Edges) {
				Graph.Node a = Rooms[ai], b = Rooms[bi];
				DrawEdge(ScreenPos(a.pos), ScreenPos(b.pos), Palette[new int[] { ai, bi }.Perform((i) => {
					return Rooms.AnySelected(Rooms[i].value) ? 1 : 0;
				}).Max()], 3, CGUI.Constants.LineStyle.Dashed, true);
			}

			foreach (var (ai, bi) in Exits.Edges) {
				Graph.Node a = Exits[ai], b = Exits[bi];

				Vector2 p0 = ExitToScreenPos(a),
						s = ScreenSize(a.size);
			
				Vector2 p1 = ExitToScreenPos(b);
			
				DrawEdge(p0, p1, Palette[new int[] { ai, bi }.Perform((i) => {
					return Exits.IndexOf(connectExits.from) == i ? 2 : Rooms.AnySelected(Exits[i].value) ? 1 : 0;
				}).Max()], 2, CGUI.Constants.LineStyle.Bezier, false);
			}
		}
		#endregion
	}
}
#endif