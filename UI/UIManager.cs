using System;
using System.Collections.Generic;
using Altar.Utilities;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Zyklus.UI;
using Zyklus;

namespace ArchipelagoRandomizer.UI;

//As it turns out, they use they own framework for UI (can't blame them), but it means 2 things:
//	1. it's harder to get into - it's manageable mostly it's looking for stuff like TextFieldUIController and filling it with data
//	2. (the actual problem) without modifications TextFields cannot be selected/typed into.
//		even TextFieldUIController doesn't allow to type into the field - this class it's not even used so I can't look into implementation
//Leaving this class tho if someone, someday would like to break into this UI and eventSystem and make proper AP GUI - using GUIManager for now
public class UIManager
{
	public static UIManager sSingleton;
	public TMP_Text pTextBox;
	public ScrollRect pScrollRect;
	public GameObject pObjectToDisable;
	public Sprite pButtonSprite;
	public bool pShowChatAlways
	{
		get { return showChatAlways_; }
		set
		{
			showChatAlways_ = value;
			SetChatBoxActive(value, ChatBoxSetCause.AlwaysShowStateChange);
		}
	}

	private bool showChatAlways_;

	private const float showInGameTime_ = 8f;
	private float showInGameTimer_ = 8f;

	public UIManager()
	{
		sSingleton = this;
	}

	public void Update()
	{
		if (showInGameTimer_ > 0)
		{
			showInGameTimer_ -= Time.deltaTime2;
			if (showInGameTimer_ <= 0)
			{
				SetChatBoxActive(false, ChatBoxSetCause.TimerExpired);
			}
		}
	}

	public void Init()
	{
		if (pTextBox != null)
			return;

		CreateAPTextWindowOnCanvas(GameObject.Find("Always Overlay").GetComponent<Canvas>());

		APLogs.OnNewLogMessage -= OnNewLogMessage;
		APLogs.OnNewLogMessage += OnNewLogMessage;

		OnNewLogMessage();
	}

	private void OnNewLogMessage()
	{

		SetChatBoxActive(true, ChatBoxSetCause.NewMessageReceived);

		pTextBox.text = APLogs.sSingleton.ScrollText;
		pScrollRect.normalizedPosition = new Vector2(0, 0);

		showInGameTimer_ = showInGameTime_;
	}

	public void SetChatBoxActive(bool state, ChatBoxSetCause cause)
	{
		if (pShowChatAlways || General.sIsCeaseFireInProgress) // or game is paused - pause menu is not good indicator...
		{
			if (cause == ChatBoxSetCause.FloorLoadingEnd)
			{
				pObjectToDisable.SetActive(state);
				return;
			}

			pObjectToDisable.SetActive(true);
			return;
		}

		if (cause == ChatBoxSetCause.AlwaysShowStateChange || cause == ChatBoxSetCause.Other)
		{
			if (showInGameTimer_ > 0)
			{
				pObjectToDisable.SetActive(true);
				return;
			}
			else
			{
				pObjectToDisable.SetActive(state);
				return;
			}
		}

		pObjectToDisable.SetActive(state);

	}

	public enum ChatBoxSetCause
	{
		AlwaysShowStateChange,
		TimerExpired,
		NewMessageReceived,
		FloorLoadingEnd,
		Other
	}

	public void SetSprites()
	{
		pButtonSprite = null;
	}

	private void CreateAPTextWindowOnCanvas(Canvas canvas)
	{
		GameObject panel = new GameObject("APConnectionDialogUI");
		panel.transform.SetParent(canvas.transform);

		CreateBaseRect(panel, 300, 260, 0, 0);

		panel.transform.localPosition = new Vector3(350, -90, 0);
		panel.transform.localScale = new Vector3(1, 1, 1);

		var scrollRect = panel.AddComponent<ScrollRect>();
		scrollRect.horizontal = false;
		scrollRect.vertical = true;
		scrollRect.movementType = ScrollRect.MovementType.Clamped;
		scrollRect.scrollSensitivity = 20f;

		var viewportGO = new GameObject("Viewport");
		var viewportRect = viewportGO.AddComponent<RectTransform>();
		viewportRect.SetParent(panel.transform, false);
		viewportRect.anchorMin = Vector2.zero;
		viewportRect.anchorMax = Vector2.one;
		viewportRect.pivot = new Vector2(0, 1);
		viewportRect.offsetMin = Vector2.zero;
		viewportRect.offsetMax = Vector2.zero;
		viewportGO.AddComponent<RectMask2D>(); // or Image + Mask
		scrollRect.viewport = viewportRect;


		var textGameObject = new GameObject("TextBox");
		var textRect = textGameObject.AddComponent<RectTransform>();
		var textBox = textGameObject.AddComponent<TextMeshProUGUI>();
		textRect.SetParent(viewportRect, false);

		textRect.anchorMin = new Vector2(0, 1);
		textRect.anchorMax = new Vector2(1, 1);   // stretch horizontally with viewport width
		textRect.pivot = new Vector2(0, 1);
		textRect.anchoredPosition = Vector2.zero;
		textRect.sizeDelta = new Vector2(0, 0);   // height driven by ContentSizeFitter

		// TMP sizing
		textBox.fontSize = 14;
		textBox.alignment = TextAlignmentOptions.TopLeft;
		textBox.enableWordWrapping = true;
		textBox.margin = new Vector4(8, 8, 8, 8); // small padding from the edges

		// Grow vertically to fit content -> this is what the ScrollRect scrolls
		var fitter = textGameObject.AddComponent<ContentSizeFitter>();
		fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained; // width = viewport width
		fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

		scrollRect.content = textRect;

		pTextBox = textBox;
		pObjectToDisable = panel;
		pScrollRect = scrollRect;
	}

