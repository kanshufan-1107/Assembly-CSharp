using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Blizzard.T5.Core.Utils;
using Hearthstone.DataModels;
using Hearthstone.UI;
using HutongGames.PlayMaker;
using PegasusUtil;
using UnityEngine;

namespace Hearthstone.Progression;

[RequireComponent(typeof(WidgetTemplate))]
public class QuestXpReward : MonoBehaviour
{
	[SerializeField]
	private PlayMakerFSM m_fsm;

	[SerializeField]
	private Widget m_questTileWidget;

	[SerializeField]
	private AsyncReference m_xpBarMeshReference;

	[SerializeField]
	private AsyncReference m_questXpRoot;

	[SerializeField]
	private AsyncReference m_xpProgressText;

	[SerializeField]
	private AsyncReference m_levelText;

	[SerializeField]
	private AsyncReference m_xpDisplayText;

	[SerializeField]
	private Global.RewardTrackType m_rewardTrackType;

	private Widget m_widget;

	private List<RewardTrackXpChange> m_xpChanges;

	private RewardTrackDataModel m_dataModel;

	private int m_targetLevel;

	private int m_targetXp;

	private Action m_callback;

	private bool m_pauseOnNext;

	private bool m_isShowing;

	private bool m_gameXpCausedLevel;

	private bool m_isIntro;

	private bool m_showingGameXp;

	private bool m_showingGameXpAndWaiting;

	private bool m_introShown;

	private const string SETUP_MODE_TRADITIONAL = "TRADITIONAL";

	private const string SETUP_MODE_BATTLEGROUNDS = "BATTLEGROUNDS";

	private const string SETUP_MODE_MERCENARIES = "MERCENARIES";

	private static Comparison<RewardTrackXpChange> SortXPChanges = delegate(RewardTrackXpChange a, RewardTrackXpChange b)
	{
		int num = a.CurrLevel.CompareTo(b.CurrLevel);
		if (num == 0)
		{
			num = a.CurrXp.CompareTo(b.CurrXp);
		}
		return num;
	};

	private void Awake()
	{
		FatalErrorMgr.Get().AddErrorListener(OnFatalError);
		m_widget = GetComponent<WidgetTemplate>();
		m_xpBarMeshReference.RegisterReadyListener(delegate(Transform t)
		{
			FsmGameObject fsmGameObject = m_fsm.FsmVariables.GetFsmGameObject("XpBarMesh");
			if (fsmGameObject != null)
			{
				fsmGameObject.Value = t.gameObject;
			}
		});
		m_questXpRoot.RegisterReadyListener(delegate(Transform t)
		{
			FsmGameObject fsmGameObject2 = m_fsm.FsmVariables.GetFsmGameObject("QuestXpRoot");
			if (fsmGameObject2 != null)
			{
				fsmGameObject2.Value = t.gameObject;
			}
		});
		m_xpProgressText.RegisterReadyListener(delegate(Transform t)
		{
			FsmGameObject fsmGameObject3 = m_fsm.FsmVariables.GetFsmGameObject("XpProgressText");
			if (fsmGameObject3 != null)
			{
				fsmGameObject3.Value = t.gameObject;
			}
		});
		m_levelText.RegisterReadyListener(delegate(Transform t)
		{
			FsmGameObject fsmGameObject4 = m_fsm.FsmVariables.GetFsmGameObject("LevelText");
			if (fsmGameObject4 != null)
			{
				fsmGameObject4.Value = t.gameObject;
			}
		});
		m_xpDisplayText.RegisterReadyListener(delegate(Transform t)
		{
			FsmGameObject fsmGameObject5 = m_fsm.FsmVariables.GetFsmGameObject("XpDisplayText");
			if (fsmGameObject5 != null)
			{
				fsmGameObject5.Value = t.gameObject;
			}
		});
	}

