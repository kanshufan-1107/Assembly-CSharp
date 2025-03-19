using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Game.Spells;
using UnityEngine;

public class AttackSpellController : SpellController
{
	public HeroAttackDef m_HeroInfo;

	public AllyAttackDef m_AllyInfo;

	public float m_ImpactStagingPoint = 1f;

	public float m_SourceImpactOffset = -0.25f;

	public SpellHandleValueRange[] m_ImpactDefHandles;

	public SpellHandleValueRange[] m_CriticalImpactDefHandles;

	public string m_DefaultImpactSpellPrefabHandle;

	private const float PROPOSED_ATTACK_IMPACT_POINT_SCALAR = 0.5f;

	private const float WINDFURY_REMINDER_WAIT_SEC = 1.2f;

	private const int FINISHER_DAMAGE_THRESHOLD = 15;

	private const string DEFAULT_SHATTER_SPELL = "Bacon_EndRound_HeroImpact.prefab:34d052b6989dcea4c8b7d22adcb31368";

	protected AttackType m_attackType;

	protected Spell m_sourceAttackSpell;

	private Coroutine m_finisherTrackingCoroutine;

	private readonly WaitForSeconds MAX_FINISHER_DURATION = new WaitForSeconds(15f);

	private Vector3 m_sourcePos;

	private Vector3 m_sourceToTarget;

	private Vector3 m_sourceFacing;

	private bool m_repeatProposed;

	protected override bool AddPowerSourceAndTargets(PowerTaskList taskList)
	{
		m_attackType = taskList.GetAttackType();
		m_repeatProposed = taskList.IsRepeatProposedAttack();
		if (m_attackType == AttackType.INVALID)
		{
			return false;
		}
		Entity attacker = taskList.GetAttacker();
		if (attacker != null)
		{
			SetSource(attacker.GetCard());
		}
		Entity defender = taskList.GetDefender();
		if (defender != null)
		{
			AddTarget(defender.GetCard());
		}
		return true;
	}

