using System;
using System.Collections.Generic;
using Altar.Localization;
using Archipelago.MultiClient.Net.Helpers;
using ArchipelagoRandomizer;
using UnityEngine;
using Zyklus;
using Zyklus.DivineRelic;
using Zyklus.LevelGeneration;
using Zyklus.Loot;
using Zyklus.Managers;

namespace Items;

public static class DivineRelics
{

	public static List<DivineRelicHandle> pDivineRelics = null;
	public static DivineRelicHandle pDivineRelicBlankHandle = null;
	public static DivineRelicHandle pDivineRelicUsableBlankDecoyHandle = null;
	internal static void BackUpRelics(List<DivineRelicHandle> relics)
	{
		if (pDivineRelics == null)
		{
			// foreach(var item in relics)
			// 	Plugin.Logger.LogWarning(item.name);
			pDivineRelics = new List<DivineRelicHandle>(relics);
			pDivineRelicBlankHandle = pDivineRelics.Find(d => d.name == "Cooldown Reduction Divine Relic - Inventory item handle");
			pDivineRelicUsableBlankDecoyHandle = pDivineRelics.Find(d => d.name == "Slow Totem Divine Relic - Tier 2 - Inventory item handle");
		}
	}

	public static void CreateLocationsRelics(ILocationCheckHelper locationCheckHelper)
	{
		List<DivineRelicHandle> list = new();

		if (locationCheckHelper.AllMissingLocations.Count == 0)
		{
			Plugin.Logger.LogWarning("no locations found, skipping relics creation");
			LootStaticDataContainer.sSingleton.pAvailableDivineRelics = list;
			return;
		}


		Plugin.Logger.LogWarning(locationCheckHelper.AllMissingLocations.Count);

		foreach (var item in locationCheckHelper.AllMissingLocations)
		{
			Plugin.Logger.LogWarning(item);
			List<DivineRelicHandle> tieredRelics = new(); //hack to make endless shop not to crash :3
			for (int i = 0; i < 3; i++)
			{
				tieredRelics.Add(TurnRelicToAPItem(Plugin.sSingleton.GetInstance(pDivineRelicBlankHandle), item, i));
			}
			SetTierHandlers(tieredRelics);
			foreach (var relic in tieredRelics)
			{
				list.Add(relic);
			}

		}

		foreach (var relic in ShopDecoys())
		{
			list.Add(relic);
		}

		LootStaticDataContainer.sSingleton.pAvailableDivineRelics = list;
	}

	private static List<DivineRelicHandle> ShopDecoys() //also hack for shop just to be sure
	{
		List<DivineRelicHandle> shopDecoyList = new();

		TurnRelicToBlankItem(shopDecoyList, pDivineRelicBlankHandle, "APItemBlankRelicDisplayName", "APItemBlankRelicDescription", "APItemBlankRelicShortDescription");
		TurnRelicToBlankItem(shopDecoyList, pDivineRelicBlankHandle, "APItemBlankRelicDisplayName", "APItemBlankRelicDescription", "APItemBlankRelicShortDescription");
		TurnRelicToBlankItem(shopDecoyList, pDivineRelicBlankHandle, "APItemBlankRelicDisplayName", "APItemBlankRelicDescription", "APItemBlankRelicShortDescription");
		TurnRelicToBlankItem(shopDecoyList, pDivineRelicBlankHandle, "APItemBlankRelicDisplayName", "APItemBlankRelicDescription", "APItemBlankRelicShortDescription");
		TurnRelicToBlankItem(shopDecoyList, pDivineRelicUsableBlankDecoyHandle, "APItemBlankRelicDisplayName", "APItemBlankRelicDescription", "APItemBlankRelicShortDescription");
		TurnRelicToBlankItem(shopDecoyList, pDivineRelicUsableBlankDecoyHandle, "APItemBlankRelicDisplayName", "APItemBlankRelicDescription", "APItemBlankRelicShortDescription");
		TurnRelicToBlankItem(shopDecoyList, pDivineRelicUsableBlankDecoyHandle, "APItemBlankRelicDisplayName", "APItemBlankRelicDescription", "APItemBlankRelicShortDescription");
		TurnRelicToBlankItem(shopDecoyList, pDivineRelicUsableBlankDecoyHandle, "APItemBlankRelicDisplayName", "APItemBlankRelicDescription", "APItemBlankRelicShortDescription");

		return shopDecoyList;

		static void TurnRelicToBlankItem(List<DivineRelicHandle> shopDecoyList, DivineRelicHandle prefab, string displayNameKey, string descriptionKey, string shortDescriptionKey)
		{
			List<DivineRelicHandle> tieredRelics = new();
			for (int i = 0; i < 3; i++)
			{
				tieredRelics.Add(TurnRelicToAPItem(Plugin.sSingleton.GetInstance(prefab), -100, i));
			}
			SetTierHandlers(tieredRelics);
			foreach (var rel in tieredRelics)
			{
				shopDecoyList.Add(rel);
			}
		}
	}

