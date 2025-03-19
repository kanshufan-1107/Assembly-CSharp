using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core.Utils;
using PegasusGame;
using UnityEngine;

[CustomEditClass]
public class JoustSpellController : SpellController
{
	private class Jouster
	{
		public Player m_player;

		public Card m_card;

		public int m_deckIndex;

		public Actor m_initialActor;

		public Actor m_revealedActor;

		public int m_effectsPendingFinish;
	}

	public Spell m_WinnerSpellPrefab;

	public Spell m_LoserSpellPrefab;

	public Spell m_NoJousterSpellPrefab;

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

	private int m_joustTaskIndex;

	private const int ONE_SIDED_JOUST = 1;

	private const int TWO_SIDED_JOUST = 2;

	private int m_joustType;

	private Jouster m_friendlyJouster;

	private Jouster m_opponentJouster;

	private Jouster m_winningJouster;

	private Jouster m_sourceJouster;

	private bool m_alwaysShowWinning;

	protected override bool AddPowerSourceAndTargets(PowerTaskList taskList)
	{
		if (!HasSourceCard(taskList))
		{
			return false;
		}
		m_joustTaskIndex = -1;
		List<PowerTask> tasks = taskList.GetTaskList();
		for (int i = 0; i < tasks.Count; i++)
		{
			if (!(tasks[i].GetPower() is Network.HistMetaData { MetaType: HistoryMeta.Type.JOUST } metaData))
			{
				continue;
			}
			m_joustTaskIndex = i;
			if (metaData.AdditionalData != null && metaData.AdditionalData.Count > 0)
			{
				int numJoustSides = metaData.AdditionalData[0];
				if ((uint)(numJoustSides - 1) <= 1u)
				{
					m_joustType = numJoustSides;
				}
				else
				{
					m_joustType = 2;
				}
			}
			else
			{
				m_joustType = 2;
			}
		}
		if (m_joustTaskIndex < 0)
		{
			return false;
		}
		Card sourceCard = taskList.GetSourceEntity().GetCard();
		SetSource(sourceCard);
		return true;
	}

	protected override void OnProcessTaskList()
	{
		StartCoroutine(DoEffectWithTiming());
	}

	private IEnumerator DoEffectWithTiming()
	{
		yield return StartCoroutine(WaitForShowEntities());
		CreateJousters();
		yield return StartCoroutine(ShowJousters());
		yield return StartCoroutine(Joust());
		yield return StartCoroutine(HideJousters());
		DestroyJousters();
		base.OnProcessTaskList();
	}

	private IEnumerator WaitForShowEntities()
	{
		bool complete = false;
		PowerTaskList.CompleteCallback completeCallback = delegate
		{
			complete = true;
		};
		m_taskList.DoTasks(0, m_joustTaskIndex, completeCallback);
		while (!complete)
		{
			yield return null;
		}
	}

	private void CreateJousters()
	{
		Network.HistMetaData metaData = (Network.HistMetaData)m_taskList.GetTaskList()[m_joustTaskIndex].GetPower();
		Player friendlyPlayer = GameState.Get().GetFriendlySidePlayer();
		Player opponentPlayer = GameState.Get().GetOpposingSidePlayer();
		m_friendlyJouster = CreateJouster(friendlyPlayer, metaData);
		m_opponentJouster = CreateJouster(opponentPlayer, metaData);
		DetermineWinner(metaData);
		DetermineSourceJouster();
	}

