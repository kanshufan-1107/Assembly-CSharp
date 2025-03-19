using System.Linq;
using Blizzard.T5.Services;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using UnityEngine;

public class QuestToast : MonoBehaviour
{
	public delegate void DelOnCloseQuestToast(object userData);

	private class ToastCallbackData
	{
		public DelOnCloseQuestToast m_onCloseCallback;

		public object m_onCloseCallbackData;

		public RewardData m_toastReward;

		public string m_toastName = string.Empty;

		public string m_toastDescription = string.Empty;

		public bool m_updateCacheValues;

		public Achievement m_quest;
	}

	public UberText m_questName;

	public GameObject m_nameLine;

	public UberText m_requirement;

	public Transform m_rewardBone;

	public PegUIElement m_clickCatcher;

	public Vector3 m_rewardScale;

	public Vector3_MobileOverride m_boosterRewardRootScale;

	public Vector3_MobileOverride m_boosterRewardPosition;

	public Vector3_MobileOverride m_boosterRewardScale;

	public Vector3_MobileOverride m_cardRewardRootScale;

	public Vector3_MobileOverride m_cardRewardScale;

	public Vector3_MobileOverride m_cardRewardLocation;

	public Vector3_MobileOverride m_signatureCardRewardScale;

	public Vector3_MobileOverride m_signatureCardRewardLocation;

	public Vector3_MobileOverride m_cardDuplicateRewardScale;

	public Vector3_MobileOverride m_cardDuplicateRewardLocation;

	public Vector3_MobileOverride m_cardBackRootScale;

	public Vector3_MobileOverride m_cardbackRewardScale;

	public Vector3_MobileOverride m_cardbackRewardLocation;

	public Vector3_MobileOverride m_goldRewardScale;

	public Vector3_MobileOverride m_goldBannerOffset;

	public Vector3_MobileOverride m_goldBannerScale;

	public Vector3_MobileOverride m_dustRewardScale;

	public Vector3_MobileOverride m_dustRewardOffset;

	public Vector3_MobileOverride m_dustBannerOffset;

	public Vector3_MobileOverride m_dustBannerScale;

	public Vector3_MobileOverride m_battlegroundsTokenRewardScale;

	public Vector3_MobileOverride m_battlegroundsTokenBannerOffset;

	public Vector3_MobileOverride m_battlegroundsTokenBannerScale;

	private Achievement m_quest;

	private DelOnCloseQuestToast m_onCloseCallback;

	private object m_onCloseCallbackData;

	private RewardData m_toastReward;

	private string m_toastName = string.Empty;

	private string m_toastDescription = string.Empty;

	private static bool m_showFullscreenEffects = true;

	private static bool m_isToastActiveOrActivating;

	private static QuestToast m_activeToast;

	private ScreenEffectsHandle m_screenEffectsHandle;

