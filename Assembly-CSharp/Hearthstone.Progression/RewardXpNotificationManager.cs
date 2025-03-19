using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Assets;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.Core;
using PegasusUtil;

namespace Hearthstone.Progression;

public class RewardXpNotificationManager : IService
{
	public static readonly AssetReference END_OF_GAME_QUEST_REWARD_FLOW_PREFAB = new AssetReference("QuestXPReward.prefab:c545a0b6333b7eb4a8ce2c6d6ac6c54b");

	private List<RewardTrackXpChange> m_xpChanges = new List<RewardTrackXpChange>();

	private Action m_callback;

	private ScreenEffectsHandle m_screenEffectsHandle;

	public QuestXpRewardHandler m_xpRewardHandler { get; private set; }

	public bool IsReady
	{
		get
		{
			if (m_xpRewardHandler != null && m_xpRewardHandler.m_widget != null)
			{
				return m_xpRewardHandler.m_widget.IsReady;
			}
			return false;
		}
	}

	public bool HasXpGainsToShow
	{
		get
		{
			if (m_xpChanges == null)
			{
				return false;
			}
			return m_xpChanges.Count > 0;
		}
	}

	public bool IsShowingXpGains { get; private set; }

	public bool JustShowGameXp { get; private set; }

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		HearthstoneApplication.Get().WillReset += WillReset;
		serviceLocator.Get<Network>().RegisterNetHandler(RewardTrackXpNotification.PacketID.ID, OnRewardTrackXpNotification);
		JustShowGameXp = false;
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[2]
		{
			typeof(Network),
			typeof(QuestManager)
		};
	}

	public void Shutdown()
	{
	}

	private void WillReset()
	{
		m_xpChanges.Clear();
		JustShowGameXp = false;
	}

	public static RewardXpNotificationManager Get()
	{
		return ServiceManager.Get<RewardXpNotificationManager>();
	}

	public void InitEndOfGameFlow(Action callback)
	{
		if (m_xpRewardHandler == null)
		{
			m_xpRewardHandler = new QuestXpRewardHandler();
		}
		m_xpRewardHandler.InitWidget(callback);
	}

	public void ShowRewardTrackXpGains(Action callback, bool justShowGameXp = false)
	{
		IsShowingXpGains = true;
		m_callback = callback;
		JustShowGameXp = justShowGameXp;
		Processor.RunCoroutine(ShowRewardTrackXpGainsWhenReady());
	}

	public void TerminateEarly()
	{
		if (IsShowingXpGains)
		{
			m_xpRewardHandler?.Terminate();
			m_xpRewardHandler = null;
			IsShowingXpGains = false;
			m_xpChanges?.Clear();
		}
	}

	public void ContinueNotifications()
	{
		if (!JustShowGameXp)
		{
			return;
		}
		JustShowGameXp = false;
		if (m_xpRewardHandler != null)
		{
			if (!m_xpRewardHandler.IsShowingGameXp)
			{
				m_xpRewardHandler.ContinueNotifications();
			}
		}
		else
		{
			TerminateEarly();
		}
	}

	private IEnumerator ShowRewardTrackXpGainsWhenReady()
	{
		while (m_xpRewardHandler == null || !m_xpRewardHandler.m_isReady)
		{
			yield return null;
		}
		m_xpRewardHandler.Initialize(m_xpChanges, delegate
		{
			IsShowingXpGains = false;
			m_callback?.Invoke();
		});
		m_xpChanges.Clear();
	}

	private void OnRewardTrackXpNotification()
	{
		RewardTrackXpNotification message = Network.Get().GetRewardTrackXpNotification();
		if (message == null)
		{
			return;
		}
		foreach (RewardTrackXpChange xpChange in message.XpChange)
		{
			m_xpChanges.Add(xpChange);
		}
	}

	private void FlushGains(Action callback)
	{
		InitEndOfGameFlow(delegate
		{
			ShowRewardTrackXpGains(callback);
		});
	}

	public void ShowXpNotificationsImmediate(Action callback)
	{
		if (HasXpGainsToShow)
		{
			m_screenEffectsHandle.StartEffect(ScreenEffectParameters.BlurVignetteDesaturatePerspective);
			FlushGains(delegate
			{
				m_screenEffectsHandle.StopEffect();
				callback?.Invoke();
			});
		}
		else
		{
			callback?.Invoke();
		}
	}

	public string GetRewardTrackDebugHudString()
	{
		StringBuilder sb = new StringBuilder();
		foreach (RewardTrackXpChange message in m_xpChanges)
		{
			sb.AppendFormat("Source={0}(id:{1}) Lvl:{2} Xp:{3} -> Lvl:{4} Xp:{5}", Enum.GetName(typeof(RewardSourceType), message.RewardSourceType), message.RewardSourceId, message.PrevLevel, message.PrevXp, message.CurrLevel, message.CurrXp);
			sb.AppendLine();
		}
		return sb.ToString();
	}

	public void DebugSimScenario(int scenarioId)
	{
		RewardTrackXpChange eogXp = new RewardTrackXpChange();
		eogXp.PrevLevel = 1;
		eogXp.CurrLevel = 1;
		eogXp.PrevXp = 0;
		eogXp.CurrXp = 40;
		eogXp.RewardSourceType = 5;
		eogXp.RewardTrackType = 1;
		int questId = 114;
		RewardTrackXpChange dontLevelQuest = new RewardTrackXpChange();
		dontLevelQuest.PrevLevel = 1;
		dontLevelQuest.CurrLevel = 1;
		dontLevelQuest.PrevXp = 40;
		dontLevelQuest.CurrXp = 90;
		dontLevelQuest.RewardSourceId = questId;
		dontLevelQuest.RewardSourceType = 1;
		dontLevelQuest.RewardTrackType = 1;
		questId = 28;
		RewardTrackXpChange levelUpQuest = new RewardTrackXpChange();
		levelUpQuest.PrevLevel = 1;
		levelUpQuest.CurrLevel = 2;
		levelUpQuest.PrevXp = 90;
		levelUpQuest.CurrXp = 90;
		levelUpQuest.RewardSourceId = questId;
		levelUpQuest.RewardSourceType = 1;
		levelUpQuest.RewardTrackType = 1;
		RewardTrackXpChange eogXpMegaLevel = new RewardTrackXpChange();
		eogXpMegaLevel.PrevLevel = 2;
		eogXpMegaLevel.CurrLevel = 7;
		eogXpMegaLevel.PrevXp = 90;
		eogXpMegaLevel.CurrXp = 23;
		eogXpMegaLevel.RewardSourceType = 5;
		eogXpMegaLevel.RewardTrackType = 1;
		RewardTrackXpChange bgEogXp = new RewardTrackXpChange();
		bgEogXp.PrevLevel = 18;
		bgEogXp.CurrLevel = 18;
		bgEogXp.PrevXp = 0;
		bgEogXp.CurrXp = 40;
		bgEogXp.RewardSourceType = 5;
		bgEogXp.RewardTrackType = 2;
		questId = 114;
		RewardTrackXpChange bgDontLevelQuest = new RewardTrackXpChange();
		bgDontLevelQuest.PrevLevel = 18;
		bgDontLevelQuest.CurrLevel = 19;
		bgDontLevelQuest.PrevXp = 40;
		bgDontLevelQuest.CurrXp = 50;
		bgDontLevelQuest.RewardSourceId = questId;
		bgDontLevelQuest.RewardSourceType = 1;
		bgDontLevelQuest.RewardTrackType = 2;
		questId = 28;
		RewardTrackXpChange bgLevelUpQuest = new RewardTrackXpChange();
		bgLevelUpQuest.PrevLevel = 18;
		bgLevelUpQuest.CurrLevel = 19;
		bgLevelUpQuest.PrevXp = 50;
		bgLevelUpQuest.CurrXp = 50;
		bgLevelUpQuest.RewardSourceId = questId;
		bgLevelUpQuest.RewardSourceType = 1;
		bgLevelUpQuest.RewardTrackType = 2;
		RewardTrackXpChange bgEogXpMegaLevel = new RewardTrackXpChange();
		bgEogXpMegaLevel.PrevLevel = 18;
		bgEogXpMegaLevel.CurrLevel = 21;
		bgEogXpMegaLevel.PrevXp = 50;
		bgEogXpMegaLevel.CurrXp = 20;
		bgEogXpMegaLevel.RewardSourceType = 5;
		bgEogXpMegaLevel.RewardTrackType = 2;
		RewardTrackXpChange eventEogXp = new RewardTrackXpChange();
		eventEogXp.PrevLevel = 1;
		eventEogXp.CurrLevel = 1;
		eventEogXp.PrevXp = 0;
		eventEogXp.CurrXp = 40;
		eventEogXp.RewardSourceType = 5;
		eventEogXp.RewardTrackType = 7;
		questId = 407;
		RewardTrackXpChange eventLevelUpQuest = new RewardTrackXpChange();
		eventLevelUpQuest.PrevLevel = 2;
		eventLevelUpQuest.CurrLevel = 3;
		eventLevelUpQuest.PrevXp = 50;
		eventLevelUpQuest.CurrXp = 50;
		eventLevelUpQuest.RewardSourceId = questId;
		eventLevelUpQuest.RewardSourceType = 1;
		eventLevelUpQuest.RewardTrackType = 7;
		RewardTrackXpChange apprenticeEogXp = new RewardTrackXpChange();
		apprenticeEogXp.PrevLevel = 1;
		apprenticeEogXp.CurrLevel = 1;
		apprenticeEogXp.PrevXp = 0;
		apprenticeEogXp.CurrXp = 20;
		apprenticeEogXp.RewardSourceType = 5;
		apprenticeEogXp.RewardTrackType = 8;
		questId = 660;
		RewardTrackXpChange apprenticeLevelUpQuest = new RewardTrackXpChange();
		apprenticeLevelUpQuest.PrevLevel = 1;
		apprenticeLevelUpQuest.CurrLevel = 2;
		apprenticeLevelUpQuest.PrevXp = 20;
		apprenticeLevelUpQuest.CurrXp = 20;
		apprenticeLevelUpQuest.RewardSourceId = questId;
		apprenticeLevelUpQuest.RewardSourceType = 1;
		apprenticeLevelUpQuest.RewardTrackType = 8;
		switch (scenarioId)
		{
		case 1:
			m_xpChanges.Add(eogXp);
			break;
		case 2:
			m_xpChanges.Add(dontLevelQuest);
			break;
		case 3:
			m_xpChanges.Add(eogXp);
			m_xpChanges.Add(dontLevelQuest);
			break;
		case 4:
			m_xpChanges.Add(eogXp);
			m_xpChanges.Add(dontLevelQuest);
			m_xpChanges.Add(levelUpQuest);
			break;
		case 5:
			eogXp.PrevXp = 0;
			eogXp.CurrXp = 90;
			eogXp.PrevLevel = 1;
			eogXp.CurrLevel = 2;
			m_xpChanges.Add(eogXp);
			levelUpQuest.PrevLevel = 2;
			levelUpQuest.CurrLevel = 5;
			levelUpQuest.CurrXp = 40;
			m_xpChanges.Add(levelUpQuest);
			dontLevelQuest.PrevLevel = 5;
			dontLevelQuest.CurrLevel = 5;
			m_xpChanges.Add(dontLevelQuest);
			break;
		case 6:
			eogXp.CurrLevel = 2;
			m_xpChanges.Add(eogXp);
			break;
		case 7:
			m_xpChanges.Add(eogXpMegaLevel);
			break;
		case 11:
			if (bgEogXp.RewardTrackType == 2)
			{
				Log.All.Print("xp: BG type");
			}
			else
			{
				Log.All.Print("xp: GLOBAL type");
			}
			if (bgEogXp.GetRewardTrackType() == Global.RewardTrackType.BATTLEGROUNDS)
			{
				Log.All.Print("xp:() BG type");
			}
			else
			{
				Log.All.Print("xp:() GLOBAL type");
			}
			m_xpChanges.Add(bgEogXp);
			break;
		case 12:
			m_xpChanges.Add(bgDontLevelQuest);
			break;
		case 13:
			m_xpChanges.Add(bgEogXp);
			m_xpChanges.Add(bgDontLevelQuest);
			break;
		case 14:
			m_xpChanges.Add(bgEogXp);
			m_xpChanges.Add(bgDontLevelQuest);
			m_xpChanges.Add(bgLevelUpQuest);
			break;
		case 15:
			bgEogXp.PrevXp = 0;
			bgEogXp.CurrXp = 90;
			bgEogXp.PrevLevel = 1;
			bgEogXp.CurrLevel = 2;
			m_xpChanges.Add(bgEogXp);
			bgLevelUpQuest.PrevLevel = 2;
			bgLevelUpQuest.CurrLevel = 5;
			bgLevelUpQuest.CurrXp = 40;
			m_xpChanges.Add(bgLevelUpQuest);
			bgDontLevelQuest.PrevLevel = 5;
			bgDontLevelQuest.CurrLevel = 5;
			m_xpChanges.Add(bgDontLevelQuest);
			break;
		case 16:
			bgEogXp.CurrLevel = 2;
			m_xpChanges.Add(bgEogXp);
			break;
		case 17:
			m_xpChanges.Add(bgEogXpMegaLevel);
			break;
		case 21:
			m_xpChanges.Add(eogXp);
			m_xpChanges.Add(bgEogXp);
			break;
		case 22:
			m_xpChanges.Add(dontLevelQuest);
			m_xpChanges.Add(bgDontLevelQuest);
			break;
		case 23:
			m_xpChanges.Add(eogXp);
			m_xpChanges.Add(dontLevelQuest);
			m_xpChanges.Add(bgEogXp);
			m_xpChanges.Add(bgDontLevelQuest);
			break;
		case 24:
			m_xpChanges.Add(eogXp);
			m_xpChanges.Add(dontLevelQuest);
			m_xpChanges.Add(levelUpQuest);
			m_xpChanges.Add(bgEogXp);
			m_xpChanges.Add(bgDontLevelQuest);
			m_xpChanges.Add(bgLevelUpQuest);
			break;
		case 25:
			eogXp.PrevXp = 0;
			eogXp.CurrXp = 90;
			eogXp.PrevLevel = 1;
			eogXp.CurrLevel = 2;
			m_xpChanges.Add(eogXp);
			levelUpQuest.PrevLevel = 2;
			levelUpQuest.CurrLevel = 5;
			levelUpQuest.CurrXp = 40;
			m_xpChanges.Add(levelUpQuest);
			dontLevelQuest.PrevLevel = 5;
			dontLevelQuest.CurrLevel = 5;
			m_xpChanges.Add(dontLevelQuest);
			bgEogXp.PrevXp = 0;
			bgEogXp.CurrXp = 90;
			bgEogXp.PrevLevel = 1;
			bgEogXp.CurrLevel = 2;
			m_xpChanges.Add(bgEogXp);
			bgLevelUpQuest.PrevLevel = 2;
			bgLevelUpQuest.CurrLevel = 5;
			bgLevelUpQuest.CurrXp = 40;
			m_xpChanges.Add(bgLevelUpQuest);
			bgDontLevelQuest.PrevLevel = 5;
			bgDontLevelQuest.CurrLevel = 5;
			m_xpChanges.Add(bgDontLevelQuest);
			break;
		case 26:
			eogXp.CurrLevel = 2;
			m_xpChanges.Add(eogXp);
			bgEogXp.CurrLevel = 2;
			m_xpChanges.Add(bgEogXp);
			break;
		case 27:
			m_xpChanges.Add(eogXpMegaLevel);
			m_xpChanges.Add(bgEogXpMegaLevel);
			break;
		case 28:
			eogXp.PrevXp = 0;
			eogXp.CurrXp = 90;
			eogXp.PrevLevel = 1;
			eogXp.CurrLevel = 2;
			m_xpChanges.Add(eogXp);
			levelUpQuest.PrevLevel = 2;
			levelUpQuest.CurrLevel = 5;
			levelUpQuest.CurrXp = 40;
			m_xpChanges.Add(levelUpQuest);
			dontLevelQuest.PrevLevel = 5;
			dontLevelQuest.CurrLevel = 5;
			m_xpChanges.Add(dontLevelQuest);
			bgEogXp.CurrLevel = 2;
			m_xpChanges.Add(bgEogXp);
			break;
		case 29:
			m_xpChanges.Add(eogXp);
			m_xpChanges.Add(dontLevelQuest);
			m_xpChanges.Add(levelUpQuest);
			bgEogXp.CurrLevel = 2;
			m_xpChanges.Add(bgEogXp);
			break;
		case 30:
			m_xpChanges.Add(eogXp);
			m_xpChanges.Add(dontLevelQuest);
			m_xpChanges.Add(levelUpQuest);
			m_xpChanges.Add(bgEogXp);
			break;
		case 31:
			m_xpChanges.Add(eogXp);
			m_xpChanges.Add(dontLevelQuest);
			m_xpChanges.Add(bgEogXp);
			m_xpChanges.Add(bgDontLevelQuest);
			break;
		case 32:
			m_xpChanges.Add(eogXp);
			m_xpChanges.Add(dontLevelQuest);
			m_xpChanges.Add(bgDontLevelQuest);
			break;
		case 33:
			m_xpChanges.Add(eogXp);
			m_xpChanges.Add(eventEogXp);
			break;
		case 34:
			DebugSimScenario(21);
			m_xpChanges.Add(eventEogXp);
			break;
		case 35:
			DebugSimScenario(23);
			m_xpChanges.Add(eventEogXp);
			m_xpChanges.Add(eventLevelUpQuest);
			break;
		case 36:
			m_xpChanges.Add(apprenticeEogXp);
			break;
		case 37:
			m_xpChanges.Add(apprenticeEogXp);
			m_xpChanges.Add(apprenticeLevelUpQuest);
			break;
		case 8:
		case 9:
		case 10:
		case 18:
		case 19:
		case 20:
			break;
		}
	}

	public void TerminateEndOfGameXp()
	{
		TerminateEarly();
	}
}
