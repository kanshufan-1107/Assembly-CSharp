using System.Collections;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Core;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

[CustomEditClass]
public class AdventureChooserTray : AccordionMenuTray
{
	private const string s_DefaultPortraitMaterialTextureName = "_MainTex";

	private const int s_DefaultPortraitMaterialIndex = 0;

	[SerializeField]
	[CustomEditField(Sections = "Sub Scene")]
	public AdventureSubScene m_ParentSubScene;

	[CustomEditField(Sections = "Choose Frame")]
	[SerializeField]
	public GameObject m_ComingSoonCoverUpSign;

	[SerializeField]
	[CustomEditField(Sections = "Choose Frame")]
	public UberText m_ComingSoonCoverUpSignHeaderText;

	[SerializeField]
	[CustomEditField(Sections = "Choose Frame")]
	public UberText m_ComingSoonCoverUpSignDescriptionText;

	public WidgetInstance m_replayableTutorialWidgetInstance;

	private AdventureChooserDescription m_CurrentChooserDescription;

	private Map<AdventureDbId, Map<AdventureModeDbId, AdventureChooserDescription>> m_Descriptions = new Map<AdventureDbId, Map<AdventureModeDbId, AdventureChooserDescription>>();

	private bool m_isTransitioning = true;

	private void Awake()
	{
		m_ChooseButton.Disable();
		m_BackButton.AddEventListener(UIEventType.RELEASE, delegate
		{
			OnBackButton();
		});
		m_ChooseButton.AddEventListener(UIEventType.RELEASE, delegate
		{
			ChangeSubScene();
		});
		AdventureConfig.Get().AddSelectedModeChangeListener(OnSelectedModeChange);
		AdventureProgressMgr.Get().RegisterProgressUpdatedListener(OnAdventureProgressUpdated);
		if (m_replayableTutorialWidgetInstance != null)
		{
			m_replayableTutorialWidgetInstance.RegisterDoneChangingStatesListener(OnReplayableTutorialDoneChangingStates);
		}
		Box.Get().AddTransitionFinishedListener(OnBoxTransitionFinished);
		StartCoroutine(InitTrayWhenReady());
	}

	private void Start()
	{
		Navigation.PushUnique(OnNavigateBack);
		m_isStarted = true;
	}

	private void OnReplayableTutorialDoneChangingStates(object payload)
	{
		if ((bool)UniversalInputManager.UsePhoneUI && m_replayableTutorialWidgetInstance != null)
		{
			m_replayableTutorialWidgetInstance.TriggerEvent("PHONE_ADV");
		}
	}

