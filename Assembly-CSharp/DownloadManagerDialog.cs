using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Hearthstone.Core.Streaming;
using Hearthstone.DataModels;
using Hearthstone.Streaming;
using Hearthstone.UI;
using UnityEngine;

[RequireComponent(typeof(Widget))]
public class DownloadManagerDialog : MonoBehaviour, IWidgetEventListener
{
	public enum DownloadStatus
	{
		NOT_REQUESTED,
		DOWNLOADING,
		PAUSED,
		CANCELLING,
		DOWNLOADED,
		QUEUED,
		ERROR
	}

	private interface IProgressTracker
	{
		string ID { get; }

		string Name { get; }

		DownloadStatus Status { get; }

		long TotalSize { get; }

		long DownloadedSize { get; }

		float Progress { get; }

		bool HasTotalSizeChanged { get; }

		void AcknowledgeChanges();
	}

	private abstract class ProgressTracker : IProgressTracker
	{
		public DownloadStatus m_status;

		private long m_totalSize;

		private float m_progress;

		private long m_downloadedSize;

		public abstract string ID { get; }

		public abstract string Name { get; }

		public bool HasTotalSizeChanged { get; private set; }

		public DownloadStatus Status
		{
			get
			{
				return m_status;
			}
			set
			{
				m_status = value;
			}
		}

		public long TotalSize => m_totalSize;

		public long DownloadedSize => m_downloadedSize;

		public float Progress => m_progress;

		public void Init(long totalSize, long downloadedSize, float progress)
		{
			HasTotalSizeChanged = m_totalSize != totalSize;
			m_totalSize = totalSize;
			m_downloadedSize = downloadedSize;
			m_progress = progress;
		}

		public void UpdateProgress(long downloadedSize, float progress)
		{
			m_downloadedSize = downloadedSize;
			m_progress = progress;
		}

		public void AcknowledgeChanges()
		{
			HasTotalSizeChanged = false;
		}
	}

	private class ModuleProgressTracker : ProgressTracker
	{
		private readonly string m_name;

		private readonly DownloadTags.Content m_tag;

		public override string ID => DownloadTags.GetTagString(Tag);

		public override string Name => m_name;

		public DownloadTags.Content Tag => m_tag;

		public ModuleProgressTracker(DownloadTags.Content tag, string name)
		{
			m_tag = tag;
			m_name = name;
		}
	}

	private class OptionalAssetProgressTracker : ProgressTracker
	{
		private readonly string m_name = GameStrings.Get("GLOBAL_DOWNLOAD_MANAGER_OPTIONAL_ASSETS");

		public override string ID => "assets";

		public override string Name => m_name;
	}

	private class Divider : IProgressTracker
	{
		public string ID => "divider";

		public string Name => "";

		public DownloadStatus Status => DownloadStatus.NOT_REQUESTED;

		public long TotalSize => 0L;

		public long DownloadedSize => 0L;

		public float Progress => 0f;

		public bool HasTotalSizeChanged => false;

		public void AcknowledgeChanges()
		{
		}
	}

	private class OptionData
	{
		public string NameLabel;

		public string DescriptionLabel;

		public Func<bool> GetIsCheckedValue;

		public Action OnSelectCallback;
	}

	public enum DownloaderState
	{
		Idle,
		Downloading,
		Paused,
		Error
	}

	public const string RESUME_PAUSE_EVENT = "RESUME_PAUSE_ACTION_EVENT";

	public const string MODULE_MAIN_EVENT = "MODULE_MAIN_ACTION_EVENT";

	public const string OPTION_SELECT_EVENT = "OPTION_SELECT_EVENT";

	private const string TOGGLE_ENABLE_DOWNLOAD_EVENT = "TOGGLE_ENABLE_DOWNLOAD_EVENT";

	private const string TOGGLE_ENABLE_CELLULAR_EVENT = "TOGGLE_ENABLE_CELLULAR_EVENT";

	private const string DISMISS_DIALOG_EVENT = "DISMISS_DIALOG_EVENT";

	private const float STATUS_UPDATE_INTERVAL = 1f;

	private static readonly AssetReference DownloadManagerMenu = new AssetReference("DownloadManagerMenu.prefab:0c10aa76eba5ad64789641980c15122d");

