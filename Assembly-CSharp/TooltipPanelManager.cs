using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hearthstone.DataModels;
using PegasusShared;
using UnityEngine;

public class TooltipPanelManager : MonoBehaviour
{
	public struct TooltipPanelData
	{
		public string m_title;

		public string m_description;
	}

	public enum Orientation
	{
		RightTop,
		RightBottom,
		LeftMiddle,
		LeftTop
	}

	public enum TooltipBoneSource
	{
		INVALID,
		TOP_RIGHT,
		TOP_LEFT,
		BOTTOM_RIGHT,
		BOTTOM_LEFT,
		BOTTOM_MIDDLE_GOING_RIGHT,
		BOTTOM_MIDDLE_GOING_LEFT
	}

	private class TooltipPanelCreationArgs
	{
		public GameObject actorMeshRoot;

		public GameObject actorRoot;

		public Card card;

		public TooltipBoneSource boneSource;

		public bool inHand;

		public bool isHeroPower;

		public bool isCoinBasedHeroBuddy;

		public bool isLettuceAbility;

		public bool isMercenary;

		public bool isHistory;

		public Vector3? overrideOffset;
	}

	private enum MulliganDisplay_RelativeHandPosition
	{
		INVALID,
		LEFT,
		RIGHT,
		MIDDLE
	}

	private struct KeywordPanelEntityInfo
	{
		public EntityBase MainEntityBase { get; set; }

		public List<EntityBase> AdditionalEntityBases { get; set; }
	}

	public TooltipPanel m_tooltipPanelPrefab;

	public SignatureTooltipPanel m_signaturePanelPrefab;

	private static TooltipPanelManager s_instance;

	private Pool<TooltipPanel> m_tooltipPanelPool = new Pool<TooltipPanel>();

	private List<TooltipPanel> m_tooltipPanels = new List<TooltipPanel>();

	private SignatureTooltipPanel m_signaturePanel;

	private Actor m_actor;

	private Card m_card;

	private float m_scaleToUse;

	private const float FADE_IN_TIME = 0.125f;

	private const float DELAY_BEFORE_FADE_IN = 0.4f;

	private CancellationTokenSource m_panelTokenSource;

	private List<KeywordTextDbfRecord> m_sortedKeywordOrderRecords;

	private static readonly GAME_TAG[] spellpowerTags = new GAME_TAG[9]
	{
		GAME_TAG.SPELLPOWER,
		GAME_TAG.SPELLPOWER_ARCANE,
		GAME_TAG.SPELLPOWER_FIRE,
		GAME_TAG.SPELLPOWER_FROST,
		GAME_TAG.SPELLPOWER_NATURE,
		GAME_TAG.SPELLPOWER_HOLY,
		GAME_TAG.SPELLPOWER_SHADOW,
		GAME_TAG.SPELLPOWER_FEL,
		GAME_TAG.SPELLPOWER_PHYSICAL
	};

	private void Awake()
	{
		s_instance = this;
		m_scaleToUse = TooltipPanel.GAMEPLAY_SCALE;
		m_tooltipPanelPool.SetCreateItemCallback(CreateKeywordPanel);
		m_tooltipPanelPool.SetDestroyItemCallback(DestroyKeywordPanel);
		m_tooltipPanelPool.SetExtensionCount(1);
		if (SceneMgr.Get() != null)
		{
			SceneMgr.Get().RegisterSceneUnloadedEvent(OnSceneUnloaded);
		}
		if (m_panelTokenSource == null)
		{
			m_panelTokenSource = new CancellationTokenSource();
		}
	}

	private void OnDestroy()
	{
		m_tooltipPanelPool.ReleaseAll();
		m_tooltipPanelPool.Clear();
		s_instance = null;
		m_panelTokenSource?.Cancel();
		m_panelTokenSource?.Dispose();
	}

	public static TooltipPanelManager Get()
	{
		return s_instance;
	}

	public void UpdateKeywordPanelsPosition(Card card, TooltipBoneSource boneSource)
	{
		Actor actor = card.GetActor();
		if (actor == null || actor.GetMeshRenderer() == null)
		{
			return;
		}
		bool inHand = card.GetZone() is ZoneHand;
		bool isHeroPower = card.GetEntity() != null && card.GetEntity().IsHeroPower();
		bool isCoinBasedHeroBuddy = card.GetEntity() != null && card.GetEntity().IsCoinBasedHeroBuddy();
		bool isLettuceAbility = card.GetEntity().IsLettuceAbility();
		bool isMercenary = card.GetEntity().HasTag(GAME_TAG.LETTUCE_MERCENARY);
		TooltipPanelCreationArgs tooltipArgs = new TooltipPanelCreationArgs
		{
			actorMeshRoot = actor.GetMeshRenderer().gameObject,
			actorRoot = actor.gameObject,
			card = card,
			boneSource = boneSource,
			inHand = inHand,
			isHeroPower = isHeroPower,
			isCoinBasedHeroBuddy = isCoinBasedHeroBuddy,
			isLettuceAbility = isLettuceAbility,
			isMercenary = isMercenary
		};
		if (CanUseBonesForPlacingTooltips(tooltipArgs))
		{
			try
			{
				PositionPanelsWithBones(tooltipArgs, m_panelTokenSource.Token);
				return;
			}
			catch (Exception e)
			{
				PrintUncaughtTooltipException(e, "gameplay");
				return;
			}
		}
		PrintUnableToDisplayTooltipError("gameplay");
	}

	public void UpdateKeywordHelp(Card card, Actor actor, TooltipBoneSource boneSource = TooltipBoneSource.TOP_RIGHT, float? overrideScale = null, Vector3? overrideOffset = null)
	{
		m_card = card;
		UpdateKeywordHelp(card.GetEntity(), actor, boneSource, overrideScale, overrideOffset);
	}

	private void GetDesiredEntityBaseForEntity(Entity entity, bool isHistoryTile, out EntityBase mainEntityBaseForKeyword, out List<EntityBase> additionalEntityBasesForKeyword)
	{
		mainEntityBaseForKeyword = null;
		additionalEntityBasesForKeyword = null;
		if (entity == null || GameState.Get().GetGameEntity().GetEntityBaseForKeywordTooltips(entity, isHistoryTile, out mainEntityBaseForKeyword, out additionalEntityBasesForKeyword))
		{
			return;
		}
		int alternateMouseOverCardID = entity.GetTag(GAME_TAG.ALTERNATE_MOUSE_OVER_CARD);
		if (alternateMouseOverCardID != 0)
		{
			EntityDef alternateCardEntityDef = DefLoader.Get().GetEntityDef(alternateMouseOverCardID);
			if (alternateCardEntityDef != null)
			{
				mainEntityBaseForKeyword = alternateCardEntityDef;
				return;
			}
			Log.Gameplay.PrintError("TooltipPanelManager.GetDesiredEntityBaseForEntity(): Unable to load EntityDef for card ID {0}.", alternateMouseOverCardID);
		}
		mainEntityBaseForKeyword = entity;
	}

	public void UpdateKeywordHelp(Entity entity, Actor actor, TooltipBoneSource boneSource, float? overrideScale = null, Vector3? overrideOffset = null)
	{
		m_card = entity.GetCard();
		if (GameState.Get().GetBooleanGameOption(GameEntityOption.SHOW_CRAZY_KEYWORD_TOOLTIP))
		{
			if (TutorialKeywordManager.Get() != null)
			{
				bool showOnRight = boneSource == TooltipBoneSource.TOP_RIGHT;
				TutorialKeywordManager.Get().UpdateKeywordHelp(entity, actor, showOnRight, overrideScale);
			}
			return;
		}
		bool inHand = m_card.GetZone() is ZoneHand || m_card.GetZone() is ZoneTeammateHand;
		bool isHeroPower = entity.IsHeroPower();
		bool isCoinBasedHeroBuddy = entity.IsCoinBasedHeroBuddy();
		bool isLettuceAbility = entity.IsLettuceAbility();
		if (overrideScale.HasValue)
		{
			m_scaleToUse = overrideScale.Value;
		}
		else if (inHand)
		{
			m_scaleToUse = TooltipPanel.HAND_SCALE;
		}
		else
		{
			m_scaleToUse = TooltipPanel.GAMEPLAY_SCALE;
		}
		PrepareToUpdateKeywordHelp(actor);
		List<TooltipPanelData> overwriteKeywordPanels = GameState.Get().GetGameEntity().GetOverwriteKeywordHelpPanelDisplay(entity);
		if (overwriteKeywordPanels == null)
		{
			string[] customKeywordData = GameState.Get().GetGameEntity().NotifyOfKeywordHelpPanelDisplay(entity);
			if (customKeywordData != null)
			{
				SetupTooltipPanel(customKeywordData[0], customKeywordData[1]);
			}
			GetDesiredEntityBaseForEntity(entity, isHistoryTile: false, out var entityBaseForKeyword, out var additionalEntityBaseForKeyword);
			SetUpPanels(entityBaseForKeyword, additionalEntityBaseForKeyword);
		}
		else
		{
			foreach (TooltipPanelData data in overwriteKeywordPanels)
			{
				SetupTooltipPanel(data.m_title, data.m_description);
			}
		}
		if (isLettuceAbility)
		{
			MercenariesAbilityTray tray = ZoneMgr.Get().GetLettuceZoneController().GetAbilityTray();
			if (tray != null)
			{
				boneSource = ((tray.GetTrayPositionOfAbility(entity.GetCard()) < 2) ? TooltipBoneSource.TOP_RIGHT : TooltipBoneSource.TOP_LEFT);
			}
		}
		TooltipPanelCreationArgs tooltipArgs = new TooltipPanelCreationArgs
		{
			actorMeshRoot = actor.GetMeshRenderer().gameObject,
			actorRoot = actor.gameObject,
			card = m_card,
			boneSource = boneSource,
			inHand = inHand,
			isHeroPower = isHeroPower,
			isCoinBasedHeroBuddy = isCoinBasedHeroBuddy,
			isLettuceAbility = isLettuceAbility,
			isMercenary = entity.HasTag(GAME_TAG.LETTUCE_MERCENARY),
			overrideOffset = overrideOffset
		};
		if (CanUseBonesForPlacingTooltips(tooltipArgs))
		{
			try
			{
				PositionPanelsWithBones(tooltipArgs, m_panelTokenSource.Token);
			}
			catch (Exception e)
			{
				PrintUncaughtTooltipException(e, "gameplay_hand");
			}
		}
		else
		{
			PrintUnableToDisplayTooltipError("gameplay_hand");
		}
		GameState.Get().GetGameEntity().NotifyOfHelpPanelDisplay(m_tooltipPanels.Count);
	}

	public void ResetCardForTooltips()
	{
		m_panelTokenSource.Cancel();
		m_card = null;
	}

