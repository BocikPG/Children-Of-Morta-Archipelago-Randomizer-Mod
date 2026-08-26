using System;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using ArchipelagoRandomizer;
using ArchipelagoRandomizer.Items;

public class Connection
{
	public static Connection sSingleton;
	public static ArchipelagoSession pSession;
	public static LoginSuccessful pLogin;

	public Connection()
	{
		sSingleton = this;
	}

	public Connection CreateSession(string server)
	{
		pSession = ArchipelagoSessionFactory.CreateSession(server);
		//TODO: hook up all functions here
		pSession.Items.ItemReceived += Items.sSingleton.ReceiveItem;
		pSession.Socket.ErrorReceived += (error, frror) => Plugin.Logger.LogError(error.Message);

		Plugin.Logger.LogInfo("session");

		return this;
	}

	public void Connect(string user, string pass = null)
	{
		LoginResult result;

		result = pSession.TryConnectAndLogin("Children of Morta", user, ItemsHandlingFlags.AllItems, new Version(0, 6, 0), password: pass); //consider other item flags
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
		Plugin.Logger.LogInfo("Connected to archipelago server");
		var loginSuccess = (LoginSuccessful)result;
		pLogin = loginSuccess;
	}
}