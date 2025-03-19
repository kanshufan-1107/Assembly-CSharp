using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core.Utils;
using UnityEngine;

[CustomEditClass]
public class RevealCardSpellController : SpellController
{
	private class RevealedCard
	{
		public Player m_player;

		public Card m_card;

		public int m_deckIndex;

		public Actor m_initialActor;

		public Actor m_revealedActor;

		public int m_effectsPendingFinish;
	}

	public Spell m_NoRevealedCardSpellPrefab;

	public float m_RandomSecMin = 0.1f;

	public float m_RandomSecMax = 0.25f;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_ShowSoundPrefab;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_DrawStingerPrefab;

	public float m_ShowTime = 1.2f;

	public float m_DriftCycleTime = 10f;

	public float m_RevealTime = 0.5f;

	public iTween.EaseType m_RevealEaseType = iTween.EaseType.easeOutBack;

	public float m_HoldTime = 1.2f;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_HideSoundPrefab;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_HideStingerPrefab;

	public float m_HideTime = 0.8f;

	public string m_FriendlyBoneName = "FriendlyJoust";

	public string m_OpponentBoneName = "OpponentJoust";

	private int m_hideEntityTask;

	private RevealedCard m_revealedCard;

	protected override bool AddPowerSourceAndTargets(PowerTaskList taskList)
	{
		if (!HasSourceCard(taskList))
		{
			return false;
		}
		Card sourceCard = taskList.GetSourceEntity().GetCard();
		SetSource(sourceCard);
		m_hideEntityTask = -1;
		if (taskList.HasTargetEntity())
		{
			int targetEntityID = taskList.GetTargetEntity().GetEntityId();
			List<PowerTask> tasks = taskList.GetTaskList();
			for (int i = 0; i < tasks.Count; i++)
			{
				if (tasks[i].GetPower() is Network.HistHideEntity hideEntity && hideEntity.Entity == targetEntityID)
				{
					m_hideEntityTask = i;
				}
			}
			if (m_hideEntityTask < 0)
			{
				return false;
			}
		}
		else if (!taskList.IsStartOfBlock())
		{
			return false;
		}
		return true;
	}

	protected override void OnProcessTaskList()
	{
		StartCoroutine(DoEffectWithTiming());
	}

	private IEnumerator DoEffectWithTiming()
	{
		yield return StartCoroutine(CompleteTasksBeforeHideEntity());
		CreateRevealedCardActors();
		if (m_revealedCard != null)
		{
			yield return StartCoroutine(PullRevealedCardFromDeck());
			yield return StartCoroutine(FlipRevealedCard());
			yield return StartCoroutine(HideRevealedCardAnim());
			DestroyRevealedCard();
		}
		else
		{
			yield return StartCoroutine(PlayNoRevealedCardSpell());
		}
		base.OnProcessTaskList();
	}

	private IEnumerator CompleteTasksBeforeHideEntity()
	{
		if (m_hideEntityTask > 0)
		{
			bool complete = false;
			PowerTaskList.CompleteCallback completeCallback = delegate
			{
				complete = true;
			};
			m_taskList.DoTasks(0, m_hideEntityTask, completeCallback);
			while (!complete)
			{
				yield return null;
			}
		}
	}

	private void CreateRevealedCardActors()
	{
		if (!m_taskList.HasTargetEntity())
		{
			return;
		}
		Entity entity = m_taskList.GetTargetEntity();
		Card card = entity.GetCard();
		card.SetInputEnabled(enabled: false);
		GameObject hiddenActorGo = AssetLoader.Get().InstantiatePrefab("Card_Hidden.prefab:1a94649d257bc284ca6e2962f634a8b9", AssetLoadingOptions.IgnorePrefabPosition);
		if (hiddenActorGo == null)
		{
			Log.Gameplay.PrintError("{0}.CreateRevealedCardActors() - Failed to load HIDDEN actor.", this);
			return;
		}
		GameObject revealedActorGo = AssetLoader.Get().InstantiatePrefab(ActorNames.GetHandActor(entity), AssetLoadingOptions.IgnorePrefabPosition);
		if (revealedActorGo == null)
		{
			Log.Gameplay.PrintError("{0}.CreateRevealedCardActors() - Failed to load HAND actor.", this);
			return;
		}
		m_revealedCard = new RevealedCard();
		m_revealedCard.m_player = entity.GetController();
		m_revealedCard.m_card = card;
		m_revealedCard.m_initialActor = hiddenActorGo.GetComponent<Actor>();
		m_revealedCard.m_revealedActor = revealedActorGo.GetComponent<Actor>();
		Action<Actor> obj = delegate(Actor actor)
		{
			actor.SetEntity(entity);
			actor.SetCard(card);
			actor.SetCardDefFromCard(card);
			actor.UpdateAllComponents();
			actor.Hide();
		};
		obj(m_revealedCard.m_initialActor);
		obj(m_revealedCard.m_revealedActor);
	}

