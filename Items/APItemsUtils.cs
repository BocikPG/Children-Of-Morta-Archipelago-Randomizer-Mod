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
using UnityEngine;
using Zyklus.Home;
using Zyklus.Loot;
using Zyklus.Managers;

namespace ArchipelagoRandomizer;

public class APItemsUtils
{
	private static Sprite aPSprite_;

	public static DivineRelicHandle TurnRelicToAPItem(DivineRelicHandle relic, long item)
	{
		relic.name = item.ToString();
		// TODO: relic.pLocalizedDisplayName = item.ItemDisplayName;
		SetInGameSprite(relic);
		return relic;
		//ILocalizable.SetLocalizedKeys()
	}

	private static void SetInGameSprite(DivineRelicHandle relic)
	{
		if (aPSprite_ == null)
		{
			var texture = new Texture2D(168, 128);
			ImageConversion.LoadImage(texture, File.ReadAllBytes(Paths.PluginPath + @"\ArchipelagoRandomizer\Assets\Binoculars.png"));
			aPSprite_ = Sprite.Create(texture, new Rect(0, 0, 168, 128), new Vector2(0.5f, 0.5f));
		}

		relic.SetFieldValue("ingame_sprite_", aPSprite_);

	}

	public static void SetUpAPItems() //generally working, TODO: populate with Archipelago Data (and handle giving out staff)
	{
		if (!Connection.pSession.Socket.Connected)
		{
			return;
		}

		var relics = LootStaticDataContainer.sSingleton.pAvailableDivineRelics;
		if (relics == null)
			return;

		DivineRelics.BackUpRelics(relics);
		DivineRelicHandle t1Relic = DivineRelics.GetTier1Relic();

		List<DivineRelicHandle> list = new();

		foreach (var item in Connection.pSession.Locations.AllMissingLocations)
		{
			list.Add(TurnRelicToAPItem(Plugin.sSingleton.GetInstance(t1Relic), item));
		}
		LootStaticDataContainer.sSingleton.pAvailableDivineRelics = list;

		PlayerManager.sSingleton.GetPlayer(0).pDivineRelicContainer.OnDivineRelicAcquired += DivineRelics.OnDivineRelicAcquiredLocally;
		PlayerManager.sSingleton.GetPlayer(0).pDivineRelicContainer.OnPassiveDivineRelicAdded += DivineRelics.OnPassiveDivineRelicAcquiredLocally;
	}




}
