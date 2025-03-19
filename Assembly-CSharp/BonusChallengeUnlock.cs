using System.Collections;
using UnityEngine;

[CustomEditClass]
public class BonusChallengeUnlock : Reward
{
	[CustomEditField(Sections = "Container")]
	public UIBObjectSpacing m_cardContainer;

	[CustomEditField(Sections = "Text Settings")]
	public UberText m_headerText;

	private Actor m_bonusChallengeBossActor;

	private ScreenEffectsHandle m_screenEffectsHandle;

	protected override void Awake()
	{
		base.Awake();
		if (!UniversalInputManager.UsePhoneUI)
		{
			m_rewardBanner.transform.localScale = m_rewardBanner.transform.localScale * 8f;
		}
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	protected override void InitData()
	{
		SetData(new BonusChallengeUnlockData(), updateVisuals: false);
	}

	protected override void ShowReward(bool updateCacheValues)
	{
		m_root.SetActive(value: true);
		m_cardContainer.UpdatePositions();
		m_cardContainer.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("amount", new Vector3(0f, 0f, 540f));
		args.Add("time", 1.5f);
		args.Add("easetype", iTween.EaseType.easeOutElastic);
		args.Add("space", Space.Self);
		iTween.RotateAdd(m_cardContainer.gameObject, args);
		ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignetteDesaturatePerspective;
		screenEffectParameters.Time = 1f;
		m_screenEffectsHandle.StartEffect(screenEffectParameters);
	}

	protected override void HideReward()
	{
		base.HideReward();
		m_screenEffectsHandle.StopEffect(DestroyBonusChallengeUnlock);
		m_root.SetActive(value: false);
	}

	protected override void OnDataSet(bool updateVisuals)
	{
		if (updateVisuals)
		{
			if (!(base.Data is BonusChallengeUnlockData unlockData))
			{
				Debug.LogWarning($"BonusChallengeUnlock.OnDataSet() - Data {base.Data} is not BonusChallengeUnlockData");
				return;
			}
			BannerManager.Get().ShowBanner(unlockData.PrefabToDisplay, null, GameStrings.Get("GLUE_ADVENTURE_DUNGEON_CRAWL_INTRO_BANNER_BUTTON"), HideReward);
			EnableClickCatcher(enabled: true);
		}
	}

	private void DestroyBonusChallengeUnlock()
	{
		Object.DestroyImmediate(base.gameObject);
	}
}
