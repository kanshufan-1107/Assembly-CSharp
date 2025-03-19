using System;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class DownloadManagerDialogModuleItem : MonoBehaviour, IWidgetEventListener
{
	[Serializable]
	public class ProgressBarComponents
	{
		public ProgressBar ProgressBar;

		public Renderer Renderer;
	}

	private const string BUTTON_MAIN_EVENT = "BUTTON_MAIN_PRESSED";

	[SerializeField]
	private Material m_progressBarDownloadingMaterial;

	[SerializeField]
	private Material m_progressBarPausedMaterial;

	[SerializeField]
	private ProgressBarComponents[] m_progressBarComponents;

	private WidgetTemplate m_rootWidget;

	public WidgetTemplate OwningWidget => m_rootWidget;

	private void Awake()
	{
		m_rootWidget = GetComponent<WidgetTemplate>();
		m_rootWidget.RegisterReadyListener(OnWidgetReady);
		if (Application.isPlaying)
		{
			m_progressBarDownloadingMaterial = new Material(m_progressBarDownloadingMaterial);
			m_progressBarPausedMaterial = new Material(m_progressBarPausedMaterial);
		}
	}

	private void OnWidgetReady(object payload)
	{
		DownloadManagerDialogModuleItemDataModel dataModel = m_rootWidget.GetDataModel<DownloadManagerDialogModuleItemDataModel>();
		dataModel.RegisterChangedListener(OnDataModelChanged);
		OnDataModelChanged(dataModel);
	}

	private void OnMainButtonPressed()
	{
		DownloadManagerDialogModuleItemDataModel itemDataModel = m_rootWidget.GetDataModel<DownloadManagerDialogModuleItemDataModel>();
		SendEventUpwardStateAction.SendEventUpward(base.gameObject, "MODULE_MAIN_ACTION_EVENT", new EventDataModel
		{
			Payload = itemDataModel.ID
		});
	}

	private void OnDataModelChanged(object _)
	{
		DownloadManagerDialogModuleItemDataModel dataModel = m_rootWidget.GetDataModel<DownloadManagerDialogModuleItemDataModel>();
		if (dataModel == null)
		{
			return;
		}
		Material material = GetProgressBarMaterial(dataModel.Status);
		for (int i = 0; i < m_progressBarComponents.Length; i++)
		{
			if (!(m_progressBarComponents[i].Renderer.material == material))
			{
				m_progressBarComponents[i].Renderer.SetMaterial(material);
				m_progressBarComponents[i].ProgressBar.SetMaterial(material);
			}
		}
	}

	private Material GetProgressBarMaterial(DownloadManagerDialog.DownloadStatus status)
	{
		switch (status)
		{
		default:
			return m_progressBarDownloadingMaterial;
		case DownloadManagerDialog.DownloadStatus.PAUSED:
		case DownloadManagerDialog.DownloadStatus.QUEUED:
			return m_progressBarPausedMaterial;
		}
	}

	public WidgetEventListenerResponse EventReceived(string eventName, TriggerEventParameters eventParams)
	{
		bool consumed = false;
		if (eventName == "BUTTON_MAIN_PRESSED")
		{
			OnMainButtonPressed();
			consumed = true;
		}
		WidgetEventListenerResponse result = default(WidgetEventListenerResponse);
		result.Consumed = consumed;
		return result;
	}
}