	private bool CanUseBonesForPlacingTooltips(TooltipPanelCreationArgs args)
	{
		if (args == null)
		{
			return false;
		}
		if (args.actorMeshRoot == null || args.actorRoot == null)
		{
			return false;
		}
		if (args.boneSource == TooltipBoneSource.INVALID)
		{
			return false;
		}
		BigCardTooltipDisplayBones tooltipBones = args.actorRoot.gameObject.GetComponentInChildren<BigCardTooltipDisplayBones>();
		if (tooltipBones == null)
		{
			return false;
		}
		if (!tooltipBones.HasBonesForCurrentPlatform())
		{
			return false;
		}
		return true;
	}

	private void PrintUncaughtTooltipException(Exception e, string context)
	{
		if (string.IsNullOrEmpty(context))
		{
			context = "[Unknown context]";
		}
		string actorName = ((!(m_actor == null) && !string.IsNullOrEmpty(m_actor.name)) ? m_actor.name : "[Actor not loaded]");
		string message = "Uncaught exception showing tooltips from context \"" + context + "\" on actor \"" + actorName + "\"";
		message = ((e == null) ? (message + ", but no exception was captured.") : (message + $"\n{e.GetType()}: {e.Message}\n{e.StackTrace}"));
		GetCorrectLogger(context).PrintError(message);
	}

	private void PrintUnableToDisplayTooltipError(string context)
	{
		if (string.IsNullOrEmpty(context))
		{
			context = "[Unknown context]";
		}
		string actorName = ((!(m_actor == null) && !string.IsNullOrEmpty(m_actor.name)) ? m_actor.name : "[Actor not loaded]");
		string message = "Unable to display tooltips from actor \"" + actorName + "\" in context \"" + context + "\".";
		GetCorrectLogger(context).PrintError(message);
	}

	private Logger GetCorrectLogger(string context)
	{
		switch (context)
		{
		case "gameplay":
		case "history":
		case "gameplay_hand":
		case "discover":
		case "suboptions":
		case "mulligan":
			return Log.Gameplay;
		case "collection manager":
			return Log.CollectionManager;
		case "adventure":
			return Log.Adventures;
		case "deck helper":
			return Log.DeckHelper;
		case "arena":
			return Log.Arena;
		default:
			return Log.All;
		}
	}

	private int TooltipBones_ComputePanelsPerColumn_LettuceAbility()
	{
		if (!PlatformSettings.IsMobile())
		{
			return 3;
		}
		return 2;
	}

	private async UniTaskVoid PositionPanelsForGameWithBones_LettuceAbility(TooltipPanelCreationArgs args, CancellationToken token)
	{
		if (args == null || args.actorRoot == null || m_tooltipPanels.Count == 0)
		{
			return;
		}
		BigCardTooltipDisplayBones tooltipBones = args.actorRoot.GetComponentInChildren<BigCardTooltipDisplayBones>();
		if (tooltipBones == null || !tooltipBones.HasBonesForCurrentPlatform())
		{
			return;
		}
		switch (args.boneSource)
		{
		case TooltipBoneSource.TOP_RIGHT:
		case TooltipBoneSource.TOP_LEFT:
		case TooltipBoneSource.BOTTOM_RIGHT:
		case TooltipBoneSource.BOTTOM_LEFT:
		{
			TooltipBoneLayout boneLayout = tooltipBones.GetRigForCurrentPlatform();
			if (boneLayout == null || !boneLayout.HasAllBones())
			{
				break;
			}
			bool showOnRight = args.boneSource == TooltipBoneSource.TOP_RIGHT || args.boneSource == TooltipBoneSource.BOTTOM_RIGHT;
			GameObject topRowBone;
			GameObject bottomRowBone;
			if (showOnRight)
			{
				topRowBone = boneLayout.m_topRightTooltipBone;
				bottomRowBone = boneLayout.m_bottomRightTooltipBone;
			}
			else
			{
				topRowBone = boneLayout.m_topLeftTooltipBone;
				bottomRowBone = boneLayout.m_bottomLeftTooltipBone;
			}
			TooltipPanel curPanel = m_tooltipPanels[0];
			while (curPanel != null && !curPanel.Destroyed && !curPanel.IsTextRendered())
			{
				await UniTask.Yield(PlayerLoopTiming.Update, token);
			}
			if (curPanel == null || curPanel.gameObject == null || args.actorMeshRoot == null || curPanel.Destroyed)
			{
				break;
			}
			Vector3 panelCorner = new Vector3((!showOnRight) ? 1 : 0, 0f, 0f);
			if (args.overrideOffset.HasValue)
			{
				TransformUtil.SetPoint(curPanel, panelCorner, topRowBone, Vector3.zero, args.overrideOffset.Value);
			}
			else
			{
				TransformUtil.SetPoint(curPanel, panelCorner, topRowBone, Vector3.zero, Vector3.zero);
			}
			TooltipPanel lastTopRowPanel = curPanel;
			if (m_tooltipPanels.Count < 2)
			{
				break;
			}
			curPanel = m_tooltipPanels[1];
			while (curPanel != null && !curPanel.Destroyed && !curPanel.IsTextRendered())
			{
				await UniTask.Yield(PlayerLoopTiming.Update, token);
			}
			if (curPanel == null || curPanel.gameObject == null || args.actorMeshRoot == null || curPanel.Destroyed)
			{
				break;
			}
			Vector3 panelCorner2 = new Vector3((!showOnRight) ? 1 : 0, 0f, 1f);
			if (args.overrideOffset.HasValue)
			{
				TransformUtil.SetPoint(curPanel, panelCorner2, bottomRowBone, Vector3.zero, args.overrideOffset.Value);
			}
			else
			{
				TransformUtil.SetPoint(curPanel, panelCorner2, bottomRowBone, Vector3.zero, Vector3.zero);
			}
			TooltipPanel lastBottomRowPanel = curPanel;
			TooltipPanel prevPanel = curPanel;
			int panelsPerColumn = TooltipBones_ComputePanelsPerColumn_LettuceAbility();
			if (panelsPerColumn <= 0)
			{
				panelsPerColumn = 2;
			}
			int i = 2;
			while (i < m_tooltipPanels.Count)
			{
				curPanel = m_tooltipPanels[i];
				while (curPanel != null && !curPanel.Destroyed && !curPanel.IsTextRendered())
				{
					await UniTask.Yield(PlayerLoopTiming.Update, token);
				}
				if (curPanel == null || curPanel.gameObject == null || args.actorMeshRoot == null || curPanel.Destroyed)
				{
					break;
				}
				switch (i % panelsPerColumn)
				{
				case 0:
				{
					float prevPanelHorizontalScale2 = prevPanel.gameObject.transform.localScale.x;
					Vector3 manualOffset3 = Vector3.zero;
					Vector3 curAnchor3;
					Vector3 prevAnchor3;
					if (showOnRight)
					{
						curAnchor3 = new Vector3(0f, 0f, 0f);
						prevAnchor3 = new Vector3(1f, 0f, 0f);
						manualOffset3.x += boneLayout.m_manualHorizontalAdjustment * prevPanelHorizontalScale2;
					}
					else
					{
						curAnchor3 = new Vector3(1f, 0f, 0f);
						prevAnchor3 = new Vector3(0f, 0f, 0f);
						manualOffset3.x -= boneLayout.m_manualHorizontalAdjustment * prevPanelHorizontalScale2;
					}
					TransformUtil.SetPoint(curPanel, curAnchor3, lastTopRowPanel, prevAnchor3, manualOffset3);
					lastTopRowPanel = curPanel;
					break;
				}
				case 1:
				{
					float prevPanelHorizontalScale = prevPanel.gameObject.transform.localScale.x;
					Vector3 manualOffset2 = Vector3.zero;
					Vector3 curAnchor2;
					Vector3 prevAnchor2;
					if (showOnRight)
					{
						curAnchor2 = new Vector3(0f, 0f, 1f);
						prevAnchor2 = new Vector3(1f, 0f, 1f);
						manualOffset2.x += boneLayout.m_manualHorizontalAdjustment * prevPanelHorizontalScale;
					}
					else
					{
						curAnchor2 = new Vector3(1f, 0f, 1f);
						prevAnchor2 = new Vector3(0f, 0f, 1f);
						manualOffset2.x -= boneLayout.m_manualHorizontalAdjustment * prevPanelHorizontalScale;
					}
					TransformUtil.SetPoint(curPanel, curAnchor2, lastBottomRowPanel, prevAnchor2, manualOffset2);
					lastBottomRowPanel = curPanel;
					break;
				}
				default:
				{
					float prevPanelVerticalScale = prevPanel.gameObject.transform.localScale.z;
					Vector3 curAnchor = new Vector3(0f, 0f, 1f);
					Vector3 prevAnchor = new Vector3(0f, 0f, 0f);
					float finalOffsetZ = boneLayout.m_manualVerticalAdjustment * prevPanelVerticalScale;
					Vector3 manualOffset = new Vector3(0f, 0f, 0f - finalOffsetZ);
					TransformUtil.SetPoint(curPanel, curAnchor, prevPanel, prevAnchor, manualOffset);
					break;
				}
				}
				prevPanel = curPanel;
				int num = i + 1;
				i = num;
			}
			break;
		}
		}
	}

	private int TooltipBones_ComputePanelsPerColumn_NonLettuceAbility(Card card, TooltipBoneSource boneSource)
	{
		if (card == null)
		{
			if (!UniversalInputManager.UsePhoneUI)
			{
				return 4;
			}
			return 3;
		}
		int panelsPerColumn = ((!(card.GetZone() is ZonePlay)) ? 3 : ((!PlatformSettings.IsMobile()) ? 4 : 3));
		if (boneSource == TooltipBoneSource.BOTTOM_MIDDLE_GOING_LEFT || boneSource == TooltipBoneSource.BOTTOM_MIDDLE_GOING_RIGHT)
		{
			panelsPerColumn--;
		}
		return panelsPerColumn;
	}

