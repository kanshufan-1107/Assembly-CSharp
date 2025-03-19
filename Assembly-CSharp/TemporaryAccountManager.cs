using System;
using System.Collections.Generic;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Core;
using Hearthstone.Login;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

public class TemporaryAccountManager
{
	private struct HealUpFrequencyData
	{
		public int NumGames { get; }

		public int NumHours { get; }

		public HealUpFrequencyData(int numGames, int numHours)
		{
			NumGames = numGames;
			NumHours = numHours;
		}
	}

	[Serializable]
	public class TemporaryAccountData
	{
		[Serializable]
		public class TemporaryAccount
		{
			public string m_temporaryAccountId;

			public string m_battleTag;

			public int m_regionId = -1;

			public bool m_isHealedUp;
		}

		public int m_selectedTemporaryAccountIndex = -1;

		public List<TemporaryAccount> m_temporaryAccounts = new List<TemporaryAccount>();
	}

	public delegate void OnHealUpDialogDismissed();

	public enum HealUpReason
	{
		UNKNOWN,
		FRIENDS_LIST,
		GAME_MENU,
		REAL_MONEY,
		LOCKED_PACK,
		WIN_GAME,
		CRAFT_CARD,
		OPEN_PACK,
		TUTORIAL_PACK_OPEN
	}

	private static readonly HealUpFrequencyData[] s_healUpFrequency = new HealUpFrequencyData[3]
	{
		new HealUpFrequencyData(20, 48),
		new HealUpFrequencyData(10, 3),
		new HealUpFrequencyData(0, 1)
	};

	private static TemporaryAccountManager s_Instance;

	private TemporaryAccountData m_temporaryAccountData;

	private bool m_isTemporaryAccountDataLoaded;

	private TemporaryAccountSignUpPopUp m_signUpPopUp;

	private bool m_isSignUpPopUpLoading;

	private TemporaryAccountSignUpPopUp.PopupTextParameters m_popupArgs;

	private OnHealUpDialogDismissed m_onSignUpDismissedHandler;

	public Action OnHealupSkipReminderDismissed { get; set; }

	public static bool IsTemporaryAccount()
	{
		if (HearthstoneApplication.IsInternal() && Options.Get().HasOption(Option.IS_TEMPORARY_ACCOUNT_CHEAT))
		{
			return Options.Get().GetBool(Option.IS_TEMPORARY_ACCOUNT_CHEAT);
		}
		return BattleNet.IsHeadlessAccount();
	}

	public static TemporaryAccountManager Get()
	{
		if (s_Instance == null)
		{
			s_Instance = new TemporaryAccountManager();
		}
		return s_Instance;
	}

	public void Initialize()
	{
		Array.Sort(s_healUpFrequency, (HealUpFrequencyData lhs, HealUpFrequencyData rhs) => rhs.NumGames - lhs.NumGames);
		if (IsTemporaryAccount())
		{
			Processor.QueueJob("TemporaryAccountManager.AddFakeBooster", Job_AddFakeBooster(), JobFlags.StartImmediately);
		}
	}

	private IEnumerator<IAsyncJobResult> Job_AddFakeBooster()
	{
		yield return new WaitForNetCacheObject<NetCache.NetCacheBoosters>();
		NetCache.NetCacheBoosters netObject = NetCache.Get().GetNetObject<NetCache.NetCacheBoosters>();
		int signupIncentiveBoosterDbId = 18;
		NetCache.BoosterStack incentiveBoosterStackToInsert = new NetCache.BoosterStack
		{
			Id = signupIncentiveBoosterDbId,
			Count = 1
		};
		netObject.BoosterStacks.Add(incentiveBoosterStackToInsert);
	}

	private void LoadTemporaryAccountData()
	{
		if (!m_isTemporaryAccountDataLoaded)
		{
			m_temporaryAccountData = null;
			m_isTemporaryAccountDataLoaded = true;
		}
	}

