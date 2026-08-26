using System.Collections.Generic;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Zyklus;
using Zyklus.Loot;
using Zyklus.Managers;
using Zyklus.Player;

namespace ArchipelagoRandomizer.Items;

public class Items
{
	public static Items sSingleton;
	private List<ReceivedItemsHelper> itemsToReceiveQueue = new();

	public Items()
	{
		sSingleton = this;
	}

	public void ReceiveItem(ReceivedItemsHelper helper)
	{
		if (DivineRelics.pDivineRelics == null) //connected before game init - doesn't care about items
		{
			helper.PeekItem();
			helper.DequeueItem();
			return;
		}

		if (General.sIsCeaseFireInProgress) //game is paused
		{
			itemsToReceiveQueue.Add(helper);
			General.sSingleton.OnCeaseFireStateChanged += OnCeaseFireStateChanged;
			return;
		}

		ReceiveItemFromHelper(helper);
	}

	public void GiveItemsToPlayer(PlayerBase player, LootStaticDataContainer lootContainer, IEnumerable<ItemInfo> items)
	{
		foreach (var item in items)
		{
			if (Talents.IfIsTalentLearnIt(item.ItemName, player))
			{
				continue;
			}
			else if (DivineRelics.SearchForRelicByNameAndAddItToPlayer(item.ItemName, lootContainer, true))
			{
				continue;
			}
			else
			{
				Plugin.Logger.LogWarning($"Item {item.ItemName} is not a talent or relic, skipping");
			}
		}
	}

	private static void ReceiveItemFromHelper(ReceivedItemsHelper helper)
	{
		var player = PlayerManager.sSingleton.GetPlayer(0); // maybe player 2 too?
		var lootContainer = LootStaticDataContainer.sSingleton;

		var peeked = helper.PeekItem();
		if (peeked == null)
			return;
		Plugin.Logger.LogInfo("relic seeking " + peeked.ItemName);
		if (DivineRelics.SearchForRelicByNameAndAddItToPlayer(peeked.ItemName, lootContainer, false, helper))
			return;
		if (Talents.IfIsTalentLearnIt(peeked.ItemName, player, helper))
			return;

		helper.DequeueItem();
	}

	private void OnCeaseFireStateChanged()
	{
		if (General.sIsCeaseFireInProgress)
			return;

		General.sSingleton.OnCeaseFireStateChanged -= OnCeaseFireStateChanged;

		foreach (var helper in itemsToReceiveQueue)
		{
			ReceiveItemFromHelper(helper);
		}

	}
}