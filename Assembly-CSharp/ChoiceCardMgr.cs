using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using Blizzard.T5.Game.Spells;
using UnityEngine;

[CustomEditClass]
public class ChoiceCardMgr : MonoBehaviour
{
	[Serializable]
	public class CommonData
	{
		public float m_FriendlyCardWidth = 2.85f;

		public float m_FriendlyCardWidthTrinket = 2.4f;

		public float m_OpponentCardWidth = 1.5f;

		public int m_MaxCardsBeforeAdjusting = 3;

		public PlatformDependentValue<float> m_FourCardScale = new PlatformDependentValue<float>(PlatformCategory.Screen)
		{
			PC = 1f,
			Tablet = 1f,
			Phone = 0.8f
		};

		public PlatformDependentValue<float> m_FiveCardScale = new PlatformDependentValue<float>(PlatformCategory.Screen)
		{
			PC = 0.85f,
			Tablet = 0.85f,
			Phone = 0.65f
		};

		public PlatformDependentValue<float> m_SixPlusCardScale = new PlatformDependentValue<float>(PlatformCategory.Screen)
		{
			PC = 0.7f,
			Tablet = 0.7f,
			Phone = 0.55f
		};

		public PlatformDependentValue<float> m_TrinketCardAdditionalScale = new PlatformDependentValue<float>(PlatformCategory.Screen)
		{
			PC = 1f,
			Tablet = 1f,
			Phone = 0.8f
		};
	}

	[Serializable]
	public class ChoiceData
	{
		public string m_FriendlyBoneName = "FriendlyChoice";

		public string m_BGTrinketFriendlyBoneName = "BGTrinketFriendlyChoice";

		public string m_OpponentBoneName = "OpponentChoice";

		public string m_BannerBoneName = "ChoiceBanner";

		public string m_ToggleChoiceButtonBoneName = "ToggleChoiceButton";

		public string m_ConfirmChoiceButtonBoneName = "ConfirmChoiceButton";

		public string m_BGTrinketConfirmChoiceButtonBoneName = "BGTrinketConfirmChoiceButton";

		public string m_BGTrinketToggleChoiceButtonBoneName = "BGTrinketToggleChoiceButton";

		public float m_MinShowTime = 1f;

		public Banner m_BannerPrefab;

		[CustomEditField(T = EditType.GAME_OBJECT)]
		public string m_ButtonPrefab;

		public GameObject m_MagicItemShopBackgroundPrefab;

		public GameObject m_xPrefab;

		public float m_CardShowTime = 0.2f;

		public float m_CardHideTime = 0.2f;

		public float m_UiShowTime = 0.5f;

		public float m_HorizontalPadding = 0.75f;

		public PlatformDependentValue<float> m_HorizontalPaddingTrinketMultiplier = new PlatformDependentValue<float>(PlatformCategory.Screen)
		{
			PC = 1f,
			Tablet = 1f,
			Phone = 0.8f
		};

		public PlatformDependentValue<float> m_HorizontalPaddingFourCards = new PlatformDependentValue<float>(PlatformCategory.Screen)
		{
			PC = 0.6f,
			Tablet = 0.5f,
			Phone = 0.4f
		};

		public PlatformDependentValue<float> m_HorizontalPaddingFiveCards = new PlatformDependentValue<float>(PlatformCategory.Screen)
		{
			PC = 0.3f,
			Tablet = 0.3f,
			Phone = 0.3f
		};

		public PlatformDependentValue<float> m_HorizontalPaddingSixPlusCards = new PlatformDependentValue<float>(PlatformCategory.Screen)
		{
			PC = 0.2f,
			Tablet = 0.2f,
			Phone = 0.2f
		};
	}

	[Serializable]
	public class SubOptionData
	{
		public string m_BoneName = "SubOption";

		public float m_AdjacentCardXOffset = 0.75f;

		public float m_PhoneMaxAdjacentCardXOffset = 0.1f;

		public float m_MinionParentXOffset = 0.9f;

		public float m_CardShowTime = 0.2f;
	}

	[Serializable]
	public class ChoiceEffectData
	{
		public bool m_AlwaysPlayEffect;

		public bool m_PlayOncePerCard;

		public Spell m_Spell;
	}

	[Serializable]
	public class TagSpecificChoiceEffect
	{
		public GAME_TAG m_Tag;

		public List<TagValueSpecificChoiceEffect> m_ValueSpellMap;
	}

	[Serializable]
	public class TagValueSpecificChoiceEffect
	{
		public int m_Value;

		public ChoiceEffectData m_ChoiceEffectData;
	}

	[Serializable]
	public class CardSpecificChoiceEffect
	{
		public string m_CardID;

		public ChoiceEffectData m_ChoiceEffectData;
	}

	[Serializable]
	public class TagPostChoiceEffect
	{
		public GAME_TAG m_Tag;

		public Spell m_SpellSelectedCards;

		public Spell m_SpellUnselectedCards;
	}

	[Serializable]
	public class StarshipUIData
	{
		public const string STARSHIP_LAUNCH_CARDID = "GDB_905";

		public const string STARSHIP_ABORT_LAUNCH_CARDID = "GDB_906";

		public Vector3 m_RotationPC;

		public Vector3 m_ScalePC;

		[Tooltip("this is relative to the bone ToggleChoiceButton")]
		public Vector3 m_PositionPC;

		public Vector3 m_RotationMobile;

		public Vector3 m_ScaleMobile;

		[Tooltip("this is relative to the bone ToggleChoiceButton_phone")]
		public Vector3 m_PositionMobile;
	}

	private class SubOptionState
	{
		public List<Card> m_cards = new List<Card>();

		public Card m_parentCard;
	}

	public struct TransformData
	{
		public Vector3 Position { get; set; }

		public Vector3 RotationAngles { get; set; }

		public Vector3 LocalScale { get; set; }
	}

	public class ChoiceState
	{
		public int m_choiceID;

		public bool m_isFriendly;

		public List<Card> m_cards = new List<Card>();

		public List<TransformData> m_cardTransforms = new List<TransformData>();

		public bool m_waitingToStart;

		public bool m_hasBeenRevealed;

		public bool m_hasBeenConcealed;

		public bool m_hideChosen;

		public int m_choiceActor;

		public PowerTaskList m_preTaskList;

		public int m_sourceEntityId;

		public List<Entity> m_chosenEntities;

		public Map<int, GameObject> m_xObjs;

		public List<Spell> m_choiceEffectSpells = new List<Spell>();

		public List<Spell> m_postChoiceSpells = new List<Spell>();

		public bool m_showFromDeck;

		public bool m_hideChoiceUI;

		public bool m_isSubOptionChoice;

		public bool m_isTitanAbility;

		public bool m_isMagicItemDiscover;

		public bool m_isLaunchpadAbility;
	}

	public CommonData m_CommonData = new CommonData();

	public ChoiceData m_ChoiceData = new ChoiceData();

	public StarshipUIData m_StarshipUIData = new StarshipUIData();

	public SubOptionData m_SubOptionData = new SubOptionData();

	public List<TagSpecificChoiceEffect> m_TagSpecificChoiceEffectData = new List<TagSpecificChoiceEffect>();

	public List<CardSpecificChoiceEffect> m_CardSpecificChoiceEffectData = new List<CardSpecificChoiceEffect>();

	public List<TagPostChoiceEffect> m_TagPostChoiceEffectData = new List<TagPostChoiceEffect>();

	private ChoiceEffectData m_DiscoverChoiceEffectData = new ChoiceEffectData();

	private ChoiceEffectData m_AdaptChoiceEffectData = new ChoiceEffectData();

	private ChoiceEffectData m_GearsChoiceEffectData = new ChoiceEffectData();

	private ChoiceEffectData m_DragonChoiceEffectData = new ChoiceEffectData();

	private ChoiceEffectData m_TrinketChoiceEffectData = new ChoiceEffectData();

	private static readonly Vector3 INVISIBLE_SCALE = new Vector3(0.0001f, 0.0001f, 0.0001f);

	private static ChoiceCardMgr s_instance;

	private SubOptionState m_subOptionState;

	private SubOptionState m_pendingCancelSubOptionState;

	private Dictionary<int, ChoiceState> m_choiceStateMap = new Dictionary<int, ChoiceState>();

	private Banner m_choiceBanner;

	private GameObject m_magicItemShopBackground;

	private NormalButton m_toggleChoiceButton;

	private NormalButton m_confirmChoiceButton;

	private bool m_friendlyChoicesShown;

	private bool m_restoreEnlargedHand;

	private ChoiceState m_lastShownChoiceState;

	private const string ALGALONS_VISION = "TTN_717t";

	private const float ALGALONS_CHOICE_CARDS_OFFSET_X = 2f;

	private List<int> m_starshipPiecesToView;

	private StarshipHUDManager m_starshipHUDManager;

	private void Awake()
	{
		s_instance = this;
		foreach (TagSpecificChoiceEffect tagSpecificChoiceEffect in m_TagSpecificChoiceEffectData)
		{
			switch (tagSpecificChoiceEffect.m_Tag)
			{
			case GAME_TAG.USE_DISCOVER_VISUALS:
				if (tagSpecificChoiceEffect.m_ValueSpellMap.Count > 0)
				{
					m_DiscoverChoiceEffectData = tagSpecificChoiceEffect.m_ValueSpellMap[0].m_ChoiceEffectData;
				}
				break;
			case GAME_TAG.ADAPT:
				if (tagSpecificChoiceEffect.m_ValueSpellMap.Count > 0)
				{
					m_AdaptChoiceEffectData = tagSpecificChoiceEffect.m_ValueSpellMap[0].m_ChoiceEffectData;
				}
				break;
			case GAME_TAG.GEARS:
				if (tagSpecificChoiceEffect.m_ValueSpellMap.Count > 0)
				{
					m_GearsChoiceEffectData = tagSpecificChoiceEffect.m_ValueSpellMap[0].m_ChoiceEffectData;
				}
				break;
			case GAME_TAG.GOOD_OL_GENERIC_FRIENDLY_DRAGON_DISCOVER_VISUALS:
				if (tagSpecificChoiceEffect.m_ValueSpellMap.Count > 0)
				{
					m_DragonChoiceEffectData = tagSpecificChoiceEffect.m_ValueSpellMap[0].m_ChoiceEffectData;
				}
				break;
			case GAME_TAG.BACON_IS_MAGIC_ITEM_DISCOVER:
				if (tagSpecificChoiceEffect.m_ValueSpellMap.Count > 0)
				{
					m_TrinketChoiceEffectData = tagSpecificChoiceEffect.m_ValueSpellMap[0].m_ChoiceEffectData;
				}
				break;
			}
		}
		m_starshipPiecesToView = new List<int>();
	}

	private void OnDestroy()
	{
		s_instance = null;
	}

	private void Start()
	{
		if (GameState.Get() == null)
		{
			Debug.LogError($"ChoiceCardMgr.Start() - GameState already Shutdown before ChoiceCardMgr was loaded.");
			return;
		}
		GameState.Get().RegisterEntityChoicesReceivedListener(OnEntityChoicesReceived);
		GameState.Get().RegisterEntitiesChosenReceivedListener(OnEntitiesChosenReceived);
		GameState.Get().RegisterGameOverListener(OnGameOver);
	}

