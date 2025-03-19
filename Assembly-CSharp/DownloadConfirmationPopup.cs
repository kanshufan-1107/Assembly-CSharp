using System;
using Hearthstone.Core.Streaming;
using Hearthstone.Streaming;
using Hearthstone.UI;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public class DownloadConfirmationPopup : MonoBehaviour
{
	public class DownloadConfirmationPopupData
	{
		public DownloadTags.Content ModuleTag { get; private set; }

		public Action<DownloadTags.Content> OnYesButtonClick { get; private set; }

		public Action<DownloadTags.Content> OnNoButtonClick { get; private set; }

		public DownloadConfirmationPopupData(DownloadTags.Content moduleTag, Action<DownloadTags.Content> onYesButtonClick, Action<DownloadTags.Content> onNoButtonClick)
		{
			ModuleTag = moduleTag;
			OnYesButtonClick = onYesButtonClick;
			OnNoButtonClick = onNoButtonClick;
		}
	}

	[SerializeField]
	private AsyncReference m_downloadDescriptionTextReference;

	[SerializeField]
	private AsyncReference m_downloadSizeTextReference;

	[SerializeField]
	private AsyncReference m_availableSpaceTextReference;

	[SerializeField]
	private AsyncReference m_downloadYesButtonReference;

	[SerializeField]
	private AsyncReference m_downloadNoButtonReference;

	private UberText m_downloadDescriptionText;

	private UberText m_downloadSizeText;

	private UberText m_availableSpaceText;

	private UIBButton m_downloadYesButton;

	private UIBButton m_downloadNoButton;

	private DownloadConfirmationPopupData m_downloadConfirmationPopupData;

	private void Awake()
	{
		m_downloadDescriptionTextReference.RegisterReadyListener(delegate(UberText downloadDescriptionText)
		{
			m_downloadDescriptionText = downloadDescriptionText;
		});
		m_downloadSizeTextReference.RegisterReadyListener(delegate(UberText downloadSizeText)
		{
			m_downloadSizeText = downloadSizeText;
		});
		m_availableSpaceTextReference.RegisterReadyListener(delegate(UberText availableSpaceText)
		{
			m_availableSpaceText = availableSpaceText;
		});
		m_downloadYesButtonReference.RegisterReadyListener(delegate(UIBButton downloadYesButton)
		{
			m_downloadYesButton = downloadYesButton;
			m_downloadYesButton.AddEventListener(UIEventType.RELEASE, DownloadYesButtonRelease);
		});
		m_downloadNoButtonReference.RegisterReadyListener(delegate(UIBButton downloadNoButton)
		{
			m_downloadNoButton = downloadNoButton;
			m_downloadNoButton.AddEventListener(UIEventType.RELEASE, DownloadNoButtonRelease);
		});
	}

	public void Init(DownloadConfirmationPopupData confirmationPopupData, string descriptionOverride = "GLUE_GAME_MODE_DOWNLOAD_CONFIRMATION_DESCRIPTION")
	{
		m_downloadConfirmationPopupData = confirmationPopupData;
		m_downloadDescriptionText.Text = GameStrings.Format(descriptionOverride, DownloadUtils.GetGameModeName(m_downloadConfirmationPopupData.ModuleTag));
		long downloadSize = GameDownloadManagerProvider.Get().GetModuleDownloadSize(m_downloadConfirmationPopupData.ModuleTag);
		m_downloadSizeText.Text = GameStrings.Format("GLUE_GAME_MODE_DOWNLOAD_CONFIRMATION_DOWNLOAD_SIZE", DownloadUtils.FormatBytesAsHumanReadable(downloadSize));
		long num = FreeSpace.Measure();
		string availableSizeText = DownloadUtils.FormatBytesAsHumanReadable(num);
		if (num < downloadSize)
		{
			availableSizeText = "<color=#ff0000ff>" + availableSizeText + "</color>";
		}
		m_availableSpaceText.Text = GameStrings.Format("GLUE_GAME_MODE_DOWNLOAD_CONFIRMATION_AVAILABLE_SIZE", availableSizeText);
	}

	private void DownloadYesButtonRelease(UIEvent e)
	{
		long downloadSize = GameDownloadManagerProvider.Get().GetModuleDownloadSize(m_downloadConfirmationPopupData.ModuleTag);
		if (FreeSpace.Measure() < downloadSize)
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = GameStrings.Get("GLUE_GAME_MODE_OUT_OF_SPACE_TITLE");
			info.m_text = string.Format(GameStrings.Get("GLUE_GAME_MODE_OUT_OF_SPACE_MESSAGE"), DownloadUtils.FormatBytesAsHumanReadable(downloadSize));
			info.m_showAlertIcon = false;
			info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
			DialogManager.Get().ShowPopup(info);
		}
		else
		{
			m_downloadConfirmationPopupData?.OnYesButtonClick?.Invoke(m_downloadConfirmationPopupData.ModuleTag);
		}
	}

	private void DownloadNoButtonRelease(UIEvent e)
	{
		m_downloadConfirmationPopupData?.OnNoButtonClick?.Invoke(m_downloadConfirmationPopupData.ModuleTag);
	}

	private void OnDestroy()
	{
		m_downloadYesButton.RemoveEventListener(UIEventType.RELEASE, DownloadYesButtonRelease);
		m_downloadNoButton.RemoveEventListener(UIEventType.RELEASE, DownloadNoButtonRelease);
	}
}