	private Jouster CreateJouster(Player player, Network.HistMetaData metaData)
	{
		Entity entity = null;
		foreach (int entityId in metaData.Info)
		{
			Entity currEntity = GameState.Get().GetEntity(entityId);
			if (currEntity != null && currEntity.GetController() == player)
			{
				entity = currEntity;
				break;
			}
		}
		if (entity == null)
		{
			return null;
		}
		Card card = entity.GetCard();
		card.SetInputEnabled(enabled: false);
		GameObject hiddenActorGo = AssetLoader.Get().InstantiatePrefab("Card_Hidden.prefab:1a94649d257bc284ca6e2962f634a8b9", AssetLoadingOptions.IgnorePrefabPosition);
		GameObject revealedActorGo = AssetLoader.Get().InstantiatePrefab(ActorNames.GetHandActor(entity), AssetLoadingOptions.IgnorePrefabPosition);
		Jouster jouster = new Jouster();
		jouster.m_player = player;
		jouster.m_card = card;
		jouster.m_initialActor = hiddenActorGo.GetComponent<Actor>();
		jouster.m_revealedActor = revealedActorGo.GetComponent<Actor>();
		Action<Actor> obj = delegate(Actor actor)
		{
			actor.SetEntity(entity);
			actor.SetCard(card);
			actor.SetCardDefFromCard(card);
			actor.UpdateAllComponents();
			actor.Hide();
		};
		obj(jouster.m_initialActor);
		obj(jouster.m_revealedActor);
		return jouster;
	}

	private void DetermineWinner(Network.HistMetaData metaData)
	{
		m_alwaysShowWinning = GameUtils.GetJoustWinner(metaData);
		Card winnerCard = GameUtils.GetJoustWinner(metaData);
		if ((bool)winnerCard)
		{
			if (winnerCard.GetController().IsFriendlySide())
			{
				m_winningJouster = m_friendlyJouster;
			}
			else
			{
				m_winningJouster = m_opponentJouster;
			}
		}
	}

	private void DetermineSourceJouster()
	{
		Player controller = GetSource().GetController();
		if (m_friendlyJouster != null && m_friendlyJouster.m_card.GetController() == controller)
		{
			m_sourceJouster = m_friendlyJouster;
		}
		else if (m_opponentJouster != null && m_opponentJouster.m_card.GetController() == controller)
		{
			m_sourceJouster = m_opponentJouster;
		}
	}

