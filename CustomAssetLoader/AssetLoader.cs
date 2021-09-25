using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using WorldBox3D;
using CustomAssetLoader;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.Tilemaps;
using System.Globalization;

namespace CustomAssetLoader
{
    [BepInPlugin("cody.worldbox.assetloader", "AssetLoader", "0.0.0.1")]
    public class AssetLoader_Main : BaseUnityPlugin
    {

        public void Awake()
        {
            Harmony harmony = new Harmony("AssetLoader");
            MethodInfo original = AccessTools.Method(typeof(Building), "setSpriteMain");
            MethodInfo patch = AccessTools.Method(typeof(AssetLoader_Main), "setSpriteMain_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony("AssetLoader");
            original = AccessTools.Method(typeof(DropsLibrary), "action_fireworks");
            patch = AccessTools.Method(typeof(AssetLoader_Main), "action_fireworks_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            /*
            original = AccessTools.Method(typeof(Building), "setTemplate");
            patch = AccessTools.Method(typeof(AssetLoader_Main), "setTemplate_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            */
            Debug.Log("Assetloader started");
        }
        public static bool action_fireworks_Prefix(WorldTile pTile = null, string pDropID = null)
        {
            MapBox.instance.stackEffects.CallMethod("spawnFireworks", new object[] { pTile, 0.05f });
            return false;
        }
      
        #region setTemplatePatch
        /* 
        public static bool setTemplate_Prefix(BuildingAsset pTemplate, Building __instance)
        {
            if (addedBuildings.Contains(pTemplate.id))
            {
                BuildingData data = Reflection.GetField(__instance.GetType(), __instance, "data") as BuildingData;
                BuildingAsset stats = Reflection.GetField(__instance.GetType(), __instance, "stats") as BuildingAsset;
                BuildingMapIcon mapIcon = Reflection.GetField(__instance.GetType(), __instance, "mapIcon") as BuildingMapIcon;
                Transform m_transform = Reflection.GetField(__instance.GetType(), __instance, "m_transform") as Transform;

                Dictionary<string, BuildingMapIcon> mapIcons = Reflection.GetField(AssetManager.buildings.GetType(), AssetManager.buildings, "mapIcons") as Dictionary<string, BuildingMapIcon>;
                stats = pTemplate;
                data.templateID = stats.id;
                if (!string.IsNullOrEmpty(stats.kingdom))
                {
                    __instance.CallMethod("setKingdom", new object[] { MapBox.instance.kingdoms.dict_hidden[stats.kingdom], true });
                }
                mapIcon = mapIcons["default"];
                m_transform.name = stats.id;
                return false;
            }
            return true;
        }
        */
        #endregion
        public void ExportVanillaAssets()
        {
            string path = Directory.GetCurrentDirectory() + "\\WorldBox_Data//assets//original//" + "buildings" + "//";
            if (!Directory.Exists(path))
            {
                return;
            }
            foreach (BuildingAsset building in AssetManager.buildings.list)
            {
                string buildingPath = path + "//" + building.id + ".txt";
                string stringToSave = "id: " + building.id;

                stringToSave += "\n" + "affectedByAcid: " + building.affectedByAcid.ToString().ToLower();
                stringToSave += "\n" + "affectedByLava: " + building.affectedByLava.ToString().ToLower();
                stringToSave += "\n" + "autoRemoveRuin: " + building.auto_remove_ruin.ToString().ToLower();
                stringToSave += "\n" + "beehive: " + building.beehive.ToString().ToLower();
                stringToSave += "\n" + "buildRoadTo: " + building.buildRoadTo.ToString().ToLower();
                stringToSave += "\n" + "burnable: " + building.burnable.ToString().ToLower();
                stringToSave += "\n" + "canBeAbandoned: " + building.canBeAbandoned.ToString().ToLower();
                stringToSave += "\n" + "canBeDamagedByTornado: " + building.canBeDamagedByTornado.ToString().ToLower();
                stringToSave += "\n" + "canBeHarvested: " + building.canBeHarvested.ToString().ToLower();
                stringToSave += "\n" + "canBeLivingHouse: " + building.canBeLivingHouse.ToString().ToLower();
                stringToSave += "\n" + "canBeLivingPlant: " + building.canBeLivingPlant.ToString().ToLower();
                stringToSave += "\n" + "canBePlacedOnBlocks: " + building.canBePlacedOnBlocks.ToString().ToLower();
                //stringToSave += "\n" + "canBePlacedOnlyOn: " + building.canBePlacedOnlyOn.ToString().ToLower(); // crashes saves
                stringToSave += "\n" + "canBePlacedOnWater: " + building.canBePlacedOnWater.ToString().ToLower();
                stringToSave += "\n" + "canBeUpgraded: " + building.canBeUpgraded.ToString().ToLower();
                stringToSave += "\n" + "checkForCloseBuilding: " + building.checkForCloseBuilding.ToString().ToLower();
                stringToSave += "\n" + "cityBuilding: " + building.cityBuilding.ToString().ToLower();
                stringToSave += "\n" + "constructionSiteTexture: " + building.construction_site_texture.ToString().ToLower();
                //stringToSave += "\n" + "cost: " + building.cost.ToString().ToLower(); // needs parsed differently
                stringToSave += "\n" + "destroyedSound: " + building.destroyedSound.ToString().ToLower();
                stringToSave += "\n" + "destroyOnWater: " + building.destroyOnWater.ToString().ToLower();
                stringToSave += "\n" + "docks: " + building.docks.ToString().ToLower();
                stringToSave += "\n" + "fauna: " + building.fauna.ToString().ToLower();
                stringToSave += "\n" + "fireDamage: " + building.fireDamage.ToString().ToLower();
                stringToSave += "\n" + "fundament: " + "needs work"; //building.fundament.ToString().ToLower(); 
                stringToSave += "\n" + "hasKingdomColor: " + building.hasKingdomColor.ToString().ToLower();
                stringToSave += "\n" + "housing: " + building.housing.ToString().ToLower();
                stringToSave += "\n" + "iceTower: " + building.iceTower.ToString().ToLower();
                stringToSave += "\n" + "ignoreBuildings: " + building.ignoreBuildings.ToString().ToLower();
                stringToSave += "\n" + "ignoreDemolish: " + building.ignoreDemolish.ToString().ToLower();
                stringToSave += "\n" + "ignoreOtherBuildingsForUpgrade: " + building.ignoreOtherBuildingsForUpgrade.ToString().ToLower();
                stringToSave += "\n" + "isRuin: " + building.isRuin.ToString().ToLower();
                stringToSave += "\n" + "kingdom: " + building.kingdom.ToString().ToLower();
                stringToSave += "\n" + "mapIconId: " + building.mapIconId.ToString().ToLower();
                stringToSave += "\n" + "maxTreesInZone: " + building.maxTreesInZone.ToString().ToLower();
                stringToSave += "\n" + "needsGrass: " + building.needsGrass.ToString().ToLower();
                stringToSave += "\n" + "onlyBuildTiles: " + building.onlyBuildTiles.ToString().ToLower();
                stringToSave += "\n" + "orderInLayer: " + building.orderInLayer.ToString().ToLower();
                stringToSave += "\n" + "priority: " + building.priority.ToString().ToLower();
                stringToSave += "\n" + "race: " + building.race.ToString().ToLower();
                stringToSave += "\n" + "randomFlip: " + building.randomFlip.ToString().ToLower();
                stringToSave += "\n" + "resourcesGiven: " + building.resources_given.ToString().ToLower();
                stringToSave += "\n" + "resourceType: " + building.resourceType.ToString().ToLower();
                stringToSave += "\n" + "resource_id: " + building.resource_id.ToString().ToLower();
                if (building.ruins != null)
                {
                    stringToSave += "\n" + "ruins: " + building.ruins.ToString().ToLower();

                }
                stringToSave += "\n" + "sfx: " + building.sfx.ToString().ToLower();
                stringToSave += "\n" + "shadow: " + building.shadow.ToString().ToLower();
                stringToSave += "\n" + "shadowID: " + building.shadowID.ToString().ToLower();
                stringToSave += "\n" + "smoke: " + building.smoke.ToString().ToLower();
                stringToSave += "\n" + "smokeInterval: " + building.smokeInterval.ToString().ToLower();
                //stringToSave += "\n" + "smokeOffset: " + building.smokeOffset.ToString().ToLower();
                stringToSave += "\n" + "spawnDropID: " + building.spawnDropID.ToString().ToLower();
                stringToSave += "\n" + "spawnPixel: " + building.spawnPixel.ToString().ToLower();
                stringToSave += "\n" + "spawnPixelInterval: " + building.spawnPixelInterval.ToString().ToLower();
                stringToSave += "\n" + "spawnPixelStartZ: " + building.spawnPixelStartZ.ToString().ToLower();
                stringToSave += "\n" + "spawnRats: " + building.spawnRats.ToString().ToLower();
                stringToSave += "\n" + "spawnUnits: " + building.spawnUnits.ToString().ToLower();
                stringToSave += "\n" + "spawnUnitsAsset: " + building.spawnUnits_asset.ToString().ToLower();
                stringToSave += "\n" + "storage: " + building.storage.ToString().ToLower();
                stringToSave += "\n" + "tower: " + building.tower.ToString().ToLower();
                stringToSave += "\n" + "towerProjectile: " + building.tower_projectile.ToString().ToLower();
                stringToSave += "\n" + "towerProjectileOffset: " + building.tower_projectile_offset.ToString().ToLower();
                stringToSave += "\n" + "transformTilesTo: " + building.transformTilesTo.ToString().ToLower();
                stringToSave += "\n" + "treeRandomChance: " + building.treeRandomChance.ToString().ToLower();
                stringToSave += "\n" + "type: " + building.type.ToString().ToLower();
                stringToSave += "\n" + "upgradeLevel: " + building.upgradeLevel.ToString().ToLower();
                stringToSave += "\n" + "upgradeTo: " + building.upgradeTo.ToString().ToLower();
                StreamWriter writer = new StreamWriter(buildingPath, false);
                writer.WriteLine(stringToSave, false);
                writer.Close();
            }

            path = Directory.GetCurrentDirectory() + "\\WorldBox_Data//assets//original//" + "traits" + "//";
            if (!Directory.Exists(path))
            {
                return;
            }
            foreach (ActorTrait trait in AssetManager.traits.list)
            {
                string traitPath = path + "//" + trait.id + ".txt";
                string traitToSave = "id: " + trait.id;
                if (trait.baseStats.accuracy != 0)
                    traitToSave += "\n" + "accuracy: " + trait.baseStats.accuracy.ToString().ToLower();
                if (trait.baseStats.areaOfEffect != 0)
                    traitToSave += "\n" + "areaOfEffect: " + trait.baseStats.areaOfEffect.ToString().ToLower();
                if (trait.baseStats.armor != 0)
                    traitToSave += "\n" + "armor: " + trait.baseStats.armor.ToString().ToLower();
                if (trait.baseStats.army != 0)
                    traitToSave += "\n" + "army: " + trait.baseStats.army.ToString().ToLower();
                if (trait.baseStats.attackSpeed != 0)
                    traitToSave += "\n" + "attackSpeed: " + trait.baseStats.attackSpeed.ToString().ToLower();
                if (trait.baseStats.cities != 0)
                    traitToSave += "\n" + "cities: " + trait.baseStats.cities.ToString().ToLower();
                if (trait.baseStats.crit != 0)
                    traitToSave += "\n" + "crit: " + trait.baseStats.crit.ToString().ToLower();
                if (trait.baseStats.damage != 0)
                    traitToSave += "\n" + "damage: " + trait.baseStats.damage.ToString().ToLower();
                if (trait.baseStats.damage != 0)
                    traitToSave += "\n" + "damageCritMod: " + trait.baseStats.damageCritMod.ToString().ToLower();
                if (trait.baseStats.diplomacy != 0)
                    traitToSave += "\n" + "diplomacy: " + trait.baseStats.diplomacy.ToString().ToLower();
                if (trait.baseStats.dodge != 0)
                    traitToSave += "\n" + "dodge: " + trait.baseStats.dodge.ToString().ToLower();
                if (trait.baseStats.health != 0)
                    traitToSave += "\n" + "health: " + trait.baseStats.health.ToString().ToLower();
                if (trait.baseStats.knockback != 0)
                    traitToSave += "\n" + "knockback: " + trait.baseStats.knockback.ToString().ToLower();
                if (trait.baseStats.knockbackReduction != 0)
                    traitToSave += "\n" + "knockbackReduction: " + trait.baseStats.knockbackReduction.ToString().ToLower();
                if (trait.baseStats.loyalty_mood != 0)
                    traitToSave += "\n" + "loyalty_mood: " + trait.baseStats.loyalty_mood.ToString().ToLower();
                if (trait.baseStats.loyalty_traits != 0)
                    traitToSave += "\n" + "loyalty_traits: " + trait.baseStats.loyalty_traits.ToString().ToLower();
                if (trait.baseStats.opinion != 0)
                    traitToSave += "\n" + "opinion: " + trait.baseStats.opinion.ToString().ToLower();
                if (trait.baseStats.personality_administration != 0)
                    traitToSave += "\n" + "administration: " + trait.baseStats.personality_administration.ToString().ToLower();
                if (trait.baseStats.personality_aggression != 0)
                    traitToSave += "\n" + "aggression: " + trait.baseStats.personality_aggression.ToString().ToLower();
                if (trait.baseStats.personality_diplomatic != 0)
                    traitToSave += "\n" + "diplomatic: " + trait.baseStats.personality_diplomatic.ToString().ToLower();
                if (trait.baseStats.personality_rationality != 0)
                    traitToSave += "\n" + "rationality: " + trait.baseStats.personality_rationality.ToString().ToLower();
                if (trait.baseStats.projectiles != 0)
                    traitToSave += "\n" + "projectiles: " + trait.baseStats.projectiles.ToString().ToLower();
                if (trait.baseStats.range != 0)
                    traitToSave += "\n" + "range: " + trait.baseStats.range.ToString().ToLower();
                if (trait.baseStats.scale != 0)
                    traitToSave += "\n" + "scale: " + trait.baseStats.scale.ToString().ToLower();
                if (trait.baseStats.size != 0)
                    traitToSave += "\n" + "size: " + trait.baseStats.size.ToString().ToLower();
                if (trait.baseStats.speed != 0)
                    traitToSave += "\n" + "speed: " + trait.baseStats.speed.ToString().ToLower();
                if (trait.baseStats.stewardship != 0)
                    traitToSave += "\n" + "stewardship: " + trait.baseStats.stewardship.ToString().ToLower();
                if (trait.baseStats.targets != 0)
                    traitToSave += "\n" + "targets: " + trait.baseStats.targets.ToString().ToLower();
                if (trait.baseStats.warfare != 0)
                    traitToSave += "\n" + "warfare: " + trait.baseStats.warfare.ToString().ToLower();
                if (trait.baseStats.zones != 0)
                    traitToSave += "\n" + "zones: " + trait.baseStats.zones.ToString().ToLower();
                if (trait.birth != 0)
                    traitToSave += "\n" + "birth: " + trait.birth.ToString().ToLower();
                if (trait.icon != null)
                {
                    traitToSave += "\n" + "icon: " + trait.icon.ToString().ToLower();

                }
                if (trait.inherit != 0)
                    traitToSave += "\n" + "inherit: " + trait.inherit.ToString().ToLower();
                if (trait.opposite != null)
                {
                    traitToSave += "\n" + "opposite: " + trait.opposite.ToString().ToLower();

                }
                if (trait.oppositeTraitMod != 0)
                    traitToSave += "\n" + "oppositeTraitMod: " + trait.oppositeTraitMod.ToString().ToLower();
                if (trait.sameTraitMod != 0)
                    traitToSave += "\n" + "sameTraitMod: " + trait.sameTraitMod.ToString().ToLower();
                StreamWriter writer = new StreamWriter(traitPath, false);
                writer.WriteLine(traitToSave);
                writer.Close();
            }

            path = Directory.GetCurrentDirectory() + "\\WorldBox_Data//assets//original//" + "resources" + "//";
            if (!Directory.Exists(path))
            {
                return;
            }
            foreach (ResourceAsset resource in AssetManager.resources.list)
            {
                if (resource.id != "_tool" && resource.id != "_equipment" && resource.id != "melee" && resource.id != "_range")
                {
                    string resourcePath = path + "//" + resource.id + ".txt";
                    string resourceToSave = "id: " + resource.id;
                    if (resource.icon != null)
                    {
                        resourceToSave += "\n" + "icon: " + resource.icon.ToString().ToLower();
                    }
                    if (resource.ingredients != null && resource.ingredients.Length >= 1)
                    {
                        foreach (string ingredient in resource.ingredients)
                        {
                            resourceToSave += "\n" + "ingredient: " + resource.ingredients.ToString().ToLower();

                        }
                    }
                    resourceToSave += "\n" + "ingredientsAmount: " + resource.ingredientsAmount.ToString().ToLower();
                    resourceToSave += "\n" + "maximum: " + resource.maximum.ToString().ToLower();
                    resourceToSave += "\n" + "mineRate: " + resource.mineRate.ToString().ToLower();
                    resourceToSave += "\n" + "restoreHealth: " + resource.restoreHealth.ToString().ToLower();
                    resourceToSave += "\n" + "restoreHunger: " + resource.restoreHunger.ToString().ToLower();
                    //traitToSave += "\n" + "sprite: " + resource.sprite.ToString().ToLower();
                    resourceToSave += "\n" + "supplyBoundGive: " + resource.supplyBoundGive.ToString().ToLower();
                    resourceToSave += "\n" + "supplyBoundTake: " + resource.supplyBoundTake.ToString().ToLower();
                    resourceToSave += "\n" + "supplyGive: " + resource.supplyGive.ToString().ToLower();
                    resourceToSave += "\n" + "tradeBound: " + resource.tradeBound.ToString().ToLower();
                    resourceToSave += "\n" + "tradeCost: " + resource.tradeCost.ToString().ToLower();
                    resourceToSave += "\n" + "tradeGive: " + resource.tradeGive.ToString().ToLower();
                    resourceToSave += "\n" + "type: " + resource.type.ToString().ToLower();
                    StreamWriter writer = new StreamWriter(resourcePath, false);
                    writer.WriteLine(resourceToSave);
                    writer.Close();
                }

            }

            path = Directory.GetCurrentDirectory() + "\\WorldBox_Data//assets//original//" + "items" + "//";
            if (!Directory.Exists(path))
            {
                return;
            }
            foreach (ItemAsset item in AssetManager.items.list)
            {
                string itemPath = path + "//" + item.id + ".txt";
                string itemToSave = "id: " + item.id;
                //itemPath += "\n" + "action: " + item.attackAction.ToString().ToLower();
                //itemPath += "\n" + "attackType: " + item.attackType.ToString().ToLower();
                itemToSave += "\n" + "accuracy: " + item.baseStats.accuracy.ToString().ToLower(); //FUCK NOT AGAIN

                itemToSave += "\n" + "cost_gold: " + item.cost_gold.ToString().ToLower();
                itemToSave += "\n" + "cost_resource_1: " + item.cost_resource_1.ToString().ToLower();
                itemToSave += "\n" + "cost_resource_2: " + item.cost_resource_2.ToString().ToLower();
                if (item.cost_resource_id_1 != null)
                {
                    itemToSave += "\n" + "cost_resource_id_1: " + item.cost_resource_id_1.ToString().ToLower();
                }
                if (item.cost_resource_id_2 != null)
                {
                    itemToSave += "\n" + "cost_resource_id_2: " + item.cost_resource_id_2.ToString().ToLower();
                }
                itemToSave += "\n" + "equipment_value: " + item.equipment_value.ToString().ToLower();
                //itemToSave += "\n" + "level: " + item.level.ToString().ToLower();
                if (item.materials != null && item.materials.Count >= 1)
                {
                    foreach (string material in item.materials)
                    {
                        itemToSave += "\n" + "material: " + material;
                    }
                }
                if (item.pool != null)
                {
                    itemToSave += "\n" + "pool: " + item.pool.ToString().ToLower();

                }
                if (item.prefixes != null && item.prefixes.Count >= 1)
                {
                    foreach (string prefix in item.prefixes)
                    {
                        itemToSave += "\n" + "prefix: " + prefix;
                    }
                }
                if (item.projectile != null)
                {
                    itemToSave += "\n" + "projectile: " + item.projectile.ToString().ToLower();
                }
                itemToSave += "\n" + "quality: " + item.quality.ToString().ToLower();
                itemToSave += "\n" + "rarity: " + item.rarity.ToString().ToLower();
                if (item.slash != null)
                {
                    itemToSave += "\n" + "slash: " + item.slash.ToString().ToLower();
                }
                if (item.suffixes != null && item.suffixes.Count >= 1)
                {
                    foreach (string suffix in item.suffixes)
                    {
                        itemToSave += "\n" + "suffix: " + suffix;
                    }
                }
                StreamWriter writer = new StreamWriter(itemPath, false);
                writer.WriteLine(itemToSave);
                writer.Close();
            }

            path = Directory.GetCurrentDirectory() + "\\WorldBox_Data//assets//original//" + "prefixes" + "//";
            if (!Directory.Exists(path))
            {
                return;
            }
            foreach (ItemAsset item in AssetManager.items_prefix.list)
            {
                string itemPath = path + "//" + item.id + ".txt";
                string itemToSave = "id: " + item.id;
                //itemPath += "\n" + "action: " + item.attackAction.ToString().ToLower();
                //itemPath += "\n" + "attackType: " + item.attackType.ToString().ToLower();
                itemToSave += "\n" + "accuracy: " + item.baseStats.accuracy.ToString().ToLower(); //FUCK NOT AGAIN

                itemToSave += "\n" + "cost_gold: " + item.cost_gold.ToString().ToLower();
                itemToSave += "\n" + "cost_resource_1: " + item.cost_resource_1.ToString().ToLower();
                itemToSave += "\n" + "cost_resource_2: " + item.cost_resource_2.ToString().ToLower();
                if (item.cost_resource_id_1 != null)
                {
                    itemToSave += "\n" + "cost_resource_id_1: " + item.cost_resource_id_1.ToString().ToLower();
                }
                if (item.cost_resource_id_2 != null)
                {
                    itemToSave += "\n" + "cost_resource_id_2: " + item.cost_resource_id_2.ToString().ToLower();
                }
                itemToSave += "\n" + "equipment_value: " + item.equipment_value.ToString().ToLower();
                //itemToSave += "\n" + "level: " + item.level.ToString().ToLower();
                if (item.materials != null && item.materials.Count >= 1)
                {
                    foreach (string material in item.materials)
                    {
                        itemToSave += "\n" + "material: " + material;
                    }
                }
                if (item.pool != null)
                {
                    itemToSave += "\n" + "pool: " + item.pool.ToString().ToLower();

                }
                if (item.prefixes != null && item.prefixes.Count >= 1)
                {
                    foreach (string prefix in item.prefixes)
                    {
                        itemToSave += "\n" + "prefix: " + prefix;
                    }
                }
                if (item.projectile != null)
                {
                    itemToSave += "\n" + "projectile: " + item.projectile.ToString().ToLower();
                }
                itemToSave += "\n" + "quality: " + item.quality.ToString().ToLower();
                itemToSave += "\n" + "rarity: " + item.rarity.ToString().ToLower();
                if (item.slash != null)
                {
                    itemToSave += "\n" + "slash: " + item.slash.ToString().ToLower();
                }
                if (item.suffixes != null && item.suffixes.Count >= 1)
                {
                    foreach (string suffix in item.suffixes)
                    {
                        itemToSave += "\n" + "suffix: " + suffix;
                    }
                }
                StreamWriter writer = new StreamWriter(itemPath, false);
                writer.WriteLine(itemToSave);
                writer.Close();
            }

            path = Directory.GetCurrentDirectory() + "\\WorldBox_Data//assets//original//" + "suffixes" + "//";
            if (!Directory.Exists(path))
            {
                return;
            }
            foreach (ItemAsset item in AssetManager.items_suffix.list)
            {
                string itemPath = path + "//" + item.id + ".txt";
                string itemToSave = "id: " + item.id;
                //itemPath += "\n" + "action: " + item.attackAction.ToString().ToLower();
                //itemPath += "\n" + "attackType: " + item.attackType.ToString().ToLower();
                itemToSave += "\n" + "accuracy: " + item.baseStats.accuracy.ToString().ToLower(); //FUCK NOT AGAIN

                itemToSave += "\n" + "cost_gold: " + item.cost_gold.ToString().ToLower();
                itemToSave += "\n" + "cost_resource_1: " + item.cost_resource_1.ToString().ToLower();
                itemToSave += "\n" + "cost_resource_2: " + item.cost_resource_2.ToString().ToLower();
                if (item.cost_resource_id_1 != null)
                {
                    itemToSave += "\n" + "cost_resource_id_1: " + item.cost_resource_id_1.ToString().ToLower();
                }
                if (item.cost_resource_id_2 != null)
                {
                    itemToSave += "\n" + "cost_resource_id_2: " + item.cost_resource_id_2.ToString().ToLower();
                }
                itemToSave += "\n" + "equipment_value: " + item.equipment_value.ToString().ToLower();
                //itemToSave += "\n" + "level: " + item.level.ToString().ToLower();
                if (item.materials != null && item.materials.Count >= 1)
                {
                    foreach (string material in item.materials)
                    {
                        itemToSave += "\n" + "material: " + material;
                    }
                }
                if (item.pool != null)
                {
                    itemToSave += "\n" + "pool: " + item.pool.ToString().ToLower();

                }
                if (item.prefixes != null && item.prefixes.Count >= 1)
                {
                    foreach (string prefix in item.prefixes)
                    {
                        itemToSave += "\n" + "prefix: " + prefix;
                    }
                }
                if (item.projectile != null)
                {
                    itemToSave += "\n" + "projectile: " + item.projectile.ToString().ToLower();
                }
                itemToSave += "\n" + "quality: " + item.quality.ToString().ToLower();
                itemToSave += "\n" + "rarity: " + item.rarity.ToString().ToLower();
                if (item.slash != null)
                {
                    itemToSave += "\n" + "slash: " + item.slash.ToString().ToLower();
                }
                if (item.suffixes != null && item.suffixes.Count >= 1)
                {
                    foreach (string suffix in item.suffixes)
                    {
                        itemToSave += "\n" + "suffix: " + suffix;
                    }
                }
                StreamWriter writer = new StreamWriter(itemPath, false);
                writer.WriteLine(itemToSave);
                writer.Close();
            }

            path = Directory.GetCurrentDirectory() + "\\WorldBox_Data//assets//original//" + "units" + "//";
            if (!Directory.Exists(path))
            {
                return;
            }
            foreach (ActorStats unit in AssetManager.unitStats.list)
            {
                string itemPath = path + "//" + unit.id + ".txt";
                string itemToSave = "id: " + unit.id;
                //itemToSave += "\n" + "actorSize: " + unit.actorSize
                itemToSave += "\n" + "aggression: " + unit.aggression.ToString().ToLower();
                itemToSave += "\n" + "animal: " + unit.animal.ToString().ToLower();
                itemToSave += "\n" + "animationAlwaysLooped: " + unit.animationAlwaysLooped.ToString().ToLower();
                itemToSave += "\n" + "animation_idle: " + unit.animation_idle.ToString().ToLower();
                itemToSave += "\n" + "animation_swim: " + unit.animation_swim.ToString().ToLower();
                itemToSave += "\n" + "animation_swim_speed: " + unit.animation_swim_speed.ToString().ToLower();
                itemToSave += "\n" + "animation_walk: " + unit.animation_walk.ToString().ToLower();
                itemToSave += "\n" + "animation_walk_speed: " + unit.animation_walk_speed.ToString().ToLower();
                //itemToSave += "\n" + "attack_spells: " + unit.attack_spells.ToString().ToLower();
                itemToSave += "\n" + "baby: " + unit.baby.ToString().ToLower();
                itemToSave += "\n" + "accuracy: " + unit.baseStats.accuracy.ToString().ToLower(); //FUCK NOT AGAIN
                itemToSave += "\n" + "body_separate_part_hands: " + unit.body_separate_part_hands.ToString().ToLower();
                itemToSave += "\n" + "body_separate_part_head: " + unit.body_separate_part_head.ToString().ToLower();
                itemToSave += "\n" + "canAttackBrains: " + unit.canAttackBrains.ToString().ToLower();
                itemToSave += "\n" + "canAttackBuildings: " + unit.canAttackBuildings.ToString().ToLower();
                itemToSave += "\n" + "canBeHurtByPowers: " + unit.canBeHurtByPowers.ToString().ToLower();
                itemToSave += "\n" + "canBeInspected: " + unit.canBeInspected.ToString().ToLower();
                itemToSave += "\n" + "canBeKilledByDrawing: " + unit.canBeKilledByDrawing.ToString().ToLower();
                itemToSave += "\n" + "canBeKilledByStuff: " + unit.canBeKilledByStuff.ToString().ToLower();
                itemToSave += "\n" + "canBeMovedByPowers: " + unit.canBeMovedByPowers.ToString().ToLower();
                itemToSave += "\n" + "canHaveStatusEffect: " + unit.canHaveStatusEffect.ToString().ToLower();
                itemToSave += "\n" + "canLevelUp: " + unit.canLevelUp.ToString().ToLower();
                itemToSave += "\n" + "canTurnIntoZombie: " + unit.canTurnIntoZombie.ToString().ToLower();
                //itemToSave += "\n" + "color: " + unit.color.ToString().ToLower();
                //itemToSave += "\n" + "cost: " + unit.cost.ToString().ToLower();
                itemToSave += "\n" + "countAsUnit: " + unit.countAsUnit.ToString().ToLower();
                itemToSave += "\n" + "currentAmount: " + unit.currentAmount.ToString().ToLower();
                itemToSave += "\n" + "deathAnimationAngle: " + unit.deathAnimationAngle.ToString().ToLower();
                //itemToSave += "\n" + "defaultWeapon: " + unit.defaultWeapon.ToString().ToLower();
                //itemToSave += "\n" + "defaultWeapons: " + unit.defaultWeapons.ToString().ToLower();
                //itemToSave += "\n" + "defaultWeaponsMaterial: " + unit.defaultWeaponsMaterial.ToString().ToLower();
                itemToSave += "\n" + "defaultZ: " + unit.defaultZ.ToString().ToLower();
                itemToSave += "\n" + "dieByLightning: " + unit.dieByLightning.ToString().ToLower();
                itemToSave += "\n" + "dieInLava: " + unit.dieInLava.ToString().ToLower();
                itemToSave += "\n" + "dieInWater: " + unit.dieInWater.ToString().ToLower();
                itemToSave += "\n" + "dieOnBlocks: " + unit.dieOnBlocks.ToString().ToLower();
                itemToSave += "\n" + "dieOnGround: " + unit.dieOnGround.ToString().ToLower();
                itemToSave += "\n" + "diet_berries: " + unit.diet_berries.ToString().ToLower();
                itemToSave += "\n" + "diet_crops: " + unit.diet_crops.ToString().ToLower();
                itemToSave += "\n" + "diet_flowers: " + unit.diet_flowers.ToString().ToLower();
                itemToSave += "\n" + "diet_grass: " + unit.diet_grass.ToString().ToLower();
                itemToSave += "\n" + "diet_meat: " + unit.diet_meat.ToString().ToLower();
                itemToSave += "\n" + "diet_meat_insect: " + unit.diet_meat_insect.ToString().ToLower();
                itemToSave += "\n" + "diet_meat_insect: " + unit.diet_meat_same_race.ToString().ToLower();
                itemToSave += "\n" + "disableJumpAnimation: " + unit.disableJumpAnimation.ToString().ToLower();
                itemToSave += "\n" + "effectDamage: " + unit.effectDamage.ToString().ToLower();
                itemToSave += "\n" + "effect_cast_ground: " + unit.effect_cast_ground.ToString().ToLower();
                itemToSave += "\n" + "effect_cast_top: " + unit.effect_cast_top.ToString().ToLower();
                itemToSave += "\n" + "effect_teleport: " + unit.effect_teleport.ToString().ToLower();
                itemToSave += "\n" + "egg: " + unit.egg.ToString().ToLower();
                itemToSave += "\n" + "eggStatsID: " + unit.eggStatsID.ToString().ToLower();
                //itemToSave += "\n" + "flags: " + unit.flags.ToString().ToLower();
                itemToSave += "\n" + "flipAnimation: " + unit.flipAnimation.ToString().ToLower();
                itemToSave += "\n" + "flying: " + unit.flying.ToString().ToLower();
                itemToSave += "\n" + "growIntoID: " + unit.growIntoID.ToString().ToLower();
                itemToSave += "\n" + "heads: " + unit.heads.ToString().ToLower();
                itemToSave += "\n" + "hideFavoriteIcon: " + unit.hideFavoriteIcon.ToString().ToLower();
                itemToSave += "\n" + "hideOnMinimap: " + unit.hideOnMinimap.ToString().ToLower();
                itemToSave += "\n" + "hit_fx_alternative_offset: " + unit.hit_fx_alternative_offset.ToString().ToLower();
                itemToSave += "\n" + "hovering: " + unit.hovering.ToString().ToLower();
                itemToSave += "\n" + "hovering_max: " + unit.hovering_max.ToString().ToLower();
                itemToSave += "\n" + "hovering_min: " + unit.hovering_min.ToString().ToLower();
                itemToSave += "\n" + "icon: " + unit.icon.ToString().ToLower();
                itemToSave += "\n" + "id: " + unit.id.ToString().ToLower();
                itemToSave += "\n" + "ignoredByInfinityCoin: " + unit.ignoredByInfinityCoin.ToString().ToLower();
                itemToSave += "\n" + "ignoreJobs: " + unit.ignoreJobs.ToString().ToLower();
                itemToSave += "\n" + "ignoreTileSpeedMod: " + unit.ignoreTileSpeedMod.ToString().ToLower();
                itemToSave += "\n" + "immune_to_injures: " + unit.immune_to_injures.ToString().ToLower();

                StreamWriter writer = new StreamWriter(itemPath, false);
                writer.WriteLine(itemToSave);
                writer.Close();
            }
            //needs work^
            //they all need basestats work

            path = Directory.GetCurrentDirectory() + "\\WorldBox_Data//assets//original//" + "tiletype" + "//";
            if (!Directory.Exists(path))
            {
                return;
            }
            foreach (TileType tiletype in TileType.list)
            {
                string itemPath = path + "//" + tiletype.name + ".txt";
                string itemToSave = "name: " + tiletype.name.ToString().ToLower();
                //itemToSave += "\n" + "additional_height: " + tiletype.additional_height.ToString().ToLower();
                itemToSave += "\n" + "id: " + tiletype.id.ToString().ToLower();
                itemToSave += "\n" + "autoGrowPlants: " + tiletype.autoGrowPlants.ToString().ToLower();
                itemToSave += "\n" + "block: " + tiletype.block.ToString().ToLower();
                itemToSave += "\n" + "burnable: " + tiletype.burnable.ToString().ToLower();
                itemToSave += "\n" + "canBeBurned: " + tiletype.canBeBurned.ToString().ToLower();
                itemToSave += "\n" + "canBecomeCreep: " + tiletype.canBecomeCreep.ToString().ToLower();
                itemToSave += "\n" + "canBeFarmField: " + tiletype.canBeFarmField.ToString().ToLower();
                itemToSave += "\n" + "canBeFilled: " + tiletype.canBeFilled.ToString().ToLower();
                itemToSave += "\n" + "canBuildOn: " + tiletype.canBuildOn.ToString().ToLower();
                itemToSave += "\n" + "canGrowFlowers: " + tiletype.canGrowFlowers.ToString().ToLower();
                itemToSave += "\n" + "canGrowGreens: " + tiletype.canGrowGreens.ToString().ToLower();
                itemToSave += "\n" + "cost: " + tiletype.cost.ToString().ToLower();
                itemToSave += "\n" + "creep: " + tiletype.creep.ToString().ToLower();
                if (tiletype.creepID != null)
                    itemToSave += "\n" + "creepID: " + tiletype.creepID.ToString().ToLower();
                itemToSave += "\n" + "damage: " + tiletype.damage.ToString().ToLower();
                itemToSave += "\n" + "damagedWhenWalked: " + tiletype.damagedWhenWalked.ToString().ToLower();
                itemToSave += "\n" + "damageUnits: " + tiletype.damageUnits.ToString().ToLower();
                if (tiletype.decreaseToID != null)
                    itemToSave += "\n" + "decreaseToID: " + tiletype.decreaseToID.ToString().ToLower();
                if (tiletype.drawLayer != null)
                    itemToSave += "\n" + "drawLayer: " + tiletype.drawLayer.ToString().ToLower();
                itemToSave += "\n" + "drawPixel: " + tiletype.drawPixel.ToString().ToLower();
                    //itemToSave += "\n" + "edge_color: " + tiletype.edge_color.ToString().ToLower();
                itemToSave += "\n" + "edge_hills: " + tiletype.edge_hills.ToString().ToLower();
                itemToSave += "\n" + "edge_mountains: " + tiletype.edge_mountains.ToString().ToLower();
                itemToSave += "\n" + "explodable: " + tiletype.explodable.ToString().ToLower();
                itemToSave += "\n" + "explodableByWater: " + tiletype.explodableByWater.ToString().ToLower();
                itemToSave += "\n" + "explodableDelayed: " + tiletype.explodableDelayed.ToString().ToLower();
                itemToSave += "\n" + "explodableTimed: " + tiletype.explodableTimed.ToString().ToLower();
                itemToSave += "\n" + "explodeRange: " + tiletype.explodeRange.ToString().ToLower();
                itemToSave += "\n" + "explodeTimer: " + tiletype.explodeTimer.ToString().ToLower();
                if (tiletype.fillToWater != null)
                    itemToSave += "\n" + "fillToWater: " + tiletype.fillToWater.ToString().ToLower();
                itemToSave += "\n" + "fireChance: " + tiletype.fireChance.ToString().ToLower();
                itemToSave += "\n" + "flowers: " + tiletype.flowers.ToString().ToLower();
                if (tiletype.flowersTypeID != null)
                    itemToSave += "\n" + "flowersTypeID: " + tiletype.flowersTypeID.ToString().ToLower();
                itemToSave += "\n" + "force_edge_variation: " + tiletype.force_edge_variation.ToString().ToLower();
                if (tiletype.force_edge_variation_frame != null)
                    itemToSave += "\n" + "force_edge_variation_frame: " + tiletype.force_edge_variation_frame.ToString().ToLower();
                if (tiletype.freezeToID != null)
                    itemToSave += "\n" + "freezeToID: " + tiletype.freezeToID.ToString().ToLower();
                itemToSave += "\n" + "frozen: " + tiletype.frozen.ToString().ToLower();
                if (tiletype.give_trait != null)
                    itemToSave += "\n" + "give_trait: " + tiletype.give_trait.ToString().ToLower();
                itemToSave += "\n" + "give_trait_chance: " + tiletype.give_trait_chance.ToString().ToLower();
                itemToSave += "\n" + "grass: " + tiletype.grass.ToString().ToLower();
                itemToSave += "\n" + "greyGoo: " + tiletype.greyGoo.ToString().ToLower();
                itemToSave += "\n" + "ground: " + tiletype.ground.ToString().ToLower();
                if (tiletype.growTo != null)
                    itemToSave += "\n" + "growTo: " + tiletype.growTo.ToString().ToLower();
                if (tiletype.growTypes != null)
                    itemToSave += "\n" + "growTypes: " + tiletype.growTypes.ToString().ToLower();
                itemToSave += "\n" + "heightMin: " + tiletype.heightMin.ToString().ToLower();
                itemToSave += "\n" + "ignoreWaterEdgeRendering: " + tiletype.ignoreWaterEdgeRendering.ToString().ToLower();
                if (tiletype.increaseToID != null)
                    itemToSave += "\n" + "increaseToID: " + tiletype.increaseToID.ToString().ToLower();
                itemToSave += "\n" + "landmine: " + tiletype.landmine.ToString().ToLower();
                itemToSave += "\n" + "lava: " + tiletype.lava.ToString().ToLower();
                itemToSave += "\n" + "lavaLevel: " + tiletype.lavaLevel.ToString().ToLower();
                itemToSave += "\n" + "layerType: " + tiletype.layerType.ToString().ToLower();
                itemToSave += "\n" + "life: " + tiletype.life.ToString().ToLower();
                itemToSave += "\n" + "liquid: " + tiletype.liquid.ToString().ToLower();
                if (tiletype.name != null)
                itemToSave += "\n" + "road: " + tiletype.road.ToString().ToLower();
                itemToSave += "\n" + "rocks: " + tiletype.rocks.ToString().ToLower();
                itemToSave += "\n" + "sand: " + tiletype.sand.ToString().ToLower();
                //itemToSave += "\n" + "spawnCreatures: " + tiletype.spawnCreatures.ToString().ToLower();
                itemToSave += "\n" + "strength: " + tiletype.strength.ToString().ToLower();
                itemToSave += "\n" + "terraformAfterFire: " + tiletype.terraformAfterFire.ToString().ToLower();
                if (tiletype.tileName != null)
                    itemToSave += "\n" + "tileName: " + tiletype.tileName.ToString().ToLower();
                itemToSave += "\n" + "trees: " + tiletype.trees.ToString().ToLower();
                itemToSave += "\n" + "variations: " + tiletype.variations.ToString().ToLower();
                itemToSave += "\n" + "walkMod: " + tiletype.walkMod.ToString().ToLower();
                itemToSave += "\n" + "water: " + tiletype.water.ToString().ToLower();
                itemToSave += "\n" + "z: " + tiletype.z.ToString().ToLower();
                
                StreamWriter writer = new StreamWriter(itemPath, false);
                writer.WriteLine(itemToSave);
                writer.Close();
            }


            //AssetManager.unitStats
            //AssetManager.status
            //AssetManager.raceLibrary
            //AssetManager.drops
            //AssetManager.job_actor
            //AssetManager.job_city
            //AssetManager.job_kingdom
        }

        public void ImportCustomAssets()
        {
            string path = "";
            path = Directory.GetCurrentDirectory() + "\\WorldBox_Data//assets//custom//" + "buildings" + "//";
            LoadBuildings(path);
            path = Directory.GetCurrentDirectory() + "\\WorldBox_Data//assets//custom//" + "traits" + "//";
            LoadTraits(path);
            path = Directory.GetCurrentDirectory() + "\\WorldBox_Data//assets//custom//" + "sounds" + "//";
            //_ = LoadSounds(path);
            path = Directory.GetCurrentDirectory() + "\\WorldBox_Data//assets//custom//" + "tiletype" + "//";
            //LoadTiles(path);
        }
        public static Dictionary<string, TileType> addedTileTypes = new Dictionary<string, TileType>();
        public bool hasLoadedTiles;
        public void LoadTiles(string path)
        {
            if (Directory.Exists(path) && !hasLoadedTiles)
            {
                FileInfo[] files = new DirectoryInfo(path).GetFiles("*.txt");
                for (int i = 0; i < files.Count<FileInfo>(); i++)
                {
                    TileType newTileType = new TileType();
                    newTileType.name = files[i].Name;

                    string[] lines = File.ReadAllLines(files[i].FullName);
                    foreach (string readLine in lines)
                    {
                        string[] splitLine = readLine.ToLower().Replace(" ", "").Split(":"[0]);
                        if (splitLine[0] == "name")
                        {
                            newTileType.name = splitLine[1];

                        }
                        if (splitLine[0] == "autoGrowPlants")
                        {
                            newTileType.autoGrowPlants =bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "block")
                        {
                            newTileType.block = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "burnable")
                        {
                            newTileType.burnable = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "canBeBurned")
                        {
                            newTileType.canBeBurned = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "canBecomeCreep")
                        {
                            newTileType.canBecomeCreep = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "canBeFarmField")
                        {
                            newTileType.canBeFarmField = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "canBeFilled")
                        {
                            newTileType.canBeFilled = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "canBuildOn")
                        {
                            newTileType.canBuildOn = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "canGrowFlowers")
                        {
                            newTileType.canGrowFlowers = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "canGrowGreens")
                        {
                            newTileType.canGrowGreens = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "cost")
                        {
                            newTileType.cost = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "creep")
                        {
                            newTileType.creep = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "creepID")
                        {
                            newTileType.creepID = splitLine[1];
                        }
                        if (splitLine[0] == "damage")
                        {
                            newTileType.damage = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "damagedWhenWalked")
                        {
                            newTileType.damagedWhenWalked = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "damageUnits")
                        {
                            newTileType.damageUnits = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "decreaseToID")
                        {
                            newTileType.decreaseToID = splitLine[1];
                        }
                        if (splitLine[0] == "drawLayer")
                        {
                            newTileType.drawLayer = splitLine[1];
                        }
                        if (splitLine[0] == "drawPixel")
                        {
                            newTileType.drawPixel = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "edge_hills")
                        {
                            newTileType.edge_hills = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "edge_mountains")
                        {
                            newTileType.edge_mountains = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "explodable")
                        {
                            newTileType.explodable = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "explodableByWater")
                        {
                            newTileType.explodableByWater = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "explodableDelayed")
                        {
                            newTileType.explodableDelayed = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "explodableTimed")
                        {
                            newTileType.explodableTimed = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "explodeRange")
                        {
                            newTileType.explodeRange = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "explodeTimer")
                        {
                            newTileType.explodeTimer = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "fillToWater")
                        {
                            newTileType.fillToWater = splitLine[1];
                        }
                        if (splitLine[0] == "fireChance")
                        {
                            newTileType.fireChance = float.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "flowers")
                        {
                            newTileType.flowers = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "flowersTypeID")
                        {
                            newTileType.flowersTypeID = splitLine[1];
                        }
                        if (splitLine[0] == "force_edge_variation")
                        {
                            newTileType.force_edge_variation = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "force_edge_variation_frame")
                        {
                            newTileType.force_edge_variation_frame = splitLine[1];
                        }
                        if (splitLine[0] == "freezeToID")
                        {
                            newTileType.freezeToID = splitLine[1];
                        }
                        if (splitLine[0] == "frozen")
                        {
                            newTileType.frozen = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "give_trait")
                        {
                            newTileType.give_trait = splitLine[1];
                        }
                        if (splitLine[0] == "grass")
                        {
                            newTileType.grass = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "greyGoo")
                        {
                            newTileType.greyGoo = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "ground")
                        {
                            newTileType.ground = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "growTo")
                        {
                            newTileType.growTo = splitLine[1];
                        }
                        if (splitLine[0] == "growTypes")
                        {
                            newTileType.growTypes = splitLine[1];
                        }
                        if (splitLine[0] == "heightMin")
                        {
                            newTileType.heightMin = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "ignoreWaterEdgeRendering")
                        {
                            newTileType.ignoreWaterEdgeRendering = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "increaseToID")
                        {
                            newTileType.increaseToID = splitLine[1];
                        }
                        if (splitLine[0] == "landmine")
                        {
                            newTileType.landmine = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "lava")
                        {
                            newTileType.lava = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "lavaLevel")
                        {
                            newTileType.lavaLevel = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "layerType")
                        {
                            TileLayerType.TryParse(splitLine[1], out TileLayerType type);
                            newTileType.layerType = type;
                        }
                        if (splitLine[0] == "life")
                        {
                            newTileType.life = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "liquid")
                        {
                            newTileType.liquid = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "road")
                        {
                            newTileType.road = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "rocks")
                        {
                            newTileType.rocks = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "sand")
                        {
                            newTileType.sand = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "strength")
                        {
                            newTileType.strength = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "terraformAfterFire")
                        {
                            newTileType.terraformAfterFire = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "tileName")
                        {
                            newTileType.tileName = splitLine[1];
                        }
                        if (splitLine[0] == "trees")
                        {
                            newTileType.trees = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "variations")
                        {
                            newTileType.variations = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "walkMod")
                        {
                            newTileType.walkMod = float.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "water")
                        {
                            newTileType.water = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "z")
                        {
                            newTileType.z = int.Parse(splitLine[1]);
                        }
                    }
                    Debug.Log("1");
                    newTileType.id = TileType.dict.Count;
                    Debug.Log("12");

                    addedTileTypes.Add(newTileType.name, newTileType);
                    Debug.Log("13");

                    TileType.dict.Add(newTileType.name, newTileType);
                    Debug.Log("14");

                    if (TextureLoader.TextureLoader_Main.customTextures != null)
                    {
                        Debug.Log("15");
                        List<string> variationsForTile = TextureLoader.TextureLoader_Main.variationsOfCustomTexture(newTileType.name);
                        if (variationsForTile.Count >= 1)
                        {
                            Debug.Log("16");

                            TileLib tileLib = new TileLib();
                            Debug.Log("17");

                            List<Tile> tiles = Reflection.GetField(tileLib.GetType(), tileLib, "tiles") as List<Tile>;
                            Debug.Log("18");

                            Dictionary<string, Tile> tiles_dict = Reflection.GetField(tileLib.GetType(), tileLib, "tiles_dict") as Dictionary<string, Tile>;
                            Debug.Log("19");

                            //addvariation replacement
                            Tile tile = (Tile)Resources.Load("tilemap/" + "close_ocean", typeof(Tile));
                            Debug.Log("20: " + newTileType.name + ", total custom textures:" + TextureLoader.TextureLoader_Main.customTextures.Count);
                            //Sprite sprite = TextureLoader.TextureLoader_Main.customTextures[variationsForTile.GetRandom()];
                            //tile.sprite = sprite;
                            Debug.Log("21");

                            tiles.Add(tile);
                            Debug.Log("202");

                            tiles_dict.Add(newTileType.name, tile);
                            Debug.Log("203");

                            // add to d_tiles, usually happens during worldtilemap.create
                            Dictionary<TileType, TileLib> d_tiles = Reflection.GetField(MapBox.instance.tilemap.GetType(), MapBox.instance.tilemap, "d_tiles") as Dictionary<TileType, TileLib>;
                            Debug.Log("204");

                            d_tiles.Add(newTileType, tileLib);
                        }
                        else
                        {
                            Debug.Log("No variation of: " + newTileType.name + " detected");
                        }
                    }
                    else
                    {
                        Debug.Log("Trying to use custom texture before theyre created");
                    }
                  
                }
            }
        }

        public static void addVariation_Prefix(string pSpriteName, TileLib __instance)
        {
        
        }


        public bool hasLoadedSounds;
        async Task LoadSounds(string path)
        {
            if (Directory.Exists(path) && !hasLoadedSounds)
            {
                FileInfo[] files = new DirectoryInfo(path).GetFiles("*.wav");

                for (int i = 0; i < files.Count<FileInfo>(); i++)
                {
                    AudioClip CurrentClip = await LoadClip(files[i].FullName);
                    if (CurrentClip != null)
                    {
                        Debug.Log("clip loaded: " + files[i].Name); // fully loaded, should be possible to play
                    }
                    string endName = files[i].Name.Replace(".wav", "");
                    customSounds.Add(endName, CurrentClip);
                    SoundController sc = new SoundController();
                    sc.soundEnabled = true;
                    sc.clips = new List<AudioClip>();
                    sc.timeoutInterval = 10f;
                    sc.copies = 1;
                    sc.clips.Add(CurrentClip);
                    customSoundControllers.Add(endName, sc);
                }
            }
        }

        public static void PlaySound(string pName, bool pRestart = true, float pX = -1f, float pY = -1f)
        {
            if (customSoundControllers.ContainsKey(pName))
            {
                List<SoundController> list = customSoundControllers.Values.ToList();
                float timeout = (float)Reflection.GetField(list[0].GetType(), list[0], "timeout");


                bool isAmbient = list[0].ambientSound && PlayerConfig.dict["sound_ambient"].boolVal;
                if (isAmbient == false)
                {
                    foreach (SoundController soundController in list)
                    {
                        AudioSource s = Reflection.GetField(soundController.GetType(), soundController, "s") as AudioSource;
                        bool flag3 = !s.isPlaying;
                        if (flag3)
                        {
                            soundController.CallMethod("play", new object[] { pX, pY });
                            return;
                        }
                        int curCopies = (int)Reflection.GetField(list[0].GetType(), list[0], "curCopies");
                        if (curCopies < list[0].copies)
                        {
                            curCopies++;
                            SoundController soundController2 = UnityEngine.Object.Instantiate<SoundController>(list[0]);
                            listAll.Add(soundController2);
                            list.Add(soundController2);
                            soundController2.transform.parent = list[0].transform.parent;
                            soundController2.CallMethod("play", new object[] { pX, pY });
                        }
                    }
                    // more copy code but idk if needed

                }
            }
            else
            {
                Debug.Log("PlaySound: file name not found in custom sound list");
            }
        }

        public static List<SoundController> listAll = new List<SoundController>();

        public void SoundUpdate()
        {
            float deltaTime = Time.deltaTime;
            foreach (SoundController soundController in listAll)
            {
                soundController.CallMethod("update", new object[] { deltaTime });
                AudioSource s = Reflection.GetField(soundController.GetType(), soundController, "s") as AudioSource;

                bool flag = s != null && !s.isPlaying;
                if (flag)
                {
                    soundController.gameObject.SetActive(false);
                }
            }
        }
        public static Dictionary<string, AudioClip> customSounds = new Dictionary<string, AudioClip>();
        public static Dictionary<string, SoundController> customSoundControllers = new Dictionary<string, SoundController>();

        

        async Task<AudioClip> LoadClip(string path)
        {
            AudioClip clip = null;
            using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV))
            {
                uwr.SendWebRequest();
                // wrap tasks in try/catch, otherwise it'll fail silently
                try
                {
                    while (!uwr.isDone)
                        await Task.Delay(5);

                    if (uwr.isNetworkError || uwr.isHttpError)
                        Debug.Log($"{uwr.error}");
                    else
                    {
                        clip = DownloadHandlerAudioClip.GetContent(uwr);
                    }
                }
                catch (Exception err)
                {
                    Debug.Log($"{err.Message}, {err.StackTrace}");
                }
            }

            return clip;
        }

        public void Start()
        {
        }

        public static AudioSource audioContainer;

        public static IEnumerator LoadAudio(string path)
        {
            WWW URL = new WWW(path);
            yield return URL;

            audioContainer.clip = URL.GetAudioClip(false, true);
        }

        public static string StringWithFirstUpper(string targetstring)
        {
            return char.ToUpper(targetstring[0]) + targetstring.Substring(1);
        }

        private void LoadBuildings(string path)
        {
            if (Directory.Exists(path) && !hasLoadedBuildings)
            {
                FileInfo[] files = new DirectoryInfo(path).GetFiles("*.txt");
                for (int i = 0; i < files.Count<FileInfo>(); i++)
                {
                    CityBuildOrderElement element = new CityBuildOrderElement();
                    BuildingAsset test2 = new BuildingAsset();
                    test2.ruins = "tree_dead";
                    test2.mapIconId = "tree";
                    test2.id = files[i].Name; // .Replace(".txt", "")
                    if (AssetManager.buildings.dict.ContainsKey(test2.id) == true)
                    {
                        test2 = AssetManager.buildings.get(test2.id);
                    }
                    int top = 0;
                    int bottom = 0;
                    int left = 0;
                    int right = 0;
                    int wood = 0;
                    int stone = 0;
                    int metals = 0;
                    int gold = 0;
                    string[] lines = File.ReadAllLines(files[i].FullName);
                    foreach (string readLine in lines)
                    {

                        string[] splitLine = readLine.ToLower().Replace(" ", "").Split(":"[0]);
                        if (splitLine[0] == "id")
                        {
                            test2.id = splitLine[1];
                        }
                        if (splitLine[0] == "mapiconid")
                        {
                            test2.mapIconId = splitLine[1];
                        }
                        if (splitLine[0] == "ruins")
                        {
                            test2.ruins = splitLine[1]; // type of building it becomes when broken
                        }
                        if (splitLine[0] == "docks") // whether this building is a dock
                        {
                            test2.docks = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "housing") // how much housing this building provides to the city
                        {
                            test2.housing = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "canbeupgraded")
                        {
                            test2.canBeUpgraded = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "upgradelevel")
                        {
                            test2.upgradeLevel = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "upgradeto")
                        {
                            test2.upgradeTo = splitLine[1];
                        }
                        if (splitLine[0] == "smoke")
                        {
                            test2.smoke = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "race")
                        {
                            test2.race = splitLine[1];
                        }
                        if (splitLine[0] == "shadow")
                        {
                            test2.shadow = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "sfx")
                        {
                            test2.sfx = splitLine[1];
                        }
                        if (splitLine[0] == "spawnunits")
                        {
                            test2.spawnUnits = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "spawnunitsasset")
                        {
                            test2.spawnUnits_asset = splitLine[1];
                        }
                        if (splitLine[0] == "tower")
                        {
                            test2.tower = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "towerprojectile")
                        {
                            test2.tower_projectile = splitLine[1];
                        }
                        if (splitLine[0] == "icetower")
                        {
                            test2.iceTower = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "ignoredemolish")
                        {
                            test2.ignoreDemolish = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "ignoredbycities")
                        {
                            test2.ignoredByCities = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "ignorebuildings")
                        {
                            test2.ignoreBuildings = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "canbeplacedonwater")
                        {
                            test2.canBePlacedOnWater = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "canbeplacedonblocks")
                        {
                            test2.canBePlacedOnBlocks = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "canbelivingplant")
                        {
                            test2.canBeLivingPlant = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "canbelivinghouse")
                        {
                            test2.canBeLivingHouse = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "canbedamagedbytornado")
                        {
                            test2.canBeDamagedByTornado = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "canbeabandoned")
                        {
                            test2.canBeAbandoned = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "burnable")
                        {
                            test2.burnable = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "buildroadto")
                        {
                            test2.buildRoadTo = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "beehive")
                        {
                            test2.beehive = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "autoremoveruin")
                        {
                            test2.auto_remove_ruin = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "affectedbylava")
                        {
                            test2.affectedByLava = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "affectedbyacid")
                        {
                            test2.affectedByAcid = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "smokeinterval")
                        {
                            test2.smokeInterval = float.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "spawnpixel")
                        {
                            test2.spawnPixel = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "spawndropid")
                        {
                            test2.spawnDropID = splitLine[1];
                        }
                        if (splitLine[0] == "spawnpixelinterval")
                        {
                            test2.spawnPixelInterval = float.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "spawnpixelstartz")
                        {
                            test2.spawnPixelStartZ = float.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "citybuilding")
                        {
                            test2.cityBuilding = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "destroyedsound")
                        {
                            test2.destroyedSound = splitLine[1];
                        }
                        if (splitLine[0] == "destroyonwater")
                        {
                            test2.destroyOnWater = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "fauna")
                        {
                            test2.fauna = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "treerandomchance")
                        {
                            test2.treeRandomChance = float.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "constructionsitetexture")
                        {
                            test2.construction_site_texture = splitLine[1];
                        }
                        if (splitLine[0] == "resourceid")
                        {
                            test2.resource_id = splitLine[1];
                        }
                        if (splitLine[0] == "resourceType")
                        {
                            // needs to be a bit extensive?
                            //test2.resourceType = ResourceType.int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "resourcesgiven")
                        {
                            test2.resources_given = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "fundamenttop")
                        {
                            top = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "fundamentbottom")
                        {
                            bottom = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "fundamentleft")
                        {
                            left = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "fundamentright")
                        {
                            right = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "wood")
                        {
                            wood = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "stone")
                        {
                            stone = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "metals")
                        {
                            metals = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "gold")
                        {
                            gold = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "usedbyraces")
                        {
                            element.usedByRaces = splitLine[1];
                        }
                        if (splitLine[0] == "usedbyracescheck")
                        {
                            element.usedByRacesCheck = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "limittype")
                        {
                            element.limitType = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "limitid")
                        {
                            element.limitID = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "requiredbuildings")
                        {
                            element.requiredBuildings = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "requiredpop")
                        {
                            element.requiredPop = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "priority")
                        {
                            element.priority = int.Parse(splitLine[1]);
                            test2.priority = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "checkfullvillage")
                        {
                            element.checkFullVillage = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "waitforresources")
                        {
                            element.waitForResources = bool.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "transformtilesto")
                        {
                            // test2.transformTilesTo = splitLine[1]; // not broken im just lazy
                        }
                        if (splitLine[0] == "height")
                        {
                            if (!_3D_Main.buildingCustomHeight.ContainsKey(test2.id))
                            {
                                _3D_Main.buildingCustomHeight.Add(test2.id, int.Parse(splitLine[1]));
                            }
                        }
                        if (splitLine[0] == "thickness")
                        {
                            if (!_3D_Main.buildingCustomThickness.ContainsKey(test2.id))
                            {
                                _3D_Main.buildingCustomThickness.Add(test2.id, int.Parse(splitLine[1]));
                            }
                        }
                        if (splitLine[0] == "rotate" && splitLine[1].Contains("true"))
                        {
                            if (!_3D_Main.buildingCustomAngle.ContainsKey(test2.id))
                            {
                                _3D_Main.buildingCustomAngle.Add(test2.id, 90);
                            }
                        }
                        if (splitLine[0] == "angle")
                        {
                            if (!_3D_Main.buildingCustomAngle.ContainsKey(test2.id))
                            {
                                _3D_Main.buildingCustomAngle.Add(test2.id, int.Parse(splitLine[1]));
                            }
                        }
                    }
                    test2.fundament = new BuildingFundament(left, right, top, bottom);
                    test2.cost = new ConstructionCost(wood, stone, metals, gold);

                    addedBuildings.Add(test2.id);
                    AssetManager.buildings.add(test2);

                    element.buildingID = test2.id;
                    CityBuildOrder.add(element);
                }
            }
        }

        private void LoadTraits(string path)
        {
            if (Directory.Exists(path) && !hasLoadedTraits)
            {

                FileInfo[] files = new DirectoryInfo(path).GetFiles("*.txt");
                //
                for (int i = 0; i < files.Count<FileInfo>(); i++)
                {
                    ActorTrait newTrait = new ActorTrait();
                    BaseStats newBaseStats = new BaseStats();
                    newTrait.id = files[i].Name.Replace(" ",""); // .Replace(".txt", "")
                    if (AssetManager.traits.dict.ContainsKey(newTrait.id) == true)
                    {
                        newTrait = AssetManager.traits.get(newTrait.id);
                        newBaseStats = newTrait.baseStats;
                    }
                    string[] lines = File.ReadAllLines(files[i].FullName);
                    foreach (string readLine in lines)
                    {
                        string[] splitLine = readLine.Split(":"[0]); // removed .Replace(" ", "")
                        if (splitLine[0] == "id")
                        {
                            newTrait.id = splitLine[1].Replace(" ", "");
                        }
                        if (splitLine[0] == "description")
                        {
                            string language = Reflection.GetField(LocalizedTextManager.instance.GetType(), LocalizedTextManager.instance, "language") as string;
                            Dictionary<string, string> localizedText = Reflection.GetField(LocalizedTextManager.instance.GetType(), LocalizedTextManager.instance, "localizedText") as Dictionary<string, string>;
                            if (!localizedText.ContainsKey("trait_" + newTrait.id))
                            {
                                localizedText.Add("trait_" + newTrait.id, StringWithFirstUpper(newTrait.id)); // need to caps first letters
                                localizedText.Add("trait_" + newTrait.id + "_info", splitLine[1]);
                            }
                        }
                        splitLine[0] = splitLine[0].ToLower();


                        if (splitLine[0] == "birth")
                        {
                            newTrait.birth = float.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "icon")
                        {
                            newTrait.icon = splitLine[1];
                        }
                        if (splitLine[0] == "inherit")
                        {
                            newTrait.inherit = float.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "opposite")
                        {
                            newTrait.opposite = splitLine[1];
                        }
                        if (splitLine[0] == "oppositetraitmod")
                        {
                            newTrait.oppositeTraitMod = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "sametraitmod")
                        {
                            newTrait.sameTraitMod = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "accuracy")
                        {
                            newBaseStats.accuracy = float.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "areaofeffect")
                        {
                            newBaseStats.areaOfEffect = float.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "armor")
                        {
                            newBaseStats.armor = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "army")
                        {
                            newBaseStats.army = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "attackspeed")
                        {
                            newBaseStats.attackSpeed = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "cities")
                        {
                            newBaseStats.cities = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "crit")
                        {
                            newBaseStats.crit = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "damage")
                        {
                            newBaseStats.damage = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "damagecritmod")
                        {
                            newBaseStats.damageCritMod = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "diplomacy")
                        {
                            newBaseStats.diplomacy = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "dodge")
                        {
                            newBaseStats.dodge = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "health")
                        {
                            newBaseStats.health = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "knockback")
                        {
                            newBaseStats.knockback = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "knockbackreduction")
                        {
                            newBaseStats.knockbackReduction = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "loyaltymood")
                        {
                            newBaseStats.loyalty_mood = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "loyaltytraits")
                        {
                            newBaseStats.loyalty_traits = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "opinion")
                        {
                            newBaseStats.opinion = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "administration")
                        {
                            newBaseStats.personality_administration = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "aggression")
                        {
                            newBaseStats.personality_aggression = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "diplomatic")
                        {
                            newBaseStats.personality_diplomatic = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "rationality")
                        {
                            newBaseStats.personality_rationality = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "projectiles")
                        {
                            newBaseStats.projectiles = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "range")
                        {
                            newBaseStats.range = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "scale")
                        {
                            newBaseStats.scale = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "size")
                        {
                            newBaseStats.size = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "speed")
                        {
                            newBaseStats.speed = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "stewardship")
                        {
                            newBaseStats.stewardship = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "targets")
                        {
                            newBaseStats.targets = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "warfare")
                        {
                            newBaseStats.warfare = int.Parse(splitLine[1]);
                        }
                        if (splitLine[0] == "zones")
                        {
                            newBaseStats.zones = int.Parse(splitLine[1]);
                        }
                        newTrait.baseStats = newBaseStats;
                        newTrait.type = TraitType.Other;
                    }
                    addedTraits.Add(newTrait.id);
                    addedActorTraits.Add(newTrait);
                    AssetManager.traits.add(newTrait);

                }

            }
        }


        public bool hasLoadedUnits;
        public bool hasLoadedTraits;
        public static List<string> addedTraits = new List<string>();
        public static List<ActorTrait> addedActorTraits = new List<ActorTrait>();

        #region units region
        /*
        public void test2()
        {
            string unitPath = Directory.GetCurrentDirectory() + "\\WorldBox_Data//" + "units" + "//custom//";
            if (!Directory.Exists(unitPath))
            {
                return;
            }
            FileInfo[] unitFiles = new DirectoryInfo(unitPath).GetFiles("*.txt");
            for (int i = 0; i < unitFiles.Count<FileInfo>(); i++)
            {
                ActorStats newActorStats = new ActorStats();
                newActorStats.id = unitFiles[i].Name.Replace(".txt", "");
                newActorStats.prefab = "p_unit"; //
                                                                          //
                string[] lines = File.ReadAllLines(unitFiles[i].FullName);
                foreach (string readLine in lines)
                {
                    string[] splitLine = readLine.ToLower().Replace(" ", "").Split(":"[0]);
                    if (splitLine[0] == "id")
                    {
                        newActorStats.id = splitLine[1];
                    }
                    if (splitLine[0] == "race")
                    {
                        newActorStats.race = splitLine[1];
                    }
                    if (splitLine[0] == "texturepath")
                    {
                        newActorStats.texture_path = splitLine[1];
                    }
                    if (splitLine[0] == "textureheads")
                    {
                        newActorStats.texture_heads = splitLine[1];
                    }
                    if (splitLine[0] == "skeletonid")
                    {
                        newActorStats.skeletonID = splitLine[1];
                    }
                    if (splitLine[0] == "maxage")
                    {
                        newActorStats.maxAge = int.Parse(splitLine[1]);
                    }
                    if (splitLine[0] == "icon")
                    {
                        newActorStats.icon = splitLine[1];
                    }
                    if (splitLine[0] == "maxage")
                    {
                        newActorStats.maxAge = int.Parse(splitLine[1]);
                    }
                    if (splitLine[0] == "maxrandomamount")
                    {
                        newActorStats.maxRandomAmount = int.Parse(splitLine[1]);
                    }
                    if (splitLine[0] == "nametemplate")
                    {
                        newActorStats.nameTemplate = splitLine[1];
                    }
                    if (splitLine[0] == "zombieid")
                    {
                        newActorStats.zombieID = splitLine[1];
                    }
                    if (splitLine[0] == "shadow")
                    {
                        newActorStats.shadow = bool.Parse(splitLine[1]);
                    }
                    if (splitLine[0] == "unit")
                    {
                        newActorStats.unit = bool.Parse(splitLine[1]);
                    }
                    if (splitLine[0] == "prefab")
                    {
                        newActorStats.prefab = splitLine[1];
                    }
                }
                addedUnits.Add(newActorStats.id);
                AssetManager.unitStats.add(newActorStats);
            }
        }
        */
        #endregion units
        public static List<string> addedUnits = new List<string>();

        public static Sprite LoadSprite(string path, float offsetx, float offsety)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            if (File.Exists(path))
            {
                byte[] data = File.ReadAllBytes(path);
                Texture2D texture2D = new Texture2D(1, 1);
                texture2D.anisoLevel = 0;
                texture2D.LoadImage(data);
                texture2D.filterMode = FilterMode.Point;
                // TextureScale.Point(texture2D, resizeX, resizeY);
                Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), new Vector2(offsetx, offsety), 1f);
                return sprite;
            }
            return null;
        }

        public static bool setSpriteMain_Prefix(bool pTween, Building __instance)
        {
            BuildingAsset stats = Reflection.GetField(__instance.GetType(), __instance, "stats") as BuildingAsset;
            if (addedBuildings.Contains(stats.id))
            {
                return false;
            }
            return true;
        }

        public void Update()
        {
            //SoundUpdate(); old sound test
            if (!hasLoadedBuildings)
            {
                if (AssetManager.tester_tasks != null)
                {
                   // ExportVanillaAssets(); only for me for now, needs fixed after 10.0 update
                    ImportCustomAssets();
                    hasLoadedBuildings = true;
                    hasLoadedTraits = true;
                }

            }
            /* buildings were relatively easy...
            if (!hasLoadedUnits)
            {
                if (AssetManager.unitStats != null)
                {

                    test2();
                    hasLoadedUnits = true;
                }
            }
           
            if (Input.GetKeyDown(KeyCode.J) && MapBox.instance.getMouseTilePos() != null)
            {
                MapBox.instance.createNewUnit(addedUnits.GetRandom(), MapBox.instance.getMouseTilePos(), null, 0f, null);
            }
             */
        }
        public static List<string> addedBuildings = new List<string>();
        public bool hasLoadedBuildings;

    }
}

