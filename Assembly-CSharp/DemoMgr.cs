using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Configuration;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using UnityEngine;

public class DemoMgr : IService
{
	private static DemoMgr s_instance;

	private DemoMode m_mode;

	private Notification m_demoText;

	private bool m_shouldGiveArenaInstruction;

	private bool m_nextTipUnclickable;

	private bool m_nextDemoTipIsNewArenaMatch;

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		HearthstoneApplication.Get().WillReset += WillReset;
		string modeString = GetStoredGameMode();
		if (modeString == null)
		{
			modeString = Vars.Key("Demo.Mode").GetStr("NONE");
		}
		SetModeFromString(modeString);
		WillReset();
		yield break;
	}

	public Type[] GetDependencies()
	{
		return null;
	}

	public void Shutdown()
	{
		s_instance = null;
	}

	private void WillReset()
	{
		if (m_mode == DemoMode.BLIZZ_MUSEUM)
		{
			ApplyBlizzMuseumDemoDefaults();
		}
	}

	public static DemoMgr Get()
	{
		if (s_instance == null)
		{
			s_instance = ServiceManager.Get<DemoMgr>();
		}
		return s_instance;
	}

	private string GetStoredGameMode()
	{
		return null;
	}

	public bool IsDemo()
	{
		return m_mode != DemoMode.NONE;
	}

	public bool IsExpoDemo()
	{
		DemoMode mode = m_mode;
		if ((uint)(mode - 1) <= 11u)
		{
			return true;
		}
		return false;
	}

	public bool IsSocialEnabled()
	{
		switch (m_mode)
		{
		case DemoMode.BLIZZCON_2013:
		case DemoMode.BLIZZCON_2015:
		case DemoMode.BLIZZ_MUSEUM:
		case DemoMode.BLIZZCON_2017_ADVENTURE:
		case DemoMode.BLIZZCON_2017_BRAWL:
		case DemoMode.BLIZZCON_2018_BRAWL:
		case DemoMode.BLIZZCON_2019_BATTLEGROUNDS:
			return false;
		default:
			return true;
		}
	}

	public bool IsCurrencyEnabled()
	{
		DemoMode mode = m_mode;
		if ((uint)(mode - 3) <= 2u || (uint)(mode - 7) <= 5u)
		{
			return false;
		}
		return true;
	}

	public bool IsHubEscMenuEnabled(bool enabledInGameplay)
	{
		switch (m_mode)
		{
		case DemoMode.BLIZZCON_2013:
		case DemoMode.BLIZZCON_2014:
		case DemoMode.BLIZZCON_2015:
		case DemoMode.ANNOUNCEMENT_5_0:
		case DemoMode.BLIZZCON_2016:
		case DemoMode.BLIZZCON_2017_ADVENTURE:
		case DemoMode.BLIZZCON_2017_BRAWL:
		case DemoMode.BLIZZCON_2018_BRAWL:
		case DemoMode.BLIZZCON_2019_BATTLEGROUNDS:
			return enabledInGameplay;
		case DemoMode.BLIZZ_MUSEUM:
			return false;
		default:
			return true;
		}
	}

	public bool CantExitArena()
	{
		if (m_mode == DemoMode.BLIZZCON_2013)
		{
			return true;
		}
		return false;
	}

	public bool ArenaIs1WinMode()
	{
		if (m_mode == DemoMode.BLIZZCON_2013)
		{
			return true;
		}
		return false;
	}

	public bool CanRestartMissions()
	{
		if (m_mode == DemoMode.BLIZZCON_2017_ADVENTURE)
		{
			return false;
		}
		return true;
	}

	public DemoMode GetMode()
	{
		return m_mode;
	}

	public void SetModeFromString(string modeString)
	{
		m_mode = GetModeFromString(modeString);
	}

	public DemoMode GetModeFromString(string modeString)
	{
		try
		{
			return EnumUtils.GetEnum<DemoMode>(modeString, StringComparison.OrdinalIgnoreCase);
		}
		catch (Exception)
		{
			return DemoMode.NONE;
		}
	}

	public void CreateDemoText(string demoText)
	{
		CreateDemoText(demoText, unclickable: false, shouldDoArenaInstruction: false);
	}

	public void CreateDemoText(string demoText, bool unclickable)
	{
		CreateDemoText(demoText, unclickable, shouldDoArenaInstruction: false);
	}

	public void CreateDemoText(string demoText, bool unclickable, bool shouldDoArenaInstruction)
	{
		if (!(m_demoText != null))
		{
			m_shouldGiveArenaInstruction = shouldDoArenaInstruction;
			m_nextTipUnclickable = unclickable;
			GameObject actorObject = AssetLoader.Get().InstantiatePrefab("DemoText.prefab:5749aead2db66ce4d958e44bab4a5219");
			OverlayUI.Get().AddGameObject(actorObject);
			m_demoText = actorObject.GetComponent<Notification>();
			m_demoText.ChangeText(demoText);
			UniversalInputManager.Get().SetSystemDialogActive(active: true);
			actorObject.transform.GetComponentInChildren<PegUIElement>().AddEventListener(UIEventType.RELEASE, RemoveDemoTextDialog);
			if (m_nextTipUnclickable)
			{
				m_nextTipUnclickable = false;
				MakeDemoTextClickable(clickable: false);
			}
		}
	}

	public void NextDemoTipIsNewArenaMatch()
	{
		m_nextDemoTipIsNewArenaMatch = true;
	}

	private void RemoveDemoTextDialog(UIEvent e)
	{
		RemoveDemoTextDialog();
	}

	public void RemoveDemoTextDialog()
	{
		UniversalInputManager.Get().SetSystemDialogActive(active: false);
		UnityEngine.Object.DestroyImmediate(m_demoText.gameObject);
		if (m_shouldGiveArenaInstruction)
		{
			NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, GameStrings.Get("VO_INNKEEPER_FORGE_INST1_19"), "VO_INNKEEPER_FORGE_INST1_19.prefab:a0e06e90b545b274290dad8e442e83d0", 3f);
			m_shouldGiveArenaInstruction = false;
		}
		if (m_nextDemoTipIsNewArenaMatch)
		{
			m_nextDemoTipIsNewArenaMatch = false;
			CreateDemoText(GameStrings.Get("GLUE_BLIZZCON2013_ARENA"), unclickable: false, shouldDoArenaInstruction: true);
		}
	}

	public void MakeDemoTextClickable(bool clickable)
	{
		if (!clickable)
		{
			m_demoText.transform.GetComponentInChildren<BoxCollider>().enabled = false;
			m_demoText.transform.Find("continue").gameObject.SetActive(value: false);
		}
		else
		{
			m_demoText.transform.GetComponentInChildren<BoxCollider>().enabled = true;
			m_demoText.transform.Find("continue").gameObject.SetActive(value: true);
		}
	}

	public void ApplyBlizzMuseumDemoDefaults()
	{
		Options.Get().SetBool(Option.CONNECT_TO_AURORA, val: false);
		Options.Get().SetBool(Option.HAS_SEEN_NEW_CINEMATIC, val: true);
	}

	public IEnumerator CompleteBlizzMuseumDemo()
	{
		yield return new WaitForSeconds(3f);
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLOBAL_DEMO_COMPLETE_HEADER");
		info.m_text = GameStrings.Get("GLOBAL_DEMO_COMPLETE_BODY");
		info.m_showAlertIcon = false;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		info.m_responseCallback = delegate
		{
			HearthstoneApplication.Get().Reset();
		};
		DialogManager.Get().ShowPopup(info);
	}
}
