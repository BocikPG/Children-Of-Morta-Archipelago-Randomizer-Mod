using System.Collections.Generic;
using ArchipelagoRandomizer;
using UnityEngine;
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
			pDivineRelics = relics;
	}

	internal static DivineRelicHandle GetTier1Relic()
	{
		return pDivineRelics[1]; //TODO: actually find differences between them xd
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

	public static void GiveReceivedRelics(Matrix matrix)
	{
		PlayerManager.sSingleton.GetPlayer(0).pDivineRelicContainer.RemoveAllPassiveDivineRelics();

		bool savedValue = LootStaticDataContainer.sSingleton.GetFieldValue<bool>("should_throw_");//to spread the items
		if (!savedValue)
			LootStaticDataContainer.sSingleton.SetFieldValue("should_throw_", true);

		foreach (var item in Connection.pSession.Items.AllItemsReceived) //ASK:all my items, or all in multiWorld? if first remove player check in function
		{
			Plugin.sSingleton.pConnection.ReceiveDivineRelic(item, true);//wonky ;v
		}

		if (!savedValue)
			LootStaticDataContainer.sSingleton.SetFieldValue("should_throw_", savedValue);

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

	public static void SearchForRelicByNameAndAddItToPlayer(string relicName, bool isReceivingMany = false)
	{
		if(relicName == null || relicName.StartsWith("Character "))
		{
			return;
		}

		foreach (var relic in pDivineRelics)
		{
			Plugin.Logger.LogInfo("relic comparing " + relic.name);
			if (relic.name.StartsWith(relicName))
			{
				//Plugin.Logger.LogInfo("relic found " + relic.name);
				if (!relic.GetIsUsable())
				{
					GameObject obj = LootStaticDataContainer.sSingleton.DropDivineRelic(relic, Vector2.zero, Vector2.zero, Zyklus.Player.PlayerNumberFlag.P1, false, !isReceivingMany); //TODO: settings for sounds :3 or just remove ear rape on loading xD 
					var passive = obj.GetComponent<PassiveDivineRelicBase>();
					passive.InteractionComponent_OnInteract(PlayerManager.sSingleton.GetPlayer(0).pInteractionManager);
				}
				else
				{
					var pos = PlayerManager.sSingleton.GetPlayer(0).transform.position;
					bool savedValue = LootStaticDataContainer.sSingleton.GetFieldValue<bool>("should_throw_");
					if (!savedValue)
						LootStaticDataContainer.sSingleton.SetFieldValue("should_throw_", true);
					GameObject obj = LootStaticDataContainer.sSingleton.DropDivineRelic(relic,
															isReceivingMany ? new Vector2(pos.x + UnityEngine.Random.Range(-5, 5), pos.y + UnityEngine.Random.Range(-5, 5)) : new Vector2(pos.x, pos.y),
															new Vector2(UnityEngine.Random.value, UnityEngine.Random.value).normalized,
															Zyklus.Player.PlayerNumberFlag.P1,
															true,
															!isReceivingMany);
					if (!savedValue)
						LootStaticDataContainer.sSingleton.SetFieldValue("should_throw_", savedValue);
					//PlayerManager.sSingleton.GetPlayer(0).pDivineRelicContainer.AddUsableDivineRelic(obj.GetComponent<UsableDivineRelicBase>());
				}

				break;
			}
		}
	}
}