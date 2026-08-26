using Zyklus.LevelGeneration;
using Zyklus.Loot;
using Zyklus.Managers;
using Zyklus.UI;
using ArchipelagoRandomizer.Items;

namespace ArchipelagoRandomizer.EventHandlers;

public static class OnMatrixGenDone
{
	public static void SubscribeToMatrixGenDone()
	{
		Matrix.sOnMatrixGenDone += Items.Talents.OnMatrixGenDone;
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
        AudioOptionsMenu.sSingleton.SetSFXBusVolume(0, false);


        //give items to player
        Items.Items.sSingleton.GiveItemsToPlayer(player, lootContainer, Connection.pSession.Items.AllItemsReceived);
		

        // restore old value
        if (!savedValue)
            lootContainer.SetFieldValue("should_throw_", savedValue);

        AudioOptionsMenu.sSingleton.SetSFXBusVolume(tempVolumeStorage, false);
    }
}