	private static readonly AssetReference s_alertPopupReference = new AssetReference("AlertPopup.prefab:504e85137efb24846826d3ce0f056765");

	[SerializeField]
	private Widget m_downloadConfirmationPopupWidget;

	[SerializeField]
	private Widget m_uninstallLoadingPopupWidget;

	private List<IProgressTracker> m_progressTrackers = new List<IProgressTracker>();

	private List<OptionData> m_optionDatas = new List<OptionData>();

	private Dictionary<string, ModuleProgressTracker> m_progressTrackerLookup = new Dictionary<string, ModuleProgressTracker>();

	private DownloaderState m_downloaderState;

	private GameObject m_popupRoot;

	private WidgetTemplate m_widgetTemplate;

	private DownloadConfirmationPopup m_downloadConfirmationPopup;

	private GenericLoadingPopup m_uninstallLoadingPopup;

	private OptionalAssetProgressTracker m_optionalAssetProgressTracker;

	private Coroutine m_statusUpdateCR;

	private Coroutine m_showInstallConfirmationPopupCR;

	private Coroutine m_showUninstallLoadingPopupCR;

	WidgetTemplate IWidgetEventListener.OwningWidget => m_widgetTemplate;

	protected void Awake()
	{
		m_popupRoot = base.transform.parent.gameObject;
		if (!m_popupRoot)
		{
			throw new Exception("DownloadManagerDialog: PopupRoot not found in parent!");
		}
		m_widgetTemplate = GetComponent<WidgetTemplate>();
		InitializeModuleData();
		InitializeOptionData();
		m_widgetTemplate.RegisterReadyListener(OnWidgetReady);
		DownloadPermissionManager.OnCellularPermissionChanged += OnCellularPermissionChanged;
	}

	protected void OnDestroy()
	{
		DownloadPermissionManager.OnCellularPermissionChanged -= OnCellularPermissionChanged;
		if (!(m_popupRoot == null))
		{
			UnityEngine.Object.Destroy(m_popupRoot);
		}
	}

	public void OnShow()
	{
		OverlayUI.Get().AddGameObject(m_popupRoot);
		UIContext.GetRoot().ShowPopup(m_popupRoot);
		BnetBar.Get().RequestDisableButtons();
		SendEventDownwardStateAction.SendEventDownward(base.gameObject, "SHOW", SendEventDownwardStateAction.BubbleDownEventDepth.DirectChildrenOnly);
	}

	public void OnHide()
	{
		DismissDownloadConfirmationPopup();
		DismissUninstallLoadingPopup();
		SendEventDownwardStateAction.SendEventDownward(base.gameObject, "HIDE", SendEventDownwardStateAction.BubbleDownEventDepth.DirectChildrenOnly);
		BnetBar.Get().CancelRequestToDisableButtons();
		if (!(m_popupRoot == null))
		{
			UIContext.GetRoot().DismissPopup(m_popupRoot);
			UnityEngine.Object.Destroy(m_popupRoot);
		}
	}

	public static void ShowMe()
	{
		WidgetInstance.Create(DownloadManagerMenu);
	}

	private void OnEnable()
	{
		if (m_widgetTemplate.IsReady)
		{
			StartModuleStatusUpdate();
		}
	}

	private void OnDisable()
	{
		if (m_statusUpdateCR != null)
		{
			StopCoroutine(m_statusUpdateCR);
			m_statusUpdateCR = null;
		}
		if (m_showInstallConfirmationPopupCR != null)
		{
			StopCoroutine(m_showInstallConfirmationPopupCR);
			m_showInstallConfirmationPopupCR = null;
		}
	}

	private void InitializeModuleData()
	{
		IGameDownloadManager downloadManager = GameDownloadManagerProvider.Get();
		if (downloadManager != null)
		{
			DownloadTags.Content[] moduleTags = downloadManager.DownloadableModuleTags;
			foreach (DownloadTags.Content tag in moduleTags)
			{
				string nameLabel = DownloadUtils.GetGameModeName(tag);
				AddModuleTracker(tag, nameLabel);
				m_progressTrackers.Add(new Divider());
			}
		}
		m_optionalAssetProgressTracker = new OptionalAssetProgressTracker();
		m_progressTrackers.Add(m_optionalAssetProgressTracker);
		m_progressTrackers.Add(new Divider());
		UpdateData(initial: true);
	}