	public void Initialize(IEnumerable<RewardTrackXpChange> xpChangesForType)
	{
		List<RewardTrackXpChange> xpChanges = xpChangesForType.ToList();
		m_showingGameXp = true;
		int changeCount = xpChanges.Count;
		if (changeCount == 0)
		{
			return;
		}
		m_pauseOnNext = false;
		m_isShowing = true;
		m_gameXpCausedLevel = false;
		m_isIntro = false;
		m_introShown = false;
		RewardTrack rewardTrack = RewardTrackManager.Get().GetRewardTrack(m_rewardTrackType);
		if (rewardTrack == null || !rewardTrack.IsValid)
		{
			Debug.LogError(string.Format("{0}: no reward track found of type {1}.", "QuestXpReward", m_rewardTrackType));
			return;
		}
		m_dataModel = rewardTrack.TrackDataModel.CloneDataModel();
		RewardTrackDbfRecord rewardTrackAsset = rewardTrack.RewardTrackAsset;
		List<RewardTrackLevelDbfRecord> rewardTrackLevels = ((rewardTrackAsset != null) ? GameUtils.GetRewardTrackLevelsForRewardTrack(rewardTrackAsset.ID) : null);
		m_dataModel.LevelHardCap = rewardTrackLevels.Count;
		m_xpChanges = new List<RewardTrackXpChange>(xpChanges);
		if (changeCount > 1)
		{
			m_xpChanges.Sort(SortXPChanges);
		}
		m_dataModel.Xp = m_xpChanges[0].PrevXp;
		m_dataModel.Level = m_xpChanges[0].PrevLevel;
		UpdateDataModelXpNeeded();
		UpdateDataModelXpProgress(m_dataModel.Xp);
		m_widget.BindDataModel(m_dataModel);
		UpdateXpAndLevelFsmVars();
		UpdateQuestXpRootOverrides();
		FsmBool ShowBarAtStartVar = m_fsm.FsmVariables.GetFsmBool("ShowBarAtStart");
		if (ShowBarAtStartVar != null)
		{
			ShowBarAtStartVar.Value = HasQuestXP() && !HasGameXP();
		}
	}

	private bool HasQuestXP()
	{
		foreach (RewardTrackXpChange xpChange in m_xpChanges)
		{
			if (xpChange.RewardSourceType == 1)
			{
				return true;
			}
		}
		return false;
	}

	private bool HasGameXP()
	{
		foreach (RewardTrackXpChange change in m_xpChanges)
		{
			if (change.RewardSourceType == 5 || change.RewardSourceType == 10)
			{
				return true;
			}
		}
		return false;
	}

	public void ShowXpGains(Action callback)
	{
		m_callback = callback;
		if (!base.isActiveAndEnabled)
		{
			if (m_xpChanges != null && m_xpChanges.Count > 0)
			{
				Error.AddDevFatal("QuestXpReward.ShowXpGains - QuestXPReward not active but there are xp changes to show.");
			}
			m_callback?.Invoke();
		}
		else
		{
			StartCoroutine(ShowXpGainsWhenReady());
		}
	}

	public void ShowNextXpGain()
	{
		RewardXpNotificationManager rewardXpNotificationManager = RewardXpNotificationManager.Get();
		if (rewardXpNotificationManager == null)
		{
			Log.EndOfGame.PrintError("RewardXPNotificationManager is null");
			return;
		}
		if (m_xpChanges == null)
		{
			m_callback?.Invoke();
			Log.EndOfGame.PrintError("XpChanges list is null");
			return;
		}
		if (m_dataModel == null)
		{
			Log.EndOfGame.PrintError("Reward Track data model clone is null");
			return;
		}
		if (m_questTileWidget == null)
		{
			Log.EndOfGame.PrintError("Quest Tile Widget clone is null");
			return;
		}
		if (m_xpChanges.Count == 0)
		{
			if (m_showingGameXp)
			{
				m_showingGameXp = false;
				m_showingGameXpAndWaiting = true;
				UpdateDataModelXpProgress(m_targetXp);
				m_callback?.Invoke();
			}
			else if (m_isShowing)
			{
				m_isShowing = false;
				m_callback?.Invoke();
			}
			else
			{
				m_callback?.Invoke();
			}
			return;
		}
		if (m_pauseOnNext)
		{
			m_pauseOnNext = false;
			return;
		}
		RewardTrackXpChange message = m_xpChanges[0];
		RewardTrackXpChange lastMessage = message;
		if (message == null)
		{
			Log.EndOfGame.PrintError("XPChange message is null");
			return;
		}
		bool messageIsXpPerGame = message.RewardSourceType == 5 || message.RewardSourceType == 10;
		if ((m_showingGameXp || m_showingGameXpAndWaiting) && !messageIsXpPerGame)
		{
			m_showingGameXp = false;
			m_showingGameXpAndWaiting = false;
			UpdateDataModelXpProgress(m_targetXp);
			m_callback?.Invoke();
			return;
		}
		if (messageIsXpPerGame)
		{
			m_showingGameXp = true;
			while (m_xpChanges.Count > 1 && (m_xpChanges[1].RewardSourceType == 5 || m_xpChanges[1].RewardSourceType == 10))
			{
				lastMessage = m_xpChanges[1];
				m_xpChanges.RemoveAt(0);
			}
		}
		m_xpChanges.RemoveAt(0);
		m_dataModel.Level = message.PrevLevel;
		m_dataModel.Xp = message.PrevXp;
		m_targetLevel = lastMessage.CurrLevel;
		m_targetXp = lastMessage.CurrXp;
		m_gameXpCausedLevel = messageIsXpPerGame && message.PrevLevel != lastMessage.CurrLevel;
		UpdateDataModelXpNeeded();
		UpdateXpAndLevelFsmVars();
		UpdateDataModelXpProgress(m_dataModel.Xp);
		if (message.RewardSourceType == 1)
		{
			if (QuestManager.Get() != null)
			{
				QuestDataModel questDataModel = QuestManager.Get().CreateQuestDataModelById(message.RewardSourceId);
				if (questDataModel == null)
				{
					Log.EndOfGame.PrintError("Quest data model clone is null");
					return;
				}
				questDataModel.RerollCount = 0;
				questDataModel.DisplayMode = QuestManager.QuestDisplayMode.Inspection;
				m_questTileWidget.BindDataModel(questDataModel);
				AnimateQuestTile();
			}
		}
		else if (messageIsXpPerGame)
		{
			if (m_xpChanges.Count > 0 || (m_xpChanges.Count == 0 && m_dataModel.Level < m_targetLevel))
			{
				m_isIntro = true;
			}
			else if (rewardXpNotificationManager.JustShowGameXp && m_xpChanges.Count == 0)
			{
				m_questTileWidget.TriggerEvent("DISABLE_INTERACTION");
				m_pauseOnNext = true;
			}
			if (!m_introShown)
			{
				ShowIntroXp();
			}
			else
			{
				AnimateXpBar();
			}
		}
		else
		{
			AnimateXpBar();
		}
	}

