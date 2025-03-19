using System.Collections;
using UnityEngine;

public class DefeatTwoScoop : EndGameTwoScoop
{
	public GameObject m_rightTrumpet;

	public GameObject m_rightBanner;

	public GameObject m_rightBannerShred;

	public GameObject m_rightCloud;

	public GameObject m_leftTrumpet;

	public GameObject m_leftBanner;

	public GameObject m_leftBannerFront;

	public GameObject m_leftCloud;

	public GameObject m_crown;

	public GameObject m_defeatBanner;

	protected override void ShowImpl()
	{
		Entity hero = GameState.Get().GetFriendlySidePlayer().GetHero();
		if (hero != null)
		{
			m_heroActor.SetFullDefFromEntity(hero);
			m_heroActor.UpdateAllComponents();
		}
		m_heroActor.TurnOffCollider();
		GameEntity gameEntity = GameState.Get().GetGameEntity();
		string bannerText = gameEntity.GetDefeatScreenBannerText();
		if (GameMgr.Get().LastGameData.GameResult == TAG_PLAYSTATE.TIED)
		{
			bannerText = gameEntity.GetTieScreenBannerText();
		}
		SetBannerLabel(bannerText);
		GetComponent<PlayMakerFSM>().SendEvent("Action");
		iTween.FadeTo(base.gameObject, 1f, 0.25f);
		base.gameObject.transform.localScale = new Vector3(EndGameTwoScoop.START_SCALE_VAL, EndGameTwoScoop.START_SCALE_VAL, EndGameTwoScoop.START_SCALE_VAL);
		Hashtable defeatScaleArgs = iTweenManager.Get().GetTweenHashTable();
		defeatScaleArgs.Add("scale", new Vector3(EndGameTwoScoop.END_SCALE_VAL, EndGameTwoScoop.END_SCALE_VAL, EndGameTwoScoop.END_SCALE_VAL));
		defeatScaleArgs.Add("time", 0.5f);
		defeatScaleArgs.Add("oncomplete", "PunchEndGameTwoScoop");
		defeatScaleArgs.Add("oncompletetarget", base.gameObject);
		defeatScaleArgs.Add("easetype", iTween.EaseType.easeOutBounce);
		iTween.ScaleTo(base.gameObject, defeatScaleArgs);
		Hashtable defeatMoveArgs = iTweenManager.Get().GetTweenHashTable();
		defeatMoveArgs.Add("position", base.gameObject.transform.position + new Vector3(0.005f, 0.005f, 0.005f));
		defeatMoveArgs.Add("time", 1.5f);
		defeatMoveArgs.Add("oncomplete", "TokyoDriftTo");
		defeatMoveArgs.Add("oncompletetarget", base.gameObject);
		iTween.MoveTo(base.gameObject, defeatMoveArgs);
		AnimateCrownTo();
		AnimateLeftTrumpetTo();
		AnimateRightTrumpetTo();
		StartCoroutine(AnimateAll());
		m_heroActor.LegendaryHeroPortrait?.RaiseAnimationEvent(LegendaryHeroAnimations.Defeat);
		using DefLoader.DisposableCardDef cardDef = hero.ShareDisposableCardDef();
		GameObject cardDefObject = cardDef.CardDef?.gameObject;
		if (cardDefObject != null && cardDefObject.TryGetComponent<HeroCardSwitcher>(out var heroCardSwitcher))
		{
			heroCardSwitcher.OnGameDefeat(m_heroActor);
		}
	}

	protected override void ResetPositions()
	{
		base.gameObject.transform.localPosition = EndGameTwoScoop.START_POSITION;
		base.gameObject.transform.eulerAngles = new Vector3(0f, 180f, 0f);
		m_rightTrumpet.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		m_rightTrumpet.transform.localEulerAngles = new Vector3(0f, -180f, 0f);
		m_leftTrumpet.transform.localEulerAngles = new Vector3(0f, -180f, 0f);
		m_rightBanner.transform.localScale = new Vector3(1f, 1f, -0.0375f);
		m_rightBannerShred.transform.localScale = new Vector3(1f, 1f, 0.05f);
		m_rightCloud.transform.localPosition = new Vector3(-0.036f, -0.3f, 0.46f);
		m_leftCloud.transform.localPosition = new Vector3(-0.047f, -0.3f, 0.41f);
		m_crown.transform.localEulerAngles = new Vector3(-0.026f, 17f, 0.2f);
		m_defeatBanner.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
	}

