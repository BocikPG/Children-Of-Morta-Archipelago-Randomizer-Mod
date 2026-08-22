using System;
using System.Collections.Generic;
using Altar.Events;
using ArchipelagoRandomizer;
using Items;
using UnityEngine;
using Zyklus.Home;
using Zyklus.Managers;
using Zyklus.Morta;
using Zyklus.UI;

public class OnMorningStarted : MonoBehaviour
{

    private const string _onMorningStartedEventJSONString = "{\"name\": \"BocikInit\",\"version_\":1,\"target_method_name_\": \"Event_OnMorningStarted\"}";

    private static List<string> locKeys_ = null;
    private static List<string> locRegions_ = null;
    private static List<string> locFields_ = null;
    private static List<string> locNames_ = null;
    private static bool locSet = false;

    public static List<string> pLocKeys
    {
        get => locKeys_;
        set => locKeys_ = value;
    }
    public static List<string> pLocRegions
    {
        get => locRegions_;
        set => locRegions_ = value;
    }
    public static List<string> pLocFields
    {
        get => locFields_;
        set => locFields_ = value;
    }
    public static List<string> pLocNames
    {
        get => locNames_;
        set => locNames_ = value;
    }

    public static void OnMorningStartedSetUp()
    {
        var target = HomeManager.sSingleton.gameObject.AddComponent<OnMorningStarted>();
        AltarEventTarget eventTarget = JsonUtility.FromJson<AltarEventTarget>(_onMorningStartedEventJSONString); //bypass private constructor
        eventTarget.pTargetBehaviour = target;

        Debug.LogError(eventTarget.GetDebugText());
        foreach (var eventInfo in HomeManager.sSingleton.GetAllAltarEvents(true))
        {
            if (eventInfo.pAltarEventFieldName == "on_morning_started_")
            {
                eventInfo.pAltarEvent.AddTarget(eventTarget);
            }
        }

        SetUpLocalization();
    }

    private static void SetUpLocalization()
    {
        if (!locSet)
        {
            APItemsUtils.SetLocationsFromAPItems();
            if (locKeys_ != null)
            {
                Utils.AddTranslationsToLocalizationData(locKeys_, locRegions_, locFields_, locNames_);
                locSet = true;
                Plugin.Logger.LogInfo("Localization for APItems set");
            }
        }
    }

    [EventTarget]
    private void Event_OnMorningStarted()
    {
        if (ProfileManager.sSingleton.pGameMode != GameMode.Endless)
            return;
            
        SetUpLocalization();

        SetUpPlayableCharacters();

        APItemsUtils.SetUpOnMorningStarted();

        //UnlockCampaignPortals(); //in endless there are no portals - keeping for maybe one day someone will code it :v
    }

