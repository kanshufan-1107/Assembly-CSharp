using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class TGTGrandStand : MonoBehaviour
{
	private const string ANIMATION_IDLE = "Idle";

	private const string ANIMATION_CLICKED = "Clicked";

	private readonly string[] ANIMATION_CHEER = new string[3] { "Cheer01", "Cheer02", "Cheer03" };

	private readonly string[] ANIMATION_OHNO = new string[2] { "OhNo01", "OhNo02" };

	private const string ANIMATION_SCORE_CARD = "ScoreCard";

	private const float MIN_RANDOM_TIME_FACTOR = 0.05f;

	private const float MAX_RANDOM_TIME_FACTOR = 0.2f;

	private const float CHEER_ANIMATION_PLAY_TIME = 4f;

	private const float OHNO_ANIMATION_PLAY_TIME = 3.5f;

	private const float FRIENDLY_HERO_DAMAGE_WEIGHT_TRGGER = 7f;

	private const float OPPONENT_HERO_DAMAGE_WEIGHT_TRGGER = 10f;

	private const float FRIENDLY_LEGENDARY_SPAWN_MIN_COST_TRGGER = 6f;

	private const float OPPONENT_LEGENDARY_SPAWN_MIN_COST_TRGGER = 9f;

	private const float FRIENDLY_LEGENDARY_DEATH_MIN_COST_TRGGER = 6f;

	private const float OPPONENT_LEGENDARY_DEATH_MIN_COST_TRGGER = 9f;

	private const float FRIENDLY_MINION_DAMAGE_WEIGHT = 15f;

	private const float OPPONENT_MINION_DAMAGE_WEIGHT = 15f;

	private const float FRIENDLY_MINION_DEATH_WEIGHT = 15f;

	private const float OPPONENT_MINION_DEATH_WEIGHT = 15f;

	private const float FRIENDLY_MINION_SPAWN_WEIGHT = 10f;

	private const float OPPONENT_MINION_SPAWN_WEIGHT = 10f;

	private const float OPPONENT_HERO_DAMAGE_SCORE_CARD_TRIGGER = 15f;

	private const float OPPONENT_HERO_DAMAGE_SCORE_CARD_10S_TRIGGER = 20f;

	public GameObject m_HumanRoot;

	public GameObject m_OrcRoot;

	public GameObject m_KnightRoot;

	public Animator m_HumanAnimator;

	public Animator m_OrcAnimator;

	public Animator m_KnightAnimator;

	public GameObject m_HumanScoreCard;

	public GameObject m_OrcScoreCard;

	public GameObject m_KnightScoreCard;

	public UberText m_HumanScoreUberText;

	public UberText m_OrcScoreUberText;

	public UberText m_KnightScoreUberText;

	[CustomEditField(T = EditType.SOUND_PREFAB, Sections = "Human Sounds")]
	public string m_ClickHumanSound;

	[CustomEditField(T = EditType.SOUND_PREFAB, Sections = "Human Sounds")]
	public List<string> m_CheerHumanSounds;

	[CustomEditField(T = EditType.SOUND_PREFAB, Sections = "Human Sounds")]
	public List<string> m_OhNoHumanSounds;

	[CustomEditField(T = EditType.SOUND_PREFAB, Sections = "Orc Sounds")]
	public string m_ClickOrcSound;

	[CustomEditField(T = EditType.SOUND_PREFAB, Sections = "Orc Sounds")]
	public List<string> m_CheerOrcSounds;

	[CustomEditField(T = EditType.SOUND_PREFAB, Sections = "Orc Sounds")]
	public List<string> m_OhNoOrcSounds;

	[CustomEditField(T = EditType.SOUND_PREFAB, Sections = "Knight Sounds")]
	public string m_ClickKnightSound;

	[CustomEditField(T = EditType.SOUND_PREFAB, Sections = "Knight Sounds")]
	public List<string> m_CheerKnightSounds;

	[CustomEditField(T = EditType.SOUND_PREFAB, Sections = "Knight Sounds")]
	public List<string> m_OhNoKnightSounds;

	[CustomEditField(T = EditType.SOUND_PREFAB, Sections = "Sounds")]
	public string m_ScoreCardSound;

	private BoardEvents m_boardEvents;

	private bool m_isAnimating;

	private static TGTGrandStand s_instance;

	private void Awake()
	{
		s_instance = this;
	}

	private void Start()
	{
		StartCoroutine(RegisterBoardEvents());
	}

	private void Update()
	{
		HandleClicks();
	}

	private void OnDestroy()
	{
		s_instance = null;
	}

	public static TGTGrandStand Get()
	{
		return s_instance;
	}

	private void HandleClicks()
	{
		if (InputCollection.GetMouseButtonDown(0))
		{
			if (IsOver(m_HumanRoot))
			{
				HumanClick();
			}
			if (IsOver(m_OrcRoot))
			{
				OrcClick();
			}
			if (IsOver(m_KnightRoot))
			{
				KnightClick();
			}
		}
	}

	private void HumanClick()
	{
		PlayClickedAnimation(m_HumanAnimator, "Clicked");
		PlaySound(m_HumanRoot, m_ClickHumanSound);
	}

	private void OrcClick()
	{
		PlayClickedAnimation(m_OrcAnimator, "Clicked");
		PlaySound(m_OrcRoot, m_ClickOrcSound);
	}

	private void KnightClick()
	{
		PlayClickedAnimation(m_KnightAnimator, "Clicked");
		PlaySound(m_KnightRoot, m_ClickKnightSound);
	}

	private IEnumerator TestAnimations()
	{
		yield return new WaitForSeconds(4f);
		PlayCheerAnimation();
		yield return new WaitForSeconds(8f);
		PlayCheerAnimation();
		yield return new WaitForSeconds(9f);
		PlayCheerAnimation();
		yield return new WaitForSeconds(8f);
		PlayOhNoAnimation();
		yield return new WaitForSeconds(8f);
		PlayOhNoAnimation();
	}

	public void PlayCheerAnimation()
	{
		int rdmIdx = Random.Range(0, ANIMATION_CHEER.Length);
		string randomAnim = ANIMATION_CHEER[rdmIdx];
		PlayAnimation(m_HumanAnimator, randomAnim, 4f);
		PlaySoundFromList(m_CheerHumanSounds, rdmIdx);
		rdmIdx = Random.Range(0, ANIMATION_CHEER.Length);
		randomAnim = ANIMATION_CHEER[rdmIdx];
		PlayAnimation(m_OrcAnimator, randomAnim, 4f);
		PlaySoundFromList(m_CheerOrcSounds, rdmIdx);
		rdmIdx = Random.Range(0, ANIMATION_CHEER.Length);
		randomAnim = ANIMATION_CHEER[rdmIdx];
		PlayAnimation(m_KnightAnimator, randomAnim, 4f);
		PlaySoundFromList(m_CheerKnightSounds, rdmIdx);
	}

	public void PlayOhNoAnimation()
	{
		int rdmIdx = Random.Range(0, ANIMATION_OHNO.Length);
		string randomAnim = ANIMATION_OHNO[rdmIdx];
		PlayAnimation(m_HumanAnimator, randomAnim, 3.5f);
		PlaySoundFromList(m_OhNoHumanSounds, rdmIdx);
		rdmIdx = Random.Range(0, ANIMATION_OHNO.Length);
		randomAnim = ANIMATION_OHNO[rdmIdx];
		PlayAnimation(m_OrcAnimator, randomAnim, 3.5f);
		PlaySoundFromList(m_OhNoOrcSounds, rdmIdx);
		rdmIdx = Random.Range(0, ANIMATION_OHNO.Length);
		randomAnim = ANIMATION_OHNO[rdmIdx];
		PlayAnimation(m_KnightAnimator, randomAnim, 3.5f);
		PlaySoundFromList(m_OhNoKnightSounds, rdmIdx);
	}

	public void PlayScoreCard(string humanScore, string orcScore, string knightScore)
	{
		m_HumanScoreUberText.Text = humanScore;
		m_OrcScoreUberText.Text = orcScore;
		m_KnightScoreUberText.Text = knightScore;
		m_HumanAnimator.SetTrigger("ScoreCard");
		m_OrcAnimator.SetTrigger("ScoreCard");
		m_KnightAnimator.SetTrigger("ScoreCard");
		PlaySound(m_OrcRoot, m_ScoreCardSound);
	}

	private void PlaySoundFromList(List<string> soundList, int index)
	{
		if (soundList != null && soundList.Count != 0)
		{
			if (index > soundList.Count)
			{
				index = 0;
			}
			PlaySound(m_OrcRoot, soundList[index]);
		}
	}

	private void PlaySound(GameObject rootObject, string soundPath)
	{
		if (!string.IsNullOrEmpty(soundPath))
		{
			SoundManager.Get().LoadAndPlay(soundPath, rootObject);
		}
	}

	private void PlayClickedAnimation(Animator animator, string animName)
	{
		m_isAnimating = true;
		animator.SetTrigger(animName);
		StartCoroutine(ReturnToIdleAnimation(animator, 0f));
	}

	private void PlayAnimation(Animator animator, string animName, float time)
	{
		m_isAnimating = true;
		m_HumanScoreCard.SetActive(value: false);
		m_OrcScoreCard.SetActive(value: false);
		m_KnightScoreCard.SetActive(value: false);
		StartCoroutine(PlayAnimationRandomStart(animator, animName, time));
	}

	private IEnumerator PlayAnimationRandomStart(Animator animator, string animName, float time)
	{
		yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
		animator.SetTrigger(animName);
		StartCoroutine(ReturnToIdleAnimation(animator, time));
	}

	private IEnumerator ReturnToIdleAnimation(Animator animator, float time)
	{
		yield return new WaitForSeconds(time);
		m_isAnimating = false;
		animator.SetTrigger("Idle");
	}

	private void Shake()
	{
		if (!m_isAnimating)
		{
			StartCoroutine(PlayAnimationRandomStart(m_HumanAnimator, "Clicked", 0f));
			StartCoroutine(PlayAnimationRandomStart(m_OrcAnimator, "Clicked", 0f));
			StartCoroutine(PlayAnimationRandomStart(m_KnightAnimator, "Clicked", 0f));
		}
	}

	private bool IsOver(GameObject go)
	{
		if (!go)
		{
			return false;
		}
		if (!InputUtil.IsPlayMakerMouseInputAllowed(go))
		{
			return false;
		}
		if (!UniversalInputManager.Get().InputIsOver(go))
		{
			return false;
		}
		return true;
	}

	private IEnumerator RegisterBoardEvents()
	{
		while (BoardEvents.Get() == null)
		{
			yield return null;
		}
		m_boardEvents = BoardEvents.Get();
		m_boardEvents.RegisterFriendlyHeroDamageEvent(FriendlyHeroDamage, 7f);
		m_boardEvents.RegisterOpponentHeroDamageEvent(OpponentHeroDamage, 10f);
		m_boardEvents.RegisterFriendlyLegendaryMinionSpawnEvent(FriendlyLegendarySpawn, 6f);
		m_boardEvents.RegisterOppenentLegendaryMinionSpawnEvent(OpponentLegendarySpawn, 9f);
		m_boardEvents.RegisterFriendlyLegendaryMinionDeathEvent(FriendlyLegendaryDeath, 6f);
		m_boardEvents.RegisterOppenentLegendaryMinionDeathEvent(OpponentLegendaryDeath, 9f);
		m_boardEvents.RegisterFriendlyMinionDamageEvent(FriendlyMinionDamage, 15f);
		m_boardEvents.RegisterOpponentMinionDamageEvent(OpponentMinionDamage, 15f);
		m_boardEvents.RegisterFriendlyMinionDeathEvent(FriendlyMinionDeath, 15f);
		m_boardEvents.RegisterOppenentMinionDeathEvent(OpponentMinionDeath, 15f);
		m_boardEvents.RegisterFriendlyMinionSpawnEvent(FriendlyMinionSpawn, 10f);
		m_boardEvents.RegisterOppenentMinionSpawnEvent(OpponentMinionSpawn, 10f);
		m_boardEvents.RegisterLargeShakeEvent(Shake);
	}

	private void FriendlyHeroDamage(float weight)
	{
		PlayOhNoAnimation();
	}

	private void OpponentHeroDamage(float weight)
	{
		if (weight > 15f)
		{
			if (weight > 20f)
			{
				PlayScoreCard("10", "10", "10");
			}
			else
			{
				PlayScoreCard("10", Random.Range(7, 9).ToString(), Random.Range(8, 10).ToString());
			}
		}
		else
		{
			PlayCheerAnimation();
		}
	}

	private void FriendlyLegendarySpawn(float weight)
	{
		PlayCheerAnimation();
	}

	private void OpponentLegendarySpawn(float weight)
	{
		PlayOhNoAnimation();
	}

	private void FriendlyLegendaryDeath(float weight)
	{
		PlayOhNoAnimation();
	}

	private void OpponentLegendaryDeath(float weight)
	{
		PlayCheerAnimation();
	}

	private void FriendlyMinionDamage(float weight)
	{
		PlayOhNoAnimation();
	}

	private void OpponentMinionDamage(float weight)
	{
		PlayCheerAnimation();
	}

	private void FriendlyMinionDeath(float weight)
	{
		PlayOhNoAnimation();
	}

	private void OpponentMinionDeath(float weight)
	{
		PlayCheerAnimation();
	}

	private void FriendlyMinionSpawn(float weight)
	{
		PlayCheerAnimation();
	}

	private void OpponentMinionSpawn(float weight)
	{
		PlayOhNoAnimation();
	}
}
