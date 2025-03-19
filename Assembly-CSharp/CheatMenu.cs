using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CustomEditClass]
public class CheatMenu : MonoBehaviour
{
	[CustomEditField(Sections = "TabGroups")]
	public List<GameObject> groups = new List<GameObject>();

	private int ActiveTabGroupIndex;

	[CustomEditField(Sections = "Arrows")]
	public GameObject LeftArrow;

	[CustomEditField(Sections = "Arrows")]
	public GameObject RightArrow;

	[CustomEditField(Sections = "Tabs")]
	public List<GameObject> tabs = new List<GameObject>();

	[CustomEditField(Sections = "Tabs")]
	public List<GameObject> contents = new List<GameObject>();

	private int ActiveTabIndex;

	private GameObject ActiveTabContents;

	[CustomEditField(Sections = "Tab_00_Contents")]
	public GameObject m_maxManaButton;

	[CustomEditField(Sections = "Tab_00_Contents")]
	public GameObject m_fullHealthButton;

	[CustomEditField(Sections = "Tab_00_Contents")]
	public GameObject m_SetHealthToOneButton;

	[CustomEditField(Sections = "Tab_00_Contents")]
	public GameObject m_ImmuneCheckMark;

	[CustomEditField(Sections = "Tab_00_Contents")]
	public GameObject m_ClearMinionsButton;

	[CustomEditField(Sections = "Tab_00_Contents")]
	public GameObject m_ClearHandButton;

	[CustomEditField(Sections = "Tab_00_Contents")]
	public GameObject m_destroyButton;

	[CustomEditField(Sections = "Tab_01_Contents")]
	public GameObject SearchTab;

	[CustomEditField(Sections = "Tab_01_Contents")]
	public GameObject PinnedTab;

	[CustomEditField(Sections = "Tab_01_Contents")]
	public GameObject SearchTabContents;

	[CustomEditField(Sections = "Tab_01_Contents")]
	public GameObject PinnedTabContents;

	[CustomEditField(Sections = "Tab_01_Contents")]
	public GameObject SearchInputField;

	[CustomEditField(Sections = "Tab_01_Contents")]
	public GameObject PinnedInputField;

	[CustomEditField(Sections = "Tab_01_Contents")]
	public GameObject exportCardButton;

	[CustomEditField(Sections = "Tab_01_Contents")]
	public GameObject m_GoldenCheckMark;

	[CustomEditField(Sections = "Tab_01_Contents")]
	public GameObject m_PinItCheckMark;

	[CustomEditField(Sections = "Tab_01_Contents")]
	public GameObject m_SearchResultItem;

	[CustomEditField(Sections = "Tab_01_Contents")]
	public GameObject m_PreviewCard;

	private TAG_PREMIUM m_premiumType;

	[CustomEditField(Sections = "Tab_02_Contents")]
	public GameObject m_runConsoleButton;

	[CustomEditField(Sections = "Tab_02_Contents")]
	public InputField m_scriptContent;

	private int tutorialProgress;

	private int DustInput;

	private int GoldInput;

	private int TicketsInput;

	[CustomEditField(Sections = "Tab_04_General")]
	public GameObject m_HUDcheckMark;

	private bool isHUDactive = true;

	[CustomEditField(Sections = "Tab_04_General")]
	public GameObject m_HideHistorycheckMark;

	private bool isHistoryactive = true;

	[CustomEditField(Sections = "Tab_04_General")]
	public GameObject m_SetboardInputField;

	private Dictionary<string, CardDbfRecord> m_allCardRecords;

	private string m_selectedCard;

	private GameObject m_cardPreview;

