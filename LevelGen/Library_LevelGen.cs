using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using static UnityEditor.EditorGUILayout;
using static CGUI.Drawers;
using static CGUI.Layout;
using static CGUI.Constants;
using static CGUI.Input.Buttons;
using static CGUI.Input.Fields;
using static CGUI.Input.Compound;
using static ToolBox.Data.Resources;
using CGUI.Utility;
using CGUI.Styles;

namespace LevelGen {
	public class Data {
		public const int LIBRARY_NODE_SIZE = 30;

		/// <summary>
		/// A persistent object carries over between rooms
		/// </summary>
		public static string[] PersistentTags => new string[] { "GameController", "Player" };

		#region Color Palette
		public static Color EdgeColor => Color.red;
		public static Color SelectColor => new Color(1, 180.0f / 255, 0);
		public static Color HighlightColor => Color.cyan;
		public static Color[] Palette => new Color[] { EdgeColor, SelectColor, HighlightColor };
		#endregion
	}

	public static class Tools {
		public static void AutoFormatTexture(Texture tex) {
			TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tex));
			importer.isReadable = true;
			importer.sRGBTexture = true;
			importer.alphaSource = TextureImporterAlphaSource.FromInput;
			importer.alphaIsTransparency = true;
			importer.wrapMode = TextureWrapMode.Clamp;
			importer.filterMode = FilterMode.Point;
			importer.textureCompression = TextureImporterCompression.Uncompressed;

			importer.SaveAndReimport();
			Debug.Log("Formatted texture " + tex);
		}
	}

	public static class Drawers {
		/// <summary>
		/// Draw icon for library nodes
		/// </summary>
		/// <param name="pos">Position in screen space</param>
		/// <param name="size">Size in screen space</param>
		public static void DrawLibraryIcon(Vector2 pos, float size, Color color) {
			Handles.color = color;
			Handles.CubeHandleCap(0, pos, default, size, default);
		}
	}
}
namespace LevelGen.Editor {
	public class RoomInspector {
		private LevelGenerator2D t;

		public RoomInspector(LevelGenerator2D generator) {
			t = generator;
		}

		private readonly ErrorHandler<Texture2D> texErrors = new ErrorHandler<Texture2D>(
			(tex) => Tools.AutoFormatTexture(tex),
			("Texture must be read-write enabled", (o) => o.isReadable),
			("Texture must be unfiltered", (o) => o.filterMode == FilterMode.Point),
			("Texture must have the RGBA32 color format", (o) => o.format == TextureFormat.RGBA32)
		);
		private readonly ErrorHandler<string> idErrors = new ErrorHandler<string>(
			(id) => id.Generate(6, ToolBox.Utility.Text.RandomAlphaNumericChar),
			("ID is invalid or already in use", (o) => o.Trim().Length != 0)
		);

		public void DrawPanel(int i, int j) {
			BlockMap curr = t.GetMapById(t.level.roomGraph[i]?.value), 
				prev = t.GetMapById(t.level.roomGraph[j]?.value);
			if (curr == null) throw new System.ArgumentNullException("No block container found");
			MapGroup group = t.GetGroupWithId(curr.Id);

			if (prev != null && !prev.minimized) { Space(); Line(); Space(); }

			EditorAction editorAction = EditorAction.Null;
			curr.minimized = !Collapsible(!curr.minimized, curr.Id, -1, () => {
				editorAction = ButtonPanel(group.maps, curr, false, false, true);
			}, () => DrawBlocksPanels(ref curr));

			if (editorAction == EditorAction.Remove) {
				t.RemoveRoom(curr.Id);
			}
		}

		/// <summary>
		/// Draws GUI for editing blocks for a given map
		/// </summary>
		/// <param name="map">Library containing blocks</param>
		public void DrawBlocksPanels(ref BlockMap map) {
			MapGroup group = t.GetGroupWithId(map.Id);
			Block[] localBlocks = map.blockOverrides.ToArray();
			Block[] sharedBlocks = group.Blocks.Filter((b) => !localBlocks.Contains(b)).ToArray();

			DrawBlocksPanel("Shared", ref sharedBlocks, (b) => {
				if (GUILayout.Button(Symbols["doubleDagger"].ToString(), Width(W.SmallButton))) {
					localBlocks = localBlocks.Append(b);
				}
			});
			DrawBlocksPanel("Local", ref localBlocks, (b) => {
				if (GUILayout.Button(Symbols["dagger"].ToString(), Width(W.SmallButton))) {
					localBlocks = localBlocks.Filter((b1) => b1 != b).ToArray();
				}
			});

			// Update block data
			group.Set(sharedBlocks);
			map.blockOverrides = new List<Block>(localBlocks);
		}

