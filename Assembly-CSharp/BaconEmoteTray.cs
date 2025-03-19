using System;
using System.Collections;
using System.Collections.Generic;
using Hearthstone;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

[RequireComponent(typeof(VisualController))]
public class BaconEmoteTray : MonoBehaviour
{
	[SerializeField]
	private Widget[] m_emoteWidgets;

	[Tooltip("Pointers for each tray slot's nested BattlegroundsImageWidget")]
	[SerializeField]
	private AsyncReference[] m_asyncImageWidgetReferences;

	private readonly Widget[] m_nestedImageWidgets = new Widget[6];

	private List<Vector3> m_emotePositions;

	[SerializeField]
	private iTween.EaseType m_shuffleEase;

	[SerializeField]
	private float m_shuffleTime;

	protected VisualController m_vc;

	protected Widget m_widget;

	private BattlegroundsEmoteLoadoutDataModel m_loadoutToSave;

	private const int INVALID_EMOTE_INDEX = -1;

	private const int LOADOUT_SIZE = 6;

	private int m_draggedIndex = -1;

	private int m_hoveredEmoteIndex = -1;

	private int m_readyImageWidgets;

	private bool m_trayHovered;

	public void Start()
	{
		m_vc = base.gameObject.GetComponent<VisualController>();
		if (m_vc == null)
		{
			Log.CollectionManager.PrintError("BaconEmoteTray was initialized without a visual controller defined.");
			return;
		}
		m_widget = m_vc.Owner;
		if (m_widget == null)
		{
			Log.CollectionManager.PrintError("BaconEmoteTray was initialized without a widget defined.");
			return;
		}
		m_widget.BindDataModel(CollectionManager.Get().CreateEmoteLoadoutDataModel());
		m_widget.RegisterEventListener(EmoteDisplayEventListener);
		m_emotePositions = new List<Vector3>();
		Widget[] emoteWidgets = m_emoteWidgets;
		foreach (Widget emote in emoteWidgets)
		{
			m_emotePositions.Add(emote.transform.localPosition);
		}
		if (m_asyncImageWidgetReferences.Length != 6)
		{
			Log.CollectionManager.PrintError($"BaconEmoteTray was initialized with incorrect number of async image widget references. Expected {6}, found {m_asyncImageWidgetReferences.Length}");
			return;
		}
		for (int j = 0; j < m_asyncImageWidgetReferences.Length; j++)
		{
			int imageIndex = j;
			m_asyncImageWidgetReferences[j].RegisterReadyListener(delegate(Widget widget)
			{
				m_nestedImageWidgets[imageIndex] = widget;
				m_readyImageWidgets++;
			});
		}
	}

	public void Show(BattlegroundsEmoteLoadoutDataModel dataModel)
	{
		if (m_widget == null)
		{
			Log.CollectionManager.PrintError("BaconEmoteTray was shown without a widget defined.");
			return;
		}
		SetLoadoutDataModel(dataModel);
		StartCoroutine(ShowWhenReady(dataModel));
	}

	private IEnumerator ShowWhenReady(BattlegroundsEmoteLoadoutDataModel dataModel)
	{
		yield return new WaitUntil(() => m_readyImageWidgets == m_asyncImageWidgetReferences.Length);
		UpdateImageWidgetVisibility(dataModel);
		m_widget.TriggerEvent("SHOW");
	}

	public void UpdateImageWidgetVisibility(BattlegroundsEmoteLoadoutDataModel dataModel)
	{
		for (int i = 0; i < dataModel.EmoteList.Count; i++)
		{
			if (dataModel.EmoteList[i].EmoteDbiId == 0)
			{
				m_nestedImageWidgets[i].Hide();
			}
			else
			{
				m_nestedImageWidgets[i].Show();
			}
		}
	}

	public void Hide()
	{
		if (m_widget == null)
		{
			Log.CollectionManager.PrintError("BaconEmoteTray was hidden without a widget defined.");
			return;
		}
		if (GetLoadoutDataModel() != null)
		{
			Network.Get().SetBattlegroundsEmoteLoadout(BattlegroundsEmoteLoadout.MakeFromDatamodel(GetLoadoutDataModel()));
			m_widget.UnbindDataModel(645);
		}
		m_widget.TriggerEvent("HIDE");
	}

