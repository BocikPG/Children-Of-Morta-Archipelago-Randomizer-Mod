
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BepInEx;
using Items;
using UnityEngine;
using Zyklus.Loot;
using Zyklus.Managers;

namespace ArchipelagoRandomizer;

public class APItemsUtils
{
	private static Sprite aPSprite_;
	private static long base_id_items = 85000; //CHANGE: before release
	private static long base_id_locations = 87000; //CHANGE: before release

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

	public static void SetUpAPItems() //generally working, TODO: populate with Archipelago Data (and handle giving out staff)
	{
		if (!Connection.pSession.Socket.Connected)
		{
			return;
		}

		var talents = LootStaticDataContainer.sSingleton.pAvailableTalents;
		var relics = LootStaticDataContainer.sSingleton.pAvailableDivineRelics;
		if (relics == null || talents == null)
			return;


		Items.Talents.BackUpTalents(talents);
		DivineRelics.BackUpRelics(relics);

		Items.DivineRelics.CreateLocationsRelics(Connection.pSession.Locations);
		Items.Talents.CreateLocationsTalents(Connection.pSession.Locations);


	}

	public static void SetUpOnMorningStarted()
	{
		TalentButtonSelected.SetupTalentButtons();
	}

	public static async void SetLocationsFromAPItems()
	{
		if (Connection.pSession == null || Connection.pSession.Locations == null)
			return;
		Dictionary<long, Archipelago.MultiClient.Net.Models.ScoutedItemInfo> locations;
		try
		{
			locations = await Connection.pSession.Locations.ScoutLocationsAsync(Archipelago.MultiClient.Net.Enums.HintCreationPolicy.None, Connection.pSession.Locations.AllLocations.ToArray());
			Plugin.Logger.LogMessage(locations.Keys.Count);
		}
		catch
		{
			return;
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
				if (location.Value.LocationId >= base_id_locations + 100 && location.Value.LocationId < base_id_locations + 800)
				{
					locRegions.Add("DivineRelic");
					locRegions.Add("DivineRelic");
					locRegions.Add("DivineRelic");
				}
				else if (location.Value.LocationId >= base_id_locations + 800 && location.Value.LocationId < base_id_locations + 1500)
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
			locNames.Add(location.Value.ItemDisplayName + " for " + location.Value.Player + " in " + location.Value.ItemGame);
			locFields.Add("Description");

			locKeys.Add(locationBase + "ShortDescription");
			locNames.Add(location.Value.ItemDisplayName);
			locFields.Add("ShortDescription");
		}

		OnMorningStarted.pLocKeys = locKeys;
		OnMorningStarted.pLocRegions = locRegions;
		OnMorningStarted.pLocFields = locFields;
		OnMorningStarted.pLocNames = locNames;

		Plugin.Logger.LogInfo("Localization from APItems gathered");
	}



}
