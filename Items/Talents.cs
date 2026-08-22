using System;
using System.Collections.Generic;
using Altar.Localization;
using Archipelago.MultiClient.Net.Helpers;
using ArchipelagoRandomizer;
using Talents;
using Zyklus.LevelGeneration;
using Zyklus.Loot;
using Zyklus.Managers;
using Zyklus.Player;
using Zyklus.Stat;

namespace Items;

public static class Talents
{
	public static List<TalentAsset> pTalents = null; //CONSIDER: split into talents rarity
	public static TalentAsset pBlankTalent = null;
	internal static void BackUpTalents(List<TalentAsset> talents)
	{
		if (pTalents == null)
		{
			// foreach (var item in talents)
			// 	if (item.pTalentRarity == TalentRarities.Generic)
			// 		Plugin.Logger.LogWarning(item.name);
			// Plugin.Logger.LogWarning(null);
			// foreach (var item in talents)
			// 	if (item.pTalentRarity == TalentRarities.SkillTree)
			// 		Plugin.Logger.LogWarning(item.name);
			// Plugin.Logger.LogWarning(null);
			// foreach (var item in talents)
			// 	if (item.pTalentRarity == TalentRarities.Rune)
			// 		Plugin.Logger.LogWarning(item.name);
			// Plugin.Logger.LogWarning(null);

			pTalents = new List<TalentAsset>(talents);

			pBlankTalent = pTalents.Find(t => t.name == "Critical Chacne Up TalentAsset");
			(pBlankTalent as StatModificationTalentAsset).pStatModifiers[0].pItem2.pConstValue = 0;
			(pBlankTalent as StatModificationTalentAsset).pStatModifiers[0].pItem2.pStatValue.pConstValue = 0;

		}
	}


	private static void NOTES()
	{
		//For endless (i guess)
		PlayerManager.sSingleton.GetPlayer(0).pTalentManager.UnlearnAllTalents();
		PlayerManager.sSingleton.GetPlayer(0).pTalentManager.LearnTalent(pTalents[0]);

		// condition for talent to be picked up to show player
		// if (this.available_talents_list_[index].CheckIsUnlock() && (this.available_talents_list_[index].pValidCharacters == player.pAvatarData.pCharacter || include_all_traits && this.available_talents_list_[index].pValidCharacters == PlayerCharacterEnum.ALL) && !this.available_talents_list_[index].CheckIsMaxed(player) && (this.available_talents_list_[index].pMaxAcquireAmount <= 0 || player.pTalentManager.GetSelectedTalentCount(this.available_talents_list_[index]) < this.available_talents_list_[index].pMaxAcquireAmount))
		// characterTalents.Add(this.available_talents_list_[index]);

		//traits
		var traits = StatStaticDataContainer.sSingleton.pFamilyTraits;

		//PlayerBase.OnLevelGain -- get family trait unlock

		//SkillTreeButton.Confirm(); -- hijack for location sake
	}

	public static void CreateLocationsTalents(ILocationCheckHelper locationCheckHelper)
	{
		List<TalentAsset> list = new();

		if (locationCheckHelper.AllMissingLocations.Count == 0)
		{
			Plugin.Logger.LogWarning("no locations found, skipping relics creation");

			for (int i = 0; i < 9; i++) //add 9 blank talents (3 of each type) to prevent crash
			{
				list.Add(TurnTalentToAPItem(Plugin.sSingleton.GetInstance(pBlankTalent), -100));
			}

			Utils.SetFieldValue(LootStaticDataContainer.sSingleton, "available_talents_list_", list);
			return;
		}

		Plugin.Logger.LogWarning(locationCheckHelper.AllMissingLocations.Count);

		foreach (var item in locationCheckHelper.AllMissingLocations)
		{
			Plugin.Logger.LogWarning(item);
			list.Add(TurnTalentToAPItem(Plugin.sSingleton.GetInstance(pBlankTalent), item));
		}

		for (int i = 0; i < 9; i++) //add 9 blank talents (3 of each type) to prevent crash
		{
			list.Add(TurnTalentToAPItem(Plugin.sSingleton.GetInstance(pBlankTalent), -100));
		}

		Utils.SetFieldValue(LootStaticDataContainer.sSingleton, "available_talents_list_", list);
	}


	private static TalentAsset TurnTalentToAPItem(TalentAsset talentAsset, long item)
	{
		if (item == -100)
		{
			talentAsset.name = "Blank";
			APItemsUtils.SetInGameSprite(talentAsset, "icon_");
			Utils.SetFieldValue(talentAsset, "talent_rarity_", TalentRarities.Generic);
			// SetAndCycleRarity(talentAsset); // depreciated - if want to revive, change back TalentManager.rune_talent_interval_ to 3
			talentAsset.GetFieldValue<LocalizedText>("localized_display_name_").SetKey("EmptyTalentDisplayName");
			talentAsset.GetFieldValue<LocalizedText>("localized_description_").SetKey("EmptyTalentDescription");
			talentAsset.GetFieldValue<LocalizedText>("localized_short_description_").SetKey("EmptyTalentShortDescription");
			return talentAsset;
		}

		talentAsset.name = item.ToString();
		APItemsUtils.SetInGameSprite(talentAsset, "icon_");

		talentAsset.GetFieldValue<LocalizedText>("localized_display_name_").SetKey($"Location{item}DisplayName");
		talentAsset.GetFieldValue<LocalizedText>("localized_description_").SetKey($"Location{item}Description");
		talentAsset.GetFieldValue<LocalizedText>("localized_short_description_").SetKey($"Location{item}ShortDescription");

		// Utils.SetFieldValue(talentAsset, "talent_category_", TalentCategory.Other);
		// SetAndCycleRarity(talentAsset);
		Utils.SetFieldValue(talentAsset, "talent_rarity_", TalentRarities.Generic);
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

	public static bool IfIsTalentLearnIt(string name, PlayerBase player, ReceivedItemsHelper helper = null)
	{

		if (name == null || player == null)
		{
			return false;
		}

		//player.pRuneManager.AddRune()

		foreach (var talent in pTalents)
		{

			if (talent == null || talent.name == null)
				continue;

			//player.pRuneManager.AddRune(talent,1f,true); (talents are runes so it can be skipped i guess)

			if (talent.name == name)
			{
				if (talent.pValidCharacters == player.pAvatarData.pCharacter || player.pAvatarData.pCharacter == PlayerCharacterEnum.ALL)
				{
					player.pTalentManager.LearnTalent(talent);
				}

				Plugin.Logger.LogInfo("Received: " + name);
				if (helper != null)
					helper.DequeueItem();

				return true;
			}
		}

		return false;
	}

	internal static void OnMatrixGenDone(Matrix matrix)
	{
		Utils.SetFieldValue(PlayerManager.sSingleton.GetPlayer(0).pTalentManager, "rune_talent_interval_", int.MaxValue); //disable rune talents from appearing, not needed to seek one for each character

	}
}