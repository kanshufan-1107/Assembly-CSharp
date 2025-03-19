using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.UI;

public class ChooseDeckReward : CustomVisualReward
{
	public AsyncReference m_chooseDeckReference;

	public AsyncReference[] m_classButtonReferences;

	private DeckChoiceDataModel m_deckChoiceDataModel;

	private DeckChoiceDataModel[] m_buttonDataModels;

	private Widget[] m_classButtonWidgets;

	private Widget m_chooseDeckWidget;

	private List<DeckTemplateDbfRecord> m_deckTemplates;

	private int m_selectedDeckTemplateId;

	public override void Start()
	{
		m_classButtonWidgets = new Widget[m_classButtonReferences.Length];
		m_buttonDataModels = new DeckChoiceDataModel[m_classButtonReferences.Length];
		for (int i = 0; i < m_classButtonReferences.Length; i++)
		{
			int classIndex = i;
			m_classButtonReferences[classIndex].RegisterReadyListener(delegate(Widget w)
			{
				SetupDataModelForButton(w, classIndex);
			});
		}
		m_deckChoiceDataModel = new DeckChoiceDataModel();
		m_chooseDeckReference.RegisterReadyListener(delegate(Widget w)
		{
			m_chooseDeckWidget = w;
			w.BindDataModel(m_deckChoiceDataModel);
		});
		m_deckTemplates = GameDbf.DeckTemplate.GetRecords((DeckTemplateDbfRecord deckTemplateRecord) => deckTemplateRecord.IsFreeReward && EventTimingManager.Get().IsEventActive(deckTemplateRecord.Event));
		base.Start();
	}

	public void SetSelectedButtonIndex(int index)
	{
		m_deckChoiceDataModel.ChoiceClassID = (int)GameUtils.ORDERED_HERO_CLASSES[index];
		m_deckChoiceDataModel.ChoiceClassName = GameStrings.GetClassName(GameUtils.ORDERED_HERO_CLASSES[index]);
		DeckTemplateDbfRecord deckTemplateRecord = m_deckTemplates.Find((DeckTemplateDbfRecord record) => record.ClassId == m_deckChoiceDataModel.ChoiceClassID);
		if (deckTemplateRecord == null)
		{
			Log.MissingAssets.PrintError("Could not find a free deck template for class id = {0}", m_deckChoiceDataModel.ChoiceClassID);
		}
		else
		{
			m_selectedDeckTemplateId = deckTemplateRecord.ID;
			m_deckChoiceDataModel.DeckDescription = (string.IsNullOrEmpty(deckTemplateRecord.DeckRecord.AltDescription) ? deckTemplateRecord.DeckRecord.Description : deckTemplateRecord.DeckRecord.AltDescription);
			m_chooseDeckWidget.TriggerEvent("UpdateVisuals");
		}
	}

	public void ChoiceConfirmed()
	{
		AlertPopup.PopupInfo popupInfo = new AlertPopup.PopupInfo();
		popupInfo.m_headerText = GameStrings.Get("GLUE_FREE_DECK_CONFIRMATION_HEADER");
		popupInfo.m_text = GameStrings.Format("GLUE_FREE_DECK_CONFIRMATION_TEXT", m_deckChoiceDataModel.ChoiceClassName);
		popupInfo.m_showAlertIcon = false;
		popupInfo.m_alertTextAlignment = UberText.AlignmentOptions.Center;
		popupInfo.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
		popupInfo.m_responseCallback = delegate(AlertPopup.Response response, object userData)
		{
			if (response == AlertPopup.Response.CONFIRM)
			{
				Network.Get().SendFreeDeckChoice(m_selectedDeckTemplateId, bypassTrialPeriod: false);
				m_chooseDeckWidget.TriggerEvent("COMPLETE");
			}
			else
			{
				m_chooseDeckWidget.TriggerEvent("SHOW");
			}
		};
		AlertPopup.PopupInfo info = popupInfo;
		DialogManager.Get().ShowPopup(info);
	}

	private void SetupDataModelForButton(Widget w, int index)
	{
		string className = GameUtils.ORDERED_HERO_CLASSES[index].ToString();
		DeckChoiceDataModel dataModel = new DeckChoiceDataModel();
		dataModel.ButtonClass = className;
		m_classButtonWidgets[index] = w;
		m_buttonDataModels[index] = dataModel;
		w.BindDataModel(dataModel);
	}
}