	public string GetSelectedTemporaryAccountId()
	{
		Log.TemporaryAccount.Print("Get selected Temporary Account Id");
		if (!m_isTemporaryAccountDataLoaded)
		{
			LoadTemporaryAccountData();
		}
		if (m_temporaryAccountData == null)
		{
			Log.TemporaryAccount.PrintWarning("Unable to load temporary account data!");
			return null;
		}
		if (m_temporaryAccountData.m_selectedTemporaryAccountIndex == -1)
		{
			Log.TemporaryAccount.PrintWarning("No selected temporary account!");
			return null;
		}
		string temporaryAccountId = m_temporaryAccountData.m_temporaryAccounts[m_temporaryAccountData.m_selectedTemporaryAccountIndex].m_temporaryAccountId;
		if (!string.IsNullOrEmpty(temporaryAccountId))
		{
			return temporaryAccountId;
		}
		return null;
	}

	public void DeleteTemporaryAccountData()
	{
		Log.TemporaryAccount.PrintWarning("Deleting Temporary Account Data!");
		m_temporaryAccountData = new TemporaryAccountData();
		Options.Get().DeleteOption(Option.TEMPORARY_ACCOUNT_DATA);
		CloudStorageManager.RemoveObject("TEMPORARY_ACCOUNT_DATA");
	}

	public bool ShowHealUpDialog(string header, string body, HealUpReason reason, bool userTriggered, OnHealUpDialogDismissed onSignUpHandler)
	{
		TemporaryAccountSignUpPopUp.PopupTextParameters popupTextParameters = default(TemporaryAccountSignUpPopUp.PopupTextParameters);
		popupTextParameters.Header = header;
		popupTextParameters.Body = body;
		TemporaryAccountSignUpPopUp.PopupTextParameters popupParams = popupTextParameters;
		return ShowHealUpDialog(popupParams, reason, userTriggered, onSignUpHandler);
	}

	public bool ShowHealUpDialog(TemporaryAccountSignUpPopUp.PopupTextParameters popupArgs, HealUpReason reason, bool userTriggered, OnHealUpDialogDismissed onSignUpDismissedHandler)
	{
		if (!IsTemporaryAccount() || !GameUtils.IsAnyTutorialComplete())
		{
			return false;
		}
		if (!userTriggered)
		{
			if (!HasBeenLongEnoughSinceLastHealupEvent())
			{
				return false;
			}
			Options.Get().SetLong(Option.LAST_HEAL_UP_EVENT_DATE, DateTime.Now.Ticks);
		}
		m_onSignUpDismissedHandler = onSignUpDismissedHandler;
		if (m_signUpPopUp == null)
		{
			if (!m_isSignUpPopUpLoading)
			{
				m_isSignUpPopUpLoading = true;
				AssetLoader.Get().InstantiatePrefab("TemporaryAccountSignUp.prefab:14791f0c7af5c6f4480fc78ab36c81bc", ShowSignUpPopUp);
			}
			m_popupArgs = popupArgs;
			return true;
		}
		m_signUpPopUp.Show(popupArgs, OnHealUpProcessCancelled);
		return true;
	}

	private bool HasBeenLongEnoughSinceLastHealupEvent()
	{
		long lastHealUpEventTicks = Options.Get().GetLong(Option.LAST_HEAL_UP_EVENT_DATE);
		DateTime nowTimeDate = DateTime.Now;
		if (lastHealUpEventTicks != 0L)
		{
			int totalWins = GetTotalWins();
			DateTime lastHealUpEventTimeDate = new DateTime(lastHealUpEventTicks);
			TimeSpan delta = nowTimeDate - lastHealUpEventTimeDate;
			int hoursBetween = GetNumHoursBetweenHealupPrompt(totalWins);
			if (delta.TotalHours < (double)hoursBetween)
			{
				return false;
			}
		}
		return true;
	}

	public bool ShowEarnCardEventHealUpDialog(HealUpReason reason)
	{
		return ShowHealUpDialog(GameStrings.Get("GLUE_TEMPORARY_ACCOUNT_DIALOG_HEADER_03"), GameStrings.Get("GLUE_TEMPORARY_ACCOUNT_DIALOG_BODY_01"), reason, userTriggered: false, null);
	}