    private static void SetUpPlayableCharacters()
    {
        if (Connection.pLogin == null || Connection.pLogin.Successful == false || Connection.pSession.Items == null || Connection.pSession.Items.AllItemsReceived == null)
            return;

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

        if (DebugPlugin.pIsDebug)
        {
            ProfileManager.sSingleton.pLocalProfile.UnlockCharacter(Zyklus.Player.PlayerCharacterEnum.John);
            ProfileManager.sSingleton.pLocalProfile.UnlockCharacter(Zyklus.Player.PlayerCharacterEnum.Mark);
            ProfileManager.sSingleton.pLocalProfile.UnlockCharacter(Zyklus.Player.PlayerCharacterEnum.Kevin);
            ProfileManager.sSingleton.pLocalProfile.UnlockCharacter(Zyklus.Player.PlayerCharacterEnum.Linda);
            ProfileManager.sSingleton.pLocalProfile.UnlockCharacter(Zyklus.Player.PlayerCharacterEnum.Lucy);
            ProfileManager.sSingleton.pLocalProfile.UnlockCharacter(Zyklus.Player.PlayerCharacterEnum.Joey);
            ProfileManager.sSingleton.pLocalProfile.UnlockCharacter(Zyklus.Player.PlayerCharacterEnum.Apon);
            ProfileManager.sSingleton.pLocalProfile.UnlockCharacter(Zyklus.Player.PlayerCharacterEnum.Bec);
        }


        Plugin.Logger.LogWarning("items are here");
        foreach (var item in Connection.pSession.Items.AllItemsReceived)
        {
            // Plugin.Logger.LogWarning(item?.ItemName);
            // Plugin.Logger.LogWarning(item?.ItemGame);
            // Plugin.Logger.LogWarning(item?.ItemDisplayName);
            // Plugin.Logger.LogWarning(item?.ItemId);
            if (item.ItemName == null)
            {
                Plugin.Logger.LogError(item.ItemId + " ItemName is null, wrong world set up - missing item_name_to_id ref");
                continue;
            }
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

    private static List<MortaPlacesEnum> PortalsToOpen = new();
    private static List<MortaPlacesEnum> OpenedPortals = new();

    private static void UnlockCampaignPortals() // it works on the next day TODO: set up for other portals than first one
    {
        bool? include = Connection.pSession.DataStorage[Archipelago.MultiClient.Net.Enums.Scope.Game, "includePortals"];
        if (include == null)
        {
            Plugin.Logger.LogError("Include portals setting not found");
            include = false;
        }

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


        var list = singleton.GetFieldValue<List<DungeonSelectButton>>("pSelectButtons");

        foreach (var item in Connection.pSession.Items.AllItemsReceived)
        {
            if (!item.ItemName.StartsWith("Dungeon"))
                continue;

            var dungName = item.ItemName.Substring(8);

            foreach (var button in list)
            {
                if (button.pDungeon.ToString() == dungName)
                {
                    Plugin.Logger.LogError(dungName);

                    button.SetFieldValue("required_dungeon_t_unlock_", Zyklus.Morta.MortaDungeonsEnum.Tutorial);

                    var portalCandidate = button.pDungeon.GetCampaignDungeonPlace();

                    if (!OpenedPortals.Contains(portalCandidate))
                    {
                        if ((bool)include)
                        {
                            Plugin.Logger.LogError("inlcude");
                            foreach (var itemSmall in Connection.pSession.Items.AllItemsReceived)
                            {
                                if (itemSmall.ItemName.StartsWith("Portal "))
                                {
                                    var portalName = item.ItemName.Substring(7);

                                    if (portalCandidate.ToString() == portalName)
                                    {
                                        PortalsToOpen.Add(portalCandidate);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Plugin.Logger.LogError("add " + portalCandidate);
                            PortalsToOpen.Add(portalCandidate);
                        }

                    }
                }
            }
        }

        if (PortalsToOpen.Count > 0)
        {
            if (PortalsToOpen[0] == MortaPlacesEnum.Cave)
            {
                SetAndRemoveFromList(singleton, -1); //using values from MortaDungeonsEnum
            }
            else if (PortalsToOpen[0] == MortaPlacesEnum.WindTemple)
            {
                SetAndRemoveFromList(singleton, 2);
            }
            else if (PortalsToOpen[0] == MortaPlacesEnum.Forest)
            {
                SetAndRemoveFromList(singleton, 7);
            }
            else if (PortalsToOpen[0] == MortaPlacesEnum.Magma)
            {
                SetAndRemoveFromList(singleton, 16); //?
            }
            else if (PortalsToOpen[0] == MortaPlacesEnum.Temple)
            {
                SetAndRemoveFromList(singleton, 10001);//?
            }

        }

        static void SetAndRemoveFromList(DungeonSelectMenu singleton, int dungeonIndex)
        {
            //these two lines actually triggers cutScenes (on the next day(u sure?) cuz triggering OnMorningStarted - preferably find better event) 

            Plugin.Logger.LogError("set " + dungeonIndex);


            singleton.SetFieldValue("is_first_time_unlocked_", true);
            singleton.SetFieldValue("first_time_cleared_dungeon_index_", dungeonIndex);
            PortalsToOpen.RemoveAt(0);
        }
    }
}
