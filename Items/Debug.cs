using System.Collections.Generic;
using ArchipelagoRandomizer;
using BepInEx.Logging;
using Talents;
using UnityEngine;
using Zyklus.LevelGeneration;
using Zyklus.Managers;

namespace Items;

public static class DebugPlugin
{

	public static bool pIsDebug = true;

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