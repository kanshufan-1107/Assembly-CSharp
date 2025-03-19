using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ICCPrologueFakeDeath : Spell
{
	private enum FakeDeathState
	{
		EXPLODING_JAINA,
		FROST_LICH_JAINA_ENTER,
		LICH_KING_EXIT,
		TIRION_ENTER,
		COMPLETE
	}

	public Spell m_ExplodeReformSpell;

	public Spell m_LichKingExitSpell;

	public Spell m_TirionEnterSpell;

	public GameObject m_FakeDefeatScreen;

	public float m_FakeDefeatScreenShowTime = 5f;

	public float m_TirionEnterDelay = 2f;

	private Card m_lichKingCard;

	private Card m_tirionCard;

	private Card m_frostLichJainaCard;

	private int m_tirionEnterTaskIndex;

	private int m_frostLichJainaEnterTaskIndex;

	private Spell m_explodeReformSpellInstance;

	private ICC_01_LICHKING m_missionEntity;

	private FakeDeathState m_fakeDeathState;

	private ScreenEffectsHandle m_screenEffectsHandle;

	public override bool AddPowerTargets()
	{
		base.AddPowerTargets();
		if (m_missionEntity == null)
		{
			m_missionEntity = GameState.Get().GetGameEntity() as ICC_01_LICHKING;
			if (m_missionEntity == null)
			{
				Log.Spells.PrintError("ICCPrologueFakeDeath.AddPowerTargets(): GameEntity is not an instance of ICC_01_LICHKING!");
			}
		}
		FindHeroCards();
		return true;
	}

	private void FindHeroCards()
	{
		if (m_lichKingCard == null)
		{
			m_lichKingCard = GameState.Get().GetOpposingSidePlayer().GetHeroCard();
		}
		if (m_frostLichJainaCard == null)
		{
			List<PowerTask> taskList = m_taskList.GetTaskList();
			for (int i = 0; i < taskList.Count; i++)
			{
				if (taskList[i].GetPower() is Network.HistFullEntity fullEntity)
				{
					Entity entity = GameState.Get().GetEntity(fullEntity.Entity.ID);
					if (entity.GetControllerSide() == Player.Side.FRIENDLY && entity.IsHero())
					{
						m_frostLichJainaCard = entity.GetCard();
						m_frostLichJainaEnterTaskIndex = i;
						break;
					}
				}
			}
		}
		if (!(m_tirionCard == null))
		{
			return;
		}
		List<PowerTask> taskList2 = m_taskList.GetTaskList();
		for (int j = 0; j < taskList2.Count; j++)
		{
			if (taskList2[j].GetPower() is Network.HistTagChange tagChange)
			{
				Entity entity2 = GameState.Get().GetEntity(tagChange.Entity);
				if (entity2.GetControllerSide() == Player.Side.OPPOSING && entity2.IsHero() && tagChange.Tag == 262)
				{
					m_tirionCard = entity2.GetCard();
					m_tirionEnterTaskIndex = j;
					break;
				}
			}
		}
	}

	public override bool CanPurge()
	{
		if (m_fakeDeathState != FakeDeathState.COMPLETE)
		{
			return false;
		}
		return base.CanPurge();
	}

	public override bool ShouldReconnectIfStuck()
	{
		return false;
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		base.OnAction(prevStateType);
		StartCoroutine(DoEffects());
	}

	private IEnumerator DoEffects()
	{
		if (m_fakeDeathState == FakeDeathState.EXPLODING_JAINA)
		{
			yield return StartCoroutine(ExplodeJaina());
		}
		if (m_fakeDeathState == FakeDeathState.FROST_LICH_JAINA_ENTER)
		{
			yield return StartCoroutine(FrostJainaEnter());
		}
		if (m_fakeDeathState == FakeDeathState.LICH_KING_EXIT)
		{
			yield return StartCoroutine(LichKingExit());
		}
		if (m_fakeDeathState == FakeDeathState.TIRION_ENTER)
		{
			yield return StartCoroutine(TirionEnter());
		}
		OnSpellFinished();
		OnStateFinished();
	}

	private IEnumerator ExplodeJaina()
	{
		EndTurnButton.Get().AddInputBlocker();
		PegCursor.Get().SetMode(PegCursor.Mode.STOPWAITING);
		MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_EndGameScreen);
		SoundManager.Get().LoadAndPlay("defeat_jingle.prefab:0744a10f38e92f1438a02349c29a7b76");
		StartCoroutine(HideBoardElements());
		Card friendlyHeroCard = GameState.Get().GetFriendlySidePlayer().GetHeroCard();
		friendlyHeroCard.ActivateCharacterDeathEffects();
		m_explodeReformSpellInstance = SpellManager.Get().GetSpell(m_ExplodeReformSpell);
		SpellUtils.SetCustomSpellParent(m_explodeReformSpellInstance, friendlyHeroCard.GetActor());
		m_explodeReformSpellInstance.ActivateState(SpellStateType.ACTION);
		while (m_explodeReformSpellInstance.GetActiveState() != 0)
		{
			yield return null;
		}
		m_fakeDeathState = FakeDeathState.FROST_LICH_JAINA_ENTER;
	}

	private IEnumerator FrostJainaEnter()
	{
		if (!(m_frostLichJainaCard == null))
		{
			m_taskList.DoTasks(0, m_frostLichJainaEnterTaskIndex);
			GameObject fakeDefeatScreenInstance = Object.Instantiate(m_FakeDefeatScreen);
			DefeatTwoScoop defeatTwoScoop = fakeDefeatScreenInstance.GetComponentInChildren<DefeatTwoScoop>(includeInactive: true);
			while (!defeatTwoScoop.IsLoaded())
			{
				yield return null;
			}
			m_screenEffectsHandle = new ScreenEffectsHandle(this);
			ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignetteDesaturatePerspective;
			m_screenEffectsHandle.StartEffect(screenEffectParameters);
			defeatTwoScoop.Show(showXPBar: false);
			yield return new WaitForSeconds(m_FakeDefeatScreenShowTime);
			m_screenEffectsHandle.StopEffect();
			defeatTwoScoop.Hide();
			m_taskList.DoTasks(0, m_frostLichJainaEnterTaskIndex + 1);
			while (m_frostLichJainaCard.GetActor() == null || m_frostLichJainaCard.IsActorLoading())
			{
				yield return null;
			}
			m_frostLichJainaCard.HideCard();
			m_explodeReformSpellInstance.ActivateState(SpellStateType.DEATH);
			if (m_missionEntity != null)
			{
				StartCoroutine(m_missionEntity.PlayLichKingRezLines());
			}
			while (!m_explodeReformSpellInstance.IsFinished())
			{
				yield return null;
			}
			m_frostLichJainaCard.ShowCard();
			m_frostLichJainaCard.GetActor().GetAttackObject().Hide();
			while (m_explodeReformSpellInstance.GetActiveState() != 0)
			{
				yield return null;
			}
			while (GameState.Get().IsBusy())
			{
				yield return null;
			}
			Object.Destroy(fakeDefeatScreenInstance);
			m_fakeDeathState = FakeDeathState.LICH_KING_EXIT;
		}
	}

	private IEnumerator LichKingExit()
	{
		Spell lichKingExitSpellInstance = SpellManager.Get().GetSpell(m_LichKingExitSpell);
		SpellUtils.SetCustomSpellParent(lichKingExitSpellInstance, m_lichKingCard.GetActor());
		lichKingExitSpellInstance.Activate();
		while (lichKingExitSpellInstance.GetActiveState() != 0)
		{
			yield return null;
		}
		yield return new WaitForSeconds(m_TirionEnterDelay);
		m_fakeDeathState = FakeDeathState.TIRION_ENTER;
	}

	private IEnumerator TirionEnter()
	{
		if (!(m_tirionCard == null))
		{
			m_taskList.DoTasks(0, m_tirionEnterTaskIndex + 1);
			m_tirionCard.SetDoNotSort(on: true);
			m_tirionCard.SetDoNotWarpToNewZone(on: true);
			while (m_tirionCard.GetActor() == null || m_tirionCard.IsActorLoading())
			{
				yield return null;
			}
			TransformUtil.CopyWorld(m_tirionCard, m_tirionCard.GetZone().transform);
			m_tirionCard.GetActor().Hide();
			Spell tirionEnterSpellInstance = SpellManager.Get().GetSpell(m_TirionEnterSpell);
			SpellUtils.SetCustomSpellParent(tirionEnterSpellInstance, m_tirionCard.GetActor());
			tirionEnterSpellInstance.Activate();
			while (tirionEnterSpellInstance.GetActiveState() != 0)
			{
				yield return null;
			}
			m_tirionCard.SetDoNotSort(on: false);
			m_tirionCard.SetDoNotWarpToNewZone(on: false);
			NameBanner nameBannerForSide = Gameplay.Get().GetNameBannerForSide(Player.Side.OPPOSING);
			nameBannerForSide.UpdateHeroNameBanner();
			nameBannerForSide.UpdateSubtext();
			m_missionEntity.StartGameplaySoundtracks();
			EndTurnButton.Get().RemoveInputBlocker();
			m_fakeDeathState = FakeDeathState.COMPLETE;
		}
	}

	private IEnumerator HideBoardElements()
	{
		yield return new WaitForSeconds(0.5f);
		Player controller = GameState.Get().GetFriendlySidePlayer();
		if (controller.GetHeroPowerCard() != null)
		{
			controller.GetHeroPowerCard().HideCard();
			controller.GetHeroPowerCard().GetActor().ToggleForceIdle(bOn: true);
			controller.GetHeroPowerCard().GetActor().SetActorState(ActorStateType.CARD_IDLE);
			controller.GetHeroPowerCard().GetActor().DoCardDeathVisuals();
		}
		if (controller.GetWeaponCard() != null)
		{
			controller.GetWeaponCard().HideCard();
			controller.GetWeaponCard().GetActor().ToggleForceIdle(bOn: true);
			controller.GetWeaponCard().GetActor().SetActorState(ActorStateType.CARD_IDLE);
			controller.GetWeaponCard().GetActor().DoCardDeathVisuals();
		}
		Actor actor = controller.GetHeroCard().GetActor();
		actor.HideArmorSpell();
		actor.GetHealthObject().Hide();
		actor.GetAttackObject().Hide();
		actor.ToggleForceIdle(bOn: true);
		actor.SetActorState(ActorStateType.CARD_IDLE);
		yield return new WaitForSeconds(3f);
		Player opposingController = GameState.Get().GetFirstOpponentPlayer(controller);
		if (opposingController.GetHeroPowerCard() != null)
		{
			opposingController.GetHeroPowerCard().HideCard();
			opposingController.GetHeroPowerCard().GetActor().ToggleForceIdle(bOn: true);
			opposingController.GetHeroPowerCard().GetActor().SetActorState(ActorStateType.CARD_IDLE);
			opposingController.GetHeroPowerCard().GetActor().DoCardDeathVisuals();
		}
		if (opposingController.GetWeaponCard() != null)
		{
			opposingController.GetWeaponCard().HideCard();
			opposingController.GetWeaponCard().GetActor().ToggleForceIdle(bOn: true);
			opposingController.GetWeaponCard().GetActor().SetActorState(ActorStateType.CARD_IDLE);
			opposingController.GetWeaponCard().GetActor().DoCardDeathVisuals();
		}
	}
}