	public void OnDestroy()
	{
		if (m_loadoutToSave != null)
		{
			Network network = Network.Get();
			if (network != null)
			{
				network.SetBattlegroundsEmoteLoadout(BattlegroundsEmoteLoadout.MakeFromDatamodel(m_loadoutToSave));
			}
			else
			{
				Debug.Log("Unable to set new BGS emote loadout on network.");
			}
		}
	}

	public void Unload()
	{
		if (m_widget == null)
		{
			Log.CollectionManager.PrintError("BaconEmoteTray was unloaded without a widget defined.");
		}
		else
		{
			m_widget.RemoveEventListener(EmoteDisplayEventListener);
		}
	}

	public void SetLoadoutDataModel(BattlegroundsEmoteLoadoutDataModel dataModel)
	{
		if (dataModel == null)
		{
			Log.CollectionManager.PrintError("BaconEmoteTray.SetLoadoutDataModel - received null datamodel");
			return;
		}
		m_widget.BindDataModel(dataModel);
		m_loadoutToSave = dataModel;
		m_widget.TriggerEvent("UPDATE");
	}

	public void DropOverEmoteTray(BattlegroundsEmoteDataModel dataModel)
	{
		if (IsShufflingEmotes())
		{
			return;
		}
		BattlegroundsEmoteLoadoutDataModel loadoutDataModel = GetLoadoutDataModel();
		if (loadoutDataModel == null)
		{
			Log.CollectionManager.PrintError("BaconEmoteTray - No bound datamodel for emote operations.");
			return;
		}
		if (loadoutDataModel.EmoteList == null)
		{
			Log.CollectionManager.PrintError("BaconEmoteTray - Bound datamodel doesn't contain a valid emote loadout.");
			return;
		}
		if (dataModel == null)
		{
			Log.CollectionManager.PrintError("BaconEmoteTray - New Emote datamodel is null.");
			return;
		}
		int newEmoteIndex = m_hoveredEmoteIndex;
		if (newEmoteIndex == -1)
		{
			DataModelList<BattlegroundsEmoteDataModel> emotes = GetLoadoutDataModel().EmoteList;
			for (int i = 0; i < emotes.Count; i++)
			{
				if (emotes[i].EmoteDbiId == 0 || emotes[i].EmoteDbiId == dataModel.EmoteDbiId)
				{
					newEmoteIndex = i;
					break;
				}
			}
		}
		if (newEmoteIndex != -1)
		{
			if (!m_widget.GetDataModel(645, out var iDataModel))
			{
				Debug.LogWarning("BaconEmoteTray - no valid data model bound to the widget");
				return;
			}
			BattlegroundsEmoteLoadoutDataModel newDataModel = (BattlegroundsEmoteLoadoutDataModel)iDataModel;
			m_emoteWidgets[newEmoteIndex].TriggerEvent("DROP_EFFECTS");
			if (m_draggedIndex != -1)
			{
				bool isFillingEmptySlot = newDataModel.EmoteList[newEmoteIndex].EmoteDbiId == 0;
				int draggedEmoteIndex = m_draggedIndex;
				SwapEmoteDatamodels(newEmoteIndex, m_draggedIndex, newDataModel);
				m_widget.RegisterDoneChangingStatesListener(delegate
				{
					FinishSwappingEmoteImages(isFillingEmptySlot, draggedEmoteIndex, newEmoteIndex);
				}, null, callImmediatelyIfSet: true, doOnce: true);
			}
			else
			{
				BaconCollectionPageManager pageManager = CollectionManager.Get().GetCollectibleDisplay().GetPageManager() as BaconCollectionPageManager;
				if (pageManager != null)
				{
					if (newDataModel.EmoteList[newEmoteIndex] != null)
					{
						pageManager.SetEmoteEquippedState(BattlegroundsEmoteId.FromTrustedValue(newDataModel.EmoteList[newEmoteIndex].EmoteDbiId), isEquipped: false);
					}
					pageManager.SetEmoteEquippedState(BattlegroundsEmoteId.FromTrustedValue(dataModel.EmoteDbiId), isEquipped: true);
				}
				newDataModel.EmoteList[newEmoteIndex] = dataModel;
				SetLoadoutDataModel(newDataModel);
				m_emoteWidgets[newEmoteIndex].RegisterDoneChangingStatesListener(delegate
				{
					m_nestedImageWidgets[newEmoteIndex].Show();
				}, null, callImmediatelyIfSet: true, doOnce: true);
			}
		}
		m_draggedIndex = -1;
	}