	private async UniTaskVoid PositionPanelsForGameWithBones_NonLettuceAbility(TooltipPanelCreationArgs args, CancellationToken token)
	{
		if (args == null || args.actorRoot == null || m_tooltipPanels.Count == 0)
		{
			return;
		}
		BigCardTooltipDisplayBones tooltipBones = args.actorRoot.GetComponentInChildren<BigCardTooltipDisplayBones>();
		if (tooltipBones == null || !tooltipBones.HasBonesForCurrentPlatform())
		{
			return;
		}
		TooltipBoneLayout boneLayout = tooltipBones.GetRigForCurrentPlatform();
		if (boneLayout == null || !boneLayout.HasAllBones())
		{
			return;
		}
		GameObject bone;
		Vector3 currPanelCorner;
		switch (args.boneSource)
		{
		case TooltipBoneSource.BOTTOM_RIGHT:
			bone = boneLayout.m_bottomRightTooltipBone;
			currPanelCorner = new Vector3(0f, 0f, 0f);
			break;
		case TooltipBoneSource.BOTTOM_LEFT:
			bone = boneLayout.m_bottomLeftTooltipBone;
			currPanelCorner = new Vector3(1f, 0f, 0f);
			break;
		case TooltipBoneSource.TOP_RIGHT:
			bone = boneLayout.m_topRightTooltipBone;
			currPanelCorner = new Vector3(0f, 0f, 1f);
			break;
		case TooltipBoneSource.TOP_LEFT:
			bone = boneLayout.m_topLeftTooltipBone;
			currPanelCorner = new Vector3(1f, 0f, 1f);
			break;
		case TooltipBoneSource.BOTTOM_MIDDLE_GOING_RIGHT:
		case TooltipBoneSource.BOTTOM_MIDDLE_GOING_LEFT:
			bone = boneLayout.m_bottomMiddleTooltipBone;
			currPanelCorner = new Vector3(0.5f, 0f, 1f);
			break;
		case TooltipBoneSource.INVALID:
			return;
		default:
			Log.All.PrintError($"Unknown tooltip bone source value {args.boneSource} in {MethodBase.GetCurrentMethod().Name}.");
			return;
		}
		TooltipPanel curPanel = m_tooltipPanels[0];
		while (curPanel != null && !curPanel.Destroyed && !curPanel.IsTextRendered())
		{
			await UniTask.Yield(PlayerLoopTiming.Update, token);
		}
		if (curPanel == null || curPanel.gameObject == null || args.actorMeshRoot == null || curPanel.Destroyed)
		{
			return;
		}
		if (args.overrideOffset.HasValue)
		{
			TransformUtil.SetPoint(curPanel.gameObject, currPanelCorner, bone, Vector3.zero, args.overrideOffset.Value);
		}
		else
		{
			TransformUtil.SetPoint(curPanel.gameObject, currPanelCorner, bone, Vector3.zero, Vector3.zero);
		}
		TooltipPanel topColumnPanel = curPanel;
		TooltipPanel prevPanel = curPanel;
		int panelsPerColumn = TooltipBones_ComputePanelsPerColumn_NonLettuceAbility(args.card, args.boneSource);
		if (panelsPerColumn <= 0)
		{
			panelsPerColumn = 3;
		}
		int i = 1;
		while (i < m_tooltipPanels.Count)
		{
			curPanel = m_tooltipPanels[i];
			while (curPanel != null && !curPanel.Destroyed && !curPanel.IsTextRendered())
			{
				await UniTask.Yield(PlayerLoopTiming.Update, token);
			}
			if (curPanel == null || curPanel.gameObject == null || args.actorMeshRoot == null || curPanel.Destroyed)
			{
				break;
			}
			Vector3 manualPositionOffset = Vector3.zero;
			if (i % panelsPerColumn == 0)
			{
				float prevPanelXScale = prevPanel.gameObject.transform.localScale.x;
				Vector3 prevTopPanelAnchor;
				switch (args.boneSource)
				{
				case TooltipBoneSource.BOTTOM_RIGHT:
					currPanelCorner = new Vector3(0f, 0f, 0f);
					prevTopPanelAnchor = new Vector3(1f, 0f, 0f);
					manualPositionOffset.x += boneLayout.m_manualHorizontalAdjustment * prevPanelXScale;
					break;
				case TooltipBoneSource.BOTTOM_LEFT:
					currPanelCorner = new Vector3(1f, 0f, 0f);
					prevTopPanelAnchor = new Vector3(0f, 0f, 0f);
					manualPositionOffset.x -= boneLayout.m_manualHorizontalAdjustment * prevPanelXScale;
					break;
				case TooltipBoneSource.TOP_RIGHT:
				case TooltipBoneSource.BOTTOM_MIDDLE_GOING_RIGHT:
					currPanelCorner = new Vector3(0f, 0f, 1f);
					prevTopPanelAnchor = new Vector3(1f, 0f, 1f);
					manualPositionOffset.x += boneLayout.m_manualHorizontalAdjustment * prevPanelXScale;
					break;
				case TooltipBoneSource.TOP_LEFT:
				case TooltipBoneSource.BOTTOM_MIDDLE_GOING_LEFT:
					currPanelCorner = new Vector3(1f, 0f, 1f);
					prevTopPanelAnchor = new Vector3(0f, 0f, 1f);
					manualPositionOffset.x -= boneLayout.m_manualHorizontalAdjustment * prevPanelXScale;
					break;
				default:
					Log.All.PrintWarning($"Unknown layout mode {args.boneSource} in {args.boneSource}.");
					goto case TooltipBoneSource.TOP_RIGHT;
				}
				TransformUtil.SetPoint(curPanel.gameObject, currPanelCorner, topColumnPanel.gameObject, prevTopPanelAnchor, manualPositionOffset);
				topColumnPanel = curPanel;
			}
			else
			{
				float prevPanelZScale = prevPanel.gameObject.transform.localScale.z;
				manualPositionOffset.z -= boneLayout.m_manualVerticalAdjustment * prevPanelZScale;
				if (args.boneSource == TooltipBoneSource.BOTTOM_RIGHT || args.boneSource == TooltipBoneSource.BOTTOM_LEFT)
				{
					TransformUtil.SetPoint(curPanel.gameObject, Vector3.zero, prevPanel.gameObject, new Vector3(0f, 0f, 1f), -manualPositionOffset);
				}
				else
				{
					TransformUtil.SetPoint(curPanel.gameObject, new Vector3(0f, 0f, 1f), prevPanel.gameObject, Vector3.zero, manualPositionOffset);
				}
			}
			prevPanel = curPanel;
			int num = i + 1;
			i = num;
		}
	}

	private void PositionPanelsWithBones(TooltipPanelCreationArgs args, CancellationToken token)
	{
		if (args != null)
		{
			if (args.isLettuceAbility)
			{
				PositionPanelsForGameWithBones_LettuceAbility(args, token).Forget();
			}
			else
			{
				PositionPanelsForGameWithBones_NonLettuceAbility(args, token).Forget();
			}
		}
	}

	public List<TooltipPanel> GetCurrentTooltipPanels()
	{
		return m_tooltipPanels;
	}

	public void UpdateKeywordHelpForHistoryCard(Entity entity, Actor actor, UberText createdByText, TooltipBoneSource boneSource, bool signatureOnly = false)
	{
		m_card = entity.GetCard();
		m_scaleToUse = TooltipPanel.HISTORY_SCALE;
		PrepareToUpdateKeywordHelp(actor);
		string[] customKeywordData = GameState.Get().GetGameEntity().NotifyOfKeywordHelpPanelDisplay(entity);
		if (customKeywordData != null)
		{
			SetupTooltipPanel(customKeywordData[0], customKeywordData[1]);
		}
		GetDesiredEntityBaseForEntity(entity, isHistoryTile: true, out var entityBaseForKeyword, out var _);
		SetUpPanels(entityBaseForKeyword);
		if ((bool)UniversalInputManager.UsePhoneUI || signatureOnly)
		{
			m_tooltipPanels = m_tooltipPanels.FindAll((TooltipPanel panel) => panel is SignatureTooltipPanel);
		}
		TooltipPanelCreationArgs tooltipArgs = new TooltipPanelCreationArgs
		{
			actorMeshRoot = actor.GetMeshRenderer().gameObject,
			actorRoot = actor.gameObject,
			card = m_card,
			boneSource = boneSource,
			isHistory = true
		};
		if (CanUseBonesForPlacingTooltips(tooltipArgs))
		{
			try
			{
				PositionPanelsWithBones(tooltipArgs, m_panelTokenSource.Token);
				return;
			}
			catch (Exception e)
			{
				PrintUncaughtTooltipException(e, "history");
				return;
			}
		}
		PrintUnableToDisplayTooltipError("history");
	}

	private async UniTaskVoid PositionPanelsForHistory(Actor actor, UberText createdByText, CancellationToken token)
	{
		GameObject firstRelativeAnchor;
		if (createdByText.gameObject.activeSelf)
		{
			firstRelativeAnchor = createdByText.gameObject;
		}
		else
		{
			GameObject historyKeywordBone = actor.FindBone("HistoryKeywordBone");
			if (historyKeywordBone == null)
			{
				Error.AddDevWarning("Missing Bone", "Missing HistoryKeywordBone on {0}", actor);
				await UniTask.Yield(PlayerLoopTiming.Update, token);
			}
			firstRelativeAnchor = historyKeywordBone;
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_tooltipPanels = m_tooltipPanels.FindAll((TooltipPanel panel) => panel is SignatureTooltipPanel);
		}
		TooltipPanel prevPanel = null;
		bool showHorizontally = false;
		for (int i = 0; i < m_tooltipPanels.Count; i++)
		{
			TooltipPanel curPanel = m_tooltipPanels[i];
			while (curPanel != null && !curPanel.Destroyed && !curPanel.IsTextRendered())
			{
				await UniTask.Yield(PlayerLoopTiming.Update, token);
			}
			if (curPanel == null || curPanel.Destroyed)
			{
				continue;
			}
			if (prevPanel == null)
			{
				TransformUtil.SetPoint(curPanel.gameObject, new Vector3(0.5f, 0f, 1f), firstRelativeAnchor, new Vector3(0.5f, 0f, 0f));
			}
			else
			{
				float newZ = prevPanel.GetHeight() * 0.35f + curPanel.GetHeight() * 0.35f;
				if (prevPanel.transform.position.z - newZ < -8.3f)
				{
					showHorizontally = true;
				}
				if (showHorizontally)
				{
					TransformUtil.SetPoint(curPanel.gameObject, new Vector3(0f, 0f, 1f), prevPanel.gameObject, new Vector3(1f, 0f, 1f), Vector3.zero);
				}
				else
				{
					TransformUtil.SetPoint(curPanel.gameObject, new Vector3(0.5f, 0f, 1f), prevPanel.gameObject, new Vector3(0.5f, 0f, 0f), new Vector3(0f, 0f, 0.06094122f));
				}
			}
			prevPanel = curPanel;
		}
	}

	public void UpdateKeywordHelpForCollectionManager(EntityDef entityDef, Actor actor, Orientation orientation)
	{
		m_scaleToUse = TooltipPanel.COLLECTION_MANAGER_SCALE;
		PrepareToUpdateKeywordHelp(actor);
		SetUpPanels(entityDef);
		TooltipBoneSource boneSource = GetTooltipBoneSourceForOrientation(orientation);
		TooltipPanelCreationArgs tooltipArgs = new TooltipPanelCreationArgs
		{
			actorMeshRoot = actor.GetMeshRenderer().gameObject,
			actorRoot = actor.gameObject,
			boneSource = boneSource
		};
		if (CanUseBonesForPlacingTooltips(tooltipArgs))
		{
			try
			{
				PositionPanelsWithBones(tooltipArgs, m_panelTokenSource.Token);
				return;
			}
			catch (Exception e)
			{
				PrintUncaughtTooltipException(e, "collection manager");
				return;
			}
		}
		PrintUnableToDisplayTooltipError("collection manager");
	}

