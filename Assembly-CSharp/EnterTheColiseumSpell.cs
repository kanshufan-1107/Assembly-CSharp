using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class EnterTheColiseumSpell : Spell
{
	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_SpellStartSoundPrefab;

	public float m_survivorLiftHeight = 2f;

	public float m_LiftTime = 0.5f;

	public float m_LiftOffset = 0.1f;

	public float m_DestroyMinionDelay = 0.5f;

	public float m_LowerDelay = 1.5f;

	public float m_LowerOffset = 0.05f;

	public float m_LowerTime = 0.7f;

	public float m_LightingFadeTime = 0.5f;

	public float m_CameraShakeMagnitude = 0.075f;

	public iTween.EaseType m_liftEaseType = iTween.EaseType.easeInQuart;

	public iTween.EaseType m_lowerEaseType = iTween.EaseType.easeOutCubic;

	public iTween.EaseType m_lightFadeEaseType = iTween.EaseType.easeOutCubic;

	public Spell m_survivorSpellPrefab;

	public Spell m_DustSpellPrefab;

	public bool m_survivorsMeetInMiddle = true;

	public Spell m_ImpactSpellPrefab;

	public string m_RaiseSoundName;

	private List<Card> m_survivorCards;

	private bool m_effectsPlaying;

	private int m_numSurvivorSpellsPlaying;

	private ScreenEffectsHandle m_screenEffectsHandle;

	protected override void Awake()
	{
		base.Awake();
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		base.OnAction(prevStateType);
		m_survivorCards = FindSurvivors();
		StartCoroutine(PerformActions());
	}

	private IEnumerator PerformActions()
	{
		m_effectsPlaying = true;
		foreach (Card card in m_survivorCards)
		{
			if (!(card == null))
			{
				card.SetDoNotSort(on: true);
				card.GetActor().SetUnlit();
				LiftCard(card);
				yield return new WaitForSeconds(m_LiftOffset);
			}
		}
		float lightingFadeTime = m_LightingFadeTime;
		iTween.EaseType lightFadeEaseType = m_lightFadeEaseType;
		VignetteParameters? vignette = new VignetteParameters(1f);
		ScreenEffectParameters screenEffectParameters = new ScreenEffectParameters(ScreenEffectType.VIGNETTE, ScreenEffectPassLocation.PERSPECTIVE, lightingFadeTime, lightFadeEaseType, null, vignette, null, null);
		m_screenEffectsHandle.StartEffect(screenEffectParameters);
		if (!string.IsNullOrEmpty(m_SpellStartSoundPrefab))
		{
			SoundManager.Get().LoadAndPlay(m_SpellStartSoundPrefab);
		}
		PlayDustCloudSpell();
		yield return new WaitForSeconds(m_LiftTime);
		foreach (Card card2 in m_survivorCards)
		{
			if (!(card2 == null))
			{
				PlaySurvivorSpell(card2);
			}
		}
		yield return new WaitForSeconds(m_DestroyMinionDelay);
		OnSpellFinished();
		CameraShakeMgr.Shake(Camera.main, new Vector3(m_CameraShakeMagnitude, m_CameraShakeMagnitude, m_CameraShakeMagnitude), 0.75f);
		yield return new WaitForSeconds(m_LowerDelay);
		while (m_numSurvivorSpellsPlaying > 0)
		{
			yield return null;
		}
		foreach (Card card3 in m_survivorCards)
		{
			if (!(card3 == null))
			{
				Zone cardZone = card3.GetZone();
				if (cardZone is ZonePlay)
				{
					ZonePlay zone = (ZonePlay)cardZone;
					LowerCard(card3.gameObject, zone.GetCardPosition(card3));
					yield return new WaitForSeconds(m_LowerOffset);
				}
			}
		}
		m_screenEffectsHandle.StopEffect(m_LightingFadeTime, m_lightFadeEaseType);
		if (m_ImpactSpellPrefab != null)
		{
			foreach (Card card4 in m_survivorCards)
			{
				if (card4 == null)
				{
					continue;
				}
				Spell spell2 = SpellManager.Get().GetSpell(m_ImpactSpellPrefab);
				spell2.transform.parent = card4.gameObject.transform;
				spell2.transform.localPosition = new Vector3(0f, 0f, 0f);
				spell2.AddStateFinishedCallback(delegate(Spell spell, SpellStateType prevStateType, object userData)
				{
					m_effectsPlaying = false;
					if (spell.GetActiveState() == SpellStateType.NONE)
					{
						SpellManager.Get().ReleaseSpell(spell);
					}
				});
				spell2.Activate();
				yield return new WaitForSeconds(m_LowerOffset);
			}
		}
		yield return new WaitForSeconds(m_LowerTime);
		foreach (Card card5 in m_survivorCards)
		{
			if (!(card5 == null))
			{
				card5.SetDoNotSort(on: false);
				card5.GetActor().SetLit();
			}
		}
		foreach (ZonePlay item in ZoneMgr.Get().FindZonesOfType<ZonePlay>())
		{
			item.UpdateLayout();
		}
		while (m_effectsPlaying)
		{
			yield return null;
		}
		OnStateFinished();
	}

	private void LiftCard(Card card)
	{
		GameObject obj = card.gameObject;
		Vector3 OrgPos = obj.transform.position;
		Vector3 zoneCenter = card.GetZone().gameObject.transform.position;
		Hashtable liftArgs = iTweenManager.Get().GetTweenHashTable();
		liftArgs.Add("time", m_LiftTime);
		liftArgs.Add("position", new Vector3(m_survivorsMeetInMiddle ? zoneCenter.x : OrgPos.x, OrgPos.y + m_survivorLiftHeight, OrgPos.z));
		liftArgs.Add("onstart", (Action<object>)delegate
		{
			SoundManager.Get().LoadAndPlay(m_RaiseSoundName);
		});
		liftArgs.Add("easetype", m_liftEaseType);
		iTween.MoveTo(obj, liftArgs);
	}

	private void LowerCard(GameObject target, Vector3 finalPosition)
	{
		Hashtable slamPosArgs = iTweenManager.Get().GetTweenHashTable();
		slamPosArgs.Add("time", m_LowerTime);
		slamPosArgs.Add("position", finalPosition);
		slamPosArgs.Add("easetype", m_lowerEaseType);
		iTween.MoveTo(target, slamPosArgs);
	}

	private List<Card> FindSurvivors()
	{
		List<Card> survivors = new List<Card>();
		foreach (GameObject target in m_targets)
		{
			Card targetCard = target.GetComponent<Card>();
			bool isSurvivor = true;
			foreach (PowerTask task in m_taskList.GetTaskList())
			{
				Network.PowerHistory power = task.GetPower();
				if (power.Type != Network.PowerType.TAG_CHANGE)
				{
					continue;
				}
				Network.HistTagChange tagChange = power as Network.HistTagChange;
				if (tagChange.Tag == 360 && tagChange.Value == 1)
				{
					Entity entity = GameState.Get().GetEntity(tagChange.Entity);
					if (entity == null)
					{
						string msg = $"{this}.FindSurvivors() - WARNING trying to get entity with id {tagChange.Entity} but there is no entity with that id";
						Log.Power.PrintWarning(msg);
					}
					else if (targetCard == entity.GetCard())
					{
						isSurvivor = false;
						break;
					}
				}
			}
			if (isSurvivor)
			{
				survivors.Add(targetCard);
			}
		}
		return survivors;
	}

	private void PlaySurvivorSpell(Card card)
	{
		if (m_survivorSpellPrefab == null)
		{
			return;
		}
		m_numSurvivorSpellsPlaying++;
		Spell spell2 = SpellManager.Get().GetSpell(m_survivorSpellPrefab);
		spell2.transform.parent = card.GetActor().transform;
		spell2.AddFinishedCallback(delegate
		{
			m_numSurvivorSpellsPlaying--;
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
	}

	private void PlayDustCloudSpell()
	{
		if (m_DustSpellPrefab == null)
		{
			return;
		}
		Spell spell2 = SpellManager.Get().GetSpell(m_DustSpellPrefab);
		spell2.AddStateFinishedCallback(delegate(Spell spell, SpellStateType prevStateType, object userData)
		{
			if (spell.GetActiveState() == SpellStateType.NONE)
			{
				SpellManager.Get().ReleaseSpell(spell);
			}
		});
		spell2.Activate();
	}
}