	private void Start()
	{
		if (ActiveTabGroupIndex > 0)
		{
			LeftArrow.SetActive(value: true);
		}
		else
		{
			LeftArrow.SetActive(value: false);
		}
		if (ActiveTabGroupIndex < groups.Count - 1)
		{
			RightArrow.SetActive(value: true);
		}
		else
		{
			RightArrow.SetActive(value: false);
		}
		ActiveTabContents = contents[ActiveTabIndex];
		for (int i = 0; i < contents.Count; i++)
		{
			if (i == ActiveTabIndex)
			{
				ColorBlock colorBlock = tabs[ActiveTabIndex].GetComponentInChildren<Button>().colors;
				colorBlock.normalColor = Color.white;
				tabs[ActiveTabIndex].GetComponentInChildren<Button>().colors = colorBlock;
				contents[ActiveTabIndex].SetActive(value: true);
			}
			else
			{
				ColorBlock colorBlock2 = tabs[ActiveTabIndex].GetComponentInChildren<Button>().colors;
				colorBlock2.normalColor = Color.clear;
				tabs[ActiveTabIndex].GetComponentInChildren<Button>().colors = colorBlock2;
				contents[i].SetActive(value: false);
			}
		}
		ActiveTabContents.SetActive(value: true);
	}

	private void OnEnable()
	{
		Debug.Log("Enabled");
		m_allCardRecords = new Dictionary<string, CardDbfRecord>();
		foreach (string id in GameUtils.GetAllCardIds())
		{
			m_allCardRecords[id] = GameUtils.GetCardRecord(id);
		}
	}

	public void SetAsActiveTab(int tabIndex)
	{
		Debug.Log("Tab Index " + tabIndex);
		ColorBlock colorBlock = tabs[ActiveTabIndex].GetComponentInChildren<Button>().colors;
		colorBlock.normalColor = Color.clear;
		tabs[ActiveTabIndex].GetComponentInChildren<Button>().colors = colorBlock;
		if (ActiveTabContents != null)
		{
			ActiveTabContents.SetActive(value: false);
		}
		ActiveTabIndex = tabIndex;
		ActiveTabContents = contents[ActiveTabIndex];
		ActiveTabContents.SetActive(value: true);
		colorBlock = tabs[ActiveTabIndex].GetComponentInChildren<Button>().colors;
		colorBlock.normalColor = Color.white;
		tabs[ActiveTabIndex].GetComponentInChildren<Button>().colors = colorBlock;
	}

	public void ShiftGroup(int indexChange)
	{
		groups[ActiveTabGroupIndex].SetActive(value: false);
		ActiveTabGroupIndex += indexChange;
		groups[ActiveTabGroupIndex].SetActive(value: true);
		if (ActiveTabGroupIndex > 0)
		{
			LeftArrow.SetActive(value: true);
		}
		else
		{
			LeftArrow.SetActive(value: false);
		}
		if (ActiveTabGroupIndex < groups.Count - 1)
		{
			RightArrow.SetActive(value: true);
		}
		else
		{
			RightArrow.SetActive(value: false);
		}
	}

	public void MaxMana()
	{
		if (Network.IsRunning())
		{
			string cmd = "maxmana friendly";
			Network.Get().SendDebugConsoleCommand(cmd);
		}
	}

	public void FullHealth()
	{
		if (Network.IsRunning())
		{
			string cmd = "healhero friendly";
			Network.Get().SendDebugConsoleCommand(cmd);
		}
	}

	public void SetHealthToOne()
	{
		if (Network.IsRunning())
		{
			string cmd = "spawncard XXX_107 friendly hand 0";
			Network.Get().SendDebugConsoleCommand(cmd);
		}
	}

	public void SetImmune()
	{
		Debug.Log("Cheat: SetImmune function called");
	}

	public void ClearMinions()
	{
		if (Network.IsRunning())
		{
			string cmd = "spawncard XXX_018 friendly hand 0";
			Network.Get().SendDebugConsoleCommand(cmd);
		}
	}

	public void Discard()
	{
		if (Network.IsRunning())
		{
			string cmd = "cyclehand friendly";
			Network.Get().SendDebugConsoleCommand(cmd);
		}
	}

	public void DrawCard()
	{
		if (Network.IsRunning())
		{
			string cmd = "drawcard friendly";
			Network.Get().SendDebugConsoleCommand(cmd);
		}
	}

	public void Destroy()
	{
		Debug.Log("Cheat: Destroy function called");
	}

