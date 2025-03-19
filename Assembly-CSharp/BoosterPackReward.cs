using System.Collections;
using Assets;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class BoosterPackReward : Reward
{
	public bool m_RotateIn = true;

	public GameObject m_BoosterPackBone;

	public GameLayer m_Layer = GameLayer.IgnoreFullScreenEffects;

	public Material m_PackGlowMaterial;

	public AnimationCurve m_RotationCurve;

	private bool m_AllowMultiStack = true;

	private UnopenedPack m_unopenedPack;

	[Header("Mercenaries")]
	public RewardBanner m_mercenariesRewardBannerPrefab;

	public bool AllowMultiStack
	{
		get
		{
			return m_AllowMultiStack;
		}
		set
		{
			m_AllowMultiStack = value;
			UpdatePackStacks();
		}
	}

	protected override void InitData()
	{
		SetData(new BoosterPackRewardData(), updateVisuals: false);
	}

	protected override void ShowReward(bool updateCacheValues)
	{
		m_root.SetActive(value: true);
		LayerUtils.SetLayer(m_root, m_Layer);
		if (m_unopenedPack != null)
		{
			Vector3 endScale = m_unopenedPack.transform.localScale;
			m_unopenedPack.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
			iTween.ScaleTo(m_unopenedPack.gameObject, iTween.Hash("scale", endScale, "time", 0.5f, "easetype", iTween.EaseType.easeOutElastic));
			if (m_RotateIn)
			{
				PlayRotateInAnimation();
			}
		}
	}

	protected override void HideReward()
	{
		base.HideReward();
		m_root.SetActive(value: false);
	}

	protected override void OnDataSet(bool updateVisuals)
	{
		if (!updateVisuals)
		{
			return;
		}
		m_BoosterPackBone.gameObject.SetActive(value: false);
		BoosterPackRewardData boosterRewardData = base.Data as BoosterPackRewardData;
		string rewardHeadline = string.Empty;
		string rewardDetails = string.Empty;
		string rewardSource = string.Empty;
		if (boosterRewardData.Id == 629)
		{
			m_rewardBannerPrefab = m_mercenariesRewardBannerPrefab;
			UpdateBannerObject();
			rewardHeadline = GameStrings.Get("GLOBAL_LETTUCE_REWARD_BANNER_TEXT");
		}
		else if (base.Data.Origin != NetCache.ProfileNotice.NoticeOrigin.OUT_OF_BAND_LICENSE)
		{
			rewardHeadline = ((boosterRewardData.Count > 1) ? GameStrings.Format("GLOBAL_REWARD_BOOSTER_HEADLINE_MULTIPLE", boosterRewardData.Count) : GameStrings.Get("GLOBAL_REWARD_BOOSTER_HEADLINE_GENERIC"));
		}
		else
		{
			BoosterDbfRecord boosterRecord = GameDbf.Booster.GetRecord(boosterRewardData.Id);
			if (boosterRecord == null)
			{
				return;
			}
			rewardHeadline = ((boosterRewardData.Count > 1) ? GameStrings.Get("GLOBAL_REWARD_BOOSTER_HEADLINE_OUT_OF_BAND_MULTI") : GameStrings.Get("GLOBAL_REWARD_BOOSTER_HEADLINE_OUT_OF_BAND"));
			EventTimingManager events = EventTimingManager.Get();
			EventTimingType buyWithGoldEvent = boosterRecord.BuyWithGoldEvent;
			rewardSource = ((events.IsEventActive(buyWithGoldEvent) || !events.GetEventStartTimeUtc(buyWithGoldEvent).HasValue || events.HasEventStarted(buyWithGoldEvent)) ? GameStrings.Format("GLOBAL_REWARD_BOOSTER_DETAILS_OUT_OF_BAND", boosterRewardData.Count) : GameStrings.Format("GLOBAL_REWARD_BOOSTER_DETAILS_PRESALE_OUT_OF_BAND", boosterRewardData.Count));
		}
		SetRewardText(rewardHeadline, rewardDetails, rewardSource);
		BoosterDbfRecord packRecord = GameDbf.Booster.GetRecord(boosterRewardData.Id);
		if (packRecord == null)
		{
			RewardBagDbfRecord record = GameDbf.RewardBag.GetRecord((RewardBagDbfRecord r) => r.BagId == boosterRewardData.RewardChestBagNum.Value);
			switch (record.Reward)
			{
			case RewardBag.Reward.LATEST_PACK:
				packRecord = GameDbf.Booster.GetRecord((int)GameUtils.GetLatestRewardableBooster());
				break;
			case RewardBag.Reward.PACK_OFFSET_FROM_LATEST:
				packRecord = GameDbf.Booster.GetRecord((int)GameUtils.GetRewardableBoosterOffsetFromLatest(record.RewardData));
				break;
			default:
				Debug.LogWarning($"Unhandled RewardBag type: {record.Reward}");
				break;
			}
			if (packRecord != null)
			{
				boosterRewardData.Id = packRecord.ID;
			}
		}
		if (packRecord == null)
		{
			return;
		}
		SetReady(ready: false);
		GameObject go = AssetLoader.Get().InstantiatePrefab(packRecord.PackOpeningPrefab, AssetLoadingOptions.IgnorePrefabPosition);
		go.transform.parent = m_BoosterPackBone.transform.parent;
		go.transform.localPosition = m_BoosterPackBone.transform.localPosition;
		go.transform.rotation = m_BoosterPackBone.transform.rotation;
		go.transform.localScale = m_BoosterPackBone.transform.localScale;
		m_unopenedPack = go.GetComponent<UnopenedPack>();
		m_MeshRoot = go;
		if (m_unopenedPack.m_SingleStack.m_MeshRenderer != null)
		{
			Texture mainSingleTex = m_unopenedPack.m_SingleStack.m_MeshRenderer.GetSharedMaterial().mainTexture;
			m_unopenedPack.m_SingleStack.m_MeshRenderer.SetMaterial(m_PackGlowMaterial);
			m_unopenedPack.m_SingleStack.m_MeshRenderer.GetMaterial().mainTexture = mainSingleTex;
			if (m_unopenedPack.m_SingleStack.m_Shadow != null)
			{
				m_unopenedPack.m_SingleStack.m_Shadow.SetActive(value: false);
			}
		}
		if (m_unopenedPack.m_MultipleStack.m_MeshRenderer != null)
		{
			Texture mainMultiTex = m_unopenedPack.m_MultipleStack.m_MeshRenderer.GetSharedMaterial().mainTexture;
			m_unopenedPack.m_MultipleStack.m_MeshRenderer.SetMaterial(m_PackGlowMaterial);
			m_unopenedPack.m_MultipleStack.m_MeshRenderer.GetMaterial().mainTexture = mainMultiTex;
			if (m_unopenedPack.m_MultipleStack.m_Shadow != null)
			{
				m_unopenedPack.m_MultipleStack.m_Shadow.SetActive(value: false);
			}
		}
		UpdatePackStacks();
		SetReady(ready: true);
	}

	[ContextMenu("Play Rotate In Animation")]
	public void PlayRotateInAnimation()
	{
		StartCoroutine(RotateAnimation());
	}

	private void UpdatePackStacks()
	{
		if (!(base.Data is BoosterPackRewardData boosterRewardData))
		{
			Debug.LogWarning($"BoosterPackReward.UpdatePackStacks() - Data {base.Data} is not CardRewardData");
			return;
		}
		m_unopenedPack.SetBoosterId(boosterRewardData.Id);
		m_unopenedPack.SetCount(boosterRewardData.Count);
		bool openable = m_unopenedPack.CanOpenPack();
		bool useMultiStack = boosterRewardData.Count > 1;
		m_unopenedPack.m_SingleStack.m_RootObject.SetActive(!m_AllowMultiStack || !useMultiStack || !openable);
		m_unopenedPack.m_MultipleStack.m_RootObject.SetActive(useMultiStack && openable && m_AllowMultiStack);
		m_unopenedPack.m_AmountBanner.SetActive(useMultiStack);
		m_unopenedPack.m_AmountText.enabled = useMultiStack;
		if (useMultiStack)
		{
			m_unopenedPack.m_AmountText.Text = boosterRewardData.Count.ToString();
		}
	}

	private IEnumerator RotateAnimation()
	{
		float startTime = Time.timeSinceLevelLoad;
		while (Time.timeSinceLevelLoad - startTime < (float)m_RotationCurve.length)
		{
			float t = Time.timeSinceLevelLoad - startTime;
			m_unopenedPack.transform.localEulerAngles = new Vector3(m_unopenedPack.transform.localEulerAngles.x, m_unopenedPack.transform.localEulerAngles.y, m_RotationCurve.Evaluate(t));
			yield return null;
		}
	}
}
