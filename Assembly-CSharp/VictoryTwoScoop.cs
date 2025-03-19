using System.Collections;
using UnityEngine;

public class VictoryTwoScoop : EndGameTwoScoop
{
	public GameObject m_godRays;

	public GameObject m_godRays2;

	public GameObject m_rightTrumpet;

	public GameObject m_rightBanner;

	public GameObject m_rightCloud;

	public GameObject m_rightLaurel;

	public GameObject m_leftTrumpet;

	public GameObject m_leftBanner;

	public GameObject m_leftCloud;

	public GameObject m_leftLaurel;

	public GameObject m_crown;

	public AudioSource m_fireworksAudio;

	private const float GOD_RAY_ANGLE = 20f;

	private const float GOD_RAY_DURATION = 20f;

	private const float LAUREL_ROTATION = 2f;

	protected EntityDef m_overrideHeroEntityDef;

	protected DefLoader.DisposableCardDef m_overrideHeroCardDef;

	public void StopFireworksAudio()
	{
		if (m_fireworksAudio != null)
		{
			SoundManager.Get().Stop(m_fireworksAudio);
		}
	}

	public void SetOverrideHero(EntityDef overrideHero)
	{
		if (overrideHero != null)
		{
			if (!overrideHero.IsHero())
			{
				Log.Gameplay.PrintError("VictoryTwoScoop.SetOverrideHero() - passed EntityDef {0} is not a hero!", overrideHero);
				return;
			}
			DefLoader.DisposableCardDef cardDef = DefLoader.Get().GetCardDef(overrideHero.GetCardId());
			if (cardDef == null)
			{
				Log.Gameplay.PrintError("VictoryTwoScoop.SetOverrideHero() - passed EntityDef {0} does not have a CardDef!", overrideHero);
			}
			else
			{
				m_overrideHeroEntityDef = overrideHero;
				m_overrideHeroCardDef?.Dispose();
				m_overrideHeroCardDef = cardDef;
			}
		}
		else
		{
			m_overrideHeroEntityDef = null;
			m_overrideHeroCardDef?.Dispose();
			m_overrideHeroCardDef = null;
		}
	}

	public override void OnDestroy()
	{
		m_overrideHeroCardDef?.Dispose();
		m_overrideHeroCardDef = null;
		base.OnDestroy();
	}

	protected override void ShowImpl()
	{
		SetupHeroActor();
		SetupBannerText();
		PlayShowAnimations();
	}

	protected override void ResetPositions()
	{
		base.gameObject.transform.localPosition = EndGameTwoScoop.START_POSITION;
		base.gameObject.transform.eulerAngles = new Vector3(0f, 180f, 0f);
		if (m_rightTrumpet != null)
		{
			m_rightTrumpet.transform.localPosition = new Vector3(0.23f, -0.6f, 0.16f);
			m_rightTrumpet.transform.localScale = new Vector3(1f, 1f, 1f);
		}
		if (m_leftTrumpet != null)
		{
			m_leftTrumpet.transform.localPosition = new Vector3(-0.23f, -0.6f, 0.16f);
			m_leftTrumpet.transform.localScale = new Vector3(-1f, 1f, 1f);
		}
		if (m_rightBanner != null)
		{
			m_rightBanner.transform.localScale = new Vector3(1f, 1f, 0.08f);
		}
		if (m_leftBanner != null)
		{
			m_leftBanner.transform.localScale = new Vector3(1f, 1f, 0.08f);
		}
		if (m_rightCloud != null)
		{
			m_rightCloud.transform.localPosition = new Vector3(-0.2f, -0.8f, 0.26f);
		}
		if (m_leftCloud != null)
		{
			m_leftCloud.transform.localPosition = new Vector3(0.16f, -0.8f, 0.2f);
		}
		if (m_godRays != null)
		{
			m_godRays.transform.localEulerAngles = new Vector3(0f, 29f, 0f);
		}
		if (m_godRays2 != null)
		{
			m_godRays2.transform.localEulerAngles = new Vector3(0f, -29f, 0f);
		}
		if (m_crown != null)
		{
			m_crown.transform.localPosition = new Vector3(-0.041f, -0.04f, -0.834f);
		}
		if (m_rightLaurel != null)
		{
			m_rightLaurel.transform.localEulerAngles = new Vector3(0f, -90f, 0f);
			m_rightLaurel.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
		}
		if (m_leftLaurel != null)
		{
			m_leftLaurel.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
			m_leftLaurel.transform.localScale = new Vector3(-0.7f, 0.7f, 0.7f);
		}
	}

