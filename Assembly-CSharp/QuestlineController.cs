using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Spell))]
[RequireComponent(typeof(Actor))]
public class QuestlineController : MonoBehaviour
{
	public UberText m_ProgressText;

	public NestedPrefab m_QuestlineProgressUIContainer;

	public string m_QuestUIBoneName = "QuestUI";

	public float m_ProgressUpdateDelay = 1f;

	public ParticleSystem m_ProgressUpdateParticles;

	private GameState m_gameState;

	private InputManager m_inputManager;

	private Spell m_spell;

	private PlayMakerFSM m_spellFSM;

	private Actor m_actor;

	private Entity m_entity;

	private QuestlineProgressUI m_QuestlineProgressUI;

	private bool m_questCompleted;

	private int m_displayedQuestProgress;

	private int m_actualQuestProgress;

	private int m_questProgressTotal;

	private bool m_isScalingDown;

	private void Awake()
	{
		m_displayedQuestProgress = 0;
		m_actualQuestProgress = 0;
		m_isScalingDown = false;
		m_spell = GetComponent<Spell>();
		if (m_spell == null)
		{
			Log.Gameplay.PrintError("QuestlineController.Awake(): GameObject " + base.gameObject.name + " does not have a Spell Component!");
		}
		m_spell.AddSpellEventCallback(OnSpellEvent);
		m_spellFSM = m_spell.GetComponent<PlayMakerFSM>();
		m_actor = GetComponent<Actor>();
		if (m_actor == null)
		{
			Log.Gameplay.PrintError("QuestlineController.Awake(): GameObject " + base.gameObject.name + " does not have an Actor Component!");
		}
		m_gameState = GameState.Get();
		if (m_gameState == null)
		{
			Log.Gameplay.PrintError("QuestlineController.Awake(): Gameobject " + base.gameObject.name + " could not initialize GameState!");
		}
		m_inputManager = InputManager.Get();
		if (m_inputManager == null)
		{
			Log.Gameplay.PrintError("QuestlineController.Awake(): Gameobject " + base.gameObject.name + " could not initialize InputManager!");
		}
	}

	private void Start()
	{
		m_QuestlineProgressUI = m_QuestlineProgressUIContainer.PrefabGameObject(instantiateIfNeeded: true).GetComponent<QuestlineProgressUI>();
		m_QuestlineProgressUI.SetOriginalQuestActor(m_actor);
		m_QuestlineProgressUI.Hide();
		Transform questUIBone = Board.Get().FindBone(m_QuestUIBoneName);
		m_QuestlineProgressUI.transform.parent = questUIBone;
		TransformUtil.Identity(m_QuestlineProgressUI);
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
			StopCoroutine("WaitThenShowQuestlineUI");
			StartCoroutine("WaitThenShowQuestlineUI");
		}
	}

	public void NotifyMousedOut()
	{
		StopCoroutine("WaitThenShowQuestlineUI");
		m_QuestlineProgressUI.Hide();
	}

	private IEnumerator WaitThenShowQuestlineUI()
	{
		if (IsEntityValid())
		{
			yield return new WaitForSeconds(m_inputManager.m_MouseOverDelay);
			RefreshQuestProgressValues();
			m_QuestlineProgressUI.UpdateText(m_actualQuestProgress, m_questProgressTotal);
			m_QuestlineProgressUI.Show();
		}
	}

	public void UpdateQuestlineUI()
	{
		if (IsEntityValid())
		{
			StartCoroutine(UpdateQuestlineUIImpl());
		}
	}

	private IEnumerator UpdateQuestlineUIImpl()
	{
		RefreshQuestProgressValues();
		if (m_actualQuestProgress != m_displayedQuestProgress)
		{
			m_displayedQuestProgress = Mathf.Min(m_displayedQuestProgress, m_actualQuestProgress);
			m_gameState.SetBusy(busy: true);
			while (m_isScalingDown)
			{
				yield return null;
			}
			if (m_actualQuestProgress < m_questProgressTotal)
			{
				m_gameState.SetBusy(busy: false);
			}
			if (!m_spell.IsActive())
			{
				UpdateProgressText();
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
			if (m_displayedQuestProgress >= m_questProgressTotal)
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
		while (m_displayedQuestProgress < m_actualQuestProgress)
		{
			m_displayedQuestProgress++;
			UpdateProgressText();
			yield return new WaitForSeconds(m_ProgressUpdateDelay);
		}
		m_isScalingDown = true;
		m_spellFSM.SendEvent("ScaleDown");
	}

	private void UpdateProgressText()
	{
		m_ProgressUpdateParticles.Stop();
		m_ProgressUpdateParticles.Play();
		m_ProgressText.Text = GameStrings.Format("GAMEPLAY_PROGRESS_X_OF_Y", m_displayedQuestProgress, m_questProgressTotal);
		m_QuestlineProgressUI.UpdateText(m_displayedQuestProgress, m_questProgressTotal);
	}

	private void CompleteQuest()
	{
		m_gameState.SetBusy(busy: false);
		m_questCompleted = true;
		m_QuestlineProgressUI.Hide();
		m_spell.ActivateState(SpellStateType.DEATH);
	}

	private bool IsEntityValid()
	{
		if (m_entity == null)
		{
			m_entity = m_actor.GetEntity();
			if (m_entity == null)
			{
				return false;
			}
		}
		return true;
	}

	private void RefreshQuestProgressValues()
	{
		if (IsEntityValid())
		{
			m_actualQuestProgress = m_entity.GetTag(GAME_TAG.QUEST_PROGRESS);
			m_questProgressTotal = m_entity.GetTag(GAME_TAG.QUEST_PROGRESS_TOTAL);
		}
	}
}
