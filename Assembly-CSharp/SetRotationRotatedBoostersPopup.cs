using System;
using System.Collections;
using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public class SetRotationRotatedBoostersPopup : BasicPopup
{
	public class SetRotationRotatedBoostersPopupInfo : PopupInfo
	{
		public Action m_onHiddenCallback;
	}

	private Widget m_widget;

	private const int NUM_DISPLAY_PACKS = 3;

	private const string SHOW_EVENT_NAME = "CODE_DIALOGMANAGER_SHOW";

	private const string HIDE_EVENT_NAME = "CODE_DIALOGMANAGER_HIDE";

	private const string HIDE_FINISHED_EVENT_NAME = "CODE_HIDE_FINISHED";

	private SetRotationRotatedBoostersPopupInfo m_info;

	protected override void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(delegate(string eventName)
		{
			if (eventName == "Button_Framed_Clicked")
			{
				Hide();
			}
			if (eventName == "CODE_HIDE_FINISHED")
			{
				if (m_popupInfo is SetRotationRotatedBoostersPopupInfo { m_onHiddenCallback: not null } setRotationRotatedBoostersPopupInfo)
				{
					setRotationRotatedBoostersPopupInfo.m_onHiddenCallback();
				}
				if (m_readyToDestroyCallback != null)
				{
					m_readyToDestroyCallback(this);
				}
			}
		});
		BindRankedPackListDataModel();
	}

	public override void Show()
	{
		if (!(m_widget == null))
		{
			OverlayUI.Get().AddGameObject(m_widget.gameObject);
			UIContext.GetRoot().ShowPopup(m_widget.gameObject);
			Vector3 popUpScale = base.transform.localScale;
			base.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
			m_widget.TriggerEvent("CODE_DIALOGMANAGER_SHOW");
			if (!string.IsNullOrEmpty(m_showAnimationSound))
			{
				SoundManager.Get().LoadAndPlay(m_showAnimationSound);
			}
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("scale", popUpScale);
			args.Add("time", 0.3f);
			args.Add("easetype", iTween.EaseType.easeOutBack);
			iTween.ScaleTo(base.gameObject, args);
		}
	}

	public override void Hide()
	{
		m_widget.TriggerEvent("CODE_DIALOGMANAGER_HIDE");
	}

	private void BindRankedPackListDataModel()
	{
		PackListDataModel dataModel = new PackListDataModel();
		EventTimingManager events = EventTimingManager.Get();
		List<BoosterDbfRecord> records = GameDbf.Booster.GetRecords((BoosterDbfRecord r) => events.IsEventActive(r.BuyWithGoldEvent));
		records.Sort((BoosterDbfRecord a, BoosterDbfRecord b) => b.LatestExpansionOrder.CompareTo(a.LatestExpansionOrder));
		foreach (BoosterDbfRecord boosterDbfRecord in records)
		{
			if (GameUtils.IsBoosterRotated((BoosterDbId)boosterDbfRecord.ID, DateTime.UtcNow))
			{
				PackDataModel packData = new PackDataModel();
				packData.Type = (BoosterDbId)boosterDbfRecord.ID;
				packData.BoosterName = boosterDbfRecord.Name;
				dataModel.Packs.Insert(0, packData);
				if (dataModel.Packs.Count >= 3)
				{
					break;
				}
			}
		}
		m_widget.BindDataModel(dataModel);
	}
}
