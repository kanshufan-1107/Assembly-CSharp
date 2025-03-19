using System;
using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class AdventureRewardsDisplayArea : MonoBehaviour
{
	public delegate void RewardsHidden();

	private const float BlurVignetteTime = 0.25f;

	[CustomEditField(Sections = "UI")]
	public GameObject m_RewardsCardArea;

	[CustomEditField(Sections = "UI")]
	public Vector3 m_RewardsDefaultOffset;

	[CustomEditField(Sections = "UI")]
	public Vector3 m_RewardsHeroSkinOffset;

	[CustomEditField(Sections = "UI")]
	public float m_RewardsCardMouseOffset;

	[CustomEditField(Sections = "UI")]
	public Vector3 m_RewardsCardScale;

	[CustomEditField(Sections = "UI")]
	public Vector3 m_RewardsHeroSkinScale;

	[CustomEditField(Sections = "UI")]
	public Vector3 m_RewardsCardBackScale;

	[CustomEditField(Sections = "UI")]
	public Vector3 m_RewardsBoosterScale;

	[CustomEditField(Sections = "UI")]
	public float m_RewardsDefaultSpacing = 10f;

	[CustomEditField(Sections = "UI")]
	public Vector3 m_RewardsCardDriftAmount;

	[CustomEditField(Sections = "UI")]
	public bool m_EnableFullscreenMode;

	[CustomEditField(Sections = "UI", Parent = "m_EnableFullscreenMode")]
	public PegUIElement m_FullscreenModeOffClicker;

	[CustomEditField(Sections = "UI", Parent = "m_EnableFullscreenMode")]
	public UIBScrollable m_FullscreenDisableScrollBar;

	[CustomEditField(Sections = "Sounds", T = EditType.SOUND_PREFAB)]
	public string m_CardPreviewAppearSound;

	private List<GameObject> m_CurrentRewards = new List<GameObject>();

	private bool m_FullscreenEnabled;

	private bool m_Showing;

	private List<RewardsHidden> m_RewardsHiddenListeners = new List<RewardsHidden>();

	private ScreenEffectsHandle m_screenEffectsHandle;

	private void Awake()
	{
		if (m_FullscreenModeOffClicker != null)
		{
			m_FullscreenModeOffClicker.AddEventListener(UIEventType.PRESS, delegate
			{
				HideRewards();
			});
		}
		if (m_FullscreenDisableScrollBar != null)
		{
			m_FullscreenDisableScrollBar.AddTouchScrollStartedListener(HideRewards);
		}
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	private void OnDestroy()
	{
		DisableFullscreen();
	}

	public bool IsShowing()
	{
		return m_Showing;
	}

	public void ShowRewardsNoFullscreen(List<RewardData> rewards, Vector3 finalPosition, Vector3? origin = null)
	{
		DoShowRewards(rewards, finalPosition, origin, disableFullscreen: true);
	}

	public void ShowRewards(List<RewardData> rewards, Vector3 finalPosition, Vector3? origin = null)
	{
		if (!m_Showing)
		{
			m_Showing = true;
			if (m_EnableFullscreenMode)
			{
				DoShowRewards(rewards, null, origin, disableFullscreen: false);
			}
			else
			{
				DoShowRewards(rewards, finalPosition, origin, disableFullscreen: false);
			}
		}
	}

	public void HideRewards()
	{
		m_Showing = false;
		foreach (GameObject reward in m_CurrentRewards)
		{
			if (reward != null)
			{
				UnityEngine.Object.Destroy(reward);
			}
		}
		m_CurrentRewards.Clear();
		DisableFullscreen();
		FireRewardsHiddenEvent();
	}

	public void AddRewardsHiddenListener(RewardsHidden dlg)
	{
		m_RewardsHiddenListeners.Add(dlg);
	}

	public void RemoveRewardsHiddenListener(RewardsHidden dlg)
	{
		m_RewardsHiddenListeners.Remove(dlg);
	}

	public List<GameObject> GetCurrentShownRewards()
	{
		if (!m_Showing)
		{
			return null;
		}
		return m_CurrentRewards;
	}

	private void FireRewardsHiddenEvent()
	{
		RewardsHidden[] array = m_RewardsHiddenListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i]();
		}
	}

	private void DoShowRewards(ICollection<RewardData> rewards, Vector3? finalPosition, Vector3? origin, bool disableFullscreen)
	{
		int i = 0;
		int rewardCount = rewards.Count;
		Vector3 positionOffset = m_RewardsDefaultOffset;
		Vector3 scale = Vector3.one;
		foreach (RewardData reward in rewards)
		{
			GameObject rewardObject = null;
			switch (reward.RewardType)
			{
			case Reward.Type.CARD:
			{
				string cardId = ((CardRewardData)reward).CardID;
				using (DefLoader.DisposableFullDef fullDef = DefLoader.Get().GetFullDef(cardId))
				{
					bool num = fullDef.EntityDef.IsHeroSkin();
					string assetPath = (num ? ActorNames.GetHeroSkinOrHandActor(fullDef.EntityDef, TAG_PREMIUM.NORMAL) : ActorNames.GetHandActor(fullDef.EntityDef, TAG_PREMIUM.NORMAL));
					rewardObject = AssetLoader.Get().InstantiatePrefab(assetPath, AssetLoadingOptions.IgnorePrefabPosition);
					rewardObject.GetComponentInChildren<Collider>().enabled = false;
					Actor actor = rewardObject.GetComponent<Actor>();
					actor.SetFullDef(fullDef);
					actor.CreateBannedRibbon();
					if (num)
					{
						rewardObject.GetComponent<CollectionHeroSkin>().SetClass(fullDef.EntityDef);
						scale = m_RewardsHeroSkinScale;
						positionOffset = m_RewardsHeroSkinOffset;
					}
					else
					{
						scale = m_RewardsCardScale;
						positionOffset = m_RewardsDefaultOffset;
					}
					if (actor.m_cardMesh != null)
					{
						BoxCollider collider = actor.m_cardMesh.GetComponent<BoxCollider>();
						if (collider != null)
						{
							collider.enabled = false;
						}
					}
				}
				break;
			}
			case Reward.Type.CARD_BACK:
			{
				CardBackManager.LoadCardBackData loadCardBackData = CardBackManager.Get().LoadCardBackByIndex(((CardBackRewardData)reward).CardBackID);
				scale = m_RewardsCardBackScale;
				rewardObject = loadCardBackData.m_GameObject;
				break;
			}
			case Reward.Type.BOOSTER_PACK:
			{
				BoosterDbfRecord boosterRecord = GameDbf.Booster.GetRecord(((BoosterPackRewardData)reward).Id);
				if (boosterRecord == null)
				{
					continue;
				}
				rewardObject = AssetLoader.Get().InstantiatePrefab(boosterRecord.PackOpeningPrefab, AssetLoadingOptions.IgnorePrefabPosition);
				scale = m_RewardsBoosterScale;
				UnopenedPack component = rewardObject.GetComponent<UnopenedPack>();
				component.SetBoosterId(((BoosterPackRewardData)reward).Id);
				component.SetCount(((BoosterPackRewardData)reward).Count);
				component.GetComponent<Collider>().enabled = false;
				break;
			}
			case Reward.Type.RANDOM_CARD:
				rewardObject = AssetLoader.Get().InstantiatePrefab("Card_Random_Reward.prefab:403211800142ebf4593a290b92655167", AssetLoadingOptions.IgnorePrefabPosition);
				scale = m_RewardsCardScale;
				break;
			}
			if (!(rewardObject == null))
			{
				m_CurrentRewards.Add(rewardObject);
				GameUtils.SetParent(rewardObject, m_RewardsCardArea);
				ShowRewardsObject(rewardObject, finalPosition, origin, positionOffset, scale, i, rewardCount);
				i++;
			}
		}
		EnableFullscreen(disableFullscreen);
	}

	private void ShowRewardsObject(GameObject obj, Vector3? finalPosition, Vector3? origin, Vector3 positionOffset, Vector3 scale, int index, int totalCount)
	{
		Vector3 position;
		if (finalPosition.HasValue)
		{
			Collider component = GetComponent<Collider>();
			Vector3 minarea = component.bounds.min;
			Vector3 maxarea = component.bounds.max;
			position = finalPosition.Value + positionOffset;
			float spacing = (float)index * m_RewardsDefaultSpacing;
			position.z = Mathf.Clamp(position.z, minarea.z, maxarea.z);
			if (position.x + m_RewardsCardMouseOffset > maxarea.x)
			{
				position.x -= m_RewardsCardMouseOffset + spacing;
			}
			else
			{
				position.x += m_RewardsCardMouseOffset + spacing;
			}
		}
		else
		{
			position = m_RewardsCardArea.transform.position + positionOffset;
			float spacing2 = (float)index * m_RewardsDefaultSpacing;
			position.x += spacing2;
			position.x -= (float)(totalCount - 1) * m_RewardsDefaultSpacing * 0.5f;
		}
		obj.transform.localScale = scale;
		obj.transform.position = position;
		obj.SetActive(value: true);
		if (m_EnableFullscreenMode)
		{
			LayerUtils.SetLayer(obj, GameLayer.IgnoreFullScreenEffects);
			if (m_FullscreenModeOffClicker != null)
			{
				LayerUtils.SetLayer(m_FullscreenModeOffClicker, GameLayer.IgnoreFullScreenEffects);
			}
		}
		iTween.StopByName(obj, "REWARD_SCALE_UP");
		iTween.ScaleFrom(obj, iTween.Hash("scale", Vector3.one * 0.05f, "time", 0.15f, "easetype", iTween.EaseType.easeOutQuart, "name", "REWARD_SCALE_UP"));
		if (origin.HasValue)
		{
			iTween.StopByName(obj, "REWARD_MOVE_FROM_ORIGIN");
			iTween.MoveFrom(obj, iTween.Hash("position", origin.Value, "time", 0.15f, "easetype", iTween.EaseType.easeOutQuart, "name", "REWARD_MOVE_FROM_ORIGIN", "oncomplete", (Action<object>)delegate
			{
				if (m_RewardsCardDriftAmount != Vector3.zero)
				{
					AnimationUtil.DriftObject(obj, m_RewardsCardDriftAmount);
				}
			}));
		}
		else if (m_RewardsCardDriftAmount != Vector3.zero)
		{
			AnimationUtil.DriftObject(obj, m_RewardsCardDriftAmount);
		}
		if (!string.IsNullOrEmpty(m_CardPreviewAppearSound))
		{
			SoundManager.Get().LoadAndPlay(m_CardPreviewAppearSound);
		}
	}

	private void EnableFullscreen(bool disableFullscreen)
	{
		if (m_EnableFullscreenMode && !disableFullscreen)
		{
			ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignetteDesaturatePerspective;
			screenEffectParameters.Time = 0.25f;
			m_screenEffectsHandle.StartEffect(screenEffectParameters);
			if (m_FullscreenModeOffClicker != null)
			{
				m_FullscreenModeOffClicker.gameObject.SetActive(value: true);
			}
			m_FullscreenEnabled = true;
		}
	}

	private void DisableFullscreen()
	{
		if (m_FullscreenEnabled)
		{
			if (FullScreenFXMgr.Get() != null)
			{
				m_screenEffectsHandle.StopEffect();
			}
			if (m_FullscreenModeOffClicker != null)
			{
				m_FullscreenModeOffClicker.gameObject.SetActive(value: false);
			}
			m_FullscreenEnabled = false;
		}
	}
}