	public void UpdateTooltipForBigCard(EntityDef entityDef, Actor actor, Orientation orientation)
	{
		m_scaleToUse = TooltipPanel.BIG_CARD_SCALE;
		PrepareToUpdateKeywordHelp(actor);
		SetupSignatureInfoPanelIfNecessary(entityDef);
		SetupTooltipPanelForTitansFromRelatedCards(entityDef);
		TooltipBoneSource boneSource = GetTooltipBoneSourceForOrientation(orientation);
		TooltipPanelCreationArgs tooltipArgs = new TooltipPanelCreationArgs
		{
			actorMeshRoot = actor.GetMeshRenderer().gameObject,
			actorRoot = actor.gameObject,
			boneSource = boneSource
		};
		if (CanUseBonesForPlacingTooltips(tooltipArgs))
		{
			try
			{
				PositionPanelsWithBones(tooltipArgs, m_panelTokenSource.Token);
				return;
			}
			catch (Exception e)
			{
				PrintUncaughtTooltipException(e, "signature");
				return;
			}
		}
		PrintUnableToDisplayTooltipError("signature");
	}

	public void UpdateSignatureTooltipAtCustomTransform(EntityDef entityDef, Actor actor, Transform customTransform)
	{
		m_scaleToUse = TooltipPanel.COLLECTION_MANAGER_SCALE;
		PrepareToUpdateKeywordHelp(actor);
		SetupSignatureInfoPanelIfNecessary(entityDef);
		if (!(m_signaturePanel == null))
		{
			m_signaturePanel.transform.position = customTransform.position;
			m_signaturePanel.transform.localScale = customTransform.localScale;
		}
	}

	public SignatureTooltipPanel UpdateSignatureTooltipFromDataModelAtCustomTransform(CardDataModel cardDataModel, Transform customTransform, bool useCustomTransformScale = false)
	{
		if (customTransform == null)
		{
			return null;
		}
		m_scaleToUse = (useCustomTransformScale ? customTransform.localScale.x : ((float)TooltipPanel.POPUP_SCALE));
		HideKeywordHelp();
		m_actor = null;
		m_tooltipPanels.Clear();
		SetupSignatureInfoPanelFromDataModel(cardDataModel);
		if (m_signaturePanel == null)
		{
			return null;
		}
		m_signaturePanel.transform.position = customTransform.position;
		return m_signaturePanel;
	}

	public void UpdateGhostCardHelpForCollectionManager(Actor actor, GhostCard.Type ghostType, Orientation orientation)
	{
		m_scaleToUse = TooltipPanel.COLLECTION_MANAGER_SCALE;
		PrepareToUpdateGhostCardHelp(actor);
		string title = "";
		string desc = "";
		string descPostfix = (UniversalInputManager.Get().IsTouchMode() ? "_TOUCH" : "");
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		switch (ghostType)
		{
		case GhostCard.Type.NOT_VALID:
		{
			DeckRuleset deckRuleset = deck?.GetRuleset(null);
			EntityDef entityDef = actor.GetEntityDef();
			bool hasSetHelpStrings = false;
			if (deck != null && GameUtils.IsBanned(deck, entityDef))
			{
				title = GameStrings.Get("GLUE_GHOST_CARD_BANNED_TITLE");
				desc = GameStrings.Get("GLUE_GHOST_CARD_BANNED_DESCRIPTION" + descPostfix);
				hasSetHelpStrings = true;
			}
			else if (deck != null && GameUtils.IsCardBannedInWild(entityDef))
			{
				title = GameStrings.Get("GLUE_GHOST_CARD_BANNED_TITLE");
				desc = GameStrings.Get("GLUE_CARD_BANNED_IN_WILD_DESCRIPTION" + descPostfix);
				hasSetHelpStrings = true;
			}
			else if (deck != null && GameUtils.IsCardBannedInTwist(entityDef))
			{
				title = GameStrings.Get("GLUE_CARD_BANNED_IN_TWIST_TITLE");
				desc = GameStrings.Get("GLUE_CARD_BANNED_IN_TWIST_DESCRIPTION" + descPostfix);
				hasSetHelpStrings = true;
			}
			else if (deck != null && deck.FormatType == FormatType.FT_TWIST && !deck.CanAddCard(entityDef, actor.GetPremium()))
			{
				title = GameStrings.Get("GLUE_CARD_INVALID_IN_TWIST_TITLE");
				desc = GameStrings.Get("GLUE_CARD_INVALID_IN_TWIST_DESCRIPTION" + descPostfix);
				hasSetHelpStrings = true;
			}
			else if (deckRuleset != null)
			{
				List<RuleInvalidReason> ruleInvalidReasons = new List<RuleInvalidReason>();
				List<DeckRule> brokenRules = new List<DeckRule>();
				deckRuleset.CanAddToDeck(entityDef, actor.GetPremium(), deck, out ruleInvalidReasons, out brokenRules);
				foreach (DeckRule deckRule in brokenRules)
				{
					if (deckRule.Type == DeckRule.RuleType.IS_CLASS_CARD_OR_NEUTRAL)
					{
						title = GameStrings.Get("GLUE_GHOST_CARD_OTHER_CLASS_TITLE");
						desc = GameStrings.Get("GLUE_GHOST_CARD_OTHER_CLASS_DESCRIPTION" + descPostfix);
						hasSetHelpStrings = true;
						break;
					}
					if (deckRule.Type == DeckRule.RuleType.TOURIST_LIMIT)
					{
						title = GameStrings.Get("GLUE_GHOST_CARD_OVER_TOURIST_LIMIT_TITLE");
						desc = GameStrings.Get("GLUE_GHOST_CARD_OVER_TOURIST_LIMIT_DESCRIPTION" + descPostfix);
						hasSetHelpStrings = true;
						break;
					}
					if (deckRule.Type == DeckRule.RuleType.IS_IN_FORMAT && GameUtils.IsCardGameplayEventEverActive(entityDef))
					{
						title = GameStrings.Get("GLUE_GHOST_CARD_NOT_VALID_TITLE");
						desc = GameStrings.Get("GLUE_GHOST_CARD_NOT_VALID_DESCRIPTION" + descPostfix);
						hasSetHelpStrings = true;
					}
				}
			}
			if (!hasSetHelpStrings)
			{
				title = GameStrings.Get("GLUE_GHOST_CARD_NEVER_VALID_TITLE");
				desc = GameStrings.Get("GLUE_GHOST_CARD_NEVER_VALID_DESCRIPTION" + descPostfix);
			}
			break;
		}
		case GhostCard.Type.MISSING_UNCRAFTABLE:
		case GhostCard.Type.MISSING:
		{
			CollectibleDisplay currentCollectibleDisplay = CollectionManager.Get().GetCollectibleDisplay();
			string obj = ((deck != null && currentCollectibleDisplay != null && currentCollectibleDisplay.GetViewMode() != CollectionUtils.ViewMode.DECK_TEMPLATE) ? "GLUE_GHOST_CARD_MISSING_DESCRIPTION" : "GLUE_GHOST_CARD_MISSING_DESCRIPTION_NO_REPLACE");
			title = GameStrings.Get("GLUE_GHOST_CARD_MISSING_TITLE");
			desc = GameStrings.Get(obj + descPostfix);
			break;
		}
		default:
			return;
		}
		SetupTooltipPanel(title, desc);
		TooltipPanelCreationArgs tooltipArgs = new TooltipPanelCreationArgs
		{
			actorMeshRoot = actor.GetMeshRenderer().gameObject,
			actorRoot = actor.gameObject,
			card = m_card,
			boneSource = TooltipBoneSource.TOP_LEFT
		};
		if (CanUseBonesForPlacingTooltips(tooltipArgs))
		{
			try
			{
				PositionPanelsWithBones(tooltipArgs, m_panelTokenSource.Token);
				return;
			}
			catch (Exception e)
			{
				PrintUncaughtTooltipException(e, "collection manager");
				return;
			}
		}
		PrintUnableToDisplayTooltipError("collection manager");
	}

	public void UpdateKeywordHelpForDeckHelper(EntityDef entityDef, Actor actor, int cardChoice = 0)
	{
		m_scaleToUse = TooltipPanel.DECK_HELPER_SCALE;
		PrepareToUpdateKeywordHelp(actor);
		SetUpPanels(entityDef);
		TooltipPanelCreationArgs tooltipArgs = new TooltipPanelCreationArgs
		{
			actorMeshRoot = actor.GetMeshRenderer().gameObject,
			actorRoot = actor.gameObject,
			boneSource = TooltipBoneSource.TOP_LEFT
		};
		if (CanUseBonesForPlacingTooltips(tooltipArgs))
		{
			try
			{
				PositionPanelsWithBones(tooltipArgs, m_panelTokenSource.Token);
				return;
			}
			catch (Exception e)
			{
				PrintUncaughtTooltipException(e, "deck helper");
				return;
			}
		}
		PrintUnableToDisplayTooltipError("deck helper");
	}

	public void UpdateKeywordHelpForAdventure(EntityDef entityDef, Actor actor)
	{
		m_scaleToUse = TooltipPanel.ADVENTURE_SCALE;
		PrepareToUpdateKeywordHelp(actor);
		SetUpPanels(entityDef);
		TooltipPanelCreationArgs tooltipArgs = new TooltipPanelCreationArgs
		{
			actorMeshRoot = actor.GetMeshRenderer().gameObject,
			actorRoot = actor.gameObject,
			boneSource = TooltipBoneSource.TOP_LEFT
		};
		if (CanUseBonesForPlacingTooltips(tooltipArgs))
		{
			try
			{
				PositionPanelsWithBones(tooltipArgs, m_panelTokenSource.Token);
				return;
			}
			catch (Exception e)
			{
				PrintUncaughtTooltipException(e, "adventure");
				return;
			}
		}
		PrintUnableToDisplayTooltipError("adventure");
	}

	public void UpdateKeywordHelpForForge(EntityDef entityDef, Actor actor, TooltipBoneSource tooltipBoneSource)
	{
		m_scaleToUse = TooltipPanel.FORGE_SCALE;
		PrepareToUpdateKeywordHelp(actor);
		SetUpPanels(entityDef);
		TooltipPanelCreationArgs tooltipArgs = new TooltipPanelCreationArgs
		{
			actorMeshRoot = actor.GetMeshRenderer().gameObject,
			actorRoot = actor.gameObject,
			boneSource = tooltipBoneSource
		};
		if (CanUseBonesForPlacingTooltips(tooltipArgs))
		{
			try
			{
				PositionPanelsWithBones(tooltipArgs, m_panelTokenSource.Token);
				return;
			}
			catch (Exception e)
			{
				PrintUncaughtTooltipException(e, "arena");
				return;
			}
		}
		PrintUnableToDisplayTooltipError("arena");
	}