	public static void SetTierHandlers(List<DivineRelicHandle> tieredRelics)
	{
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				tieredRelics[i].SetFieldValue<DivineRelicHandle>($"tier_{j + 1}_handle_", tieredRelics[j]);
			}
		}
	}

	public static DivineRelicHandle TurnRelicToAPItem(DivineRelicHandle relic, long apItemId, int tier)
	{
		relic.name = apItemId.ToString();
		APItemsUtils.SetInGameSprite(relic, "ingame_sprite_");
		Utils.SetFieldValue<List<SpritePair>>(relic, "conditional_ingame_sprite_list_", null);

		relic.GetFieldValue<LocalizedText>("localized_display_name_").SetKey($"Location{apItemId}DisplayName");
		relic.GetFieldValue<LocalizedText>("localized_description_").SetKey($"Location{apItemId}Description");
		relic.GetFieldValue<LocalizedText>("localized_short_description_").SetKey($"Location{apItemId}ShortDescription");

		relic.SetFieldValue<int>("item_tier_", tier);

		return relic;
		//ILocalizable.SetLocalizedKeys()
	}


	public static void OnPassiveDivineRelicAcquiredLocally(PassiveDivineRelicBase divine_relic)
	{
		Plugin.Logger.LogInfo("Found: " + divine_relic.pHandle.name);

		if (long.TryParse(divine_relic.pHandle.name, out var id))
		{
			try
			{
				//PlayerManager.sSingleton.GetPlayer(0).pDivineRelicContainer.RemovePassiveDiniveRelic(divine_relic); //remove from pool, not from player...
				LootStaticDataContainer.sSingleton.RemoveDivineRelicVariationsFromList(divine_relic.pHandle);
			}
			catch { }
			Connection.pSession.Locations.CompleteLocationChecks(id);
		}
		if (divine_relic.pHandle.name == "decoy")
		{
			try
			{
				PlayerManager.sSingleton.GetPlayer(0).pDivineRelicContainer.RemovePassiveDiniveRelic(divine_relic);
			}
			catch { }
		}


	}

	public static void OnDivineRelicAcquiredLocally(UsableDivineRelicBase current_divine_relic, UsableDivineRelicSlot slot)
	{
		Plugin.Logger.LogInfo("Found: " + current_divine_relic.pHandle.name);
		if (long.TryParse(current_divine_relic.pHandle.name, out var id))
		{
			Plugin.Logger.LogInfo("relic ID: " + id);
			try
			{
				PlayerManager.sSingleton.GetPlayer(0).pDivineRelicContainer.RemoveUsableDivineRelic(slot);
				LootStaticDataContainer.sSingleton.RemoveDivineRelicVariationsFromList(current_divine_relic.pHandle);
			}
			catch { }
			Connection.pSession.Locations.CompleteLocationChecks(id);
		}
		if (current_divine_relic.pHandle.name == "decoy")
		{
			try
			{
				PlayerManager.sSingleton.GetPlayer(0).pDivineRelicContainer.RemoveUsableDivineRelic(slot);
			}
			catch { }
		}

	}

	public static bool SearchForRelicByNameAndAddItToPlayer(string relicName, LootStaticDataContainer lootStaticDataContainer, bool isReceivingMany = false, ReceivedItemsHelper helper = null)
	{
		if (relicName == null || lootStaticDataContainer == null)
		{
			return false;
		}

		foreach (var relic in pDivineRelics)
		{
			//Plugin.Logger.LogInfo("relic comparing " + relic.name);
			if (relic.name.StartsWith(relicName))
			{
				Plugin.Logger.LogInfo("relic found " + relic.name);
				if (!relic.GetIsUsable())
				{
					try
					{
						GameObject obj = lootStaticDataContainer.DropDivineRelic(relic, Vector2.zero, Vector2.zero, Zyklus.Player.PlayerNumberFlag.P1, false, !isReceivingMany) ?? LootStaticDataContainer.sSingleton.DropDivineRelic(relic, Vector2.zero, Vector2.zero, Zyklus.Player.PlayerNumberFlag.P1, false, !isReceivingMany); //TODO: settings for sounds :3 or just remove ear rape on loading xD 
						var passive = obj.GetComponent<PassiveDivineRelicBase>();
						passive.InteractionComponent_OnInteract(PlayerManager.sSingleton.GetPlayer(0).pInteractionManager);
						//PlayerManager.sSingleton.GetPlayer(0).pDivineRelicContainer.AddPassiveDivineRelic(passive);
					}
					catch { }

				}
				else
				{
					var pos = PlayerManager.sSingleton.GetPlayer(0).transform.position;
					bool savedValue = lootStaticDataContainer.GetFieldValue<bool>("should_throw_");
					if (!savedValue)
						lootStaticDataContainer.SetFieldValue("should_throw_", true);
					GameObject obj = lootStaticDataContainer.DropDivineRelic(relic,
															isReceivingMany ? new Vector2(pos.x + UnityEngine.Random.Range(-5, 5), pos.y + UnityEngine.Random.Range(-5, 5)) : new Vector2(pos.x, pos.y),
															new Vector2(UnityEngine.Random.value, UnityEngine.Random.value).normalized,
															Zyklus.Player.PlayerNumberFlag.P1,
															true,
															!isReceivingMany);
					if (!savedValue)
						lootStaticDataContainer.SetFieldValue("should_throw_", savedValue);
					//PlayerManager.sSingleton.GetPlayer(0).pDivineRelicContainer.AddUsableDivineRelic(obj.GetComponent<UsableDivineRelicBase>());
				}

				Plugin.Logger.LogInfo("Received: " + relicName);
				if (helper != null)
				{
					helper.DequeueItem();
				}

				return true;
			}
		}

		return false;
	}
}