	private IEnumerator PullRevealedCardFromDeck()
	{
		if (!string.IsNullOrEmpty(m_DrawStingerPrefab))
		{
			SoundManager.Get().LoadAndPlay(m_DrawStingerPrefab);
		}
		string boneName = ((m_revealedCard.m_player.GetSide() == Player.Side.FRIENDLY) ? m_FriendlyBoneName : m_OpponentBoneName);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			boneName += "_phone";
		}
		Transform obj = Board.Get().FindBone(boneName);
		Vector3 localScale = obj.localScale;
		Vector3 position = obj.position;
		Quaternion rotation = obj.rotation;
		float delaySec = GetRandomSec();
		float showSec = m_ShowTime + GetRandomSec();
		PullRevealedCardFromDeckAnim(localScale, rotation, position, delaySec, showSec);
		while (IsRevealedCardBusy())
		{
			yield return null;
		}
	}

	private void PullRevealedCardFromDeckAnim(Vector3 localScale, Quaternion rotation, Vector3 position, float delaySec, float showSec)
	{
		m_revealedCard.m_effectsPendingFinish++;
		Card card = m_revealedCard.m_card;
		ZoneDeck deck = m_revealedCard.m_player.GetDeckZone();
		Actor thicknessForLayout = deck.GetThicknessForLayout();
		m_revealedCard.m_deckIndex = deck.RemoveCard(card);
		deck.SetSuppressEmotes(suppress: true);
		deck.UpdateLayout();
		float halfShowSec = 0.5f * showSec;
		Vector3 initialPos = thicknessForLayout.GetMeshRenderer().bounds.center + Card.IN_DECK_OFFSET;
		Vector3 intermedPos = initialPos + Card.ABOVE_DECK_OFFSET;
		Vector3 finalAngles = rotation.eulerAngles;
		Vector3[] movePath = new Vector3[3] { initialPos, intermedPos, position };
		card.ShowCard();
		m_revealedCard.m_initialActor.Show();
		card.transform.position = initialPos;
		card.transform.rotation = Card.IN_DECK_HIDDEN_ROTATION;
		card.transform.localScale = Card.IN_DECK_SCALE;
		iTween.MoveTo(card.gameObject, iTween.Hash("path", movePath, "delay", delaySec, "time", showSec, "easetype", iTween.EaseType.easeInOutQuart));
		iTween.RotateTo(card.gameObject, iTween.Hash("rotation", finalAngles, "delay", delaySec + halfShowSec, "time", halfShowSec, "easetype", iTween.EaseType.easeInOutCubic));
		iTween.ScaleTo(card.gameObject, iTween.Hash("scale", localScale, "delay", delaySec + halfShowSec, "time", halfShowSec, "easetype", iTween.EaseType.easeInOutQuint));
		if (!string.IsNullOrEmpty(m_ShowSoundPrefab))
		{
			SoundManager.Get().LoadAndPlay(m_ShowSoundPrefab);
		}
		Action<object> tweensCompleteCallback = delegate
		{
			m_revealedCard.m_effectsPendingFinish--;
			DriftRevealedCard();
		};
		iTween.Timer(card.gameObject, iTween.Hash("delay", delaySec, "time", showSec, "oncomplete", tweensCompleteCallback));
	}

	private void DriftRevealedCard()
	{
		Card card = m_revealedCard.m_card;
		Vector3 center = card.transform.position;
		float height = m_revealedCard.m_initialActor.GetMeshRenderer().bounds.size.z;
		float sizeScalar = 0.02f * height;
		Vector3 zSide = GeneralUtils.RandomSign() * sizeScalar * card.transform.up;
		Vector3 zSideOpposite = -zSide;
		Vector3 xSide = GeneralUtils.RandomSign() * sizeScalar * card.transform.right;
		Vector3 xSideOpposite = -xSide;
		List<Vector3> path = new List<Vector3>();
		path.Add(center + zSide + xSide);
		path.Add(center + zSideOpposite + xSide);
		path.Add(center);
		path.Add(center + zSide + xSideOpposite);
		path.Add(center + zSideOpposite + xSideOpposite);
		path.Add(center);
		float driftSec = m_DriftCycleTime + GetRandomSec();
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("path", path.ToArray());
		args.Add("time", driftSec);
		args.Add("easetype", iTween.EaseType.linear);
		args.Add("looptype", iTween.LoopType.loop);
		iTween.MoveTo(card.gameObject, args);
	}

	private IEnumerator FlipRevealedCard()
	{
		float revealSec = m_RevealTime + GetRandomSec();
		FlipRevealedCardAnim(revealSec);
		while (IsRevealedCardBusy())
		{
			yield return null;
		}
		iTween.Timer(base.gameObject, iTween.Hash("time", m_HoldTime));
		while (iTween.HasTween(base.gameObject))
		{
			yield return null;
		}
	}

	private void FlipRevealedCardAnim(float revealSec)
	{
		m_revealedCard.m_effectsPendingFinish++;
		Card card = m_revealedCard.m_card;
		Actor hiddenActor = m_revealedCard.m_initialActor;
		Actor revealedActor = m_revealedCard.m_revealedActor;
		TransformUtil.SetEulerAngleZ(revealedActor.gameObject, -180f);
		iTween.RotateAdd(hiddenActor.gameObject, iTween.Hash("z", 180f, "time", revealSec, "easetype", m_RevealEaseType));
		iTween.RotateAdd(revealedActor.gameObject, iTween.Hash("z", 180f, "time", revealSec, "easetype", m_RevealEaseType));
		float startAngleZ = revealedActor.transform.rotation.eulerAngles.z;
		Action<object> updateCallback = delegate
		{
			float z = revealedActor.transform.rotation.eulerAngles.z;
			if (Mathf.DeltaAngle(startAngleZ, z) >= 90f)
			{
				revealedActor.Show();
				hiddenActor.Hide();
			}
		};
		Action<object> completeCallback = delegate
		{
			revealedActor.Show();
			hiddenActor.Hide();
			m_revealedCard.m_effectsPendingFinish--;
		};
		iTween.Timer(card.gameObject, iTween.Hash("time", revealSec, "onupdate", updateCallback, "oncomplete", completeCallback));
	}

	private IEnumerator HideRevealedCardAnim()
	{
		if (!string.IsNullOrEmpty(m_HideStingerPrefab))
		{
			SoundManager.Get().LoadAndPlay(m_HideStingerPrefab);
		}
		float delaySec = GetRandomSec();
		float hideSec = m_HideTime + GetRandomSec();
		HideRevealedCardAnim(delaySec, hideSec);
		while (IsRevealedCardBusy())
		{
			yield return null;
		}
	}

	private void HideRevealedCardAnim(float delaySec, float hideSec)
	{
		m_revealedCard.m_effectsPendingFinish++;
		Card card = m_revealedCard.m_card;
		ZoneDeck deck = m_revealedCard.m_player.GetDeckZone();
		Vector3 center = deck.GetThicknessForLayout().GetMeshRenderer().bounds.center;
		float halfHideSec = 0.5f * hideSec;
		Vector3 initialPos = card.transform.position;
		Vector3 intermedPos = center + Card.ABOVE_DECK_OFFSET;
		Vector3 finalPos = center + Card.IN_DECK_OFFSET;
		Vector3 finalAngles = Card.IN_DECK_ANGLES;
		Vector3 finalScale = Card.IN_DECK_SCALE;
		Vector3[] movePath = new Vector3[3] { initialPos, intermedPos, finalPos };
		iTween.MoveTo(card.gameObject, iTween.Hash("path", movePath, "delay", delaySec, "time", hideSec, "easetype", iTween.EaseType.easeInOutQuad));
		iTween.RotateTo(card.gameObject, iTween.Hash("rotation", finalAngles, "delay", delaySec, "time", halfHideSec, "easetype", iTween.EaseType.easeInOutCubic));
		iTween.ScaleTo(card.gameObject, iTween.Hash("scale", finalScale, "delay", delaySec + halfHideSec, "time", halfHideSec, "easetype", iTween.EaseType.easeInOutQuint));
		if (!string.IsNullOrEmpty(m_HideSoundPrefab))
		{
			SoundManager.Get().LoadAndPlay(m_HideSoundPrefab);
		}
		Action<object> tweensCompleteCallback = delegate
		{
			m_revealedCard.m_effectsPendingFinish--;
			m_revealedCard.m_initialActor.GetCard().HideCard();
			deck.InsertCard(m_revealedCard.m_deckIndex, card);
			deck.UpdateLayout();
			deck.SetSuppressEmotes(suppress: false);
		};
		iTween.Timer(card.gameObject, iTween.Hash("delay", delaySec, "time", hideSec, "oncomplete", tweensCompleteCallback));
	}

	private void DestroyRevealedCard()
	{
		m_revealedCard.m_card.SetInputEnabled(enabled: true);
		m_revealedCard.m_initialActor.Destroy();
		m_revealedCard.m_revealedActor.Destroy();
		m_revealedCard = null;
	}

	private IEnumerator PlayNoRevealedCardSpell()
	{
		ZoneDeck deck = GetSource().GetController().GetDeckZone();
		if (deck == null)
		{
			yield break;
		}
		Spell noCardSpellInstance = SpellManager.Get().GetSpell(m_NoRevealedCardSpellPrefab);
		if (noCardSpellInstance == null)
		{
			yield break;
		}
		noCardSpellInstance.SetPosition(deck.transform.position);
		noCardSpellInstance.AddStateFinishedCallback(delegate(Spell spell, SpellStateType prevStateType, object userData)
		{
			if (spell.GetActiveState() == SpellStateType.NONE)
			{
				SpellManager.Get().ReleaseSpell(spell);
			}
		});
		noCardSpellInstance.Activate();
		while (!noCardSpellInstance.IsFinished())
		{
			yield return null;
		}
	}

	private float GetRandomSec()
	{
		return UnityEngine.Random.Range(m_RandomSecMin, m_RandomSecMax);
	}

	private bool IsRevealedCardBusy()
	{
		if (m_revealedCard == null)
		{
			return false;
		}
		return m_revealedCard.m_effectsPendingFinish > 0;
	}
}
