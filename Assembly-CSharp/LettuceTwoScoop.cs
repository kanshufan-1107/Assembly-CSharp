using System.Collections;
using UnityEngine;

public class LettuceTwoScoop : EndGameTwoScoop
{
	public GameObject m_Root;

	public GameObject m_RatingBanner;

	public UberText m_RatingText;

	public UberText m_RatingChangeText;

	public Color m_RatingChangeTextColorPositive;

	public Color m_RatingChangeTextColorNegative;

	public float m_RatingTextUpdateTimeSeconds = 0.5f;

	public float m_DelayBeforeRatingChangeSeconds = 0.5f;

	private const float WAIT_FOR_RATING_TIMEOUT_SECONDS = 5f;

	private float m_waitForRatingTimeoutTimer;

	private int m_newRating;

	private int m_ratingChange;

	public override void Awake()
	{
		base.gameObject.SetActive(value: false);
	}

	public override bool IsLoaded()
	{
		return true;
	}

	protected override void ShowImpl()
	{
		StartCoroutine(ShowWhenReady());
	}

	public IEnumerator ShowWhenReady()
	{
		m_Root.SetActive(value: false);
		while (GameState.Get() == null || GameState.Get().GetGameEntity() == null)
		{
			yield return null;
		}
		LettuceMissionEntity lettuceGameEntity = null;
		if (GameState.Get().GetGameEntity() is LettuceMissionEntity)
		{
			lettuceGameEntity = (LettuceMissionEntity)GameState.Get().GetGameEntity();
		}
		if (GameState.Get().GetBooleanGameOption(GameEntityOption.WAIT_FOR_RATING_INFO))
		{
			while (lettuceGameEntity != null && lettuceGameEntity.RatingChangeData == null && m_waitForRatingTimeoutTimer < 5f)
			{
				m_waitForRatingTimeoutTimer += Time.unscaledDeltaTime;
				yield return null;
			}
		}
		m_Root.SetActive(value: true);
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
		iTween.MoveTo(base.gameObject, victoryMoveArgs);
		if (GameMgr.Get().IsSpectator() || lettuceGameEntity == null || lettuceGameEntity.RatingChangeData == null)
		{
			m_RatingBanner.SetActive(value: false);
			yield break;
		}
		m_newRating = lettuceGameEntity.RatingChangeData.PvpRating;
		m_ratingChange = lettuceGameEntity.RatingChangeData.Delta;
		m_RatingBanner.SetActive(value: true);
		yield return PlayRatingChangeAnimation();
	}

	private IEnumerator PlayRatingChangeAnimation()
	{
		int oldRating = m_newRating - m_ratingChange;
		m_RatingChangeText.Text = "";
		m_RatingText.Text = oldRating.ToString();
		Animator ratingChangeTextAnimator = m_RatingChangeText.GetComponent<Animator>();
		ratingChangeTextAnimator.enabled = false;
		Animator ratingTextAnimator = m_RatingText.GetComponent<Animator>();
		ratingTextAnimator.enabled = false;
		yield return new WaitForSeconds(m_DelayBeforeRatingChangeSeconds);
		string ratingChangeText = ((m_ratingChange >= 0) ? "+" : "") + m_ratingChange;
		m_RatingChangeText.Text = ratingChangeText;
		m_RatingChangeText.TextColor = ((m_ratingChange >= 0) ? m_RatingChangeTextColorPositive : m_RatingChangeTextColorNegative);
		ratingChangeTextAnimator.enabled = true;
		ratingTextAnimator.enabled = true;
		float timer = 0f;
		while (timer < m_RatingTextUpdateTimeSeconds)
		{
			float ratio = Mathf.Clamp01(timer / m_RatingTextUpdateTimeSeconds);
			int currentRatingNumber = Mathf.FloorToInt(Mathf.Lerp(oldRating, m_newRating, ratio));
			m_RatingText.Text = currentRatingNumber.ToString();
			timer += Time.deltaTime;
			yield return null;
		}
		m_RatingText.Text = m_newRating.ToString();
	}
}
