using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwinspellHoldSpell : Spell
{
	private Entity m_originalSpellEntity;

	private List<Actor> m_fakeReplacementsWhenPlayedActors = new List<Actor>();

	private int m_fakeTwinspellHandSlot = -1;

	private int m_numFakeActorsLoaded;

	private int m_numFakeActorsGoingToLoad;

	protected override void OnBirth(SpellStateType prevStateType)
	{
		base.OnBirth(prevStateType);
		StartCoroutine(DoUpdate());
	}

	protected override void OnDeath(SpellStateType prevStateType)
	{
		base.OnDeath(prevStateType);
		StopAllCoroutines();
		HideFakeHandReplacementActors();
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		base.OnAction(prevStateType);
		StopAllCoroutines();
	}

	private SpellType GetCorrectHoldSpell(EntityDef childCard)
	{
		SpellType animationType = SpellType.TWINSPELLPENDING;
		if (childCard != null)
		{
			animationType = (childCard.HasTag(GAME_TAG.MINI) ? SpellType.MINIATURIZE_PENDING : ((!childCard.HasTag(GAME_TAG.GIGANTIC)) ? SpellType.TWINSPELLPENDING : SpellType.GIGANTIFY_PENDING));
		}
		else if (m_originalSpellEntity != null)
		{
			animationType = (m_originalSpellEntity.HasTag(GAME_TAG.MINIATURIZE) ? SpellType.MINIATURIZE_PENDING : ((!m_originalSpellEntity.HasTag(GAME_TAG.GIGANTIFY)) ? SpellType.TWINSPELLPENDING : SpellType.GIGANTIFY_PENDING));
		}
		return animationType;
	}

	private IEnumerator DoUpdate()
	{
		if (m_fakeReplacementsWhenPlayedActors == null)
		{
			yield break;
		}
		while (IsStillLoadingActors())
		{
			yield return new WaitForEndOfFrame();
		}
		ZoneHand handZone = InputManager.Get().GetFriendlyHand();
		ShowFakeHandReplacementActors();
		int additionalCardsGoingIntoHand = m_numFakeActorsGoingToLoad;
		int currentHandSize = handZone.GetCardCount();
		int handSpaceOverride = additionalCardsGoingIntoHand + currentHandSize - 1;
		int playerMaxHandSize = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.MAXHANDSIZE);
		handSpaceOverride = Math.Clamp(handSpaceOverride, 0, playerMaxHandSize);
		while (true)
		{
			int handSlotNumber = m_fakeTwinspellHandSlot;
			foreach (Actor fakeReplacementsWhenPlayedActor in m_fakeReplacementsWhenPlayedActors)
			{
				int overrideSize = ((handSpaceOverride != playerMaxHandSize) ? handSpaceOverride : (-1));
				fakeReplacementsWhenPlayedActor.transform.position = handZone.GetCardPosition(handSlotNumber, overrideSize);
				fakeReplacementsWhenPlayedActor.transform.localEulerAngles = handZone.GetCardRotation(handSlotNumber, overrideSize);
				fakeReplacementsWhenPlayedActor.transform.localScale = handZone.GetCardScale();
				handSlotNumber++;
			}
			yield return null;
		}
	}

	private bool IsStillLoadingActors()
	{
		return m_numFakeActorsLoaded < m_numFakeActorsGoingToLoad;
	}

	public bool Initialize(int heldEntityId, int zonePosition)
	{
		m_numFakeActorsLoaded = 0;
		m_numFakeActorsGoingToLoad = 0;
		m_originalSpellEntity = GameState.Get().GetEntity(heldEntityId);
		if (m_originalSpellEntity == null)
		{
			Log.Spells.PrintError("TwinspellHoldSpell.Initialize(): Unable to find Entity for Entity ID {0}.", heldEntityId);
			return false;
		}
		if (!m_originalSpellEntity.HasReplacementsWhenPlayed())
		{
			Log.Spells.PrintError("TwinspellHoldSpell.Initialize(): TwinspellHoldSpell has been hooked up to a Card that does not have replacements!");
			return false;
		}
		DestroyAllLoadedActors();
		if (!LoadFakeOnPlayReplacementActors())
		{
			Log.Spells.PrintError("TwinspellHoldSpell.Initialize(): Failed to load the fake hand replacement-on-play actor", heldEntityId);
			return false;
		}
		m_fakeTwinspellHandSlot = zonePosition - 1;
		return true;
	}

	private void DestroyAllLoadedActors()
	{
		if (m_fakeReplacementsWhenPlayedActors == null)
		{
			return;
		}
		foreach (Actor actor in m_fakeReplacementsWhenPlayedActors)
		{
			if (!(actor == null))
			{
				actor.DeactivateAllSpells();
				actor.Destroy();
			}
		}
		m_fakeReplacementsWhenPlayedActors.Clear();
	}

	public int GetOriginalSpellEntityId()
	{
		if (m_originalSpellEntity == null)
		{
			return -1;
		}
		return m_originalSpellEntity.GetEntityId();
	}

	public int GetFakeReplacementZonePosition()
	{
		return m_fakeTwinspellHandSlot + 1;
	}

	private bool LoadFakeOnPlayReplacementActors()
	{
		DestroyAllLoadedActors();
		if (m_originalSpellEntity == null)
		{
			Log.Spells.PrintError("TwinspellHoldSpell.LoadFakeTwinspellActor(): m_originalSpellEntity is null. Has TwinspellHoldSpell.Initialize() been called?");
			return false;
		}
		if (!m_originalSpellEntity.HasReplacementsWhenPlayed())
		{
			Log.Spells.PrintError("TwinspellHoldSpell.LoadFakeTwinspellActor(): m_originalSpellEntity does not have on-play replacements listed");
			return false;
		}
		List<int> replacementsWhenPlayed = m_originalSpellEntity.GetReplacementsWhenPlayed();
		int totalSizeWithReplacements = InputManager.Get().GetFriendlyHand().GetCardCount() + replacementsWhenPlayed.Count - 1;
		int maxHandSize = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.MAXHANDSIZE);
		if (totalSizeWithReplacements >= maxHandSize)
		{
			int numToCut = totalSizeWithReplacements - maxHandSize;
			int remainingSafeToSpawn = replacementsWhenPlayed.Count - numToCut;
			replacementsWhenPlayed = replacementsWhenPlayed.GetRange(0, remainingSafeToSpawn);
			m_numFakeActorsGoingToLoad = remainingSafeToSpawn;
		}
		else
		{
			m_numFakeActorsGoingToLoad = replacementsWhenPlayed.Count;
		}
		while (m_fakeReplacementsWhenPlayedActors.Count < replacementsWhenPlayed.Count)
		{
			m_fakeReplacementsWhenPlayedActors.Add(null);
		}
		for (int i = 0; i < replacementsWhenPlayed.Count; i++)
		{
			int replacementDbid = replacementsWhenPlayed[i];
			using DefLoader.DisposableFullDef onPlayReplacementDef = DefLoader.Get().GetFullDef(replacementDbid);
			if (onPlayReplacementDef?.EntityDef == null)
			{
				Log.Spells.PrintError("TwinspellHoldSpell.LoadFakeOnPlayReplacementActor(): Unable to load EntityDef for card ID {0}.", replacementDbid);
				continue;
			}
			if (onPlayReplacementDef?.CardDef == null)
			{
				Log.Spells.PrintError("TwinspellHoldSpell.LoadFakeOnPlayReplacementActor(): Unable to load CardDef for card ID {0}.", replacementDbid);
				continue;
			}
			string replacementCardId = GameUtils.TranslateDbIdToCardId(replacementDbid);
			int replacementIndex = i;
			AssetLoader.Get().InstantiatePrefab(ActorNames.GetHandActor(onPlayReplacementDef.EntityDef, m_originalSpellEntity.GetPremiumType()), delegate(AssetReference actorName, GameObject actorGameObject, object data)
			{
				OnFakeHandReplacementActorLoaded(actorName, actorGameObject, replacementCardId, m_originalSpellEntity.GetPremiumType(), replacementIndex);
			}, replacementCardId, AssetLoadingOptions.IgnorePrefabPosition);
		}
		return true;
	}

	private void OnFakeHandReplacementActorLoaded(AssetReference assetRef, GameObject actorGameObject, string fakeTwinspellCardId, TAG_PREMIUM premium, int index)
	{
		if (actorGameObject == null)
		{
			Debug.LogError($"TwinspellHoldSpell.OnFakeTwinspellActorLoaded: Unable to load fake actor for card: {assetRef}");
			return;
		}
		Actor actor = actorGameObject.GetComponent<Actor>();
		if (!(actor == null))
		{
			using (DefLoader.DisposableFullDef twinspellDef = DefLoader.Get().GetFullDef(fakeTwinspellCardId, actor.CardPortraitQuality))
			{
				actor.SetFullDef(twinspellDef);
			}
			actor.SetPremium(premium);
			actor.SetCardBackSideOverride(m_originalSpellEntity.GetControllerSide());
			actor.SetWatermarkCardSetOverride(m_originalSpellEntity.GetWatermarkCardSetOverride());
			actor.UpdateAllComponents();
			actor.Hide();
			m_fakeReplacementsWhenPlayedActors[index] = actor;
			m_numFakeActorsLoaded++;
		}
	}

	private void ShowFakeHandReplacementActors()
	{
		if (m_fakeReplacementsWhenPlayedActors == null)
		{
			return;
		}
		ZoneHand hand = InputManager.Get().GetFriendlyHand();
		int handSlot = m_fakeTwinspellHandSlot;
		List<int> slotsToReserve = new List<int>();
		for (int i = 0; i < m_fakeReplacementsWhenPlayedActors.Count; i++)
		{
			slotsToReserve.Add(i + handSlot);
		}
		hand.ReserveCardSlot(slotsToReserve.ToArray());
		foreach (Actor fakeActor in m_fakeReplacementsWhenPlayedActors)
		{
			if (!(fakeActor == null))
			{
				SpellType holdSpell = GetCorrectHoldSpell(fakeActor.GetEntityDef());
				ShowFakeHandReplacementActor(holdSpell, fakeActor, handSlot);
				handSlot++;
			}
		}
		if (m_numFakeActorsGoingToLoad > 1)
		{
			hand.UpdateLayout(null, forced: true);
		}
	}

	private void ShowFakeHandReplacementActor(SpellType chosenHoldSpell, Actor thisActor, int handSlot)
	{
		if (!(thisActor != null) || !thisActor.IsShown())
		{
			ZoneHand handZone = InputManager.Get().GetFriendlyHand();
			thisActor.transform.position = handZone.GetCardPosition(handSlot, -1);
			thisActor.transform.localEulerAngles = handZone.GetCardRotation(handSlot, -1);
			thisActor.transform.localScale = handZone.GetCardScale();
			thisActor.ActivateSpellBirthState(chosenHoldSpell);
			thisActor.Show();
		}
	}

	private void HideFakeHandReplacementActors()
	{
		if (m_fakeReplacementsWhenPlayedActors == null)
		{
			return;
		}
		foreach (Actor actor in m_fakeReplacementsWhenPlayedActors)
		{
			if (!(actor == null))
			{
				actor.ActivateSpellDeathState(GetCorrectHoldSpell(actor.GetEntityDef()));
			}
		}
		InputManager.Get().GetFriendlyHand()?.ClearReservedCard();
	}
}
