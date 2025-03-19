using System.Collections.Generic;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Services;
using Hearthstone.Login;
using UnityEngine;

public class RegionSwitchMenuController
{
	public struct RegionMenuSettings
	{
		public struct RegionButtonSetting
		{
			public string ButtonLabel { get; set; }

			public BnetRegion Region { get; set; }
		}

		public List<RegionButtonSetting> Buttons { get; set; }

		public BnetRegion CurrentRegion { get; set; }
	}

	private RegionMenu m_regionMenu;

	private const int BUTTON_COUNT = 3;

	private const string WARNING_PREFAB = "RegionSelect.prefab:a29650226d94fae408628b0c5aad1348";

	private const string REGION_MENU_PREFAB = "RegionMenu.prefab:81394e6ea3adb1140a29ff4b44744891";

	private static readonly PlatformDependentValue<float> WARNING_PADDING = new PlatformDependentValue<float>(PlatformCategory.Screen)
	{
		PC = 60f,
		Phone = 80f
	};

	public void ShowRegionMenuWithDefaultSettings()
	{
		if (ShouldSkipRegionSwitch())
		{
			GameUtils.LogoutConfirmation();
			return;
		}
		if (PlatformSettings.LocaleVariant == LocaleVariant.China)
		{
			SwitchRegion(BnetRegion.REGION_CN, requestConfirmation: true);
			return;
		}
		RegionMenuSettings settings = CreateDefaultSettings();
		ShowRegionMenu(settings);
	}

	private bool ShouldSkipRegionSwitch()
	{
		if (ServiceManager.Get<ILoginService>() != null)
		{
			if (!PlatformSettings.IsMobileRuntimeOS)
			{
				return !PlatformSettings.IsSteam;
			}
			return false;
		}
		return true;
	}

	private static RegionMenuSettings CreateDefaultSettings()
	{
		RegionMenuSettings settings = default(RegionMenuSettings);
		settings.CurrentRegion = BattleNet.GetCurrentRegion();
		settings.Buttons = CreateDefaultRegionButtons();
		return settings;
	}

	private static List<RegionMenuSettings.RegionButtonSetting> CreateDefaultRegionButtons()
	{
		return new List<RegionMenuSettings.RegionButtonSetting>(3)
		{
			new RegionMenuSettings.RegionButtonSetting
			{
				Region = BnetRegion.REGION_US,
				ButtonLabel = "GLOBAL_REGION_AMERICAS"
			},
			new RegionMenuSettings.RegionButtonSetting
			{
				Region = BnetRegion.REGION_EU,
				ButtonLabel = "GLOBAL_REGION_EUROPE"
			},
			new RegionMenuSettings.RegionButtonSetting
			{
				Region = BnetRegion.REGION_KR,
				ButtonLabel = "GLOBAL_REGION_ASIA"
			}
		};
	}

	public void ShowRegionMenu(RegionMenuSettings settings)
	{
		if (!(m_regionMenu != null) || !m_regionMenu.IsShown())
		{
			AssetLoader.Get().InstantiatePrefab("RegionMenu.prefab:81394e6ea3adb1140a29ff4b44744891", OnMenuLoaded, settings);
		}
	}

	private void OnMenuLoaded(AssetReference assetRef, GameObject instance, object callbackData)
	{
		m_regionMenu = instance.GetComponent<RegionMenu>();
		if (m_regionMenu == null)
		{
			Log.Login.PrintError("Could not load Region Menu game object");
			Object.Destroy(instance);
		}
		else if (callbackData == null || !(callbackData is RegionMenuSettings settings))
		{
			Log.Login.PrintError("No region menu settings found");
			Object.Destroy(instance);
		}
		else
		{
			SetMenuButtonsAndShow(settings);
		}
	}

	private void SetMenuButtonsAndShow(RegionMenuSettings settings)
	{
		List<UIBButton> buttons = new List<UIBButton>(3);
		foreach (RegionMenuSettings.RegionButtonSetting buttonSettings in settings.Buttons)
		{
			UIBButton button = m_regionMenu.CreateMenuButton(null, buttonSettings.ButtonLabel, delegate
			{
				OnRegionButtonPressed(buttonSettings.Region, settings.CurrentRegion);
			});
			if (buttonSettings.Region == settings.CurrentRegion)
			{
				UberText[] componentsInChildren = button.gameObject.GetComponentsInChildren<UberText>();
				foreach (UberText comp in componentsInChildren)
				{
					if (comp.gameObject.name == "CurrentRegionButtonText")
					{
						comp.SetText(GameStrings.Get(buttonSettings.ButtonLabel));
					}
				}
				button.Flip(faceUp: false);
				button.SetEnabled(enabled: false);
			}
			buttons.Add(button);
		}
		m_regionMenu.SetButtons(buttons);
		m_regionMenu.Show();
	}

	private void OnRegionButtonPressed(BnetRegion selectedRegion, BnetRegion currentRegion)
	{
		m_regionMenu.Hide();
		if (selectedRegion != currentRegion)
		{
			ShowRegionWarningDialog(selectedRegion);
		}
		else
		{
			SwitchRegion(selectedRegion, requestConfirmation: true);
		}
	}

	private void ShowRegionWarningDialog(BnetRegion region)
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get("GLUE_MOBILE_REGION_SELECT_WARNING_HEADER"),
			m_text = GameStrings.Get("GLUE_MOBILE_REGION_SELECT_WARNING"),
			m_showAlertIcon = false,
			m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
			m_responseCallback = OnRegionWarningResponse,
			m_padding = WARNING_PADDING,
			m_responseUserData = region
		};
		DialogManager.Get().ShowPopup(info, OnDialogProcess);
	}

	private bool OnDialogProcess(DialogBase dialog, object userData)
	{
		AlertPopup popup = dialog as AlertPopup;
		if (popup != null)
		{
			((GameObject)GameUtils.InstantiateGameObject("RegionSelect.prefab:a29650226d94fae408628b0c5aad1348", popup.m_body.gameObject)).SetActive(value: true);
		}
		return true;
	}

	private void OnRegionWarningResponse(AlertPopup.Response response, object userData)
	{
		if (response == AlertPopup.Response.CONFIRM)
		{
			BnetRegion region = (BnetRegion)userData;
			SwitchRegion(region, requestConfirmation: false);
		}
	}

	private void SwitchRegion(BnetRegion newRegion, bool requestConfirmation)
	{
		int currentRegion = Options.Get().GetInt(Option.PREFERRED_REGION, -1);
		TelemetryManager.Client().SendRegionSwitched(currentRegion, (int)newRegion);
		Options.Get().SetInt(Option.PREFERRED_REGION, (int)newRegion);
		if (requestConfirmation)
		{
			GameUtils.LogoutConfirmation();
		}
		else
		{
			GameUtils.Logout();
		}
	}
}
