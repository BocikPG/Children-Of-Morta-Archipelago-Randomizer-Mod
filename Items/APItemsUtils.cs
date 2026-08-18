using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Altar.Events;
using Altar.Localization;
using BepInEx;
using BepInEx.Logging;
using Items;
using Talents;
using UnityEngine;
using Zyklus.Home;
using Zyklus.Loot;
using Zyklus.Managers;
using Zyklus.Player;
using Zyklus.Stat;

namespace ArchipelagoRandomizer;

public class APItemsUtils
{
	private static Sprite aPSprite_;
	private static bool fired = false;

	private static void SetInGameSprite<T>(T relic, string fieldName) where T : class
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

		//CreateLocationsRelics();
		CreateLocationsTalents();

		if (!fired)
		{
			PlayerManager.sSingleton.GetPlayer(0).pDivineRelicContainer.OnDivineRelicAcquired += DivineRelics.OnDivineRelicAcquiredLocally;
			PlayerManager.sSingleton.GetPlayer(0).pDivineRelicContainer.OnPassiveDivineRelicAdded += DivineRelics.OnPassiveDivineRelicAcquiredLocally;
			fired = true;
		}

	}

	private static void CreateLocationsRelics()
	{
		DivineRelicHandle t1Relic = DivineRelics.GetTier1Relic();

		List<DivineRelicHandle> list = new();

		Plugin.Logger.LogWarning(Connection.pSession.Locations.AllMissingLocations.Count);

		foreach (var item in Connection.pSession.Locations.AllMissingLocations)
		{
			Plugin.Logger.LogWarning(item);
			list.Add(TurnRelicToAPItem(Plugin.sSingleton.GetInstance(t1Relic), item));
		}
		LootStaticDataContainer.sSingleton.pAvailableDivineRelics = list;
	}

	private static void CreateLocationsTalents()
	{
		List<TalentAsset> list = new();

		Plugin.Logger.LogWarning(Connection.pSession.Locations.AllMissingLocations.Count);

		foreach (var item in Connection.pSession.Locations.AllMissingLocations)
		{
			Plugin.Logger.LogWarning(item);
			list.Add(TurnTalentToAPItem(Plugin.sSingleton.GetInstance<TalentAsset>(Items.Talents.pTalents[0]), item));
		}
		Utils.SetFieldValue(LootStaticDataContainer.sSingleton, "available_talents_list_", list);

		for (int i = 0; i < 9; i++) //add 9 blank talents (3 of each type) to prevent crash
		{
			list.Add(TurnTalentToAPItem(Plugin.sSingleton.GetInstance<TalentAsset>(Items.Talents.pTalents[0]), -100));
		}
	}

	public static DivineRelicHandle TurnRelicToAPItem(DivineRelicHandle relic, long item)
	{
		relic.name = item.ToString();
		// TODO: relic.pLocalizedDisplayName = item.ItemDisplayName;
		SetInGameSprite(relic, "ingame_sprite_");
		return relic;
		//ILocalizable.SetLocalizedKeys()
	}

	private static TalentAsset TurnTalentToAPItem(TalentAsset talentAsset, long item)
	{
		if (item == -100)
		{
			talentAsset.name = "Blank";
			SetInGameSprite(talentAsset, "icon_");
			SetAndCycleRarity(talentAsset);
			// Utils.SetFieldValue(talentAsset, "localized_description_", "Unlucky it gives nothing");
			// Utils.SetFieldValue(talentAsset, "localized_display_name_", "Empty talent");
			// Utils.SetFieldValue(talentAsset, "talent_category_", TalentCategory.Other);
			return talentAsset;
		}

		talentAsset.name = item.ToString();
		SetInGameSprite(talentAsset, "icon_");
		// Utils.SetFieldValue(talentAsset, "localized_description_", "Menaingful desscription");
		// Utils.SetFieldValue(talentAsset, "localized_display_name_", "AP Item");
		// Utils.SetFieldValue(talentAsset, "talent_category_", TalentCategory.Other);
		SetAndCycleRarity(talentAsset);
		Utils.SetFieldValue(talentAsset, "valid_character_", PlayerCharacterEnum.ALL);
		Utils.SetFieldValue(talentAsset, "max_value_stat_", StatEnum.NONE);
		Utils.SetFieldValue(talentAsset, "max_acquire_amount_", 0); //this value should be from archipelago (as all)

		return talentAsset;
	}

	static int isRune = 0;

	private static void SetAndCycleRarity(TalentAsset talentAsset)
	{
		if (isRune == 0)
		{
			Utils.SetFieldValue(talentAsset, "talent_rarity_", TalentRarities.Rune);
			isRune = 1;
		}
		else if (isRune == 1)
		{
			Utils.SetFieldValue(talentAsset, "talent_rarity_", TalentRarities.SkillTree);
			isRune = 2;
		}
		else
		{
			Utils.SetFieldValue(talentAsset, "talent_rarity_", TalentRarities.Generic);
			isRune = 0;
		}
	}
}
