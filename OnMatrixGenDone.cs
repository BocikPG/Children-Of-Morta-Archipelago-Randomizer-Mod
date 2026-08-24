

using ArchipelagoRandomizer;
using Zyklus.LevelGeneration;
using Zyklus.Loot;
using Zyklus.Managers;
using Zyklus.UI;

namespace Items;

public static class OnMatrixGenDone
{
	public static void SubscribeToMatrixGenDone()
	{
		Matrix.sOnMatrixGenDone += Talents.OnMatrixGenDone;
		Matrix.sOnMatrixGenDone += GiveReceivedItems;
	}

	public static void GiveReceivedItems(Matrix matrix)
	{
		var player = PlayerManager.sSingleton.GetPlayer(0); // maybe player 2 too?
		var lootContainer = LootStaticDataContainer.sSingleton;

		// Clear all talents and relics before giving new ones
		player.pDivineRelicContainer.RemoveAllPassiveDivineRelics();
		player.pTalentManager.UnlearnAllTalents();

		player.pDivineRelicContainer.OnDivineRelicAcquired -= DivineRelics.OnDivineRelicAcquiredLocally;
		player.pDivineRelicContainer.OnPassiveDivineRelicAdded -= DivineRelics.OnPassiveDivineRelicAcquiredLocally;
		player.pDivineRelicContainer.OnDivineRelicAcquired += DivineRelics.OnDivineRelicAcquiredLocally;
		player.pDivineRelicContainer.OnPassiveDivineRelicAdded += DivineRelics.OnPassiveDivineRelicAcquiredLocally;

		// save old value and allow items to spread
		bool savedValue = lootContainer.GetFieldValue<bool>("should_throw_");//to spread the items

		if (!savedValue)
			lootContainer.SetFieldValue("should_throw_", true);

		var tempVolumeStorage = AudioOptionsMenu.sSingleton.pSfxVolume;
		AudioOptionsMenu.sSingleton.SetSFXBusVolume(0,false);

		//give items to player
		foreach (var item in Connection.pSession.Items.AllItemsReceived)
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

		// restore old value
		if (!savedValue)
			lootContainer.SetFieldValue("should_throw_", savedValue);

		AudioOptionsMenu.sSingleton.SetSFXBusVolume(tempVolumeStorage,false);
	}
}