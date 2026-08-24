using System;
using System.Collections.Generic;
using System.Reflection;
using Altar.Events;
using ArchipelagoRandomizer;
using Talents;
using UnityEngine;
using Zyklus.Loot;
using Zyklus.Managers;
using Zyklus.Player;
using Zyklus.UI;

public class TalentButtonSelected : MonoBehaviour
{
	private const string _onButtonClickedJSONString = "{\"name\": \"BocikTalentButtonClick\",\"version_\":1,\"target_method_name_\": \"OnTalentButtonClicked\"}";

	public static void SetupTalentButtons()
	{
		var buttonsList = Utils.GetFieldValue<TalentSelectButton[]>(TalentSelectMenu.sSingleton, "talent_buttons_");

		foreach (var button in buttonsList)
		{
			var target = button.pButton.gameObject.AddComponent<TalentButtonSelected>();
			AltarEventTarget eventTarget = JsonUtility.FromJson<AltarEventTarget>(_onButtonClickedJSONString); //bypass private constructor
			eventTarget.pTargetBehaviour = target;

			Debug.LogError(eventTarget.GetDebugText());
			foreach (var eventInfo in button.pButton.GetAllAltarEvents(true))
			{
				if (eventInfo.pAltarEventFieldName == "on_clicked_")
				{
					if(eventInfo.pAltarEvent.pTargets == null)
						eventInfo.pAltarEvent.pTargets = new();
					eventInfo.pAltarEvent.pTargets.Insert(0,eventTarget);
				}
			}
		}

		
	}

	[EventTarget]
	private void OnTalentButtonClicked()
	{
		TalentSelectButton button = gameObject.GetComponent<TalentSelectButton>();

		if (button == null)
		{
			Plugin.Logger.LogError("TalentButtonSelected: button is null");
			return;
		}

		int buttonId = Utils.GetFieldValue<int>(button, "id_");
		int playerId = Utils.GetFieldValue<int>(button, "player_id_");

		TalentAsset selectedTalent = null;

		if (playerId == 1)
		{
			selectedTalent = Utils.GetFieldValue<List<TalentAsset>>(TalentSelectMenu.sSingleton, "talent_list_")[buttonId];
		}

		if (playerId == 2)
		{
			selectedTalent = Utils.GetFieldValue<List<TalentAsset>>(TalentSelectMenu.sSingleton, "talent_list_2_")[buttonId];
		}

		if (selectedTalent == null)
			return;

		if (long.TryParse(selectedTalent.name, out var id))
		{
			Plugin.Logger.LogInfo($"Talent {selectedTalent.name} clicked, sending location check");
			Connection.pSession.Locations.CompleteLocationChecks(id);

			//I could just set max value of talent to 1, but this seems safer/more consistent with loading of missing locations
			Utils.GetFieldValue<List<TalentAsset>>(LootStaticDataContainer.sSingleton, "available_talents_list_").Remove(selectedTalent); //remove picked one from pool
		}

	}
}