using System.Collections;
using UnityEngine;

namespace Hearthstone.LookDev;

public class BoardEventListener : MonoBehaviour
{
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

	private BoardEvents m_boardEvents;

	[Header("References")]
	[SerializeField]
	private Animator m_animator;

	[SerializeField]
	[Header("Animator Trigger Names (leave blank if not used)")]
	private string m_friendlyHeroDamage = "";

	[SerializeField]
	private string m_opponentHeroDamage = "";

	[SerializeField]
	private string m_friendlyLegendarySpawn = "";

	[SerializeField]
	private string m_opponentLegendarySpawn = "";

	[SerializeField]
	private string m_friendlyLegendaryDeath = "";

	[SerializeField]
	private string m_opponentLegendaryDeath = "";

	[SerializeField]
	private string m_friendlyMinionDamage = "";

	[SerializeField]
	private string m_opponentMinionDamage = "";

	[SerializeField]
	private string m_friendlyMinionDeath = "";

	[SerializeField]
	private string m_opponentMinionDeath = "";

	[SerializeField]
	private string m_friendlyMinionSpawn = "";

	[SerializeField]
	private string m_opponentMinionSpawn = "";

	[SerializeField]
	private string m_largeShake = "";

	private void Start()
	{
		StartCoroutine(RegisterBoardEvents());
	}

	private IEnumerator RegisterBoardEvents()
	{
		while (m_boardEvents == null)
		{
			m_boardEvents = BoardEvents.Get();
			yield return null;
		}
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
		m_boardEvents.RegisterLargeShakeEvent(LargeShake);
	}

	private void FriendlyHeroDamage(float weight)
	{
		TriggerAnimation(m_friendlyHeroDamage);
	}

	private void OpponentHeroDamage(float weight)
	{
		TriggerAnimation(m_opponentHeroDamage);
	}

	private void FriendlyLegendarySpawn(float weight)
	{
		TriggerAnimation(m_friendlyLegendarySpawn);
	}

	private void OpponentLegendarySpawn(float weight)
	{
		TriggerAnimation(m_opponentLegendarySpawn);
	}

	private void FriendlyLegendaryDeath(float weight)
	{
		TriggerAnimation(m_friendlyLegendaryDeath);
	}

	private void OpponentLegendaryDeath(float weight)
	{
		TriggerAnimation(m_opponentLegendaryDeath);
	}

	private void FriendlyMinionDamage(float weight)
	{
		TriggerAnimation(m_friendlyMinionDamage);
	}

	private void OpponentMinionDamage(float weight)
	{
		TriggerAnimation(m_opponentMinionDamage);
	}

	private void FriendlyMinionDeath(float weight)
	{
		TriggerAnimation(m_friendlyMinionDeath);
	}

	private void OpponentMinionDeath(float weight)
	{
		TriggerAnimation(m_opponentMinionDeath);
	}

	private void FriendlyMinionSpawn(float weight)
	{
		TriggerAnimation(m_friendlyMinionSpawn);
	}

	private void OpponentMinionSpawn(float weight)
	{
		TriggerAnimation(m_opponentMinionSpawn);
	}

	private void LargeShake()
	{
		TriggerAnimation(m_largeShake);
	}

	private void TriggerAnimation(string triggerName)
	{
		if (m_animator == null)
		{
			Debug.LogError(GetType().ToString() + " does not have an Animator assigned.");
		}
		else if (!string.IsNullOrEmpty(triggerName))
		{
			m_animator.SetTrigger(triggerName);
		}
	}
}
