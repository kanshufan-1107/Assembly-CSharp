using System.Collections;
using UnityEngine;

public class AdventureClassChallengeChestButton : PegUIElement
{
	public GameObject m_RootObject;

	public Transform m_UpBone;

	public Transform m_DownBone;

	public GameObject m_HighlightPlane;

	public GameObject m_RewardBone;

	public GameObject m_RewardCard;

	public bool m_IsRewardLoading;

	private ScreenEffectsHandle m_screenEffectsHandle;

	protected override void Awake()
	{
		base.Awake();
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	protected override void OnOver(InteractionState oldState)
	{
		SoundManager.Get().LoadAndPlay("collection_manager_hero_mouse_over.prefab:653cc8000b988cd468d2210a209adce6", base.gameObject);
		ShowHighlight(show: true);
		StartCoroutine(ShowRewardCard());
	}

	protected override void OnOut(InteractionState oldState)
	{
		ShowHighlight(show: false);
		HideRewardCard();
	}

	public void Press()
	{
		SoundManager.Get().LoadAndPlay("collection_manager_hero_mouse_over.prefab:653cc8000b988cd468d2210a209adce6", base.gameObject);
		Depress();
		ShowHighlight(show: true);
		StartCoroutine(ShowRewardCard());
	}

	public void Release()
	{
		Raise();
		ShowHighlight(show: false);
		HideRewardCard();
	}

	private void Raise()
	{
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("position", m_UpBone.localPosition);
		args.Add("time", 0.1f);
		args.Add("easetype", iTween.EaseType.linear);
		args.Add("islocal", true);
		iTween.MoveTo(m_RootObject, args);
	}

	private void Depress()
	{
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("position", m_DownBone.localPosition);
		args.Add("time", 0.1f);
		args.Add("easetype", iTween.EaseType.linear);
		args.Add("islocal", true);
		iTween.MoveTo(m_RootObject, args);
	}

	private void ShowHighlight(bool show)
	{
		m_HighlightPlane.GetComponent<Renderer>().enabled = show;
	}

	private IEnumerator ShowRewardCard()
	{
		while (m_IsRewardLoading)
		{
			yield return null;
		}
		LayerUtils.SetLayer(base.gameObject, GameLayer.IgnoreFullScreenEffects);
		ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignettePerspective;
		m_screenEffectsHandle.StartEffect(screenEffectParameters);
		m_RewardBone.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		iTween.ScaleTo(m_RewardBone, new Vector3(10f, 10f, 10f), 0.2f);
		m_RewardCard.SetActive(value: true);
	}

	private void HideRewardCard()
	{
		iTween.ScaleTo(m_RewardBone, new Vector3(0.1f, 0.1f, 0.1f), 0.2f);
		m_screenEffectsHandle.StopEffect();
	}

	private void EffectFadeOutFinished()
	{
		if (!(this == null))
		{
			LayerUtils.SetLayer(base.gameObject, GameLayer.Default);
			if (m_RewardCard != null)
			{
				m_RewardCard.SetActive(value: false);
			}
		}
	}
}