	public override void StopAnimating()
	{
		StopCoroutine("AnimateAll");
		iTween.Stop(base.gameObject, includechildren: true);
		StartCoroutine(ResetPositionsForGoldEvent());
	}

	protected void SetupHeroActor()
	{
		if (m_overrideHeroEntityDef != null && m_overrideHeroCardDef != null)
		{
			m_heroActor.SetEntityDef(m_overrideHeroEntityDef);
			m_heroActor.SetCardDef(m_overrideHeroCardDef);
			m_heroActor.UpdateAllComponents();
		}
		else
		{
			Entity hero = GameState.Get().GetFriendlySidePlayer().GetHero();
			if (hero != null)
			{
				m_heroActor.SetFullDefFromEntity(hero);
				m_heroActor.UpdateAllComponents();
				using DefLoader.DisposableCardDef cardDef = hero.ShareDisposableCardDef();
				GameObject cardDefObject = cardDef.CardDef?.gameObject;
				if (cardDefObject != null && cardDefObject.TryGetComponent<HeroCardSwitcher>(out var heroCardSwitcher))
				{
					heroCardSwitcher.OnGameVictory(m_heroActor);
				}
			}
		}
		m_heroActor.TurnOffCollider();
	}

	protected void SetupBannerText()
	{
		string bannerText = GameState.Get().GetGameEntity().GetVictoryScreenBannerText();
		SetBannerLabel(bannerText);
	}

	protected override Vector3 GetXPBarPosition()
	{
		if (m_heroActor.GetCustomFrameRequiresMetaCalibration())
		{
			return m_heroActor.GetCustomeFrameEndGameVictoryXPBarPosition();
		}
		return m_xpBarLocalPosition;
	}

	protected void PlayShowAnimations()
	{
		GetComponent<PlayMakerFSM>().SendEvent("Action");
		iTween.FadeTo(base.gameObject, 1f, 0.25f);
		base.gameObject.transform.localScale = new Vector3(EndGameTwoScoop.START_SCALE_VAL, EndGameTwoScoop.START_SCALE_VAL, EndGameTwoScoop.START_SCALE_VAL);
		Hashtable victoryScaleArgs = iTweenManager.Get().GetTweenHashTable();
		victoryScaleArgs.Add("scale", new Vector3(EndGameTwoScoop.END_SCALE_VAL, EndGameTwoScoop.END_SCALE_VAL, EndGameTwoScoop.END_SCALE_VAL));
		victoryScaleArgs.Add("time", 0.5f);
		victoryScaleArgs.Add("oncomplete", "PunchEndGameTwoScoop");
		victoryScaleArgs.Add("oncompletetarget", base.gameObject);
		iTween.ScaleTo(base.gameObject, victoryScaleArgs);
		Hashtable victoryMoveArgs = iTweenManager.Get().GetTweenHashTable();
		victoryMoveArgs.Add("position", base.gameObject.transform.position + new Vector3(0.005f, 0.005f, 0.005f));
		victoryMoveArgs.Add("time", 1.5f);
		victoryMoveArgs.Add("oncomplete", "TokyoDriftTo");
		victoryMoveArgs.Add("oncompletetarget", base.gameObject);
		iTween.MoveTo(base.gameObject, victoryMoveArgs);
		AnimateGodraysTo();
		AnimateCrownTo();
		StartCoroutine(AnimateAll());
		m_heroActor.LegendaryHeroPortrait?.RaiseAnimationEvent(LegendaryHeroAnimations.Victory);
	}

