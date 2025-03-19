using System.Collections.Generic;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Hearthstone.Core;
using Hearthstone.DataModels;
using Hearthstone.InGameMessage.UI;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.InGameMessage;

public class MessageModalContentPager : MonoBehaviour
{
	public List<Widget> m_contentWidgets = new List<Widget>();

	private int m_activeWidget;

	private int m_currentVisibleDataIndex;

	private List<MessageUIData> m_messageDataList;

	private List<int> m_contentDataIds = new List<int>();

	private void Awake()
	{
		Log.InGameMessage.Log(LogLevel.Debug, "** content pager awake");
		foreach (Widget contentWidget in m_contentWidgets)
		{
			MessageModalDataModel modalDataModel = new MessageModalDataModel
			{
				LayoutType = MessageModalUtils.GetLayoutTypeID(MessageLayoutType.EMPTY)
			};
			contentWidget.BindDataModel(modalDataModel);
			contentWidget.Hide();
		}
	}

	public void SetMessageUIData(List<MessageUIData> dataList)
	{
		Log.InGameMessage.Log(LogLevel.Debug, "** content pager SetMessageUIData");
		m_messageDataList = dataList;
		SetContentDataModel(m_messageDataList[m_currentVisibleDataIndex], m_contentWidgets[m_activeWidget]);
		ShowWidget(m_activeWidget);
		SetNextContentWidget();
	}

	public void OnPageButtonPressed(int currentVisibleDataIndex, bool nextPressed)
	{
		Log.InGameMessage.Log(LogLevel.Debug, $"** content pager OnPageButtonPressed: currentVisibleDataIndex {currentVisibleDataIndex}");
		m_currentVisibleDataIndex = currentVisibleDataIndex;
		m_activeWidget = (nextPressed ? GetNextWidgetIndex() : GetPrevWidgetIndex());
		ShowWidget(m_activeWidget);
		SetNextContentWidget();
		SetPrevContentWidget();
	}

	private void SetNextContentWidget()
	{
		if (m_currentVisibleDataIndex < m_messageDataList.Count - 1)
		{
			Log.InGameMessage.Log(LogLevel.Debug, $"** content pager SetNextContentWidget: m_currentVisibleDataIndex {m_currentVisibleDataIndex} , m_messageDataList.Count {m_messageDataList.Count} ");
			SetContentDataModel(m_messageDataList[m_currentVisibleDataIndex + 1], m_contentWidgets[GetNextWidgetIndex()]);
		}
	}

	private void SetPrevContentWidget()
	{
		if (m_currentVisibleDataIndex > 0)
		{
			Log.InGameMessage.Log(LogLevel.Debug, $"** content pager SetPrevContentWidget: m_currentVisibleDataIndex {m_currentVisibleDataIndex} , m_messageDataList.Count {m_messageDataList.Count} ");
			SetContentDataModel(m_messageDataList[m_currentVisibleDataIndex - 1], m_contentWidgets[GetPrevWidgetIndex()]);
		}
	}

	private void ShowWidget(int widgetToShow)
	{
		Log.InGameMessage.Log(LogLevel.Debug, $"** content pager ShowWidget {widgetToShow}");
		List<WaitForJob> allWidgitsInitJobs = new List<WaitForJob>();
		foreach (Widget contentWidget in m_contentWidgets)
		{
			JobDefinition job = new JobDefinition("IGM_WaitForWidgetInit", WaitForWidgetInit(contentWidget));
			Processor.QueueJob(job);
			allWidgitsInitJobs.Add(job.CreateDependency());
		}
		IEnumerator<IAsyncJobResult> jobAction = OnAllWidgetInit(widgetToShow);
		IJobDependency[] dependencies = allWidgitsInitJobs.ToArray();
		Processor.QueueJob("IGM_WaitAllWidgetInit", jobAction, dependencies);
	}

	private IEnumerator<IAsyncJobResult> WaitForWidgetInit(Widget widget)
	{
		while (widget.InitState != Widget.InitializationState.Done)
		{
			yield return null;
		}
		Log.InGameMessage.Log(LogLevel.Debug, "** Content pager ready:" + widget.gameObject.name);
	}

	private IEnumerator<IAsyncJobResult> OnAllWidgetInit(int widgetToShow)
	{
		Log.InGameMessage.Log(LogLevel.Debug, "** content pager OnAllWidgetInit");
		foreach (Widget contentWidget in m_contentWidgets)
		{
			contentWidget.Hide();
			Clickable[] componentsInChildren = contentWidget.GetComponentsInChildren<Clickable>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Active = false;
			}
			Collider[] componentsInChildren2 = contentWidget.GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].enabled = false;
			}
		}
		Log.InGameMessage.Log(LogLevel.Debug, $"** content pager OnAllWidgetInit Show content widget:{widgetToShow}");
		m_contentWidgets[widgetToShow].Show();
		Clickable[] clickablesToShow = m_contentWidgets[widgetToShow].GetComponentsInChildren<Clickable>();
		if (clickablesToShow != null)
		{
			Clickable[] componentsInChildren = clickablesToShow;
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Active = true;
			}
		}
		Collider[] collidersToShow = m_contentWidgets[widgetToShow].GetComponentsInChildren<Collider>();
		if (collidersToShow != null)
		{
			Collider[] componentsInChildren2 = collidersToShow;
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].enabled = true;
			}
		}
		yield return null;
	}

	private int GetNextWidgetIndex()
	{
		if (m_activeWidget != m_contentWidgets.Count - 1)
		{
			return m_activeWidget + 1;
		}
		return 0;
	}

	private int GetPrevWidgetIndex()
	{
		if (m_activeWidget != 0)
		{
			return m_activeWidget - 1;
		}
		return m_contentWidgets.Count - 1;
	}

	private void SetContentDataModel(MessageUIData data, Widget widget)
	{
		Log.InGameMessage.Log(LogLevel.Debug, "** content pager SetContentDataModel :" + widget.gameObject.name + "." + widget.transform.parent.name);
		List<IDataModel> contentDataModels = MessageDataModelFactory.CreateDataModel(data);
		if (contentDataModels == null)
		{
			Log.InGameMessage.PrintError("MessageModalContentPager - Could not create a content model for IGM data");
			return;
		}
		if (widget == null)
		{
			Log.InGameMessage.PrintError("MessageModalContentPager - Missing Content Widget! IGM will not work correctly");
			return;
		}
		foreach (int contentDataId in m_contentDataIds)
		{
			widget.UnbindDataModel(contentDataId);
		}
		foreach (IDataModel contentDataModel in contentDataModels)
		{
			widget.BindDataModel(contentDataModel);
			if (!m_contentDataIds.Contains(contentDataModel.DataModelId))
			{
				m_contentDataIds.Add(contentDataModel.DataModelId);
			}
		}
		MessageModalDataModel modalDataModel = new MessageModalDataModel
		{
			LayoutType = MessageModalUtils.GetLayoutTypeID(data.LayoutType)
		};
		widget.BindDataModel(modalDataModel);
	}
}
