
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Altar.Localization;
using ArchipelagoRandomizer.EventHandlers;
using BepInEx;
using UnityEngine;
using Zyklus.Loot;

namespace ArchipelagoRandomizer.Items;

public class APItemsUtils
{
	private static Sprite aPSprite_;
	private static Sprite aPUISprite_;
	public static long pBaseItemsId = 85000; //CHANGE: before release
	public static long pBaseLocationsId = 87000; //CHANGE: before release

	public static void SetInGameSprite<T>(T relic, string fieldName) where T : class
	{
		if (aPSprite_ == null)
		{
			var texture = new Texture2D(168, 128);
			ImageConversion.LoadImage(texture, File.ReadAllBytes(Paths.PluginPath + @"\ArchipelagoRandomizer\Assets\Binoculars.png"));
			aPSprite_ = Sprite.Create(texture, new Rect(0, 0, 168, 128), new Vector2(0.5f, 0.5f));
		}

		relic.SetFieldValue(fieldName, aPSprite_);
	}

	public static void SetUISprite<T>(T relic, string fieldName) where T : class
	{
		if (aPUISprite_ == null)
		{
			var texture = new Texture2D(73, 79);
			ImageConversion.LoadImage(texture, File.ReadAllBytes(Paths.PluginPath + @"\ArchipelagoRandomizer\Assets\BinocularsUI.png"));
			aPUISprite_ = Sprite.Create(texture, new Rect(0, 0, 73, 79), new Vector2(0.5f, 0.5f), 1, 0, SpriteMeshType.FullRect, new Vector4(0, 0, 0, 0));
			//aPSprite_.textureRectOffset = new Vector2(23.0761f, 20.0761f);
		}

		relic.SetFieldValue(fieldName, aPUISprite_);
	}

	public static void SetUpAPItems()
	{
		if (!Connection.pSession.Socket.Connected)
		{
			return;
		}

		var talents = LootStaticDataContainer.sSingleton.pAvailableTalents;
		var relics = LootStaticDataContainer.sSingleton.pAvailableDivineRelics;
		if (relics == null || talents == null)
			return;


		Talents.BackUpTalents(talents);
		DivineRelics.BackUpRelics(relics);

		var locations = Connection.pSession.Locations.AllMissingLocations;

		List<long> relicLocIdsList = new();
		List<long> talentLocIdsList = new();

		foreach (var locId in locations)
		{
			if (locId >= pBaseLocationsId + 100 && locId < pBaseLocationsId + 800) //LOCATION_CHANGES: on location changes update this
			{
				relicLocIdsList.Add(locId);
			}
			else if (locId >= pBaseLocationsId + 800 && locId < pBaseLocationsId + 1500) //LOCATION_CHANGES: on location changes update this
			{
				talentLocIdsList.Add(locId);
			}
		}

		DivineRelics.CreateLocationsRelics(relicLocIdsList);
		Talents.CreateLocationsTalents(talentLocIdsList);
	}

	public static async Task<bool> SetLocalizationsFromAPItems()
	{
		if (Connection.pSession == null || Connection.pSession.Locations == null || LocalizedTextUtility.sSingleton == null)
			return false;
		Dictionary<long, Archipelago.MultiClient.Net.Models.ScoutedItemInfo> locations = new();
		try
		{
			locations = await Connection.pSession.Locations.ScoutLocationsAsync(Archipelago.MultiClient.Net.Enums.HintCreationPolicy.None, Connection.pSession.Locations.AllLocations.ToArray());
			Plugin.Logger.LogMessage(locations.Keys.Count);
		}
		catch
		{
			Plugin.Logger.LogWarning("Not connected - localization strings not set");
			return false;
		}

		List<string> locKeys = new();
		List<string> locRegions = new();
		List<string> locFields = new();
		List<string> locNames = new();

		foreach (var location in locations)
		{
			var locationName = location.Value.LocationDisplayName;
			if (locationName == null)
				Connection.pSession.Locations.GetLocationNameFromId(location.Value.LocationId);
			if (locationName == null)
				locationName = "";
			Plugin.Logger.LogMessage(locationName);

			if (locationName.StartsWith("Divine"))
			{
				locRegions.Add("DivineRelic");
				locRegions.Add("DivineRelic");
				locRegions.Add("DivineRelic");
			}
			else if (locationName.StartsWith("Talent"))
			{
				locRegions.Add("Talent");
				locRegions.Add("Talent");
				locRegions.Add("Talent");
			}
			else
			{
				if (location.Value.LocationId >= pBaseLocationsId + 100 && location.Value.LocationId < pBaseLocationsId + 800)
				{
					locRegions.Add("DivineRelic");
					locRegions.Add("DivineRelic");
					locRegions.Add("DivineRelic");
				}
				else if (location.Value.LocationId >= pBaseLocationsId + 800 && location.Value.LocationId < pBaseLocationsId + 1500)
				{
					locRegions.Add("Talent");
					locRegions.Add("Talent");
					locRegions.Add("Talent");
				}
				else
				{
					locRegions.Add("Location"); //fallback to not crush
					locRegions.Add("Location");
					locRegions.Add("Location");
				}
			}

			string locationBase = "Location" + location.Value.LocationId.ToString();

			locKeys.Add(locationBase + "DisplayName");
			locNames.Add(locationName);
			locFields.Add("DisplayName");

			locKeys.Add(locationBase + "Description");
			locNames.Add(location.Value.ItemDisplayName + " for " + location.Value.Player.Name + " in " + location.Value.ItemGame);
			locFields.Add("Description");

			locKeys.Add(locationBase + "ShortDescription");
			locNames.Add(location.Value.ItemDisplayName);
			locFields.Add("ShortDescription");
		}

		if(!Utils.AddTranslationsToLocalizationData(locKeys, locRegions, locFields, locNames))
			return false;

		Plugin.Logger.LogInfo("Localization from APItems gathered");

		return true;
	}



}