	public void ClearAndHide()
	{
		if (m_xpChanges != null && m_xpChanges.Count != 0)
		{
			m_xpChanges.Clear();
		}
		if (m_widget != null)
		{
			m_widget.Hide();
		}
		m_pauseOnNext = false;
	}

	public void ContinueNotifications()
	{
		m_pauseOnNext = false;
		ShowNextXpGain();
	}

	public void Hide()
	{
		UpdateDataModelXpProgress(m_targetXp);
		m_fsm.SendEvent("Popup_Outro");
	}

	public Global.RewardTrackType GetTrackType()
	{
		return m_rewardTrackType;
	}

	public void SetTrackType(Global.RewardTrackType type)
	{
		m_rewardTrackType = type;
	}

	public int GetXpChangesRemaining()
	{
		if (m_xpChanges != null)
		{
			return m_xpChanges.Count;
		}
		return 0;
	}

	private IEnumerator ShowXpGainsWhenReady()
	{
		while (!m_widget.IsReady || m_widget.IsChangingStates)
		{
			yield return null;
		}
		if (m_xpChanges == null || m_xpChanges.Count() == 0)
		{
			m_callback?.Invoke();
		}
		else
		{
			ShowNextXpGain();
		}
	}

	private void UpdateDataModelXpNeeded()
	{
		RewardTrack rewardTrack = RewardTrackManager.Get().GetRewardTrack(m_dataModel.RewardTrackType);
		if (rewardTrack != null)
		{
			RewardTrackLevelDbfRecord levelAsset = rewardTrack.GetRewardTrackLevelRecord(m_dataModel.Level);
			m_dataModel.XpNeeded = levelAsset?.XpNeeded ?? 0;
		}
	}

	private void UpdateXpAndLevelFsmVars()
	{
		int nextLevel = m_dataModel.Level + 1;
		FsmInt startLevelVar = m_fsm.FsmVariables.GetFsmInt("StartLevel");
		if (startLevelVar != null)
		{
			startLevelVar.Value = nextLevel;
		}
		FsmInt startXpVar = m_fsm.FsmVariables.GetFsmInt("StartXp");
		if (startXpVar != null)
		{
			startXpVar.Value = m_dataModel.Xp;
		}
		FsmInt endLevelVar = m_fsm.FsmVariables.GetFsmInt("EndLevel");
		if (endLevelVar != null)
		{
			if (m_dataModel.Level < m_targetLevel)
			{
				endLevelVar.Value = nextLevel + 1;
			}
			else
			{
				endLevelVar.Value = nextLevel;
			}
		}
		FsmInt endXpVar = m_fsm.FsmVariables.GetFsmInt("EndXp");
		if (endXpVar != null)
		{
			if (m_dataModel.Level < m_targetLevel)
			{
				endXpVar.Value = m_dataModel.XpNeeded;
			}
			else
			{
				endXpVar.Value = m_targetXp;
			}
		}
		FsmInt xpNeededVar = m_fsm.FsmVariables.GetFsmInt("XpNeeded");
		if (xpNeededVar != null)
		{
			xpNeededVar.Value = m_dataModel.XpNeeded;
		}
		FsmFloat startBarPctVar = m_fsm.FsmVariables.GetFsmFloat("StartBarPct");
		if (startBarPctVar != null)
		{
			startBarPctVar.Value = (float)startXpVar.Value / (float)xpNeededVar.Value;
		}
		FsmFloat endBarPctVar = m_fsm.FsmVariables.GetFsmFloat("EndBarPct");
		if (endBarPctVar != null)
		{
			endBarPctVar.Value = (float)endXpVar.Value / (float)xpNeededVar.Value;
		}
	}