	private IEnumerator ShowJousters()
	{
		if (!string.IsNullOrEmpty(m_DrawStingerPrefab))
		{
			SoundManager.Get().LoadAndPlay(m_DrawStingerPrefab);
		}
		string friendlyBoneName = m_FriendlyBoneName;
		string opponentBoneName = m_OpponentBoneName;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			friendlyBoneName += "_phone";
			opponentBoneName += "_phone";
		}
		Transform friendlyBone = Board.Get().FindBone(friendlyBoneName);
		Transform opponentBone = Board.Get().FindBone(opponentBoneName);
		Quaternion rotation = Quaternion.LookRotation(opponentBone.position - friendlyBone.position);
		if (m_friendlyJouster != null)
		{
			Vector3 localScale = friendlyBone.localScale;
			Vector3 position = friendlyBone.position;
			float delaySec = GetRandomSec();
			float showSec = m_ShowTime + GetRandomSec();
			ShowJouster(m_friendlyJouster, localScale, rotation, position, delaySec, showSec);
		}
		else if (m_joustType == 2)
		{
			PlayNoJousterSpell(GameState.Get().GetFriendlySidePlayer());
		}
		if (m_opponentJouster != null)
		{
			Vector3 localScale2 = opponentBone.localScale;
			Vector3 position2 = opponentBone.position;
			float delaySec2 = GetRandomSec();
			float showSec2 = m_ShowTime + GetRandomSec();
			ShowJouster(m_opponentJouster, localScale2, rotation, position2, delaySec2, showSec2);
		}
		else if (m_joustType == 2)
		{
			PlayNoJousterSpell(GameState.Get().GetOpposingSidePlayer());
		}
		while (IsJousterBusy(m_friendlyJouster) || IsJousterBusy(m_opponentJouster))
		{
			yield return null;
		}
	}

	private void ShowJouster(Jouster jouster, Vector3 localScale, Quaternion rotation, Vector3 position, float delaySec, float showSec)
	{
		jouster.m_effectsPendingFinish++;
		Card card = jouster.m_card;
		ZoneDeck deck = jouster.m_player.GetDeckZone();
		Actor deckActor = deck.GetThicknessForLayout();
		jouster.m_deckIndex = deck.RemoveCard(card);
		Card tempRemovalCard = deck.GetFirstCard();
		deck.RemoveCard(tempRemovalCard);
		deck.SetSuppressEmotes(suppress: true);
		deck.UpdateLayout();
		if (tempRemovalCard != null)
		{
			deck.InsertCard(0, tempRemovalCard);
		}
		float halfShowSec = 0.5f * showSec;
		Vector3 initialPos = deckActor.GetMeshRenderer().bounds.center + Card.IN_DECK_OFFSET;
		Vector3 intermedPos = initialPos + Card.ABOVE_DECK_OFFSET;
		Vector3 finalAngles = rotation.eulerAngles;
		Vector3[] movePath = new Vector3[3] { initialPos, intermedPos, position };
		card.ShowCard();
		jouster.m_initialActor.Show();
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
			jouster.m_effectsPendingFinish--;
			DriftJouster(jouster);
		};
		iTween.Timer(card.gameObject, iTween.Hash("delay", delaySec, "time", showSec, "oncomplete", tweensCompleteCallback));
	}

	private void PlayNoJousterSpell(Player player)
	{
		ZoneDeck deck = player.GetDeckZone();
		Spell spell2 = SpellManager.Get().GetSpell(m_NoJousterSpellPrefab);
		spell2.SetPosition(deck.transform.position);
		spell2.AddStateFinishedCallback(delegate(Spell spell, SpellStateType prevStateType, object userData)
		{
			if (spell.GetActiveState() == SpellStateType.NONE)
			{
				SpellManager.Get().ReleaseSpell(spell);
			}
		});
		spell2.Activate();
	}

	private void DriftJouster(Jouster jouster)
	{
		Card card = jouster.m_card;
		Vector3 center = card.transform.position;
		float height = jouster.m_initialActor.GetMeshRenderer().bounds.size.z;
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

	private IEnumerator Joust()
	{
		if (m_friendlyJouster != null)
		{
			float revealSec = m_RevealTime + GetRandomSec();
			RevealJouster(m_friendlyJouster, revealSec);
		}
		if (m_opponentJouster != null)
		{
			float revealSec2 = m_RevealTime + GetRandomSec();
			RevealJouster(m_opponentJouster, revealSec2);
		}
		if (m_sourceJouster != null)
		{
			while (IsJousterBusy(m_friendlyJouster) || IsJousterBusy(m_opponentJouster))
			{
				yield return null;
			}
			Spell resultSpellPrefab;
			if (m_joustType == 1)
			{
				resultSpellPrefab = ((!m_sourceJouster.m_player.IsFriendlySide()) ? m_LoserSpellPrefab : ((m_sourceJouster == m_winningJouster) ? m_WinnerSpellPrefab : m_LoserSpellPrefab));
			}
			else
			{
				resultSpellPrefab = ((m_sourceJouster == m_winningJouster) ? m_WinnerSpellPrefab : m_LoserSpellPrefab);
				if (m_sourceJouster != m_winningJouster && m_winningJouster != null && m_alwaysShowWinning)
				{
					resultSpellPrefab = m_WinnerSpellPrefab;
					if (m_sourceJouster == m_opponentJouster)
					{
						m_sourceJouster = m_friendlyJouster;
					}
					else if (m_sourceJouster == m_friendlyJouster)
					{
						m_sourceJouster = m_opponentJouster;
					}
				}
			}
			PlaySpellOnActor(m_sourceJouster, m_sourceJouster.m_revealedActor, resultSpellPrefab);
		}
		if (m_friendlyJouster != null || m_opponentJouster != null)
		{
			iTween.Timer(base.gameObject, iTween.Hash("time", m_HoldTime));
		}
		while (IsJousterBusy(m_friendlyJouster) || IsJousterBusy(m_opponentJouster) || iTween.HasTween(base.gameObject))
		{
			yield return null;
		}
	}

	private void RevealJouster(Jouster jouster, float revealSec)
	{
		if (m_joustType == 1 && !m_sourceJouster.m_player.IsFriendlySide())
		{
			return;
		}
		jouster.m_effectsPendingFinish++;
		Card card = jouster.m_card;
		Actor hiddenActor = jouster.m_initialActor;
		Actor revealedActor = jouster.m_revealedActor;
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
			jouster.m_effectsPendingFinish--;
		};
		iTween.Timer(card.gameObject, iTween.Hash("time", revealSec, "onupdate", updateCallback, "oncomplete", completeCallback));
	}

	private IEnumerator HideJousters()
	{
		if (!string.IsNullOrEmpty(m_HideStingerPrefab))
		{
			SoundManager.Get().LoadAndPlay(m_HideStingerPrefab);
		}
		if (m_friendlyJouster != null)
		{
			float delaySec = GetRandomSec();
			float hideSec = m_HideTime + GetRandomSec();
			HideJouster(m_friendlyJouster, delaySec, hideSec);
		}
		if (m_opponentJouster != null)
		{
			float delaySec2 = GetRandomSec();
			float hideSec2 = m_HideTime + GetRandomSec();
			HideJouster(m_opponentJouster, delaySec2, hideSec2);
		}
		while (IsJousterBusy(m_friendlyJouster) || IsJousterBusy(m_opponentJouster))
		{
			yield return null;
		}
	}

	private void HideJouster(Jouster jouster, float delaySec, float hideSec)
	{
		jouster.m_effectsPendingFinish++;
		Card card = jouster.m_card;
		ZoneDeck deck = jouster.m_player.GetDeckZone();
		Vector3 center = deck.GetThicknessForLayout().GetMeshRenderer().bounds.center;
		float halfHideSec = 0.5f * hideSec;
		Vector3 initialPos = card.transform.position;
		Vector3 intermedPos = center + Card.ABOVE_DECK_OFFSET;
		Vector3 finalPos = center + Card.IN_DECK_OFFSET;
		Vector3 finalAngles = Card.IN_DECK_ANGLES;
		if (m_joustType == 1 && !m_sourceJouster.m_player.IsFriendlySide())
		{
			finalAngles.x *= -1f;
		}
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
			jouster.m_effectsPendingFinish--;
			jouster.m_initialActor.GetCard().HideCard();
			deck.InsertCard(jouster.m_deckIndex, card);
			deck.UpdateLayout();
			deck.SetSuppressEmotes(suppress: false);
		};
		iTween.Timer(card.gameObject, iTween.Hash("delay", delaySec, "time", hideSec, "oncomplete", tweensCompleteCallback));
	}

	private void DestroyJousters()
	{
		if (m_friendlyJouster != null)
		{
			DestroyJouster(m_friendlyJouster);
			m_friendlyJouster = null;
		}
		if (m_opponentJouster != null)
		{
			DestroyJouster(m_opponentJouster);
			m_opponentJouster = null;
		}
	}

	private void DestroyJouster(Jouster jouster)
	{
		if (jouster != null)
		{
			jouster.m_card.SetInputEnabled(enabled: true);
			jouster.m_initialActor.Destroy();
			jouster.m_revealedActor.Destroy();
		}
	}

	private float GetRandomSec()
	{
		return UnityEngine.Random.Range(m_RandomSecMin, m_RandomSecMax);
	}

	private bool PlaySpellOnActor(Jouster jouster, Actor actor, Spell spellPrefab)
	{
		if (!spellPrefab)
		{
			return false;
		}
		jouster.m_effectsPendingFinish++;
		Card card = actor.GetCard();
		Spell spell2 = SpellManager.Get().GetSpell(spellPrefab);
		spell2.transform.parent = actor.transform;
		spell2.AddFinishedCallback(delegate
		{
			jouster.m_effectsPendingFinish--;
		});
		spell2.AddStateFinishedCallback(delegate(Spell spell, SpellStateType prevStateType, object userData)
		{
			if (spell.GetActiveState() == SpellStateType.NONE)
			{
				SpellManager.Get().ReleaseSpell(spell);
			}
		});
		spell2.SetSource(card.gameObject);
		spell2.Activate();
		return true;
	}

	private bool IsJousterBusy(Jouster jouster)
	{
		if (jouster == null)
		{
			return false;
		}
		return jouster.m_effectsPendingFinish > 0;
	}
}