	private IEnumerator AnimateAll()
	{
		yield return new WaitForSeconds(0.25f);
		float TRUMPET_OUT_TIME = 0.4f;
		Hashtable trumpetMoveArgsRight = iTweenManager.Get().GetTweenHashTable();
		trumpetMoveArgsRight.Add("position", new Vector3(-0.52f, -0.6f, -0.23f));
		trumpetMoveArgsRight.Add("time", TRUMPET_OUT_TIME);
		trumpetMoveArgsRight.Add("islocal", true);
		trumpetMoveArgsRight.Add("easetype", iTween.EaseType.easeOutElastic);
		iTween.MoveTo(m_rightTrumpet, trumpetMoveArgsRight);
		Hashtable trumpetMoveArgsLeft = iTweenManager.Get().GetTweenHashTable();
		trumpetMoveArgsLeft.Add("position", new Vector3(0.44f, -0.6f, -0.23f));
		trumpetMoveArgsLeft.Add("time", TRUMPET_OUT_TIME);
		trumpetMoveArgsLeft.Add("islocal", true);
		trumpetMoveArgsLeft.Add("easetype", iTween.EaseType.easeOutElastic);
		iTween.MoveTo(m_leftTrumpet, trumpetMoveArgsLeft);
		Hashtable trumpetScaleArgsRight = iTweenManager.Get().GetTweenHashTable();
		trumpetScaleArgsRight.Add("scale", new Vector3(1.1f, 1.1f, 1.1f));
		trumpetScaleArgsRight.Add("time", 0.25f);
		trumpetScaleArgsRight.Add("delay", 0.3f);
		trumpetScaleArgsRight.Add("islocal", true);
		trumpetScaleArgsRight.Add("easetype", iTween.EaseType.easeOutBounce);
		iTween.ScaleTo(m_rightTrumpet, trumpetScaleArgsRight);
		Hashtable trumpetScaleArgsLeft = iTweenManager.Get().GetTweenHashTable();
		trumpetScaleArgsLeft.Add("scale", new Vector3(-1.1f, 1.1f, 1.1f));
		trumpetScaleArgsLeft.Add("time", 0.25f);
		trumpetScaleArgsLeft.Add("delay", 0.3f);
		trumpetScaleArgsLeft.Add("islocal", true);
		trumpetScaleArgsLeft.Add("easetype", iTween.EaseType.easeOutBounce);
		iTween.ScaleTo(m_leftTrumpet, trumpetScaleArgsLeft);
		Hashtable bannerScaleArgsRight = iTweenManager.Get().GetTweenHashTable();
		bannerScaleArgsRight.Add("z", 1f);
		bannerScaleArgsRight.Add("delay", 0.24f);
		bannerScaleArgsRight.Add("time", 1f);
		bannerScaleArgsRight.Add("islocal", true);
		bannerScaleArgsRight.Add("easetype", iTween.EaseType.easeOutElastic);
		iTween.ScaleTo(m_rightBanner, bannerScaleArgsRight);
		Hashtable bannerScaleArgsLeft = iTweenManager.Get().GetTweenHashTable();
		bannerScaleArgsLeft.Add("z", 1f);
		bannerScaleArgsLeft.Add("delay", 0.24f);
		bannerScaleArgsLeft.Add("time", 1f);
		bannerScaleArgsLeft.Add("islocal", true);
		bannerScaleArgsLeft.Add("easetype", iTween.EaseType.easeOutElastic);
		iTween.ScaleTo(m_leftBanner, bannerScaleArgsLeft);
		Hashtable cloudMoveArgsRight = iTweenManager.Get().GetTweenHashTable();
		cloudMoveArgsRight.Add("x", -1.227438f);
		cloudMoveArgsRight.Add("time", 5f);
		cloudMoveArgsRight.Add("islocal", true);
		cloudMoveArgsRight.Add("easetype", iTween.EaseType.easeOutCubic);
		cloudMoveArgsRight.Add("oncomplete", "CloudTo");
		cloudMoveArgsRight.Add("oncompletetarget", base.gameObject);
		iTween.MoveTo(m_rightCloud, cloudMoveArgsRight);
		Hashtable cloudMoveArgsLeft = iTweenManager.Get().GetTweenHashTable();
		cloudMoveArgsLeft.Add("x", 1.053244f);
		cloudMoveArgsLeft.Add("time", 5f);
		cloudMoveArgsLeft.Add("islocal", true);
		cloudMoveArgsLeft.Add("easetype", iTween.EaseType.easeOutCubic);
		iTween.MoveTo(m_leftCloud, cloudMoveArgsLeft);
		Hashtable laurelRotateArgsRight = iTweenManager.Get().GetTweenHashTable();
		laurelRotateArgsRight.Add("rotation", new Vector3(0f, 2f, 0f));
		laurelRotateArgsRight.Add("time", 0.5f);
		laurelRotateArgsRight.Add("islocal", true);
		laurelRotateArgsRight.Add("easetype", iTween.EaseType.easeOutElastic);
		laurelRotateArgsRight.Add("oncomplete", "LaurelWaveTo");
		laurelRotateArgsRight.Add("oncompletetarget", base.gameObject);
		iTween.RotateTo(m_rightLaurel, laurelRotateArgsRight);
		Hashtable laurelScaleArgsRight = iTweenManager.Get().GetTweenHashTable();
		laurelScaleArgsRight.Add("scale", new Vector3(1f, 1f, 1f));
		laurelScaleArgsRight.Add("time", 0.25f);
		laurelScaleArgsRight.Add("islocal", true);
		laurelScaleArgsRight.Add("easetype", iTween.EaseType.easeOutBounce);
		iTween.ScaleTo(m_rightLaurel, laurelScaleArgsRight);
		Hashtable laurelRotateArgsLeft = iTweenManager.Get().GetTweenHashTable();
		laurelRotateArgsLeft.Add("rotation", new Vector3(0f, -2f, 0f));
		laurelRotateArgsLeft.Add("time", 0.5f);
		laurelRotateArgsLeft.Add("islocal", true);
		laurelRotateArgsLeft.Add("easetype", iTween.EaseType.easeOutElastic);
		iTween.RotateTo(m_leftLaurel, laurelRotateArgsLeft);
		Hashtable laurelScaleArgsLeft = iTweenManager.Get().GetTweenHashTable();
		laurelScaleArgsLeft.Add("scale", new Vector3(-1f, 1f, 1f));
		laurelScaleArgsLeft.Add("time", 0.25f);
		laurelScaleArgsLeft.Add("islocal", true);
		laurelScaleArgsLeft.Add("easetype", iTween.EaseType.easeOutBounce);
		iTween.ScaleTo(m_leftLaurel, laurelScaleArgsLeft);
	}

