#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using static UnityEditor.EditorGUILayout;
using static CGUI.Layout;
using static CGUI.Constants;
using static CGUI.Input.Fields;
using LevelGen.Editor;

namespace LevelGen.Editor {
	public class Window_Inspector : CGUIWindow {
		protected static LevelGenerator2D gen => GameObject.Find("_Level")?.GetComponent<LevelGenerator2D>();
		protected LevelGenerator2D t;

		public override string Title => "Inspector";

		#region Menu Items
		[MenuItem("Level Generator/Show all", false, 0)]
		public static void ShowAll() {
			ShowInspector(); ShowGraph();
		}

		[MenuItem("Level Generator/Show Inspector", false, 1)]
		static void ShowInspector() { GetWindow<Window_Inspector>("Room Inspector"); }

		[MenuItem("Level Generator/Show Graph", false,  2)]
		static void ShowGraph() { GetWindow<Window_Graph>("Room Graph"); }

		[MenuItem("Level Generator/Delete Level %#g", false, 14)]
		static void DeleteLevel() { gen.DeleteLevel(); Debug.Log("Level deleted"); }

		[MenuItem("Level Generator/Generate Level %g", false, 15)]
		static void GenerateLevel() { gen.GenerateLevel(); Debug.Log("Level generated"); }
		#endregion

		private RoomInspector gui;

		private float WindowWidth { get { return Screen.width - SCROLLVIEW_BAR_WIDTH; } }

		protected override bool SetupInvariant => gen == null || !(t == null || gui == null);
		protected override bool DrawGUIInvariant => gen != null;

		/// <summary>
		/// Sets up level generator and parent
		/// </summary>
		/// <returns>Created level generator</returns>
		internal LevelGenerator2D CreateLevelGenerator() {
			GameObject obj = GameObject.Find("_Level");
			if (obj == null) {
				obj = ToolBox.Management.Game.CreateGameObject(null, "_Level");
			}
			if (obj.GetComponent<LevelGenerator2D>() == null) {
				obj.AddComponent<LevelGenerator2D>();
				Debug.Log("Level generator created");
			}
			return obj.GetComponent<LevelGenerator2D>();
		}

		protected sealed override void OnGUI() {
			if (t?.level != null) Undo.RecordObject(t.level, t.level.name);
			base.OnGUI();
			if (t?.level != null) EditorUtility.SetDirty(t.level);
		}

		protected override void Setup() {
			// Debug.Log("Auto-locating level generator");
			t = gen;
			gui = new RoomInspector(t);
		}

		protected override void DrawErrorGUI() {
			if (gen == null) {
				Horizontal(() => {
					LabelField("No level generator in scene", EditorStyles.helpBox);
					if (GUILayout.Button("Create")) CreateLevelGenerator();
				});
			}
		}

		protected override void DrawGUI() {
			Scroll("main", () => {
				t.level = ObjField(t.level);
				if (t.level == null) LabelField("A level must be assigned", EditorStyles.helpBox);
				else {
					Space();
					Scroll("blocks", () => {
						if (t.level.roomGraph.SelectI.Length == 0) LabelField("Nodes selected in Room Graph will show up here", EditorStyles.helpBox);
						else {
							int j = -1;
							t.level.roomGraph.SelectI.Perform((i) => {
								gui.DrawPanel(i, j);
								j = i;
							});
						}
					});
					Line(); Space();

					gui.DrawMapCreation();
					gui.DrawLevelGeneration();

					ResetGuiBgColor();
				}
			});
		}
	}
}
#endif