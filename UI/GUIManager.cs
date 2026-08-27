

using System.Collections.Generic;
using UnityEngine;
using Zyklus.UI;

namespace ArchipelagoRandomizer.UI;

public class GUIManager
{
	public static GUIManager sSingleton;
	public bool pIsVisible = false;

	private string uri_;
	private string slotName_;
	private string password_;

	public string pSlotName
	{
		get => slotName_;
	}
	public string pPassword
	{
		get => slotName_;
	}

	public GUIManager()
	{
		sSingleton = this;
		uri_ = PlayerPrefs.GetString("APuriSavedValue", "archipelago.gg:");
		slotName_ = PlayerPrefs.GetString("APslotNameSavedValue", "PlayerName1");
		password_ = PlayerPrefs.GetString("APpasswordSavedValue", "");
	}

	public void OnGUI()
	{
		//draw connection dialog
		string statusMessage;
		if (Connection.pIsConnected)
		{
			if (!pIsVisible)
				return;

			statusMessage = "Connected as " + Connection.pSession.Players.ActivePlayer.Name;
			GUI.Label(new Rect(16, 50, 300, 20), statusMessage);
		}
		else
		{
			statusMessage = "Archipelago Status: Disconnected";
			GUI.Label(new Rect(16, 50, 300, 20), statusMessage);
			GUI.Label(new Rect(16, 70, 150, 20), "Host: ");
			GUI.Label(new Rect(16, 90, 150, 20), "Player Name: ");
			GUI.Label(new Rect(16, 110, 150, 20), "Password: ");

			uri_ = GUI.TextField(new Rect(150, 70, 150, 20),
				uri_);
			slotName_ = GUI.TextField(new Rect(150, 90, 150, 20),
				slotName_);
			password_ = GUI.TextField(new Rect(150, 110, 150, 20),
				password_);

			// requires that the player at least puts *something* in the slot name
			if (GUI.Button(new Rect(16, 130, 100, 20), "Connect") && !string.IsNullOrWhiteSpace(slotName_))
			{
				Connection.sSingleton.CreateSession(uri_).Connect(slotName_, password_);

				if(!Connection.pIsConnected)
					return;

				Plugin.sSingleton.StartCoroutine(nameof(Plugin.sSingleton.WaitAndInit));

				PlayerPrefs.SetString("APuriSavedValue", uri_);
				PlayerPrefs.SetString("APslotNameSavedValue", slotName_);
				PlayerPrefs.SetString("APpasswordSavedValue", password_);
			}
		}
	}
}