	private void InitializeOptionData()
	{
		m_optionDatas.Add(new OptionData
		{
			NameLabel = GameStrings.Get("GLOBAL_DOWNLOAD_MANAGER_OPTION_CELLULAR_DATA_LABEL"),
			DescriptionLabel = GameStrings.Get("GLOBAL_DOWNLOAD_MANAGER_OPTION_CELLULAR_DATA_DESC"),
			GetIsCheckedValue = () => DownloadPermissionManager.CellularEnabled,
			OnSelectCallback = OnToggleCellularDownload
		});
	}

	private void AddModuleTracker(DownloadTags.Content tag, string name)
	{
		ModuleProgressTracker tracker = new ModuleProgressTracker(tag, name);
		m_progressTrackers.Add(tracker);
		m_progressTrackerLookup[tracker.ID] = tracker;
	}

	private void StartModuleStatusUpdate()
	{
		if (m_statusUpdateCR == null)
		{
			m_statusUpdateCR = StartCoroutine(UpdateDataCoroutine());
		}
	}

	private void UpdateData(bool initial)
	{
		DownloadManagerDialogDataModel dataModel = m_widgetTemplate.GetDataModel<DownloadManagerDialogDataModel>();
		IGameDownloadManager downloadManager = GameDownloadManagerProvider.Get();
		bool hasProgressChanged = false;
		for (int i = 0; i < m_progressTrackers.Count; i++)
		{
			IProgressTracker progressTracker = m_progressTrackers[i];
			if (!(progressTracker is ModuleProgressTracker moduleProgressTracker))
			{
				if (!(progressTracker is OptionalAssetProgressTracker optionalProgressTracker))
				{
					if (!(progressTracker is Divider))
					{
						throw new Exception($"Unhandled progress tracker type: {progressTracker.GetType()}");
					}
					continue;
				}
				UpdateOptionalAssetsProgress(downloadManager, optionalProgressTracker, initial);
			}
			else
			{
				UpdateModuleProgress(downloadManager, moduleProgressTracker, initial);
			}
			if (dataModel != null)
			{
				DownloadManagerDialogModuleItemDataModel itemDataModel = dataModel.ModuleList[i];
				if (!hasProgressChanged)
				{
					hasProgressChanged = !Mathf.Approximately(itemDataModel.Progress, progressTracker.Progress);
				}
				itemDataModel.Status = progressTracker.Status;
				itemDataModel.Progress = progressTracker.Progress;
				itemDataModel.DownloadedSizeLabel = DownloadUtils.FormatBytesAsHumanReadable(progressTracker.DownloadedSize);
				if (progressTracker.HasTotalSizeChanged)
				{
					itemDataModel.TotalSizeLabel = DownloadUtils.FormatBytesAsHumanReadable(progressTracker.TotalSize);
				}
				progressTracker.AcknowledgeChanges();
			}
		}
		if (dataModel != null)
		{
			if (hasProgressChanged)
			{
				dataModel.SizeAvailableLabel = DownloadUtils.FormatBytesAsHumanReadable(FreeSpace.Measure());
			}
			dataModel.OptionalAssetsDownloadEnabled = DownloadPermissionManager.DownloadEnabled;
			for (int j = 0; j < dataModel.OptionList.Count; j++)
			{
				bool value = m_optionDatas[j].GetIsCheckedValue();
				dataModel.OptionList[j].IsChecked = value;
			}
			m_downloaderState = GetDownloaderState(downloadManager);
			dataModel.DownloaderState = m_downloaderState;
		}
	}

	private void ShowModuleDownloadPopup(ModuleProgressTracker tracker)
	{
		if (m_showInstallConfirmationPopupCR == null)
		{
			m_showInstallConfirmationPopupCR = StartCoroutine(ShowDownloadConfirmationPopupCR(tracker.Tag));
		}
	}

