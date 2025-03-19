using System.Collections.Generic;
using Assets;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class ChooseOneDeckRewardDisplay : MonoBehaviour
{
	public List<Widget> m_deckButtons;

	private DataModelList<RewardItemDataModel> m_deckRewards;

	private Widget m_widget;

	private LoanerDeckDetailsController m_deckDetailsController;

	private LoanerDecksInfoDataModel m_deckInfoDataModel;

	public const string CODE_CLAIM_CHOOSE_ONE_REWARD = "CODE_CLAIM_CHOOSE_ONE_REWARD";

	private void Awake()
	{
		m_widget = base.gameObject.GetComponent<Widget>();
		m_widget.RegisterReadyListener(OnWidgetReady);
		m_widget.RegisterActivatedListener(OnWidgetActivate);
		m_deckInfoDataModel = new LoanerDecksInfoDataModel();
		m_deckInfoDataModel.DeckChoiceTemplateId = 0;
		m_widget.BindDataModel(m_deckInfoDataModel);
		m_deckDetailsController = m_widget.GetComponentInChildren<LoanerDeckDetailsController>();
	}

	private void OnWidgetReady(object unused)
	{
		if (!UpdateDecksWithRewardList())
		{
			return;
		}
		for (int i = 0; i < m_deckRewards.Count; i++)
		{
			Widget buttonWidget = m_deckButtons[i];
			RewardItemDataModel deckReward = m_deckRewards[i];
			int rewardIndex = i;
			buttonWidget.RegisterReadyListener(delegate
			{
				LoanerDeckSelectButton componentInChildren = buttonWidget.GetComponentInChildren<LoanerDeckSelectButton>();
				if (componentInChildren == null)
				{
					Log.Decks.PrintError("Could not find LoanerDeckSelectButton component on deck selection button");
					buttonWidget.Hide();
				}
				else
				{
					buttonWidget.RegisterEventListener(componentInChildren.OnDeckChoiceButtonClicked);
					buttonWidget.BindDataModel(new DeckChoiceDataModel());
					ConfigureButtonForDeckReward(buttonWidget, deckReward);
					if (rewardIndex == 0)
					{
						componentInChildren.OnDeckChoiceButtonClicked("Selected");
					}
				}
			});
		}
	}

	private void OnWidgetActivate(object unused)
	{
		if (!UpdateDecksWithRewardList())
		{
			return;
		}
		for (int i = 0; i < m_deckRewards.Count; i++)
		{
			Widget buttonWidget = m_deckButtons[i];
			int num = i;
			ConfigureButtonForDeckReward(buttonWidget, m_deckRewards[i]);
			if (num == 0)
			{
				buttonWidget.GetComponentInChildren<LoanerDeckSelectButton>().OnDeckChoiceButtonClicked("Selected");
			}
		}
	}

	private bool UpdateDecksWithRewardList()
	{
		RewardListDataModel rewards = m_widget.GetDataModel<RewardListDataModel>();
		if (rewards == null)
		{
			Log.All.PrintError("ChooseOneDeckRewardDisplay popup opened without deck reward data");
			return false;
		}
		m_deckRewards = rewards.Items;
		if (m_deckButtons == null || m_deckButtons.Count < m_deckRewards.Count)
		{
			Log.Decks.PrintError("Not enough button widgets for available decks");
			return false;
		}
		return true;
	}

	private void ConfigureButtonForDeckReward(Widget buttonWidget, RewardItemDataModel deckReward)
	{
		LoanerDeckSelectButton selectButton = buttonWidget.GetComponentInChildren<LoanerDeckSelectButton>();
		if (selectButton == null)
		{
			Log.Decks.PrintError("Could not find LoanerDeckSelectButton component on deck selection button");
			buttonWidget.Hide();
			return;
		}
		DeckPouchDataModel deck = deckReward.Deck;
		buttonWidget.GetDataModel<DeckChoiceDataModel>().ButtonClass = deck.Class.ToString();
		string vanillaHeroCardId = CollectionManager.GetHeroCardId(deck.Class, CardHero.HeroType.VANILLA);
		DeckTemplateDbfRecord deckRecord = GameDbf.DeckTemplate.GetRecord(deckReward.DeckTemplateId);
		selectButton.DeckTemplateRecord = deckRecord;
		selectButton.DeckDetailsController = m_deckDetailsController;
		selectButton.DataModel = m_deckInfoDataModel;
		selectButton.SetDeckSelectButtonIcon(vanillaHeroCardId, deck.Pouch.Name);
	}
}
