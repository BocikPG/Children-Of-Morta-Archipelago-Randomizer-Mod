
using System.IO;
using BepInEx;
using Items;
using UnityEngine;
using Zyklus.Loot;
using Zyklus.Managers;

namespace ArchipelagoRandomizer;

public class APItemsUtils
{
	private static Sprite aPSprite_;
	private static bool fired = false;

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

		if (!fired)
		{
			PlayerManager.sSingleton.GetPlayer(0).pDivineRelicContainer.OnDivineRelicAcquired += DivineRelics.OnDivineRelicAcquiredLocally;
			PlayerManager.sSingleton.GetPlayer(0).pDivineRelicContainer.OnPassiveDivineRelicAdded += DivineRelics.OnPassiveDivineRelicAcquiredLocally;
			fired = true;
		}

	}

	public static void SetUpOnMorningStarted()
	{
		TalentButtonSelected.SetupTalentButtons();
	}


	
}