	protected void TokyoDriftTo()
	{
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("position", EndGameTwoScoop.START_POSITION + new Vector3(0.2f, 0.2f, 0.2f));
		args.Add("time", 10f);
		args.Add("islocal", true);
		args.Add("easetype", iTween.EaseType.linear);
		args.Add("oncomplete", "TokyoDriftFro");
		args.Add("oncompletetarget", base.gameObject);
		iTween.MoveTo(base.gameObject, args);
	}

	private void TokyoDriftFro()
	{
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("position", EndGameTwoScoop.START_POSITION);
		args.Add("time", 10f);
		args.Add("islocal", true);
		args.Add("easetype", iTween.EaseType.linear);
		args.Add("oncomplete", "TokyoDriftTo");
		args.Add("oncompletetarget", base.gameObject);
		iTween.MoveTo(base.gameObject, args);
	}

	private void CloudTo()
	{
		Hashtable cloudMoveArgsRight = iTweenManager.Get().GetTweenHashTable();
		cloudMoveArgsRight.Add("x", -0.92f);
		cloudMoveArgsRight.Add("time", 10f);
		cloudMoveArgsRight.Add("islocal", true);
		cloudMoveArgsRight.Add("easetype", iTween.EaseType.linear);
		cloudMoveArgsRight.Add("oncomplete", "CloudFro");
		cloudMoveArgsRight.Add("oncompletetarget", base.gameObject);
		iTween.MoveTo(m_rightCloud, cloudMoveArgsRight);
		Hashtable cloudMoveArgsLeft = iTweenManager.Get().GetTweenHashTable();
		cloudMoveArgsLeft.Add("x", 0.82f);
		cloudMoveArgsLeft.Add("time", 10f);
		cloudMoveArgsLeft.Add("islocal", true);
		cloudMoveArgsLeft.Add("easetype", iTween.EaseType.linear);
		iTween.MoveTo(m_leftCloud, cloudMoveArgsLeft);
	}