	public bool ShowHealUpDialogWithReminderOnSkip(HealUpReason reason)
	{
		return ShowHealUpDialog(GameStrings.Get("GLUE_TEMPORARY_ACCOUNT_DIALOG_HEADER_01"), GameStrings.Get("GLUE_TEMPORARY_ACCOUNT_DIALOG_BODY_03"), reason, userTriggered: true, ShowHealupSkipReminder);
	}

	public void ShowHealUpPage(HealUpReason reason, Action<bool> onDismissed = null)
	{
		ShowHealUpPage(onDismissed);
	}

	public void ShowHealUpPage(Action<bool> onDismissed = null)
	{
		ILoginService loginService = ServiceManager.Get<ILoginService>();
		if (loginService != null)
		{
			Log.TemporaryAccount.PrintDebug("Using Login Service for account heal up");
			loginService.HealupCurrentTemporaryAccount(onDismissed);
		}
		else
		{
			Log.TemporaryAccount.PrintError("Login Service null when trying to heal up temporary account");
		}
	}

	public void ShowMergeAccountPage(Action<bool> onDismissed = null)
	{
		ILoginService loginService = ServiceManager.Get<ILoginService>();
		if (loginService != null)
		{
			Log.TemporaryAccount.PrintDebug("Using Login Service for temporary account merge");
			loginService.MergeCurrentTemporaryAccount(onDismissed);
		}
		else
		{
			Log.TemporaryAccount.PrintError("Login Service null when trying to merge temporary account");
		}
	}

	private void ShowSignUpPopUp(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_signUpPopUp = go.GetComponent<TemporaryAccountSignUpPopUp>();
		m_signUpPopUp.Show(m_popupArgs, OnHealUpProcessCancelled);
		m_isSignUpPopUpLoading = false;
	}

	private void OnHealUpProcessCancelled()
	{
		if (m_onSignUpDismissedHandler != null)
		{
			m_onSignUpDismissedHandler();
			m_onSignUpDismissedHandler = null;
		}
	}

	private int GetTotalWins()
	{
		int totalWins = 0;
		NetCache.NetCachePlayerRecords cachedPlayerRecords = NetCache.Get()?.GetNetObject<NetCache.NetCachePlayerRecords>();
		if (cachedPlayerRecords?.Records == null)
		{
			return totalWins;
		}
		foreach (NetCache.PlayerRecord record in cachedPlayerRecords.Records)
		{
			if (record.Data == 0)
			{
				switch (record.RecordType)
				{
				case GameType.GT_VS_AI:
				case GameType.GT_ARENA:
				case GameType.GT_RANKED:
				case GameType.GT_CASUAL:
				case GameType.GT_TAVERNBRAWL:
				case GameType.GT_FSG_BRAWL:
				case GameType.GT_FSG_BRAWL_2P_COOP:
					totalWins += record.Wins;
					break;
				}
			}
		}
		return totalWins;
	}

	public void ShowHealupSkipReminder()
	{
		WidgetInstance tempAccountPopup = WidgetInstance.Create("TempAccountAlert.prefab:cc78a5abd9122f948ad9d7d22a4a53be");
		tempAccountPopup.RegisterReadyListener(delegate
		{
			OverlayUI.Get().AddGameObject(tempAccountPopup.gameObject);
			UIContext.GetRoot().ShowPopup(tempAccountPopup.gameObject);
			tempAccountPopup.RegisterEventListener(delegate(string eventName)
			{
				if (eventName == "SHRINK_DONE")
				{
					OnHealupSkipReminderDismissed?.Invoke();
					OnHealupSkipReminderDismissed = null;
					UIContext.GetRoot().DismissPopup(tempAccountPopup.gameObject);
					UnityEngine.Object.Destroy(tempAccountPopup.gameObject);
				}
			});
		});
	}

	private static int GetNumHoursBetweenHealupPrompt(int totalGames)
	{
		int hoursBetween = 1;
		HealUpFrequencyData[] array = s_healUpFrequency;
		for (int i = 0; i < array.Length; i++)
		{
			HealUpFrequencyData data = array[i];
			if (totalGames >= data.NumGames)
			{
				hoursBetween = data.NumHours;
				break;
			}
		}
		return hoursBetween;
	}
}