	private void FinishSwappingEmoteImages(bool isFillingEmptySlot, int draggedIndex, int hoveredIndex)
	{
		if (!isFillingEmptySlot)
		{
			m_nestedImageWidgets[draggedIndex].Show();
			iTween.Stop(m_emoteWidgets[draggedIndex].gameObject);
			m_emoteWidgets[draggedIndex].transform.localPosition = m_emotePositions[hoveredIndex];
			iTween.MoveTo(m_emoteWidgets[draggedIndex].gameObject, iTween.Hash("position", m_emotePositions[draggedIndex], "time", m_shuffleTime, "easetype", m_shuffleEase, "islocal", true));
		}
		else
		{
			m_nestedImageWidgets[draggedIndex].Hide();
			m_nestedImageWidgets[hoveredIndex].Show();
		}
	}

	public bool IsEmoteOverTray()
	{
		if (!m_trayHovered)
		{
			return m_hoveredEmoteIndex != -1;
		}
		return true;
	}

	public bool IsLoadoutValid()
	{
		return GetLoadoutDataModel() != null;
	}

	public bool IsEmoteInLoadout(int emoteId)
	{
		foreach (BattlegroundsEmoteDataModel emote in GetLoadoutDataModel().EmoteList)
		{
			if (emote.EmoteDbiId == emoteId)
			{
				return true;
			}
		}
		return false;
	}

	public void RemoveEmote(BattlegroundsEmoteDataModel dataModel)
	{
		if (m_draggedIndex == -1)
		{
			Debug.LogError("Tried to remove emote from loadout without a held emote index saved.");
			return;
		}
		m_nestedImageWidgets[m_draggedIndex].Hide();
		if (!m_widget.GetDataModel(645, out var iDataModel))
		{
			Log.CollectionManager.PrintError("BaconEmoteTray - no valid data model bound to the widget");
			return;
		}
		BattlegroundsEmoteLoadoutDataModel newDataModel = (BattlegroundsEmoteLoadoutDataModel)iDataModel;
		BaconCollectionPageManager pageManager = CollectionManager.Get().GetCollectibleDisplay().GetPageManager() as BaconCollectionPageManager;
		if (pageManager != null && newDataModel.EmoteList[m_draggedIndex] != null)
		{
			pageManager.SetEmoteEquippedState(BattlegroundsEmoteId.FromTrustedValue(newDataModel.EmoteList[m_draggedIndex].EmoteDbiId), isEquipped: false);
		}
		newDataModel.EmoteList[m_draggedIndex] = new BattlegroundsEmoteDataModel();
		m_widget.BindDataModel(newDataModel);
		m_loadoutToSave = newDataModel;
		m_draggedIndex = -1;
	}

	public void UpdateTrayHighlight(bool trayHovered)
	{
		if (m_trayHovered != trayHovered)
		{
			m_trayHovered = trayHovered;
			string triggerName = (trayHovered ? "SHOW_TRAY_HIGHLIGHT" : "HIDE_TRAY_HIGHLIGHT");
			Widget[] emoteWidgets = m_emoteWidgets;
			for (int i = 0; i < emoteWidgets.Length; i++)
			{
				emoteWidgets[i].TriggerEvent(triggerName);
			}
		}
	}

	private void EmoteDisplayEventListener(string eventName)
	{
		switch (eventName)
		{
		case "EMOTE_drag_started":
			OnEmoteDragStart();
			break;
		case "EMOTE_drag_released":
			CollectionInputMgr.Get().DropBattlegroundsEmote(dragCanceled: false);
			break;
		case "EMOTE_mouse_over":
			OnEmoteMouseOver();
			break;
		case "EMOTE_mouse_out":
			OnEmoteMouseOut();
			break;
		}
	}