	private IEnumerator AnimateAll()
	{
		yield return new WaitForSeconds(0.25f);
		Hashtable trumpetScaleArgsRight = iTweenManager.Get().GetTweenHashTable();
		trumpetScaleArgsRight.Add("scale", new Vector3(1f, 1f, 1.1f));
		trumpetScaleArgsRight.Add("time", 0.25f);
		trumpetScaleArgsRight.Add("islocal", true);
		trumpetScaleArgsRight.Add("easetype", iTween.EaseType.easeOutBounce);
		iTween.ScaleTo(m_rightTrumpet, trumpetScaleArgsRight);
		Hashtable bannerScaleArgsRight = iTweenManager.Get().GetTweenHashTable();
		bannerScaleArgsRight.Add("z", 1f);
		bannerScaleArgsRight.Add("delay", 0.5f);
		bannerScaleArgsRight.Add("time", 1f);
		bannerScaleArgsRight.Add("islocal", true);
		bannerScaleArgsRight.Add("easetype", iTween.EaseType.easeOutElastic);
		iTween.ScaleTo(m_rightBanner, bannerScaleArgsRight);
		Hashtable bannerShredScaleArgsRight = iTweenManager.Get().GetTweenHashTable();
		bannerShredScaleArgsRight.Add("z", 1f);
		bannerShredScaleArgsRight.Add("delay", 0.5f);
		bannerShredScaleArgsRight.Add("time", 1f);
		bannerShredScaleArgsRight.Add("islocal", true);
		bannerShredScaleArgsRight.Add("easetype", iTween.EaseType.easeOutElastic);
		iTween.ScaleTo(m_rightBannerShred, bannerShredScaleArgsRight);
		Hashtable cloudMoveArgsRight = iTweenManager.Get().GetTweenHashTable();
		cloudMoveArgsRight.Add("x", -0.81f);
		cloudMoveArgsRight.Add("time", 5f);
		cloudMoveArgsRight.Add("islocal", true);
		cloudMoveArgsRight.Add("easetype", iTween.EaseType.easeOutCubic);
		cloudMoveArgsRight.Add("oncomplete", "CloudTo");
		cloudMoveArgsRight.Add("oncompletetarget", base.gameObject);
		iTween.MoveTo(m_rightCloud, cloudMoveArgsRight);
		Hashtable cloudMoveArgsLeft = iTweenManager.Get().GetTweenHashTable();
		cloudMoveArgsLeft.Add("x", 0.824f);
		cloudMoveArgsLeft.Add("time", 5f);
		cloudMoveArgsLeft.Add("islocal", true);
		cloudMoveArgsLeft.Add("easetype", iTween.EaseType.easeOutCubic);
		iTween.MoveTo(m_leftCloud, cloudMoveArgsLeft);
		Hashtable woodBannerRotateArgs = iTweenManager.Get().GetTweenHashTable();
		woodBannerRotateArgs.Add("rotation", new Vector3(0f, 183f, 0f));
		woodBannerRotateArgs.Add("time", 0.5f);
		woodBannerRotateArgs.Add("delay", 0.75f);
		woodBannerRotateArgs.Add("islocal", true);
		woodBannerRotateArgs.Add("easetype", iTween.EaseType.easeOutBounce);
		iTween.RotateTo(m_defeatBanner, woodBannerRotateArgs);
	}

	private void AnimateLeftTrumpetTo()
	{
		Hashtable trumpetRotateArgs = iTweenManager.Get().GetTweenHashTable();
		trumpetRotateArgs.Add("rotation", new Vector3(0f, -184f, 0f));
		trumpetRotateArgs.Add("time", 5f);
		trumpetRotateArgs.Add("islocal", true);
		trumpetRotateArgs.Add("easetype", iTween.EaseType.easeInOutCirc);
		trumpetRotateArgs.Add("oncomplete", "AnimateLeftTrumpetFro");
		trumpetRotateArgs.Add("oncompletetarget", base.gameObject);
		iTween.RotateTo(m_leftTrumpet, trumpetRotateArgs);
	}

	private void AnimateLeftTrumpetFro()
	{
		Hashtable trumpetRotateArgs = iTweenManager.Get().GetTweenHashTable();
		trumpetRotateArgs.Add("rotation", new Vector3(0f, -180f, 0f));
		trumpetRotateArgs.Add("time", 5f);
		trumpetRotateArgs.Add("islocal", true);
		trumpetRotateArgs.Add("easetype", iTween.EaseType.easeInOutCirc);
		trumpetRotateArgs.Add("oncomplete", "AnimateLeftTrumpetTo");
		trumpetRotateArgs.Add("oncompletetarget", base.gameObject);
		iTween.RotateTo(m_leftTrumpet, trumpetRotateArgs);
	}

