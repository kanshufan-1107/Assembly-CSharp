using System;
using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class InputMgr : MonoBehaviour
{
	public delegate void OnCardDroppedCallback();

	[SerializeField]
	protected CollectionDraggableCardVisual m_heldCardVisual;

	[SerializeField]
	protected AsyncReference m_mercenariesDraggablesReference;

	[SerializeField]
	protected Collider TooltipPlane;

	public AsyncReference m_battlegroundsDraggablesReference;

	public AsyncReference m_battlegroundsDragEmoteSpriteReference;

	public AsyncReference m_battlegroundsEmoteTrayReference;

	public static readonly PlatformDependentValue<float> PHONE_HEIGHT_OFFSET = new PlatformDependentValue<float>(PlatformCategory.Screen)
	{
		Phone = 10f
	};

	protected static List<InputMgr> s_instances = new List<InputMgr>();

	protected bool m_heldCardOffscreen;

	protected Widget m_mercenariesDraggablesWidget;

	protected Widget m_battlegroundsDraggablesWidget;

	protected SpriteRenderer m_battlegroundsDragEmoteSprite;

	protected BaconEmoteTray m_battlegroundsEmoteTray;

	protected CollectionUtils.MercenariesModeCardType m_heldType;

	protected CollectionUtils.BattlegroundsModeDraggableType m_bgHeldType;

	protected string m_heldMercenariesModeCardId;

	protected string m_heldBattlegroundsEmoteCardId;

	protected Vector3 m_offScreenPosition;

	private bool m_wasMouseOverDeck;

	public OnCardDroppedCallback m_cardDroppedCallback;

	protected virtual bool MouseIsOverDeck { get; set; }

	public event Action<CollectionUtils.MercenariesModeCardType, string> OnDropMercenariesModeCard;

	protected virtual void Awake()
	{
		s_instances.Add(this);
		UniversalInputManager.Get().RegisterMouseOnOrOffScreenListener(OnMouseOnOrOffScreen);
		m_mercenariesDraggablesReference?.RegisterReadyListener<Widget>(OnMercenariesDraggablesReady);
		m_battlegroundsDraggablesReference?.RegisterReadyListener<Widget>(OnBattlegroundsDraggablesReady);
		m_battlegroundsDragEmoteSpriteReference?.RegisterReadyListener<SpriteRenderer>(OnBattlegroundsDragEmoteSpriteReady);
		m_battlegroundsEmoteTrayReference?.RegisterReadyListener<BaconEmoteTray>(OnBattlegroundsEmoteTrayReady);
	}

	protected virtual void OnDestroy()
	{
		s_instances.Remove(this);
	}

	protected void Update()
	{
		UpdateHeldCard();
	}

	public static InputMgr Get()
	{
		int count = s_instances.Count;
		if (count == 0)
		{
			return null;
		}
		return s_instances[count - 1];
	}

	public void Unload()
	{
		UniversalInputManager.Get().UnregisterMouseOnOrOffScreenListener(OnMouseOnOrOffScreen);
	}

	public virtual bool HandleKeyboardInput()
	{
		return false;
	}

	public virtual bool GrabMercenariesModeCard(IDataModel dataModel, CollectionUtils.MercenariesModeCardType cardType, OnCardDroppedCallback callback = null)
	{
		if (dataModel == null)
		{
			return false;
		}
		if (!CanGrabMercenariesModeItem(cardType))
		{
			return false;
		}
		if (m_mercenariesDraggablesWidget == null)
		{
			return false;
		}
		if (!UniversalInputManager.Get().GetInputHitInfo(Box.Get().GetCamera(), GameLayer.DragPlane.LayerBit(), out var hit))
		{
			return false;
		}
		m_cardDroppedCallback = callback;
		m_mercenariesDraggablesWidget.BindDataModel(dataModel);
		PegCursor.Get().SetMode(PegCursor.Mode.DRAG);
		string holdOverCollectionEvent = null;
		string holdOverTeamTrayEvent = null;
		switch (cardType)
		{
		case CollectionUtils.MercenariesModeCardType.Mercenary:
			holdOverCollectionEvent = "START_MERC_OVER_COLLECTION_code";
			holdOverTeamTrayEvent = "HOLD_MERC_OVER_TEAM_TRAY_code";
			break;
		case CollectionUtils.MercenariesModeCardType.Equipment:
			holdOverCollectionEvent = "HOLD_ABILITY_OVER_COLLECTION_code";
			holdOverTeamTrayEvent = "HOLD_ABILITY_OVER_TEAM_TRAY_code";
			break;
		}
		SetHeldMercenaryCard(dataModel, cardType);
		m_mercenariesDraggablesWidget.TriggerEvent(holdOverCollectionEvent);
		DisableDraggableColliders();
		if (MouseIsOverDeck)
		{
			m_mercenariesDraggablesWidget.TriggerEvent(holdOverTeamTrayEvent);
		}
		else
		{
			m_mercenariesDraggablesWidget.TriggerEvent(holdOverCollectionEvent);
		}
		m_offScreenPosition = m_mercenariesDraggablesWidget.gameObject.transform.position;
		m_mercenariesDraggablesWidget.gameObject.transform.position = hit.point;
		return true;
	}

	public virtual void SetHeldMercenaryCard(IDataModel dataModel, CollectionUtils.MercenariesModeCardType cardType)
	{
		m_heldType = cardType;
		switch (cardType)
		{
		case CollectionUtils.MercenariesModeCardType.Mercenary:
			if (!(dataModel is LettuceMercenaryDataModel mercenaryData))
			{
				Log.Lettuce.PrintWarning("CollectionInputMgr.SetHeldMercenaryCard - mercenary data model is not valid!");
			}
			else
			{
				m_heldMercenariesModeCardId = CollectionManager.Get().GetMercenary(mercenaryData.MercenaryId).GetCardId();
			}
			break;
		case CollectionUtils.MercenariesModeCardType.Equipment:
		{
			if (!(dataModel is LettuceAbilityDataModel abilityData))
			{
				Log.Lettuce.PrintWarning("CollectionInputMgr.SetHeldMercenaryCard - ability data model is not valid!");
				break;
			}
			LettuceAbilityTierDataModel tierData = abilityData.AbilityTiers[abilityData.CurrentTier - 1];
			if (tierData == null)
			{
				Log.Lettuce.PrintWarning("CollectionInputMgr.SetHeldMercenaryCard - ability tier data model is not valid!");
			}
			else
			{
				m_heldMercenariesModeCardId = tierData.AbilityTierCard.CardId;
			}
			break;
		}
		}
	}

	public CollectionDraggableCardVisual GetHeldCardVisual()
	{
		return m_heldCardVisual;
	}

	public bool BattlegroundsIsDragging()
	{
		return m_bgHeldType != CollectionUtils.BattlegroundsModeDraggableType.None;
	}

	private void UpdateHeldCard()
	{
		if (m_heldType != 0)
		{
			UpdateHeldMercenariesModeCard();
		}
		else if (BattlegroundsIsDragging())
		{
			UpdateBattlegroundsModeEmote();
		}
		else if (m_heldCardVisual != null && m_heldCardVisual.IsShown())
		{
			UpdateHeldCardVisual();
		}
	}

	protected virtual bool CanGrabMercenariesModeItem(CollectionUtils.MercenariesModeCardType itemType)
	{
		if (m_heldType != 0)
		{
			return false;
		}
		return true;
	}

	protected virtual void UpdateHeldCardVisual()
	{
		if (!UniversalInputManager.Get().GetInputHitInfo(GameLayer.DragPlane.LayerBit(), out var hit))
		{
			return;
		}
		if (m_heldCardVisual != null && (bool)UniversalInputManager.UsePhoneUI)
		{
			Transform[] componentsInChildren = m_heldCardVisual.GetComponentsInChildren<Transform>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.layer = 19;
			}
		}
		Vector3 newPos = hit.point;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			newPos.y += PHONE_HEIGHT_OFFSET;
		}
		m_heldCardVisual.transform.position = newPos;
	}

	private void UpdateHeldMercenariesModeCard()
	{
		if (UniversalInputManager.Get().GetInputHitInfo(Box.Get().GetCamera(), GameLayer.DragPlane.LayerBit(), out var hit))
		{
			Vector3 newPos = hit.point;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				newPos.y += PHONE_HEIGHT_OFFSET;
			}
			m_mercenariesDraggablesWidget.gameObject.transform.position = newPos;
			UpdateMercenariesHeldVisual(m_heldType);
			if (InputCollection.GetMouseButtonUp(0))
			{
				DropMercenariesModeCard(dragCanceled: false);
			}
		}
	}

	private void UpdateBattlegroundsModeEmote()
	{
		if (!InputUtil.IsMouseOnScreen())
		{
			DropBattlegroundsEmote(dragCanceled: false, trayDropCanceled: true);
			m_battlegroundsEmoteTray.UpdateTrayHighlight(trayHovered: false);
			return;
		}
		if (!UniversalInputManager.Get().GetInputHitInfo(Box.Get().GetCamera(), GameLayer.DragPlane.LayerBit(), out var hit) || !InputCollection.GetMouseButton(0))
		{
			DropBattlegroundsEmote(dragCanceled: false);
			m_battlegroundsEmoteTray.UpdateTrayHighlight(trayHovered: false);
			return;
		}
		RaycastHit hitInfo;
		bool bgTrayHovered = UniversalInputManager.Get().ForcedUnblockableInputIsOver(Box.Get().GetCamera(), m_battlegroundsEmoteTray.gameObject, out hitInfo);
		m_battlegroundsEmoteTray.UpdateTrayHighlight(bgTrayHovered);
		Vector3 newPos = hit.point;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			newPos.y += PHONE_HEIGHT_OFFSET;
		}
		m_battlegroundsDraggablesWidget.gameObject.transform.position = newPos;
	}

	protected virtual void UpdateMercenariesHeldVisual(CollectionUtils.MercenariesModeCardType heldType)
	{
		string eventName = "";
		bool isOverTarget = MouseIsOverDeck;
		switch (heldType)
		{
		case CollectionUtils.MercenariesModeCardType.Equipment:
			if (isOverTarget && !m_wasMouseOverDeck)
			{
				eventName = "HOLD_ABILITY_OVER_TEAM_TRAY_code";
			}
			else if (!isOverTarget && m_wasMouseOverDeck)
			{
				eventName = "HOLD_ABILITY_OVER_COLLECTION_code";
			}
			break;
		case CollectionUtils.MercenariesModeCardType.Mercenary:
			if (isOverTarget && !m_wasMouseOverDeck)
			{
				eventName = "HOLD_MERC_OVER_TEAM_TRAY_code";
			}
			else if (!isOverTarget && m_wasMouseOverDeck)
			{
				eventName = "HOLD_MERC_OVER_COLLECTION_code";
			}
			break;
		}
		m_wasMouseOverDeck = isOverTarget;
		if (!string.IsNullOrEmpty(eventName))
		{
			m_mercenariesDraggablesWidget.TriggerEvent(eventName);
		}
	}

	protected virtual void DropCard(bool dragCanceled)
	{
		PegCursor.Get().SetMode(PegCursor.Mode.STOPDRAG);
		if (m_heldCardVisual == null)
		{
			return;
		}
		if (!dragCanceled)
		{
			SoundManager.Get().LoadAndPlay("collection_manager_drop_card.prefab:8275e45efb8280347b35c2548e706d84", m_heldCardVisual.gameObject);
			if (m_cardDroppedCallback != null)
			{
				m_cardDroppedCallback();
				m_cardDroppedCallback = null;
			}
		}
		m_heldCardVisual.Hide();
	}

	public virtual void DropMercenariesModeCard(bool dragCanceled)
	{
		if (m_heldType == CollectionUtils.MercenariesModeCardType.None)
		{
			return;
		}
		PegCursor.Get().SetMode(PegCursor.Mode.STOPDRAG);
		if (m_mercenariesDraggablesWidget == null)
		{
			return;
		}
		if (!dragCanceled)
		{
			this.OnDropMercenariesModeCard?.Invoke(m_heldType, m_heldMercenariesModeCardId);
			if (m_cardDroppedCallback != null)
			{
				m_cardDroppedCallback();
				m_cardDroppedCallback = null;
			}
		}
		m_mercenariesDraggablesWidget.gameObject.transform.position = m_offScreenPosition;
		m_heldMercenariesModeCardId = string.Empty;
		m_heldType = CollectionUtils.MercenariesModeCardType.None;
	}

	public virtual void DropBattlegroundsEmote(bool dragCanceled, bool trayDropCanceled = false)
	{
	}

	protected virtual void OnMouseOnOrOffScreen(bool onScreen)
	{
		if (m_heldCardVisual == null || m_heldCardVisual.gameObject == null)
		{
			return;
		}
		if (onScreen)
		{
			if (m_heldCardOffscreen)
			{
				m_heldCardOffscreen = false;
				if (InputCollection.GetMouseButton(0))
				{
					m_heldCardVisual.Show(MouseIsOverDeck);
				}
				else
				{
					DropCard(dragCanceled: true);
				}
			}
		}
		else if (m_heldCardVisual.IsShown())
		{
			m_heldCardVisual.Hide();
			m_heldCardOffscreen = true;
		}
	}

	protected void DisableDraggableColliders()
	{
		BoxCollider[] colliders = m_mercenariesDraggablesWidget.gameObject.GetComponentsInChildren<BoxCollider>(includeInactive: true);
		if (colliders != null)
		{
			BoxCollider[] array = colliders;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
		}
	}

	protected void DisableBattlegroundsDraggableColliders()
	{
		BoxCollider[] colliders = m_battlegroundsDraggablesWidget.gameObject.GetComponentsInChildren<BoxCollider>(includeInactive: true);
		if (colliders != null)
		{
			BoxCollider[] array = colliders;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
		}
	}

	private void OnMercenariesDraggablesReady(Widget widget)
	{
		if (widget != null)
		{
			m_mercenariesDraggablesWidget = widget;
			DisableDraggableColliders();
		}
	}

	private void OnBattlegroundsDraggablesReady(Widget widget)
	{
		if (widget != null)
		{
			m_battlegroundsDraggablesWidget = widget;
			DisableBattlegroundsDraggableColliders();
		}
	}

	private void OnBattlegroundsDragEmoteSpriteReady(SpriteRenderer spriteRenderer)
	{
		if (spriteRenderer != null)
		{
			m_battlegroundsDragEmoteSprite = spriteRenderer;
		}
	}

	private void OnBattlegroundsEmoteTrayReady(BaconEmoteTray baconEmoteTray)
	{
		SceneMgr.Mode mode = SceneMgr.Get()?.GetMode() ?? SceneMgr.Mode.INVALID;
		if (baconEmoteTray == null && (mode == SceneMgr.Mode.BACON || mode == SceneMgr.Mode.BACON_COLLECTION))
		{
			Log.CollectionManager.PrintError("BaconEmoteTray not found on Battlegrounds emote tray widget reference");
		}
		else
		{
			m_battlegroundsEmoteTray = baconEmoteTray;
		}
	}
}
