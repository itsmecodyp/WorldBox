using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace SimpleAdditions {

	class aTraits {
		public static void AddTraits()
		{
			ActorTrait testTrait = new ActorTrait() {
				id = "testFireworks",
				icon = "iconVermin",
				group = TraitGroup.Other,
				type = TraitType.Positive,
				baseStats = new BaseStats() { health = 100 }
			 };
			testTrait.action_special_effect = (WorldAction)Delegate.Combine(testTrait.action_special_effect, new WorldAction(traitFireworks));
			AssetManager.traits.add(testTrait);
			addTraitToLocalizedLibrary(testTrait.id, "Spawns fireworks every now and then");

			ActorTrait statAbsorb = new ActorTrait() {
				id = "statAbsorb", // maybe size too
				icon = "iconVermin",
				group = TraitGroup.Other,
				type = TraitType.Positive
			};

			ActorTrait wolfTrainer = new ActorTrait() {
				id = "wolfTrainer",
				icon = "iconVermin",
				group = TraitGroup.Other,
				type = TraitType.Positive
			};

			ActorTrait wizard = new ActorTrait() {
				id = "wizard", // we have wizards, but mine are cooler
				icon = "iconVermin",
				group = TraitGroup.Other,
				type = TraitType.Positive
			};

		}

		public static bool traitFireworks(BaseSimObject pTarget, WorldTile pTile = null)
		{
			Actor a = Reflection.GetField(pTarget.GetType(), pTarget, "a") as Actor;

			if(Toolbox.randomChance(0.25f)) {
				MapBox.instance.stackEffects.CallMethod("spawnFireworks", new object[] { a.currentTile, 0.5f });
			}

			return true;
		}

		public static void addTraitToLocalizedLibrary(string id, string description)
		{
			string language = Reflection.GetField(LocalizedTextManager.instance.GetType(), LocalizedTextManager.instance, "language") as string;
			Dictionary<string, string> localizedText = Reflection.GetField(LocalizedTextManager.instance.GetType(), LocalizedTextManager.instance, "localizedText") as Dictionary<string, string>;
			if(language == "en") {
				localizedText.Add("trait_" + id, id);
				localizedText.Add("trait_" + id + "_info", description);
			}
		}

	}

	[HarmonyPatch(typeof(Actor))]
	class Actor_takeItems {
		[HarmonyPatch("takeItems", MethodType.Normal)]
		public static void Postfix(Actor pActor, bool pIgnoreRangeWeapons, Actor __instance)
		{
			if(__instance.haveTrait("statAbsorb")) { 

			}
		}
	}
}