	public void SearchOnValueChanged()
	{
		string keyword = SearchInputField.GetComponent<InputField>().text;
		Debug.Log("Search keyword changed to: " + keyword);
	}

	public void SearchOnEndEdit()
	{
		if (m_allCardRecords.Count == 0)
		{
			DefLoader.Get().Clear();
			Localization.SetLocale(Locale.enUS);
			GameDbf.Load();
			GameStrings.ReloadAll();
			foreach (string id in GameUtils.GetAllCardIds())
			{
				m_allCardRecords[id] = GameUtils.GetCardRecord(id);
			}
			DefLoader.Get().LoadAllEntityDefs();
		}
		string keyword = SearchInputField.GetComponent<InputField>().text.ToLower();
		Debug.Log("User pressed 'enter'. Keyword: " + keyword);
		Transform t = SearchTabContents.transform.Find("Search Results List").transform.Find("Search Result Items").transform;
		for (int i = 0; i < t.childCount; i++)
		{
			Object.Destroy(t.GetChild(i).gameObject);
		}
		if (string.IsNullOrEmpty(keyword) || m_allCardRecords.Count <= 0)
		{
			return;
		}
		Vector3 pos = new Vector3(0f, 0f, -73f);
		Vector3 scale = Vector3.one;
		foreach (KeyValuePair<string, CardDbfRecord> record in m_allCardRecords)
		{
			if ((record.Key + record.Value.Name.GetString(Locale.enUS).ToLower()).Contains(keyword))
			{
				GameObject item = Object.Instantiate(m_SearchResultItem);
				SearchResultItem result = item.GetComponent<SearchResultItem>();
				result.m_text = record.Value.Name.GetString(Locale.enUS);
				result.m_card = record.Key;
				item.name = "Item";
				item.transform.SetParent(t);
				item.transform.localPosition = pos;
				item.transform.localRotation = Quaternion.identity;
				item.transform.localScale = scale;
				item.GetComponent<Button>().onClick.AddListener(delegate
				{
					CardSelectedHandler(result);
				});
			}
		}
	}

	public void CardSelectedHandler(SearchResultItem item)
	{
		Debug.Log(item.m_text);
		m_selectedCard = item.m_card;
		PreviewCard();
	}

	private void PreviewCard()
	{
		if (m_cardPreview != null)
		{
			Object.Destroy(m_cardPreview);
		}
		m_cardPreview = LoadCard(m_selectedCard, m_premiumType);
	}