	private IEnumerator ShowDownloadConfirmationPopupCR(DownloadTags.Content content)
	{
		if (m_downloadConfirmationPopup == null)
		{
			m_downloadConfirmationPopupWidget.RegisterReadyListener(delegate
			{
				m_downloadConfirmationPopup = m_downloadConfirmationPopupWidget.GetComponentInChildren<DownloadConfirmationPopup>(includeInactive: true);
			});
		}
		while (m_downloadConfirmationPopup == null)
		{
			yield return null;
		}
		DownloadConfirmationPopup.DownloadConfirmationPopupData confirmationPopupData = new DownloadConfirmationPopup.DownloadConfirmationPopupData(content, OnDownloadPopupYesClicked, OnDownloadPopupNoClicked);
		m_downloadConfirmationPopup.Init(confirmationPopupData);
		UIContext.GetRoot().ShowPopup(m_downloadConfirmationPopupWidget.gameObject);
		m_downloadConfirmationPopupWidget.TriggerEvent("SHOW");
		m_showInstallConfirmationPopupCR = null;
		void OnDownloadPopupNoClicked(DownloadTags.Content content)
		{
			DismissDownloadConfirmationPopup();
		}
		void OnDownloadPopupYesClicked(DownloadTags.Content content)
		{
			GameDownloadManagerProvider.Get().DownloadModule(content);
			UpdateData(initial: false);
			DismissDownloadConfirmationPopup();
		}
	}

	private void DismissDownloadConfirmationPopup()
	{
		if (m_showInstallConfirmationPopupCR != null)
		{
			StopCoroutine(m_showInstallConfirmationPopupCR);
			m_showInstallConfirmationPopupCR = null;
		}
		m_downloadConfirmationPopupWidget.TriggerEvent("HIDE");
		if (m_downloadConfirmationPopup != null)
		{
			UIContext.GetRoot().DismissPopup(m_downloadConfirmationPopupWidget.gameObject);
		}
	}

	private void ShowModuleUninstallPopup(ModuleProgressTracker tracker, bool isCancel)
	{
		string title = GameStrings.Get(isCancel ? "GLOBAL_DOWNLOAD_MANAGER_CANCEL_MODULE_POPUP_TITLE" : "GLOBAL_DOWNLOAD_MANAGER_UNINSTALL_MODULE_POPUP_TITLE");
		string prompt = GameStrings.Format(isCancel ? "GLOBAL_DOWNLOAD_MANAGER_CANCEL_MODULE_POPUP_PROMPT" : "GLOBAL_DOWNLOAD_MANAGER_UNINSTALL_MODULE_POPUP_PROMPT", tracker.Name);
		ShowUninstallConfirmationPopup(title, prompt, tracker.TotalSize, OnPopupResponded);
		void OnPopupResponded(AlertPopup.Response response)
		{
			if (response != AlertPopup.Response.CANCEL)
			{
				ShowUninstallLoadingPopup(() => UninstallModule(tracker.Tag, isCancel), isCancel);
			}
		}
	}

	private async UniTask UninstallModule(DownloadTags.Content tag, bool isCancel)
	{
		await GameDownloadManagerProvider.Get().DeleteModuleAsync(tag);
		string message = (isCancel ? "GLOBAL_DOWNLOAD_MANAGER_CANCEL_LOADING_POPUP_CANCEL_MESSAGE" : "GLOBAL_DOWNLOAD_MANAGER_UNINSTALL_LOADING_POPUP_COMPLETE_MESSAGE");
		SetUninstallLoadingPopupComplete(message);
		UpdateData(initial: false);
	}

	private void ShowOptionalAssetsUninstallPopup()
	{
		string title = GameStrings.Get("GLOBAL_DOWNLOAD_MANAGER_UNINSTALL_OPTIONAL_ASSETS_POPUP_TITLE");
		string prompt = GameStrings.Get("GLOBAL_DOWNLOAD_MANAGER_UNINSTALL_OPTIONAL_ASSETS_POPUP_PROMPT");
		ShowUninstallConfirmationPopup(title, prompt, m_optionalAssetProgressTracker.TotalSize, OnPopupResponded);
		void OnPopupResponded(AlertPopup.Response response)
		{
			if (response != AlertPopup.Response.CANCEL)
			{
				DownloadPermissionManager.DownloadEnabled = false;
				if (GameDownloadManagerProvider.Get() != null)
				{
					ShowUninstallLoadingPopup(UninstallOptionalAssets, isCancel: false);
				}
			}
		}
	}