	private void CreateBaseRect(GameObject panel, float sizeX, float sizeY, float anchoredPosX, float anchoredPosY)
	{
		RectTransform panelRect = panel.AddComponent<RectTransform>();
		panelRect.sizeDelta = new Vector2(sizeX, sizeY);
		panelRect.anchorMin = new Vector2(0, 1);
		panelRect.anchorMax = new Vector2(0, 1);
		panelRect.pivot = new Vector2(0, 1);
		panelRect.anchoredPosition = new Vector2(anchoredPosX, anchoredPosY);

		Image panelImage = panel.AddComponent<Image>();
		if (pButtonSprite != null)
			panelImage.sprite = pButtonSprite;
		else
			panelImage.color = new(0.15f, 0.15f, 0.15f, 0.80f);

	}




	// Because of how game handles input connection can't be don't with this method
	// Leaving because MAYBE sometime in the future will overcome this COPIUM

	private UIPanel CreateAPConnectionDialogUI(MenuBase menu)
	{
		GameObject panel = new GameObject("APConnectionDialogUI");

		var panelUI = panel.AddComponent<UIPanel>();

		CreateBaseRect(panel, 450, 350, 550, -300);
		var mainHorizontalGroup = CreateGroup<HorizontalLayoutGroup>(panel, "MainHorizontalGroup", new Vector2(400, 350), true, false);

		var array = CreateTextBoxes(mainHorizontalGroup.gameObject, menu);
		CreateButton(mainHorizontalGroup.gameObject);

		panelUI.SetFieldValue<UIElement[]>("manual_elements_", array);
		panelUI.SetFieldValue("menu_", menu);
		panelUI.SetFieldValue("wrap_x_", true);
		panelUI.SetFieldValue("wrap_y_", true);

		return panelUI;



	}

	private UIElement[] CreateTextBoxes(GameObject parent, MenuBase menu)
	{
		var verticalTextGroup = CreateGroup<VerticalLayoutGroup>(parent, "VerticalTextGroup", new Vector2(160, 350), false, true);
		TextFieldUIController addressTextField = AddTextBox(verticalTextGroup.gameObject, "AddressTextField", menu);
		TextFieldUIController slotNameTextField = AddTextBox(verticalTextGroup.gameObject, "SlotNameTextField", menu);
		TextFieldUIController passwordTextField = AddTextBox(verticalTextGroup.gameObject, "PasswordTextField", menu);
		UIElement[] array = { addressTextField, slotNameTextField, passwordTextField };
		return array;
	}

	private void CreateButton(GameObject parent)
	{
		var verticalTextGroup = CreateGroup<VerticalLayoutGroup>(parent, "VerticalTextGroup", new Vector2(160, 350), false, true);

		CreateButton(verticalTextGroup.gameObject, OnConnectButtonClicked);
	}

	private void CreateButton(GameObject parent, UnityAction onClickAction)
	{
		GameObject buttonObj = new GameObject("ConnectButton");
		buttonObj.transform.SetParent(parent.transform, false);

		RectTransform panelRect = buttonObj.AddComponent<RectTransform>();
		panelRect.sizeDelta = new Vector2(160, 100);

		Image btnImage = buttonObj.AddComponent<Image>();
		if (pButtonSprite != null)
			btnImage.sprite = pButtonSprite;
		else
			btnImage.color = new(0.15f, 0.85f, 0.15f, 0.95f);

		var button = buttonObj.AddComponent<Button>();
		button.onClick.AddListener(onClickAction);
		button.targetGraphic = btnImage;
	}

	private void OnConnectButtonClicked()
	{
		Plugin.Logger.LogInfo("button clicked");
	}