	private void OnEmoteDragStart()
	{
		if (IsShufflingEmotes())
		{
			return;
		}
		EventDataModel eventDataModel = m_widget.GetDataModel<EventDataModel>();
		if (eventDataModel == null)
		{
			Log.CollectionManager.PrintError("No event data model attached to BaconEmoteTray");
			return;
		}
		BattlegroundsEmoteDataModel emoteData = (BattlegroundsEmoteDataModel)eventDataModel.Payload;
		if (emoteData == null || emoteData.EmoteDbiId == 0)
		{
			return;
		}
		m_draggedIndex = -1;
		DataModelList<BattlegroundsEmoteDataModel> loadout = GetLoadoutDataModel().EmoteList;
		for (int loadoutIndex = 0; loadoutIndex < loadout.Count; loadoutIndex++)
		{
			if (loadout[loadoutIndex].EmoteDbiId == emoteData.EmoteDbiId)
			{
				m_draggedIndex = loadoutIndex;
				break;
			}
		}
		if (m_draggedIndex == -1)
		{
			Debug.LogError("Unable to determine which emote was dragged.");
		}
		else
		{
			CollectionInputMgr.Get().GrabBattlegroundsEmote(emoteData, CollectionUtils.BattlegroundsModeDraggableType.TrayEmote, null, m_nestedImageWidgets[m_draggedIndex]);
		}
	}

	private void OnEmoteMouseOver()
	{
		EventDataModel eventDataModel = m_widget.GetDataModel<EventDataModel>();
		if (eventDataModel == null)
		{
			Log.CollectionManager.PrintError("No event data model attached to BaconEmoteTray");
			return;
		}
		if (eventDataModel.Payload is IConvertible convertibleIndex)
		{
			m_hoveredEmoteIndex = Convert.ToInt32(convertibleIndex);
			return;
		}
		Log.CollectionManager.PrintError("Unrecognized event payload in OnEmoteMouseOver().");
		m_hoveredEmoteIndex = -1;
	}

	private void OnEmoteMouseOut()
	{
		EventDataModel eventDataModel = m_widget.GetDataModel<EventDataModel>();
		if (eventDataModel == null)
		{
			Log.CollectionManager.PrintError("No event data model attached to BaconEmoteTray");
		}
		else if (eventDataModel.Payload is IConvertible convertibleIndex)
		{
			if (Convert.ToInt32(convertibleIndex) == m_hoveredEmoteIndex)
			{
				m_hoveredEmoteIndex = -1;
			}
		}
		else
		{
			Log.CollectionManager.PrintError("Unrecognized event payload in OnEmoteMouseOver().");
		}
	}

	private void SwapEmoteDatamodels(int slot1, int slot2, BattlegroundsEmoteLoadoutDataModel dataModel)
	{
		if (slot1 < 0 || slot1 >= 6 || slot2 < 0 || slot2 >= 6)
		{
			Log.CollectionManager.PrintError("BaconEmoteTray - Attempted to swap emote at invalid index");
			return;
		}
		if (dataModel == null)
		{
			Log.CollectionManager.PrintError("BaconEmoteTray - Attempted to swap emote with null datamodel.");
			return;
		}
		BattlegroundsEmoteDataModel temp = dataModel.EmoteList[slot1];
		dataModel.EmoteList[slot1] = dataModel.EmoteList[slot2];
		dataModel.EmoteList[slot2] = temp;
		SetLoadoutDataModel(dataModel);
	}

	public BattlegroundsEmoteLoadoutDataModel GetLoadoutDataModel()
	{
		m_widget.GetDataModel(645, out var dataModel);
		return dataModel as BattlegroundsEmoteLoadoutDataModel;
	}

	public void ShuffleEmotePositions(List<int> newIndices)
	{
		for (int widgetIndex = 0; widgetIndex < m_emoteWidgets.Length; widgetIndex++)
		{
			if (widgetIndex != newIndices[widgetIndex])
			{
				iTween.Stop(m_emoteWidgets[widgetIndex].gameObject);
				m_emoteWidgets[widgetIndex].transform.localPosition = m_emotePositions[newIndices[widgetIndex]];
				iTween.MoveTo(m_emoteWidgets[widgetIndex].gameObject, iTween.Hash("position", m_emotePositions[widgetIndex], "time", m_shuffleTime, "easetype", m_shuffleEase, "islocal", true));
			}
		}
	}

	private bool IsShufflingEmotes()
	{
		Widget[] emoteWidgets = m_emoteWidgets;
		for (int i = 0; i < emoteWidgets.Length; i++)
		{
			if (iTween.Count(emoteWidgets[i].gameObject) > 0)
			{
				return true;
			}
		}
		return false;
	}
}