	private GameObject LoadCard(string cardID, TAG_PREMIUM premium)
	{
		using DefLoader.DisposableFullDef fullDef = DefLoader.Get().GetFullDef(cardID, new CardPortraitQuality(3, premium));
		string actorPath = ActorNames.GetHandActor(fullDef.EntityDef, premium);
		GameObject cardGO = AssetLoader.Get().InstantiatePrefab(actorPath, AssetLoadingOptions.IgnorePrefabPosition);
		Actor actor = cardGO.GetComponent<Actor>();
		if (actor == null)
		{
			Debug.LogWarning($"Error getting Actor for: {cardID}");
			return null;
		}
		m_PreviewCard.SetActive(value: false);
		actor.SetPremium(premium);
		actor.SetEntityDef(fullDef.EntityDef);
		actor.SetCardDef(fullDef.DisposableCardDef);
		actor.UpdateAllComponents();
		actor.SetUnlit();
		cardGO.transform.SetParent(contents[1].transform, worldPositionStays: false);
		cardGO.transform.localPosition = m_PreviewCard.transform.localPosition;
		cardGO.transform.localRotation = Quaternion.identity;
		cardGO.transform.localScale = m_PreviewCard.transform.localScale;
		Transform[] componentsInChildren = cardGO.GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = LayerMask.NameToLayer("UI");
		}
		cardGO.layer = LayerMask.NameToLayer("UI");
		return cardGO;
	}

	public void PinnedOnValueChanged()
	{
		string keyword = PinnedInputField.GetComponent<InputField>().text;
		Debug.Log("Pinned keyword changed to: " + keyword);
	}

	public void PinnedOnEndEdit()
	{
		string keyword = PinnedInputField.GetComponent<InputField>().text;
		Debug.Log("User pressed 'enter'. Keyword: " + keyword);
	}

	public void ShowSearchTab()
	{
		Debug.Log("Showing Search");
		SearchTabContents.SetActive(value: true);
		PinnedTabContents.SetActive(value: false);
		RectTransform component = SearchTab.GetComponent<RectTransform>();
		Vector3 searchPos = component.localPosition;
		component.localPosition = new Vector3(searchPos.x, searchPos.y, 0.109f);
		RectTransform component2 = PinnedTab.GetComponent<RectTransform>();
		Vector3 pinnedPos = component2.localPosition;
		component2.localPosition = new Vector3(pinnedPos.x, pinnedPos.y, 0.095f);
	}

	public void ShowPinnedTab()
	{
		Debug.Log("Showing Pinned Items");
		SearchTabContents.SetActive(value: false);
		PinnedTabContents.SetActive(value: true);
		RectTransform component = SearchTab.GetComponent<RectTransform>();
		Vector3 searchPos = component.localPosition;
		component.localPosition = new Vector3(searchPos.x, searchPos.y, 0.095f);
		RectTransform component2 = PinnedTab.GetComponent<RectTransform>();
		Vector3 pinnedPos = component2.localPosition;
		component2.localPosition = new Vector3(pinnedPos.x, pinnedPos.y, 0.109f);
	}

	public void PreviewCard(GameObject textObj)
	{
		string cardString = textObj.GetComponent<Text>().text;
		Debug.Log("Search Result Click. Previewing: " + cardString);
	}

	public void ToggleGolden()
	{
		Debug.Log("Cheat: ToggleGolden function called");
		m_GoldenCheckMark.SetActive(!m_GoldenCheckMark.activeSelf);
		if (m_premiumType == TAG_PREMIUM.GOLDEN || m_premiumType == TAG_PREMIUM.DIAMOND)
		{
			m_premiumType = TAG_PREMIUM.NORMAL;
		}
		else
		{
			m_premiumType = TAG_PREMIUM.GOLDEN;
		}
		PreviewCard();
	}

	public void ExportCard()
	{
		Debug.Log("Export Card function called");
	}

	public void AddCardTo(string location)
	{
		switch (location)
		{
		case "opponentHand":
			Debug.Log("AddCardTo function called. Adding card to Opponent's Hand");
			if (Network.IsRunning())
			{
				string cmd4 = $"spawncard {m_selectedCard} opponent hand 0";
				Network.Get().SendDebugConsoleCommand(cmd4);
			}
			break;
		case "opponentField":
			Debug.Log("AddCardTo function called. Adding card to Opponent's Field");
			if (Network.IsRunning())
			{
				string cmd6 = $"spawncard {m_selectedCard} opponent play 0";
				Network.Get().SendDebugConsoleCommand(cmd6);
			}
			break;
		case "opponentDeck":
			Debug.Log("AddCardTo function called. Adding card to Opponent's Deck");
			if (Network.IsRunning())
			{
				string cmd2 = $"spawncard {m_selectedCard} opponent deck 0";
				Network.Get().SendDebugConsoleCommand(cmd2);
			}
			break;
		case "yourField":
			Debug.Log("AddCardTo function called. Adding card to Your Field");
			if (Network.IsRunning())
			{
				string cmd5 = $"spawncard {m_selectedCard} friendly play 0";
				Network.Get().SendDebugConsoleCommand(cmd5);
			}
			break;
		case "yourHand":
			Debug.Log("AddCardTo function called. Adding card to Your Hand");
			if (Network.IsRunning())
			{
				string cmd3 = $"spawncard {m_selectedCard} friendly hand 0";
				Network.Get().SendDebugConsoleCommand(cmd3);
			}
			break;
		case "yourDeck":
			Debug.Log("AddCardTo function called. Adding card to Your Deck");
			if (Network.IsRunning())
			{
				string cmd = $"spawncard {m_selectedCard} friendly deck 0";
				Network.Get().SendDebugConsoleCommand(cmd);
			}
			break;
		}
	}

	public void RunConsole()
	{
		Debug.Log("Cheat: RunConsole function called");
	}

	public void ClearConsole()
	{
		m_scriptContent.text = "";
	}

	public void DustValueInput(InputField input)
	{
		DustInput = int.Parse(input.text);
		Debug.Log("Arcane Dust input field changed to: " + DustInput);
	}

	public void GoldValueInput(InputField input)
	{
		GoldInput = int.Parse(input.text);
		Debug.Log("Gold input field changed to: " + GoldInput);
	}

	public void TicketValueInput(InputField input)
	{
		TicketsInput = int.Parse(input.text);
		Debug.Log("Tickets input field changed to: " + TicketsInput);
	}

	public void TutorialDropdownValueChanged(int value)
	{
		tutorialProgress = value;
		Debug.Log("Tut: " + tutorialProgress);
	}

	public void SetTutorialProgress()
	{
		switch (tutorialProgress)
		{
		case 0:
			Debug.Log("Tutorial Progress set to: " + tutorialProgress + " : Rexxar");
			break;
		case 1:
			Debug.Log("Tutorial Progress set to: " + tutorialProgress + " : Garrosh");
			break;
		case 2:
			Debug.Log("Tutorial Progress set to: " + tutorialProgress + " : Lich King");
			break;
		case 3:
			Debug.Log("Tutorial Progress set to: " + tutorialProgress + " : Tutorial Complete");
			break;
		}
	}

	public void SetArcaneDust()
	{
		Debug.Log("Cheat: SetArcaneDust function called to add " + DustInput + " Arcane Dust to account");
	}

	public void SetGoldBalance()
	{
		Debug.Log("Cheat: SetGoldBalance function called to add " + GoldInput + " Gold to account");
	}

	public void OpenArena()
	{
		Debug.Log("Cheat: OpenArena function called");
	}

	public void SetTickets()
	{
		Debug.Log("Cheat: SetTickets function called to add " + TicketsInput + " Tickets to account");
	}

	public void BuyAllAdventures()
	{
		Debug.Log("Cheat: BuyAllAdventures function called");
	}

	public void DefeatAllAdventures()
	{
		Debug.Log("Cheat: DefeatAllAdventures function called");
	}

	public void MaxLevelAllHeroes()
	{
		Debug.Log("Cheat: MaxLevelAllHeroes function called");
	}

	public void CloneAccount()
	{
		Debug.Log("Cheat: CloneAccount function called");
	}

	public void ResetAccount()
	{
		Debug.Log("Cheat: ResetAccount function called");
	}

	public void GiveMeEverything()
	{
		Debug.Log("Cheat: GiveMeEverything function called");
	}

	public void ToggleHUD()
	{
		Debug.Log("Cheat: ToggleHUD function called");
		m_HUDcheckMark.SetActive(!m_HUDcheckMark.activeSelf);
		isHUDactive = !isHUDactive;
	}

	public void ToggleHideHistory()
	{
		Debug.Log("Cheat: ToggleHideHistory function called");
		m_HideHistorycheckMark.SetActive(!m_HideHistorycheckMark.activeSelf);
		isHistoryactive = !isHistoryactive;
	}

	public void RenameInnkeeper(Text name)
	{
		Debug.Log("Cheat: RenameInnkeeper function called. Renaming to: " + name.GetComponent<Text>().text);
	}

	public void ResetClient()
	{
		Debug.Log("Cheat: ResetClient function called");
	}

	public void ExportCardsTool()
	{
		Debug.Log("Cheat: ExportCardsTool function called");
	}

	public void BoardOnValueChanged()
	{
		string keyword = m_SetboardInputField.GetComponent<InputField>().text;
		Debug.Log("Pinned keyword changed to: " + keyword);
	}

	public void BoardOnEndEdit()
	{
		string keyword = m_SetboardInputField.GetComponent<InputField>().text;
		Debug.Log("User pressed 'enter'. Keyword: " + keyword);
	}
}