	protected IEnumerator InitTrayWhenReady()
	{
		if (m_ChooseFrameScroller == null || m_ChooseFrameScroller.ScrollObject == null)
		{
			Debug.LogError("m_ChooseFrameScroller or m_ChooseFrameScroller.m_ScrollObject cannot be null. Unable to create button.", this);
			yield break;
		}
		bool num = AdventureConfig.Get().PreviousSubScene != AdventureData.Adventuresubscene.INVALID;
		int latestWingId = 0;
		AdventureDbId latestUnseenAdventure = AdventureConfig.GetAdventurePlayerShouldSee(out latestWingId);
		if (!Options.Get().GetBool(Option.HAS_SEEN_PRACTICE_MODE, defaultVal: false))
		{
			Log.Adventures.Print("HAS_SEEN_PRACTICE_MODE set to true.");
			Options.Get().SetBool(Option.HAS_SEEN_PRACTICE_MODE, val: true);
		}
		if (!num && latestUnseenAdventure != 0)
		{
			AdventureConfig.Get().SetSelectedAdventureMode(latestUnseenAdventure, AdventureConfig.GetDefaultModeDbIdForAdventure(latestUnseenAdventure));
			if (latestWingId != 0)
			{
				GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.PLAYER_OPTIONS, GameSaveKeySubkeyId.LATEST_ADVENTURE_WING_SEEN, latestWingId));
			}
		}
		List<AdventureDef> advDefs = AdventureScene.Get().GetSortedAdventureDefs();
		Map<AdventureDbId, List<AdventureDef>> nestedAdventureDefs = new Map<AdventureDbId, List<AdventureDef>>();
		foreach (AdventureDef advDef in advDefs)
		{
			AdventureDbId parentAdventure = advDef.m_AdventureToNestUnder;
			if (parentAdventure != 0)
			{
				if (!nestedAdventureDefs.ContainsKey(parentAdventure))
				{
					nestedAdventureDefs.Add(parentAdventure, new List<AdventureDef>());
				}
				nestedAdventureDefs[parentAdventure].Add(advDef);
			}
		}
		List<Widget> buttonWidgets = new List<Widget>();
		foreach (AdventureDef advDef2 in advDefs)
		{
			if (AdventureConfig.ShouldDisplayAdventure(advDef2.GetAdventureId()) && !advDef2.IsNestedUnderAnotherAdventureOnChooserScreen)
			{
				List<AdventureDef> nestedAdvDefsForThisAdventure = null;
				if (nestedAdventureDefs.ContainsKey(advDef2.GetAdventureId()))
				{
					nestedAdvDefsForThisAdventure = nestedAdventureDefs[advDef2.GetAdventureId()];
				}
				Widget buttonWidget = CreateAdventureChooserButton(advDef2, nestedAdvDefsForThisAdventure);
				if (buttonWidget != null)
				{
					buttonWidgets.Add(buttonWidget);
				}
			}
		}
		while (!buttonWidgets.TrueForAll((Widget w) => w.IsReady && !w.IsChangingStates))
		{
			yield return null;
		}
		OnButtonVisualUpdated();
		if (m_SelectedSubButton != null && m_ChooseFrameScroller != null)
		{
			m_ChooseFrameScroller.UpdateScroll();
			m_ChooseFrameScroller.CenterObjectInView(m_SelectedSubButton.gameObject, 0f, null, iTween.EaseType.easeOutCubic, 0f);
		}
		if (m_ParentSubScene != null)
		{
			m_ParentSubScene.SetIsLoaded(loaded: true);
			m_ParentSubScene.AddSubSceneTransitionFinishedListener(OnSubSceneTransitionFinished);
		}
		AdventureDbId selectedAdventure = AdventureConfig.Get().GetSelectedAdventure();
		ShowComingSoonCoverUpSignIfActive(selectedAdventure);
	}

	private void OnDestroy()
	{
		if (AdventureConfig.Get() != null)
		{
			AdventureConfig.Get().RemoveSelectedModeChangeListener(OnSelectedModeChange);
		}
		if (Box.Get() != null)
		{
			Box.Get().RemoveTransitionFinishedListener(OnBoxTransitionFinished);
		}
		if (AdventureProgressMgr.Get() != null)
		{
			AdventureProgressMgr.Get().RemoveProgressUpdatedListener(OnAdventureProgressUpdated);
		}
		CancelInvoke("ShowDisabledAdventureModeRequirementsWarning");
	}

	private void OnBackButton()
	{
		Navigation.GoBack();
	}

	private static bool OnNavigateBack()
	{
		DisableTrayButtons();
		BackToGameModes();
		return true;
	}

	private static void BackToGameModes()
	{
		SceneMgr.Get().SetNextMode(SceneMgr.Mode.GAME_MODE, SceneMgr.TransitionHandlerType.NEXT_SCENE);
	}

	private static void BackToHub()
	{
		SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
	}

	private static void DisableTrayButtons()
	{
		AdventureChooserTray[] array = Object.FindObjectsOfType<AdventureChooserTray>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].DisableAllButtons();
		}
	}

	private Widget CreateAdventureChooserButton(AdventureDef advDef, List<AdventureDef> nestedAdvDefs)
	{
		string chooserButtonPrefab = m_DefaultChooserButtonPrefab;
		if (!string.IsNullOrEmpty(advDef.m_ChooserButtonPrefab))
		{
			chooserButtonPrefab = advDef.m_ChooserButtonPrefab;
		}
		Widget widget = WidgetInstance.Create(chooserButtonPrefab);
		widget.RegisterReadyListener(delegate
		{
			AdventureChooserButton newbutton = widget.transform.GetComponentInChildren<AdventureChooserButton>();
			if (!(newbutton == null))
			{
				GameUtils.SetParent(widget, m_ChooseFrameScroller.ScrollObject);
				AdventureDbId adventureId = advDef.GetAdventureId();
				newbutton.gameObject.name = $"{newbutton.gameObject.name}_{adventureId}";
				newbutton.SetAdventure(adventureId);
				newbutton.SetButtonText(advDef.GetAdventureName());
				newbutton.SetPortraitTexture(advDef.m_Texture);
				newbutton.SetPortraitTiling(advDef.m_TextureTiling);
				newbutton.SetPortraitOffset(advDef.m_TextureOffset);
				AdventureDbId selectedAdventure = AdventureConfig.Get().GetSelectedAdventure();
				AdventureDef adventureDef = AdventureScene.Get().GetAdventureDef(selectedAdventure);
				if (selectedAdventure == adventureId || (adventureDef != null && adventureDef.m_AdventureToNestUnder == adventureId))
				{
					newbutton.Toggle = true;
				}
				if (AdventureConfig.IsAdventureComingSoon(adventureId) && AreAllAdventuresComingSoon(nestedAdvDefs))
				{
					CreateAdventureChooserComingSoonSubButton(advDef, newbutton);
				}
				else
				{
					CreateAdventureChooserModeSubButtons(advDef, newbutton);
					if (nestedAdvDefs != null)
					{
						nestedAdvDefs.Sort((AdventureDef l, AdventureDef r) => l.GetSortOrder() - r.GetSortOrder());
						foreach (AdventureDef current in nestedAdvDefs)
						{
							CreateAdventureChooserModeSubButtons(current, newbutton);
						}
					}
				}
				newbutton.AddVisualUpdatedListener(base.OnButtonVisualUpdated);
				int index = m_ChooserButtons.Count;
				newbutton.AddToggleListener(delegate(bool toggle)
				{
					OnChooserButtonToggled(newbutton, toggle, index);
				});
				newbutton.AddModeSelectionListener(ButtonModeSelected);
				newbutton.AddExpandedListener(ButtonExpanded);
				m_ChooserButtons.Add(newbutton);
				newbutton.FireVisualUpdatedEvent();
			}
		});
		return widget;
	}

	private bool AreAllAdventuresComingSoon(List<AdventureDef> advDefs, bool emptyListDefault = true)
	{
		if (advDefs == null || advDefs.Count == 0)
		{
			return emptyListDefault;
		}
		foreach (AdventureDef advDef in advDefs)
		{
			if (!AdventureConfig.IsAdventureComingSoon(advDef.GetAdventureId()))
			{
				return false;
			}
		}
		return true;
	}

	private void CreateAdventureChooserModeSubButtons(AdventureDef advDef, AdventureChooserButton newbutton)
	{
		List<AdventureSubDef> sortedSubDefs = advDef.GetSortedSubDefs();
		AdventureDbId adventureDbId = advDef.GetAdventureId();
		AdventureDbId selectedAdventure = AdventureConfig.Get().GetSelectedAdventure();
		AdventureModeDbId selectedMode = AdventureConfig.Get().GetClientChooserAdventureMode(adventureDbId);
		string chooseSubButtonPrefab = m_DefaultChooserSubButtonPrefab;
		if (!string.IsNullOrEmpty(advDef.m_ChooserSubButtonPrefab))
		{
			chooseSubButtonPrefab = advDef.m_ChooserSubButtonPrefab;
		}
		bool canBeLastSelected = true;
		if (advDef.IsNestedUnderAnotherAdventureOnChooserScreen)
		{
			canBeLastSelected = !AdventureConfig.IsAdventureComingSoon(adventureDbId) && AdventureProgressMgr.Get().IsAdventureComplete(advDef.m_AdventureToNestUnder);
		}
		foreach (AdventureSubDef subDef in sortedSubDefs)
		{
			AdventureModeDbId mode = subDef.GetAdventureModeId();
			AdventureChooserSubButton newSubButton = newbutton.CreateSubButton(adventureDbId, mode, subDef, chooseSubButtonPrefab, canBeLastSelected && selectedMode == mode);
			if (!(newSubButton == null))
			{
				bool selected = newbutton.Toggle && selectedAdventure == advDef.GetAdventureId() && selectedMode == mode;
				if (selected)
				{
					newSubButton.SetHighlight(enable: true);
					UpdateChooseButton(adventureDbId, mode);
					SetTitleText(adventureDbId, mode);
					m_SelectedSubButton = newSubButton;
				}
				else if (AdventureConfig.IsFeaturedMode(adventureDbId, mode))
				{
					newSubButton.SetNewGlow(enable: true);
				}
				bool canPlay = AdventureConfig.CanPlayMode(adventureDbId, mode);
				newSubButton.SetDesaturate(!canPlay);
				if (selectedAdventure == AdventureDbId.PRACTICE && mode == AdventureModeDbId.EXPERT && !canPlay)
				{
					newSubButton.SetContrast(0.3f);
				}
				CreateAdventureChooserDescriptionFromPrefab(adventureDbId, subDef, selected);
			}
		}
	}

	private void CreateAdventureChooserComingSoonSubButton(AdventureDef advDef, AdventureChooserButton newbutton)
	{
		AdventureDbId adventureDbId = advDef.GetAdventureId();
		AdventureModeDbId selectedMode = AdventureConfig.Get().GetClientChooserAdventureMode(adventureDbId);
		List<AdventureSubDef> subDefs = advDef.GetSortedSubDefs();
		AdventureSubDef subDef = new AdventureSubDef();
		AdventureModeDbId modeId = AdventureModeDbId.LINEAR;
		if (subDefs.Count > 0)
		{
			subDef = subDefs[0];
			modeId = subDefs[0].GetAdventureModeId();
		}
		ChooserSubButton newSubButton = newbutton.CreateComingSoonSubButton(modeId, m_DefaultChooserComingSoonSubButtonPrefab);
		if (newSubButton == null)
		{
			Debug.LogError("newSubButton cannot be null. Unable to create newSubButton.", this);
			return;
		}
		if (newbutton.Toggle && selectedMode == modeId)
		{
			newSubButton.SetHighlight(enable: true);
			UpdateChooseButton(adventureDbId, modeId);
			SetTitleText(adventureDbId, modeId);
			m_SelectedSubButton = newSubButton;
		}
		CreateAdventureChooserDescriptionFromPrefab(adventureDbId, subDef, newbutton.Toggle);
	}

	private void CreateAdventureChooserDescriptionFromPrefab(AdventureDbId adventureId, AdventureSubDef subDef, bool active)
	{
		if (string.IsNullOrEmpty(subDef.m_ChooserDescriptionPrefab))
		{
			return;
		}
		if (!m_Descriptions.TryGetValue(adventureId, out var chooserDescs))
		{
			chooserDescs = new Map<AdventureModeDbId, AdventureChooserDescription>();
			m_Descriptions[adventureId] = chooserDescs;
		}
		string dataDescText = subDef.GetDescription();
		string dataReqDescText = null;
		if (!AdventureConfig.CanPlayMode(adventureId, subDef.GetAdventureModeId(), checkEventTimings: false))
		{
			dataReqDescText = subDef.GetRequirementsDescription();
			if (!string.IsNullOrEmpty(subDef.GetLockedDescription()))
			{
				dataDescText = subDef.GetLockedDescription();
			}
		}
		AdventureChooserDescription newChooserDesc = GameUtils.LoadGameObjectWithComponent<AdventureChooserDescription>(subDef.m_ChooserDescriptionPrefab);
		if (newChooserDesc == null)
		{
			return;
		}
		GameUtils.SetParent(newChooserDesc, m_DescriptionContainer);
		newChooserDesc.SetText(dataReqDescText, dataDescText);
		newChooserDesc.m_WidgetElement.RegisterReadyListener(delegate(Widget w)
		{
			if (w != null)
			{
				AdventureChooserDescriptionDataModel dataModel = new AdventureChooserDescriptionDataModel
				{
					Heroes = AdventureUtils.GetAvailableGuestHeroesAsCardListSortedByReleaseDate(adventureId)
				};
				StartCoroutine(UpdateDataModelWhenGameSaveDataIsReady(dataModel, adventureId, subDef.GetAdventureModeId(), active));
				w.BindDataModel(dataModel);
			}
		});
		newChooserDesc.gameObject.SetActive(active);
		chooserDescs[subDef.GetAdventureModeId()] = newChooserDesc;
		if (active)
		{
			m_CurrentChooserDescription = newChooserDesc;
		}
	}

	private IEnumerator UpdateDataModelWhenGameSaveDataIsReady(AdventureChooserDescriptionDataModel dataModel, AdventureDbId adventureId, AdventureModeDbId modeId, bool active)
	{
		dataModel.HasNewHero = false;
		if (!AdventureUtils.DoesAdventureShowNewlyUnlockedGuestHeroTreatment(adventureId))
		{
			yield break;
		}
		AdventureDataDbfRecord adventureData = AdventureConfig.GetAdventureDataRecord(adventureId, modeId);
		GameSaveKeyId adventureClientKey = (GameSaveKeyId)adventureData.GameSaveDataClientKey;
		if (!GameSaveDataManager.IsGameSaveKeyValid(adventureClientKey))
		{
			yield break;
		}
		if (active && !GameSaveDataManager.Get().IsDataReady(adventureClientKey))
		{
			GameSaveDataManager.Get().Request(adventureClientKey, delegate(bool success)
			{
				if (success)
				{
					dataModel.HasNewHero = AdventureUtils.DoesAdventureHaveUnseenGuestHeroes(adventureId, modeId);
				}
				else
				{
					Log.Adventures.PrintWarning("Unable to set AdventureChooserDescriptionDataModel.HasNewHero - GameSaveData request failed!");
				}
			});
			yield break;
		}
		while (!GameSaveDataManager.Get().IsDataReady(adventureClientKey))
		{
			Log.Adventures.Print("Waiting for client key {0} before updating DataModel for that Adventure Chooser Description!", adventureClientKey);
			yield return null;
		}
		dataModel.HasNewHero = AdventureUtils.DoesAdventureHaveUnseenGuestHeroes(adventureId, modeId);
	}

	private AdventureChooserDescription GetAdventureChooserDescription(AdventureDbId adventureId, AdventureModeDbId modeId)
	{
		if (!m_Descriptions.TryGetValue(adventureId, out var descs))
		{
			return null;
		}
		if (!descs.TryGetValue(modeId, out var desc))
		{
			return null;
		}
		return desc;
	}

	private void DisableAllAdventureChoosers()
	{
		foreach (Map<AdventureModeDbId, AdventureChooserDescription> entry in m_Descriptions.Values)
		{
			if (entry == null)
			{
				continue;
			}
			foreach (AdventureChooserDescription desc in entry.Values)
			{
				if (desc != null)
				{
					desc.gameObject.SetActive(value: false);
				}
			}
		}
	}

	private void ButtonModeSelected(ChooserSubButton btn)
	{
		foreach (ChooserButton chooserButton in m_ChooserButtons)
		{
			chooserButton.DisableSubButtonHighlights();
		}
		AdventureChooserSubButton adventureSubButton = (AdventureChooserSubButton)(m_SelectedSubButton = (AdventureChooserSubButton)btn);
		if (AdventureConfig.MarkFeaturedMode(adventureSubButton.GetAdventure(), adventureSubButton.GetMode()))
		{
			btn.SetNewGlow(enable: false);
		}
		AdventureConfig.Get().SetSelectedAdventureMode(adventureSubButton.GetAdventure(), adventureSubButton.GetMode());
		SetTitleText(adventureSubButton.GetAdventure(), adventureSubButton.GetMode());
	}

	protected void ButtonExpanded(ChooserButton button, bool expand)
	{
		if (!expand)
		{
			return;
		}
		ToggleScrollable(enable: true);
		AdventureChooserButton adventureButton = (AdventureChooserButton)button;
		ChooserSubButton[] subButtons = adventureButton.GetSubButtons();
		foreach (ChooserSubButton subBtn in subButtons)
		{
			AdventureChooserSubButton adventureSubButton = (AdventureChooserSubButton)subBtn;
			if (AdventureConfig.IsFeaturedMode(adventureButton.GetAdventure(), adventureSubButton.GetMode()))
			{
				subBtn.Flash();
			}
			if (AdventureConfig.ShouldShowNewModePopup(adventureButton.GetAdventure(), adventureSubButton.GetMode()))
			{
				StartCoroutine(ShowNewModePopupOnSubButtonAfterScrollingFinished(adventureSubButton));
			}
		}
	}

	private IEnumerator ShowNewModePopupOnSubButtonAfterScrollingFinished(AdventureChooserSubButton subButton)
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		while (m_isTransitioning)
		{
			yield return new WaitForEndOfFrame();
		}
		subButton.ShowNewModePopup(GameStrings.Get("GLUE_ADVENTURE_NEW_MODE_UNLOCKED_POPUP_TEXT"));
		subButton.HideNewModePopupAfterDelay();
		AdventureConfig.MarkHasSeenNewModePopup(subButton.GetAdventure(), subButton.GetMode());
	}

	private void SetTitleText(AdventureDbId adventureId, AdventureModeDbId modeId)
	{
		AdventureDataDbfRecord dataRecord = GameUtils.GetAdventureDataRecord((int)adventureId, (int)modeId);
		m_DescriptionTitleObject.Text = dataRecord.Name;
	}

	private void OnSelectedModeChange(AdventureDbId adventureId, AdventureModeDbId modeId)
	{
		AdventureChooserDescription desc = GetAdventureChooserDescription(adventureId, modeId);
		if (m_CurrentChooserDescription != desc)
		{
			DisableAllAdventureChoosers();
			m_CurrentChooserDescription = desc;
			if (m_CurrentChooserDescription != null)
			{
				m_CurrentChooserDescription.gameObject.SetActive(value: true);
			}
		}
		UpdateChooseButton(adventureId, modeId);
		if (m_ChooseButton.IsEnabled())
		{
			PlayMakerFSM fsm = m_ChooseButton.GetComponent<PlayMakerFSM>();
			if (fsm != null)
			{
				fsm.SendEvent("Burst");
			}
		}
		ShowComingSoonCoverUpSignIfActive(adventureId);
		if (!AdventureConfig.CanPlayMode(adventureId, modeId, checkEventTimings: false))
		{
			if (!m_isStarted)
			{
				Invoke("ShowDisabledAdventureModeRequirementsWarning", 0f);
			}
			else
			{
				ShowDisabledAdventureModeRequirementsWarning();
			}
		}
	}

	private void ShowComingSoonCoverUpSignIfActive(AdventureDbId adventureId)
	{
		if (AdventureConfig.IsAdventureComingSoon(adventureId))
		{
			m_ComingSoonCoverUpSign.SetActive(value: true);
			SetComingSoonCoverUpSignText(adventureId);
		}
		else
		{
			m_ComingSoonCoverUpSign.SetActive(value: false);
		}
	}

	private void SetComingSoonCoverUpSignText(AdventureDbId adventureId)
	{
		AdventureDbfRecord adventureRecord = GameDbf.Adventure.GetRecord((int)adventureId);
		m_ComingSoonCoverUpSignHeaderText.Text = adventureRecord.ComingSoonText;
		m_ComingSoonCoverUpSignDescriptionText.Text = TimeUtils.GetComingSoonText(adventureRecord.ComingSoonEvent);
	}

	private void ShowDisabledAdventureModeRequirementsWarning()
	{
		CancelInvoke("ShowDisabledAdventureModeRequirementsWarning");
		if (!m_isStarted || SceneMgr.Get().GetMode() != SceneMgr.Mode.ADVENTURE || !(m_ChooseButton != null) || m_ChooseButton.IsEnabled())
		{
			return;
		}
		AdventureDbId adventureId = AdventureConfig.Get().GetSelectedAdventure();
		AdventureModeDbId modeId = AdventureConfig.Get().GetSelectedMode();
		if (!AdventureConfig.CanPlayMode(adventureId, modeId, checkEventTimings: false))
		{
			int modeDbId = (int)modeId;
			string dataReqDescText = GameUtils.GetAdventureDataRecord((int)adventureId, modeDbId).RequirementsDescription;
			if (!string.IsNullOrEmpty(dataReqDescText))
			{
				Error.AddWarning(GameStrings.Get("GLUE_ADVENTURE_LOCKED"), dataReqDescText);
			}
		}
	}

	private void UpdateChooseButton(AdventureDbId adventureId, AdventureModeDbId modeId)
	{
		if (!m_AttemptedLoad && AdventureConfig.CanPlayMode(adventureId, modeId) && AdventureConfig.IsAdventureEventActive(adventureId))
		{
			m_ChooseButton.SetText(GameStrings.Get("GLOBAL_ADVENTURE_CHOOSE_BUTTON_TEXT"));
			if (!m_ChooseButton.IsEnabled())
			{
				m_ChooseButton.Enable();
			}
		}
		else
		{
			m_ChooseButton.SetText(GameStrings.Get("GLUE_QUEST_LOG_CLASS_LOCKED"));
			m_ChooseButton.Disable();
		}
	}

	private void OnBoxTransitionFinished(object userData)
	{
		if (!m_isStarted || SceneMgr.Get().GetMode() != SceneMgr.Mode.ADVENTURE)
		{
			return;
		}
		if (m_ChooseButton.IsEnabled())
		{
			PlayMakerFSM fsm = m_ChooseButton.GetComponent<PlayMakerFSM>();
			if (fsm != null)
			{
				fsm.SendEvent("Burst");
			}
		}
		else
		{
			ShowDisabledAdventureModeRequirementsWarning();
		}
		m_isTransitioning = false;
	}

	private void OnSubSceneTransitionFinished()
	{
		if (AdventureConfig.Get().CurrentSubScene == AdventureData.Adventuresubscene.CHOOSER && !SceneMgr.Get().IsTransitioning())
		{
			m_isTransitioning = false;
		}
	}

	private void ChangeSubScene()
	{
		m_AttemptedLoad = true;
		m_ChooseButton.SetText(GameStrings.Get("GLUE_LOADING"));
		DisableAllButtons();
		StartCoroutine(WaitThenChangeSubScene());
	}

	private void DisableAllButtons()
	{
		m_ChooseButton.Disable();
		m_BackButton.Flip(faceUp: false);
		m_BackButton.SetEnabled(enabled: false);
		foreach (ChooserButton chooserButton in m_ChooserButtons)
		{
			chooserButton.SetEnabled(enabled: false);
		}
	}

	private IEnumerator WaitThenChangeSubScene()
	{
		yield return null;
		AdventureConfig.Get().ChangeSubSceneToSelectedAdventure();
	}

	private void OnAdventureProgressUpdated(bool isStartupAction, AdventureMission.WingProgress oldProgress, AdventureMission.WingProgress newProgress, object userData)
	{
		if (newProgress == null || (oldProgress != null && oldProgress.IsOwned()) || !newProgress.IsOwned() || GameDbf.Wing.GetRecord(newProgress.Wing) == null)
		{
			return;
		}
		foreach (ChooserButton chooserButton in m_ChooserButtons)
		{
			ChooserSubButton[] subButtons = chooserButton.GetSubButtons();
			for (int i = 0; i < subButtons.Length; i++)
			{
				AdventureChooserSubButton adventureSubButton = subButtons[i] as AdventureChooserSubButton;
				if (adventureSubButton == null)
				{
					Debug.LogErrorFormat("AdventureChooserTray: Button is either null or not of type AdventureChooserSubButton: {0}", adventureSubButton);
				}
				else
				{
					adventureSubButton.ShowRemainingProgressCount();
					bool canPlay = AdventureConfig.CanPlayMode(adventureSubButton.GetAdventure(), adventureSubButton.GetMode());
					adventureSubButton.SetDesaturate(!canPlay);
				}
			}
		}
	}
}
