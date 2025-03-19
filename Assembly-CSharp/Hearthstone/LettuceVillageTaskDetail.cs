using System.Collections;
using System.Collections.Generic;
using Assets;
using Hearthstone.DataModels;
using Hearthstone.UI;
using JetBrains.Annotations;
using PegasusLettuce;
using UnityEngine;

namespace Hearthstone;

[RequireComponent(typeof(WidgetTemplate))]
public class LettuceVillageTaskDetail : MonoBehaviour
{
	[SerializeField]
	private AsyncReference m_dismissButtonReference;

	private UIBButton m_dismissButton;

	private bool m_isDismissButtonReady;

	[SerializeField]
	private AsyncReference m_purchaseButtonReference;

	private UIBButton m_purchaseButton;

	private bool m_isPurchaseButtonReady;

	[SerializeField]
	private AsyncReference m_previousTaskButtonReference;

	private Clickable m_previousTaskButton;

	private bool m_isPreviousTaskButtonReady;

	[SerializeField]
	private AsyncReference m_nextTaskButtonReference;

	private Clickable m_nextTaskButton;

	private bool m_isNextTaskButtonReady;

	[SerializeField]
	private GameObject m_clickBlocker;

	private Widget m_widget;

	private MercenaryVillageTaskDetailDataModel m_dataModel = new MercenaryVillageTaskDetailDataModel();

	[CanBeNull]
	private VisitorTaskChainDbfRecord m_currentTaskChain;

	[CanBeNull]
	private MercenariesVisitorState m_currentVisitorState;

	[CanBeNull]
	private MercenaryVillageTaskItemDataModel m_currentProcessingTaskData;

	[CanBeNull]
	private GameObject m_bone;

	private Dictionary<int, AudioSource> m_taskIdForBarkVo = new Dictionary<int, AudioSource>();

	private AudioSource m_prevTaskBarkVoAudio;

	private Coroutine m_waitUntilReadyCoroutine;

	public MercenaryVillageTaskItemDataModel CurrentTaskData { get; set; } = new MercenaryVillageTaskItemDataModel();

	private void Awake()
	{
		m_widget = GetComponent<Widget>();
		m_widget.RegisterEventListener(HandleEvent);
		m_widget.BindDataModel(m_dataModel);
	}

	private void Start()
	{
		m_dismissButtonReference.RegisterReadyListener<UIBButton>(OnDismissButtonReady);
		m_purchaseButtonReference.RegisterReadyListener<UIBButton>(OnPurchaseButtonReady);
		m_previousTaskButtonReference.RegisterReadyListener<Clickable>(OnPreviousTaskButtonReady);
		m_nextTaskButtonReference.RegisterReadyListener<Clickable>(OnNextTaskButtonReady);
		m_waitUntilReadyCoroutine = StartCoroutine(WaitUntilReady());
	}

	private void OnDestroy()
	{
		if (m_waitUntilReadyCoroutine != null)
		{
			StopCoroutine(m_waitUntilReadyCoroutine);
			m_waitUntilReadyCoroutine = null;
		}
	}

	private IEnumerator WaitUntilReady()
	{
		while (!m_isDismissButtonReady)
		{
			yield return null;
		}
		while (!m_isPurchaseButtonReady)
		{
			yield return null;
		}
		while (!m_isNextTaskButtonReady || !m_isPreviousTaskButtonReady)
		{
			yield return null;
		}
		m_waitUntilReadyCoroutine = null;
	}

	public void SetBone(GameObject bone)
	{
		m_bone = bone;
	}

	public bool IsProcessing()
	{
		return m_dataModel?.ProcessingRequest ?? false;
	}

	public void SetProcessing(bool isProcessing)
	{
		m_dataModel.ProcessingRequest = isProcessing;
	}

	public void SetRequestRenownConversion(bool isRequested)
	{
		m_dataModel.HasRequestedRenownConversion = isRequested;
	}

