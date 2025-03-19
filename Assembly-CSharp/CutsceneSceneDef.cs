using System;
using System.Collections.Generic;
using Blizzard.T5.Game.Spells;
using UnityEngine;

[CustomEditClass]
public class CutsceneSceneDef : MonoBehaviour
{
	public delegate void OnSceneDefDestroyed();

	public enum ActionType
	{
		NONE,
		SUMMON,
		ATTACK,
		SPELL,
		HERO_POWER,
		EMOTE,
		RESET
	}

	public enum CardType
	{
		NONE,
		HERO,
		MINION,
		HERO_POWER,
		WEAPON,
		ALTERNATE_FORM
	}

	[Serializable]
	public class CutsceneActionTarget
	{
		public CardType TargetCardType;

		public Player.Side TargetSide;

		[CustomEditField(HidePredicate = "ShouldHideCustomSocketInSpell")]
		public string CustomSocketInSpell;

		[CustomEditField(HidePredicate = "ShouldHideCustomSocketInSpell")]
		public string CustomSoundSpell;
	}

	[Serializable]
	public class CutsceneActionRequest
	{
		[CustomEditField]
		public ActionType ActionType;

		[CustomEditField]
		public bool ResetAfterPlay;

		[CustomEditField(T = EditType.SPELL, HidePredicate = "ShouldHideExplicitSpellOption")]
		public SpellBase ActionCustomSpell;

		[CustomEditField(HidePredicate = "ShouldHideSpellType")]
		public SpellType SpellType;

		[CustomEditField(HidePredicate = "ShouldHideEmoteType")]
		public EmoteType EmoteType;

		[CustomEditField(HidePredicate = "ShouldHideSourceCard")]
		public CutsceneActionTarget SourceCard;

		[CustomEditField(HidePredicate = "ShouldHideCornerSpell")]
		public CornerReplacementSpellType FriendlyCornerSpell;

		[CustomEditField(HidePredicate = "ShouldHideTargetTypes")]
		public CutsceneActionTarget TargetCard;

		[CustomEditField(T = EditType.TEXT_AREA, HidePredicate = "ShouldHideCaption")]
		public string CaptionLocalizedString;

		public bool HasStartDelayOverride;

		[CustomEditField(Parent = "HasStartDelayOverride", HidePredicate = "ShouldHideStartDelayOverride")]
		public float StartDelayOverrideSeconds = -1f;

		public bool HasEndDelayOverride;

		[CustomEditField(Parent = "HasEndDelayOverride", HidePredicate = "ShouldHideEndDelayOverride")]
		public float EndDelayOverrideSeconds = -1f;

		public bool HasTimeOutOverride;

		[CustomEditField(Parent = "HasTimeOutOverride", HidePredicate = "ShouldHideTimeoutOverride")]
		public float TimeoutOverrideSeconds = -1f;

		public bool HasCustomSpell()
		{
			if (SpellType == SpellType.NONE)
			{
				return ActionCustomSpell != null;
			}
			return false;
		}
	}

	[Serializable]
	public class CutsceneSetupData
	{
		[CustomEditField(T = EditType.GAME_OBJECT)]
		public string Board;

		[CustomEditField(Sections = "Hero Setup")]
		public string FriendlyHeroCardId = "HERO_08";

		[CustomEditField(Sections = "Hero Setup")]
		public string OpponentHeroCardId = "HERO_08";

		[CustomEditField(Sections = "Hero Setup")]
		public bool FriendlyHeroPowerEnabled = true;

		[CustomEditField(Sections = "Hero Setup")]
		public Vector3 FriendlyHeroScale = new Vector3(1f, 1f, 1f);

		[CustomEditField(Sections = "Corner Spell Replacement")]
		public CornerReplacementSpellType FriendlyCornerReplacement;

		[CustomEditField(Sections = "Corner Spell Replacement")]
		public CornerReplacementSpellType OpposingCornerReplacement;

		[CustomEditField(Sections = "Alternate Hero")]
		public bool HasAlternateForm;

		[CustomEditField(HidePredicate = "ShouldShowAlternateHeroCardId", Sections = "Alternate Hero")]
		public string AlternateHeroCardId;

		[CustomEditField(Sections = "Minion Setup")]
		public string MinionCardId = "LOEA10_3";

		[CustomEditField(Range = "0-7", Sections = "Minion Setup")]
		public int FriendlyMinionCount;

		[CustomEditField(Range = "0-7", Sections = "Minion Setup")]
		public int OpponentMinionCount;
	}

	[CustomEditField(Sections = "Scene Setup")]
	public CutsceneSetupData SetupData;

	[CustomEditField(Sections = "Actions", ListSortable = true)]
	public List<CutsceneActionRequest> Actions;

	public event OnSceneDefDestroyed SceneDefDestroyed;

	private void OnDestroy()
	{
		this.SceneDefDestroyed?.Invoke();
	}
}
