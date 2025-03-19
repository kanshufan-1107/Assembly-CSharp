using UnityEngine;

[CustomEditClass]
public class AdventureCompleteReward : Reward
{
	private const string s_EventShowHurt = "ShowHurt";

	private const string s_EventShowBadlyHurt = "ShowBadlyHurt";

	private const string s_EventHide = "Hide";

	[CustomEditField(Sections = "State Event Table")]
	public StateEventTable m_StateTable;

	[CustomEditField(Sections = "Banner")]
	public UberText m_BannerTextObject;

	[CustomEditField(Sections = "Banner")]
	public GameObject m_BannerObject;

	[CustomEditField(Sections = "Banner")]
	public Vector3_MobileOverride m_BannerScaleOverride;

	private ScreenEffectsHandle m_screenEffectsHandle;

	protected override void InitData()
	{
		SetData(new AdventureCompleteRewardData(), updateVisuals: false);
	}

	protected override void ShowReward(bool updateCacheValues)
	{
		if (base.IsShown)
		{
			return;
		}
		AdventureCompleteRewardData rewardData = base.Data as AdventureCompleteRewardData;
		if (m_StateTable != null)
		{
			string eventName = ((GameUtils.IsModeHeroic(rewardData.ModeId) && m_StateTable.HasState("ShowBadlyHurt")) ? "ShowBadlyHurt" : "ShowHurt");
			m_StateTable.TriggerState(eventName);
		}
		if (m_BannerTextObject != null)
		{
			m_BannerTextObject.Text = rewardData.BannerText;
		}
		if (m_BannerObject != null && m_BannerScaleOverride != null)
		{
			Vector3 scale = m_BannerScaleOverride;
			if (scale != Vector3.zero)
			{
				m_BannerObject.transform.localScale = scale;
			}
		}
		FadeFullscreenEffectsIn();
	}

	protected override void PlayShowSounds()
	{
	}

	protected override void HideReward()
	{
		if (base.IsShown)
		{
			base.HideReward();
			if (m_StateTable != null)
			{
				m_StateTable.TriggerState("Hide");
			}
			FadeFullscreenEffectsOut();
		}
	}

	private void FadeFullscreenEffectsIn()
	{
		if (m_screenEffectsHandle == null)
		{
			m_screenEffectsHandle = new ScreenEffectsHandle(this);
		}
		ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignettePerspective;
		screenEffectParameters.Blur = new BlurParameters(1f, 0.85f);
		m_screenEffectsHandle.StartEffect(screenEffectParameters);
	}

	private void FadeFullscreenEffectsOut()
	{
		if (FullScreenFXMgr.Get() == null)
		{
			Debug.LogWarning("AdventureCompleteReward: FullScreenFXMgr.Get() returned null!");
		}
		else
		{
			m_screenEffectsHandle.StopEffect();
		}
	}

	protected override void OnDataSet(bool updateVisuals)
	{
		if (!updateVisuals)
		{
			return;
		}
		if (!(base.Data is AdventureCompleteRewardData))
		{
			Debug.LogWarning($"AdventureCompleteReward.OnDataSet() - Data {base.Data} is not AdventureCompleteRewardData");
			return;
		}
		EnableClickCatcher(enabled: true);
		RegisterClickListener(delegate
		{
			HideReward();
		});
		SetReady(ready: true);
	}

	private void DestroyThis()
	{
		Object.DestroyImmediate(base.gameObject);
	}
}