	public void UpdateKeywordHelpForPackOpening(EntityDef entityDef, Actor actor, TooltipBoneSource tooltipBoneSource, bool useMassPackOpeningPhoneScale = false)
	{
		m_scaleToUse = (useMassPackOpeningPhoneScale ? 5f : 2.75f);
		PrepareToUpdateKeywordHelp(actor);
		SetUpPanels(entityDef);
		TooltipPanelCreationArgs tooltipArgs = new TooltipPanelCreationArgs
		{
			actorMeshRoot = actor.GetMeshRenderer().gameObject,
			actorRoot = actor.gameObject,
			boneSource = tooltipBoneSource
		};
		if (CanUseBonesForPlacingTooltips(tooltipArgs))
		{
			try
			{
				PositionPanelsWithBones(tooltipArgs, m_panelTokenSource.Token);
				return;
			}
			catch (Exception e)
			{
				PrintUncaughtTooltipException(e, "pack opening");
				return;
			}
		}
		PrintUnableToDisplayTooltipError("pack opening");
	}

	public void UpdateKeywordHelpForDiscover(Entity entity, Actor actor, TooltipBoneSource boneSource)
	{
		m_card = entity.GetCard();
		m_scaleToUse = TooltipPanel.HAND_SCALE;
		PrepareToUpdateKeywordHelp(actor);
		string[] customKeywordData = GameState.Get().GetGameEntity().NotifyOfKeywordHelpPanelDisplay(entity);
		if (customKeywordData != null)
		{
			SetupTooltipPanel(customKeywordData[0], customKeywordData[1]);
		}
		List<EntityBase> additionalEntityBasesForKeyword;
		if (entity.GetTag(GAME_TAG.IS_NIGHTMARE_BONUS) == 1)
		{
			Entity minionToSummon = GameState.Get().GetEntity(entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_ENT_1));
			if (minionToSummon != null)
			{
				GetDesiredEntityBaseForEntity(minionToSummon, isHistoryTile: false, out var minionBase, out additionalEntityBasesForKeyword);
				GetDesiredEntityBaseForEntity(entity, isHistoryTile: false, out var enchantmentBase, out additionalEntityBasesForKeyword);
				string descriptionOverride = minionToSummon.GetCardTextInHand();
				SetupSignatureInfoPanelIfNecessary(minionBase, descriptionOverride);
				SetUpPanels(minionBase, new List<EntityBase> { enchantmentBase }, descriptionOverride);
			}
		}
		else
		{
			GetDesiredEntityBaseForEntity(entity, isHistoryTile: false, out var entityBaseForKeyword, out additionalEntityBasesForKeyword);
			SetupSignatureInfoPanelIfNecessary(entityBaseForKeyword);
			SetUpPanels(entityBaseForKeyword);
		}
		MeshRenderer renderer = actor.GetMeshRenderer();
		GameObject rootObject = null;
		if (renderer != null)
		{
			rootObject = renderer.gameObject;
		}
		TooltipPanelCreationArgs tooltipArgs = new TooltipPanelCreationArgs
		{
			actorMeshRoot = rootObject,
			actorRoot = actor.gameObject,
			card = m_card,
			boneSource = boneSource
		};
		if (CanUseBonesForPlacingTooltips(tooltipArgs))
		{
			try
			{
				PositionPanelsWithBones(tooltipArgs, m_panelTokenSource.Token);
				return;
			}
			catch (Exception e)
			{
				PrintUncaughtTooltipException(e, "discover");
				return;
			}
		}
		PrintUnableToDisplayTooltipError("discover");
	}

	public void UpdateKeywordHelpForSubOptions(Entity entity, Actor actor, TooltipBoneSource boneSource)
	{
		if (entity == null || actor == null)
		{
			return;
		}
		m_card = entity.GetCard();
		if (m_card == null)
		{
			return;
		}
		m_scaleToUse = TooltipPanel.HAND_SCALE;
		PrepareToUpdateKeywordHelp(actor);
		if (GameState.Get() != null && GameState.Get().GetGameEntity() != null)
		{
			string[] customKeywordData = GameState.Get().GetGameEntity().NotifyOfKeywordHelpPanelDisplay(entity);
			if (customKeywordData != null && customKeywordData.Length >= 2 && customKeywordData[0] != null && customKeywordData[1] != null)
			{
				SetupTooltipPanel(customKeywordData[0], customKeywordData[1]);
			}
		}
		GetDesiredEntityBaseForEntity(entity, isHistoryTile: false, out var entityBaseForKeyword, out var _);
		if (IsLockedTitanAbility(entity))
		{
			string name = GameStrings.Get("GAMEPLAY_TOOLTIP_TITAN_ABILITY_LOCKED_NAME");
			string text = GameStrings.Get("GAMEPLAY_TOOLTIP_TITAN_ABILITY_LOCKED_TEXT");
			SetupTooltipPanel(name, text);
		}
		SetUpPanels(entityBaseForKeyword);
		GameObject actorMeshRootGameObject = null;
		if (actor.GetMeshRenderer() != null)
		{
			actorMeshRootGameObject = actor.GetMeshRenderer().gameObject;
		}
		TooltipPanelCreationArgs tooltipArgs = new TooltipPanelCreationArgs
		{
			actorMeshRoot = actorMeshRootGameObject,
			actorRoot = actor.gameObject,
			card = m_card,
			boneSource = boneSource,
			isLettuceAbility = false
		};
		if (CanUseBonesForPlacingTooltips(tooltipArgs))
		{
			try
			{
				PositionPanelsWithBones(tooltipArgs, m_panelTokenSource.Token);
				return;
			}
			catch (Exception e)
			{
				PrintUncaughtTooltipException(e, "suboptions");
				return;
			}
		}
		PrintUnableToDisplayTooltipError("suboptions");
	}

	private bool IsLockedTitanAbility(Entity entity)
	{
		if (entity == null)
		{
			return false;
		}
		bool num = entity.HasTag(GAME_TAG.LITERALLY_UNPLAYABLE);
		Entity parentEntity = entity.GetParentEntity();
		bool parentIsTitan = false;
		if (parentEntity != null)
		{
			parentIsTitan = parentEntity.HasTag(GAME_TAG.TITAN);
		}
		return num && parentIsTitan;
	}

	private MulliganDisplay_RelativeHandPosition Mulligan_GetRelativePositionOfCardInHand(Entity ent)
	{
		if (ent == null)
		{
			return MulliganDisplay_RelativeHandPosition.INVALID;
		}
		int zonePosition = m_card.GetZonePosition();
		Zone handZone = m_card.GetZone();
		if (!(handZone is ZoneHand))
		{
			return MulliganDisplay_RelativeHandPosition.INVALID;
		}
		int totalNonCoinCards = 0;
		foreach (Card card in handZone.GetCards())
		{
			if (!(card == null))
			{
				Entity currentEnt = card.GetEntity();
				if (currentEnt != null && !currentEnt.HasTag(GAME_TAG.COIN_CARD))
				{
					totalNonCoinCards++;
				}
			}
		}
		float middlePosition = (float)(totalNonCoinCards + 1) / 2f;
		float distanceToMiddleIndex = (float)zonePosition - middlePosition;
		if (distanceToMiddleIndex == 0f)
		{
			return MulliganDisplay_RelativeHandPosition.MIDDLE;
		}
		if (distanceToMiddleIndex < 0f)
		{
			return MulliganDisplay_RelativeHandPosition.LEFT;
		}
		return MulliganDisplay_RelativeHandPosition.RIGHT;
	}

	public void UpdateKeywordHelpForMulliganCard(Entity entity, Actor actor)
	{
		m_card = entity.GetCard();
		m_scaleToUse = TooltipPanel.MULLIGAN_SCALE;
		PrepareToUpdateKeywordHelp(actor);
		string[] customKeywordData = GameState.Get().GetGameEntity().NotifyOfKeywordHelpPanelDisplay(entity);
		if (customKeywordData != null)
		{
			SetupTooltipPanel(customKeywordData[0], customKeywordData[1]);
		}
		GetDesiredEntityBaseForEntity(entity, isHistoryTile: false, out var entityBaseForKeyword, out var _);
		SetUpPanels(entityBaseForKeyword);
		MulliganDisplay_RelativeHandPosition cardLocation = Mulligan_GetRelativePositionOfCardInHand(entity);
		TooltipBoneSource startingBone = (UniversalInputManager.UsePhoneUI ? ((cardLocation != MulliganDisplay_RelativeHandPosition.RIGHT) ? TooltipBoneSource.TOP_RIGHT : TooltipBoneSource.TOP_LEFT) : ((cardLocation != MulliganDisplay_RelativeHandPosition.RIGHT) ? TooltipBoneSource.BOTTOM_MIDDLE_GOING_RIGHT : TooltipBoneSource.BOTTOM_MIDDLE_GOING_LEFT));
		TooltipPanelCreationArgs tooltipArgs = new TooltipPanelCreationArgs
		{
			actorMeshRoot = actor.GetMeshRenderer().gameObject,
			actorRoot = actor.gameObject,
			card = m_card,
			boneSource = startingBone
		};
		if (CanUseBonesForPlacingTooltips(tooltipArgs))
		{
			try
			{
				PositionPanelsWithBones(tooltipArgs, m_panelTokenSource.Token);
				return;
			}
			catch (Exception e)
			{
				PrintUncaughtTooltipException(e, "mulligan");
				return;
			}
		}
		PrintUnableToDisplayTooltipError("mulligan");
	}

	private void PrepareToUpdateKeywordHelp(Actor actor)
	{
		HideKeywordHelp();
		m_actor = actor;
		m_tooltipPanels.Clear();
	}

	private void PrepareToUpdateGhostCardHelp(Actor actor)
	{
		HideTooltipPanels();
		m_actor = actor;
		m_tooltipPanels.Clear();
	}

	private void ShowIncompatibleRunesPanelIfNecessary(EntityBase entityBase)
	{
		if (entityBase.HasRuneCost && CollectionManager.Get().IsInEditMode())
		{
			RunePattern runePattern = new RunePattern(entityBase);
			if (!CollectionManager.Get().GetEditedDeck().CanAddRunes(runePattern, DeckRule_DeathKnightRuneLimit.MaxRuneSlots))
			{
				string header = GameStrings.Get("GLUE_COLLECTION_INCOMPATIBLE_RUNES_HEADER");
				string description = GameStrings.Get("GLUE_COLLECTION_INCOMPATIBLE_RUNES_DESCRIPTION");
				SetupTooltipPanel(header, description);
			}
		}
	}

	private void ShowIncompatibleTouristPanelIfNecessary(EntityBase entityBase)
	{
		if (entityBase.HasTag(GAME_TAG.TOURIST) && CollectionManager.Get().IsInEditMode())
		{
			CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
			if (deck.GetCardIdCount(entityBase.GetCardId()) <= 0 && deck.GetRuleset(null).HasMaxTouristsRule(out var maxTourists) && deck.GetCardCountHasTag(GAME_TAG.TOURIST) >= maxTourists)
			{
				string header = GameStrings.Get("GLUE_COLLECTION_TOURIST_LIMIT_HEADER");
				string description = GameStrings.Format("GLUE_COLLECTION_TOURIST_LIMIT_DESCRIPTION", maxTourists);
				SetupTooltipPanel(header, description);
			}
		}
	}

	private void SetupSignatureInfoPanelIfNecessary(EntityBase entityBase, string descriptionOverride = null)
	{
		if (!(m_actor == null) && m_actor.GetPremium() == TAG_PREMIUM.SIGNATURE && !ActorNames.SignatureFrameHasPowersText(entityBase.GetCardId()))
		{
			float delayAmount = 0f;
			if (m_signaturePanel == null)
			{
				m_signaturePanel = UnityEngine.Object.Instantiate(m_signaturePanelPrefab);
				delayAmount = 0.1f;
			}
			m_signaturePanel.SetTribe(entityBase.GetRaceText(), entityBase.GetRaceCount() > 1);
			m_signaturePanel.SetSilenced(entityBase.IsSilenced());
			string header = "";
			EntityDef entityDef = DefLoader.Get().GetEntityDef(entityBase.GetCardId());
			if (entityDef != null)
			{
				header = entityDef.GetName();
			}
			string description = ((descriptionOverride == null) ? m_actor.GetPowersText() : descriptionOverride);
			SetupTooltipPanel(header, description, m_signaturePanel, delayAmount);
		}
	}

	private void SetupTooltipPanelForTitansFromRelatedCards(EntityBase entityBase)
	{
		if (!entityBase.IsTitan())
		{
			return;
		}
		int databaseID = GameUtils.TranslateCardIdToDbId(entityBase.GetCardId());
		foreach (RelatedCardsDbfRecord relatedCard in GameDbf.Card.GetRecord(databaseID).RelatedCards)
		{
			string relatedCardID = GameDbf.Card.GetRecord(relatedCard.RelatedCardDatabaseId)?.NoteMiniGuid;
			if (!string.IsNullOrEmpty(relatedCardID))
			{
				EntityDef relatedDef = DefLoader.Get().GetEntityDef(relatedCardID);
				if (relatedDef.IsSpell())
				{
					string name = relatedDef.GetName();
					string text = relatedDef.GetCardTextInHand();
					SetupTooltipPanel(name, text);
				}
			}
		}
	}

	private void SetupSignatureInfoPanelFromDataModel(CardDataModel cardDataModel)
	{
		if (cardDataModel.Premium != TAG_PREMIUM.SIGNATURE)
		{
			return;
		}
		EntityBase entityBase = DefLoader.Get().GetEntityDef(cardDataModel.CardId);
		if (entityBase != null)
		{
			float delayAmount = 0f;
			if (m_signaturePanel == null)
			{
				m_signaturePanel = UnityEngine.Object.Instantiate(m_signaturePanelPrefab);
				delayAmount = 0.1f;
			}
			m_signaturePanel.SetTribe(entityBase.GetRaceText(), entityBase.GetRaceCount() > 1);
			m_signaturePanel.SetSilenced(entityBase.IsSilenced());
			string header = "";
			EntityDef entityDef = DefLoader.Get().GetEntityDef(entityBase.GetCardId());
			if (entityDef != null)
			{
				header = entityDef.GetName();
			}
			string description = entityDef.GetCardTextInHand();
			SetupTooltipPanel(header, description, m_signaturePanel, delayAmount);
		}
	}

	private List<KeywordTextDbfRecord> GetKeywordRecordsSortedByDisplayOrder()
	{
		if (m_sortedKeywordOrderRecords == null)
		{
			List<KeywordTextDbfRecord> allKeywordRecords = GameDbf.KeywordText.GetRecords();
			if (allKeywordRecords == null)
			{
				return new List<KeywordTextDbfRecord>();
			}
			m_sortedKeywordOrderRecords = new List<KeywordTextDbfRecord>(allKeywordRecords);
			m_sortedKeywordOrderRecords.Sort(delegate(KeywordTextDbfRecord a, KeywordTextDbfRecord b)
			{
				if (a == null && b == null)
				{
					return 0;
				}
				if (a == null)
				{
					return -1;
				}
				if (b == null)
				{
					return 1;
				}
				return (a.TooltipInitOrder >= b.TooltipInitOrder) ? 1 : (-1);
			});
		}
		return m_sortedKeywordOrderRecords;
	}

	private void SetUpPanels(EntityBase mainEntityBaseForKeyword, List<EntityBase> additionalEntityBasesForKeyword = null, string signatureDescriptionOverride = null)
	{
		if (mainEntityBaseForKeyword == null)
		{
			Log.All.PrintWarning("SetUpPanels: entity base is null");
			return;
		}
		KeywordPanelEntityInfo keywordPanelEntityInfo = default(KeywordPanelEntityInfo);
		keywordPanelEntityInfo.MainEntityBase = mainEntityBaseForKeyword;
		keywordPanelEntityInfo.AdditionalEntityBases = additionalEntityBasesForKeyword;
		KeywordPanelEntityInfo entityInfo = keywordPanelEntityInfo;
		SetupSignatureInfoPanelIfNecessary(mainEntityBaseForKeyword, signatureDescriptionOverride);
		ShowIncompatibleRunesPanelIfNecessary(mainEntityBaseForKeyword);
		if (mainEntityBaseForKeyword.IsMultiClass() && !mainEntityBaseForKeyword.IsHeroPower() && (SceneMgr.Get().GetMode() == SceneMgr.Mode.COLLECTIONMANAGER || !mainEntityBaseForKeyword.HasFaction()))
		{
			SetupKeywordRefPanelMultiClass(mainEntityBaseForKeyword);
		}
		foreach (KeywordTextDbfRecord record in GetKeywordRecordsSortedByDisplayOrder())
		{
			if (record == null)
			{
				continue;
			}
			GAME_TAG tag = (GAME_TAG)record.Tag;
			switch (tag)
			{
			case GAME_TAG.SECRET:
				if (mainEntityBaseForKeyword.GetZone() != TAG_ZONE.SECRET || !mainEntityBaseForKeyword.IsSecret())
				{
					SetupKeywordPanelIfNecessary(entityInfo, tag);
				}
				continue;
			case GAME_TAG.AI_MUST_PLAY:
				if (mainEntityBaseForKeyword.IsHeroPower())
				{
					SetupKeywordPanelIfNecessary(entityInfo, tag);
				}
				continue;
			}
			bool neededThisTooltip = SetupKeywordPanelIfNecessary(entityInfo, tag);
			if (neededThisTooltip && tag == GAME_TAG.TITAN && entityInfo.MainEntityBase.IsTitan() && SceneMgr.Get().GetMode() == SceneMgr.Mode.GAMEPLAY)
			{
				return;
			}
			if (neededThisTooltip && GameUtils.IsClassTouristTag(tag))
			{
				ShowIncompatibleTouristPanelIfNecessary(mainEntityBaseForKeyword);
			}
		}
	}

	private void GetEntityTagValuesForKeywordPanel(EntityBase entityInfo, GAME_TAG tag, out int tagValue, out int referenceTagValue)
	{
		tagValue = 0;
		if (entityInfo.HasTag(tag))
		{
			tagValue = entityInfo.GetTag(tag);
		}
		else if (entityInfo.HasCachedTagForDormant(tag))
		{
			tagValue = entityInfo.GetCachedTagForDormant(tag);
		}
		referenceTagValue = 0;
		if (entityInfo.HasReferencedTag(tag))
		{
			referenceTagValue = entityInfo.GetReferencedTag(tag);
		}
	}

	private bool SetupKeywordPanelIfNecessary(KeywordPanelEntityInfo entityInfo, GAME_TAG tag)
	{
		EntityBase entityBase = entityInfo.MainEntityBase;
		GetEntityTagValuesForKeywordPanel(entityBase, tag, out var tagValue, out var referenceTagValue);
		KeywordTextDbfRecord keyWordRecord = GameDbf.KeywordText.GetRecord((KeywordTextDbfRecord r) => r.Tag == (int)tag);
		bool isValidForCollection = tagValue != 0 && SceneMgr.Get().GetMode() == SceneMgr.Mode.COLLECTIONMANAGER && GameStrings.HasCollectionKeywordText(tag);
		if (keyWordRecord != null && isValidForCollection && keyWordRecord.IsCollectionOnly)
		{
			SetupTooltipPanel(GameStrings.Get(keyWordRecord.Name), GameStrings.Get(keyWordRecord.CollectionText));
			return true;
		}
		if (tagValue == 0 && referenceTagValue == 0 && entityInfo.AdditionalEntityBases != null)
		{
			foreach (EntityBase additionEntityInfo in entityInfo.AdditionalEntityBases)
			{
				GetEntityTagValuesForKeywordPanel(additionEntityInfo, tag, out var additionalTagValue, out var additionalReferenceValue);
				if (additionalTagValue != 0 || additionalReferenceValue != 0)
				{
					tagValue = additionalTagValue;
					referenceTagValue = additionalReferenceValue;
					entityBase = additionEntityInfo;
					break;
				}
			}
		}
		if (tagValue == 0 && referenceTagValue == 0)
		{
			return false;
		}
		if (tag == GAME_TAG.BACON_BLOOD_GEM_TOOLTIP)
		{
			Player controller;
			if (ZoneMgr.Get().IsBattlegroundShoppingPhase())
			{
				controller = GameState.Get().GetLocalSidePlayer();
			}
			else
			{
				int controllerId = entityBase.GetControllerId();
				controller = GameState.Get().GetPlayer(controllerId);
			}
			int attack = controller.GetTag(GAME_TAG.BACON_BLOODGEMBUFFATKVALUE) + 1;
			int health = controller.GetTag(GAME_TAG.BACON_BLOODGEMBUFFHEALTHVALUE) + 1;
			string name = GameStrings.GetKeywordName(tag);
			string text = GameStrings.Get(GameStrings.GetKeywordTextKey(tag));
			string bonus = "+" + attack + "/+" + health;
			if (text == null)
			{
				Log.Gameplay.PrintError("TooltipPanelManager.SetupKeywordPanelIfNecessary(): Unable to load Blood Gem text.");
				return false;
			}
			text = string.Format(text, bonus);
			if (controller.GetEntityId() != GameState.Get().GetLocalSidePlayer().GetEntityId())
			{
				Player localSidePlayer = GameState.Get().GetLocalSidePlayer();
				int attack2 = localSidePlayer.GetTag(GAME_TAG.BACON_BLOODGEMBUFFATKVALUE) + 1;
				int health2 = localSidePlayer.GetTag(GAME_TAG.BACON_BLOODGEMBUFFHEALTHVALUE) + 1;
				if (attack != 1 || health != 1 || attack2 != 1 || health2 != 1)
				{
					string additionalText = GameStrings.Get("GLOBAL_KEYWORD_BLOOD_GEM_TEXT_ADDITIONAL");
					if (additionalText != null)
					{
						bonus = "+" + attack2 + "/+" + health2;
						additionalText = string.Format(additionalText, bonus);
						text += additionalText;
					}
				}
			}
			SetupTooltipPanel(name, text);
			return true;
		}
		if (tag == GAME_TAG.BACON_YAMATO_CANNON_TOOLTIP || tag == GAME_TAG.BACON_LIBERATOR_TOOLTIP || tag == GAME_TAG.BACON_MEDIVAC_TOOLTIP)
		{
			string name2 = GameStrings.GetKeywordName(tag);
			string text2 = GameStrings.Get(GameStrings.GetKeywordTextKey(tag));
			int bonus2 = entityBase.GetTag(tag);
			if (tag == GAME_TAG.BACON_YAMATO_CANNON_TOOLTIP && entityBase.HasTag(GAME_TAG.BACON_YAMATO_CANNON))
			{
				text2 = GameStrings.Get("GLOBAL_KEYWORD_YAMATO_CANNON_TEXT_ALT");
			}
			if (text2 == null)
			{
				Log.Gameplay.PrintError("TooltipPanelManager.SetupKeywordPanelIfNecessary(): Unable to load battlecruiser tooltip text.");
				return false;
			}
			text2 = string.Format(text2, bonus2);
			SetupTooltipPanel(name2, text2);
			return true;
		}
		if (tag == GAME_TAG.LETTUCE_FACTION && (tagValue != 0 || referenceTagValue != 0))
		{
			TAG_LETTUCE_FACTION tAG_LETTUCE_FACTION = (TAG_LETTUCE_FACTION)tagValue;
			string key = "GLUE_LETTUCE_FACTION_" + tAG_LETTUCE_FACTION;
			string title = GameStrings.Get("GLUE_LETTUCE_FACTION");
			string localText = GameStrings.Get(key);
			if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(localText))
			{
				SetupTooltipPanel(title, localText);
				return true;
			}
			return false;
		}
		SceneMgr.Mode currentSceneMode = SceneMgr.Get().GetMode();
		if (tagValue != 0 && tag == GAME_TAG.TITAN)
		{
			switch (currentSceneMode)
			{
			case SceneMgr.Mode.GAMEPLAY:
			{
				int entityID = entityBase.GetEntityId();
				Entity parent = GameState.Get().GetEntity(entityID);
				if (parent == null)
				{
					SetupKeywordPanel(tag);
					return false;
				}
				parent.GetCard();
				List<int> subCardIDs = parent.GetSubCardIDs();
				Entity[] children = new Entity[subCardIDs.Count];
				for (int i = 0; i < subCardIDs.Count; i++)
				{
					children[i] = GameState.Get().GetEntity(subCardIDs[i]);
				}
				Entity[] array = children;
				foreach (Entity child in array)
				{
					string name3 = child.GetName();
					string transformedCardText = UberText.RemoveMarkupAndCollapseWhitespaces(child.GetCardTextInHand(), replaceCarriageReturnWithBreakHint: true, preserveBreakHint: true);
					SetupTooltipPanel(name3, transformedCardText).SetLockVisibility(child.HasTag(GAME_TAG.LITERALLY_UNPLAYABLE));
				}
				SetupKeywordPanel(tag);
				return true;
			}
			case SceneMgr.Mode.COLLECTIONMANAGER:
			case SceneMgr.Mode.PACKOPENING:
			case SceneMgr.Mode.DRAFT:
				SetupTooltipPanelForTitansFromRelatedCards(entityBase);
				SetupKeywordPanel(tag);
				return true;
			}
		}
		if (tagValue != 0 && tag == GAME_TAG.HAS_DARK_GIFT && currentSceneMode == SceneMgr.Mode.GAMEPLAY)
		{
			if (entityBase.GetTag<TAG_ZONE>(GAME_TAG.ZONE) != TAG_ZONE.HAND)
			{
				return false;
			}
			int entityID2 = entityBase.GetEntityId();
			Entity parent2 = GameState.Get().GetEntity(entityID2);
			if (parent2 == null)
			{
				Log.Gameplay.PrintWarning("Unable to locate the parent entity for entityId {0}", entityID2);
				return false;
			}
			List<Entity> displayedEnchantments = parent2.GetDisplayedEnchantments();
			List<Entity> darkGifts = new List<Entity>();
			foreach (Entity child2 in displayedEnchantments)
			{
				if (child2.GetTag(GAME_TAG.IS_NIGHTMARE_BONUS) == 1)
				{
					darkGifts.Add(child2);
				}
			}
			if (darkGifts.Count == 0)
			{
				Log.Gameplay.PrintWarning("{0} claims to have dark gifts but doesn't have any dark gift enchantments attatched", entityBase);
				return false;
			}
			if (darkGifts.Count > 2)
			{
				string name4 = GameStrings.Format("GLOBAL_KEYWORD_DARKGIFT_BONUS_MULTI");
				string text3 = GameStrings.Format("GLOBAL_KEYWORD_DARKGIFT_BONUS_MULTI_TEXT", darkGifts.Count);
				SetupTooltipPanel(name4, text3);
			}
			else
			{
				foreach (Entity child3 in darkGifts)
				{
					string name5 = GameStrings.Get("GLOBAL_KEYWORD_DARKGIFT_BONUS");
					SetupTooltipPanel(name5, child3.GetCardTextInHand());
				}
			}
			return true;
		}
		if (tagValue != 0 && tag == GAME_TAG.EXCAVATE && currentSceneMode == SceneMgr.Mode.GAMEPLAY)
		{
			int controllerId2 = entityBase.GetControllerId();
			Player player = GameState.Get().GetPlayer(controllerId2);
			string maxString = ((player.GetTag(GAME_TAG.MAX_EXCAVATE_TIER) == 3) ? GameStrings.Get("GLOBAL_KEYWORD_EXCAVATE_LEGENDARY") : GameStrings.Get("GLOBAL_KEYWORD_EXCAVATE_EPIC"));
			int current = player.GetTag(GAME_TAG.CURRENT_EXCAVATE_TIER);
			string currentString = "";
			switch (current)
			{
			case 0:
				currentString = GameStrings.Get("GLOBAL_KEYWORD_EXCAVATE_COMMON");
				break;
			case 1:
				currentString = GameStrings.Get("GLOBAL_KEYWORD_EXCAVATE_RARE");
				break;
			case 2:
				currentString = GameStrings.Get("GLOBAL_KEYWORD_EXCAVATE_EPIC");
				break;
			case 3:
				currentString = GameStrings.Get("GLOBAL_KEYWORD_EXCAVATE_LEGENDARY");
				break;
			}
			string name6 = GameStrings.GetKeywordName(tag);
			string text4 = GameStrings.Format("GLOBAL_KEYWORD_EXCAVATE_GAMEPLAY", maxString, currentString);
			SetupTooltipPanel(name6, text4);
			return true;
		}
		if (keyWordRecord != null && !keyWordRecord.IsCollectionOnly)
		{
			if (isValidForCollection)
			{
				if (tag == GAME_TAG.EMPOWER)
				{
					if (entityBase.GetClass() != TAG_CLASS.NEUTRAL)
					{
						tag = GetEmpowerTagByClass(entityBase.GetClass());
					}
					if (CollectionManager.Get().IsInEditMode())
					{
						CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
						string galakrondCardIdForClass = GameUtils.GetGalakrondCardIdByClass(deck.GetClass());
						if (deck.GetCardIdCount(galakrondCardIdForClass) > 0)
						{
							tag = GetEmpowerTagByClass(deck.GetClass());
						}
					}
				}
				SetupCollectionKeywordPanel(tag);
				return true;
			}
			if (tagValue != 0 && GameStrings.HasKeywordText(tag))
			{
				GAME_TAG[] array2 = spellpowerTags;
				foreach (GAME_TAG spellpowerTag in array2)
				{
					if (tag == spellpowerTag)
					{
						int spellPower = entityBase.GetTag(tag);
						string text5 = string.Empty;
						text5 = ((spellPower <= 0) ? GameStrings.Get(GameStrings.GetRefKeywordTextKey(tag)) : GameStrings.Format(GameStrings.GetKeywordTextKey(tag), spellPower));
						string name7 = GameStrings.GetKeywordName(tag);
						SetupTooltipPanel(name7, text5);
						return true;
					}
				}
				if (tag == GAME_TAG.WINDFURY && tagValue > 1)
				{
					return false;
				}
				if (GameMgr.Get() != null && GameMgr.Get().IsBattlegrounds())
				{
					if (tag == GAME_TAG.FROZEN)
					{
						SetupKeywordPanel(GAME_TAG.BACON_FREEZE_TOOLTIP);
						return true;
					}
					if (tag == GAME_TAG.STEALTH)
					{
						SetupKeywordPanel(GAME_TAG.BACON_STEALTH_TOOLTIP);
						return true;
					}
					if (tag == GAME_TAG.QUEST)
					{
						SetupKeywordPanel(GAME_TAG.BACON_QUEST_TOOLTIP);
						return true;
					}
					if (tag == GAME_TAG.REBORN)
					{
						SetupKeywordPanel(GAME_TAG.BACON_REBORN_TOOLTIP);
						return true;
					}
				}
				if (tag == GAME_TAG.SHIFTING_MINION || tag == GAME_TAG.SHIFTING_WEAPON || tag == GAME_TAG.SHIFTING_SPELL || tag == GAME_TAG.SHIFTING_TOP || tag == GAME_TAG.SHIFTING)
				{
					int cardDbId = entityBase.GetTag(GAME_TAG.TRANSFORMED_FROM_CARD);
					if (cardDbId == 0)
					{
						return false;
					}
					EntityDef preTransformEntityDef = DefLoader.Get().GetEntityDef(cardDbId);
					string text6 = GameStrings.Get(GameStrings.GetKeywordTextKey(tag));
					if (cardDbId == 102614)
					{
						text6 = text6 + " " + GameStrings.Get("GLOBAL_KEYWORD_SHIFTING_ADDS_STEALTH_TEXT");
					}
					if (cardDbId == 102564)
					{
						text6 = text6 + " " + GameStrings.Get("GLOBAL_KEYWORD_SHIFTING_ADDS_SPELL_DAMAGE_TEXT");
					}
					if (cardDbId == 102549)
					{
						text6 = text6 + " " + GameStrings.Get("GLOBAL_KEYWORD_SHIFTING_ADDS_POISONOUS_TEXT");
					}
					SetupTooltipPanel(preTransformEntityDef.GetName(), text6);
					return true;
				}
				if (tag == GAME_TAG.AI_MUST_PLAY && SceneMgr.Get().GetMode() == SceneMgr.Mode.GAMEPLAY)
				{
					int controllerId3 = entityBase.GetControllerId();
					Player controller2 = GameState.Get().GetPlayer(controllerId3);
					if (controller2 != null && !controller2.IsAI())
					{
						return false;
					}
				}
				if (tag == GAME_TAG.EMPOWER && SceneMgr.Get().GetMode() == SceneMgr.Mode.GAMEPLAY)
				{
					int controllerId4 = entityBase.GetControllerId();
					Player controller3 = GameState.Get().GetPlayer(controllerId4);
					if (controller3 != null && controller3.HasTag(GAME_TAG.PROXY_GALAKROND))
					{
						Entity galakrond = GameState.Get().GetEntity(controller3.GetTag(GAME_TAG.PROXY_GALAKROND));
						tag = GetEmpowerTagByClass(galakrond.GetClass());
					}
				}
				if (keyWordRecord.IsCollectionOnly)
				{
					return true;
				}
				if (!entityBase.IsHeroPower() && (tag == GAME_TAG.PROTOSS || tag == GAME_TAG.TERRAN || tag == GAME_TAG.ZERG || tag == GAME_TAG.GRIMY_GOONS || tag == GAME_TAG.JADE_LOTUS || tag == GAME_TAG.KABAL) && entityBase.IsMultiClass() && SceneMgr.Get().GetMode() != SceneMgr.Mode.COLLECTIONMANAGER)
				{
					SetupKeywordPanelFactionMultiClass(tag, entityBase);
					return true;
				}
				SetupKeywordPanel(tag);
				return true;
			}
			if (referenceTagValue != 0 && GameStrings.HasRefKeywordText(tag))
			{
				SetupKeywordRefPanel(tag);
				return true;
			}
			SetupKeywordPanel(tag);
			return true;
		}
		return false;
	}

	private void SetupCollectionKeywordPanel(GAME_TAG tag)
	{
		string name = GameStrings.GetKeywordName(tag);
		string text = GameStrings.Get(GameStrings.GetCollectionKeywordTextKey(tag));
		SetupTooltipPanel(name, text);
	}

	private void SetupKeywordPanel(GAME_TAG tag)
	{
		string name = GameStrings.GetKeywordName(tag);
		string text = GameStrings.Get(GameStrings.GetKeywordTextKey(tag));
		SetupTooltipPanel(name, text);
	}

	private void SetupKeywordRefPanel(GAME_TAG tag)
	{
		string name = GameStrings.GetKeywordName(tag);
		string text = GameStrings.Get(GameStrings.GetRefKeywordTextKey(tag));
		SetupTooltipPanel(name, text);
	}

	private void SetupKeywordRefPanelMultiClass(EntityBase entityBase)
	{
		List<TAG_CLASS> cardClasses = new List<TAG_CLASS>();
		entityBase.GetClasses(cardClasses);
		string name = GameStrings.Format("GLOBAL_KEYWORD_MULTICLASS", cardClasses.Count);
		string text = GameStrings.Format("GLOBAL_KEYWORD_MULTICLASS_TEXT", GameStrings.GetClassesNameComma(cardClasses));
		SetupTooltipPanel(name, text);
	}

	private void SetupKeywordPanelFactionMultiClass(GAME_TAG tag, EntityBase entityBase)
	{
		List<TAG_CLASS> cardClasses = new List<TAG_CLASS>();
		entityBase.GetClasses(cardClasses);
		string name = "";
		string text = "";
		switch (tag)
		{
		case GAME_TAG.PROTOSS:
			name = GameStrings.Get("GLOBAL_KEYWORD_PROTOSS_MULTI");
			text = GameStrings.Format("GLOBAL_KEYWORD_PROTOSS_MULTI_TEXT", cardClasses.Count);
			break;
		case GAME_TAG.ZERG:
			name = GameStrings.Get("GLOBAL_KEYWORD_ZERG_MULTI");
			text = GameStrings.Format("GLOBAL_KEYWORD_ZERG_MULTI_TEXT", cardClasses.Count);
			break;
		case GAME_TAG.TERRAN:
			name = GameStrings.Get("GLOBAL_KEYWORD_TERRAN_MULTI");
			text = GameStrings.Format("GLOBAL_KEYWORD_TERRAN_MULTI_TEXT", cardClasses.Count);
			break;
		case GAME_TAG.GRIMY_GOONS:
			name = GameStrings.Get("GLOBAL_KEYWORD_GRIMY_GOONS_MULTI");
			text = GameStrings.Format("GLOBAL_KEYWORD_GRIMY_GOONS_MULTI_TEXT", cardClasses.Count);
			break;
		case GAME_TAG.JADE_LOTUS:
			name = GameStrings.Get("GLOBAL_KEYWORD_JADE_LOTUS_MULTI");
			text = GameStrings.Format("GLOBAL_KEYWORD_JADE_LOTUS_MULTI_TEXT", cardClasses.Count);
			break;
		case GAME_TAG.KABAL:
			name = GameStrings.Get("GLOBAL_KEYWORD_KABAL_MULTI");
			text = GameStrings.Format("GLOBAL_KEYWORD_KABAL_MULTI_TEXT", cardClasses.Count);
			break;
		}
		SetupTooltipPanel(name, text);
	}

	private TooltipPanel SetupTooltipPanel(string headline, string description)
	{
		return SetupTooltipPanel(headline, description, m_tooltipPanelPool.Acquire());
	}

	private TooltipPanel SetupTooltipPanel(string headline, string description, TooltipPanel panel)
	{
		return SetupTooltipPanel(headline, description, panel, 0.4f);
	}

	private TooltipPanel SetupTooltipPanel(string headline, string description, TooltipPanel panel, float delayAmount)
	{
		if (panel == null)
		{
			return panel;
		}
		panel.Reset();
		panel.Initialize(headline, description);
		panel.SetScale(m_scaleToUse);
		m_tooltipPanels.Add(panel);
		FadeInPanel(panel, delayAmount);
		return panel;
	}

	private void FadeInPanel(TooltipPanel helpPanel, float delayAmount)
	{
		RenderUtils.SetAlpha(helpPanel.gameObject, 0f, includeInactive: true);
		if (GameState.Get() != null && GameState.Get().GetBooleanGameOption(GameEntityOption.KEYWORD_HELP_DELAY_OVERRIDDEN))
		{
			delayAmount = 0f;
		}
		if (helpPanel is SignatureTooltipPanel)
		{
			iTween.Stop(helpPanel.gameObject);
			iTween.FadeTo(helpPanel.gameObject, iTween.Hash("alpha", 1f, "time", 0.125f, "delay", delayAmount));
		}
		else
		{
			iTween.Stop(base.gameObject);
			iTween.ValueTo(base.gameObject, iTween.Hash("onupdatetarget", base.gameObject, "onupdate", "OnPanelFadeUpdate", "time", 0.125f, "delay", delayAmount, "to", 1f, "from", 0f));
		}
	}

	private void OnPanelFadeUpdate(float newValue)
	{
		foreach (TooltipPanel helpPanel in m_tooltipPanels)
		{
			if (!(helpPanel is SignatureTooltipPanel))
			{
				RenderUtils.SetAlpha(helpPanel.gameObject, newValue, includeInactive: true);
			}
		}
	}

	public void ShowKeywordHelp()
	{
		foreach (TooltipPanel tooltipPanel in m_tooltipPanels)
		{
			tooltipPanel.gameObject.SetActive(value: true);
		}
	}

	public void HideKeywordHelp()
	{
		GameState gameState = GameState.Get();
		if (gameState != null && gameState.GetBooleanGameOption(GameEntityOption.SHOW_CRAZY_KEYWORD_TOOLTIP) && TutorialKeywordManager.Get() != null)
		{
			TutorialKeywordManager.Get().HideKeywordHelp();
		}
		HideTooltipPanels();
	}

	public void HideTooltipPanels()
	{
		iTween.Stop(base.gameObject);
		foreach (TooltipPanel panel in m_tooltipPanels)
		{
			if (!(panel == null))
			{
				iTween.Stop(panel.gameObject);
				RenderUtils.SetAlpha(panel.gameObject, 0f, includeInactive: true);
				panel.gameObject.SetActive(value: false);
				if (!(panel is SignatureTooltipPanel))
				{
					m_tooltipPanelPool.Release(panel);
				}
			}
		}
	}

	public Card GetCard()
	{
		return m_card;
	}

	public Vector3 GetPositionOfTopPanel()
	{
		if (m_tooltipPanels.Count == 0)
		{
			return new Vector3(0f, 0f, 0f);
		}
		return m_tooltipPanels[0].transform.position;
	}

	public TooltipPanel CreateKeywordPanel(int i)
	{
		return UnityEngine.Object.Instantiate(m_tooltipPanelPrefab);
	}

	private void DestroyKeywordPanel(TooltipPanel panel)
	{
		if (panel != null)
		{
			UnityEngine.Object.Destroy(panel.gameObject);
		}
	}

	private void OnSceneUnloaded(SceneMgr.Mode prevMode, PegasusScene prevScene, object userData)
	{
		foreach (TooltipPanel tooltipPanel in m_tooltipPanels)
		{
			UnityEngine.Object.Destroy(tooltipPanel.gameObject);
		}
		m_tooltipPanels.Clear();
		m_tooltipPanelPool.Clear();
		UnityEngine.Object.Destroy(m_actor);
		m_actor = null;
		UnityEngine.Object.Destroy(m_card);
		m_card = null;
	}

	private GAME_TAG GetEmpowerTagByClass(TAG_CLASS tagClass)
	{
		GAME_TAG tag = GAME_TAG.EMPOWER;
		switch (tagClass)
		{
		case TAG_CLASS.PRIEST:
			tag = GAME_TAG.EMPOWER_PRIEST;
			break;
		case TAG_CLASS.ROGUE:
			tag = GAME_TAG.EMPOWER_ROGUE;
			break;
		case TAG_CLASS.SHAMAN:
			tag = GAME_TAG.EMPOWER_SHAMAN;
			break;
		case TAG_CLASS.WARLOCK:
			tag = GAME_TAG.EMPOWER_WARLOCK;
			break;
		case TAG_CLASS.WARRIOR:
			tag = GAME_TAG.EMPOWER_WARRIOR;
			break;
		}
		return tag;
	}

	private static TooltipBoneSource GetTooltipBoneSourceForOrientation(Orientation orientation)
	{
		TooltipBoneSource boneSource = TooltipBoneSource.INVALID;
		switch (orientation)
		{
		case Orientation.RightTop:
			boneSource = TooltipBoneSource.TOP_RIGHT;
			break;
		case Orientation.RightBottom:
			boneSource = TooltipBoneSource.BOTTOM_RIGHT;
			break;
		case Orientation.LeftMiddle:
			boneSource = TooltipBoneSource.BOTTOM_LEFT;
			break;
		case Orientation.LeftTop:
			boneSource = TooltipBoneSource.TOP_LEFT;
			break;
		}
		return boneSource;
	}
}
