using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Spell))]
[RequireComponent(typeof(Actor))]
public class QuestController : MonoBehaviour
{
	public UberText m_ProgressText;

	public NestedPrefab m_QuestProgressUIContainer;

	public string m_QuestUIBoneName = "QuestUI";

	public string m_QuestUITeammateBoneName = "QuestUITeammate";

	public float m_ProgressUpdateDelay = 1f;

	[Tooltip("When incrementing quest progress if this is set it will set the text to the total progress. If the is not set the text increments one by one.")]
	public bool m_FullTextProgressOnUpdate;

	public ParticleSystem m_ProgressUpdateParticles;

	private Spell m_spell;

	private Actor m_actor;

	private Entity m_entity;

	private QuestProgressUI m_questProgressUI;

	private bool m_questCompleted;

	private int m_currentQuestProgress;

	private int m_questProgressTotal;

	private int m_targetQuestProgress;

	private bool m_isScalingDown;

	private void Awake()
	{
		m_currentQuestProgress = 0;
		m_targetQuestProgress = 0;
		m_isScalingDown = false;
		m_spell = GetComponent<Spell>();
		if (m_spell == null)
		{
			Log.Gameplay.PrintError("QuestController.Awake(): GameObject {0} does not have a Spell Component!", base.gameObject.name);
		}
		m_spell.AddSpellEventCallback(OnSpellEvent);
		m_actor = GetComponent<Actor>();
		if (m_actor == null)
		{
			Log.Gameplay.PrintError("QuestController.Awake(): GameObject {0} does not have an Actor Component!", base.gameObject.name);
		}
	}

	private void Start()
	{
		m_questProgressUI = m_QuestProgressUIContainer.PrefabGameObject(instantiateIfNeeded: true).GetComponent<QuestProgressUI>();
		m_questProgressUI.SetOriginalQuestActor(m_actor);
		m_questProgressUI.Hide();
		Transform questUIBone = Board.Get().FindBone(m_QuestUIBoneName);
		m_questProgressUI.transform.parent = questUIBone;
		TransformUtil.Identity(m_questProgressUI);
	}

	public static string GetRewardCardIDFromQuestCardID(Entity ent)
	{
		int cardDBID = 53649;
		if (ent != null && ent.HasTag(GAME_TAG.QUEST_REWARD_DATABASE_ID))
		{
			cardDBID = ent.GetTag(GAME_TAG.QUEST_REWARD_DATABASE_ID);
		}
		return GameUtils.TranslateDbIdToCardId(cardDBID);
	}

	public void NotifyMousedOver()
	{
		if (!m_questCompleted)
		{
			StopCoroutine("WaitThenShowQuestUI");
			StartCoroutine("WaitThenShowQuestUI");
		}
	}

	public void NotifyMousedOut()
	{
		StopCoroutine("WaitThenShowQuestUI");
		m_questProgressUI.Hide();
	}

	private IEnumerator WaitThenShowQuestUI()
	{
		yield return new WaitForSeconds(InputManager.Get().m_MouseOverDelay);
		if (GetEntity() != null)
		{
			m_questProgressUI.UpdateText(m_currentQuestProgress, m_questProgressTotal);
			m_questProgressUI.Show();
		}
	}

	public void UpdateQuestUI()
	{
		StartCoroutine(UpdateQuestUIImpl());
	}

	private IEnumerator UpdateQuestUIImpl()
	{
		Entity entity = GetEntity();
		if (entity == null)
		{
			yield break;
		}
		int newQuestProgress = entity.GetTag(GAME_TAG.QUEST_PROGRESS);
		newQuestProgress = Mathf.Min(newQuestProgress, m_questProgressTotal);
		if (newQuestProgress != m_targetQuestProgress)
		{
			m_targetQuestProgress = newQuestProgress;
			GameState.Get().SetBusy(busy: true);
			while (m_isScalingDown)
			{
				yield return null;
			}
			if (m_targetQuestProgress < m_questProgressTotal)
			{
				GameState.Get().SetBusy(busy: false);
			}
			if (!m_spell.IsActive())
			{
				UpdateProgressText(m_targetQuestProgress);
				m_spell.ActivateState(SpellStateType.ACTION);
			}
		}
	}

	private void OnSpellEvent(string eventName, object eventData, object userData)
	{
		if (eventName == "ScaledUp")
		{
			StartCoroutine(UpdateQuestProgress());
		}
		else if (eventName == "ScaledDown")
		{
			m_isScalingDown = false;
			if (m_currentQuestProgress >= m_questProgressTotal)
			{
				CompleteQuest();
			}
			else
			{
				m_spell.ActivateState(SpellStateType.NONE);
			}
		}
	}

	private IEnumerator UpdateQuestProgress()
	{
		bool done = false;
		while (!done)
		{
			yield return new WaitForSeconds(m_ProgressUpdateDelay);
			if (m_currentQuestProgress < m_targetQuestProgress)
			{
				if (m_FullTextProgressOnUpdate)
				{
					UpdateProgressText(m_targetQuestProgress);
				}
				else
				{
					UpdateProgressText(m_currentQuestProgress + 1);
				}
			}
			else
			{
				done = true;
			}
		}
		m_isScalingDown = true;
		m_spell.GetComponent<PlayMakerFSM>().SendEvent("ScaleDown");
	}

	private void UpdateProgressText(int currentProgress)
	{
		m_currentQuestProgress = currentProgress;
		m_ProgressUpdateParticles.Stop();
		m_ProgressUpdateParticles.Play();
		m_ProgressText.Text = GameStrings.Format("GAMEPLAY_PROGRESS_X_OF_Y", m_currentQuestProgress, m_questProgressTotal);
		m_questProgressUI.UpdateText(m_currentQuestProgress, m_questProgressTotal);
	}

	private void CompleteQuest()
	{
		GameState.Get().SetBusy(busy: false);
		m_questCompleted = true;
		m_questProgressUI.Hide();
		m_spell.ActivateState(SpellStateType.DEATH);
	}

	private Entity GetEntity()
	{
		if (m_entity == null)
		{
			m_entity = m_actor.GetEntity();
			if (m_entity != null)
			{
				m_currentQuestProgress = m_entity.GetTag(GAME_TAG.QUEST_PROGRESS);
				m_questProgressTotal = m_entity.GetTag(GAME_TAG.QUEST_PROGRESS_TOTAL);
			}
			if (TeammateBoardViewer.Get() != null && TeammateBoardViewer.Get().GetTeammateSecretViewer().IsActorInViewer(m_actor))
			{
				Transform questUIBone = Board.Get().FindBone(m_QuestUITeammateBoneName);
				m_questProgressUI.transform.parent = questUIBone;
				TransformUtil.Identity(m_questProgressUI);
			}
		}
		return m_entity;
	}
}
