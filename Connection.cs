using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using ArchipelagoRandomizer;
using BepInEx.Logging;
using Items;
using UnityEngine;
using Zyklus;
using Zyklus.DivineRelic;
using Zyklus.Loot;
using Zyklus.Managers;

public class Connection
{
	public static ArchipelagoSession pSession;
	public static LoginSuccessful pLogin;

	public Connection CreateSession(string server)
	{
		pSession = ArchipelagoSessionFactory.CreateSession(server);
		//TODO: hook up all functions here
		pSession.Items.ItemReceived += ReceiveItem;
		pSession.Socket.ErrorReceived += (error, frror) => Plugin.Logger.LogError(error.Message);

		Plugin.Logger.LogInfo("session");

		return this;
	}

	public void Connect(string user, string pass = null)
	{
		LoginResult result;


		result = pSession.TryConnectAndLogin("Children of Morta", user, ItemsHandlingFlags.AllItems, new Version(0, 6, 1)); //consider other item flags
																															//app will freeze here (hopefully not for long)

		if (!result.Successful)
		{
			LoginFailure failure = (LoginFailure)result;
			string errorMessage = $"Failed to Connect to {pSession.Socket.Uri} as {user}:";
			foreach (string error in failure.Errors)
			{
				errorMessage += $"\n    {error}";
			}
			foreach (ConnectionRefusedError error in failure.ErrorCodes)
			{
				errorMessage += $"\n    {error}";
			}

			Plugin.Logger.LogError(errorMessage);

			return; // Did not connect, show the user the contents of `errorMessage`
		}

		// Successfully connected, `ArchipelagoSession` (assume statically defined as `session` from now on) can now be
		// used to interact with the server and the returned `LoginSuccessful` contains some useful information about the
		// initial connection (e.g. a copy of the slot data as `loginSuccess.SlotData`)
		Plugin.Logger.LogInfo("success");
		var loginSuccess = (LoginSuccessful)result;
		pLogin = loginSuccess;
	}

	private List<ReceivedItemsHelper> itemsToReceiveQueue = new();

	public void ReceiveItem(ReceivedItemsHelper helper)
    {
        if (DivineRelics.pDivineRelics == null) //connected before game init - doesn't care about items
        {
            helper.PeekItem();
            helper.DequeueItem();
            return;
        }

        if (General.sIsCeaseFireInProgress) //game is paused
        {
            itemsToReceiveQueue.Add(helper);
            General.sSingleton.OnCeaseFireStateChanged += OnCeaseFireStateChanged;
            return;
        }

        ReceiveItemFromHelper(helper);
    }

    private static void ReceiveItemFromHelper(ReceivedItemsHelper helper)
    {
        var player = PlayerManager.sSingleton.GetPlayer(0); // maybe player 2 too?
        var lootContainer = LootStaticDataContainer.sSingleton;

        var peeked = helper.PeekItem();
        if (peeked == null)
            return;
        Plugin.Logger.LogInfo("relic seeking " + peeked.ItemName);
        if (DivineRelics.SearchForRelicByNameAndAddItToPlayer(peeked.ItemName, lootContainer, false, helper))
            return;
        if (Items.Talents.IfIsTalentLearnIt(peeked.ItemName, player, helper))
            return;

        helper.DequeueItem();
    }

    private void OnCeaseFireStateChanged()
    {
		if(General.sIsCeaseFireInProgress)
			return;

		General.sSingleton.OnCeaseFireStateChanged -= OnCeaseFireStateChanged;

		foreach(var helper in itemsToReceiveQueue)
		{
			ReceiveItemFromHelper(helper);
		}
        
    }
}