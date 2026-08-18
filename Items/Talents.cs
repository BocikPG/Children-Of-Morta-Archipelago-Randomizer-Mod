using System.Collections.Generic;
using ArchipelagoRandomizer;
using BepInEx.Logging;
using Talents;
using Zyklus.LevelGeneration;
using Zyklus.Loot;
using Zyklus.Managers;
using Zyklus.Player;
using Zyklus.Stat;
using Zyklus.UI;

namespace Items;

public static class Talents
{
	public static List<TalentAsset> pTalents = null; //CONSIDER: split into talents rarity
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

			pTalents = talents;
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

	public static void SetTalents(Matrix matrix)
	{
		if (PlayerManager.sSingleton.GetPlayer(0).pTalentManager == null)
		{
			Plugin.Logger.LogWarning("no talent manager");
			return;
		}

		var player = PlayerManager.sSingleton.GetPlayer(0);

		try
		{
			player.pTalentManager.UnlearnAllTalents();
		}
		catch
		{ }

		foreach (var item in Connection.pSession.Items.AllItemsReceived)
		{
			IfIsTalentLearnIt(item.ItemName);
		}

	}

	public static bool IfIsTalentLearnIt(string name)
	{
		var player = PlayerManager.sSingleton.GetPlayer(0);

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
				return true;
			}
		}

		return false;
	}


}