	public void Awake()
	{
		OverlayUI.Get().AddGameObject(base.gameObject);
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	public void OnDestroy()
	{
		if (this == m_activeToast)
		{
			if (m_isToastActiveOrActivating)
			{
				FadeEffectsOut();
				m_isToastActiveOrActivating = false;
			}
			m_activeToast = null;
		}
	}

	public static void ShowQuestToast(UserAttentionBlocker blocker, DelOnCloseQuestToast onClosedCallback, bool updateCacheValues, Achievement quest)
	{
		ShowQuestToast(blocker, onClosedCallback, updateCacheValues, quest, fullScreenEffects: true);
	}

	public static void ShowQuestToast(UserAttentionBlocker blocker, DelOnCloseQuestToast onClosedCallback, bool updateCacheValues, Achievement quest, bool fullScreenEffects)
	{
		ShowQuestToast(blocker, onClosedCallback, null, updateCacheValues, quest, fullScreenEffects);
	}

	public static void ShowQuestToast(UserAttentionBlocker blocker, DelOnCloseQuestToast onClosedCallback, object callbackUserData, bool updateCacheValues, Achievement quest)
	{
		ShowQuestToast(blocker, onClosedCallback, callbackUserData, updateCacheValues, quest, fullscreenEffects: true);
	}

	public static void ShowQuestToast(UserAttentionBlocker blocker, DelOnCloseQuestToast onClosedCallback, object callbackUserData, bool updateCacheValues, Achievement quest, bool fullscreenEffects)
	{
		if (!UserAttentionManager.CanShowAttentionGrabber(blocker, "ShowQuestToast:" + ((quest == null) ? "null" : quest.ID.ToString())))
		{
			onClosedCallback?.Invoke(callbackUserData);
			return;
		}
		Log.Achievements.Print("ShowQuestToast: {0}", quest);
		if (quest.Rewards.Any((RewardData r) => r.RewardType == Reward.Type.ARCANE_ORBS) && Shop.Get() != null && ServiceManager.TryGet<CurrencyManager>(out var currencyManager))
		{
			currencyManager.MarkCurrencyDirty(CurrencyType.CN_ARCANE_ORBS);
		}
		quest.AckCurrentProgressAndRewardNotices();
		if (quest.ID == 56)
		{
			onClosedCallback?.Invoke(callbackUserData);
		}
		else
		{
			ShowQuestToastPopup(blocker, onClosedCallback, callbackUserData, (quest.Rewards == null) ? null : quest.Rewards.FirstOrDefault(), quest.Name, quest.Description, fullscreenEffects, updateCacheValues, quest);
		}
	}

	public static void ShowFixedRewardQuestToast(UserAttentionBlocker blocker, DelOnCloseQuestToast onClosedCallback, RewardData rewardData, string name, string description)
	{
		ShowFixedRewardQuestToast(blocker, onClosedCallback, null, rewardData, name, description, fullscreenEffects: true);
	}

	public static void ShowFixedRewardQuestToast(UserAttentionBlocker blocker, DelOnCloseQuestToast onClosedCallback, object callbackUserData, RewardData rewardData, string name, string description, bool fullscreenEffects)
	{
		ShowQuestToastPopup(blocker, onClosedCallback, callbackUserData, rewardData, name, description, fullscreenEffects, updateCacheValues: true, null);
	}

	public static void ShowGenericRewardQuestToast(UserAttentionBlocker blocker, DelOnCloseQuestToast onClosedCallback, RewardData rewardData, string name, string description)
	{
		ShowGenericRewardQuestToast(blocker, onClosedCallback, null, rewardData, name, description, fullscreenEffects: true);
	}

	public static void ShowGenericRewardQuestToast(UserAttentionBlocker blocker, DelOnCloseQuestToast onClosedCallback, object callbackUserData, RewardData rewardData, string name, string description, bool fullscreenEffects)
	{
		ShowQuestToastPopup(blocker, onClosedCallback, callbackUserData, rewardData, name, description, fullscreenEffects, updateCacheValues: false, null);
	}

	public static void ShowQuestToastPopup(UserAttentionBlocker blocker, DelOnCloseQuestToast onClosedCallback, object callbackUserData, RewardData rewardData, string name, string description, bool fullscreenEffects, bool updateCacheValues, Achievement quest)
	{
		if (!UserAttentionManager.CanShowAttentionGrabber(blocker, "ShowQuestToastPopup:" + ((rewardData == null) ? "null" : (rewardData.Origin.ToString() + ":" + rewardData.OriginData + ":" + rewardData.RewardType))))
		{
			onClosedCallback?.Invoke(callbackUserData);
			return;
		}
		Log.Achievements.Print("ShowQuestToastPopup: name={0} desc={1}", name, description);
		m_showFullscreenEffects = fullscreenEffects;
		m_isToastActiveOrActivating = true;
		ToastCallbackData callbackData = new ToastCallbackData
		{
			m_toastReward = rewardData,
			m_toastName = name,
			m_toastDescription = description,
			m_onCloseCallback = onClosedCallback,
			m_onCloseCallbackData = callbackUserData,
			m_quest = quest,
			m_updateCacheValues = updateCacheValues
		};
		AssetLoader.Get().InstantiatePrefab("QuestToast.prefab:ebf10185d03f14f41a367b9a7170c4c4", PositionActor, callbackData);
	}

	private static void PositionActor(AssetReference assetRef, GameObject go, object callbackData)
	{
		go.transform.localPosition = new Vector3(0f, 85f, 0f);
		Vector3 targetScale = go.transform.localScale;
		go.transform.localScale = 0.01f * Vector3.one;
		go.SetActive(value: true);
		iTween.ScaleTo(go, targetScale, 0.5f);
		QuestToast questToastInfo = go.GetComponent<QuestToast>();
		if (questToastInfo == null)
		{
			Debug.LogWarning("QuestToast.PositionActor(): actor has no QuestToast component");
			m_isToastActiveOrActivating = false;
			return;
		}
		m_activeToast = questToastInfo;
		ToastCallbackData toastCallbackData = callbackData as ToastCallbackData;
		questToastInfo.m_onCloseCallback = toastCallbackData.m_onCloseCallback;
		questToastInfo.m_toastReward = toastCallbackData.m_toastReward;
		questToastInfo.m_toastName = toastCallbackData.m_toastName;
		questToastInfo.m_toastDescription = toastCallbackData.m_toastDescription;
		questToastInfo.m_onCloseCallbackData = toastCallbackData;
		questToastInfo.m_quest = toastCallbackData.m_quest;
		questToastInfo.SetUpToast(toastCallbackData.m_updateCacheValues);
	}

	private void CloseQuestToast(UIEvent e)
	{
		CloseQuestToast();
	}

	public void CloseQuestToast()
	{
		if (base.gameObject == null)
		{
			return;
		}
		m_isToastActiveOrActivating = false;
		m_clickCatcher.RemoveEventListener(UIEventType.RELEASE, CloseQuestToast);
		SoundManager.Get().LoadAndPlay("new_quest_click_and_shrink.prefab:601ba6676276eab43947e38f110f7b99");
		FadeEffectsOut();
		iTween.ScaleTo(base.gameObject, iTween.Hash("scale", Vector3.zero, "time", 0.5f, "oncompletetarget", base.gameObject, "oncomplete", "DestroyQuestToast"));
		UIContext.GetRoot().DismissPopup(base.gameObject);
		if (m_onCloseCallback != null)
		{
			if (m_onCloseCallbackData is ToastCallbackData { m_quest: not null } cbData)
			{
				NarrativeManager.Get().OnAchieveDismissed(cbData.m_quest);
			}
			m_onCloseCallback(m_onCloseCallbackData);
		}
	}

	public static bool IsQuestActive()
	{
		if (m_isToastActiveOrActivating)
		{
			return m_activeToast != null;
		}
		return false;
	}

	public static QuestToast GetCurrentToast()
	{
		return m_activeToast;
	}

	private void DestroyQuestToast()
	{
		Object.Destroy(base.gameObject);
	}

	public void SetUpToast(bool updateCacheValues)
	{
		m_clickCatcher.AddEventListener(UIEventType.RELEASE, CloseQuestToast);
		m_questName.Text = m_toastName;
		m_requirement.Text = m_toastDescription;
		if (m_toastReward != null)
		{
			if (EventTimingManager.Get().IsEventActive(EventTimingType.SPECIAL_EVENT_GOLD_DOUBLED) && m_quest != null && m_quest.IsAffectedByDoubleGold && m_toastReward is GoldRewardData)
			{
				GoldRewardData newReward = new GoldRewardData(m_toastReward as GoldRewardData);
				newReward.Amount *= 2L;
				m_toastReward = newReward;
			}
			m_toastReward.LoadRewardObject(RewardObjectLoaded, updateCacheValues);
		}
		UIContext.GetRoot().ShowPopup(base.gameObject, UIContext.BlurType.None);
		FadeEffectsIn();
	}

	private void RewardObjectLoaded(Reward reward, object callbackData)
	{
		if (this == null)
		{
			return;
		}
		bool updateCacheValues = (bool)callbackData;
		reward.Hide();
		reward.transform.parent = m_rewardBone;
		reward.transform.localEulerAngles = Vector3.zero;
		reward.transform.localScale = m_rewardScale;
		reward.transform.localPosition = Vector3.zero;
		BoosterPackReward pack = reward.gameObject.GetComponentInChildren<BoosterPackReward>();
		if (pack != null)
		{
			reward.transform.localScale = m_boosterRewardRootScale;
			reward.m_MeshRoot.transform.localPosition = m_boosterRewardPosition;
			reward.m_MeshRoot.transform.localScale = m_boosterRewardScale;
			pack.AllowMultiStack = false;
			pack.m_Layer = (GameLayer)base.gameObject.layer;
		}
		CardReward card = reward.gameObject.GetComponentInChildren<CardReward>();
		if (card != null)
		{
			reward.transform.localScale = m_cardRewardRootScale;
			CardRewardData obj = card.Data as CardRewardData;
			if (obj != null && obj.Premium == TAG_PREMIUM.SIGNATURE)
			{
				card.m_cardParent.transform.localScale = m_signatureCardRewardScale;
				card.m_cardParent.transform.localPosition = m_signatureCardRewardLocation;
			}
			else
			{
				card.m_cardParent.transform.localScale = m_cardRewardScale;
				card.m_cardParent.transform.localPosition = m_cardRewardLocation;
			}
			card.m_duplicateCardParent.transform.localScale = m_cardDuplicateRewardScale;
			card.m_duplicateCardParent.transform.localPosition = m_cardDuplicateRewardLocation;
		}
		CardBackReward cardback = reward.gameObject.GetComponentInChildren<CardBackReward>();
		if (cardback != null)
		{
			reward.transform.localScale = m_cardBackRootScale;
			cardback.m_cardbackBone.transform.localScale = m_cardbackRewardScale;
			cardback.m_cardbackBone.transform.localPosition = m_cardbackRewardLocation;
		}
		GoldReward goldReward = reward.gameObject.GetComponentInChildren<GoldReward>();
		if (goldReward != null)
		{
			goldReward.m_root.transform.localScale = m_goldRewardScale;
			goldReward.m_rewardBannerBone.transform.localPosition += (Vector3)m_goldBannerOffset;
			goldReward.m_rewardBannerBone.transform.localScale = m_goldBannerScale;
		}
		BattlegroundsTokenReward tokenReward = reward.gameObject.GetComponentInChildren<BattlegroundsTokenReward>();
		if (tokenReward != null)
		{
			tokenReward.m_root.transform.localScale = m_battlegroundsTokenRewardScale;
			tokenReward.m_rewardBannerBone.transform.localPosition += (Vector3)m_battlegroundsTokenBannerOffset;
			tokenReward.m_rewardBannerBone.transform.localScale = m_battlegroundsTokenBannerScale;
		}
		ArcaneDustReward dustReward = reward.gameObject.GetComponentInChildren<ArcaneDustReward>();
		if (dustReward != null)
		{
			dustReward.m_root.transform.localScale = m_dustRewardScale;
			dustReward.m_root.transform.localPosition += (Vector3)m_dustRewardOffset;
			dustReward.m_rewardBannerBone.transform.localPosition += (Vector3)m_dustBannerOffset;
			dustReward.m_rewardBannerBone.transform.localScale = m_dustBannerScale;
		}
		base.gameObject.GetComponent<IPopupRoot>()?.ApplyPopupRendering(reward.transform, null, overrideLayer: true, 29);
		reward.Show(updateCacheValues);
	}

	private void FadeEffectsIn()
	{
		if (m_showFullscreenEffects)
		{
			ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignettePerspective;
			screenEffectParameters.Time = 0.4f;
			screenEffectParameters.Blur = new BlurParameters(1f, 1f);
			m_screenEffectsHandle.StartEffect(screenEffectParameters);
		}
	}

	private void FadeEffectsOut()
	{
		if (m_showFullscreenEffects && FullScreenFXMgr.Get() != null)
		{
			m_screenEffectsHandle.StopEffect();
		}
	}
}