	private void CloudFro()
	{
		Hashtable cloudMoveArgsRight = iTweenManager.Get().GetTweenHashTable();
		cloudMoveArgsRight.Add("x", -1.227438f);
		cloudMoveArgsRight.Add("time", 10f);
		cloudMoveArgsRight.Add("islocal", true);
		cloudMoveArgsRight.Add("easetype", iTween.EaseType.linear);
		cloudMoveArgsRight.Add("oncomplete", "CloudTo");
		cloudMoveArgsRight.Add("oncompletetarget", base.gameObject);
		iTween.MoveTo(m_rightCloud, cloudMoveArgsRight);
		Hashtable cloudMoveArgsLeft = iTweenManager.Get().GetTweenHashTable();
		cloudMoveArgsLeft.Add("x", 1.053244f);
		cloudMoveArgsLeft.Add("time", 10f);
		cloudMoveArgsLeft.Add("islocal", true);
		cloudMoveArgsLeft.Add("easetype", iTween.EaseType.linear);
		iTween.MoveTo(m_leftCloud, cloudMoveArgsLeft);
	}

	private void LaurelWaveTo()
	{
		Hashtable laurelRotationArgsRight = iTweenManager.Get().GetTweenHashTable();
		laurelRotationArgsRight.Add("rotation", new Vector3(0f, 0f, 0f));
		laurelRotationArgsRight.Add("time", 10f);
		laurelRotationArgsRight.Add("islocal", true);
		laurelRotationArgsRight.Add("easetype", iTween.EaseType.linear);
		laurelRotationArgsRight.Add("oncomplete", "LaurelWaveFro");
		laurelRotationArgsRight.Add("oncompletetarget", base.gameObject);
		iTween.RotateTo(m_rightLaurel, laurelRotationArgsRight);
		Hashtable laurelRotationArgsLeft = iTweenManager.Get().GetTweenHashTable();
		laurelRotationArgsLeft.Add("rotation", new Vector3(0f, 0f, 0f));
		laurelRotationArgsLeft.Add("time", 10f);
		laurelRotationArgsLeft.Add("islocal", true);
		laurelRotationArgsLeft.Add("easetype", iTween.EaseType.linear);
		iTween.RotateTo(m_leftLaurel, laurelRotationArgsLeft);
	}