	protected override void OnProcessTaskList()
	{
		Log.Gameplay.PrintDebug("AttackSpellController -- OnProcessTaskList");
		if (m_attackType == AttackType.ONLY_PROPOSED_ATTACKER || m_attackType == AttackType.ONLY_PROPOSED_DEFENDER || m_attackType == AttackType.ONLY_ATTACKER || m_attackType == AttackType.ONLY_DEFENDER || m_attackType == AttackType.WAITING_ON_PROPOSED_ATTACKER || m_attackType == AttackType.WAITING_ON_PROPOSED_DEFENDER || m_attackType == AttackType.WAITING_ON_ATTACKER || m_attackType == AttackType.WAITING_ON_DEFENDER)
		{
			FinishEverything();
			return;
		}
		if (m_repeatProposed)
		{
			FinishEverything();
			return;
		}
		Card sourceCard = GetSource();
		if (sourceCard == null || sourceCard.GetActor() == null)
		{
			FinishEverything();
			return;
		}
		Entity sourceEntity = sourceCard.GetEntity();
		if (sourceEntity == null)
		{
			FinishEverything();
			return;
		}
		Zone sourceZone = sourceCard.GetZone();
		if (sourceZone == null)
		{
			sourceZone = ZoneMgr.Get().FindZoneOfType<ZonePlay>(sourceCard.GetControllerSide());
		}
		if ((GameMgr.Get().IsBattlegrounds() || GameMgr.Get().IsBattlegroundsTutorial() || GameMgr.Get().IsFriendlyBattlegrounds()) && sourceEntity.IsHero())
		{
			FinisherGameplaySettings finisherSettings = FinisherGameplaySettings.GetFinisherGameplaySettings(sourceEntity);
			Card targetCard = GetTarget();
			Entity targetEntity = targetCard.GetEntity();
			bool opponentFinisher = sourceEntity.IsControlledByOpposingSidePlayer();
			string spellPath = GetSpellPath(sourceEntity, targetEntity, opponentFinisher, finisherSettings);
			if (string.IsNullOrEmpty(spellPath))
			{
				AssetReference finisherReference = AssetReference.CreateFromAssetString(GameDbf.BattlegroundsFinisher.GetRecord(1).GameplaySettings);
				finisherSettings = AssetLoader.Get().LoadAsset<FinisherGameplaySettings>(finisherReference).Asset;
				int favoriteFinisherId = sourceEntity.GetTag(GAME_TAG.BATTLEGROUNDS_FAVORITE_FINISHER);
				if (opponentFinisher)
				{
					Log.Spells.PrintError($"Finisher ID {favoriteFinisherId} is missing a small opponent finisher prefab entry in its gameplay settings. Using default finisher.");
					spellPath = finisherSettings.SmallOpponentPrefab;
				}
				else
				{
					Log.Spells.PrintError($"Finisher ID {favoriteFinisherId} is missing a small finisher prefab entry in its gameplay settings. Using default finisher.");
					spellPath = finisherSettings.SmallPrefab;
				}
				if (string.IsNullOrEmpty(spellPath))
				{
					string finisherType = (opponentFinisher ? "Small Opponent" : "Small");
					Error.AddDevWarning("Missing Default Finisher Path", "Unable to get spellpath for the " + finisherType + " Default Finisher. Make sure " + finisherReference.FileName + " contains a prefab for " + finisherType + " Prefab.");
				}
			}
			m_sourceAttackSpell = InstantiateFinisherSpell(sourceCard.gameObject, spellPath);
			if (m_sourceAttackSpell != null)
			{
				m_sourceAttackSpell.SetSource(sourceCard.gameObject);
				m_sourceAttackSpell.AddTarget(targetCard.gameObject);
				m_sourceAttackSpell.AddFinishedCallback(OnBattlegroundsFinisherFinished, finisherSettings);
				m_finisherTrackingCoroutine = StartCoroutine(EnsureFinisherCompletes(m_sourceAttackSpell));
				SuperSpell finisherSuperSpell = m_sourceAttackSpell as SuperSpell;
				if (finisherSuperSpell != null)
				{
					finisherSuperSpell.ActivateFinisher(opponentFinisher);
				}
				else
				{
					m_sourceAttackSpell.Activate();
				}
				return;
			}
			int favoriteFinisherId2 = sourceEntity.GetTag(GAME_TAG.BATTLEGROUNDS_FAVORITE_FINISHER);
			Log.Spells.PrintError($"Finisher ID {favoriteFinisherId2} failed to instantiate finisher spell at {spellPath}. No spell will be played.");
		}
		bool isSourceFriendly = sourceZone.m_Side == Player.Side.FRIENDLY;
		m_sourceAttackSpell = GetSourceAttackSpell(sourceCard, isSourceFriendly);
		if (m_attackType == AttackType.CANCELED)
		{
			CancelAttackSpell(sourceEntity, m_sourceAttackSpell);
			sourceCard.SetDoNotSort(on: false);
			sourceZone.UpdateLayout();
			sourceCard.EnableAttacking(enable: false);
			FinishEverything();
			return;
		}
		if (m_sourceAttackSpell == null)
		{
			FinishEverything();
			return;
		}
		sourceCard.EnableAttacking(enable: true);
		if (sourceEntity.GetTag(GAME_TAG.IMMUNE_WHILE_ATTACKING) != 0)
		{
			sourceCard.ActivateActorSpell(SpellType.IMMUNE);
		}
		else if (!sourceCard.ShouldShowImmuneVisuals())
		{
			SpellUtils.ActivateDeathIfNecessary(sourceCard.GetActor().GetSpellIfLoaded(SpellType.IMMUNE));
		}
		m_sourceAttackSpell.AddStateStartedCallback(OnSourceAttackStateStarted);
		if (GameState.Get().GetBooleanGameOption(GameEntityOption.USE_FASTER_ATTACK_SPELL_BIRTH_STATE))
		{
			List<ISpellState> birthStates = m_sourceAttackSpell.GetSpellStates(SpellStateType.BIRTH);
			if (birthStates != null)
			{
				foreach (ISpellState spellState in birthStates)
				{
					if (spellState.NumExternalAnimatedObjects <= 0)
					{
						continue;
					}
					foreach (ISpellStateAnimObject externalAnimatedObject in spellState.ExternalAnimatedObjects)
					{
						if (externalAnimatedObject is SpellStateAnimObject obj)
						{
							obj.m_AnimSpeed = 2f;
						}
					}
				}
			}
		}
		if (isSourceFriendly)
		{
			if (m_sourceAttackSpell.GetActiveState() != SpellStateType.IDLE && m_sourceAttackSpell.GetActiveState() != SpellStateType.ACTION)
			{
				if (m_sourceAttackSpell.GetActiveState() != SpellStateType.BIRTH)
				{
					m_sourceAttackSpell.ActivateState(SpellStateType.BIRTH);
				}
			}
			else
			{
				m_sourceAttackSpell.ActivateState(SpellStateType.ACTION);
			}
		}
		else if (m_sourceAttackSpell.GetActiveState() != SpellStateType.IDLE && m_sourceAttackSpell.GetActiveState() != SpellStateType.ACTION)
		{
			m_sourceAttackSpell.ActivateState(SpellStateType.BIRTH);
		}
		else
		{
			m_sourceAttackSpell.ActivateState(SpellStateType.ACTION);
		}
	}