	public static ChoiceCardMgr Get()
	{
		return s_instance;
	}

	public bool RestoreEnlargedHandAfterChoice()
	{
		return m_restoreEnlargedHand;
	}

	public Banner GetChoiceBanner()
	{
		return m_choiceBanner;
	}

	public GameObject GetChoiceBackground()
	{
		return m_magicItemShopBackground;
	}

	public NormalButton GetToggleButton()
	{
		return m_toggleChoiceButton;
	}

	public List<Card> GetFriendlyCards()
	{
		if (m_subOptionState != null)
		{
			return m_subOptionState.m_cards;
		}
		int friendlyPlayerId = GameState.Get().GetFriendlyPlayerId();
		if (m_choiceStateMap.TryGetValue(friendlyPlayerId, out var choiceState))
		{
			return choiceState.m_cards;
		}
		return null;
	}

	private void EnableConfirmButton(bool enable)
	{
		if (m_confirmChoiceButton == null)
		{
			Log.All.PrintWarning("Enable Confirm Button called when confirm button is null");
			return;
		}
		PlayMakerFSM fsm = m_confirmChoiceButton.GetComponentInChildren<PlayMakerFSM>();
		if (fsm != null && fsm.FsmVariables.GetFsmBool("enabled").Value != enable)
		{
			fsm.FsmVariables.GetFsmBool("enabled").Value = enable;
			fsm.SendEvent("Birth");
		}
	}

	public void ChooseBGTrinket(Entity entity)
	{
		if (!IsFriendlyMagicItemDiscover())
		{
			return;
		}
		foreach (Entity chosenTrinket in GameState.Get().GetChosenEntities())
		{
			ManaCrystalMgr.Get().CancelAllProposedMana(chosenTrinket);
		}
		GameState.Get().ClearFriendlyChoicesList();
		List<Card> cards = GetFriendlyCards();
		if (cards != null)
		{
			foreach (Card card in cards)
			{
				Actor actor = card.GetActor();
				if (!(actor == null))
				{
					ActorStateMgr actorStateMgr = actor.GetActorStateMgr();
					if (actorStateMgr != null)
					{
						actorStateMgr.ChangeState((card.GetEntity() == entity) ? ActorStateType.CARD_SELECTED : ActorStateType.NONE);
					}
				}
			}
		}
		Network.Get().SendMulliganChooseOneTentativeSelect(entity.GetEntityId(), isConfirmation: false);
		if (m_magicItemShopBackground != null)
		{
			BaconTrinketBacker trinketBacker = m_magicItemShopBackground.GetComponent<BaconTrinketBacker>();
			if (trinketBacker != null)
			{
				trinketBacker.UpdateCoinText(entity.GetTag(GAME_TAG.COST));
			}
		}
		ManaCrystalMgr.Get().ProposeManaCrystalUsage(entity);
		EnableConfirmButton(entity != null);
	}

	public void OnChooseOneTentativeSelection(Network.MulliganChooseOneTentativeSelection selection)
	{
		Debug.LogWarning($"On ChooseOneTentativeSelection {selection}");
	}

	public bool CardIsFirstChoice(Card card)
	{
		List<Card> choices = GetFriendlyCards();
		if (choices == null)
		{
			string cardObjectName = ((card == null) ? "" : card.name);
			Debug.LogErrorFormat("ChoiceCardMgr.CardIsFirstChoice() - choices is null. card parameter = '" + cardObjectName + "'");
			return false;
		}
		if (choices.Count == 0)
		{
			return false;
		}
		Card firstChoice = choices[0];
		if (firstChoice == null)
		{
			string cardObjectName2 = ((card == null) ? "" : card.name);
			Debug.LogErrorFormat("ChoiceCardMgr.CardIsFirstChoice() - firstChoice is null. card parameter = '" + cardObjectName2 + "'");
			return false;
		}
		if (card == null)
		{
			Debug.LogErrorFormat("ChoiceCardMgr.CardIsFirstChoice() - card parameter is null. firstChoice = '" + firstChoice.name + "'");
			return false;
		}
		return firstChoice.GetEntity()?.GetEntityId() == card.GetEntity()?.GetEntityId();
	}

	public bool IsShown()
	{
		if (m_subOptionState != null)
		{
			return true;
		}
		if (m_choiceStateMap.Count > 0)
		{
			return true;
		}
		return false;
	}

	public bool IsFriendlyShown()
	{
		if (m_subOptionState != null)
		{
			return true;
		}
		int friendlyPlayerId = GameState.Get().GetFriendlyPlayerId();
		if (m_choiceStateMap.ContainsKey(friendlyPlayerId))
		{
			return true;
		}
		return false;
	}

	public bool HasSubOption()
	{
		return m_subOptionState != null;
	}

	public Card GetSubOptionParentCard()
	{
		if (m_subOptionState != null)
		{
			return m_subOptionState.m_parentCard;
		}
		return null;
	}

	public void ClearSubOptions()
	{
		int playerID = (m_subOptionState?.m_parentCard?.GetEntity()?.GetControllerId()).GetValueOrDefault();
		m_subOptionState = null;
		ChoiceState choiceState = GetChoiceStateForPlayer(playerID);
		DeactivateChoiceEffects(choiceState);
		OnFinishedConcealChoices(playerID);
	}

	public bool ShowSubOptions(Card parentCard, List<int> dynamicSubOptionEntitIds = null)
	{
		Entity parentEntity = parentCard.GetEntity();
		if (parentEntity != null && !parentEntity.IsControlledByFriendlySidePlayer())
		{
			return false;
		}
		if (dynamicSubOptionEntitIds != null)
		{
			for (int i = 0; i < dynamicSubOptionEntitIds.Count; i++)
			{
				Entity toAdd = GameState.Get().GetEntity(dynamicSubOptionEntitIds[i]);
				if (toAdd != null)
				{
					parentEntity.AddSubCard(toAdd);
				}
			}
		}
		m_subOptionState = new SubOptionState();
		m_subOptionState.m_parentCard = parentCard;
		if (parentEntity != null && !parentCard.GetEntity().IsTitan())
		{
			parentCard.ActivateChooseOneEffects();
		}
		StartCoroutine(WaitThenShowSubOptions());
		return true;
	}

	public void QuenePendingCancelSubOptions()
	{
		m_pendingCancelSubOptionState = m_subOptionState;
	}

	public bool HasPendingCancelSubOptions()
	{
		if (m_pendingCancelSubOptionState != null)
		{
			return m_pendingCancelSubOptionState == m_subOptionState;
		}
		return false;
	}

	public void ClearPendingCancelSubOptions()
	{
		m_pendingCancelSubOptionState = null;
	}

	public void ForceUpdateAllSubcards()
	{
		GameState.Get()?.ForceUpdateAllSubcards();
	}

	public bool IsWaitingToShowSubOptions()
	{
		if (!HasSubOption())
		{
			return false;
		}
		Entity parentEntity = m_subOptionState.m_parentCard.GetEntity();
		Player controllingPlayer = parentEntity.GetController();
		Zone curZone = m_subOptionState.m_parentCard.GetZone();
		if (parentEntity.IsMinion())
		{
			if (curZone.m_ServerTag == TAG_ZONE.SETASIDE)
			{
				return false;
			}
			ZonePlay controllerPlayZone = controllingPlayer.GetBattlefieldZone();
			if (curZone != controllerPlayZone)
			{
				return true;
			}
			if (m_subOptionState.m_parentCard.GetZonePosition() == 0)
			{
				return true;
			}
		}
		if (parentEntity.IsHero())
		{
			ZoneHero controllerHeroZone = controllingPlayer.GetHeroZone();
			if (curZone != controllerHeroZone)
			{
				return true;
			}
			if (!m_subOptionState.m_parentCard.IsActorReady())
			{
				return true;
			}
		}
		if (!parentEntity.HasSubCards())
		{
			ForceUpdateAllSubcards();
			return true;
		}
		return false;
	}

	public void CancelSubOptions()
	{
		if (!HasSubOption())
		{
			return;
		}
		Entity parentEntity = m_subOptionState.m_parentCard.GetEntity();
		Card parentCard = parentEntity.GetCard();
		for (int i = 0; i < m_subOptionState.m_cards.Count; i++)
		{
			Spell subOptionSpell = parentCard.GetSubOptionSpell(i, 0, loadIfNeeded: false);
			if ((bool)subOptionSpell)
			{
				SpellStateType activeState = subOptionSpell.GetActiveState();
				if (activeState != 0 && activeState != SpellStateType.CANCEL)
				{
					subOptionSpell.ActivateState(SpellStateType.CANCEL);
				}
			}
		}
		parentCard.ActivateHandStateSpells();
		if (parentEntity.IsHeroPower() || parentEntity.IsGameModeButton())
		{
			parentEntity.SetTagAndHandleChange(GAME_TAG.EXHAUSTED, 0);
		}
		HideSubOptions();
	}

	public void OnSubOptionClicked(Entity chosenEntity)
	{
		if (HasSubOption())
		{
			HideSubOptions(chosenEntity);
		}
	}

	public bool HasChoices()
	{
		return m_choiceStateMap.Count > 0;
	}

	public bool HasChoices(int playerId)
	{
		return m_choiceStateMap.ContainsKey(playerId);
	}

	public ChoiceState GetFriendlyChoiceState()
	{
		Player player = GameState.Get().GetFriendlySidePlayer();
		if (player == null)
		{
			return null;
		}
		return GetChoiceStateForPlayer(player.GetPlayerId());
	}

	public bool IsFriendlyMagicItemDiscover()
	{
		return GetFriendlyChoiceState()?.m_isMagicItemDiscover ?? false;
	}

	public ChoiceState GetChoiceStateForPlayer(int playerId)
	{
		if (!HasChoices(playerId))
		{
			return null;
		}
		return m_choiceStateMap[playerId];
	}

	public bool HasFriendlyChoices()
	{
		int friendlyPlayerId = GameState.Get().GetFriendlyPlayerId();
		return HasChoices(friendlyPlayerId);
	}

	public PowerTaskList GetPreChoiceTaskList(int playerId)
	{
		if (m_choiceStateMap.TryGetValue(playerId, out var state))
		{
			return state.m_preTaskList;
		}
		return null;
	}

	public PowerTaskList GetFriendlyPreChoiceTaskList()
	{
		int friendlyPlayerId = GameState.Get().GetFriendlyPlayerId();
		return GetPreChoiceTaskList(friendlyPlayerId);
	}

	public bool IsWaitingToStartChoices(int playerId)
	{
		if (m_choiceStateMap.TryGetValue(playerId, out var state))
		{
			return state.m_waitingToStart;
		}
		return false;
	}

	public bool IsFriendlyWaitingToStartChoices()
	{
		int friendlyPlayerId = GameState.Get().GetFriendlyPlayerId();
		return IsWaitingToStartChoices(friendlyPlayerId);
	}

