using System.Collections.Generic;
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
	internal static void BackUpRelics(List<DivineRelicHandle> relics)
	{
		if (pDivineRelics == null)
		{
			// foreach(var item in relics)
			// 	Plugin.Logger.LogWarning(item.name);
			pDivineRelics = new List<DivineRelicHandle>(relics);
		}
	}

	internal static DivineRelicHandle GetTier1Relic()
	{
		return pDivineRelics[0];  //TODO: actually find differences between them xd
	}

	internal static DivineRelicHandle GetTier2Relic()
	{
		//TODO: find one
		return null;
	}

	internal static DivineRelicHandle GetTier3Relic()
	{
		//TODO: find one
		return null;
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

		DivineRelicHandle t1Relic = GetTier1Relic();


		Plugin.Logger.LogWarning(locationCheckHelper.AllMissingLocations.Count);

		foreach (var item in locationCheckHelper.AllMissingLocations)
		{
			Plugin.Logger.LogWarning(item);
			list.Add(TurnRelicToAPItem(Plugin.sSingleton.GetInstance(t1Relic), item));
		}
		LootStaticDataContainer.sSingleton.pAvailableDivineRelics = list;
	}

	public static DivineRelicHandle TurnRelicToAPItem(DivineRelicHandle relic, long item)
	{
		relic.name = item.ToString();
		// TODO: relic.pLocalizedDisplayName = item.ItemDisplayName;
		APItemsUtils.SetInGameSprite(relic, "ingame_sprite_");
		Utils.SetFieldValue<List<SpritePair>>(relic, "conditional_ingame_sprite_list_", null);
		return relic;
		//ILocalizable.SetLocalizedKeys()
	}


	public static void OnPassiveDivineRelicAcquiredLocally(PassiveDivineRelicBase divine_relic)
	{
		Plugin.Logger.LogInfo(divine_relic.pHandle.name);
		//do something archipellagy with this :3

		if (long.TryParse(divine_relic.pHandle.name, out var id))
		{
			try
			{
				PlayerManager.sSingleton.GetPlayer(0).pDivineRelicContainer.RemovePassiveDiniveRelic(divine_relic); //remove from pool, not from player...
			}
			catch { }
			Connection.pSession.Locations.CompleteLocationChecks(id);
		}


	}

	public static void OnDivineRelicAcquiredLocally(UsableDivineRelicBase current_divine_relic, UsableDivineRelicSlot slot)
	{
		Plugin.Logger.LogInfo(current_divine_relic.pHandle.name);
		if (long.TryParse(current_divine_relic.pHandle.name, out var id))
		{
			try
			{
				PlayerManager.sSingleton.GetPlayer(0).pDivineRelicContainer.RemoveUsableDivineRelic(slot);
			}
			catch { }
			Connection.pSession.Locations.CompleteLocationChecks(id);
		}

	}

	public static bool SearchForRelicByNameAndAddItToPlayer(string relicName, LootStaticDataContainer lootStaticDataContainer, bool isReceivingMany = false)
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
				//Plugin.Logger.LogInfo("relic found " + relic.name);
				if (!relic.GetIsUsable())
				{
					GameObject obj = lootStaticDataContainer.DropDivineRelic(relic, Vector2.zero, Vector2.zero, Zyklus.Player.PlayerNumberFlag.P1, false, !isReceivingMany) ?? LootStaticDataContainer.sSingleton.DropDivineRelic(relic, Vector2.zero, Vector2.zero, Zyklus.Player.PlayerNumberFlag.P1, false, !isReceivingMany); //TODO: settings for sounds :3 or just remove ear rape on loading xD 
					var passive = obj.GetComponent<PassiveDivineRelicBase>();
					passive.InteractionComponent_OnInteract(PlayerManager.sSingleton.GetPlayer(0).pInteractionManager);
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

				return true;
			}
		}

		return false;
	}
}