	private void HandleEvent(string eventName)
	{
		switch (eventName)
		{
		case "OPEN_TASK_DETAIL":
		{
			SetProcessing(isProcessing: false);
			SetRequestRenownConversion(isRequested: false);
			MercenaryVillageTaskItemDataModel taskData = m_widget.GetDataModel<EventDataModel>().Payload as MercenaryVillageTaskItemDataModel;
			if (taskData == null)
			{
				taskData = CurrentTaskData;
			}
			if (taskData != null)
			{
				VisitorTaskDbfRecord taskRecord = LettuceVillageDataUtil.GetTaskRecordByID(taskData.TaskId);
				if (taskRecord != null)
				{
					m_currentVisitorState = LettuceVillageDataUtil.GetVisitorStateByID(taskRecord.MercenaryVisitorId);
				}
				if (taskData.TaskType == MercenaryVisitor.VillageVisitorType.STANDARD && taskData.TaskChainLength > 1)
				{
					m_currentTaskChain = LettuceVillageDataUtil.GetVisitorTaskChainByID(taskData.TaskChainId);
					m_nextTaskButton.gameObject.SetActive(taskData.TaskChainIndex + 1 < taskData.TaskChainLength);
					m_previousTaskButton.gameObject.SetActive(taskData.TaskChainIndex - 1 >= 0);
				}
				else
				{
					m_currentTaskChain = null;
					m_nextTaskButton.gameObject.SetActive(value: false);
					m_previousTaskButton.gameObject.SetActive(value: false);
				}
				EnableInteraction(enable: true);
				base.transform.localPosition = 1000f * Vector3.forward;
				if (taskData.IsRenownOffer && !TryUpdateRenownDataModel(taskData))
				{
					Log.Lettuce.PrintError("Failed to update renown model");
				}
				if (CurrentTaskData != null && CurrentTaskData.TaskId == taskData.TaskId && CurrentTaskData.Progress == taskData.Progress && CurrentTaskData.TaskStatus == taskData.TaskStatus)
				{
					OnOpened(null);
					break;
				}
				CurrentTaskData = taskData;
				m_widget.BindDataModel(CurrentTaskData);
				m_widget.RegisterDoneChangingStatesListener(OnOpened, null, callImmediatelyIfSet: false, doOnce: true);
			}
			else
			{
				Log.Lettuce.PrintError("Could not handle Open for Task Detail. No task data has been supplied");
			}
			break;
		}
		case "CLOSE_TASK_DETAIL":
			UIContext.GetRoot().DismissPopup(base.gameObject);
			break;
		case "PLAY_CURRENT_BARKVO":
			PlayCurrentVO();
			break;
		}
	}

	private void OnOpened(object _)
	{
		if (m_bone != null)
		{
			base.transform.position = m_bone.transform.position;
		}
		UIContext.GetRoot().ShowPopup(m_widget.gameObject);
	}

	private void OnClaimTaskResponseReceived()
	{
		Network.Get().RemoveNetHandler(MercenariesClaimTaskResponse.PacketID.ID, OnClaimTaskResponseReceived);
		MercenariesClaimTaskResponse mercenariesClaimTaskResponse = Network.Get().ClaimMercenaryTaskResponse();
		m_currentVisitorState.ActiveTaskState.Status_ = MercenariesTaskState.Status.CLAIMED;
		SetProcessing(isProcessing: false);
		if (!mercenariesClaimTaskResponse.Success)
		{
			EnableInteraction(enable: true);
		}
		m_currentProcessingTaskData = null;
	}

	private void OnDismissTaskResponseReceived()
	{
		Network.Get().RemoveNetHandler(MercenariesDismissTaskResponse.PacketID.ID, OnDismissTaskResponseReceived);
		MercenariesDismissTaskResponse mercenariesDismissTaskResponse = Network.Get().DismissMercenaryTaskResponse();
		SetProcessing(isProcessing: false);
		if (mercenariesDismissTaskResponse.Success)
		{
			if (m_currentProcessingTaskData != null)
			{
				LettuceVillageDataUtil.RemoveCompletedOrDismissedTaskDialogue(m_currentProcessingTaskData.TaskId);
			}
		}
		else
		{
			EnableInteraction(enable: true);
		}
		m_currentProcessingTaskData = null;
	}