	private TextFieldUIController AddTextBox(GameObject parent, string name, MenuBase menu)
	{
		GameObject inputFieldObj = new GameObject(name);
		inputFieldObj.transform.SetParent(parent.transform, false);

		RectTransform panelRect = inputFieldObj.AddComponent<RectTransform>();
		panelRect.sizeDelta = new Vector2(160, 30);

		Image bgImage = inputFieldObj.AddComponent<Image>();
		bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

		var inputField = inputFieldObj.AddComponent<TMP_InputField>();
		inputField.targetGraphic = bgImage;

		inputField.interactable = true;

		inputFieldObj.SetActive(false);

		TextFieldUIController uIController = inputFieldObj.AddComponent<TextFieldUIController>();
		Plugin.Logger.LogWarning("kisiel");
		uIController.SetFieldValue("menu_object_", menu.gameObject);
		Plugin.Logger.LogWarning("kisiel1");
		uIController.SetFieldValue("highlight_overlay_", inputFieldObj.transform);
		Plugin.Logger.LogWarning("kisiel2");
		inputFieldObj.SetActive(true);
		uIController.SetPrivateFieldInBase("navigations_", new List<UINavigationData>());
		Plugin.Logger.LogWarning("kisiel3");
		// ReadOnlyCollection<UINavigationData> list = new(new List<UINavigationData>());
		// ((UIElement)uIController).SetFieldValue("readonly_navigations_", list);


		inputField.onSelect.AddListener((string text) =>
		{
			Debug.Log("Input field selected!");
		});
		inputField.onValueChanged.AddListener((string newText) =>
		{
			Debug.Log("Typing: " + newText);
		});

		GameObject textObj = new GameObject("Text");
		textObj.transform.SetParent(inputFieldObj.transform, false);

		TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
		text.text = "kisiel"; // empty initially
		text.fontSize = 14;
		text.color = Color.white;
		text.alignment = TextAlignmentOptions.Left;

		RectTransform textRect = textObj.GetComponent<RectTransform>();
		textRect.sizeDelta = new Vector2(160, 30);
		textRect.anchoredPosition = Vector2.zero;

		inputField.textComponent = text;

		return uIController;
	}

	private static T CreateGroup<T>(GameObject parent, string name, Vector2 size, bool expandHeight, bool expandWidth) where T : HorizontalOrVerticalLayoutGroup
	{
		GameObject groupObj = new GameObject(name);
		groupObj.transform.SetParent(parent.transform, false);

		RectTransform rect = groupObj.AddComponent<RectTransform>();
		rect.sizeDelta = size;

		T group = groupObj.AddComponent<T>();
		group.childControlHeight = false;
		group.childControlWidth = false;

		group.childForceExpandHeight = expandHeight;
		group.childForceExpandWidth = expandWidth;

		return group;
	}

	private void EnsureEventSystem()
	{
		if (GameObject.FindObjectOfType<EventSystem>() == null)
		{
			GameObject eventSystemObj = new GameObject("EventSystem");
			eventSystemObj.AddComponent<EventSystem>();
			eventSystemObj.AddComponent<StandaloneInputModule>();
		}
	}

	private void OnPostPush()
	{
		// OnMorningStarted - to show/close ap ui at proper times
		// else if (event_code == (int)UIManager_EventsEnum.SHOWING_CHARACTER_SELECT_MENU_REQUESTED)
		// {
		//     var canvases = UI.UIManager.sSingleton.pMenus;

		//     var canvas = CharacterSelect.sActiveMenu;
		//     Plugin.Logger.LogWarning("kisiel2");
		//     if (canvas != null)
		//     {
		//         Plugin.Logger.LogWarning("kisiel3");
		//         if (!canvases.Contains(canvas))
		//         {
		//             canvases.Add(canvas);
		//             Plugin.Logger.LogWarning("kisiel4");
		//             UI.UIManager.sSingleton.DrawOnCanvas(canvas);
		//         }
		//         else
		//         {
		//             UI.UIManager.sSingleton.pPanels[canvas].SetActive(true);
		//         }
		//     }
		// }
		// else if (event_code == (int)UIManager_EventsEnum.HIDING_CHARACTER_SELECT_MENU_REQUESTED)
		// {
		//     var menu = CharacterSelect.sActiveMenu;
		//     if (menu != null)
		//         if (UI.UIManager.sSingleton.pPanels.TryGetValue(menu, out var value))
		//             value.SetActive(false);
		// }
		// else if (event_code == (int)UIManager_EventsEnum.SHOWING_PAUSE_MENU_REQUESTED)
		// {
		//     var menus = UI.UIManager.sSingleton.pMenus;

		//     var menu = PauseMenuComponent.sSingleton; // pCanvas is not set
		//     if (menu == null)
		//         return;
		//     if (!menus.Contains(menu))
		//     {
		//         menus.Add(menu);
		//         UI.UIManager.sSingleton.DrawOnCanvas(menu);
		//     }
		//     else
		//     {
		//         UI.UIManager.sSingleton.pPanels[menu].SetActive(true);
		//     }
		// }
		// else if (event_code == (int)UIManager_EventsEnum.HIDING_PAUSE_MENU_REQUESTED)
		// {
		//     var menu = PauseMenuComponent.sSingleton;
		//     if (menu != null)
		//         if (UI.UIManager.sSingleton.pPanels.TryGetValue(menu, out var value))
		//             value.SetActive(false);
		// }
	}
}