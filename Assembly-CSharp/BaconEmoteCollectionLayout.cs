using System;
using System.Collections.Generic;
using Hearthstone;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(VisualController))]
public class BaconEmoteCollectionLayout : MonoBehaviour
{
	private VisualController m_mainController;

	[SerializeField]
	private float m_animationTime = 0.15f;

	private bool m_animating;

	[SerializeField]
	private Widget[] m_emoteWidgets;

	private List<BaconCollectionEmoteLayoutWidgetBehaviour> m_emoteBehaviors = new List<BaconCollectionEmoteLayoutWidgetBehaviour>();

	[SerializeField]
	private Widget m_draggableWidget;

	[SerializeField]
	private AsyncReference m_draggableReference;

	[SerializeField]
	private int m_dragSortOffset;

	private BaconCollectionEmoteLayoutWidgetBehaviour m_draggableBehavior;

	private BattlegroundsEmoteDataModel m_draggedDatamodel;

	private int m_draggedIndex = -1;

	private bool m_draggingEmote;

	private bool m_allowDrag = true;

	[SerializeField]
	private float m_returnTime;

	[SerializeField]
	private iTween.EaseType m_returnEase;

	private Vector3 m_offScreenPosition;

	private Camera m_fxCamera;

	private const int INVALID_EMOTE_INDEX = -1;

	private List<int> m_updatedEmoteIndices = new List<int>();

	private BaconEmoteTray m_emoteTray;

	private ScreenEffectsHandle m_screenEffectsHandle;

	public void Show(BattlegroundsEmoteLoadoutDataModel dataModel, BaconEmoteTray tray)
	{
		if (m_animating)
		{
			return;
		}
		if (m_mainController == null)
		{
			Debug.LogWarning("BaconEmoteCollectionLayout was shown without a m_mainController defined.");
			return;
		}
		m_emoteTray = tray;
		m_mainController.Owner.BindDataModel(dataModel);
		m_mainController.Owner.TriggerEvent("UPDATE");
		CollectiblePageManager cpm = CollectionManager.Get().GetCollectibleDisplay().GetPageManager();
		if (cpm != null)
		{
			cpm.EnablePageTurn(enable: false);
			cpm.EnablePageTurnArrows(enable: false);
		}
		ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignetteDesaturatePerspective;
		screenEffectParameters.Time = m_animationTime;
		m_screenEffectsHandle.StartEffect(screenEffectParameters);
		base.gameObject.SetActive(value: true);
		m_animating = true;
		iTween.ScaleFrom(base.gameObject, iTween.Hash("scale", new Vector3(0.01f, 0.01f, 0.01f), "time", m_animationTime, "easetype", iTween.EaseType.easeOutCirc, "oncomplete", (Action<object>)delegate
		{
			m_animating = false;
		}));
		m_fxCamera = CameraUtils.FindFullScreenEffectsCamera(activeOnly: false);
		m_updatedEmoteIndices.Clear();
		for (int i = 0; i < m_emoteWidgets.Length; i++)
		{
			m_updatedEmoteIndices.Add(i);
		}
	}

	public void Hide()
	{
		if (m_animating)
		{
			return;
		}
		if (m_mainController == null)
		{
			Debug.LogWarning("BaconEmoteCollectionLayout was hidden without a m_mainController defined.");
			return;
		}
		CollectiblePageManager cpm = CollectionManager.Get().GetCollectibleDisplay().GetPageManager();
		if (cpm != null)
		{
			cpm.EnablePageTurn(enable: true);
			cpm.EnablePageTurnArrows(enable: true);
		}
		m_screenEffectsHandle.StopEffect();
		Vector3 origScale = base.transform.localScale;
		m_animating = true;
		iTween.ScaleTo(base.gameObject, iTween.Hash("scale", new Vector3(0.01f, 0.01f, 0.01f), "time", m_animationTime, "easetype", iTween.EaseType.easeOutCirc, "oncomplete", (Action<object>)delegate
		{
			m_animating = false;
			if (base.gameObject != null)
			{
				base.transform.localScale = origScale;
				base.gameObject.SetActive(value: false);
				m_draggableWidget.Hide();
			}
		}));
		BattlegroundsEmoteLoadoutDataModel dataModel = GetLoadoutDataModel();
		if (dataModel == null)
		{
			Debug.LogWarning("Tried to save new emote layout without a bound datamodel.");
			return;
		}
		BattlegroundsEmoteLoadout loadout = BattlegroundsEmoteLoadout.MakeFromDatamodel(dataModel);
		Network.Get().SetBattlegroundsEmoteLoadout(loadout);
		((BaconCollectionDisplay)CollectionManager.Get().GetCollectibleDisplay()).SetEmoteLoadout(dataModel);
		m_emoteTray.ShuffleEmotePositions(m_updatedEmoteIndices);
	}

