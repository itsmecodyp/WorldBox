using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using Amazon.Runtime;
using Amazon.Runtime.Internal.Transform;
using BepInEx;
using Google.MiniJSON;
using Newtonsoft.Json;
using SimplerGUI.Submods.UnitClipboard;
using SimpleJSON;
using UnityEngine;
using BodySnatchers;
using UnityEngine.Tilemaps;
using static SimplerGUI.Menus.ActorControlMain;

namespace SimplerGUI.Submods.SimplePowers
{
	[BepInPlugin(pluginGuid, pluginName, pluginVersion)]
	public class PowerMain : BaseUnityPlugin
	{
		public const string pluginGuid = "cody.worldbox.simple.powers";
		public const string pluginName = "SimplePowers";
		public const string pluginVersion = "0.0.0.1";
		public static int windowInUse = 0;
		public bool showSubMod = true;
		public bool showHidePowersWindow;
		public void Awake()
		{
			
		}

		public void OnGUI()
		{
			if (GUI.Button(new Rect(Screen.width - 120, 120, 95, 20), "tPow"))
			{
				showHidePowersWindow = !showHidePowersWindow;
			}
			if (GUI.Button(new Rect(Screen.width - 25, 120, 25, 20), "x"))
			{
				showHidePowersWindow = false;
				showSubMod = false;
			}
			if (showHidePowersWindow)
			{
				mainWindowRect = GUILayout.Window(710401, mainWindowRect, PowerWindow, "Power Stuff", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
			}

		}

		public bool hoverTileTestThing;

		public void PowerWindow(int windowID)
		{
			GuiMain.SetWindowInUse(windowID);
			Color orig = GUI.backgroundColor;
			if (hoverTileTestThing) {GUI.backgroundColor = Color.green;}
			else {GUI.backgroundColor = Color.red; ;}
			if(GUILayout.Button("CopyTest"))
			{
				hoverTileTestThing = !hoverTileTestThing;
				ClearVanillaPower();
				if (hoverTileTestThing)
				{
					PowerButtonSelector.instance.sizeButtMover.setVisible(true, true);
				}
				else
				{
					PowerButtonSelector.instance.sizeButtMover.setVisible(false, true);
				}

			}
			if (useRectangularSelection) { GUI.backgroundColor = Color.green; }
			else { GUI.backgroundColor = Color.red; ; }
			if (GUILayout.Button("UseRectRegion"))
			{
				useRectangularSelection = !useRectangularSelection;
			}
			if (directUnitPath) { GUI.backgroundColor = Color.green; }
			else { GUI.backgroundColor = Color.red; ; }
			if (GUILayout.Button("DirectUnitPath"))
			{
				directUnitPath = !directUnitPath;
				if (directUnitPath)
				{
					PowerButtonSelector.instance.sizeButtMover.setVisible(true, true);
				}
				else
				{
					PowerButtonSelector.instance.sizeButtMover.setVisible(false, true);
				}
			}
			GUI.backgroundColor = orig;
			GUI.DragWindow();
		}

		public bool directUnitPath;

		public static List<TileObject> tileObjects = new List<TileObject>();

		public class TileObject
		{
			public GameObject tileObj;
			public SpriteRenderer spriteRenderer;
			public Vector3 posToHoverAt;
			public float hoverAmplitude = 0.1f; // Adjust this value to control the hover intensity
			public float hoverSpeed = 1f; // Adjust this value to control the speed of the hover

			public float hoverOffset = UnityEngine.Random.Range(0f, Mathf.PI* 2f);
			public Vector3 originalOffset;

			//tiletypes
			public string tileType = "";
			public string topTileType = "";

			//building
			public string buildingID = "";
			public GameObject buildingObj;
			public SpriteRenderer buildingRenderer;

			//1 actor
			//public string buildingID = "";
			public GameObject actorObj;
			public SpriteRenderer actorRenderer;

			//actors
			public List<Actor> actorsOnTile = new List<Actor>();

			//city for setting zone ownership
			public City citySaved;


			public TileObject(WorldTile tileTarget, Vector3 originalOffset)
			{
				tileObj = new GameObject("TileObject");
				tileObj.transform.position = tileTarget.posV3;
				tileObj.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

				spriteRenderer = tileObj.AddComponent<SpriteRenderer>();
				spriteRenderer.sortingLayerID = MapBox.instance.units.GetRandom().spriteRenderer.sortingLayerID;

				Sprite tileSprite = MapBox.instance.tilemap.getVariation(tileTarget).sprite;
				spriteRenderer.sprite = tileSprite;
				// Store the original offset
				posToHoverAt = tileTarget.posV3 + originalOffset;

				// Store the original offset
				this.originalOffset = originalOffset;

				
				//store tileType for changing when "placed" again later
				tileType = tileTarget.main_type.id;
				if(tileTarget.top_type != null)
				{
					topTileType = tileTarget.top_type.id;
				}

				//store building for placing again later
				if(tileTarget.building != null && tileTarget.building.currentTile == tileTarget)
				{
					buildingID = tileTarget.building.asset.id;

					buildingObj = new GameObject("BuildingObject");
					buildingObj.transform.position = tileTarget.posV3 + new Vector3(0f, 0f, 1f);
					buildingObj.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
					//buildingObj.transform.localPosition = ;
					//buildingObj.transform.parent = tileObj.transform;

					buildingRenderer = buildingObj.AddComponent<SpriteRenderer>();
					buildingRenderer.sortingLayerID = tileTarget.building.spriteRenderer.sortingLayerID;
					buildingRenderer.sprite = tileTarget.building.spriteRenderer.sprite;
				}

				//store units for constant moving, have to cancel movement as well in update
				if (tileTarget._units.Count > 0)
				{
					foreach(Actor actor in tileTarget._units)
					{
						actor.enabled = false;
						actorsOnTile.Add(actor);
					}
					actorObj = new GameObject("ActorObject");
					actorObj.transform.position = tileTarget.posV3 + new Vector3(0f, 0f, 1f);
					actorObj.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
					//buildingObj.transform.localPosition = ;
					//actorObj.transform.parent = tileObj.transform;

					actorRenderer = actorObj.AddComponent<SpriteRenderer>();
					actorRenderer.sortingLayerID = tileTarget._units.GetRandom().spriteRenderer.sortingLayerID;
					actorRenderer.sprite = tileTarget._units.GetRandom().getSpriteToRender();
					//actorsOnTile = tileTarget._units;
				}

				//decrease and flash tile for "ripping up" effect
				//MapAction.terraformMain(tileTarget, AssetManager.tiles.get("deep_ocean"));
				//if we do this, moved actors "swim" >:c

				//MapAction.decreaseTile(tileTarget); MapAction.decreaseTile(tileTarget); MapAction.decreaseTile(tileTarget);
				MapBox.instance.flashEffects.flashPixel(tileTarget, 20, ColorType.White);

				if(tileTarget.zone.city != null)
				{
					citySaved = tileTarget.zone.city;
					//Debug.Log("City in tileObj set to: " + tileTarget.zone.city.data.id);
				}

				//add to list for updating later
				tileObjects.Add(this);
			}
		}				


		public static Rect mainWindowRect = new Rect(0f, 1f, 1f, 1f);

		public bool isDragging = false;
		public Vector3 dragStartPosition;
		public Vector3 dragEndPosition;
		public bool useRectangularSelection = false;

		public List<Actor> actorsToDirect = new List<Actor>();

		public void Update()
		{
			if (global::Config.gameLoaded)
			{
				WorldTile mouseTile = MapBox.instance.getMouseTilePos();
				if(directUnitPath && mouseTile != null)
				{
					MapBox.instance.highlightFrom(mouseTile, global::Config.currentBrushData);
				}
				if (Input.GetMouseButtonDown(0) && directUnitPath && mouseTile != null) {
					BrushData brush = global::Config.currentBrushData;
					for (int i = 0; i < global::Config.currentBrushData.pos.Length; i++)
					{
						WorldTile tile = MapBox.instance.GetTile(global::Config.currentBrushData.pos[i].x + mouseTile.x, global::Config.currentBrushData.pos[i].y + mouseTile.y);
						if (tile != null)
						{
							if (tile._units.Count > 0)
							{
								for (int y = 0; y < tile._units.Count; y++)
								{
									Actor actor = tile._units[y];
									if (actorsToDirect.Contains(actor) == false)
									{
										SimpleMessages.Messages.ActorSay(actor, "Ready to move!", 1.5f);
										actorsToDirect.Add(actor);
									}
								}
							}
						}
					}
					
				}
				if (Input.GetMouseButtonDown(1) && directUnitPath && mouseTile != null && actorsToDirect.Count > 0)
				{
					for (int i = 0; i < actorsToDirect.Count; i++)
					{
						Actor actor = actorsToDirect[i];
						actor.cancelAllBeh();
						actor.goTo(mouseTile, true, true);
						SimpleMessages.Messages.ActorSay(actor, "Moving!", 1.5f);
					}
					actorsToDirect.Clear();
				}

				if (hoverTileTestThing && mouseTile != null)
				{
					if(useRectangularSelection == false)
					{
						//show brush highlight
						MapBox.instance.highlightFrom(mouseTile, global::Config.currentBrushData);
					}
					else
					{
						if(isDragging == false)
						{
							//flash single tile
							MapBox.instance.flashEffects.flashPixel(mouseTile, 20, ColorType.White);
						}
						else
						{
							List<WorldTile> tempList = CheckTilesBetween2(MapBox.instance.GetTile((int)dragStartPosition.x, (int)dragStartPosition.y), MapBox.instance.getMouseTilePos());
							foreach (WorldTile tile in tempList)
							{
								MapBox.instance.flashEffects.flashPixel(tile, 20, ColorType.White);
							}
						}
					}
					if (Input.GetKey(KeyCode.LeftControl) && /*Input.GetMouseButtonDown(0)*/Input.GetKeyDown(KeyCode.C))
					{
						if (useRectangularSelection)
						{
							isDragging = true;
							dragStartPosition = mouseTile.posV3;
						}
						else
						{
							for (int i = 0; i < global::Config.currentBrushData.pos.Length; i++)
							{
								WorldTile tile = MapBox.instance.GetTile(global::Config.currentBrushData.pos[i].x + mouseTile.x, global::Config.currentBrushData.pos[i].y + mouseTile.y);
								if (tile != null)
								{
									// Store the original position relative to the mouse cursor
									Vector3 originalOffset = tile.posV3 - mouseTile.posV3;
									TileObject newTileObject = new TileObject(tile, originalOffset);
								}
							}
						}
					}
					else if (isDragging && Input.GetKeyUp(KeyCode.C))
					{
						isDragging = false;
						dragEndPosition = mouseTile.posV3;
						if (useRectangularSelection)
						{
							// Calculate the rectangular area between start and end positions
							Rect selectionRect = new Rect(dragStartPosition.x, dragStartPosition.y, dragEndPosition.x - dragStartPosition.x, dragEndPosition.y - dragStartPosition.y);

							// Hover the selected rectangular area centered around the mouse

							//first, detect if tiles are in the selection, make their hover objects
							Vector3 center = new Vector3(selectionRect.x + selectionRect.width / 2f, selectionRect.y + selectionRect.height / 2f, 0f);
							foreach (WorldTile tile in MapBox.instance.tilesList)
							{
								// Check if the tile's position is inside the selection rectangle
								Vector3 tilePos = tile.posV3;
								if (tilePos.x >= selectionRect.x && tilePos.x <= selectionRect.x + selectionRect.width &&
									tilePos.y >= selectionRect.y && tilePos.y <= selectionRect.y + selectionRect.height)
								{
									Vector3 originalOffset = tile.posV3 - mouseTile.posV3;
									TileObject newTileObject = new TileObject(tile, originalOffset);
									newTileObject.posToHoverAt = center + originalOffset;
								}
								else
								{
									// If the tile is not inside the selection rectangle, you may want to reset its hover position or handle it differently
									// tileObj.posToHoverAt = someDefaultPosition;
								}
							}
						}
						else
						{
							for (int i = 0; i < global::Config.currentBrushData.pos.Length; i++)
							{
								WorldTile tile = MapBox.instance.GetTile(global::Config.currentBrushData.pos[i].x + mouseTile.x, global::Config.currentBrushData.pos[i].y + mouseTile.y);
								if (tile != null)
								{
									// Store the original position relative to the mouse cursor
									Vector3 originalOffset = tile.posV3 - mouseTile.posV3;
									TileObject newTileObject = new TileObject(tile, originalOffset);
								}
							}
						}
						
						
					}
					if (Input.GetKeyDown(KeyCode.V) && Input.GetKey(KeyCode.LeftControl))
					{

						//"paste" tiles
						foreach (TileObject obj in tileObjects)
						{

							Vector3 targetPosition = obj.tileObj.transform.position;
							WorldTile targetTile = MapBox.instance.GetTile((int)targetPosition.x, (int)targetPosition.y);
							if (targetTile != null)
							{
								//finalize actor positions onto the "pasted" tiles
								//this needs changed to duplicate actors here, finalize actor position back at his original
								foreach (Actor actor in obj.actorsOnTile)
								{
									actor.enabled = true;
									UnitClipboard_Main.CopyUnit(actor, true);
									UnitClipboard_Main.PasteUnit(targetTile, UnitClipboard_Main.selectedUnitToPaste);
									//actor.setCurrentTilePosition(targetTile);
								}
								//should clear the dict when finished
								//UnitClipboard_Main.unitClipboardDict.Clear();
								obj.actorsOnTile.Clear();

								//change tile type
								if (obj.tileType != "")
								{
									if (obj.topTileType != "")
									{
										MapAction.terraformTile(targetTile, AssetManager.tiles.get(obj.tileType), AssetManager.topTiles.get(obj.topTileType));
									}
									else
									{
										MapAction.terraformMain(targetTile, AssetManager.tiles.get(obj.tileType));
									}
								}

								if(obj.citySaved != null && targetTile.zone.city != obj.citySaved)
								{
									obj.citySaved.addZone(targetTile.zone);
								}
							}
							if(obj.citySaved != null)
							{
								obj.citySaved.setStatusDirty();
							}

							//disable renderer of objects
							//they need recycled, destroyed, etc as well...
							obj.spriteRenderer.enabled = false;
							if (obj.buildingRenderer != null)
							{
								Building newBuilding = MapBox.instance.buildings.addBuilding(obj.buildingID, targetTile, false, false);
								if(obj.citySaved != null)
								{
									obj.citySaved.addBuilding(newBuilding);
								}
								obj.buildingRenderer.enabled = false;
							}
							if (obj.actorRenderer != null)
							{
								obj.actorRenderer.enabled = false;
							}
						}

						tileObjects.Clear();
					}
				}
			}

			// Update the position to hover around the mouse cursor
			Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			foreach (TileObject tileObj in tileObjects)
			{
				// Calculate the vertical offset using a sine function to create a floating effect
				float yOffset = Mathf.Sin((Time.time + tileObj.hoverOffset) * tileObj.hoverSpeed) * tileObj.hoverAmplitude;

				// Update the position of the TileObject to hover around the mouse cursor while maintaining the original offset
				tileObj.posToHoverAt = mousePosition + tileObj.originalOffset;
				tileObj.tileObj.transform.position = tileObj.posToHoverAt + new Vector3(0f, yOffset, 0f);

				if(tileObj.buildingObj != null)
					tileObj.buildingObj.transform.position = tileObj.posToHoverAt + new Vector3(0f, yOffset, 1f);
				if (tileObj.actorObj != null)
					tileObj.actorObj.transform.position = tileObj.posToHoverAt + new Vector3(0f, yOffset, 1.5f);

				foreach (Actor actor in tileObj.actorsOnTile)
				{
					actor.stopMovement();
					float yOffset2 = 0.2f; // Adjust this value as needed
					Vector3 localOffset = new Vector3(0f, yOffset2, 1f);
					actor.transform.position = tileObj.tileObj.transform.position + localOffset;
				}
			}
		}
		public void ClearVanillaPower()
		{
			World.world.selectedButtons.unselectAll();
			World.world.selectedButtons.clearHighlightedButton();
		}
		public bool dragCircular = false;
		int dif(int num1, int num2)
		{
			int cout;
			cout = Mathf.Max(num2, num1) - Mathf.Min(num1, num2);
			return cout;
		}
		public List<WorldTile> CheckTilesBetween2(WorldTile target1, WorldTile target2)
		{
			if (target1 == null || target2 == null) return null;
			List<WorldTile> tilesToCheck = new List<WorldTile>(); // list for later
			Vector2Int pos1 = target1.pos;
			Vector2Int pos2 = target2.pos;
			float distanceBetween = Toolbox.DistTile(target1, target2);
			int pSize = (int)distanceBetween;
			PixelFlashEffects flashEffects = MapBox.instance.flashEffects;
			if (dragCircular == false)
			{
				int difx = dif(pos1.x, pos2.x) + 1;
				int dify = dif(pos1.y, pos2.y) + 1;
				if (pos1.x - pos2.x <= 0 && pos1.y - pos2.y <= 0)
				{
					for (int x = 0; x < difx; x++)
					{
						for (int y = 0; y < dify; y++)
						{
							Vector2Int newPos = target1.pos + new Vector2Int(x, y);
							WorldTile checkedTile = MapBox.instance.GetTile(newPos.x, newPos.y);
							tilesToCheck.Add(checkedTile);

						}
					}
				}
				if (pos1.x - pos2.x >= 0 && pos1.y - pos2.y <= 0)
				{
					for (int x = 0; x < difx; x++)
					{
						for (int y = 0; y < dify; y++)
						{
							Vector2Int newPos = target1.pos + new Vector2Int(-x, y);
							WorldTile checkedTile = MapBox.instance.GetTile(newPos.x, newPos.y);
							tilesToCheck.Add(checkedTile);
						}
					}
				}
				if (pos1.x - pos2.x <= 0 && pos1.y - pos2.y >= 0)
				{
					for (int x = 0; x < difx; x++)
					{
						for (int y = 0; y < dify; y++)
						{
							Vector2Int newPos = target1.pos + new Vector2Int(x, -y);
							WorldTile checkedTile = MapBox.instance.GetTile(newPos.x, newPos.y);
							tilesToCheck.Add(checkedTile);

						}
					}
				}
				if (pos1.x - pos2.x >= 0 && pos1.y - pos2.y >= 0)
				{
					for (int x = 0; x < difx; x++)
					{
						for (int y = 0; y < dify; y++)
						{
							Vector2Int newPos = target1.pos + new Vector2Int(-x, -y);
							WorldTile checkedTile = MapBox.instance.GetTile(newPos.x, newPos.y);
							tilesToCheck.Add(checkedTile);
						}
					}
				}
				foreach (WorldTile tile in tilesToCheck)
				{
					flashEffects.flashPixel(tile, 10);
				}
				return tilesToCheck;
			}

			{
				int x = pos1.x;
				int y = pos1.y;
				int radius = (int)distanceBetween;
				Vector2 center = new Vector2(x, y);
				for (int i = x - radius; i < x + radius + 1; i++)
				{
					for (int j = y - radius; j < y + radius + 1; j++)
					{
						if (Vector2.Distance(center, new Vector2(i, j)) <= radius)
						{
							WorldTile tile = MapBox.instance.GetTile(i, j);
							if (tile != null)
							{
								flashEffects.flashPixel(tile, 10);
								tilesToCheck.Add(tile);
							}
						}
					}
				}
				return tilesToCheck;
			}
		}


	}
}