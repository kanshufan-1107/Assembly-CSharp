using System.Collections;
using UnityEngine;

public class BaconTwoScoop : VictoryTwoScoop
{
	public GameObject m_Root;

	public GameObject m_RatingBanner;

	public GameObject m_Top1Visual;

	public GameObject m_Top4Visual;

	public GameObject m_Bottom4Visual;

	public GameObject m_Top1DuoVisual;

	public GameObject m_Top2DuoVisual;

	public GameObject m_Bottom2DuoVisual;

	public AudioSource m_Top1Sound;

	public AudioSource m_Top4Sound;

	public AudioSource m_Bottom4Sound;

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

	protected override void ShowImpl()
	{
		StartCoroutine(ShowWhenReady());
	}

	protected override string GetActorName()
	{
		return "Card_Play_Bacon_Hero.prefab:227eb40f91281fa429c48c8a730c982f";
	}

	private IEnumerator ShowWhenReady()
	{
		m_Root.SetActive(value: false);
		m_heroActor.gameObject.SetActive(value: false);
		if (m_teammateHeroActor != null)
		{
			m_teammateHeroActor.gameObject.SetActive(value: false);
		}
		while (GameState.Get() == null || GameState.Get().GetGameEntity() == null)
		{
			yield return null;
		}
		TB_BaconShop baconGameEntity = null;
		if (GameState.Get().GetGameEntity() is TB_BaconShop)
		{
			baconGameEntity = (TB_BaconShop)GameState.Get().GetGameEntity();
		}
		bool ratingChangeDisabled = GameMgr.Get().IsFriendlyBattlegrounds() || GameMgr.Get().IsAI();
		if (GameState.Get().GetBooleanGameOption(GameEntityOption.WAIT_FOR_RATING_INFO))
		{
			while (baconGameEntity != null && baconGameEntity.RatingChangeData == null && !ratingChangeDisabled && m_waitForRatingTimeoutTimer < 5f)
			{
				m_waitForRatingTimeoutTimer += Time.unscaledDeltaTime;
				yield return null;
			}
		}
		m_Root.SetActive(value: true);
		m_heroActor.gameObject.SetActive(value: true);
		CustomHeroFrameBehaviour frame = m_heroActor.gameObject.GetComponent<CustomHeroFrameBehaviour>();
		if (frame != null)
		{
			frame.UpdateFrame();
		}
		if (m_teammateHeroActor != null)
		{
			m_teammateHeroActor.gameObject.SetActive(value: true);
			frame = m_teammateHeroActor.gameObject.GetComponent<CustomHeroFrameBehaviour>();
			if (frame != null)
			{
				frame.UpdateFrame();
			}
		}
		SetupHeroActor();
		SetupTeammateHeroActor();
		SetupBannerText();
		SetupTwoScoopForPlace();
		if (GameMgr.Get().IsSpectator() || baconGameEntity == null || baconGameEntity.RatingChangeData == null)
		{
			m_RatingBanner.SetActive(value: false);
			yield break;
		}
		m_newRating = baconGameEntity.RatingChangeData.NewRating;
		m_ratingChange = baconGameEntity.RatingChangeData.RatingChange;
		m_RatingBanner.SetActive(value: true);
		yield return PlayRatingChangeAnimation();
	}

	private void SetupTwoScoopForPlace()
	{
		m_Top1Visual.SetActive(value: false);
		m_Top4Visual.SetActive(value: false);
		m_Bottom4Visual.SetActive(value: false);
		int place = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetRealTimePlayerLeaderboardPlace();
		if (GameMgr.Get().IsBattlegroundDuoGame())
		{
			if (place <= 1)
			{
				m_Top1DuoVisual.SetActive(value: true);
				SoundManager.Get().Play(m_Top1Sound);
			}
			else if (place <= 2)
			{
				m_Top2DuoVisual.SetActive(value: true);
				SoundManager.Get().Play(m_Top4Sound);
			}
			else
			{
				m_Bottom2DuoVisual.SetActive(value: true);
				SoundManager.Get().Play(m_Bottom4Sound);
			}
		}
		else if (place <= 1)
		{
			m_Top1Visual.SetActive(value: true);
			SoundManager.Get().Play(m_Top1Sound);
		}
		else if (place <= 4)
		{
			m_Top4Visual.SetActive(value: true);
			SoundManager.Get().Play(m_Top4Sound);
		}
		else
		{
			m_Bottom4Visual.SetActive(value: true);
			SoundManager.Get().Play(m_Bottom4Sound);
		}
	}

	private IEnumerator PlayRatingChangeAnimation()
	{
		int oldRating = m_newRating - m_ratingChange;
		m_RatingChangeText.Text = "";
		m_RatingText.Text = oldRating.ToString();
		Animator ratingChangeAnimator = m_RatingChangeText.GetComponent<Animator>();
		ratingChangeAnimator.enabled = false;
		Animator ratingTextAnimator = m_RatingText.GetComponent<Animator>();
		ratingTextAnimator.enabled = false;
		yield return new WaitForSeconds(m_DelayBeforeRatingChangeSeconds);
		string ratingChangeText = ((m_ratingChange >= 0) ? "+" : "") + m_ratingChange;
		m_RatingChangeText.Text = ratingChangeText;
		m_RatingChangeText.TextColor = ((m_ratingChange >= 0) ? m_RatingChangeTextColorPositive : m_RatingChangeTextColorNegative);
		ratingChangeAnimator.enabled = true;
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

	private void SetupTeammateHeroActor()
	{
		if (!(m_teammateHeroActor == null))
		{
			Entity hero = TeammateBoardViewer.Get().GetTeammateHero();
			if (hero != null)
			{
				m_teammateHeroActor.SetFullDefFromEntity(hero);
				m_teammateHeroActor.UpdateAllComponents();
			}
			m_teammateHeroActor.TurnOffCollider();
		}
	}
}