	private void OnDismissRenownOfferResponseReceived()
	{
		Network.Get().RemoveNetHandler(MercenariesDismissRenownOfferResponse.PacketID.ID, OnDismissRenownOfferResponseReceived);
		MercenariesDismissRenownOfferResponse mercenariesDismissRenownOfferResponse = Network.Get().DismissRenownOfferResponse();
		SetProcessing(isProcessing: false);
		if (!mercenariesDismissRenownOfferResponse.Success)
		{
			EnableInteraction(enable: true);
		}
		m_currentProcessingTaskData = null;
	}

	private void OnPurchaseRenownOfferResponseReceived()
	{
		Network.Get().RemoveNetHandler(MercenariesPurchaseRenownOfferResponse.PacketID.ID, OnPurchaseRenownOfferResponseReceived);
		MercenariesPurchaseRenownOfferResponse mercenariesPurchaseRenownOfferResponse = Network.Get().PurchaseRenownOfferResponse();
		SetProcessing(isProcessing: false);
		if (!mercenariesPurchaseRenownOfferResponse.Success)
		{
			EnableInteraction(enable: true);
		}
		m_currentProcessingTaskData = null;
	}

	private bool TryUpdateRenownDataModel(MercenaryVillageTaskItemDataModel taskData)
	{
		if (taskData == null)
		{
			return false;
		}
		if (!taskData.IsRenownOffer)
		{
			return false;
		}
		foreach (MercenariesRenownOfferData activeRenownOffer in LettuceVillageDataUtil.ActiveRenownStates)
		{
			if (activeRenownOffer.RenownOfferId == taskData.TaskId)
			{
				m_dataModel.RenownTradeData = LettuceRenownUtil.GetCurrentRenownTradeData();
				if (m_dataModel.CurrentRenownOfferData == null)
				{
					m_dataModel.CurrentRenownOfferData = new MercenaryVillageRenownOfferDataModel();
				}
				m_dataModel.CurrentRenownOfferData.OfferName = taskData.Title;
				m_dataModel.CurrentRenownOfferData.RenownCost = activeRenownOffer.RenownCost;
				m_widget.BindDataModel(m_dataModel);
				return true;
			}
		}
		return false;
	}

	private void OnDismissButtonReady(UIBButton button)
	{
		m_isDismissButtonReady = true;
		if (button == null)
		{
			Log.Lettuce.PrintError("DismissButton could not be found.");
			return;
		}
		m_dismissButton = button;
		m_dismissButton.AddEventListener(UIEventType.RELEASE, OnDismissButtonRelease);
	}

	private void OnDismissButtonRelease(UIEvent e)
	{
		if (m_currentProcessingTaskData != null)
		{
			return;
		}
		m_currentProcessingTaskData = CurrentTaskData;
		if (CurrentTaskData == null)
		{
			return;
		}
		SetProcessing(isProcessing: true);
		EnableInteraction(enable: false);
		if (CurrentTaskData.IsRenownOffer)
		{
			int taskId = CurrentTaskData.TaskId;
			Network.Get().RegisterNetHandler(MercenariesDismissRenownOfferResponse.PacketID.ID, OnDismissRenownOfferResponseReceived);
			LettuceRenownUtil.PromptDismissOffer(taskId, OnCancelProcess);
			return;
		}
		int taskId2 = CurrentTaskData.TaskId;
		if (taskId2 > 0 && (CurrentTaskData.Progress >= CurrentTaskData.ProgressNeeded || CurrentTaskData.TaskStatus == MercenariesTaskState.Status.COMPLETE))
		{
			Network.Get().RegisterNetHandler(MercenariesClaimTaskResponse.PacketID.ID, OnClaimTaskResponseReceived);
			LettuceTaskUtil.ClaimTask(taskId2);
		}
		else
		{
			Network.Get().RegisterNetHandler(MercenariesDismissTaskResponse.PacketID.ID, OnDismissTaskResponseReceived);
			LettuceTaskUtil.PromptDismissTask(taskId2, OnCancelProcess);
		}
	}

	private void OnPurchaseButtonReady(UIBButton button)
	{
		m_isPurchaseButtonReady = true;
		if (button == null)
		{
			Log.Lettuce.PrintError("PurchaseButton could not be found.");
			return;
		}
		m_purchaseButton = button;
		m_purchaseButton.AddEventListener(UIEventType.RELEASE, OnPurchaseButtonRelease);
	}

