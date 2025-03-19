using System.Collections;
using UnityEngine;

public class EndGameTwoScoop : MonoBehaviour
{
	public UberText m_bannerLabel;

	public GameObject m_heroBone;

	public GameObject m_duoHeroBone;

	public GameObject m_duoTeammateHeroBone;

	public Actor m_heroActor;

	public Actor m_teammateHeroActor;

	public HeroXPBar m_xpBarPrefab;

	public Vector3 m_xpBarLocalScale = new Vector3(0.9064f, 0.9064f, 0.9064f);

	public Vector3 m_xpBarLocalPosition = new Vector3(-0.166f, 0.224f, -0.738f);

	public GameObject m_levelUpTier1;

	public GameObject m_levelUpTier2;

	public GameObject m_levelUpTier3;

	protected bool m_heroActorLoaded;

	protected HeroXPBar m_xpBar;

	private bool m_isShown;

	private static readonly float AFTER_PUNCH_SCALE_VAL = 2.3f;

	protected static readonly float START_SCALE_VAL = 0.01f;

	protected static readonly float END_SCALE_VAL = 2.5f;

	protected static readonly Vector3 START_POSITION = new Vector3(-7.8f, 8.2f, -5f);

	protected static readonly float BAR_ANIMATION_DELAY = 1f;

	public virtual void Awake()
	{
		base.gameObject.SetActive(value: false);
		GameObject placementBone = m_heroBone;
		if (GameMgr.Get().IsBattlegroundDuoGame())
		{
			placementBone = m_duoHeroBone;
			AssetLoader.Get().InstantiatePrefab(GetActorName(), OnTeammateHeroActorLoaded, m_duoTeammateHeroBone, AssetLoadingOptions.IgnorePrefabPosition);
		}
		AssetLoader.Get().InstantiatePrefab(GetActorName(), OnHeroActorLoaded, placementBone, AssetLoadingOptions.IgnorePrefabPosition);
	}

	public virtual void OnDestroy()
	{
	}

	private void Start()
	{
		LayerUtils.SetLayer(base.gameObject, GameLayer.IgnoreFullScreenEffects);
		ResetPositions();
	}

	public bool IsShown()
	{
		return m_isShown;
	}

	public void Show(bool showXPBar = true)
	{
		m_isShown = true;
		base.gameObject.SetActive(value: true);
		ShowImpl();
		if (!showXPBar || GameMgr.Get().IsTraditionalTutorial() || GameMgr.Get().IsSpectator())
		{
			return;
		}
		NetCache.HeroLevel heroLevel = null;
		int totalLevel = 0;
		Entity heroEntity = GameState.Get().GetFriendlySidePlayer().GetStartingHero();
		if (heroEntity != null)
		{
			heroLevel = GameUtils.GetHeroLevel(heroEntity.GetClass());
			totalLevel = GameUtils.GetTotalHeroLevel().GetValueOrDefault();
		}
		if (heroLevel == null)
		{
			HideXpBar();
		}
		else if (m_xpBarPrefab != null)
		{
			m_xpBar = Object.Instantiate(m_xpBarPrefab);
			if (m_heroActor.m_xpBarRootObject != null)
			{
				m_xpBar.transform.parent = m_heroActor.m_xpBarRootObject.transform;
				m_xpBar.transform.localScale = Vector3.one;
				m_xpBar.transform.localPosition = Vector3.zero;
			}
			else
			{
				m_xpBar.transform.parent = m_heroActor.transform;
				m_xpBar.transform.localPosition = GetXPBarPosition();
				m_xpBar.transform.localScale = GetXPBarScale();
			}
			NetCache.NetCacheFeatures guardianVars = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
			m_xpBar.m_soloLevelLimit = guardianVars?.XPSoloLimit ?? 60;
			m_xpBar.m_isAnimated = true;
			m_xpBar.m_delay = BAR_ANIMATION_DELAY;
			m_xpBar.m_levelUpCallback = PlayLevelUpEffect;
			m_xpBar.UpdateDisplay(heroLevel, totalLevel);
		}
	}

	protected virtual Vector3 GetXPBarPosition()
	{
		return m_xpBarLocalPosition;
	}

	protected virtual Vector3 GetXPBarScale()
	{
		if (m_heroActor.GetCustomFrameRequiresMetaCalibration())
		{
			return m_heroActor.GetCustomFrameEndGameXPBarScale();
		}
		return m_xpBarLocalScale;
	}

