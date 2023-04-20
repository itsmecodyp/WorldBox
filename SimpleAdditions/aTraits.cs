using System.Collections.Generic;
using HarmonyLib;

namespace SimpleAdditions {

	class aTraits {
		public static void AddTraits()
		{
			/*
			ActorTrait testTrait = new ActorTrait() {
				id = "testFireworks",
				path_icon = "ui/Icons/iconVoicesInMyHead",
				type = TraitType.Positive,
			};
			testTrait.action_special_effect = (WorldAction)Delegate.Combine(testTrait.action_special_effect, new WorldAction(traitFireworks));
			AssetManager.traits.add(testTrait);
			addTraitToLocalizedLibrary(testTrait.id, "Spawns fireworks every now and then");
			*/
		}


		public static bool traitFireworks(BaseSimObject pTarget, WorldTile pTile = null)
		{
			Actor a = Reflection.GetField(pTarget.GetType(), pTarget, "a") as Actor;

			if(Toolbox.randomChance(0.25f)) {
				MapBox.instance.stackEffects.CallMethod("spawnFireworks", a.currentTile, 0.5f);
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
			if(__instance.hasTrait("statAbsorb")) { 

			}
		}
	}
}