	private void OnPurchaseButtonRelease(UIEvent e)
	{
		if (m_currentProcessingTaskData != null)
		{
			return;
		}
		m_currentProcessingTaskData = CurrentTaskData;
		if (CurrentTaskData != null)
		{
			SetProcessing(isProcessing: true);
			EnableInteraction(enable: false);
			if (CurrentTaskData.IsRenownOffer)
			{
				int taskId = CurrentTaskData.TaskId;
				Network.Get().RegisterNetHandler(MercenariesPurchaseRenownOfferResponse.PacketID.ID, OnPurchaseRenownOfferResponseReceived);
				LettuceRenownUtil.PromptPurchaseOffer(taskId, OnCancelProcess);
			}
			else
			{
				Log.Lettuce.PrintError("Cannot use PurchaseButton with a non-renown offer");
			}
		}
	}

	private void OnConvertButtonRelease(UIEvent e)
	{
		SetRequestRenownConversion(isRequested: true);
	}

	private void OnCancelProcess()
	{
		m_currentProcessingTaskData = null;
		SetProcessing(isProcessing: false);
		EnableInteraction(enable: true);
	}

	private void EnableInteraction(bool enable)
	{
		if (m_dismissButton != null)
		{
			if (m_dismissButton.enabled && !enable)
			{
				m_dismissButton.TriggerOut();
			}
			m_dismissButton.SetEnabled(enable);
			m_dismissButton.Flip(enable);
		}
		if (m_purchaseButton != null)
		{
			if (m_purchaseButton.enabled && !enable)
			{
				m_purchaseButton.TriggerOut();
			}
			m_purchaseButton.SetEnabled(enable);
			m_purchaseButton.Flip(enable);
		}
		if (m_clickBlocker != null)
		{
			m_clickBlocker.SetActive(!enable);
		}
	}

	public bool ShouldCloseAfterClaim()
	{
		if (m_currentTaskChain != null)
		{
			return CurrentTaskData.TaskChainIndex == CurrentTaskData.TaskChainLength - 1;
		}
		return true;
	}

	public void OnTaskRewardsDismissed()
	{
		if (!ShouldCloseAfterClaim())
		{
			m_currentVisitorState = LettuceVillageDataUtil.GetVisitorStateByID(m_currentTaskChain.MercenaryVisitorId);
			EnableInteraction(enable: true);
			NavigateToTaskChainIndex(CurrentTaskData.TaskChainIndex + 1);
		}
	}

	private void OnPreviousTaskButtonReady(Clickable button)
	{
		m_isPreviousTaskButtonReady = true;
		if (button == null)
		{
			Log.Lettuce.PrintError("PreviousTaskButton could not be found.");
			return;
		}
		m_previousTaskButton = button;
		m_previousTaskButton.AddEventListener(UIEventType.RELEASE, OnPreviousTaskButtonReleased);
	}

	private void OnPreviousTaskButtonReleased(UIEvent e)
	{
		if (CurrentTaskData != null)
		{
			NavigateToTaskChainIndex(CurrentTaskData.TaskChainIndex - 1);
		}
	}

	private void OnNextTaskButtonReady(Clickable button)
	{
		m_isNextTaskButtonReady = true;
		if (button == null)
		{
			Log.Lettuce.PrintError("NextTaskButton could not be found.");
			return;
		}
		m_nextTaskButton = button;
		m_nextTaskButton.AddEventListener(UIEventType.RELEASE, OnNextTaskButtonReleased);
	}

	private void OnNextTaskButtonReleased(UIEvent e)
	{
		if (CurrentTaskData != null)
		{
			NavigateToTaskChainIndex(CurrentTaskData.TaskChainIndex + 1);
		}
	}

