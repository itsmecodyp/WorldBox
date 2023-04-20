using System.Collections.Generic;
using UnityEngine;
using Resources = SimpleAdditions.Properties.Resources;

namespace SimpleAdditions {
	class aBuildings {
		public static void AddBuildings()
		{
			Sprite testSprite = LoadSprite(Resources.bluesword3, 16, 16, 0f, 0f);

			BuildingAnimationDataNew newAnimationData = new BuildingAnimationDataNew();
			newAnimationData.animated = false;
			newAnimationData.list_main = new List<Sprite>();
			newAnimationData.list_ruins = new List<Sprite>();
			//newAnimationData.list_shadows = new List<Sprite>();
			newAnimationData.list_special = new List<Sprite>(); // something to do with fruit

			newAnimationData.list_main.Add(testSprite);


			BuildingSprites newBuildingSprites = new BuildingSprites();
			newBuildingSprites.construction = testSprite;
			newBuildingSprites.mapIcon = null; // new BuildingMapIcon(testSprite);
			newBuildingSprites.animationData = new List<BuildingAnimationDataNew>();
			newBuildingSprites.animationData.Add(newAnimationData);

			BuildingAsset testBuilding = new BuildingAsset
			{
				id = "testBuilding",
				sprites = newBuildingSprites,
				fundament = new BuildingFundament(1, 1, 1, 0),
				//fauna = false,
			};
			AssetManager.buildings.add(testBuilding);
		}

		public static Sprite LoadSprite(byte[] bytes, int resizeX = 0, int resizeY = 0, float offsetx = 0f, float offsety = 0.5f)
		{
			byte[] data = bytes;
			Texture2D texture2D = new Texture2D(1, 1);
			texture2D.anisoLevel = 0;
			texture2D.LoadImage(data);
			texture2D.filterMode = FilterMode.Point;
			if(resizeX != 0) {
				TextureScale.Point(texture2D, resizeX, resizeY);
			}
			return Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(offsetx, offsety), 1f);
		}

	}
}
