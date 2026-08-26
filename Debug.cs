using UnityEngine;

namespace ArchipelagoRandomizer.Items;

public static class DebugPlugin
{

	public static bool pIsDebug = false;

	public static void Update()
	{
		if (pIsDebug)
		{
			if (Input.GetKeyDown(KeyCode.F8))
			{
				Connection.pSession.Locations.CompleteLocationChecks(Connection.pSession.Locations.AllMissingLocations[0]);
			}
		}
			
	}

}