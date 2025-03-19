using Blizzard.GameService.SDK.Client.Integration;
using Hearthstone;
using Hearthstone.Devices;
using UnityEngine;

public class GameMenuBase
{
	public delegate void ShowCallback();

	public delegate void HideCallback();

	public ShowCallback m_showCallback;

	public HideCallback m_hideCallback;

	private const string OPTIONS_MENU_NAME = "OptionsMenu.prefab:a6e5621068fd7c8429475b3e1a1aa991";

	private OptionsMenu m_optionsMenu;

	public void ShowOptionsMenu()
	{
		if (m_hideCallback != null)
		{
			m_hideCallback();
		}
		if (m_optionsMenu == null)
		{
			GameObject go = AssetLoader.Get().InstantiatePrefab("OptionsMenu.prefab:a6e5621068fd7c8429475b3e1a1aa991");
			m_optionsMenu = go.GetComponent<OptionsMenu>();
			if (m_optionsMenu != null)
			{
				SwitchToOptionsMenu();
			}
		}
		else
		{
			SwitchToOptionsMenu();
		}
	}

	public void DestroyOptionsMenu()
	{
		if (m_optionsMenu != null)
		{
			m_optionsMenu.RemoveHideHandler(OnOptionsMenuHidden);
		}
	}

	public bool UseKoreanRating()
	{
		if (SceneMgr.Get().IsInGame())
		{
			return false;
		}
		bool useKoreanRating = BattleNet.GetAccountCountry() == "KOR";
		if (PlatformSettings.IsMobile() && !useKoreanRating)
		{
			useKoreanRating = DeviceLocaleHelper.GetCountryCode() == "KR";
		}
		return useKoreanRating;
	}

	private void SwitchToOptionsMenu()
	{
		m_optionsMenu.SetHideHandler(OnOptionsMenuHidden);
		m_optionsMenu.Show();
	}

	private void OnOptionsMenuHidden()
	{
		Object.Destroy(m_optionsMenu.gameObject);
		m_optionsMenu = null;
		if (!SceneMgr.Get().IsModeRequested(SceneMgr.Mode.FATAL_ERROR) && !HearthstoneApplication.Get().IsResetting() && BnetBar.Get().AreButtonsEnabled() && m_showCallback != null)
		{
			m_showCallback();
		}
	}
}