	private int CalculateIntroXpGained()
	{
		bool hasLeveled = m_dataModel.Level < m_targetLevel;
		int xpTotalGained = (hasLeveled ? m_dataModel.XpNeeded : m_targetXp) - m_dataModel.Xp;
		if (m_targetLevel > m_dataModel.Level + 1)
		{
			RewardTrack rewardTrack = RewardTrackManager.Get().GetRewardTrack(m_dataModel);
			for (int curLevel = m_dataModel.Level + 1; curLevel < m_targetLevel; curLevel++)
			{
				xpTotalGained += (rewardTrack?.GetRewardTrackLevelRecord(curLevel)?.XpNeeded).GetValueOrDefault();
			}
		}
		if (hasLeveled)
		{
			xpTotalGained += m_targetXp;
		}
		return xpTotalGained;
	}

	private void UpdateIntroXpVar(int xpGained)
	{
		FsmInt xpGainedVar = m_fsm.FsmVariables.GetFsmInt("XpGained");
		if (xpGainedVar != null)
		{
			xpGainedVar.Value = xpGained;
		}
	}

	private void AnimateQuestTile()
	{
		m_fsm.SendEvent("AnimateQuestTile");
	}

	private void AnimateXpBar()
	{
		m_fsm.SendEvent("AnimateXpBar");
	}

	private void AnimateLevelFromGameXp()
	{
		m_fsm.SendEvent("LevelFromGameXp");
	}

	private void AnimateIntroXp()
	{
		m_fsm.SendEvent("Intro");
	}

	private void ShowIntroXp()
	{
		UpdateIntroXpVar(CalculateIntroXpGained());
		AnimateIntroXp();
		m_introShown = true;
	}

	private void OnPlayMakerFinished()
	{
		if (m_isIntro)
		{
			m_isIntro = false;
			AnimateXpBar();
			return;
		}
		bool isStillAnimating = false;
		if (m_dataModel.Level < m_targetLevel || m_dataModel.Xp < m_targetXp)
		{
			if (m_dataModel.Level < m_targetLevel)
			{
				m_dataModel.Level++;
				m_dataModel.Xp = 0;
				UpdateDataModelXpNeeded();
				if (m_dataModel.Level < m_targetLevel || m_dataModel.Xp < m_targetXp)
				{
					UpdateXpAndLevelFsmVars();
					UpdateDataModelXpProgress(m_dataModel.Xp);
					if (m_gameXpCausedLevel)
					{
						AnimateLevelFromGameXp();
					}
					else
					{
						AnimateXpBar();
					}
					isStillAnimating = true;
				}
			}
			else
			{
				m_dataModel.Xp = m_targetXp;
			}
		}
		if (!isStillAnimating)
		{
			ShowNextXpGain();
		}
	}

	private void UpdateDataModelXpProgress(int currentAnimatedXp)
	{
		m_dataModel.XpProgress = GameStrings.Format("GLUE_PROGRESSION_REWARD_TRACK_XP_PROGRESS", currentAnimatedXp, m_dataModel.XpNeeded);
	}

	private void UpdateQuestXpRootOverrides()
	{
		if (GameMgr.Get().IsBattlegrounds())
		{
			m_widget.TriggerEvent("BATTLEGROUNDS");
		}
		else if (GameMgr.Get().IsMercenaries())
		{
			m_widget.TriggerEvent("MERCENARIES");
		}
		else
		{
			m_widget.TriggerEvent("TRADITIONAL");
		}
	}

	private void OnFatalError(FatalErrorMessage message, object userData)
	{
		Widget rootWidget = null;
		Widget currentWidget = GameObjectUtils.GetComponentOnSelfOrParent<Widget>(base.transform);
		while (currentWidget != null)
		{
			rootWidget = currentWidget;
			currentWidget = GameObjectUtils.GetComponentOnSelfOrParent<Widget>(currentWidget.transform.parent);
		}
		if (rootWidget != null)
		{
			UnityEngine.Object.Destroy(rootWidget.gameObject);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