	public void Hide()
	{
		HideAll();
	}

	public virtual bool IsLoaded()
	{
		return m_heroActorLoaded;
	}

	public void HideXpBar()
	{
		if (m_xpBar != null)
		{
			m_xpBar.gameObject.SetActive(value: false);
		}
	}

	public virtual void StopAnimating()
	{
	}

	protected virtual string GetActorName()
	{
		return "Card_Play_Hero.prefab:42cbbd2c4969afb46b3887bb628de19d";
	}

	protected virtual void ShowImpl()
	{
	}

	protected virtual void ResetPositions()
	{
	}

	protected void SetBannerLabel(string label)
	{
		m_bannerLabel.Text = label;
	}

	protected void EnableBannerLabel(bool enable)
	{
		m_bannerLabel.gameObject.SetActive(enable);
	}

	protected void PunchEndGameTwoScoop()
	{
		if (EndGameScreen.Get() != null)
		{
			EndGameScreen.Get().SetPlayingBlockingAnim(set: false);
		}
		iTween.ScaleTo(base.gameObject, new Vector3(AFTER_PUNCH_SCALE_VAL, AFTER_PUNCH_SCALE_VAL, AFTER_PUNCH_SCALE_VAL), 0.15f);
		if (m_heroActor != null)
		{
			m_heroActor.UpdateCustomFrameDiamondMaterial();
		}
	}

	private void HideAll()
	{
		ScreenEffectsMgr.Get().SetActive(enabled: false);
		Hashtable scaleArgs = iTweenManager.Get().GetTweenHashTable();
		scaleArgs.Add("scale", new Vector3(START_SCALE_VAL, START_SCALE_VAL, START_SCALE_VAL));
		scaleArgs.Add("time", 0.25f);
		scaleArgs.Add("oncomplete", "OnAllHidden");
		scaleArgs.Add("oncompletetarget", base.gameObject);
		iTween.FadeTo(base.gameObject, 0f, 0.25f);
		iTween.ScaleTo(base.gameObject, scaleArgs);
		m_isShown = false;
	}

	private void OnAllHidden()
	{
		iTween.FadeTo(base.gameObject, 0f, 0f);
		base.gameObject.SetActive(value: false);
		ResetPositions();
	}

	private void OnHeroActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		GameObject bone = (GameObject)callbackData;
		go.transform.parent = base.transform;
		go.transform.localPosition = bone.transform.localPosition;
		go.transform.localScale = bone.transform.localScale;
		go.transform.localRotation = bone.transform.localRotation;
		m_heroActor = go.GetComponent<Actor>();
		m_heroActor.TurnOffCollider();
		m_heroActor.m_healthObject.SetActive(value: false);
		m_heroActorLoaded = true;
		Card heroCard = GameState.Get().GetFriendlySidePlayer().GetHeroCard();
		if (heroCard != null)
		{
			m_heroActor.SetPremium(heroCard.GetPremium());
		}
		m_heroActor.UpdateAllComponents();
	}

	private void OnTeammateHeroActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		GameObject bone = (GameObject)callbackData;
		go.transform.parent = base.transform;
		go.transform.localPosition = bone.transform.localPosition;
		go.transform.localScale = bone.transform.localScale;
		go.transform.localRotation = bone.transform.localRotation;
		m_teammateHeroActor = go.GetComponent<Actor>();
		m_teammateHeroActor.TurnOffCollider();
		m_teammateHeroActor.m_healthObject.SetActive(value: false);
		Entity hero = TeammateBoardViewer.Get().GetTeammateHero();
		if (hero != null && hero.GetCard() != null)
		{
			m_teammateHeroActor.SetPremium(hero.GetCard().GetPremium());
		}
		m_teammateHeroActor.SetActorSpecificOverlayActive(hero == null);
		m_teammateHeroActor.UpdateAllComponents();
		if (hero == null)
		{
			m_teammateHeroActor.m_healthObject.SetActive(value: false);
			m_teammateHeroActor.m_attackObject.SetActive(value: false);
		}
	}

	protected void PlayLevelUpEffect()
	{
		GameObject levelUpEffect = Object.Instantiate(m_levelUpTier1);
		if ((bool)levelUpEffect)
		{
			levelUpEffect.transform.parent = base.transform;
			levelUpEffect.GetComponent<PlayMakerFSM>().SendEvent("Birth");
		}
	}
}