	private void AnimateRightTrumpetTo()
	{
		Hashtable trumpetRightRotateArgs = iTweenManager.Get().GetTweenHashTable();
		trumpetRightRotateArgs.Add("rotation", new Vector3(0f, -172f, 0f));
		trumpetRightRotateArgs.Add("time", 8f);
		trumpetRightRotateArgs.Add("islocal", true);
		trumpetRightRotateArgs.Add("easetype", iTween.EaseType.easeInOutCirc);
		trumpetRightRotateArgs.Add("oncomplete", "AnimateRightTrumpetFro");
		trumpetRightRotateArgs.Add("oncompletetarget", base.gameObject);
		iTween.RotateTo(m_rightTrumpet, trumpetRightRotateArgs);
	}

	private void AnimateRightTrumpetFro()
	{
		Hashtable trumpetRightRotateArgs = iTweenManager.Get().GetTweenHashTable();
		trumpetRightRotateArgs.Add("rotation", new Vector3(0f, -180f, 0f));
		trumpetRightRotateArgs.Add("time", 8f);
		trumpetRightRotateArgs.Add("islocal", true);
		trumpetRightRotateArgs.Add("easetype", iTween.EaseType.easeInOutCirc);
		trumpetRightRotateArgs.Add("oncomplete", "AnimateRightTrumpetTo");
		trumpetRightRotateArgs.Add("oncompletetarget", base.gameObject);
		iTween.RotateTo(m_rightTrumpet, trumpetRightRotateArgs);
	}

	private void TokyoDriftTo()
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
		cloudMoveArgsRight.Add("x", -0.38f);
		cloudMoveArgsRight.Add("time", 10f);
		cloudMoveArgsRight.Add("islocal", true);
		cloudMoveArgsRight.Add("easetype", iTween.EaseType.linear);
		cloudMoveArgsRight.Add("oncomplete", "CloudFro");
		cloudMoveArgsRight.Add("oncompletetarget", base.gameObject);
		iTween.MoveTo(m_rightCloud, cloudMoveArgsRight);
		Hashtable cloudMoveArgsLeft = iTweenManager.Get().GetTweenHashTable();
		cloudMoveArgsLeft.Add("x", 0.443f);
		cloudMoveArgsLeft.Add("time", 10f);
		cloudMoveArgsLeft.Add("islocal", true);
		cloudMoveArgsLeft.Add("easetype", iTween.EaseType.linear);
		iTween.MoveTo(m_leftCloud, cloudMoveArgsLeft);
	}

	private void CloudFro()
	{
		Hashtable cloudMoveArgsRight = iTweenManager.Get().GetTweenHashTable();
		cloudMoveArgsRight.Add("x", -0.81f);
		cloudMoveArgsRight.Add("time", 10f);
		cloudMoveArgsRight.Add("islocal", true);
		cloudMoveArgsRight.Add("easetype", iTween.EaseType.linear);
		cloudMoveArgsRight.Add("oncomplete", "CloudTo");
		cloudMoveArgsRight.Add("oncompletetarget", base.gameObject);
		iTween.MoveTo(m_rightCloud, cloudMoveArgsRight);
		Hashtable cloudMoveArgsLeft = iTweenManager.Get().GetTweenHashTable();
		cloudMoveArgsLeft.Add("x", 0.824f);
		cloudMoveArgsLeft.Add("time", 10f);
		cloudMoveArgsLeft.Add("islocal", true);
		cloudMoveArgsLeft.Add("easetype", iTween.EaseType.linear);
		iTween.MoveTo(m_leftCloud, cloudMoveArgsLeft);
	}

	private void AnimateCrownTo()
	{
		Hashtable crownRotateArgs = iTweenManager.Get().GetTweenHashTable();
		crownRotateArgs.Add("rotation", new Vector3(0f, 1.8f, 0f));
		crownRotateArgs.Add("time", 0.75f);
		crownRotateArgs.Add("islocal", true);
		crownRotateArgs.Add("easetype", iTween.EaseType.linear);
		crownRotateArgs.Add("oncomplete", "AnimateCrownFro");
		crownRotateArgs.Add("oncompletetarget", base.gameObject);
		iTween.RotateTo(m_crown, crownRotateArgs);
	}

	private void AnimateCrownFro()
	{
		Hashtable crownRotateArgs = iTweenManager.Get().GetTweenHashTable();
		crownRotateArgs.Add("rotation", new Vector3(0f, 17f, 0f));
		crownRotateArgs.Add("time", 0.75f);
		crownRotateArgs.Add("islocal", true);
		crownRotateArgs.Add("easetype", iTween.EaseType.linear);
		crownRotateArgs.Add("oncomplete", "AnimateCrownTo");
		crownRotateArgs.Add("oncompletetarget", base.gameObject);
		iTween.RotateTo(m_crown, crownRotateArgs);
	}
}
