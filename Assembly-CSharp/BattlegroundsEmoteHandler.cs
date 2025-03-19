using System.Collections;
using Hearthstone;
using Hearthstone.Core;
using UnityEngine;

[RequireComponent(typeof(PlayMakerFSM), typeof(Collider))]
public class BattlegroundsEmoteHandler : MonoBehaviour
{
	[SerializeField]
	private BattlegroundsEmoteOption[] m_battlegroundsEmoteOptions;

	[SerializeField]
	private PlayMakerFSM m_visibilityPlayMakerFsm;

	[SerializeField]
	private Collider m_collider;

	private static BattlegroundsEmoteHandler s_instance;

	private BattlegroundsEmoteOption m_mousedOverOption;

	private bool m_emotesShown;

	private float m_timeLastEmoteSent;

	private int m_totalEmotesSent;

	private int m_chainedEmotesSent;

	private bool m_initialized;

	private bool m_isGameStateBusy;

	private int m_emoteOptionsReady;

	private const int EmoteCount = 6;

	private const float NetCacheQueryInterval = 2f;

	private const float MinTimeBetweenEmotes = 4f;

	private const int NumEmotesBeforeConsideredASpammer = 20;

	private const float SpammerMinTimeBetweenEmotes = 15f;

	private const int NumEmotesBeforeConsideredUberSpammer = 25;

	private const float UberSpammerMinTimeBetweenEmotes = 45f;

	private const int NumChainEmotesBeforeConsideredSpam = 2;

	private const float TimeWindowToBeConsideredAChain = 5f;

	private const string InitializeEventName = "INITIALIZE";

	private const string ShowEventName = "SHOW";

	private const string HideEventName = "HIDE";

	public bool IsMouseOverEmoteOption => m_mousedOverOption != null;

	private void Awake()
	{
		s_instance = this;
		if (6 != m_battlegroundsEmoteOptions.Length)
		{
			Debug.LogError(string.Format("{0}: Incorrect number of emote slots found. Expected {1}, counted {2}", "BattlegroundsEmoteHandler", 6, m_battlegroundsEmoteOptions.Length));
		}
		if (m_visibilityPlayMakerFsm == null)
		{
			Debug.LogError("BattlegroundsEmoteHandler: Missing required PlaymakerFSM component");
		}
		if (m_collider == null)
		{
			Debug.LogError("BattlegroundsEmoteHandler: Missing required Collider component");
		}
	}

	private void Start()
	{
		Processor.RunCoroutine(CheckNetCacheForEmoteLoadout());
		HideEmotes(shouldForceHide: true);
		m_isGameStateBusy = GameState.Get()?.IsBusy() ?? false;
		GameState.Get()?.RegisterBusyStateChangedListener(OnBusyStateChanged);
	}

	private void OnDestroy()
	{
		s_instance = null;
		GameState.Get()?.UnregisterBusyStateChangedListener(OnBusyStateChanged);
		BattlegroundsEmoteOption[] battlegroundsEmoteOptions = m_battlegroundsEmoteOptions;
		foreach (BattlegroundsEmoteOption option in battlegroundsEmoteOptions)
		{
			if (option != null)
			{
				option.UnregisterReadyListener(OnBattlegroundsEmoteOptionReady);
			}
		}
	}

	public static BattlegroundsEmoteHandler Get()
	{
		return s_instance;
	}

	public static bool TryGetActiveInstance(out BattlegroundsEmoteHandler handler)
	{
		handler = s_instance;
		if (GameMgr.Get().IsBattlegroundsMatchOrTutorial() && s_instance != null)
		{
			return s_instance.AreEmotesActive();
		}
		return false;
	}

	public bool AreEmotesActive()
	{
		return m_emotesShown;
	}

	public void ShowEmotes()
	{
		if (!m_emotesShown && !m_isGameStateBusy && m_initialized)
		{
			m_visibilityPlayMakerFsm.SendEvent("SHOW");
			m_emotesShown = true;
			m_collider.enabled = true;
		}
	}

	public void HideEmotes(bool shouldForceHide = false)
	{
		if (m_emotesShown || shouldForceHide)
		{
			m_visibilityPlayMakerFsm.SendEvent("HIDE");
			m_mousedOverOption = null;
			m_emotesShown = false;
			m_collider.enabled = false;
		}
	}

	public void HandleMouseOver(BattlegroundsEmoteOption battlegroundsEmoteOption)
	{
		if (!(battlegroundsEmoteOption == null) && !(m_mousedOverOption == battlegroundsEmoteOption))
		{
			if (m_mousedOverOption != null)
			{
				m_mousedOverOption.HandleMouseOut();
			}
			m_mousedOverOption = battlegroundsEmoteOption;
			m_mousedOverOption.HandleMouseOver();
		}
	}