	private void Start()
	{
		m_mainController = base.gameObject.GetComponent<VisualController>();
		m_mainController.GetComponent<Widget>().RegisterEventListener(LayoutEventListener);
		base.gameObject.SetActive(value: false);
		m_offScreenPosition = m_draggableWidget.transform.position;
		m_draggableBehavior = m_draggableWidget.GetComponent<BaconCollectionEmoteLayoutWidgetBehaviour>();
		m_draggableReference.RegisterReadyListener<Transform>(delegate
		{
			m_draggableWidget.GetComponentInChildren<SortingGroup>().sortingOrder += m_dragSortOffset;
		});
		Widget[] emoteWidgets = m_emoteWidgets;
		foreach (Widget emote in emoteWidgets)
		{
			m_emoteBehaviors.Add(emote.GetComponent<BaconCollectionEmoteLayoutWidgetBehaviour>());
		}
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	private void Update()
	{
		if (m_draggingEmote)
		{
			if (!CameraUtils.Raycast(m_fxCamera, InputCollection.GetMousePosition(), GameLayer.DragPlane.LayerBit(), out var hit) || !InputCollection.GetMouseButton(0) || !InputUtil.IsMouseOnScreen())
			{
				OnEmoteDrop();
			}
			else
			{
				m_draggableWidget.gameObject.transform.position = hit.point;
			}
		}
	}

	public void Unload()
	{
		m_mainController.GetComponent<Widget>().RemoveEventListener(LayoutEventListener);
	}

	public void SwapEmotes(int slot1, int slot2)
	{
		BattlegroundsEmoteLoadoutDataModel dataModel = GetLoadoutDataModel();
		if (dataModel == null || dataModel.EmoteList == null || dataModel.EmoteList.Count != 6)
		{
			Debug.LogWarning("Unable to retrieve datamodel with a valid emote loadout.");
			return;
		}
		BattlegroundsEmoteDataModel temp = dataModel.EmoteList[slot1];
		dataModel.EmoteList[slot1] = dataModel.EmoteList[slot2];
		dataModel.EmoteList[slot2] = temp;
		int tempIndex = m_updatedEmoteIndices[slot1];
		m_updatedEmoteIndices[slot1] = m_updatedEmoteIndices[slot2];
		m_updatedEmoteIndices[slot2] = tempIndex;
		m_mainController.Owner.BindDataModel(dataModel);
		m_mainController.Owner.TriggerEvent("UPDATE");
	}

	private int GetHoveredEmote()
	{
		for (int i = 0; i < m_emoteBehaviors.Count; i++)
		{
			if (UniversalInputManager.Get().ForcedUnblockableInputIsOver(CameraUtils.FindFullScreenEffectsCamera(activeOnly: false), m_emoteBehaviors[i].GetDragCollider().gameObject, out var _))
			{
				return i;
			}
		}
		return -1;
	}

	private void LayoutEventListener(string eventName)
	{
		switch (eventName)
		{
		case "OffDialogClick_code":
			Hide();
			break;
		case "EMOTE_drag_started":
			OnEmoteDragStart();
			break;
		case "EMOTE_drag_released":
			OnEmoteDrop();
			break;
		}
	}

	private void OnEmoteDragStart()
	{
		EventDataModel eventDataModel = m_mainController.Owner.GetDataModel<EventDataModel>();
		if (eventDataModel == null)
		{
			Log.All.PrintError("Tried to drag without event datamodel");
		}
		else if (!(eventDataModel.Payload is BattlegroundsEmoteDataModel emoteData))
		{
			Log.All.PrintError("Recieved event without emote datamodel.");
		}
		else
		{
			if (emoteData.EmoteDbiId == 0 || !m_allowDrag)
			{
				return;
			}
			m_allowDrag = false;
			m_draggedDatamodel = emoteData;
			m_draggedIndex = -1;
			int currentIndex = 0;
			foreach (BattlegroundsEmoteDataModel emote in GetLoadoutDataModel().EmoteList)
			{
				if (emote.EmoteDbiId == m_draggedDatamodel.EmoteDbiId)
				{
					m_draggedIndex = currentIndex;
					break;
				}
				currentIndex++;
			}
			m_draggableWidget.Hide();
			BindAndConfigureDraggableWidget(emoteData);
			m_draggableWidget.RegisterDoneChangingStatesListener(delegate
			{
				PickUpEmote();
			}, null, callImmediatelyIfSet: true, doOnce: true);
			SoundManager.Get().LoadAndPlay("collection_manager_pick_up_card.prefab:f7fb595cdc26f2f4997b4a10eaf1d0e1", m_draggableWidget.gameObject);
		}
	}

	private void PickUpEmote()
	{
		PegCursor.Get().SetMode(PegCursor.Mode.DRAG);
		m_draggingEmote = true;
		m_emoteWidgets[m_draggedIndex].Hide();
		m_emoteWidgets[m_draggedIndex].GetComponentInChildren<PegUIElement>().SetEnabled(enabled: true);
		m_draggableWidget.Show();
		m_draggableWidget.TriggerEvent("PICKUP_EFFECTS");
	}

	private void OnEmoteDrop()
	{
		if (!m_draggingEmote)
		{
			return;
		}
		m_draggingEmote = false;
		PegCursor.Get().SetMode(PegCursor.Mode.STOPDRAG);
		bool showDroppedEmoteOnTop = false;
		int droppedSortOffset = m_dragSortOffset * 2;
		Vector3 startPos = default(Vector3);
		int hoveredEmoteIndex = GetHoveredEmote();
		if (hoveredEmoteIndex != -1 && hoveredEmoteIndex != m_draggedIndex)
		{
			BattlegroundsEmoteDataModel emoteData = GetLoadoutDataModel().EmoteList[hoveredEmoteIndex];
			BindAndConfigureDraggableWidget(emoteData);
			m_emoteWidgets[hoveredEmoteIndex].TriggerEvent("SHOW");
			showDroppedEmoteOnTop = true;
			m_emoteBehaviors[hoveredEmoteIndex].IncreaseSpriteSortOrder(droppedSortOffset);
			SwapEmotes(hoveredEmoteIndex, m_draggedIndex);
			startPos = m_emoteWidgets[hoveredEmoteIndex].transform.position;
			m_emoteWidgets[hoveredEmoteIndex].TriggerEvent("DROP_EFFECTS");
		}
		else
		{
			startPos = m_draggableWidget.gameObject.transform.position;
		}
		int returnIndex = m_draggedIndex;
		m_draggableWidget.transform.position = startPos;
		iTween.MoveTo(m_draggableWidget.gameObject, iTween.Hash("position", m_emoteWidgets[returnIndex].transform.position, "time", m_returnTime, "easetype", m_returnEase, "oncomplete", (Action<object>)delegate
		{
			if (showDroppedEmoteOnTop)
			{
				m_emoteBehaviors[hoveredEmoteIndex].IncreaseSpriteSortOrder(-droppedSortOffset);
			}
			m_draggableWidget.gameObject.transform.position = m_offScreenPosition;
			m_emoteWidgets[returnIndex].GetComponent<Widget>().Show();
			m_allowDrag = true;
		}));
		m_draggedIndex = -1;
		SoundManager.Get().LoadAndPlay("collection_manager_drop_card.prefab:8275e45efb8280347b35c2548e706d84", m_draggableWidget.gameObject);
	}

	private void BindAndConfigureDraggableWidget(BattlegroundsEmoteDataModel emoteDataModel)
	{
		m_draggableWidget.BindDataModel(emoteDataModel);
		if (m_emoteBehaviors[m_draggedIndex].m_flipBubble)
		{
			m_draggableWidget.TriggerEvent("POINT_RIGHT");
		}
		else
		{
			m_draggableWidget.TriggerEvent("POINT_LEFT");
		}
	}

	private BattlegroundsEmoteLoadoutDataModel GetLoadoutDataModel()
	{
		return m_mainController.Owner.GetDataModel<BattlegroundsEmoteLoadoutDataModel>();
	}
}