	private async UniTask UninstallOptionalAssets()
	{
		await GameDownloadManagerProvider.Get().DeleteOptionalAssetsAsync();
		SetUninstallLoadingPopupComplete("GLOBAL_DOWNLOAD_MANAGER_UNINSTALL_LOADING_POPUP_COMPLETE_MESSAGE");
		UpdateData(initial: false);
	}

	private void ShowUninstallConfirmationPopup(string title, string prompt, long moduleSize, Action<AlertPopup.Response> callback)
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		string fileSize = GameStrings.Format("GLOBAL_DOWNLOAD_MODULE_POPUP_FILE_SIZE", DownloadUtils.FormatBytesAsHumanReadable(moduleSize));
		string freeSpace = GameStrings.Format("GLOBAL_DOWNLOAD_MODULE_POPUP_FREE_SPACE", DownloadUtils.FormatBytesAsHumanReadable(FreeSpace.Measure()));
		info.m_headerText = title;
		info.m_text = GameStrings.Format("GLOBAL_DOWNLOAD_MANAGER_UNINSTALL_MODULE_POPUP_BODY_FORMAT", prompt, fileSize, freeSpace);
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
		info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
		info.m_showAlertIcon = false;
		info.m_blurWhenShown = false;
		info.m_layerToUse = GameLayer.UI;
		info.m_scaleOverride = Vector3.one;
		info.m_responseCallback = delegate(AlertPopup.Response response, object userData)
		{
			callback?.Invoke(response);
		};
		DialogManager.Get().ShowPopup(info);
	}

	private void ShowUninstallLoadingPopup(Func<UniTask> onShownCallback, bool isCancel)
	{
		if (m_showUninstallLoadingPopupCR == null)
		{
			m_showUninstallLoadingPopupCR = StartCoroutine(ShowUninstallLoadingPopupCR(onShownCallback, isCancel));
		}
	}

	private IEnumerator ShowUninstallLoadingPopupCR(Func<UniTask> onShownCallback, bool isCancel)
	{
		if (m_uninstallLoadingPopup == null)
		{
			m_uninstallLoadingPopupWidget.RegisterReadyListener(delegate
			{
				m_uninstallLoadingPopup = m_uninstallLoadingPopupWidget.GetComponentInChildren<GenericLoadingPopup>();
			});
		}
		while (m_uninstallLoadingPopup == null)
		{
			yield return null;
		}
		string message = (isCancel ? "GLOBAL_DOWNLOAD_MANAGER_CANCEL_LOADING_POPUP_LOADING_MESSAGE" : "GLOBAL_DOWNLOAD_MANAGER_UNINSTALL_LOADING_POPUP_LOADING_MESSAGE");
		m_uninstallLoadingPopup.SetLoading(message);
		UIContext.GetRoot().ShowPopup(m_uninstallLoadingPopupWidget.gameObject);
		m_uninstallLoadingPopupWidget.TriggerEvent("SHOW");
		onShownCallback?.Invoke();
	}

	private void SetUninstallLoadingPopupComplete(string message)
	{
		m_uninstallLoadingPopup.SetCompleted(message, DismissUninstallLoadingPopup);
	}

	private void DismissUninstallLoadingPopup()
	{
		if (m_showUninstallLoadingPopupCR != null)
		{
			StopCoroutine(m_showUninstallLoadingPopupCR);
			m_showInstallConfirmationPopupCR = null;
		}
		m_uninstallLoadingPopupWidget.TriggerEvent("HIDE");
		if (m_uninstallLoadingPopupWidget != null)
		{
			UIContext.GetRoot().DismissPopup(m_uninstallLoadingPopupWidget.gameObject);
		}
	}

	private void UpdateModuleProgress(IGameDownloadManager downloadManager, ModuleProgressTracker tracker, bool initial)
	{
		if (downloadManager == null)
		{
			tracker.Status = DownloadStatus.DOWNLOADED;
			tracker.Init(0L, 0L, 1f);
			return;
		}
		DownloadStatus status = GetModuleStatus(downloadManager, tracker.Tag);
		tracker.Status = status;
		if (initial)
		{
			TagDownloadStatus moduleDownloadStatus = downloadManager.GetModuleDownloadStatus(tracker.Tag);
			long totalSizeInByte = downloadManager.GetModuleDownloadSize(tracker.Tag);
			DownloadUtils.CalculateModuleDownloadProgress(moduleDownloadStatus, totalSizeInByte, out var bytesDownloaded, out var progress);
			tracker.Init(totalSizeInByte, bytesDownloaded, progress);
			return;
		}
		switch (tracker.Status)
		{
		case DownloadStatus.NOT_REQUESTED:
			tracker.UpdateProgress(0L, 0f);
			break;
		case DownloadStatus.DOWNLOADED:
			tracker.UpdateProgress(tracker.TotalSize, 1f);
			break;
		case DownloadStatus.DOWNLOADING:
			if (downloadManager.AssetDownloaderState == AssetDownloaderState.DOWNLOADING)
			{
				TagDownloadStatus downloadStatus = downloadManager.GetModuleDownloadStatus(tracker.Tag);
				if (downloadStatus.BytesTotal > 0)
				{
					DownloadUtils.CalculateModuleDownloadProgress(downloadStatus, tracker.TotalSize, out var bytesDownloaded2, out var progress2);
					tracker.UpdateProgress(bytesDownloaded2, progress2);
				}
			}
			break;
		case DownloadStatus.PAUSED:
		case DownloadStatus.CANCELLING:
		case DownloadStatus.QUEUED:
		case DownloadStatus.ERROR:
			break;
		}
	}

	private void UpdateOptionalAssetsProgress(IGameDownloadManager downloadManager, OptionalAssetProgressTracker tracker, bool initial)
	{
		if (downloadManager == null)
		{
			tracker.Status = DownloadStatus.DOWNLOADED;
			tracker.Init(0L, 0L, 1f);
			return;
		}
		DownloadStatus optionalStatus = GetOptionalStatus(downloadManager);
		tracker.Status = optionalStatus;
		long totalBytes = downloadManager.GetOptionalAssetsDownloadSize();
		switch (tracker.Status)
		{
		case DownloadStatus.DOWNLOADED:
			tracker.Init(totalBytes, totalBytes, 1f);
			break;
		case DownloadStatus.NOT_REQUESTED:
			tracker.Init(totalBytes, 0L, 0f);
			break;
		case DownloadStatus.DOWNLOADING:
			if (downloadManager.AssetDownloaderState == AssetDownloaderState.DOWNLOADING)
			{
				TagDownloadStatus downloadStatus = downloadManager.GetOptionalDownloadStatus();
				if (downloadStatus.BytesTotal > 0)
				{
					DownloadUtils.CalculateModuleDownloadProgress(downloadStatus, totalBytes, out var bytesDownloaded, out var progress);
					tracker.Init(totalBytes, bytesDownloaded, progress);
				}
			}
			else if (totalBytes != tracker.TotalSize)
			{
				tracker.Init(totalBytes, tracker.DownloadedSize, (float)tracker.DownloadedSize / (float)tracker.TotalSize);
			}
			break;
		default:
			if (totalBytes != tracker.TotalSize)
			{
				tracker.Init(totalBytes, tracker.DownloadedSize, (float)tracker.DownloadedSize / (float)tracker.TotalSize);
			}
			break;
		}
	}

	private static DownloadStatus GetModuleStatus(IGameDownloadManager downloadManager, DownloadTags.Content moduleTag)
	{
		ModuleState state = downloadManager.GetModuleState(moduleTag);
		switch (state)
		{
		case ModuleState.NotRequested:
			return DownloadStatus.NOT_REQUESTED;
		case ModuleState.Queued:
			return DownloadStatus.QUEUED;
		case ModuleState.Downloading:
			return GetInterruptionReason(downloadManager.InterruptionReason);
		case ModuleState.ReadyToPlay:
		case ModuleState.FullyInstalled:
			return DownloadStatus.DOWNLOADED;
		default:
			Log.Downloader.PrintError(string.Format("{0}: Unhandled ModuleState {1}", "DownloadManagerDialog", state));
			return DownloadStatus.ERROR;
		}
	}

	private static DownloadStatus GetOptionalStatus(IGameDownloadManager downloadManager)
	{
		if (!DownloadPermissionManager.DownloadEnabled)
		{
			return DownloadStatus.NOT_REQUESTED;
		}
		if (downloadManager.IsOptionalDownloadCompleted())
		{
			return DownloadStatus.DOWNLOADED;
		}
		if (!downloadManager.IsCompletedInitialBaseDownload() || !downloadManager.IsCompletedInitialModulesDownload())
		{
			return DownloadStatus.QUEUED;
		}
		return GetInterruptionReason(downloadManager.InterruptionReason);
	}

	private static DownloadStatus GetInterruptionReason(InterruptionReason interruptionReason)
	{
		switch (interruptionReason)
		{
		case InterruptionReason.Disabled:
			return DownloadStatus.NOT_REQUESTED;
		case InterruptionReason.None:
		case InterruptionReason.Fetching:
			return DownloadStatus.DOWNLOADING;
		case InterruptionReason.Paused:
		case InterruptionReason.AwaitingWifi:
			return DownloadStatus.PAUSED;
		case InterruptionReason.Error:
		case InterruptionReason.DiskFull:
		case InterruptionReason.AgentImpeded:
			return DownloadStatus.ERROR;
		default:
			throw new ArgumentException($"Unknown interruption reason {interruptionReason}!");
		}
	}

	private static DownloaderState GetDownloaderState(IGameDownloadManager downloadManager)
	{
		if (downloadManager == null)
		{
			return DownloaderState.Idle;
		}
		if (!downloadManager.IsAnyDownloadRequestedAndIncomplete && (!DownloadPermissionManager.DownloadEnabled || downloadManager.IsOptionalDownloadCompleted()))
		{
			return DownloaderState.Idle;
		}
		switch (downloadManager.InterruptionReason)
		{
		case InterruptionReason.Paused:
			return DownloaderState.Paused;
		case InterruptionReason.Disabled:
			return DownloaderState.Idle;
		case InterruptionReason.Error:
		case InterruptionReason.DiskFull:
		case InterruptionReason.AgentImpeded:
			return DownloaderState.Error;
		default:
			return DownloaderState.Downloading;
		}
	}

	private IEnumerator UpdateDataCoroutine()
	{
		WaitForSeconds wait = new WaitForSeconds(1f);
		while (true)
		{
			UpdateData(initial: false);
			yield return wait;
		}
	}

	private void OnWidgetReady(object payload)
	{
		DownloadManagerDialogDataModel dataModel = new DownloadManagerDialogDataModel();
		for (int i = 0; i < m_progressTrackers.Count; i++)
		{
			IProgressTracker tracker = m_progressTrackers[i];
			DownloadManagerDialogModuleItemDataModel itemDataModel = new DownloadManagerDialogModuleItemDataModel
			{
				ID = tracker.ID,
				NameLabel = tracker.Name,
				Status = tracker.Status,
				TotalSizeLabel = DownloadUtils.FormatBytesAsHumanReadable(tracker.TotalSize),
				DownloadedSizeLabel = DownloadUtils.FormatBytesAsHumanReadable(tracker.DownloadedSize)
			};
			dataModel.ModuleList.Add(itemDataModel);
		}
		for (int j = 0; j < m_optionDatas.Count; j++)
		{
			OptionData option = m_optionDatas[j];
			DownloadManagerDialogOptionCheckboxDataModel optionDataModel = new DownloadManagerDialogOptionCheckboxDataModel
			{
				ID = j,
				NameLabel = option.NameLabel,
				DescriptionLabel = option.DescriptionLabel,
				IsChecked = option.GetIsCheckedValue()
			};
			dataModel.OptionList.Add(optionDataModel);
		}
		dataModel.SizeAvailableLabel = DownloadUtils.FormatBytesAsHumanReadable(FreeSpace.Measure());
		m_widgetTemplate.BindDataModel(dataModel);
		StartModuleStatusUpdate();
		OnShow();
	}

	private void OnToggleResumePause()
	{
		IGameDownloadManager downloadManager = GameDownloadManagerProvider.Get();
		if (downloadManager == null)
		{
			Log.Downloader.PrintWarning("DownloadManagerDialog: Resume/Pause is ignored as IGameDownloadManager is null.");
			return;
		}
		switch (downloadManager.InterruptionReason)
		{
		case InterruptionReason.None:
			downloadManager.PauseAllDownloads(requestedByPlayer: true);
			break;
		case InterruptionReason.Paused:
			downloadManager.ResumeAllDownloads();
			break;
		default:
			Log.Downloader.PrintWarning(string.Format("{0}: Resume/Pause is ignored as the interruption reason is unhandled ({1}).", "DownloadManagerDialog", downloadManager.InterruptionReason));
			break;
		}
		UpdateData(initial: false);
	}

	private void OnModuleAction(object eventPayload)
	{
		if (!TryGetEventModuleTracker(eventPayload, out var tracker))
		{
			return;
		}
		if (GameDownloadManagerProvider.Get() == null)
		{
			Log.Downloader.PrintWarning("DownloadManagerDialog: Module action is ignored as IGameDownloadManager is null.");
			return;
		}
		switch (tracker.Status)
		{
		case DownloadStatus.NOT_REQUESTED:
			ShowModuleDownloadPopup(tracker);
			break;
		case DownloadStatus.DOWNLOADING:
		case DownloadStatus.PAUSED:
		case DownloadStatus.QUEUED:
			ShowModuleUninstallPopup(tracker, isCancel: true);
			break;
		case DownloadStatus.DOWNLOADED:
			ShowModuleUninstallPopup(tracker, isCancel: false);
			break;
		default:
			Log.Downloader.PrintError(string.Format("{0}: Unexpected module status for the main button press: {1}", "DownloadManagerDialog", tracker.Status));
			break;
		}
		UpdateData(initial: false);
	}

	private void OnOptionSelected(object eventPayload)
	{
		if (eventPayload == null)
		{
			Log.Downloader.PrintError("DownloadManagerDialog: Event payload is null!");
			return;
		}
		int index = (int)eventPayload;
		if (index < 0 || index >= m_optionDatas.Count)
		{
			Log.Downloader.PrintError(string.Format("{0}: Invalid option index ({1})!", "DownloadManagerDialog", index));
			return;
		}
		m_optionDatas[index].OnSelectCallback?.Invoke();
		UpdateData(initial: false);
	}

	private void OnToggleCellularDownload()
	{
		DownloadPermissionManager.CellularEnabled = !DownloadPermissionManager.CellularEnabled;
		UpdateData(initial: false);
	}

	private void OnCellularPermissionChanged()
	{
		UpdateData(initial: false);
	}

	private void OnToggleOptionalAssetDownload()
	{
		IGameDownloadManager downloadManager = GameDownloadManagerProvider.Get();
		if (downloadManager != null)
		{
			if (DownloadPermissionManager.DownloadEnabled)
			{
				ShowOptionalAssetsUninstallPopup();
				return;
			}
			DownloadPermissionManager.DownloadEnabled = true;
			downloadManager.StartUpdateProcessForOptional();
		}
	}

	private void OnDismissDialog()
	{
		OnHide();
	}

	private bool TryGetEventModuleTracker(object eventPayload, out ModuleProgressTracker tracker)
	{
		tracker = null;
		if (eventPayload == null)
		{
			Log.Downloader.PrintError("DownloadManagerDialog: Event payload is null!");
			return false;
		}
		string id = (string)eventPayload;
		if (string.IsNullOrEmpty(id) || !m_progressTrackerLookup.TryGetValue(id, out var moduleProgressTracker))
		{
			Log.Downloader.PrintError("DownloadManagerDialog: Module index (" + id + ") is invalid!");
			return false;
		}
		tracker = moduleProgressTracker;
		return tracker != null;
	}

	WidgetEventListenerResponse IWidgetEventListener.EventReceived(string eventName, TriggerEventParameters eventParams)
	{
		bool consumed = true;
		switch (eventName)
		{
		case "MODULE_MAIN_ACTION_EVENT":
			OnModuleAction(eventParams.Payload);
			break;
		case "RESUME_PAUSE_ACTION_EVENT":
			OnToggleResumePause();
			break;
		case "OPTION_SELECT_EVENT":
			OnOptionSelected(eventParams.Payload);
			break;
		case "DISMISS_DIALOG_EVENT":
			OnDismissDialog();
			break;
		case "TOGGLE_ENABLE_CELLULAR_EVENT":
			OnToggleCellularDownload();
			break;
		case "TOGGLE_ENABLE_DOWNLOAD_EVENT":
			OnToggleOptionalAssetDownload();
			break;
		default:
			consumed = false;
			break;
		}
		WidgetEventListenerResponse result = default(WidgetEventListenerResponse);
		result.Consumed = consumed;
		return result;
	}
}
