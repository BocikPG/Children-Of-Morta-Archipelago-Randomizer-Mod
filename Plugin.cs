using System.Collections;
using System.Collections.Generic;
using ArchipelagoRandomizer.EventHandlers;
using ArchipelagoRandomizer.Items;
using ArchipelagoRandomizer.UI;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using Zyklus.GameManager;
using Zyklus.Home;
using Zyklus.Loot;
using Zyklus.Managers;
using Zyklus.UI;

namespace ArchipelagoRandomizer;


[BepInPlugin("bocik.plugins.archipelago", "Archipelago Randomizer", "0.1.0")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    public static Plugin sSingleton;
    public Connection pConnection;
    public Items.Items pItems;
    public static System.Action OnScreenSizeChanged;

    private bool initedBefore_ = false;
    private Vector2Int lastScreenSize_;

    public void Update() 
    {
        //DebugPlugin.Update(); //DEBUG: remove if want to debug 

        Vector2Int currentScreenSize = new Vector2Int(Screen.width, Screen.height);
        
        // Check if the size has changed
        if (currentScreenSize != lastScreenSize_)
        {
            lastScreenSize_ = currentScreenSize;
            OnScreenSizeChanged?.Invoke(); // Notify all subscribers
        }
    }

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        if (sSingleton != this)
            sSingleton = this;

        pItems = new();

        pConnection = new();

        lastScreenSize_ = new Vector2Int(Screen.width, Screen.height);

        new GUIManager();
        //pConnection.CreateSession("localhost:38281").Connect("PlayerName");
    }


    public IEnumerator WaitAndInit()
    {
        if(initedBefore_)
            yield break;
        while (ZyklusSceneManager.sSingleton == null || PlayerManager.sSingleton == null || HomeManager.sSingleton == null || GameFlowInterface.sSingleton == null || EndlessShopManager.sSingleton == null)
        {
            // Wait for the next frame
            yield return null;
        }

        OnMorningStarted.OnMorningStartedInit();
        OnMatrixGenDone.SubscribeToMatrixGenDone();
        //ZyklusSceneManager.sSingleton.OnMaterialPlaceChanged += TalentsManager.SetTalents;
        ZyklusSceneManager.sSingleton.OnMaterialPlaceChanged += APItemsUtils.SetUpAPItems;
        initedBefore_ = true;
    }

    private void OnGUI()
    {
        if(GUIManager.sSingleton == null)
            return;

        GUIManager.sSingleton.OnGUI();

    }

    public T GetInstance<T>(T prefab) where T : UnityEngine.Object
    {
        return Instantiate(prefab);
    }

    //notes

    //Info on cutScene end (for new portals unlocks)
    //     [Info   : Unity Log] Begining cutscene skip by user. debug_text=Wind opening sequence - Sequence
    // [Info   : Unity Log] Sequence finished. sequence=Wind opening sequence - Sequence assign_idles=False is_aborting=True
    // [Info   : Unity Log] Raising cutscene finished! name=Wind opening sequence time_finished=False
    // [Info   : Unity Log] Acquiring entities for starting sequence AnaStand
    // runtime_entities=Empty
    // entities=1 item: CaveAspect
    // [Info   : Unity Log] Finishing cutscene skip by user. debug_text=Wind opening sequence - Sequence
    // [Info   : Unity Log] Removing menu House (Zyklus.Home.HomeLoiteringManager) from UIManager
    // [Info   : Unity Log] Pushing Menu= Den


    public virtual void Notes()
    {
        ProfileManager.sSingleton.pLocalProfile.pInventory.AddItem(ProfileModifier.AcquireInstance(0), new Zyklus.Inventory.InventoryItemHandle(), 1);


        //Shop.OnInteractionStart() //ItemHandle adding to inventory

        //ProfileManager.sSingleton.pLocalProfile.pStats // stats in run


        //LootStaticDataContainer.sSingleton.AddDivineRelicVariationsToList(new DivineRelicHandle(){});
        //PlayerManager.sSingleton.GetUser(0);
        //ConsumableBase //po tym dziedziczą consumamble

        LootStaticDataContainer.sSingleton.pAvailableDivineRelics = new(); // available relics etc.
        LootStaticDataContainer.sSingleton.available_consumable_list_ = new(); //someone messed up :dogeKEK:

        //DivineRelicRoomComponent.Initialize(); //creates new item on pedestal
        //LootStaticDataContainer.sSingleton.DropDivineRelic(); //

        DungeonSelectMenu.sSingleton.Show(Zyklus.Morta.MortaPlacesEnum.Magma); //big portal
        LocalDatabaseHelper.sSingleton.UnlockPlace(Zyklus.Morta.MortaPlacesEnum.WindTemple);
        LocalDatabaseHelper.sSingleton.ClearDungeon(Zyklus.Morta.MortaDungeonsEnum.Ruins, false);

        //HouseDenTransitionManagar //handles transitions, opening den portals
        // int owner_id = 18365;
        // ProfileModifier mainStory1 = ProfileModifier.Create_MainStory(owner_id, nameof(HouseDenTransitionManagar));
        // ProfileManager.sSingleton.pLocalProfile.AccessDataBase(mainStory1, (DataBaseJob)(db =>
        // {
        //     Record record1 = db.QueryByName("PlayDenCleansing");
        //     Record record2 = db.QueryByName("PlayWindOpening");
        //     Record record3 = db.QueryByName("PlayMagmaOpening");
        //     Record record4 = db.QueryByName("PlayTempleOpening");
        //     Record record5 = db.QueryByName("PlayMA2GoalSet");
        // }

        var list = DungeonSelectMenu.sSingleton.GetFieldValue<List<DungeonSelectButton>>("pSelectButtons");

        foreach (var button in list)
        {
            button.SetFieldValue("required_dungeon_t_unlock_", Zyklus.Morta.MortaPlacesEnum.Tutorial);
        }

        // LocalDatabaseHelper.sSingleton.IsPlaceUnlocked(Zyklus.Morta.MortaPlacesEnum.Magma);
        // LocalDatabaseHelper.sSingleton.IsDungeonCleared(Zyklus.Morta.MortaDungeonsEnum.Ziggurat);

        // kisiel = new AltarEventTarget(); //create with JSON

        //HealingPotion basea = new();
        //this.altar_events_.Add(new AltarEventInfo(this.on_morning_started_, "on_morning_started_"));

        //basea.player_.pConsumableManager.ConsumableObtained(basea);

        //LootStaticDataContainer.sSingleton.AddToDropList(); //immediately (in update) drops loot on floor


        //GameStats.endless_mode_total_side_objectives_done_ //can be check at the matrix done, to add location for side objectives
        //crashes on receiving:
        //  Contact Damage divine relic - Inventory item handle
        //  Blood Rush Divine Relic - Tier 3 - Inventory item handle
        //  JoeyWhirlwindFONRune ??
        //  Crit Chance Relic - Tier 3 - Inventory item handle
    }


    //namespace Zyklus.UI;
    // class DungeonSelectMenu : MenuBase
    // public void Show(MortaPlacesEnum chapter)
    // {
    //     this.chapter_ = chapter;
    //     this.SetDungeonState();
    //     this.Setup();
    //     this.SetupButtons();
    //     this.animation_interact_lock_ = true;
    //     this.tween_.PlayOpenSequence();
    //     this.canvas_.enabled = true;
    // }

    // private void Setup()
    // {
    //     if ((Object)null != (Object)this.pSelectedButton)
    //         this.pSelectedButton.Unselect();
    //     this.is_joystick_ = LocalUserManager.sSingleton.IsControllerJoystick(PlayerLocalNumber.Local1);
    //     this.SetRootTransformPosition(this.is_joystick_);
    //     this.is_camera_handle_pushed_ = true;
    //     UIParliament.sSingleton.pMenuFixerCamera.CloneFromHandle(CameraManagerComponent.GetSingleton().pHandleStack.pActiveHandle);
    //     UIParliament.sSingleton.pMenuFixerCamera.Push();
    //     this.director_handle_.Push();
    //     this.current_legend_.SetActive(false);
    //     this.current_legend_.SetActive(this.is_joystick_);
    //     this.exit_button_.SetActive(false);
    //     this.exit_button_.SetActive(!this.is_joystick_);
    // }

    // private void SetDungeonState()
    // {
    //     this.is_first_time_unlocked_ = LocalDatabaseHelper.sSingleton.IsNewDungeonAvailableForFirstTime(out this.first_time_cleared_dungeon_index_);
    //     this.pFirstTimeUnlockedDungeonIndex = (int)((MortaDungeonsEnum)this.first_time_cleared_dungeon_index_).GetNextDungeon();
    // }

    //
    // public enum MortaDungeonsEnum
    // {
    //   Invalid = -2, // 0xFFFFFFFE
    //   Tutorial = -1, // 0xFFFFFFFF
    //   SpiderLair = 0,
    //   GoblinTerritory = 1,
    //   AnayDaia = 2,
    //   Ruins = 5,
    //   CityOfThieves = 6,
    //   Ziggurat = 7,
    //   Forest = 10, // 0x0000000A
    //   Factory = 15, // 0x0000000F
    //   Area30 = 16, // 0x00000010
    //   OU = 20, // 0x00000014
    //   Endless = 30, // 0x0000001E
    //   ProfileTest = 40, // 0x00000028
    // }

    // public enum MortaPlacesEnum
    // {
    //     None = -1,
    //     Tutorial = 100001,
    //     Cave = 0,
    //     WindTemple = 5,
    //     Forest = 10,
    //     Magma = 15,
    //     Temple = 20
    // }

}