	private void LaurelWaveFro()
	{
		Hashtable laurelRotationArgsRight = iTweenManager.Get().GetTweenHashTable();
		laurelRotationArgsRight.Add("rotation", new Vector3(0f, 2f, 0f));
		laurelRotationArgsRight.Add("time", 10f);
		laurelRotationArgsRight.Add("islocal", true);
		laurelRotationArgsRight.Add("easetype", iTween.EaseType.linear);
		laurelRotationArgsRight.Add("oncomplete", "LaurelWaveTo");
		laurelRotationArgsRight.Add("oncompletetarget", base.gameObject);
		iTween.RotateTo(m_rightLaurel, laurelRotationArgsRight);
		Hashtable laurelRotationArgsLeft = iTweenManager.Get().GetTweenHashTable();
		laurelRotationArgsLeft.Add("rotation", new Vector3(0f, -2f, 0f));
		laurelRotationArgsLeft.Add("time", 10f);
		laurelRotationArgsLeft.Add("islocal", true);
		laurelRotationArgsLeft.Add("easetype", iTween.EaseType.linear);
		iTween.RotateTo(m_leftLaurel, laurelRotationArgsLeft);
	}

	protected void AnimateCrownTo()
	{
		Hashtable crownMoveArgs = iTweenManager.Get().GetTweenHashTable();
		crownMoveArgs.Add("z", -0.78f);
		crownMoveArgs.Add("time", 5f);
		crownMoveArgs.Add("islocal", true);
		crownMoveArgs.Add("easetype", iTween.EaseType.easeInOutBack);
		crownMoveArgs.Add("oncomplete", "AnimateCrownFro");
		crownMoveArgs.Add("oncompletetarget", base.gameObject);
		iTween.MoveTo(m_crown, crownMoveArgs);
	}

	private void AnimateCrownFro()
	{
		Hashtable crownMoveArgs = iTweenManager.Get().GetTweenHashTable();
		crownMoveArgs.Add("z", -0.834f);
		crownMoveArgs.Add("time", 5f);
		crownMoveArgs.Add("islocal", true);
		crownMoveArgs.Add("easetype", iTween.EaseType.easeInOutBack);
		crownMoveArgs.Add("oncomplete", "AnimateCrownTo");
		crownMoveArgs.Add("oncompletetarget", base.gameObject);
		iTween.MoveTo(m_crown, crownMoveArgs);
	}

	protected void AnimateGodraysTo()
	{
		Hashtable godrayRotationArgs = iTweenManager.Get().GetTweenHashTable();
		godrayRotationArgs.Add("rotation", new Vector3(0f, -20f, 0f));
		godrayRotationArgs.Add("time", 20f);
		godrayRotationArgs.Add("islocal", true);
		godrayRotationArgs.Add("easetype", iTween.EaseType.linear);
		godrayRotationArgs.Add("oncomplete", "AnimateGodraysFro");
		godrayRotationArgs.Add("oncompletetarget", base.gameObject);
		iTween.RotateTo(m_godRays, godrayRotationArgs);
		Hashtable godray2RotationArgs = iTweenManager.Get().GetTweenHashTable();
		godray2RotationArgs.Add("rotation", new Vector3(0f, 20f, 0f));
		godray2RotationArgs.Add("time", 20f);
		godray2RotationArgs.Add("islocal", true);
		godray2RotationArgs.Add("easetype", iTween.EaseType.linear);
		iTween.RotateTo(m_godRays2, godray2RotationArgs);
	}

	private void AnimateGodraysFro()
	{
		Hashtable godrayRotationArgs = iTweenManager.Get().GetTweenHashTable();
		godrayRotationArgs.Add("rotation", new Vector3(0f, 20f, 0f));
		godrayRotationArgs.Add("time", 20f);
		godrayRotationArgs.Add("islocal", true);
		godrayRotationArgs.Add("easetype", iTween.EaseType.linear);
		godrayRotationArgs.Add("oncomplete", "AnimateGodraysTo");
		godrayRotationArgs.Add("oncompletetarget", base.gameObject);
		iTween.RotateTo(m_godRays, godrayRotationArgs);
		Hashtable godray2RotationArgs = iTweenManager.Get().GetTweenHashTable();
		godray2RotationArgs.Add("rotation", new Vector3(0f, -20f, 0f));
		godray2RotationArgs.Add("time", 20f);
		godray2RotationArgs.Add("islocal", true);
		godray2RotationArgs.Add("easetype", iTween.EaseType.linear);
		iTween.RotateTo(m_godRays2, godray2RotationArgs);
	}