		private void DrawBlocksPanel(string title, ref Block[] blocks, params System.Action<Block>[] extensions) {
			if (blocks.Length == 0) return;
			if (title != null) LabelField(title, LabelStyles.centeredBoldMiniLabel);
			//DrawStasisPanel(blocks);

			Block prev = null;
			blocks.Perform((block) => {
				if (prev != null && prev.type != Block.Type.Simple) { Line(); Space(); }
				Horizontal(() => {
					#region Block Panel
					GUILayout.Box("", BoxStyles.Colored(block.color), Width(60), Height(16));

					if (block.type == Block.Type.Simple) DrawImage(block.obj != null ? AssetPreview.GetAssetPreview(block.obj.gameObject) : null, 16);
					else GUILayout.FlexibleSpace();

					Vertical(() => {
						VerticalSpace(2);
						Horizontal(() => {
							if (block.type == Block.Type.Simple) block.obj = ObjField(block.obj);
							block.type = (Block.Type)EnumPopup(block.type, Width(W.Enum));
							block.shape = (Block.Shape)EnumPopup(block.shape, Width(W.Enum));
						});
					});
					
					#endregion
					extensions.Perform((f) => f(block.Clone()));
				});
				
				if (block.type != Block.Type.Simple) {
					DrawBlockSlicePanel(block);
				}

				prev = block.Clone();
			});
		}

		private void DrawBlockSlicePanel(Block block) {
			Horizontal(() => {
				DrawGallery(Aesthetic.AssetsPreview(block.convexSlice), 3, 16);
				DrawTable(block.convexSlice, 3, 0, 0, 4, 0);
			});

			VerticalSpace(6);
			if (block.type == Block.Type.Concave) {
				Horizontal(() => {
					HorizontalSpace(8);
					DrawGallery(Aesthetic.AssetsPreview(block.concaveSlice), 2, 16);
					HorizontalSpace(8);
					DrawTable(block.concaveSlice, 2, 0, 0, 4, 0);
				});
			}
		}

		/// <summary>
		/// Draws GUI for creating a new block map
		/// </summary>
		public void DrawMapCreation() {
			if (!t.createMap) {
				t.createMap = ToggleButton(t.createMap, "", "Create Room", Width(110));
			}
			else {
				t.tempExitColor = Field("Exit Color", 60, () => { return ColorField(t.tempExitColor); });
				t.tempTexture = Field("Map", 60, () => { return ObjField(t.tempTexture); });
				Horizontal(() => {
					t.tempId = Field("ID", 60, () => { return TextField(t.tempId); });
					t.autoFreshId = Toggle(t.autoFreshId, Width(W.Toggle));
				});
				
				string checkId = t.IdAvailable(t.tempId) ? t.tempId : "";

				bool texAssigned = t.tempTexture != null,
					 texCorrect = (texAssigned && texErrors.Correct(t.tempTexture)), 
					 idCorrect  = idErrors.Correct(checkId);
				Horizontal(() => {
					if (ToggleButton(false, "", "Cancel", Width(110))) { t.createMap = false; return; }
					Disable(!idCorrect || !texCorrect, () => {
						if (GUILayout.Button("Add Room", Width(110))) {
							t.AddRoom(new BlockMap(t.tempId, t.tempTexture, t.tempExitColor));
							if (t.autoFreshId) t.tempId = ToolBox.Utility.Text.FreshId(t.tempId);
						}
					});
					if (texAssigned && !texCorrect && GUILayout.Button("Fix Texture", Width(110))) {
						t.tempTexture = texErrors.Resolve(t.tempTexture);
					}
				});

				if (!texAssigned) LabelField("A map texture must be assigned", EditorStyles.helpBox);
				else if (!texCorrect) texErrors.Draw(t.tempTexture);
				if (!idCorrect) idErrors.Draw(checkId);
			}
		}

		/// <summary>
		/// Draws GUI for generating the level
		/// </summary>
		public void DrawLevelGeneration() {
			Horizontal(() => {
				GUI.backgroundColor = new Color(0.175f, 0.55f, 1);
				if (GUILayout.Button("Generate Root", ButtonStyles.Standard)) t.GenerateLevel();
				GUI.backgroundColor = new Color(0.9f, 0, 0);
				if (GUILayout.Button("Delete", ButtonStyles.Standard)) t.DeleteLevel();
				GUI.backgroundColor = new Color(0, 0.85f, 0);
				if (GUILayout.Button("Save level", ButtonStyles.Standard)) t.SaveLevel();
			});
			Space();
		}
	}


}