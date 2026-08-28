using System;
using System.Collections.Generic;
using Altar.Events;
using Altar.HFSM;
using Altar.Pool;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Exceptions;
using Archipelago.MultiClient.Net.Packets;
using ArchipelagoRandomizer.Items;
using ArchipelagoRandomizer.UI;
using Newtonsoft.Json.Linq;
using Talents;
using UnityEngine;
using Zyklus;
using Zyklus.GameManager;
using Zyklus.Home;
using Zyklus.Managers;
using Zyklus.Morta;
using Zyklus.UI;

namespace ArchipelagoRandomizer.EventHandlers;

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

    public static void OnMorningStartedInit()
    {
        HookUpToEvent();

        ProfileManager.sSingleton.pLocalUserData.SetEndlessUnlockState(true); //unlock endless (on game start)

        TalentButtonSelected.SetUpTalentButtons();

        GameFlowInterface.sSingleton.GetFieldValue<UIManagerHFSM>("ui_manager_hfsm_").pHFSM.PreEventPush += OnUIStateChangePrePush;
        GameFlowInterface.sSingleton.GetFieldValue<UIManagerHFSM>("ui_manager_hfsm_").pHFSM.PostEventPush += OnUIStateChangePostPush;

        Items.Items.sSingleton.ReceiveItemsOnInit();

        SetUpLocalization();
        SetUpPlayableCharacters();
        SetUpEndlessShop();

        static void HookUpToEvent()
        {
            var target = HomeManager.sSingleton.gameObject.AddComponent<OnMorningStarted>();
            AltarEventTarget eventTarget = JsonUtility.FromJson<AltarEventTarget>(_onMorningStartedEventJSONString); //bypass private constructor
            eventTarget.pTargetBehaviour = target;

            Plugin.Logger.LogInfo(eventTarget.GetDebugText());
            foreach (var eventInfo in HomeManager.sSingleton.GetAllAltarEvents(true))
            {
                if (eventInfo.pAltarEventFieldName == "on_morning_started_")
                {
                    eventInfo.pAltarEvent.AddTarget(eventTarget);
                }
            }
        }
    }

    private static void SetUpEndlessShop()
    {
        var array = EndlessShopManager.sSingleton.GetFieldValue<EndlessShopItemPrice[]>("item_price_");
        foreach (var entry in array)
        {
            if (entry.pItem1 == EndlessShopItemType.DivineRelic)
            {
                entry.pItem3 = 99999;
            }
        }
        EndlessShopManager.sSingleton.SetFieldValue<EndlessShopItemPrice[]>("item_price_", array);
    }

    private static void OnUIStateChangePrePush(HFSM hfsm, int event_code, object sender, ListPoolInstance<object> event_parameters)
    {
        var session = Connection.pSession;

        if (event_code == (int)UIManager_EventsEnum.SHOWING_SESSION_END_REQUESTED) //if end screen is to be shown
        {
            if ((int)event_parameters.pList[0] == (int)GameRunFinishReasonEnum.Win) //reason - of the event, game won in this case
            {
                Plugin.Logger.LogInfo("Win detected");
                var currentCharacter = PlayerManager.sSingleton.pLocalPlayerCharacters[0];

                Plugin.Logger.LogInfo(currentCharacter.ToString());

                try
                {
                    session.DataStorage["ChildrenOfMortaTimesWon"].Initialize(0);
                    session.DataStorage["ChildrenOfMortaTimesWon"] += (long)1;

                    long number = (long)Connection.pSession.DataStorage["ChildrenOfMortaTimesWon"];
                    session.Locations.CompleteLocationChecks(APItemsUtils.pBaseLocationsId + (long)currentCharacter, APItemsUtils.pBaseLocationsId + 8 + number);

                    var aPSettings = session.DataStorage.GetSlotData()["settings"] as JObject;
                    if (aPSettings.Value<int>("isEndlessMode") == 1)
                    {
                        //TODO: campaign implementation
                        int numberOfCharacters = 8; // depends if campaign or not 
                        if (aPSettings.Value<int>("goalEndless") == 0) // defeat_boss_with_every_family_member
                        {
                            CheckIfGoalIsCompleted(session, 0, numberOfCharacters);
                        }
                        else if (aPSettings.Value<int>("goalEndless") == 1) // defeat_boss_X_times
                        {
                            Plugin.Logger.LogInfo("endless");
                            var timesToDefeat = aPSettings.Value<int>("defeatBossXTimes");
                            CheckIfGoalIsCompleted(session, 8, timesToDefeat);
                        }
                    }
                }
                catch (ArchipelagoSocketClosedException)
                {
                    Plugin.Logger.LogError("Winning location failed to send - client not connected");
                }

            }
            else if ((int)event_parameters.pList[0] == (int)GameRunFinishReasonEnum.Lose)
            {
                var aPSettings = session.DataStorage.GetSlotData()["settings"] as JObject;
                if (aPSettings.Value<int>("deathLinkEnabled") == 1)
                {
                    Connection.pDeathLinkService.SendDeathLink(new DeathLink(Connection.pSession.Players.ActivePlayer.Name, "Excluded from family"));
                    Connection.pSession.Socket.SendPacket(new SayPacket {Text = Connection.pSession.Players.ActivePlayer.Name + " has been excluded from family"});
                    Plugin.Logger.LogInfo("Sending death link");

                }
            }

            Plugin.Logger.LogInfo("End of game session");
        }


    }
    private static void OnUIStateChangePostPush(HFSM hfsm, int event_code, object sender, ListPoolInstance<object> event_parameters)
    {
        var session = Connection.pSession;

        if (event_code == (int)UIManager_EventsEnum.SHOWING_TALENT_SELECT_MENU_REQUESTED) // talent select menu show  
        {
            if (PlayerManager.sSingleton.GetPlayer(0).pTalentManager.pAvailableTalentCount > 0)
            {
                var list = TalentSelectMenu.sSingleton.GetFieldValue<List<TalentAsset>>("talent_list_");
                foreach (var item in list)
                {
                    if (long.TryParse(item.name, out var result))
                    {
                        session.Hints.CreateHints(HintStatus.Found, result);
                    }
                }
            }

            // if (PlayerManager.sSingleton.GetPlayer(1)?.pTalentManager.pAvailableTalentCount > 0)
            // {
            //     var list = TalentSelectMenu.sSingleton.GetFieldValue<List<TalentAsset>>("talent_list_2_");
            //     foreach (var item in list)
            //     {
            //         if (long.TryParse(item.name, out var result))
            //         {
            //             session.Hints.CreateHints(HintStatus.Found, result);
            //         }
            //     }
            // }
        }
        else if (event_code == (int)UIManager_EventsEnum.SHOWING_ENDLESS_SHOP_MENU_REQUESTED)
        {
            var list = EndlessShopManager.sSingleton.pSelectedItems;
            foreach (var item in list)
            {
                if (long.TryParse(item.GetName(), out var result))
                {
                    session.Hints.CreateHints(HintStatus.Found, result);
                }
            }
        }
        else if (event_code == (int)UIManager_EventsEnum.SHOWING_CHARACTER_SELECT_MENU_REQUESTED)
        {
            GUIManager.sSingleton.pIsVisible = true;
        }
        else if (event_code == (int)UIManager_EventsEnum.HIDING_CHARACTER_SELECT_MENU_REQUESTED)
        {
            GUIManager.sSingleton.pIsVisible = false;
        }
        else if (event_code == (int)UIManager_EventsEnum.SHOWING_PAUSE_MENU_REQUESTED)
        {
            GUIManager.sSingleton.pIsVisible = true;
        }
        else if (event_code == (int)UIManager_EventsEnum.HIDING_PAUSE_MENU_REQUESTED)
        {
            GUIManager.sSingleton.pIsVisible = false;
        }
        //Plugin.Logger.LogInfo("ui change triggered " + event_code);

    }

    private static void CheckIfGoalIsCompleted(ArchipelagoSession session, long firstIdOffset, long iterations)
    {
        bool goalCompleted = true;
        for (int i = 0; i < iterations; i++)
        {
            if (session.Locations.AllLocationsChecked.Contains(APItemsUtils.pBaseLocationsId + firstIdOffset + i))
            {
                //Plugin.Logger.LogInfo(APItemsUtils.pBaseLocationsId + firstIdOffset + i + " found");
                continue;
            }
            else
            {
                //Plugin.Logger.LogInfo(APItemsUtils.pBaseLocationsId + firstIdOffset + i + " not found");
                goalCompleted = false;
                break;
            }
        }
        if (goalCompleted)
        {
            Plugin.Logger.LogWarning("GOAL COMPLETED!");
            session.SetClientState(ArchipelagoClientState.ClientGoal);
        }

    }

    private static async void SetUpLocalization()
    {
        if (!locSet)
        {
            if (await APItemsUtils.SetLocalizationsFromAPItems())
            {
                locSet = true;
                Plugin.Logger.LogInfo("Localization for APItems set");
            }
        }
    }

    [EventTarget]
    public void Event_OnMorningStarted()
    {
        if (ProfileManager.sSingleton.pGameMode != GameMode.Endless || !Connection.pIsConnected)
            return;

        SetUpLocalization();

        SetUpPlayableCharacters();

        //UnlockCampaignPortals(); //in endless there are no portals - keeping for maybe one day someone will code it :v
    }

    private static void SetUpPlayableCharacters()
    {
        if (!Connection.pIsConnected || Connection.pSession.Items == null || Connection.pSession.Items.AllItemsReceived == null)
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
        foreach (var item in Items.Items.pEnabledCharacters)
        {
            if (item.Value == true)
                ProfileManager.sSingleton.pLocalProfile.UnlockCharacter(item.Key);

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
