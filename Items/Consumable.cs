using System;
using System.Collections.Generic;
using ArchipelagoRandomizer.Items;

public static class Consumable
{
	public static List<ConsumableInvetoryItemAsset> pConsumable = null;
	
	internal static void BackUpConsumable(List<ConsumableInvetoryItemAsset> consumable)
	{
		if (pConsumable == null)
		{
			pConsumable = new List<ConsumableInvetoryItemAsset>(consumable);

			var reroll = consumable.Find(c => c.name == "RerollPassiveRelics ConsumableInvetoryItemAsset");

			if(reroll != null)
				Items.sSingleton.itemsToRemove[Items.RemoveItemsFromPoolReason.ForceDivineRelicsShowUpInOrder].Add(reroll);

		}
	}
}