	public void OnSendChoices(Network.EntityChoices choicePacket, List<Entity> chosenEntities)
	{
		if (choicePacket.ChoiceType == CHOICE_TYPE.GENERAL)
		{
			int friendlyPlayerId = GameState.Get().GetFriendlyPlayerId();
			if (!m_choiceStateMap.TryGetValue(friendlyPlayerId, out var state))
			{
				Error.AddDevFatal("ChoiceCardMgr.OnSendChoices() - there is no ChoiceState for friendly player {0}", friendlyPlayerId);
			}
			else
			{
				state.m_chosenEntities = new List<Entity>(chosenEntities);
				ConcealChoicesFromInput(friendlyPlayerId, state);
			}
		}
	}

	public void OnChosenEntityAdded(Entity entity)
	{
		if (entity == null)
		{
			Log.Gameplay.PrintError("ChoiceCardMgr.OnChosenEntityAdded(): null entity passed!");
			return;
		}
		Network.EntityChoices choicePacket = GameState.Get().GetFriendlyEntityChoices();
		if (choicePacket == null || choicePacket.IsSingleChoice() || !m_choiceStateMap.ContainsKey(GameState.Get().GetFriendlyPlayerId()))
		{
			return;
		}
		ChoiceState choiceState = m_choiceStateMap[GameState.Get().GetFriendlyPlayerId()];
		if (choiceState.m_xObjs == null)
		{
			Log.Gameplay.PrintError("ChoiceCardMgr.OnChosenEntityAdded(): ChoiceState does not have an m_xObjs map!");
		}
		else if (!choiceState.m_xObjs.ContainsKey(entity.GetEntityId()))
		{
			Card card = entity.GetCard();
			if (card == null)
			{
				Log.Gameplay.PrintError("ChoiceCardMgr.OnChosenEntityAdded(): Entity does not have a card!");
				return;
			}
			GameObject newX = UnityEngine.Object.Instantiate(m_ChoiceData.m_xPrefab);
			TransformUtil.AttachAndPreserveLocalTransform(newX.transform, card.transform);
			newX.transform.localRotation = Quaternion.identity;
			newX.transform.localPosition = Vector3.zero;
			choiceState.m_xObjs.Add(entity.GetEntityId(), newX);
		}
	}

	public void OnChosenEntityRemoved(Entity entity)
	{
		if (entity == null)
		{
			Log.Gameplay.PrintError("ChoiceCardMgr.OnChosenEntityRemoved(): null entity passed!");
			return;
		}
		Network.EntityChoices choicePacket = GameState.Get().GetFriendlyEntityChoices();
		if (choicePacket == null || choicePacket.IsSingleChoice() || !m_choiceStateMap.ContainsKey(GameState.Get().GetFriendlyPlayerId()))
		{
			return;
		}
		ChoiceState choiceState = m_choiceStateMap[GameState.Get().GetFriendlyPlayerId()];
		if (choiceState.m_xObjs == null)
		{
			Log.Gameplay.PrintError("ChoiceCardMgr.OnChosenEntityRemoved(): ChoiceState does not have an m_xObjs map!");
			return;
		}
		int entityID = entity.GetEntityId();
		if (choiceState.m_xObjs.ContainsKey(entityID))
		{
			GameObject xObj = choiceState.m_xObjs[entityID];
			choiceState.m_xObjs.Remove(entityID);
			UnityEngine.Object.Destroy(xObj);
		}
	}

	private void OnEntityChoicesReceived(Network.EntityChoices choices, PowerTaskList preChoiceTaskList, object userData)
	{
		if (choices.ChoiceType == CHOICE_TYPE.GENERAL)
		{
			StartCoroutine(WaitThenStartChoices(choices, preChoiceTaskList));
		}
	}

	private bool OnEntitiesChosenReceived(Network.EntitiesChosen chosen, object userData)
	{
		if (chosen.ChoiceType != CHOICE_TYPE.GENERAL)
		{
			return false;
		}
		StartCoroutine(WaitThenConcealChoicesFromPacket(chosen));
		return true;
	}

	private void OnGameOver(TAG_PLAYSTATE playState, object userData)
	{
		StopAllCoroutines();
		CancelSubOptions();
		CancelChoices();
	}

	private IEnumerator WaitThenStartChoices(Network.EntityChoices choices, PowerTaskList preChoiceTaskList)
	{
		int playerId = choices.PlayerId;
		ChoiceState state = new ChoiceState();
		if (m_choiceStateMap.ContainsKey(playerId))
		{
			m_choiceStateMap[playerId] = state;
		}
		else
		{
			m_choiceStateMap.Add(playerId, state);
		}
		state.m_waitingToStart = true;
		state.m_hasBeenConcealed = false;
		state.m_hasBeenRevealed = false;
		state.m_choiceID = choices.ID;
		state.m_hideChosen = choices.HideChosen;
		state.m_sourceEntityId = choices.Source;
		state.m_preTaskList = preChoiceTaskList;
		state.m_xObjs = new Map<int, GameObject>();
		Entity sourceEntity = GameState.Get().GetEntity(choices.Source);
		if (sourceEntity != null)
		{
			state.m_showFromDeck = sourceEntity.HasTag(GAME_TAG.SHOW_DISCOVER_FROM_DECK);
			state.m_isMagicItemDiscover = sourceEntity.HasTag(GAME_TAG.BACON_IS_MAGIC_ITEM_DISCOVER);
		}
		if (state.m_isMagicItemDiscover)
		{
			m_ChoiceData.m_CardHideTime = 0f;
			m_ChoiceData.m_CardShowTime = 0f;
		}
		else
		{
			m_ChoiceData.m_CardHideTime = 0.2f;
			m_ChoiceData.m_CardShowTime = 0.2f;
		}
		PowerProcessor powerProcessor = GameState.Get().GetPowerProcessor();
		if (powerProcessor.HasTaskList(state.m_preTaskList) && (GameState.Get()?.GameScenarioAllowsPowerPrinting() ?? true))
		{
			Log.Power.Print("ChoiceCardMgr.WaitThenShowChoices() - id={0} WAIT for taskList {1}", choices.ID, preChoiceTaskList.GetId());
		}
		while (powerProcessor.HasTaskList(state.m_preTaskList))
		{
			yield return null;
		}
		HistoryManager historyManager = HistoryManager.Get();
		if (historyManager.HasBigCard() && historyManager.GetCurrentBigCard().GetEntity().GetEntityId() == state.m_sourceEntityId)
		{
			historyManager.HandleClickOnBigCard(historyManager.GetCurrentBigCard());
		}
		if (GameState.Get()?.GameScenarioAllowsPowerPrinting() ?? true)
		{
			Log.Power.Print("ChoiceCardMgr.WaitThenShowChoices() - id={0} BEGIN", choices.ID);
		}
		List<Card> linkedChoiceCards = new List<Card>();
		Entity source = GameState.Get().GetEntity(state.m_sourceEntityId);
		for (int i = 0; i < choices.Entities.Count; i++)
		{
			int entityId = choices.Entities[i];
			Entity entity = GameState.Get().GetEntity(entityId);
			Card card = entity.GetCard();
			if (card == null)
			{
				Error.AddDevFatal("ChoiceCardMgr.WaitThenShowChoices() - Entity {0} (option {1}) has no Card", entity, i);
				continue;
			}
			if (entity.HasTag(GAME_TAG.LINKED_ENTITY))
			{
				int linkedEntityId = entity.GetRealTimeLinkedEntityId();
				Entity linkedEntity = GameState.Get().GetEntity(linkedEntityId);
				if (linkedEntity != null && linkedEntity.GetCard() != null)
				{
					linkedChoiceCards.Add(linkedEntity.GetCard());
				}
			}
			state.m_cards.Add(card);
			StartCoroutine(LoadChoiceCardActors(source, entity, card));
		}
		int i2 = 0;
		while (i2 < linkedChoiceCards.Count)
		{
			Card linkedCard = linkedChoiceCards[i2];
			while (linkedCard != null && !linkedCard.IsActorReady())
			{
				yield return null;
			}
			int num = i2 + 1;
			i2 = num;
		}
		i2 = 0;
		while (i2 < state.m_cards.Count)
		{
			Card linkedCard = state.m_cards[i2];
			while (!IsChoiceCardReady(linkedCard))
			{
				yield return null;
			}
			int num = i2 + 1;
			i2 = num;
		}
		int friendlyPlayerId = GameState.Get().GetFriendlyPlayerId();
		bool friendly = playerId == friendlyPlayerId;
		if (friendly)
		{
			while (GameState.Get().IsTurnStartManagerBlockingInput())
			{
				if (GameState.Get().IsTurnStartManagerActive())
				{
					TurnStartManager.Get().NotifyOfStartOfTurnChoice();
				}
				yield return null;
			}
		}
		state.m_isFriendly = friendly;
		state.m_waitingToStart = false;
		PopulateTransformDatas(state);
		StartChoices(state);
	}

	private IEnumerator LoadChoiceCardActors(Entity source, Entity entity, Card card)
	{
		while (!IsEntityReady(entity))
		{
			yield return null;
		}
		card.HideCard();
		while (!IsCardReady(card))
		{
			yield return null;
		}
		CHOICE_ACTOR choiceActor = CHOICE_ACTOR.CARD;
		if (source.HasTag(GAME_TAG.CHOICE_ACTOR_TYPE))
		{
			choiceActor = (CHOICE_ACTOR)source.GetTag(GAME_TAG.CHOICE_ACTOR_TYPE);
		}
		if ((uint)choiceActor > 1u && choiceActor == CHOICE_ACTOR.HERO)
		{
			LoadHeroChoiceCardActor(source, entity, card);
			card.ActivateHandStateSpells();
		}
		else
		{
			card.ForceLoadHandActor();
			card.ActivateHandStateSpells();
		}
	}

	private void LoadHeroChoiceCardActor(Entity source, Entity entity, Card card)
	{
		GameObject actorGameObject = AssetLoader.Get().InstantiatePrefab("Choose_Hero.prefab:1834beb8747ef06439f3a1b86a35ff3d", AssetLoadingOptions.IgnorePrefabPosition);
		if (actorGameObject == null)
		{
			Log.Gameplay.PrintWarning(string.Format("ChoiceCardManager.LoadHeroChoiceActor() - FAILED to load actor \"{0}\"", "Choose_Hero.prefab:1834beb8747ef06439f3a1b86a35ff3d"));
			return;
		}
		Actor heroCardActor = actorGameObject.GetComponent<Actor>();
		if (heroCardActor == null)
		{
			Log.Gameplay.PrintWarning(string.Format("ChoiceCardManager.LoadHeroChoiceActor() - ERROR actor \"{0}\" has no Actor component", "Choose_Hero.prefab:1834beb8747ef06439f3a1b86a35ff3d"));
			return;
		}
		if (card.GetActor() != null)
		{
			card.GetActor().Destroy();
		}
		card.SetActor(heroCardActor);
		heroCardActor.SetCard(card);
		heroCardActor.SetCardDefFromCard(card);
		heroCardActor.SetPremium(card.GetPremium());
		heroCardActor.UpdateAllComponents();
		heroCardActor.SetEntity(entity);
		heroCardActor.UpdateAllComponents();
		heroCardActor.SetUnlit();
		LayerUtils.SetLayer(heroCardActor.gameObject, base.gameObject.layer, null);
		heroCardActor.GetMeshRenderer().gameObject.layer = 8;
		ConfigureHeroChoiceActor(source, entity, heroCardActor as HeroChoiceActor);
	}