	public void HandleMouseOut()
	{
		if (!(m_mousedOverOption == null))
		{
			m_mousedOverOption.HandleMouseOut();
			m_mousedOverOption = null;
		}
	}

	public void HandleEmoteClicked()
	{
		if (m_emotesShown && !(m_mousedOverOption == null) && !EmoteSpamBlocked())
		{
			m_mousedOverOption.SendBattlegroundsEmote();
			m_totalEmotesSent++;
			ResetTimeSinceLastEmote();
			Processor.RunCoroutine(BeginCooldownTimer());
			HideEmotes();
		}
	}

	private IEnumerator CheckNetCacheForEmoteLoadout()
	{
		NetCache.NetCacheBattlegroundsEmotes battlegroundsEmotes;
		for (battlegroundsEmotes = NetCache.Get()?.GetNetObject<NetCache.NetCacheBattlegroundsEmotes>(); battlegroundsEmotes == null; battlegroundsEmotes = NetCache.Get()?.GetNetObject<NetCache.NetCacheBattlegroundsEmotes>())
		{
			yield return new WaitForSeconds(2f);
		}
		CreateAndBindLoadoutDataModels(battlegroundsEmotes);
	}

	private void CreateAndBindLoadoutDataModels(NetCache.NetCacheBattlegroundsEmotes battlegroundsEmotes)
	{
		BattlegroundsEmoteId[] loadout = battlegroundsEmotes.CurrentLoadout.Emotes;
		if (loadout.Length != m_battlegroundsEmoteOptions.Length)
		{
			Debug.LogError("BattlegroundsEmoteHandler: Emote loadout does not equal available UI slots. Filling slots up to capacity.");
		}
		for (int i = 0; i < loadout.Length && i < m_battlegroundsEmoteOptions.Length; i++)
		{
			BattlegroundsEmoteDbfRecord dbfRecord = GameDbf.BattlegroundsEmote.GetRecord(loadout[i].ToValue());
			BattlegroundsEmoteOption obj = m_battlegroundsEmoteOptions[i];
			obj.BindAndInitializeWidget(dbfRecord);
			obj.InvokeOrRegisterReadyListener(OnBattlegroundsEmoteOptionReady);
		}
	}

	private void OnBattlegroundsEmoteOptionReady()
	{
		if (++m_emoteOptionsReady == m_battlegroundsEmoteOptions.Length)
		{
			InitializePlayMakers();
			m_initialized = true;
		}
	}

	private void InitializePlayMakers()
	{
		m_visibilityPlayMakerFsm.SendEvent("INITIALIZE");
	}

	private IEnumerator BeginCooldownTimer()
	{
		BattlegroundsEmoteOption[] battlegroundsEmoteOptions = m_battlegroundsEmoteOptions;
		for (int i = 0; i < battlegroundsEmoteOptions.Length; i++)
		{
			battlegroundsEmoteOptions[i].SetCooldown(isOnCooldown: true);
		}
		yield return new WaitForSeconds(GetCooldownDuration());
		battlegroundsEmoteOptions = m_battlegroundsEmoteOptions;
		for (int i = 0; i < battlegroundsEmoteOptions.Length; i++)
		{
			battlegroundsEmoteOptions[i].SetCooldown(isOnCooldown: false);
		}
	}

	private bool EmoteSpamBlocked()
	{
		if (GameMgr.Get().IsFriendly() || GameMgr.Get().IsAI())
		{
			return false;
		}
		return Time.time - m_timeLastEmoteSent < GetCooldownDuration();
	}

	private void ResetTimeSinceLastEmote()
	{
		if (Time.time - m_timeLastEmoteSent < 9f)
		{
			m_chainedEmotesSent++;
		}
		else
		{
			m_chainedEmotesSent = 0;
		}
		m_timeLastEmoteSent = Time.time;
	}

	private float GetCooldownDuration()
	{
		if (m_totalEmotesSent >= 25)
		{
			return 45f;
		}
		if (m_totalEmotesSent >= 20 || m_chainedEmotesSent >= 2)
		{
			return 15f;
		}
		return 4f;
	}

	private void OnBusyStateChanged(bool isGameStateBusy, object userData)
	{
		if (m_isGameStateBusy != isGameStateBusy)
		{
			m_isGameStateBusy = isGameStateBusy;
			if (m_emotesShown && isGameStateBusy)
			{
				HideEmotes();
			}
		}
	}
}
