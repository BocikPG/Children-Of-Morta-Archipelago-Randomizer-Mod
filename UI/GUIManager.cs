

using System;
using System.Collections.Generic;
using Archipelago.MultiClient.Net.Packets;
using UnityEngine;
using UnityEngine.UI;
using Zyklus.UI;

namespace ArchipelagoRandomizer.UI;

public class GUIManager
{
	public static GUIManager sSingleton;
	public bool pIsVisible = false;

	private string uri_;
	private string slotName_;
	private string password_;

	// message vars
	private List<string> logLines_ = new();
	private int maxLines_ = 8;
	private Vector2 scrollView_;
	private Rect window_;
	private Rect scroll_;
	private Rect text_;
	private string scrollText_;
	private GUIStyle textStyle_;
	private float lastUpdateTime_;
    private int scrollDepth_;
    private const float HideTimeout = 10f;

	//public

	public string pSlotName
	{
		get => slotName_;
	}
	public string pPassword
	{
		get => slotName_;
	}
	public Rect CommandTextRect { get; private set; }
	public Rect SendCommandButton { get; private set; }
	public string CommandText { get; private set; }

	public GUIManager()
	{
		sSingleton = this;
		uri_ = PlayerPrefs.GetString("APuriSavedValue", "archipelago.gg:");
		slotName_ = PlayerPrefs.GetString("APslotNameSavedValue", "PlayerName1");
		password_ = PlayerPrefs.GetString("APpasswordSavedValue", "");

		SetUpMessageLogVariables();
		Plugin.OnScreenSizeChanged += SetUpMessageLogVariables;
	}

	public void OnGUI()
	{
		//draw connection dialog
		string statusMessage;
		if (Connection.pIsConnected)
		{
			DrawMessageLog();

			if (!pIsVisible)
				return;

			statusMessage = "Connected to AP as " + Connection.pSession.Players.ActivePlayer.Name;
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

				if (!Connection.pIsConnected)
					return;

				Plugin.sSingleton.StartCoroutine(nameof(Plugin.sSingleton.WaitAndInit));

				PlayerPrefs.SetString("APuriSavedValue", uri_.Trim());
				PlayerPrefs.SetString("APslotNameSavedValue", slotName_);
				PlayerPrefs.SetString("APpasswordSavedValue", password_);
			}
		}
	}

	public void LogMessage(string message)
	{
		if (logLines_.Count >= maxLines_)
		{
			logLines_.RemoveAt(0);
		}
		logLines_.Add(message);

		foreach (var line in logLines_)
		{
			scrollText_ += $"> {line}\n";
		}

		lastUpdateTime_ = Time.time2;
		scrollView_ = new Vector2(0, scrollDepth_);
	}

	private void DrawMessageLog()
	{
		if (logLines_.Count == 0) return;



		if (pIsVisible || Time.time2 - lastUpdateTime_ < HideTimeout)
		{
			scrollView_ = GUI.BeginScrollView(window_, scrollView_, scroll_);
			GUI.Box(text_, "");
			GUI.Box(text_, scrollText_, textStyle_);
			GUI.EndScrollView();

			CommandText = GUI.TextField(CommandTextRect, CommandText);
			if (!string.IsNullOrWhiteSpace(CommandText) && GUI.Button(SendCommandButton, "Send"))
			{
				Connection.pSession.Socket.SendPacket(new SayPacket { Text = CommandText });
				CommandText = "";
			}
		}
	}

	private void SetUpMessageLogVariables()
	{
		int width = (int)(Screen.width * 0.25f);
		int height = (int)(Screen.height * 0.25f);
		scrollDepth_ = height * 10;
		int cmdRectHeight = (int)(Screen.height * 0.025f);

		window_ = new Rect(Screen.width - width, Screen.height - height, width, height);
		scroll_ = new Rect(0, 0, width * 0.9f, scrollDepth_);
		scrollView_ = new Vector2(0, scrollDepth_);
		text_ = new Rect(0, 0, width, scrollDepth_);

		textStyle_ = new();
		textStyle_.alignment = TextAnchor.LowerLeft;
		textStyle_.fontSize = (int)(Screen.height * 0.0185f);
		textStyle_.normal.textColor = Color.white;
		textStyle_.wordWrap = true;

		var xPadding = (int)(Screen.width * 0.01f);
		var yPadding = (int)(Screen.height * 0.01f);

		textStyle_.padding = new RectOffset(xPadding, xPadding, yPadding, yPadding);

		var xPos = (int)(Screen.width - width);
		var yPos = (int)(Screen.height - cmdRectHeight);

		CommandTextRect = new Rect(xPos, yPos, width * (3f / 4f), cmdRectHeight);

		xPos += (int)(width * (3f / 4f));
		SendCommandButton = new Rect(xPos, yPos, width / 4f, cmdRectHeight);
	}
}