	private void ConfigureHeroChoiceActor(Entity source, Entity entity, HeroChoiceActor actor)
	{
		if (actor == null)
		{
			return;
		}
		if (entity == null || source == null)
		{
			actor.SetNameTextActive(active: false);
			return;
		}
		CHOICE_NAME_DISPLAY nameDisplay = CHOICE_NAME_DISPLAY.INVALID;
		if (source.HasTag(GAME_TAG.CHOICE_NAME_DISPLAY_TYPE))
		{
			nameDisplay = (CHOICE_NAME_DISPLAY)source.GetTag(GAME_TAG.CHOICE_NAME_DISPLAY_TYPE);
		}
		switch (nameDisplay)
		{
		case CHOICE_NAME_DISPLAY.HERO:
			actor.SetNameText(entity.GetName());
			actor.SetNameTextActive(active: true);
			break;
		case CHOICE_NAME_DISPLAY.PLAYER:
		{
			int playerId = entity.GetTag(GAME_TAG.PLAYER_ID);
			if (playerId == 0)
			{
				playerId = entity.GetTag(GAME_TAG.PLAYER_ID_LOOKUP);
			}
			actor.SetNameText(GameState.Get().GetGameEntity().GetBestNameForPlayer(playerId));
			actor.SetNameTextActive(active: true);
			break;
		}
		default:
			actor.SetNameTextActive(active: false);
			break;
		}
	}

	private bool IsChoiceCardReady(Card card)
	{
		Entity entity = card.GetEntity();
		if (!IsEntityReady(entity))
		{
			return false;
		}
		if (!IsCardReady(card))
		{
			return false;
		}
		if (!IsCardActorReady(card))
		{
			return false;
		}
		return true;
	}

