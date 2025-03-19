using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FatigueSpellController : SpellController
{
	private const float FATIGUE_DRAW_ANIM_TIME = 1.2f;

	private const float FATIGUE_DRAW_SCALE_TIME = 1f;

	private static readonly Vector3 FATIGUE_ACTOR_START_SCALE = new Vector3(0.88f, 0.88f, 0.88f);

	private static readonly Vector3 FATIGUE_ACTOR_FINAL_SCALE = Vector3.one;

	private static readonly Vector3 FATIGUE_ACTOR_INITIAL_LOCAL_ROTATION = new Vector3(270f, 270f, 0f);

	private static readonly Vector3 FATIGUE_ACTOR_FINAL_LOCAL_ROTATION = Vector3.zero;

	private const float FATIGUE_HOLD_TIME = 0.8f;

	private Network.HistTagChange m_fatigueTagChange;

	private Actor m_fatigueActor;

	protected override bool AddPowerSourceAndTargets(PowerTaskList taskList)
	{
		if (!HasSourceCard(taskList))
		{
			return false;
		}
		m_fatigueTagChange = null;
		List<PowerTask> tasks = m_taskList.GetTaskList();
		for (int i = 0; i < tasks.Count; i++)
		{
			Network.PowerHistory power = tasks[i].GetPower();
			if (power.Type == Network.PowerType.TAG_CHANGE)
			{
				Network.HistTagChange tagChange = (Network.HistTagChange)power;
				if (tagChange.Tag == 22)
				{
					m_fatigueTagChange = tagChange;
				}
			}
		}
		if (m_fatigueTagChange == null)
		{
			return false;
		}
		Card sourceCard = taskList.GetSourceEntity().GetCard();
		SetSource(sourceCard);
		return true;
	}

	protected override void OnProcessTaskList()
	{
		if (!AssetLoader.Get().InstantiatePrefab("Card_Hand_Fatigue.prefab:ae394ca0bb29a964eb4c7eeb555f2fae", OnFatigueActorLoadAttempted, null, AssetLoadingOptions.IgnorePrefabPosition))
		{
			OnFatigueActorLoadAttempted("Card_Hand_Fatigue.prefab:ae394ca0bb29a964eb4c7eeb555f2fae", null, null);
		}
	}

	private void OnFatigueActorLoadAttempted(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogWarning($"FatigueSpellController.OnFatigueActorLoadAttempted() - FAILED to load actor \"{assetRef}\"");
			DoFinishFatigue();
			return;
		}
		Actor actor = go.GetComponent<Actor>();
		if (actor == null)
		{
			Debug.LogWarning($"FatigueSpellController.OnFatigueActorLoadAttempted() - ERROR actor \"{assetRef}\" has no Actor component");
			DoFinishFatigue();
			return;
		}
		Player controller = GetSource().GetController();
		Player.Side side = controller.GetControllerSide();
		bool friendlySide = side == Player.Side.FRIENDLY;
		m_fatigueActor = actor;
		UberText nameText = m_fatigueActor.GetNameText();
		if (nameText != null)
		{
			nameText.Text = GameStrings.Get("GAMEPLAY_FATIGUE_TITLE");
		}
		int fatigueDamage = m_fatigueTagChange.Value;
		if (controller.HasTag(GAME_TAG.DOUBLE_FATIGUE_DAMAGE))
		{
			fatigueDamage *= (int)Mathf.Pow(2f, controller.GetTag(GAME_TAG.DOUBLE_FATIGUE_DAMAGE));
		}
		UberText powersText = m_fatigueActor.GetPowersTextObject();
		if (powersText != null)
		{
			powersText.Text = GameStrings.Format("GAMEPLAY_FATIGUE_TEXT", fatigueDamage);
		}
		actor.SetCardBackSideOverride(side);
		actor.UpdateCardBack();
		ZoneDeck deckZone = (friendlySide ? GameState.Get().GetFriendlySidePlayer().GetDeckZone() : GameState.Get().GetOpposingSidePlayer().GetDeckZone());
		deckZone.DoFatigueGlow();
		m_fatigueActor.transform.localEulerAngles = FATIGUE_ACTOR_INITIAL_LOCAL_ROTATION;
		m_fatigueActor.transform.localScale = FATIGUE_ACTOR_START_SCALE;
		m_fatigueActor.transform.position = deckZone.transform.position;
		Vector3[] drawPath = new Vector3[3]
		{
			m_fatigueActor.transform.position,
			new Vector3(m_fatigueActor.transform.position.x, m_fatigueActor.transform.position.y + 3.6f, m_fatigueActor.transform.position.z),
			Board.Get().FindBone("FatigueCardBone").position
		};
		iTween.MoveTo(m_fatigueActor.gameObject, iTween.Hash("path", drawPath, "time", 1.2f, "easetype", iTween.EaseType.easeInSineOutExpo));
		iTween.RotateTo(m_fatigueActor.gameObject, iTween.Hash("rotation", FATIGUE_ACTOR_FINAL_LOCAL_ROTATION, "time", 1.2f, "delay", 0.15f));
		iTween.ScaleTo(m_fatigueActor.gameObject, FATIGUE_ACTOR_FINAL_SCALE, 1f);
		StartCoroutine(WaitThenFinishFatigue(0.8f));
	}

	private IEnumerator WaitThenFinishFatigue(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		DoFinishFatigue();
	}

	private void DoFinishFatigue()
	{
		Spell spell = GetSource().GetActor().GetSpell(SpellType.FATIGUE_DEATH);
		spell.AddFinishedCallback(OnFatigueDamageFinished);
		spell.ActivateState(SpellStateType.BIRTH);
	}

	private void OnFatigueDamageFinished(Spell spell, object userData)
	{
		spell.RemoveFinishedCallback(OnFatigueDamageFinished);
		if (m_fatigueActor == null)
		{
			OnFinishedTaskList();
			return;
		}
		Spell fatigueDeathSpell = m_fatigueActor.GetSpell(SpellType.DEATH);
		if (fatigueDeathSpell == null)
		{
			OnFinishedTaskList();
			return;
		}
		Actor actor = m_fatigueActor;
		m_fatigueActor = null;
		fatigueDeathSpell.AddStateFinishedCallback(OnFatigueDeathSpellFinished, actor);
		fatigueDeathSpell.Activate();
		OnFinishedTaskList();
	}

	private void OnFatigueDeathSpellFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		Actor fatigueActor = (Actor)userData;
		if (fatigueActor != null)
		{
			fatigueActor.Destroy();
		}
		OnFinished();
	}
}
