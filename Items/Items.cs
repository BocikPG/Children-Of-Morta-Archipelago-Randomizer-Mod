using System;
using System.Collections.Generic;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Zyklus;
using Zyklus.Inventory;
using Zyklus.Loot;
using Zyklus.Managers;
using Zyklus.Player;

namespace ArchipelagoRandomizer.Items;

public class Items
{
	public static Items sSingleton;
	private List<ReceivedItemsHelper> itemsToReceiveQueue_ = new();
	private static Dictionary<PlayerCharacterEnum, bool> enabledCharacters_ = new();

	public static Dictionary<PlayerCharacterEnum, bool> pEnabledCharacters
	{
		get => enabledCharacters_;
	}

	public Items()
	{
		sSingleton = this;

		enabledCharacters_.Add(PlayerCharacterEnum.ALL, false);
		enabledCharacters_.Add(PlayerCharacterEnum.NO_ONE, false);
		enabledCharacters_.Add(PlayerCharacterEnum.John, false);
		enabledCharacters_.Add(PlayerCharacterEnum.Mark, false);
		enabledCharacters_.Add(PlayerCharacterEnum.Kevin, false);
		enabledCharacters_.Add(PlayerCharacterEnum.Linda, false);
		enabledCharacters_.Add(PlayerCharacterEnum.Lucy, false);
		enabledCharacters_.Add(PlayerCharacterEnum.Joey, false);
		enabledCharacters_.Add(PlayerCharacterEnum.Apon, false);
		enabledCharacters_.Add(PlayerCharacterEnum.Bec, false);

		foreach (RemoveItemsFromPoolReason reason in Enum.GetValues(typeof(RemoveItemsFromPoolReason)))
		{
			itemsToRemove[reason] = new();
		}

	}

	public void Init()
	{
		enabledCharacters_[PlayerCharacterEnum.ALL] = false;
		enabledCharacters_[PlayerCharacterEnum.NO_ONE] = false;
		enabledCharacters_[PlayerCharacterEnum.John] = false;
		enabledCharacters_[PlayerCharacterEnum.Mark] = false;
		enabledCharacters_[PlayerCharacterEnum.Kevin] = false;
		enabledCharacters_[PlayerCharacterEnum.Linda] = false;
		enabledCharacters_[PlayerCharacterEnum.Lucy] = false;
		enabledCharacters_[PlayerCharacterEnum.Joey] = false;
		enabledCharacters_[PlayerCharacterEnum.Apon] = false;
		enabledCharacters_[PlayerCharacterEnum.Bec] = false;
	}

	public void ReceiveItemsOnInit()
	{
		if (!Connection.pIsConnected)
			return;

		foreach (var item in Connection.pSession.Items.AllItemsReceived)
		{
			if (UnlockCharacter(item))
			{
				continue;
			}
		}
	}

	public void ReceiveItem(ReceivedItemsHelper helper)
	{
		if (General.sIsCeaseFireInProgress) //game is paused
		{
			itemsToReceiveQueue_.Add(helper);
			General.sSingleton.OnCeaseFireStateChanged += OnCeaseFireStateChanged;
			return;
		}

		ReceiveItemFromHelper(helper);
	}

	public void GiveItemsToPlayer(PlayerBase player, LootStaticDataContainer lootContainer, IEnumerable<ItemInfo> items)
	{
		foreach (var item in items)
		{
			if (UnlockCharacter(item))
			{
				continue;
			}
			else if (Talents.IfIsTalentLearnIt(item.ItemName, player))
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
		var peeked = helper.PeekItem();
		if (peeked == null)
			return;

		Plugin.Logger.LogInfo("item seeking " + peeked.ItemName);

		if (UnlockCharacter(peeked, helper))
			return;

		if (DivineRelics.pDivineRelics == null) //connected before game init - doesn't care about items
		{
			Plugin.Logger.LogInfo("game not inited");
			helper.PeekItem();
			helper.DequeueItem();
			return;
		}

		var player = PlayerManager.sSingleton.GetPlayer(0); // maybe player 2 too?
		var lootContainer = LootStaticDataContainer.sSingleton;

		if (DivineRelics.SearchForRelicByNameAndAddItToPlayer(peeked.ItemName, lootContainer, false, helper))
			return;
		if (Talents.IfIsTalentLearnIt(peeked.ItemName, player, helper))
			return;

		helper.DequeueItem();
	}

	private static bool UnlockCharacter(ItemInfo peeked, ReceivedItemsHelper helper = null)
	{
		if (!peeked.ItemName.StartsWith("Character "))
			return false;

		PlayerCharacterEnum characterToUnlock = (PlayerCharacterEnum)(peeked.ItemId - APItemsUtils.pBaseItemsId - 84);
		enabledCharacters_[characterToUnlock] = true;

		if (helper != null)
		{
			helper.DequeueItem();
		}

		return true;

	}

	private void OnCeaseFireStateChanged()
	{
		if (General.sIsCeaseFireInProgress)
			return;

		General.sSingleton.OnCeaseFireStateChanged -= OnCeaseFireStateChanged;

		foreach (var helper in itemsToReceiveQueue_)
		{
			ReceiveItemFromHelper(helper);
		}
		itemsToReceiveQueue_.Clear();

	}
	internal Dictionary<RemoveItemsFromPoolReason, List<ItemHandle>> itemsToRemove = new();

	internal void RemoveProblematicItems()
	{
		foreach (RemoveItemsFromPoolReason reason in Enum.GetValues(typeof(RemoveItemsFromPoolReason)))
		{
			RemoveProblematicItems(reason);
		}
	}

	internal void RemoveProblematicItems(RemoveItemsFromPoolReason reason)
	{

		foreach (var item in itemsToRemove[reason])
		{
			try
			{
				LootStaticDataContainer.sSingleton.pAvailableConsumableList.Remove(item as ConsumableInvetoryItemAsset);

			}
			catch
			{

			}

		}

	}


	public enum RemoveItemsFromPoolReason
	{
		ForceDivineRelicsShowUpInOrder
	}
}