	protected void OnSourceAttackStateStarted(Spell spell, SpellStateType prevStateType, object userData)
	{
		switch (spell.GetActiveState())
		{
		case SpellStateType.IDLE:
			spell.ActivateState(SpellStateType.ACTION);
			break;
		case SpellStateType.ACTION:
			spell.RemoveStateStartedCallback(OnSourceAttackStateStarted);
			LaunchAttack();
			break;
		}
	}

	protected void LaunchAttack()
	{
		Log.Gameplay.PrintDebug("AttackSpellController -- LaunchAttack");
		Card sourceCard = GetSource();
		if (sourceCard == null || m_sourceAttackSpell == null)
		{
			FinishEverything();
			return;
		}
		Entity sourceEntity = sourceCard.GetEntity();
		Card targetCard = GetTarget();
		if (sourceEntity == null || targetCard == null)
		{
			FinishEverything();
			return;
		}
		bool isProposedAttack = m_attackType == AttackType.PROPOSED;
		if (isProposedAttack && sourceEntity.IsHero())
		{
			m_sourceAttackSpell.ActivateState(SpellStateType.IDLE);
			FinishEverything();
			return;
		}
		if (sourceEntity.IsHero() && sourceEntity.HasTag(GAME_TAG.HERO_DOESNT_MOVE_ON_ATTACK))
		{
			m_sourceAttackSpell.RemoveAllTargets();
			m_sourceAttackSpell.AddTarget(targetCard.gameObject);
			m_sourceAttackSpell.AddStateFinishedCallback(OnStaticHeroSourceAttackStateFinished);
			return;
		}
		m_sourcePos = sourceCard.transform.position;
		m_sourceToTarget = targetCard.transform.position - m_sourcePos;
		Vector3 impactPos = ComputeImpactPos();
		sourceCard.SetDoNotSort(!isProposedAttack);
		MoveSourceToTarget(sourceCard, sourceEntity, impactPos);
		if (sourceEntity.IsHero())
		{
			OrientSourceHeroToTarget(sourceCard);
		}
		if (!isProposedAttack)
		{
			targetCard.SetDoNotSort(on: true);
			MoveTargetToSource(targetCard, sourceEntity, impactPos);
		}
	}

	private bool HasFinishAttackSpellOnDamage()
	{
		Card sourceCard = GetSource();
		if (sourceCard == null)
		{
			return false;
		}
		Entity sourceEntity = sourceCard.GetEntity();
		if (sourceEntity == null)
		{
			return false;
		}
		if (!sourceEntity.IsHero())
		{
			return sourceEntity.HasTag(GAME_TAG.FINISH_ATTACK_SPELL_ON_DAMAGE);
		}
		if (sourceEntity.HasTag(GAME_TAG.CUTSCENE_CARD_TYPE))
		{
			return true;
		}
		Player player = sourceEntity.GetController();
		if (player == null)
		{
			return false;
		}
		Card weapon = player.GetWeaponCard();
		if (weapon == null)
		{
			return false;
		}
		return weapon.GetEntity()?.HasTag(GAME_TAG.FINISH_ATTACK_SPELL_ON_DAMAGE) ?? false;
	}

	private void UpdateTargetOnMoveToTargetFinished(Card targetCard)
	{
		targetCard.SetDoNotSort(on: false);
		Zone targetZone = targetCard.GetZone();
		if (targetZone == null)
		{
			targetZone = targetCard.GetPrevZone();
			if (targetZone == null)
			{
				return;
			}
			if (!targetCard.GetEntity().IsHero())
			{
				Log.Spells.PrintWarning("AttackSpellController.UpdateTargetOnMoveToTargetFinished() - Non-hero target ({0}) was moved from {1} to SETASIDE before the attack was resolved.", targetCard.name, targetZone.name);
			}
		}
		targetZone.UpdateLayout();
	}