	private void NavigateToTaskChainIndex(int taskChainIndex)
	{
		if (CurrentTaskData == null || m_currentTaskChain == null)
		{
			return;
		}
		int taskChainLength = m_currentTaskChain.TaskList.Count;
		taskChainIndex = Mathf.Clamp(taskChainIndex, 0, taskChainLength - 1);
		if (taskChainIndex == CurrentTaskData.TaskChainIndex)
		{
			return;
		}
		TaskListDbfRecord taskTarget = m_currentTaskChain.TaskList[taskChainIndex];
		if (taskTarget == null)
		{
			Log.Lettuce.PrintError($"Cannot set task details to task in chain {m_currentTaskChain.ID} at index {taskChainIndex} as the task chain entry is null");
			return;
		}
		VisitorTaskDbfRecord taskRecord = taskTarget.TaskRecord;
		if (taskTarget == null)
		{
			Log.Lettuce.PrintError($"Cannot find task record to task in chain {m_currentTaskChain.ID} at task list entry {taskTarget.ID}");
			return;
		}
		CurrentTaskData = LettuceVillageDataUtil.CreateTaskModel(taskRecord, LettuceVillageDataUtil.GetCurrentProgressForTaskRecord(taskRecord, m_currentVisitorState), taskChainIndex, LettuceVillageDataUtil.GetCurrentTaskStatusForTaskRecord(taskRecord, taskChainIndex, m_currentVisitorState), null, setTaskContext: true);
		m_widget.BindDataModel(CurrentTaskData);
		m_nextTaskButton.gameObject.SetActive(taskChainIndex + 1 < taskChainLength);
		m_previousTaskButton.gameObject.SetActive(taskChainIndex - 1 >= 0);
	}

	public void PlayCurrentVO()
	{
		if (CurrentTaskData != null)
		{
			if (SoundManager.Get().IsPlaying(m_prevTaskBarkVoAudio))
			{
				SoundManager.Get().Stop(m_prevTaskBarkVoAudio);
			}
			if (m_taskIdForBarkVo.ContainsKey(CurrentTaskData.TaskId))
			{
				SoundManager.Get().Play(m_taskIdForBarkVo[CurrentTaskData.TaskId]);
				m_prevTaskBarkVoAudio = m_taskIdForBarkVo[CurrentTaskData.TaskId];
			}
		}
	}

	public void PreloadTaskBarkVo(IEnumerable<MercenariesVisitorState> states)
	{
		foreach (MercenariesVisitorState state in states)
		{
			if (state.HasActiveTaskState)
			{
				MercenaryVisitorDbfRecord visitorRecord = LettuceVillageDataUtil.GetVisitorRecordByID(state.VisitorId);
				if (visitorRecord != null && visitorRecord.VisitorType != 0 && visitorRecord.VisitorType != MercenaryVisitor.VillageVisitorType.PROCEDURAL)
				{
					PreloadTaskBarkVo(state.ActiveTaskState.TaskId);
				}
			}
		}
	}

	private void PreloadTaskBarkVo(int taskId)
	{
		VisitorTaskDbfRecord taskRecord = LettuceVillageDataUtil.GetTaskRecordByID(taskId);
		if (taskRecord == null || m_taskIdForBarkVo.ContainsKey(taskId))
		{
			return;
		}
		string path = taskRecord.MercenaryTaskBarkVo;
		if (string.IsNullOrEmpty(path) || !AssetLoader.Get().IsAssetAvailable(path))
		{
			Log.Lettuce.PrintError($"No path for loading Bark VO object for task id: {taskId} in Lettuce TaskBoard.");
			return;
		}
		GameObject obj = SoundLoader.LoadSound(path);
		if (obj == null)
		{
			Log.Lettuce.PrintError($"Bark VO Object Loading Error for task: {taskId} in Lettuce TaskBoard.");
			return;
		}
		SoundLoader.GetAudioDataForObject(obj, out var barkSource, out var barkDef);
		if (barkDef == null)
		{
			Object.Destroy(obj);
			Log.Lettuce.PrintError($"No sound def for Bark VO Object of task: {taskId} in Lettuce TaskBoard.");
		}
		else if (barkSource == null)
		{
			Object.Destroy(obj);
			Log.Lettuce.PrintError($"No audio source for Bark V of task: {taskId} in Lettuce TaskBoard.");
		}
		else
		{
			m_taskIdForBarkVo[taskId] = barkSource;
		}
	}
}
