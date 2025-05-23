using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Altar.Events;
using Altar.Pool;
using ArchipelagoRandomizer;
using UnityEngine;
using UnityEngine.Diagnostics;
using Zyklus.Home;
using Zyklus.Loot;
using Zyklus.Managers;
using Zyklus.UI;

public class OnMorningStarted : MonoBehaviour
{


    [EventTarget]
    private void Event_OnMorningStarted()
    {
        SetUpPlayableCharacters();

        //ProfileManager.sSingleton.pLocalProfile.UnlockCharacter(Zyklus.Player.PlayerCharacterEnum.Kevin);

        //UnlockPortals(); 

        //LootStaticDataContainer.sSingleton.AddToDropList(); //immediately (in update) drops loot on floor
    }

    private static void SetUpPlayableCharacters()
    {
        //block all characters
        ProfileManager.sSingleton.pLocalProfile.LockCharacter(Zyklus.Player.PlayerCharacterEnum.John);
        ProfileManager.sSingleton.pLocalProfile.LockCharacter(Zyklus.Player.PlayerCharacterEnum.Mark);
        ProfileManager.sSingleton.pLocalProfile.LockCharacter(Zyklus.Player.PlayerCharacterEnum.Kevin);
        ProfileManager.sSingleton.pLocalProfile.LockCharacter(Zyklus.Player.PlayerCharacterEnum.Linda);
        ProfileManager.sSingleton.pLocalProfile.LockCharacter(Zyklus.Player.PlayerCharacterEnum.Lucy);
        ProfileManager.sSingleton.pLocalProfile.LockCharacter(Zyklus.Player.PlayerCharacterEnum.Joey);
        ProfileManager.sSingleton.pLocalProfile.LockCharacter(Zyklus.Player.PlayerCharacterEnum.Apon);
        ProfileManager.sSingleton.pLocalProfile.LockCharacter(Zyklus.Player.PlayerCharacterEnum.Bec);
        //block end

        if (Connection.pSession.Items == null || Connection.pSession.Items.AllItemsReceived == null)
            return;

        Plugin.Logger.LogWarning("items are here");
        foreach (var item in Connection.pSession.Items.AllItemsReceived)
        {
            Plugin.Logger.LogWarning(item?.ItemName);
            Plugin.Logger.LogWarning(item?.ItemGame);
            Plugin.Logger.LogWarning(item?.ItemDisplayName);
            Plugin.Logger.LogWarning(item?.ItemId);
            if (item.ItemName == null)
                continue;
            if (item.ItemName.StartsWith("Character "))
            {
                var name = item.ItemName.Substring(10);
                if (Enum.TryParse<Zyklus.Player.PlayerCharacterEnum>(name, out var result))
                {
                    Plugin.Logger.LogWarning("unlocking");
                    ProfileManager.sSingleton.pLocalProfile.UnlockCharacter(result);
                }
                else
                    Plugin.Logger.LogError("Character not found");
            }
        }
    }

    private static void UnlockPortals() // it works on the next day TODO: set up for other portals than first one
    {
        LocalDatabaseHelper.sSingleton.UnlockPlace(Zyklus.Morta.MortaPlacesEnum.WindTemple);
        LocalDatabaseHelper.sSingleton.ClearDungeon(Zyklus.Morta.MortaDungeonsEnum.Ruins, true);

        int owner_id = 18365;
        ProfileModifier mainStory1 = ProfileModifier.Create_MainStory(owner_id, nameof(HouseDenTransitionManagar));
        ProfileManager.sSingleton.pLocalProfile.AccessDataBase(mainStory1, (DataBaseJob)(db =>
        {
            db.AddRecord("PlayWindOpening");
            db.AddRecord("PlayMagmaOpening");
            db.AddRecord("PlayTempleOpening");
            //db.AddRecord("PlayMA2GoalSet");

        }), true);

        var singleton = DungeonSelectMenu.sSingleton;

        //these two lines actually triggers cutScenes (on the next day(u sure?) cuz triggering OnMorningStarted - preferably find better event) 
        singleton.SetFieldValue("is_first_time_unlocked_", true);
        singleton.SetFieldValue("first_time_cleared_dungeon_index_", 2);


        var list = singleton.GetFieldValue<List<DungeonSelectButton>>("pSelectButtons");

        foreach (var button in list)
        {
            button.SetFieldValue("required_dungeon_t_unlock_", Zyklus.Morta.MortaDungeonsEnum.Tutorial);
        }
    }
}