	public void PopulateTransformDatas(ChoiceState state)
	{
		bool isFriendly = state.m_isFriendly;
		state.m_cardTransforms.Clear();
		int cardCount = state.m_cards.Count;
		float horizontalPadding = m_ChoiceData.m_HorizontalPadding;
		if (isFriendly && cardCount > m_CommonData.m_MaxCardsBeforeAdjusting)
		{
			horizontalPadding = GetPaddingForCardCount(cardCount);
		}
		if (state.m_isMagicItemDiscover)
		{
			horizontalPadding *= (float)m_ChoiceData.m_HorizontalPaddingTrinketMultiplier;
		}
		float width = ((!isFriendly) ? m_CommonData.m_OpponentCardWidth : (state.m_isMagicItemDiscover ? m_CommonData.m_FriendlyCardWidthTrinket : m_CommonData.m_FriendlyCardWidth));
		float wantedScale = 1f;
		if (isFriendly && cardCount > m_CommonData.m_MaxCardsBeforeAdjusting)
		{
			wantedScale = GetScaleForCardCount(cardCount);
			if (state.m_isMagicItemDiscover)
			{
				wantedScale *= (float)m_CommonData.m_TrinketCardAdditionalScale;
			}
			width *= wantedScale;
		}
		float halfWidth = 0.5f * width;
		float totalWidth = width * (float)cardCount + horizontalPadding * (float)(cardCount - 1);
		float halfTotalWidth = 0.5f * totalWidth;
		string boneName = ((!isFriendly) ? m_ChoiceData.m_OpponentBoneName : (state.m_isMagicItemDiscover ? m_ChoiceData.m_BGTrinketFriendlyBoneName : m_ChoiceData.m_FriendlyBoneName));
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			boneName += "_phone";
		}
		Transform obj = Board.Get().FindBone(boneName);
		Vector3 centerPos = obj.position;
		Vector3 centerBoneRotationAngles = obj.rotation.eulerAngles;
		Vector3 centerBoneScale = obj.localScale;
		float slotCenterX = centerPos.x - halfTotalWidth + halfWidth;
		Entity sourceEntity = GameState.Get().GetEntity(state.m_sourceEntityId);
		string cardID = "";
		if (sourceEntity != null)
		{
			cardID = sourceEntity.GetCardId();
		}
		for (int i = 0; i < cardCount; i++)
		{
			TransformData transformData = default(TransformData);
			Vector3 pos = default(Vector3);
			pos.x = slotCenterX;
			if ((bool)UniversalInputManager.UsePhoneUI && cardID == "TTN_717t")
			{
				pos.x = slotCenterX + 2f;
			}
			pos.y = centerPos.y;
			pos.z = centerPos.z;
			transformData.Position = pos;
			Vector3 scale = centerBoneScale;
			scale.x *= wantedScale;
			scale.y *= wantedScale;
			scale.z *= wantedScale;
			transformData.LocalScale = scale;
			transformData.RotationAngles = centerBoneRotationAngles;
			state.m_cardTransforms.Add(transformData);
			slotCenterX += width + horizontalPadding;
		}
	}

	private float GetScaleForCardCount(int cardCount)
	{
		if (cardCount <= m_CommonData.m_MaxCardsBeforeAdjusting)
		{
			return 1f;
		}
		return cardCount switch
		{
			4 => m_CommonData.m_FourCardScale, 
			5 => m_CommonData.m_FiveCardScale, 
			_ => m_CommonData.m_SixPlusCardScale, 
		};
	}

	private float GetPaddingForCardCount(int cardCount)
	{
		if (cardCount <= m_CommonData.m_MaxCardsBeforeAdjusting)
		{
			return m_ChoiceData.m_HorizontalPadding;
		}
		return cardCount switch
		{
			4 => m_ChoiceData.m_HorizontalPaddingFourCards, 
			5 => m_ChoiceData.m_HorizontalPaddingFiveCards, 
			_ => m_ChoiceData.m_HorizontalPaddingSixPlusCards, 
		};
	}

	public ChoiceState GetLastChoiceState()
	{
		return m_lastShownChoiceState;
	}

	public string GetToggleButtonBoneName()
	{
		return m_ChoiceData.m_ToggleChoiceButtonBoneName;
	}

	private void StartChoices(ChoiceState state)
	{
		m_lastShownChoiceState = state;
		int cardCount = state.m_cards.Count;
		for (int i = 0; i < cardCount; i++)
		{
			Card card = state.m_cards[i];
			TransformData transformData = state.m_cardTransforms[i];
			card.transform.position = transformData.Position;
			card.transform.rotation = Quaternion.Euler(transformData.RotationAngles);
			card.transform.localScale = transformData.LocalScale;
		}
		RevealChoiceCards(state);
	}

	private void RevealChoiceCards(ChoiceState state)
	{
		ISpell customChoiceRevealSpell = GetCustomChoiceRevealSpell(state);
		if (customChoiceRevealSpell != null)
		{
			RevealChoiceCardsUsingCustomSpell(customChoiceRevealSpell, state);
		}
		else
		{
			DefaultRevealChoiceCards(state);
		}
		CorpseCounter.UpdateTextAll();
	}

	private void DefaultRevealChoiceCards(ChoiceState choiceState)
	{
		bool isFriendly = choiceState.m_isFriendly;
		if (isFriendly)
		{
			ShowChoiceUi(choiceState);
		}
		ShowChoiceCards(choiceState, isFriendly);
		choiceState.m_hasBeenRevealed = true;
	}

	private void ShowChoiceCards(ChoiceState state, bool friendly)
	{
		StartCoroutine(PlayCardAnimation(state, friendly));
	}

	private void GetDeckTransform(ZoneDeck deckZone, out Vector3 startPos, out Vector3 startRot, out Vector3 startScale)
	{
		Actor deckActor = deckZone.GetThicknessForLayout();
		startPos = deckActor.GetMeshRenderer().bounds.center + Card.IN_DECK_OFFSET;
		startRot = Card.IN_DECK_ANGLES;
		startScale = Card.IN_DECK_SCALE;
	}

	private IEnumerator PlayCardAnimation(ChoiceState state, bool friendly)
	{
		if (state.m_showFromDeck)
		{
			state.m_showFromDeck = false;
			ZoneDeck deckZone = GameState.Get().GetEntity(state.m_sourceEntityId).GetController()
				.GetDeckZone();
			GetDeckTransform(deckZone, out var deckPos, out var deckRot, out var deckScale);
			float timingBonus = 0.1f;
			int cardCount = state.m_cards.Count;
			int i = 0;
			while (i < cardCount)
			{
				Card card = state.m_cards[i];
				card.ShowCard();
				GameObject cardObject = card.gameObject;
				cardObject.transform.position = deckPos;
				cardObject.transform.rotation = Quaternion.Euler(deckRot);
				cardObject.transform.localScale = deckScale;
				TransformData transformData = state.m_cardTransforms[i];
				iTween.Stop(cardObject);
				Vector3[] drawPath = new Vector3[3]
				{
					cardObject.transform.position,
					new Vector3(cardObject.transform.position.x, cardObject.transform.position.y + 3.6f, cardObject.transform.position.z),
					transformData.Position
				};
				iTween.MoveTo(cardObject, iTween.Hash("path", drawPath, "time", MulliganManager.ANIMATION_TIME_DEAL_CARD, "easetype", iTween.EaseType.easeInSineOutExpo));
				iTween.ScaleTo(cardObject, MulliganManager.FRIENDLY_PLAYER_CARD_SCALE, MulliganManager.ANIMATION_TIME_DEAL_CARD);
				iTween.RotateTo(cardObject, iTween.Hash("rotation", new Vector3(0f, 0f, 0f), "time", MulliganManager.ANIMATION_TIME_DEAL_CARD, "delay", MulliganManager.ANIMATION_TIME_DEAL_CARD / 16f));
				yield return new WaitForSeconds(0.04f);
				SoundManager.Get().LoadAndPlay("FX_GameStart09_CardsOntoTable.prefab:da502e035813b5742a04d2ef4f588255", cardObject);
				yield return new WaitForSeconds(0.05f + timingBonus);
				timingBonus = 0f;
				int num = i + 1;
				i = num;
			}
		}
		else
		{
			int cardCount2 = state.m_cards.Count;
			for (int j = 0; j < cardCount2; j++)
			{
				Card card2 = state.m_cards[j];
				TransformData transformData2 = state.m_cardTransforms[j];
				card2.ShowCard();
				card2.transform.localScale = INVISIBLE_SCALE;
				iTween.Stop(card2.gameObject);
				iTween.RotateTo(card2.gameObject, transformData2.RotationAngles, m_ChoiceData.m_CardShowTime);
				iTween.ScaleTo(card2.gameObject, transformData2.LocalScale, m_ChoiceData.m_CardShowTime);
				iTween.MoveTo(card2.gameObject, transformData2.Position, m_ChoiceData.m_CardShowTime);
				ActivateChoiceCardStateSpells(card2);
			}
		}
		PlayChoiceEffects(state, friendly);
	}

	private void PlayChoiceEffects(ChoiceState state, bool friendly)
	{
		if (!friendly)
		{
			return;
		}
		Entity sourceEntity = GameState.Get().GetEntity(state.m_sourceEntityId);
		if (sourceEntity == null)
		{
			return;
		}
		ChoiceEffectData effectData = GetChoiceEffectDataForCard(sourceEntity.GetCard());
		if (effectData == null || effectData.m_Spell == null || (state.m_hasBeenRevealed && !effectData.m_AlwaysPlayEffect))
		{
			return;
		}
		ISpellCallbackHandler<Spell>.StateFinishedCallback OnChoiceEffectStateFinished = delegate(Spell spell, SpellStateType prevStateType, object userData)
		{
			if (spell.GetActiveState() == SpellStateType.NONE)
			{
				SpellManager.Get().ReleaseSpell(spell);
			}
		};
		if (effectData.m_PlayOncePerCard)
		{
			foreach (Card card in state.m_cards)
			{
				Spell spellInstance = SpellManager.Get().GetSpell(effectData.m_Spell);
				TransformUtil.AttachAndPreserveLocalTransform(spellInstance.transform, card.GetActor().transform);
				spellInstance.AddStateFinishedCallback(OnChoiceEffectStateFinished);
				spellInstance.Activate();
				state.m_choiceEffectSpells.Add(spellInstance);
			}
			return;
		}
		Spell spellInstance2 = SpellManager.Get().GetSpell(effectData.m_Spell);
		spellInstance2.AddStateFinishedCallback(OnChoiceEffectStateFinished);
		spellInstance2.Activate();
		state.m_choiceEffectSpells.Add(spellInstance2);
	}

	private void ActivateChoiceCardStateSpells(Card card)
	{
		Actor actor = card.GetActor();
		if (!(actor != null))
		{
			return;
		}
		Entity entity = card.GetEntity();
		if (entity.HasTag(GAME_TAG.BACON_SHOW_COST_ON_DISCOVER))
		{
			actor.SetShowCostOverride();
			actor.UpdateTextComponents(entity);
		}
		bool useTechLevelManaGem = actor.UseTechLevelManaGem();
		bool considerCoinManaGem = entity.HasTag(GAME_TAG.BACON_SHOW_COST_ON_DISCOVER) || !useTechLevelManaGem;
		if (useTechLevelManaGem)
		{
			Spell techLevelSpell = actor.GetSpell(SpellType.TECH_LEVEL_MANA_GEM);
			if (techLevelSpell != null && entity != null)
			{
				techLevelSpell.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmInt("TechLevel").Value = entity.GetTechLevel();
				techLevelSpell.ActivateState(SpellStateType.BIRTH);
			}
		}
		if (considerCoinManaGem)
		{
			actor.ReleaseSpell(SpellType.TECH_LEVEL_MANA_GEM);
			if (actor.UseCoinManaGemForChoiceCard())
			{
				actor.ActivateSpellBirthState(SpellType.COIN_MANA_GEM);
			}
			else
			{
				actor.ReleaseSpell(SpellType.COIN_MANA_GEM);
			}
		}
		bool showPairTripleVFX = true;
		ChoiceState data = GetChoiceStateForPlayer(GameState.Get().GetFriendlyPlayerId());
		if (data != null)
		{
			Entity sourceEntity = GameState.Get().GetEntity(data.m_sourceEntityId);
			if (sourceEntity != null)
			{
				showPairTripleVFX = !sourceEntity.HasTag(GAME_TAG.BACON_DONT_SHOW_PAIR_TRIPLE_DISCOVER_VFX);
			}
		}
		if (showPairTripleVFX)
		{
			if (entity.HasTag(GAME_TAG.BACON_TRIPLE_CANDIDATE))
			{
				actor.ActivateSpellBirthState(SpellType.BACON_TRIPLE_CANDIDATE);
			}
			if (entity.HasTag(GAME_TAG.BACON_PAIR_CANDIDATE))
			{
				actor.ActivateSpellBirthState(SpellType.BACON_PAIR_CANDIDATE);
			}
			if (entity.HasTag(GAME_TAG.BACON_DUO_TRIPLE_CANDIDATE_TEAMMATE))
			{
				actor.ActivateSpellBirthState(SpellType.BACON_DUO_TRIPLE_CANDIDATE_TEAMMATE);
			}
			if (entity.HasTag(GAME_TAG.BACON_DUO_PAIR_CANDIDATE_TEAMMATE))
			{
				actor.ActivateSpellBirthState(SpellType.BACON_DUO_PAIR_CANDIDATE_TEAMMATE);
			}
		}
	}

	private void DeactivateChoiceCardStateSpells(Card card)
	{
		Actor actor = card.GetActor();
		if (actor != null)
		{
			if (actor.UseCoinManaGemForChoiceCard())
			{
				actor.ReleaseSpell(SpellType.COIN_MANA_GEM);
			}
			if (actor.UseTechLevelManaGem())
			{
				actor.ReleaseSpell(SpellType.TECH_LEVEL_MANA_GEM);
			}
			actor.ReleaseSpell(SpellType.BACON_TRIPLE_CANDIDATE);
			actor.ReleaseSpell(SpellType.BACON_PAIR_CANDIDATE);
			actor.ReleaseSpell(SpellType.BACON_DUO_TRIPLE_CANDIDATE_TEAMMATE);
			actor.ReleaseSpell(SpellType.BACON_DUO_PAIR_CANDIDATE_TEAMMATE);
			actor.ReleaseSpell(SpellType.TEAMMATE_PING);
			actor.ReleaseSpell(SpellType.TEAMMATE_PING_WHEEL);
		}
	}

	private void DeactivateChoiceEffects(ChoiceState state)
	{
		if (state == null)
		{
			return;
		}
		foreach (Spell spell in state.m_choiceEffectSpells)
		{
			if (!(spell == null) && spell.HasUsableState(SpellStateType.DEATH))
			{
				spell.ActivateState(SpellStateType.DEATH);
			}
		}
		state.m_choiceEffectSpells.Clear();
	}

	private TagPostChoiceEffect GetTagPostChoiceEffect(ChoiceState choiceState)
	{
		Entity sourceEntity = GameState.Get().GetEntity(choiceState.m_sourceEntityId);
		if (sourceEntity == null)
		{
			Log.Gameplay.PrintWarning("ChoiceCardMgr - GetTagPostChoiceEffect - sourceEntity is null");
			return null;
		}
		foreach (TagPostChoiceEffect tagPostEffect in m_TagPostChoiceEffectData)
		{
			if (sourceEntity.HasTag(tagPostEffect.m_Tag))
			{
				return tagPostEffect;
			}
		}
		return null;
	}

	private void ApplyPostChoiceEffects(TagPostChoiceEffect postChoiceEffect, ChoiceState choiceState, Network.EntitiesChosen chosen)
	{
		ISpellCallbackHandler<Spell>.StateFinishedCallback OnPostChoiceEffectStateFinished = delegate(Spell spell, SpellStateType prevStateType, object userData)
		{
			if (spell.GetActiveState() == SpellStateType.NONE)
			{
				SpellManager.Get().ReleaseSpell(spell);
			}
		};
		if (postChoiceEffect != null)
		{
			List<Card> choices = choiceState.m_cards;
			for (int i = 0; i < choices.Count; i++)
			{
				Card card = choices[i];
				Spell postEffectSpell = (WasCardChosen(card, chosen.Entities) ? postChoiceEffect.m_SpellSelectedCards : postChoiceEffect.m_SpellUnselectedCards);
				Spell spellInstance = SpellManager.Get().GetSpell(postEffectSpell);
				TransformUtil.AttachAndPreserveLocalTransform(spellInstance.transform, card.GetActor().transform);
				spellInstance.AddStateFinishedCallback(OnPostChoiceEffectStateFinished);
				spellInstance.ActivateState(SpellStateType.DEATH);
				choiceState.m_postChoiceSpells.Add(spellInstance);
			}
		}
	}

	private bool HavePostChoiceEffectsFinished(ChoiceState choiceState)
	{
		foreach (Spell spell in choiceState.m_postChoiceSpells)
		{
			if (spell != null && !spell.IsFinished())
			{
				return false;
			}
		}
		return true;
	}

	private ChoiceEffectData GetChoiceEffectDataForCard(Card sourceCard)
	{
		if (sourceCard == null)
		{
			return null;
		}
		foreach (CardSpecificChoiceEffect cardSpecificEffect in m_CardSpecificChoiceEffectData)
		{
			if (cardSpecificEffect.m_CardID == sourceCard.GetEntity().GetCardId())
			{
				return cardSpecificEffect.m_ChoiceEffectData;
			}
		}
		foreach (TagSpecificChoiceEffect tagSpecificChoiceEffect in m_TagSpecificChoiceEffectData)
		{
			if (!sourceCard.GetEntity().HasTag(tagSpecificChoiceEffect.m_Tag))
			{
				continue;
			}
			foreach (TagValueSpecificChoiceEffect tagValueSpecificChoiceEffect in tagSpecificChoiceEffect.m_ValueSpellMap)
			{
				if (tagValueSpecificChoiceEffect.m_Value == sourceCard.GetEntity().GetTag(tagSpecificChoiceEffect.m_Tag))
				{
					return tagValueSpecificChoiceEffect.m_ChoiceEffectData;
				}
			}
		}
		if (sourceCard.GetEntity().HasTag(GAME_TAG.USE_DISCOVER_VISUALS))
		{
			return m_DiscoverChoiceEffectData;
		}
		if (sourceCard.GetEntity().HasReferencedTag(GAME_TAG.ADAPT))
		{
			return m_AdaptChoiceEffectData;
		}
		if (sourceCard.GetEntity().HasTag(GAME_TAG.GEARS))
		{
			return m_GearsChoiceEffectData;
		}
		if (sourceCard.GetEntity().HasTag(GAME_TAG.GOOD_OL_GENERIC_FRIENDLY_DRAGON_DISCOVER_VISUALS))
		{
			return m_DragonChoiceEffectData;
		}
		if (sourceCard.GetEntity().HasTag(GAME_TAG.BACON_IS_MAGIC_ITEM_DISCOVER))
		{
			return m_TrinketChoiceEffectData;
		}
		return null;
	}

	private IEnumerator WaitThenConcealChoicesFromPacket(Network.EntitiesChosen chosen)
	{
		bool allowedToPrintPowers = GameState.Get()?.GameScenarioAllowsPowerPrinting() ?? true;
		int playerId = chosen.PlayerId;
		if (m_choiceStateMap.TryGetValue(playerId, out var choiceState))
		{
			if (choiceState.m_waitingToStart || !choiceState.m_hasBeenRevealed)
			{
				if (allowedToPrintPowers)
				{
					Log.Power.Print("ChoiceCardMgr.WaitThenHideChoicesFromPacket() - id={0} BEGIN WAIT for EntityChoice", chosen.ID);
				}
				while (choiceState.m_waitingToStart)
				{
					yield return null;
				}
				while (!choiceState.m_hasBeenRevealed)
				{
					yield return null;
				}
				yield return new WaitForSeconds(m_ChoiceData.m_MinShowTime);
			}
		}
		else if (m_lastShownChoiceState.m_choiceID == chosen.ID)
		{
			choiceState = m_lastShownChoiceState;
		}
		if (choiceState == null && allowedToPrintPowers)
		{
			Log.Power.Print("ChoiceCardMgr.WaitThenHideChoicesFromPacket(): Unable to find ChoiceState corresponding to EntitiesChosen packet with ID %d.", chosen.ID);
			Log.Power.Print("ChoiceCardMgr.WaitThenHideChoicesFromPacket() - id={0} END WAIT", chosen.ID);
			GameState.Get().OnEntitiesChosenProcessed(chosen);
			yield break;
		}
		ResolveConflictBetweenLocalChoiceAndServerPacket(choiceState, chosen);
		if (choiceState.m_isFriendly)
		{
			TagPostChoiceEffect postChoiceEffect = GetTagPostChoiceEffect(choiceState);
			ApplyPostChoiceEffects(postChoiceEffect, choiceState, chosen);
			while (!HavePostChoiceEffectsFinished(choiceState))
			{
				yield return null;
			}
		}
		if (allowedToPrintPowers)
		{
			Log.Power.Print("ChoiceCardMgr.WaitThenHideChoicesFromPacket() - id={0} END WAIT", chosen.ID);
		}
		ConcealChoicesFromPacket(playerId, choiceState, chosen);
	}

	private void ResolveConflictBetweenLocalChoiceAndServerPacket(ChoiceState choiceState, Network.EntitiesChosen chosen)
	{
		if (DoesLocalChoiceMatchPacket(choiceState.m_chosenEntities, chosen.Entities))
		{
			return;
		}
		choiceState.m_chosenEntities = new List<Entity>();
		foreach (int entityID in chosen.Entities)
		{
			Entity chosenEntity = GameState.Get().GetEntity(entityID);
			if (chosenEntity != null)
			{
				choiceState.m_chosenEntities.Add(chosenEntity);
			}
		}
		if (!choiceState.m_hasBeenConcealed)
		{
			return;
		}
		foreach (Card card in choiceState.m_cards)
		{
			card.ShowCard();
		}
		choiceState.m_hasBeenConcealed = false;
	}

	private bool DoesLocalChoiceMatchPacket(List<Entity> localChoices, List<int> packetChoices)
	{
		if (localChoices == null || packetChoices == null)
		{
			GameState gameState = GameState.Get();
			if (gameState == null || gameState.GameScenarioAllowsPowerPrinting())
			{
				Log.Power.Print($"ChoiceCardMgr.DoesLocalChoiceMatchPacket(): Null list passed in! localChoices={localChoices}, packetChoices={packetChoices}.");
			}
			return false;
		}
		if (localChoices.Count != packetChoices.Count)
		{
			return false;
		}
		for (int i = 0; i < packetChoices.Count; i++)
		{
			int packetEntityID = packetChoices[i];
			Entity packetEntity = GameState.Get().GetEntity(packetEntityID);
			if (!localChoices.Contains(packetEntity))
			{
				return false;
			}
		}
		return true;
	}

	private void ConcealChoicesFromPacket(int playerId, ChoiceState choiceState, Network.EntitiesChosen chosen)
	{
		if (choiceState.m_isFriendly)
		{
			HideChoiceUI();
		}
		ISpell customChoiceConcealSpell = GetCustomChoiceConcealSpell(choiceState);
		if (customChoiceConcealSpell != null)
		{
			ConcealChoiceCardsUsingCustomSpell(customChoiceConcealSpell, choiceState, chosen);
		}
		else
		{
			DefaultConcealChoicesFromPacket(playerId, choiceState, chosen);
		}
	}

	private void DefaultConcealChoicesFromPacket(int playerId, ChoiceState choiceState, Network.EntitiesChosen chosen)
	{
		if (!choiceState.m_hasBeenConcealed)
		{
			List<Card> choices = choiceState.m_cards;
			bool hideChosen = choiceState.m_hideChosen;
			for (int i = 0; i < choices.Count; i++)
			{
				Card card = choices[i];
				if (hideChosen || !WasCardChosen(card, chosen.Entities))
				{
					card.DeactivateHandStateSpells(card.GetActor());
					DeactivateChoiceCardStateSpells(card);
					card.HideCard();
				}
			}
			DeactivateChoiceEffects(choiceState);
			choiceState.m_hasBeenConcealed = true;
		}
		OnFinishedConcealChoices(playerId);
		GameState.Get().OnEntitiesChosenProcessed(chosen);
	}

	private bool WasCardChosen(Card card, List<int> chosenEntityIds)
	{
		Entity entity = card.GetEntity();
		int entityId = entity.GetEntityId();
		return chosenEntityIds.FindIndex((int currEntityId) => entityId == currEntityId) >= 0;
	}

	private void ConcealChoicesFromInput(int playerId, ChoiceState choiceState)
	{
		if (choiceState.m_isFriendly)
		{
			HideChoiceUI();
		}
		ISpell customChoiceConcealSpell = GetCustomChoiceConcealSpell(choiceState);
		TagPostChoiceEffect postChoiceEffect = GetTagPostChoiceEffect(choiceState);
		if (customChoiceConcealSpell != null || postChoiceEffect != null)
		{
			return;
		}
		for (int i = 0; i < choiceState.m_cards.Count; i++)
		{
			Card card = choiceState.m_cards[i];
			Entity entity = card.GetEntity();
			card.GetActor()?.SetShowCostOverride(0);
			if (choiceState.m_hideChosen || !choiceState.m_chosenEntities.Contains(entity))
			{
				card.HideCard();
				card.DeactivateHandStateSpells(card.GetActor());
				DeactivateChoiceCardStateSpells(card);
			}
		}
		DeactivateChoiceEffects(choiceState);
		choiceState.m_hasBeenConcealed = true;
		OnFinishedConcealChoices(playerId);
	}

	private void OnFinishedConcealChoices(int playerId)
	{
		if (!m_choiceStateMap.ContainsKey(playerId))
		{
			return;
		}
		foreach (GameObject value in m_choiceStateMap[playerId].m_xObjs.Values)
		{
			UnityEngine.Object.Destroy(value);
		}
		m_choiceStateMap.Remove(playerId);
	}

	private void HideChoiceCards(ChoiceState state)
	{
		for (int i = 0; i < state.m_cards.Count; i++)
		{
			Card card = state.m_cards[i];
			HideChoiceCard(card);
		}
		DeactivateChoiceEffects(state);
		CorpseCounter.UpdateTextAll();
	}

	private void HideChoiceCard(Card card)
	{
		Action<object> onHideComplete = delegate(object userData)
		{
			((Card)userData).HideCard();
		};
		iTween.Stop(card.gameObject);
		Hashtable scaleArgs = iTweenManager.Get().GetTweenHashTable();
		scaleArgs.Add("scale", INVISIBLE_SCALE);
		scaleArgs.Add("time", m_ChoiceData.m_CardHideTime);
		scaleArgs.Add("oncomplete", onHideComplete);
		scaleArgs.Add("oncompleteparams", card);
		scaleArgs.Add("oncompletetarget", base.gameObject);
		iTween.ScaleTo(card.gameObject, scaleArgs);
	}

	private void ShowChoiceUi(ChoiceState choiceState)
	{
		ShowChoiceBanner(choiceState);
		ShowChoiceButtons(choiceState);
		ShowMagicItemShopChoiceBackground(choiceState);
		HideEnlargedHand();
	}

	private void HideChoiceUI()
	{
		HideChoiceBanner();
		HideChoiceButtons();
		HideMagicItemShopChoiceBackground(playSound: true);
		RestoreEnlargedHand();
	}

	private void ShowChoiceBanner(ChoiceState choiceState)
	{
		HideChoiceBanner();
		if ((!choiceState.m_isSubOptionChoice || choiceState.m_isTitanAbility) && !choiceState.m_isMagicItemDiscover)
		{
			Transform bone = Board.Get().FindBone(m_ChoiceData.m_BannerBoneName);
			m_choiceBanner = UnityEngine.Object.Instantiate(m_ChoiceData.m_BannerPrefab, bone.position, bone.rotation);
			m_choiceBanner.SetupBanner(choiceState.m_sourceEntityId, choiceState.m_cards, choiceState.m_isSubOptionChoice);
			if (!(GameState.Get().GetEntity(choiceState.m_sourceEntityId).GetCardId() != "TTN_717t"))
			{
				Vector3 originalScale = m_choiceBanner.transform.localScale;
				m_choiceBanner.transform.localScale = INVISIBLE_SCALE;
				Hashtable args = iTweenManager.Get().GetTweenHashTable();
				args.Add("scale", originalScale);
				args.Add("time", m_ChoiceData.m_UiShowTime);
				iTween.ScaleTo(m_choiceBanner.gameObject, args);
			}
		}
	}

	private void ShowStarshipLaunchpadChoiceBanner(ChoiceState choiceState)
	{
		HideChoiceBanner();
		Transform bone = Board.Get().FindBone(m_ChoiceData.m_BannerBoneName);
		m_choiceBanner = UnityEngine.Object.Instantiate(m_ChoiceData.m_BannerPrefab, bone.position, bone.rotation);
		m_choiceBanner.SetupBanner(choiceState.m_sourceEntityId, choiceState.m_cards, choiceState.m_isSubOptionChoice);
		Vector3 originalScale = m_choiceBanner.transform.localScale;
		m_choiceBanner.transform.localScale = INVISIBLE_SCALE;
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("scale", originalScale);
		args.Add("time", m_ChoiceData.m_UiShowTime);
		iTween.ScaleTo(m_choiceBanner.gameObject, args);
	}

	private void ShowMagicItemShopChoiceBackground(ChoiceState choiceState)
	{
		HideMagicItemShopChoiceBackground();
		if (choiceState.m_isMagicItemDiscover)
		{
			m_magicItemShopBackground = UnityEngine.Object.Instantiate(m_ChoiceData.m_MagicItemShopBackgroundPrefab);
			ToggleMagicItemShopBackgroundVisibility(visible: true);
		}
	}

	private void HideMagicItemShopChoiceBackground(bool playSound = false)
	{
		if ((bool)m_magicItemShopBackground)
		{
			if (playSound)
			{
				PlayTrinketShopEvent("hide");
			}
			UnityEngine.Object.Destroy(m_magicItemShopBackground);
		}
	}

	private void HideChoiceBanner()
	{
		if ((bool)m_choiceBanner)
		{
			UnityEngine.Object.Destroy(m_choiceBanner.gameObject);
		}
	}

	private void ShowChoiceButtons(ChoiceState choiceState)
	{
		HideChoiceButtons();
		string boneName = (choiceState.m_isMagicItemDiscover ? m_ChoiceData.m_BGTrinketToggleChoiceButtonBoneName : m_ChoiceData.m_ToggleChoiceButtonBoneName);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			boneName += "_phone";
		}
		if (choiceState.m_isSubOptionChoice)
		{
			m_toggleChoiceButton = CreateChoiceButton(boneName, ChoiceButton_OnPress, CancelChoiceButton_OnRelease, GameStrings.Get("GLOBAL_CANCEL"));
		}
		else
		{
			m_toggleChoiceButton = CreateChoiceButton(boneName, ChoiceButton_OnPress, ToggleChoiceButton_OnRelease, GameStrings.Get("GLOBAL_HIDE"));
		}
		Network.EntityChoices choicePacket = GameState.Get().GetFriendlyEntityChoices();
		if (choicePacket != null && (!choicePacket.IsSingleChoice() || choiceState.m_isMagicItemDiscover))
		{
			boneName = (choiceState.m_isMagicItemDiscover ? m_ChoiceData.m_BGTrinketConfirmChoiceButtonBoneName : m_ChoiceData.m_ConfirmChoiceButtonBoneName);
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				boneName += "_phone";
			}
			m_confirmChoiceButton = CreateChoiceButton(boneName, ChoiceButton_OnPress, ConfirmChoiceButton_OnRelease, GameStrings.Get("GLOBAL_CONFIRM"));
			UpdateConfirmButtonVFX(choiceState);
		}
	}

	public void ShowStarshipPiecesForOpposingPlayer(Entity OpposingPlayerStarship)
	{
		if (OpposingPlayerStarship == null || !OpposingPlayerStarship.HasSubCards())
		{
			return;
		}
		GameState gameState = GameState.Get();
		if (gameState == null)
		{
			return;
		}
		List<int> subCardList = OpposingPlayerStarship.GetSubCardIDs();
		ChoiceState choiceStateOpposing = new ChoiceState();
		List<Card> starshipSubcards = new List<Card>();
		for (int i = 0; i < subCardList.Count; i++)
		{
			Entity entity = gameState.GetEntity(subCardList[i]);
			if (entity != null)
			{
				starshipSubcards.Add(entity.GetCard());
			}
		}
		choiceStateOpposing.m_cards = starshipSubcards;
		choiceStateOpposing.m_isLaunchpadAbility = OpposingPlayerStarship.IsLaunchpad();
		choiceStateOpposing.m_isFriendly = false;
		choiceStateOpposing.m_sourceEntityId = OpposingPlayerStarship.GetEntityId();
		choiceStateOpposing.m_isSubOptionChoice = true;
		ShowStarshipHUD(choiceStateOpposing);
	}

	public void HideStarshipPiecesForOpposingPlayer()
	{
		HideChoiceUI();
	}

	private void ShowStarshipHUD(ChoiceState choiceState)
	{
		if (!choiceState.m_isLaunchpadAbility || (m_starshipHUDManager != null && m_starshipHUDManager.IsWaitingOnDestroy()))
		{
			return;
		}
		m_starshipHUDManager = StarshipHUDManager.Get();
		GameObject m_starshipHUDInstance = m_starshipHUDManager.transform.gameObject;
		ShowStarshipLaunchpadChoiceBanner(choiceState);
		string boneName = m_ChoiceData.m_ToggleChoiceButtonBoneName;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			boneName += "_phone";
		}
		Transform bone = Board.Get().FindBone(boneName);
		TransformUtil.CopyWorld(m_starshipHUDInstance, bone);
		BigCard.Get().Hide();
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_starshipHUDInstance.transform.localRotation = Quaternion.Euler(m_StarshipUIData.m_RotationMobile);
			m_starshipHUDInstance.transform.localScale = m_StarshipUIData.m_ScaleMobile;
			m_starshipHUDInstance.transform.localPosition = m_StarshipUIData.m_PositionMobile;
		}
		else
		{
			m_starshipHUDInstance.transform.localRotation = Quaternion.Euler(m_StarshipUIData.m_RotationPC);
			m_starshipHUDInstance.transform.localScale = m_StarshipUIData.m_ScalePC;
			m_starshipHUDInstance.transform.localPosition = m_StarshipUIData.m_PositionPC;
		}
		GameState.Get();
		Entity launchChoiceEntity = null;
		m_starshipPiecesToView.Clear();
		for (int i = 0; i < choiceState.m_cards.Count; i++)
		{
			Card card = choiceState.m_cards[i];
			Entity entity = card.GetEntity();
			if (card.GetEntity().GetCardId() == "GDB_905")
			{
				launchChoiceEntity = entity;
			}
			else if (!(entity.GetCardId() == "GDB_906") && entity.HasTag(GAME_TAG.STARSHIP_PIECE))
			{
				m_starshipPiecesToView.Add(entity.GetEntityId());
			}
		}
		if (choiceState.m_isFriendly)
		{
			Player friendlyPlayer = GameState.Get().GetFriendlySidePlayer();
			m_starshipHUDManager.SetupLaunchAndAbortButtons(delegate
			{
				Player friendlySidePlayer = GameState.Get().GetFriendlySidePlayer();
				int num = GameUtils.StarshipLaunchCost(friendlySidePlayer);
				GameState gameState = GameState.Get();
				InputManager inputManager = InputManager.Get();
				if (friendlySidePlayer.GetNumAvailableResources() < num)
				{
					PlayErrors.DisplayPlayError(gameState.GetErrorType(launchChoiceEntity), gameState.GetErrorParam(launchChoiceEntity), launchChoiceEntity);
					inputManager.HidePlayerStarshipUI();
				}
				else if (inputManager != null)
				{
					inputManager.HandleClickOnSubOption(launchChoiceEntity);
					inputManager.HidePlayerStarshipUI();
				}
			}, friendlyPlayer);
		}
		else
		{
			m_starshipHUDManager.SetupButtonsOpponentStarship();
		}
		m_starshipHUDManager.SetupSubcards(m_starshipPiecesToView);
	}

	private void UpdateConfirmButtonVFX(ChoiceState state)
	{
		if (m_confirmChoiceButton != null && state.m_isMagicItemDiscover)
		{
			EnableConfirmButton(GameState.Get().GetChosenEntities().Count > 0);
		}
	}

	public NormalButton CreateChoiceButton(string boneName, UIEvent.Handler OnPressHandler, UIEvent.Handler OnReleaseHandler, string buttonText)
	{
		NormalButton component = AssetLoader.Get().InstantiatePrefab(m_ChoiceData.m_ButtonPrefab, AssetLoadingOptions.IgnorePrefabPosition).GetComponent<NormalButton>();
		component.GetButtonUberText().TextAlpha = 1f;
		Transform bone = Board.Get().FindBone(boneName);
		TransformUtil.CopyWorld(component, bone);
		m_friendlyChoicesShown = true;
		component.AddEventListener(UIEventType.PRESS, OnPressHandler);
		component.AddEventListener(UIEventType.RELEASE, OnReleaseHandler);
		component.SetText(buttonText);
		component.m_button.GetComponent<Spell>().ActivateState(SpellStateType.BIRTH);
		return component;
	}

	private void HideChoiceButtons()
	{
		if (m_toggleChoiceButton != null)
		{
			UnityEngine.Object.Destroy(m_toggleChoiceButton.gameObject);
			m_toggleChoiceButton = null;
		}
		if (m_confirmChoiceButton != null)
		{
			UnityEngine.Object.Destroy(m_confirmChoiceButton.gameObject);
			m_confirmChoiceButton = null;
		}
		if (m_starshipHUDManager != null)
		{
			m_starshipHUDManager.AnimateAndDestroyHUD();
			m_starshipHUDManager = null;
		}
	}

	private void HideEnlargedHand()
	{
		ZoneHand hand = GameState.Get().GetFriendlySidePlayer().GetHandZone();
		if (hand.HandEnlarged())
		{
			m_restoreEnlargedHand = true;
			hand.SetHandEnlarged(enlarged: false);
		}
	}

	private void RestoreEnlargedHand()
	{
		if (!m_restoreEnlargedHand)
		{
			return;
		}
		m_restoreEnlargedHand = false;
		if (!GameState.Get().IsInTargetMode())
		{
			ZoneHand hand = GameState.Get().GetFriendlySidePlayer().GetHandZone();
			if (!hand.HandEnlarged())
			{
				hand.SetHandEnlarged(enlarged: true);
			}
		}
	}

	private void ChoiceButton_OnPress(UIEvent e)
	{
		SoundManager.Get().LoadAndPlay("UI_MouseClick_01.prefab:fa537702a0db1c3478c989967458788b");
	}

	private void CancelChoiceButton_OnRelease(UIEvent e)
	{
		InputManager.Get().CancelSubOptionMode();
	}

	private void ToggleChoiceButton_OnRelease(UIEvent e)
	{
		int friendlyPlayerId = GameState.Get().GetFriendlyPlayerId();
		ChoiceState friendlyState = m_choiceStateMap[friendlyPlayerId];
		if (m_friendlyChoicesShown)
		{
			m_toggleChoiceButton.SetText(GameStrings.Get("GLOBAL_SHOW"));
			HideChoiceCards(friendlyState);
			m_friendlyChoicesShown = false;
		}
		else
		{
			m_toggleChoiceButton.SetText(GameStrings.Get("GLOBAL_HIDE"));
			ShowChoiceCards(friendlyState, friendly: true);
			m_friendlyChoicesShown = true;
		}
		ToggleMagicItemShopBackgroundVisibility(m_friendlyChoicesShown);
		ToggleConfirmButtonVisibility(m_friendlyChoicesShown);
		ToggleChoiceBannerVisibility(m_friendlyChoicesShown);
	}

	private void ToggleChoiceBannerVisibility(bool visible)
	{
		if ((bool)m_choiceBanner)
		{
			m_choiceBanner.gameObject.SetActive(visible);
		}
	}

	private void ToggleConfirmButtonVisibility(bool visible)
	{
		if ((bool)m_confirmChoiceButton)
		{
			m_confirmChoiceButton.gameObject.SetActive(visible);
			if (visible)
			{
				m_confirmChoiceButton.m_button.GetComponent<Spell>().ActivateState(SpellStateType.BIRTH);
			}
		}
	}

	private void PlayTrinketShopEvent(string playmakerEvent)
	{
		BaconTrinketBacker backer = m_magicItemShopBackground.GetComponent<BaconTrinketBacker>();
		if (backer != null && backer.m_playmaker != null)
		{
			backer.m_playmaker.SendEvent(playmakerEvent);
		}
	}

	private void ToggleMagicItemShopBackgroundVisibility(bool visible)
	{
		if (!(m_magicItemShopBackground == null))
		{
			BaconTrinketBacker backer = m_magicItemShopBackground.GetComponent<BaconTrinketBacker>();
			if (backer != null)
			{
				backer.Show(visible);
			}
		}
	}

	private void ConfirmChoiceButton_OnRelease(UIEvent e)
	{
		GameState.Get().SendChoices();
	}

	private void CancelChoices()
	{
		HideChoiceUI();
		foreach (ChoiceState state in m_choiceStateMap.Values)
		{
			for (int i = 0; i < state.m_cards.Count; i++)
			{
				Card card = state.m_cards[i];
				card.HideCard();
				card.DeactivateHandStateSpells(card.GetActor());
				DeactivateChoiceCardStateSpells(card);
			}
		}
		m_choiceStateMap.Clear();
	}

	private IEnumerator WaitThenShowSubOptions()
	{
		while (IsWaitingToShowSubOptions())
		{
			yield return null;
			if (m_subOptionState == null)
			{
				yield break;
			}
		}
		ShowSubOptions();
	}

	private void ShowSubOptions()
	{
		GameState state = GameState.Get();
		Card parentCard = m_subOptionState.m_parentCard;
		Entity parentEntity = m_subOptionState.m_parentCard.GetEntity();
		int playerId = parentEntity.GetController().GetPlayerId();
		int teamId = parentEntity.GetController().GetTeamId();
		ChoiceState choiceState = new ChoiceState();
		if (m_choiceStateMap.ContainsKey(playerId))
		{
			m_choiceStateMap[playerId] = choiceState;
		}
		else
		{
			m_choiceStateMap.Add(playerId, choiceState);
		}
		choiceState.m_waitingToStart = false;
		choiceState.m_hasBeenConcealed = false;
		choiceState.m_hasBeenRevealed = false;
		choiceState.m_choiceID = 0;
		choiceState.m_hideChosen = true;
		choiceState.m_sourceEntityId = parentEntity.GetEntityId();
		choiceState.m_preTaskList = null;
		choiceState.m_xObjs = new Map<int, GameObject>();
		choiceState.m_isFriendly = teamId == state.GetFriendlySideTeamId();
		choiceState.m_hideChoiceUI = true;
		choiceState.m_isSubOptionChoice = true;
		choiceState.m_isTitanAbility = parentEntity.IsTitan();
		choiceState.m_isMagicItemDiscover = parentEntity.HasTag(GAME_TAG.BACON_IS_MAGIC_ITEM_DISCOVER);
		choiceState.m_isLaunchpadAbility = parentEntity.HasTag(GAME_TAG.LAUNCHPAD);
		string boneName = m_SubOptionData.m_BoneName;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			boneName += "_phone";
		}
		Transform subOptionBone = Board.Get().FindBone(boneName);
		float xOffset = m_CommonData.m_FriendlyCardWidth;
		float startingXPos = subOptionBone.position.x;
		ZonePlay playZone = parentEntity.GetController().GetBattlefieldZone();
		List<int> subCardIDs = parentEntity.GetSubCardIDs();
		if (parentEntity.IsMinion() && !UniversalInputManager.UsePhoneUI && subCardIDs.Count <= 2)
		{
			int zonePosition = parentCard.GetZonePosition();
			startingXPos = playZone.GetCardPosition(parentCard).x;
			if (zonePosition > 5)
			{
				xOffset += m_SubOptionData.m_AdjacentCardXOffset;
				startingXPos -= m_CommonData.m_FriendlyCardWidth * 1.5f + m_SubOptionData.m_AdjacentCardXOffset + m_SubOptionData.m_MinionParentXOffset;
			}
			else if (zonePosition == 1 && playZone.GetCards().Count > 6)
			{
				xOffset += m_SubOptionData.m_AdjacentCardXOffset;
				startingXPos += m_CommonData.m_FriendlyCardWidth / 2f + m_SubOptionData.m_MinionParentXOffset;
			}
			else
			{
				xOffset += m_SubOptionData.m_MinionParentXOffset * 2f;
				startingXPos -= m_CommonData.m_FriendlyCardWidth / 2f + m_SubOptionData.m_MinionParentXOffset;
			}
		}
		else
		{
			int numCards = subCardIDs.Count;
			xOffset += ((numCards > m_CommonData.m_MaxCardsBeforeAdjusting) ? m_SubOptionData.m_PhoneMaxAdjacentCardXOffset : m_SubOptionData.m_AdjacentCardXOffset);
			startingXPos -= xOffset / 2f * (float)(numCards - 1);
		}
		ISpell customChoiceRevealSpell = GetCustomChoiceRevealSpell(choiceState);
		if (parentEntity.IsTitan())
		{
			customChoiceRevealSpell = null;
		}
		if (choiceState.m_isFriendly && parentEntity.IsLaunchpad())
		{
			for (int i = 0; i < subCardIDs.Count; i++)
			{
				int subCardID = subCardIDs[i];
				Card card = state.GetEntity(subCardID).GetCard();
				m_subOptionState.m_cards.Add(card);
				choiceState.m_cards.Add(card);
			}
			ShowStarshipHUD(choiceState);
			HideEnlargedHand();
			return;
		}
		bool forceRevealSuboption = GameMgr.Get().IsBattlegrounds();
		if (customChoiceRevealSpell != null)
		{
			for (int j = 0; j < subCardIDs.Count; j++)
			{
				int subCardID2 = subCardIDs[j];
				Card card2 = state.GetEntity(subCardID2).GetCard();
				if (!(card2 == null))
				{
					choiceState.m_cards.Add(card2);
					m_subOptionState.m_cards.Add(card2);
					card2.ForceLoadHandActor(forceRevealSuboption);
					card2.GetActor().Hide();
					card2.transform.position = parentCard.transform.position;
					Vector3 pos = default(Vector3);
					pos.x = startingXPos + (float)j * xOffset;
					pos.y = subOptionBone.position.y;
					pos.z = subOptionBone.position.z;
					iTween.MoveTo(card2.gameObject, pos, m_SubOptionData.m_CardShowTime);
					Vector3 scale = subOptionBone.localScale;
					if (subCardIDs.Count > m_CommonData.m_MaxCardsBeforeAdjusting)
					{
						float wantedScale = GetScaleForCardCount(subCardIDs.Count);
						scale.x *= wantedScale;
						scale.y *= wantedScale;
						scale.z *= wantedScale;
					}
					card2.transform.localScale = scale;
				}
			}
			PopulateTransformDatas(choiceState);
			RevealChoiceCardsUsingCustomSpell(customChoiceRevealSpell, choiceState);
		}
		else
		{
			for (int k = 0; k < subCardIDs.Count; k++)
			{
				int subCardID3 = subCardIDs[k];
				Entity entity = state.GetEntity(subCardID3);
				Card card3 = entity.GetCard();
				if (card3 == null)
				{
					continue;
				}
				choiceState.m_cards.Add(card3);
				if (entity.GetCardType() == TAG_CARDTYPE.LETTUCE_ABILITY)
				{
					Transform[] componentsInChildren = card3.gameObject.GetComponentsInChildren<Transform>();
					for (int l = 0; l < componentsInChildren.Length; l++)
					{
						componentsInChildren[l].position = default(Vector3);
					}
				}
				m_subOptionState.m_cards.Add(card3);
				card3.ForceLoadHandActor(forceRevealSuboption);
				card3.transform.position = parentCard.transform.position;
				card3.transform.localScale = INVISIBLE_SCALE;
				Vector3 pos2 = default(Vector3);
				pos2.x = startingXPos + (float)k * xOffset;
				pos2.y = subOptionBone.position.y;
				pos2.z = subOptionBone.position.z;
				iTween.MoveTo(card3.gameObject, pos2, m_SubOptionData.m_CardShowTime);
				Vector3 scale2 = subOptionBone.localScale;
				if (subCardIDs.Count > m_CommonData.m_MaxCardsBeforeAdjusting)
				{
					float wantedScale2 = GetScaleForCardCount(subCardIDs.Count);
					scale2.x *= wantedScale2;
					scale2.y *= wantedScale2;
					scale2.z *= wantedScale2;
				}
				iTween.ScaleTo(card3.gameObject, scale2, m_SubOptionData.m_CardShowTime);
				card3.ActivateHandStateSpells();
				ActivateChoiceCardStateSpells(card3);
			}
		}
		if (choiceState.m_isFriendly && parentEntity.IsTitan())
		{
			ShowChoiceUi(choiceState);
		}
		HideEnlargedHand();
		if (GameMgr.Get().IsBattlegroundDuoGame())
		{
			Network.Get().SendNotifyTeammateChooseOne(subCardIDs);
		}
	}

	private void HideSubOptions(Entity chosenEntity = null)
	{
		for (int i = 0; i < m_subOptionState.m_cards.Count; i++)
		{
			Card card = m_subOptionState.m_cards[i];
			card.DeactivateHandStateSpells();
			DeactivateChoiceCardStateSpells(card);
			Entity entity = card.GetEntity();
			if (entity != chosenEntity || !entity.IsControlledByFriendlySidePlayer())
			{
				card.HideCard();
			}
		}
		HideChoiceUI();
		RestoreEnlargedHand();
		if (GameMgr.Get().IsBattlegroundDuoGame())
		{
			Network.Get().SendNotifyTeammateChooseOne(new List<int>());
		}
	}

	private bool IsEntityReady(Entity entity)
	{
		if (entity.GetZone() == TAG_ZONE.INVALID)
		{
			return false;
		}
		if (entity.IsBusy())
		{
			return false;
		}
		return true;
	}

	private bool IsCardReady(Card card)
	{
		return card.HasCardDef;
	}

	private bool IsCardActorReady(Card card)
	{
		return card.IsActorReady();
	}

	private ISpell GetCustomChoiceRevealSpell(ChoiceState choiceState)
	{
		Entity sourceEntity = GameState.Get().GetEntity(choiceState.m_sourceEntityId);
		if (sourceEntity == null)
		{
			return null;
		}
		Card sourceCard = sourceEntity.GetCard();
		if (sourceCard == null)
		{
			return null;
		}
		return sourceCard.GetCustomChoiceRevealSpell();
	}

	private ISpell GetCustomChoiceConcealSpell(ChoiceState choiceState)
	{
		Entity sourceEntity = GameState.Get().GetEntity(choiceState.m_sourceEntityId);
		if (sourceEntity == null)
		{
			return null;
		}
		Card sourceCard = sourceEntity.GetCard();
		if (sourceCard == null)
		{
			return null;
		}
		return sourceCard.GetCustomChoiceConcealSpell();
	}

	private void RevealChoiceCardsUsingCustomSpell(ISpell customChoiceRevealSpell, ChoiceState state)
	{
		ShowMagicItemShopChoiceBackground(state);
		CustomChoiceSpell customChoiceSpell = customChoiceRevealSpell as CustomChoiceSpell;
		if (customChoiceSpell != null)
		{
			customChoiceSpell.SetChoiceState(state);
		}
		customChoiceSpell?.AddFinishedCallback(OnCustomChoiceRevealSpellFinished, state);
		customChoiceRevealSpell.Activate();
	}

	private void OnCustomChoiceRevealSpellFinished(ISpell spell, object userData)
	{
		ChoiceState choiceState = userData as ChoiceState;
		if (choiceState == null)
		{
			Log.Power.PrintError("userData passed to ChoiceCardMgr.OnCustomChoiceRevealSpellFinished() is not of type ChoiceState.");
		}
		if (choiceState.m_isFriendly && !choiceState.m_hideChoiceUI)
		{
			ShowChoiceUi(choiceState);
		}
		foreach (Card card in choiceState.m_cards)
		{
			card.ShowCard();
			ActivateChoiceCardStateSpells(card);
		}
		PlayChoiceEffects(choiceState, choiceState.m_isFriendly);
		choiceState.m_hasBeenRevealed = true;
	}

	private void ConcealChoiceCardsUsingCustomSpell(ISpell customChoiceConcealSpell, ChoiceState choiceState, Network.EntitiesChosen chosen)
	{
		if (customChoiceConcealSpell.IsActive())
		{
			Log.Power.PrintError("ChoiceCardMgr.HideChoicesFromPacket(): CustomChoiceConcealSpell is already active!");
		}
		CustomChoiceSpell customChoiceSpell = customChoiceConcealSpell as CustomChoiceSpell;
		if (customChoiceSpell != null)
		{
			customChoiceSpell.SetChoiceState(choiceState);
		}
		DeactivateChoiceEffects(choiceState);
		choiceState.m_hasBeenConcealed = true;
		customChoiceSpell?.AddFinishedCallback(OnCustomChoiceConcealSpellFinished, chosen);
		customChoiceConcealSpell.Activate();
	}

	private void OnCustomChoiceConcealSpellFinished(Spell spell, object userData)
	{
		Network.EntitiesChosen chosen = userData as Network.EntitiesChosen;
		OnFinishedConcealChoices(chosen.PlayerId);
		GameState.Get().OnEntitiesChosenProcessed(chosen);
	}
}