	private void OnMoveToTargetFinished()
	{
		Log.Gameplay.PrintDebug("AttackSpellController -- OnMoveToTargetFinished");
		Card sourceCard = GetSource();
		Entity sourceEntity = sourceCard.GetEntity();
		Card targetCard = GetTarget();
		bool isProposedAttack = m_attackType == AttackType.PROPOSED;
		DoTasks(sourceCard, targetCard);
		if (!isProposedAttack)
		{
			ActivateImpactEffects(sourceCard, targetCard);
		}
		if (sourceEntity.IsHero())
		{
			MoveSourceHeroBack(sourceCard);
			OrientSourceHeroBack(sourceCard);
			UpdateTargetOnMoveToTargetFinished(targetCard);
			if (HasFinishAttackSpellOnDamage())
			{
				FinishHeroAttack();
			}
			return;
		}
		if (isProposedAttack)
		{
			FinishEverything();
			return;
		}
		if (!sourceEntity.IsCutsceneEntity())
		{
			sourceCard.SetDoNotSort(on: false);
			Zone sourceZone = sourceCard.GetZone();
			if (sourceZone != null)
			{
				sourceZone.UpdateLayout();
			}
		}
		else
		{
			MoveSourceHeroBack(sourceCard);
			OrientSourceHeroBack(sourceCard);
		}
		UpdateTargetOnMoveToTargetFinished(targetCard);
		if (HasFinishAttackSpellOnDamage())
		{
			FinishAttackSpellController();
		}
		else
		{
			m_sourceAttackSpell.AddStateFinishedCallback(OnMinionSourceAttackStateFinished);
		}
		m_sourceAttackSpell.ActivateState(SpellStateType.DEATH);
	}

	private void DoTasks(Card sourceCard, Card targetCard)
	{
		if (m_taskList != null)
		{
			GameUtils.DoDamageTasks(m_taskList, sourceCard, targetCard);
		}
	}

	private void MoveSourceHeroBack(Card sourceCard)
	{
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("position", m_sourcePos);
		args.Add("time", m_HeroInfo.m_MoveBackDuration);
		args.Add("easetype", m_HeroInfo.m_MoveBackEaseType);
		args.Add("oncomplete", "OnHeroMoveBackFinished");
		args.Add("oncompletetarget", base.gameObject);
		iTween.MoveTo(sourceCard.gameObject, args);
	}

	private void OrientSourceHeroBack(Card sourceCard)
	{
		Quaternion orientation = Quaternion.LookRotation(m_sourceFacing);
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("rotation", orientation.eulerAngles);
		args.Add("time", m_HeroInfo.m_OrientBackDuration);
		args.Add("easetype", m_HeroInfo.m_OrientBackEaseType);
		iTween.RotateTo(sourceCard.gameObject, args);
	}

	protected virtual void OnHeroMoveBackFinished()
	{
		Card source = GetSource();
		Entity sourceEntity = source.GetEntity();
		source.SetDoNotSort(on: false);
		source.EnableAttacking(enable: false);
		if (!HasFinishAttackSpellOnDamage())
		{
			if (sourceEntity.GetController().IsLocalUser() || m_sourceAttackSpell.GetActiveState() == SpellStateType.NONE)
			{
				FinishHeroAttack();
			}
			else
			{
				m_sourceAttackSpell.AddStateFinishedCallback(OnHeroSourceAttackStateFinished);
			}
		}
	}

	private void OnHeroSourceAttackStateFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (spell.GetActiveState() == SpellStateType.NONE)
		{
			spell.RemoveStateFinishedCallback(OnHeroSourceAttackStateFinished);
			FinishHeroAttack();
		}
	}

	private void OnStaticHeroSourceAttackStateFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (spell.GetActiveState() == SpellStateType.NONE)
		{
			spell.RemoveStateFinishedCallback(OnStaticHeroSourceAttackStateFinished);
			OnHeroMoveBackFinished();
		}
	}

	private void FinishHeroAttack()
	{
		Log.Gameplay.PrintDebug("AttackSpellController -- FinishHeroAttack");
		Card sourceCard = GetSource();
		Entity sourceEntity = sourceCard.GetEntity();
		PlayWindfuryReminderIfPossible(sourceEntity, sourceCard);
		FinishEverything();
	}

	private IEnumerator EnsureFinisherCompletes(Spell spell)
	{
		yield return MAX_FINISHER_DURATION;
		m_finisherTrackingCoroutine = null;
		Log.Spells.PrintError("Finisher spell " + spell.gameObject.name + " did not terminate and was killed to prevent game hang. Run the finisher in the authoring scene to diagnose potential problems.");
		spell.ReleaseSpell();
	}

	private void OnBattlegroundsFinisherFinished(Spell spell, object favoriteFinisherRecordObject)
	{
		if (m_finisherTrackingCoroutine != null)
		{
			StopCoroutine(m_finisherTrackingCoroutine);
			m_finisherTrackingCoroutine = null;
		}
		Card sourceCard = GetSource();
		Card targetCard = GetTarget();
		FinisherGameplaySettings finisherSettings = (FinisherGameplaySettings)favoriteFinisherRecordObject;
		ActivateSpellForDestroyedHero(finisherSettings);
		ActivateSpellDeathState();
		OnFinishedTaskList();
		if (finisherSettings.ShowImpactEffects)
		{
			ActivateImpactEffects(sourceCard, targetCard);
		}
		spell.RemoveAllTargets();
		BaconBoard baconBoard = BaconBoard.Get();
		if (baconBoard != null)
		{
			baconBoard.FriendlyPlayerFinisherCalled();
		}
		StartCoroutine(WaitThenDestroySpellAndFinish(finisherSettings));
	}

	private void ActivateSpellDeathState(Card source = null)
	{
		Card sourceCard = source;
		if (sourceCard == null)
		{
			sourceCard = GetSource();
		}
		if (sourceCard != null && !sourceCard.ShouldShowImmuneVisuals() && (sourceCard.GetEntity() == null || !sourceCard.GetEntity().HasTag(GAME_TAG.IMMUNE_WHILE_ATTACKING) || m_attackType != AttackType.PROPOSED))
		{
			sourceCard.GetActor().ActivateSpellDeathState(SpellType.IMMUNE);
		}
	}

	private IEnumerator WaitThenDestroySpellAndFinish(FinisherGameplaySettings finisherGameplaySettings)
	{
		yield return new WaitForSeconds(10f);
		SpellUtils.PurgeSpell(m_sourceAttackSpell);
		finisherGameplaySettings.Dispose();
		OnFinished();
	}

	private void OnMinionSourceAttackStateFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (spell.GetActiveState() == SpellStateType.NONE)
		{
			spell.RemoveStateFinishedCallback(OnMinionSourceAttackStateFinished);
			FinishAttackSpellController();
		}
	}

	private void FinishAttackSpellController()
	{
		Log.Gameplay.PrintDebug("AttackSpellController -- FinishAttackSpellController");
		Card sourceCard = GetSource();
		Entity sourceEntity = sourceCard.GetEntity();
		sourceCard.EnableAttacking(enable: false);
		if (!CanPlayWindfuryReminder(sourceEntity, sourceCard))
		{
			FinishEverything();
			return;
		}
		OnFinishedTaskList();
		ActivateSpellDeathState(sourceCard);
		StartCoroutine(WaitThenPlayWindfuryReminder(sourceEntity, sourceCard));
	}

	private void FinishEverything()
	{
		Log.Gameplay.PrintDebug("AttackSpellController -- FinishEverything");
		ActivateSpellDeathState();
		OnFinishedTaskList();
		OnFinished();
	}

	private IEnumerator WaitThenPlayWindfuryReminder(Entity entity, Card card)
	{
		yield return new WaitForSeconds(1.2f);
		PlayWindfuryReminderIfPossible(entity, card);
		OnFinished();
	}

	private bool CanPlayWindfuryReminder(Entity entity, Card card)
	{
		if (!entity.HasWindfury())
		{
			return false;
		}
		if (entity.IsExhausted())
		{
			return false;
		}
		if (entity.GetZone() != TAG_ZONE.PLAY)
		{
			return false;
		}
		if (!entity.GetController().IsCurrentPlayer())
		{
			return false;
		}
		if (card.GetActorSpell(SpellType.WINDFURY_BURST) == null)
		{
			return false;
		}
		return true;
	}

	private void PlayWindfuryReminderIfPossible(Entity entity, Card card)
	{
		if (CanPlayWindfuryReminder(entity, card))
		{
			card.ActivateActorSpell(SpellType.WINDFURY_BURST);
		}
	}

	private void MoveSourceToTarget(Card sourceCard, Entity sourceEntity, Vector3 impactPos)
	{
		Vector3 impactOffset = ComputeImpactOffset(sourceCard, impactPos);
		Vector3 sourceCardDestinationPos = impactPos + impactOffset;
		float sourceMovementDuration = 0f;
		iTween.EaseType sourceEaseType = iTween.EaseType.linear;
		if (sourceEntity.IsHero())
		{
			sourceMovementDuration = m_HeroInfo.m_MoveToTargetDuration;
			sourceEaseType = m_HeroInfo.m_MoveToTargetEaseType;
		}
		else
		{
			sourceMovementDuration = m_AllyInfo.m_MoveToTargetDuration;
			sourceEaseType = m_AllyInfo.m_MoveToTargetEaseType;
		}
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("position", sourceCardDestinationPos);
		args.Add("time", sourceMovementDuration);
		args.Add("easetype", sourceEaseType);
		args.Add("oncomplete", "OnMoveToTargetFinished");
		args.Add("oncompletetarget", base.gameObject);
		iTween.MoveTo(sourceCard.gameObject, args);
	}

	private void OrientSourceHeroToTarget(Card sourceCard)
	{
		m_sourceFacing = sourceCard.transform.forward;
		Vector3 sourceToTargetLocal = sourceCard.transform.InverseTransformDirection(m_sourceToTarget);
		if (Vector3.Dot(m_sourceFacing, sourceToTargetLocal) < 0f)
		{
			sourceToTargetLocal = -sourceToTargetLocal;
		}
		Quaternion orientation = Quaternion.LookRotation(sourceToTargetLocal);
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("rotation", orientation.eulerAngles);
		args.Add("time", m_HeroInfo.m_OrientToTargetDuration);
		args.Add("easetype", m_HeroInfo.m_OrientToTargetEaseType);
		iTween.RotateTo(sourceCard.gameObject, args);
	}

	private void MoveTargetToSource(Card targetCard, Entity sourceEntity, Vector3 impactPos)
	{
		float targetMovementDuration = 0f;
		iTween.EaseType targetEaseType = iTween.EaseType.linear;
		if (sourceEntity.IsHero())
		{
			targetMovementDuration = m_HeroInfo.m_MoveToTargetDuration;
			targetEaseType = m_HeroInfo.m_MoveToTargetEaseType;
		}
		else
		{
			targetMovementDuration = m_AllyInfo.m_MoveToTargetDuration;
			targetEaseType = m_AllyInfo.m_MoveToTargetEaseType;
		}
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("position", impactPos);
		args.Add("time", targetMovementDuration);
		args.Add("easetype", targetEaseType);
		iTween.MoveTo(targetCard.gameObject, args);
	}

	private Vector3 ComputeImpactPos()
	{
		float attackTypeScalar = 1f;
		if (m_attackType == AttackType.PROPOSED)
		{
			attackTypeScalar = 0.5f;
		}
		Vector3 sourceToImpact = attackTypeScalar * m_ImpactStagingPoint * m_sourceToTarget;
		return m_sourcePos + sourceToImpact;
	}

	private Vector3 ComputeImpactOffset(Card sourceCard, Vector3 impactPos)
	{
		if (Mathf.Approximately(m_SourceImpactOffset, 0.5f))
		{
			return Vector3.zero;
		}
		if (sourceCard.GetActor().GetMeshRenderer() == null)
		{
			return Vector3.zero;
		}
		Bounds sourceBounds = sourceCard.GetActor().GetMeshRenderer().bounds;
		sourceBounds.center = m_sourcePos;
		Ray ray = new Ray(impactPos, sourceBounds.center - impactPos);
		if (!sourceBounds.IntersectRay(ray, out var rayHitDistance))
		{
			return Vector3.zero;
		}
		Vector3 hitLineStart = ray.origin + rayHitDistance * ray.direction;
		Vector3 hitStartToEnd = 2f * sourceBounds.center - hitLineStart - hitLineStart;
		return 0.5f * hitStartToEnd - m_SourceImpactOffset * hitStartToEnd;
	}

	private void ActivateImpactEffects(Card sourceCard, Card targetCard)
	{
		string impactSpellPrefabName = DetermineImpactSpellPrefab(sourceCard, targetCard);
		if (!string.IsNullOrEmpty(impactSpellPrefabName))
		{
			Spell spell = SpellManager.Get().GetSpell(impactSpellPrefabName);
			spell.transform.parent = null;
			spell.SetSource(sourceCard.gameObject);
			spell.AddTarget(targetCard.gameObject);
			Vector3 position = targetCard.transform.position;
			spell.SetPosition(position);
			Quaternion rotation = Quaternion.LookRotation(m_sourceToTarget);
			spell.SetOrientation(rotation);
			spell.AddStateFinishedCallback(OnImpactSpellStateFinished);
			spell.Activate();
			BaconBoard baconBoard = BaconBoard.Get();
			if (baconBoard != null)
			{
				baconBoard.CheckForHeroHeavyHitBoardEffects(sourceCard, targetCard);
			}
		}
	}

	private string DetermineImpactSpellPrefab(Card sourceCard, Card targetCard)
	{
		int attack = sourceCard.GetEntity().GetATK();
		SpellHandleValueRange spellToUse = SpellUtils.GetAppropriateElementAccordingToRanges((WasAttackCriticalHit(sourceCard, targetCard) && m_CriticalImpactDefHandles != null && m_CriticalImpactDefHandles.Length != 0) ? m_CriticalImpactDefHandles : m_ImpactDefHandles, (SpellHandleValueRange x) => x.m_range, attack);
		if (spellToUse != null && !string.IsNullOrEmpty(spellToUse.m_spellPrefabName))
		{
			return spellToUse.m_spellPrefabName;
		}
		return m_DefaultImpactSpellPrefabHandle;
	}

	private bool WasAttackCriticalHit(Card sourceCard, Card targetCard)
	{
		bool wasCrit = false;
		if (sourceCard == null || targetCard == null)
		{
			return wasCrit;
		}
		Player.Side sourceSide = sourceCard.GetControllerSide();
		Player.Side targetSide = targetCard.GetControllerSide();
		if (sourceSide != 0 && targetSide != 0)
		{
			wasCrit = sourceSide != targetSide && sourceCard.GetEntity().IsLettuceMercenary() && sourceCard.GetEntity().IsMyLettuceRoleStrongAgainst(targetCard.GetEntity());
		}
		return wasCrit;
	}

	private void OnImpactSpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (spell.GetActiveState() == SpellStateType.NONE)
		{
			SpellManager.Get().ReleaseSpell(spell);
		}
	}

	protected override float GetLostFrameTimeCatchUpSeconds()
	{
		Card sourceCard = GetSource();
		if (sourceCard != null && sourceCard.GetEntity() != null && sourceCard.GetEntity().IsHero())
		{
			return 0f;
		}
		Card targetCard = GetTarget();
		if (targetCard != null && targetCard.GetEntity() != null && targetCard.GetEntity().IsHero())
		{
			return 0f;
		}
		return 0.8f;
	}

	protected override void OnFinishedTaskList()
	{
		Log.Gameplay.PrintDebug("AttackSpellController -- OnFinishedTaskList");
		if (m_attackType != AttackType.PROPOSED)
		{
			Card sourceCard = GetSource();
			if (sourceCard != null && sourceCard.GetEntity() != null)
			{
				sourceCard.SetDoNotSort(on: false);
				if (!sourceCard.GetEntity().IsHero())
				{
					Zone sourceZone = sourceCard.GetZone();
					if (sourceZone != null)
					{
						sourceZone.UpdateLayout();
						if (m_sourceAttackSpell == null)
						{
							bool isSourceFriendly = sourceZone.m_Side == Player.Side.FRIENDLY;
							m_sourceAttackSpell = GetSourceAttackSpell(sourceCard, isSourceFriendly);
						}
					}
					if (m_sourceAttackSpell != null && (m_sourceAttackSpell.GetActiveState() == SpellStateType.BIRTH || m_sourceAttackSpell.GetActiveState() == SpellStateType.IDLE || m_sourceAttackSpell.GetActiveState() == SpellStateType.ACTION))
					{
						CancelAttackSpell(sourceCard.GetEntity(), m_sourceAttackSpell);
					}
				}
			}
		}
		base.OnFinishedTaskList();
	}

	private void CancelAttackSpell(Entity sourceEntity, Spell attackSpell)
	{
		if (!(attackSpell == null))
		{
			if (sourceEntity == null)
			{
				attackSpell.ActivateState(SpellStateType.DEATH);
			}
			else if (sourceEntity.IsHero())
			{
				attackSpell.ActivateState(SpellStateType.CANCEL);
			}
			else
			{
				attackSpell.ActivateState(SpellStateType.DEATH);
			}
		}
	}

	private Spell GetSourceAttackSpell(Card sourceCard, bool isSourceFriendly)
	{
		Log.Gameplay.PrintDebug("AttackSpellController -- GetSourceAttackSpell");
		try
		{
			if (!GameState.Get().GetGameEntity().HasTag(GAME_TAG.HIGHLIGHT_ATTACKING_MINION_DURING_COMBAT))
			{
				if (isSourceFriendly)
				{
					return sourceCard.GetActorSpell(SpellType.FRIENDLY_ATTACK);
				}
				return sourceCard.GetActorSpell(SpellType.OPPONENT_ATTACK);
			}
			Spell autoAttackHighlightSpell = sourceCard.GetActorSpell(SpellType.AUTO_ATTACK_WITH_HIGHLIGHT);
			if (autoAttackHighlightSpell != null)
			{
				return autoAttackHighlightSpell;
			}
		}
		catch (Exception ex)
		{
			string message = "Uncaught exception while looking for Attack Spell in GetSourceAttackSpell ";
			message = ((ex == null) ? (message + ", but no exception was captured.") : (message + $"\n{ex.GetType()}: {ex.Message}\n{ex.StackTrace}"));
			Log.Gameplay.PrintError(message);
		}
		return null;
	}

	private Spell InstantiateFinisherSpell(GameObject sourceObject, string spellPrefabName)
	{
		Log.Gameplay.PrintDebug("AttackSpellController -- InstantiateFinisherSpell");
		if (string.IsNullOrEmpty(spellPrefabName))
		{
			return null;
		}
		Spell spell = null;
		try
		{
			spell = SpellManager.Get().GetSpell(spellPrefabName);
		}
		catch (Exception ex)
		{
			string message = "Uncaught exception while looking for a Spell in InstantiateFinisherSpell ";
			message = ((ex == null) ? (message + ", but no exception was captured.") : (message + $"\n{ex.GetType()}: {ex.Message}\n{ex.StackTrace}"));
			Log.Gameplay.PrintError(message);
			spell = null;
		}
		if (spell == null)
		{
			return null;
		}
		TransformUtil.AttachAndPreserveLocalTransform(spell.transform, sourceObject.transform);
		return spell;
	}

	private string GetSpellPath(Entity sourceEntity, Entity targetEntity, bool opponentFinisher, FinisherGameplaySettings finisherSettings)
	{
		if (sourceEntity.GetATK() >= targetEntity.GetCurrentDefense())
		{
			if (GameState.Get().CountPlayersAlive() == 1)
			{
				if (opponentFinisher && !string.IsNullOrEmpty(finisherSettings.FirstPlaceVictoryOpponentPrefab))
				{
					return finisherSettings.FirstPlaceVictoryOpponentPrefab;
				}
				if (!string.IsNullOrEmpty(finisherSettings.FirstPlaceVictoryPrefab))
				{
					return finisherSettings.FirstPlaceVictoryPrefab;
				}
			}
			if (opponentFinisher && !string.IsNullOrEmpty(finisherSettings.LethalOpponentPrefab))
			{
				return finisherSettings.LethalOpponentPrefab;
			}
			if (!string.IsNullOrEmpty(finisherSettings.LethalPrefab))
			{
				return finisherSettings.LethalPrefab;
			}
		}
		if (sourceEntity.GetATK() >= 15)
		{
			if (opponentFinisher)
			{
				if (string.IsNullOrEmpty(finisherSettings.LargeOpponentPrefab))
				{
					return finisherSettings.SmallOpponentPrefab;
				}
				return finisherSettings.LargeOpponentPrefab;
			}
			if (string.IsNullOrEmpty(finisherSettings.LargePrefab))
			{
				return finisherSettings.SmallPrefab;
			}
			return finisherSettings.LargePrefab;
		}
		if (opponentFinisher)
		{
			return finisherSettings.SmallOpponentPrefab;
		}
		return finisherSettings.SmallPrefab;
	}

	private void ActivateSpellForDestroyedHero(FinisherGameplaySettings finisherSettings)
	{
		if (finisherSettings.IgnoreDestroyPrefabs)
		{
			return;
		}
		Entity friendlyHero = GameState.Get().GetFriendlySidePlayer().GetHero();
		Entity opposingHero = GameState.Get().GetOpposingSidePlayer().GetHero();
		if (friendlyHero.GetATK() >= opposingHero.GetCurrentDefense())
		{
			bool defaultDestroy = false;
			string spellPrefabName;
			if (!string.IsNullOrEmpty(finisherSettings.FirstPlaceVictoryDestroyOpponentPrefab) && GameState.Get().CountPlayersAlive() == 1)
			{
				spellPrefabName = finisherSettings.FirstPlaceVictoryDestroyOpponentPrefab;
			}
			else if (!string.IsNullOrEmpty(finisherSettings.DestroyOpponentPrefab))
			{
				spellPrefabName = finisherSettings.DestroyOpponentPrefab;
			}
			else
			{
				spellPrefabName = "Bacon_EndRound_HeroImpact.prefab:34d052b6989dcea4c8b7d22adcb31368";
				defaultDestroy = true;
			}
			Spell spell = SpellManager.Get().GetSpell(spellPrefabName);
			if (spell == null)
			{
				Debug.LogError("Error [AttackSpellController] ActivateSpellForDestroyedHero spell could not be found for " + spellPrefabName);
				return;
			}
			spell.AddFinishedCallback(OnFinishedCallback);
			GameObject sourceObject = friendlyHero.GetCard().gameObject;
			GameObject targetObject = opposingHero.GetCard().gameObject;
			spell.SetSource(defaultDestroy ? targetObject : sourceObject);
			spell.AddTarget(defaultDestroy ? sourceObject : targetObject);
			spell.transform.parent = targetObject.transform;
			spell.Activate();
		}
	}

	private void OnFinishedCallback(Spell spell, object userData)
	{
		if (spell != null)
		{
			UnityEngine.Object.Destroy(spell, 5f);
		}
	}
}
