using System.Collections.Generic;
using ai.behaviours;
using BepInEx;
using HarmonyLib;
using UnityEngine;
//using System.Drawing;

namespace SimpleAdditions
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]

    public class Main : BaseUnityPlugin {
        public const string pluginGuid = "cody.worldbox.simple.additions";
        public const string pluginName = "SimpleAdditions";
        public const string pluginVersion = "0.0.0.0";

        public void Awake()
		{
            Harmony harmony;
            harmony = new Harmony(pluginName);
            harmony.PatchAll();
        }

        public void AddJobs()
        {
            //checking inside city zone for mountains to remove
            //initially meant for "wall" tiles, but who wants mountain inside the city anyway
            BehaviourTaskActor removeMountains = new BehaviourTaskActor();
            removeMountains.id = "removeMountains";
            removeMountains.addBeh(new BehFindMountainInCity());
            //cant path directly onto mountain tiles, so going to neighbor of mountain.. hopefully
            //no that didnt help, mountains arent being removed at all??
            removeMountains.addBeh(new BehTargetCityNeighborTile());
            removeMountains.addBeh(new BehGoToTileTarget { walkOnWater = true, walkOnBlocks = true }); ;
            removeMountains.addBeh(new BehRandomWait(1f, 3f));
            removeMountains.addBeh(new BehRemoveMountainTile());

            //creating wall of mountain using task
            //need "gate" or permanent opening somewhere
            //maybe dict of wall tiles?
            BehaviourTaskActor wallTask = new BehaviourTaskActor();
            wallTask.id = "makeWall";
            wallTask.addBeh(new BehFindBorderTile());
            wallTask.addBeh(new BehGoToTileTarget { walkOnWater = true, walkOnBlocks = true });
            wallTask.addBeh(new BehRandomWait(1f, 3f));
            wallTask.addBeh(new BehTargetCityNeighborTile());
            wallTask.addBeh(new BehGoToTileTarget { walkOnWater = true, walkOnBlocks = true });
            wallTask.addBeh(new BehTargetNonCityNeighborTile());
            wallTask.addBeh(new BehBuildWallTile());

            AssetManager.tasks_actor.add(wallTask);
            AssetManager.tasks_actor.add(removeMountains);
            
            ActorJob newJobMakeWall = new ActorJob();
            newJobMakeWall.id = "wallerJob";
            newJobMakeWall.addTask("removeMountains");
            newJobMakeWall.addTask("makeWall");

            AssetManager.job_actor.add(newJobMakeWall);
            hasAddedAi = true;
            Debug.LogError("Added tasks and jobs");
        }

        public static bool hasAddedAi;

        public void Update()
		{
            if(hasAddedAi == false && AssetManager.job_actor != null) {
                AddJobs();
            }
            /*
            if(hasAddedTraits == false && AssetManager.traits != null) {
                aTraits.AddTraits();
                UnityEngine.Debug.LogError("Added traits");
                hasAddedTraits = true;
			}
            if(hasAddedItems == false && AssetManager.items != null) {
                aItems.AddItems();
                UnityEngine.Debug.LogError("Added items");
                hasAddedItems = true;
            }
            if(hasAddedBuildings == false && AssetManager.buildings != null) {
                aBuildings.AddBuildings();
                UnityEngine.Debug.LogError("Added buildings");
                hasAddedBuildings = true;
            }
            */
        }
        /*
        public bool hasAddedTraits;
        public bool hasAddedItems;
        public bool hasAddedBuildings;
        */

    }

    public class BehFindBorderTile : BehaviourActionActor {
        public override BehResult execute(Actor pActor)
        {
            if(pActor != null && pActor.city != null) {
                HashSet<TileZone> borderz = pActor.city.borderZones;
                foreach(TileZone zone in borderz) {
                    foreach(WorldTile tile in zone.tiles) {
                        foreach(WorldTile worldTileNeighbor in tile.neighbours) {
                            TileTypeBase tTileType = worldTileNeighbor.cur_tile_type;
                            if(worldTileNeighbor.zone.city != pActor.city && tTileType.mountains == false) {
                                Reflection.SetField(pActor, "beh_tile_target", worldTileNeighbor);
                                //pActor.beh_tile_target = worldTileNeighbor;
                                return BehResult.Continue;
                            }
                        }
					}
				}
            }
            return BehResult.Stop;
        }
    }
    public class BehTargetCityNeighborTile : BehaviourActionActor {
        public override BehResult execute(Actor pActor)
        {
            if(pActor != null && pActor.city != null) {
                WorldTile tTile = Reflection.GetField(pActor.GetType(), pActor, "beh_tile_target") as WorldTile;
                if(tTile != null) {
                    foreach(WorldTile worldTileNeighbor in tTile.neighbours) {
                        TileTypeBase tTileType = Reflection.GetField(worldTileNeighbor.GetType(), worldTileNeighbor, "cur_tile_type") as TileTypeBase;
                        if(worldTileNeighbor.zone.city == pActor.city && tTileType.mountains == false) {
                            Reflection.SetField(pActor, "beh_tile_target", worldTileNeighbor);
                            //pActor.beh_tile_target = worldTileNeighbor;
                            return BehResult.Continue;
                        }
                    }
                }
            }
            return BehResult.Stop;
        }
    }
    public class BehTargetNonCityNeighborTile : BehaviourActionActor {
        public override BehResult execute(Actor pActor)
        {
            if(pActor != null && pActor.city != null) {
                WorldTile tTile = Reflection.GetField(pActor.GetType(), pActor, "beh_tile_target") as WorldTile;
                if(tTile != null) {
                    foreach(WorldTile worldTileNeighbor in tTile.neighbours) {
                        TileTypeBase tTileType = Reflection.GetField(worldTileNeighbor.GetType(), worldTileNeighbor, "cur_tile_type") as TileTypeBase;
                        if(worldTileNeighbor.zone.city != pActor.city && tTileType.mountains == false) {
                            Reflection.SetField(pActor, "beh_tile_target", worldTileNeighbor);
                            return BehResult.Continue;
                        }
                    }
                }
            }
            return BehResult.Stop;
        }
    }
    public class BehBuildWallTile : BehaviourActionActor {
        public override BehResult execute(Actor pActor)
        {
            if(pActor != null && pActor.city != null) {
                WorldTile tTile = Reflection.GetField(pActor.GetType(), pActor, "beh_tile_target") as WorldTile;
                if(tTile != null) {
                    MapAction.terraformTile(tTile, TileLibrary.mountains, null);
                    // force actors to restart task and whole job forever (bad but its my example ok)
                    return BehResult.RestartTask;
                }
            }
            return BehResult.Stop;
        }
    }

    public class BehFindMountainInCity: BehaviourActionActor {
        public override BehResult execute(Actor pActor)
        {
            if(pActor != null && pActor.city != null) {
                List<TileZone> zones = Reflection.GetField(pActor.city.GetType(), pActor.city, "zones") as List<TileZone>;
                foreach(TileZone cityZone in zones) {
                    cityZone.tiles.Shuffle();
                    foreach(WorldTile cityTile in cityZone.tiles) {
						if(cityTile.Type.rocks) {
                            Reflection.SetField(pActor, "beh_tile_target", cityTile);
                            return BehResult.Continue;
                        }
                    }
                }
            }
            return BehResult.Stop;
        }
    }
    public class BehRemoveMountainTile : BehaviourActionActor {
        public override BehResult execute(Actor pActor)
        {
            if(pActor != null && pActor.city != null) {
                WorldTile tTile = Reflection.GetField(pActor.GetType(), pActor, "beh_tile_target") as WorldTile;
                if(tTile != null) {
                    // mountains arent being removed at all??
                    MapAction.decreaseTile(tTile);
                    //MapAction.terraformTile(tTile, tTile.Type.decreaseTo, null, null);
                    // force actors to restart task and whole job forever (bad but its my example ok)
                    return BehResult.Continue;
                }
            }
            return BehResult.Stop;
        }
    }
}