	private IEnumerator ResetPositionsForGoldEvent()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		float resetTime = 0.25f;
		Hashtable cloudMoveArgsRight = iTweenManager.Get().GetTweenHashTable();
		cloudMoveArgsRight.Add("position", new Vector3(-1.211758f, -0.8f, -0.2575677f));
		cloudMoveArgsRight.Add("time", resetTime);
		cloudMoveArgsRight.Add("islocal", true);
		cloudMoveArgsRight.Add("easetype", iTween.EaseType.linear);
		iTween.MoveTo(m_rightCloud, cloudMoveArgsRight);
		Hashtable cloudMoveArgsLeft = iTweenManager.Get().GetTweenHashTable();
		cloudMoveArgsLeft.Add("position", new Vector3(1.068925f, -0.8f, -0.197469f));
		cloudMoveArgsLeft.Add("time", resetTime);
		cloudMoveArgsLeft.Add("islocal", true);
		cloudMoveArgsLeft.Add("easetype", iTween.EaseType.linear);
		iTween.MoveTo(m_leftCloud, cloudMoveArgsLeft);
		m_rightLaurel.transform.localRotation = Quaternion.Euler(Vector3.zero);
		Hashtable laurelMoveArgsRight = iTweenManager.Get().GetTweenHashTable();
		laurelMoveArgsRight.Add("position", new Vector3(0.1723f, -0.206f, 0.753f));
		laurelMoveArgsRight.Add("time", resetTime);
		laurelMoveArgsRight.Add("islocal", true);
		laurelMoveArgsRight.Add("easetype", iTween.EaseType.linear);
		iTween.MoveTo(m_rightLaurel, laurelMoveArgsRight);
		m_leftLaurel.transform.localRotation = Quaternion.Euler(Vector3.zero);
		Hashtable laurelMoveArgsLeft = iTweenManager.Get().GetTweenHashTable();
		laurelMoveArgsLeft.Add("position", new Vector3(-0.2201783f, -0.318f, 0.753f));
		laurelMoveArgsLeft.Add("time", resetTime);
		laurelMoveArgsLeft.Add("islocal", true);
		laurelMoveArgsLeft.Add("easetype", iTween.EaseType.linear);
		iTween.MoveTo(m_leftLaurel, laurelMoveArgsLeft);
		Hashtable crownMoveArgs = iTweenManager.Get().GetTweenHashTable();
		crownMoveArgs.Add("z", -0.9677765f);
		crownMoveArgs.Add("time", resetTime);
		crownMoveArgs.Add("islocal", true);
		crownMoveArgs.Add("easetype", iTween.EaseType.easeInOutBack);
		iTween.MoveTo(m_crown, crownMoveArgs);
		Hashtable godrayRotationArgs = iTweenManager.Get().GetTweenHashTable();
		godrayRotationArgs.Add("rotation", new Vector3(0f, 20f, 0f));
		godrayRotationArgs.Add("time", resetTime);
		godrayRotationArgs.Add("islocal", true);
		godrayRotationArgs.Add("easetype", iTween.EaseType.linear);
		iTween.RotateTo(m_godRays, godrayRotationArgs);
		Hashtable godray2RotationArgs = iTweenManager.Get().GetTweenHashTable();
		godray2RotationArgs.Add("rotation", new Vector3(0f, -20f, 0f));
		godray2RotationArgs.Add("time", resetTime);
		godray2RotationArgs.Add("islocal", true);
		godray2RotationArgs.Add("easetype", iTween.EaseType.linear);
		iTween.RotateTo(m_godRays2, godray2RotationArgs);
	}
}
