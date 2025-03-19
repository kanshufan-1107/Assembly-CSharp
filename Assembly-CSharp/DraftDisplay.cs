using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Configuration;
using Hearthstone;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

[CustomEditClass]
public class DraftDisplay : MonoBehaviour
{
	public enum DraftMode
	{
		INVALID,
		NO_ACTIVE_DRAFT,
		DRAFTING,
		ACTIVE_DRAFT_DECK,
		IN_REWARDS
	}

	private class ChoiceCallback
	{
		public DefLoader.DisposableFullDef fullDef;

		public int choiceID;

		public int slot;

		public TAG_PREMIUM premium;

		public ChoiceCallback Copy()
		{
			return new ChoiceCallback
			{
				fullDef = fullDef?.Share(),
				choiceID = choiceID,
				slot = slot,
				premium = premium
			};
		}
	}

	private class DraftChoice
	{
		public string m_cardID = string.Empty;

		public TAG_PREMIUM m_premium;

		public Actor m_actor;

		public Actor m_subActor;
	}

	public Collider m_pickArea;

	public UberText m_instructionText;

	public UberText m_instructionDetailText;

	public UberText m_forgeLabel;

	public DraftManaCurve m_manaCurve;

	public GameObject m_heroLabel;

	public Spell m_DeckCompleteSpell;

	public float m_DeckCardBarFlareUpDelay;

	public Spell m_heroPowerChosenFadeOut;

	public Spell m_heroPowerChosenFadeIn;

	public PegUIElement m_heroClickCatcher;

	public DraftPhoneDeckTray m_draftDeckTray;

	[CustomEditField(Sections = "Buttons")]
	public UIBButton m_backButton;

	public StandardPegButtonNew m_retireButton;

	public PlayButton m_playButton;

	[CustomEditField(Sections = "Bones")]
	public Transform m_bigHeroBone;

	public Transform m_socketHeroBone;

	public List<Transform> m_heroPowerBones = new List<Transform>();

	public Transform m_socketHeroPowerBone;

	[CustomEditField(Sections = "Phone")]
	public GameObject m_PhonePlayButtonTray;

	public Transform m_PhoneBackButtonBone;

	public Transform m_PhoneDeckTrayHiddenBone;

	public GameObject m_Phone3WayButtonRoot;

	public GameObject m_PhoneChooseHero;

	public GameObject m_PhoneLargeViewDeckButton;

	public ArenaPhoneControl m_PhoneDeckControl;

	private const string ALERTPOPUPID_FIRSTTIME = "arena_first_time";

	private static readonly Vector3 CHOICE_ACTOR_LOCAL_SCALE = new Vector3(7.2f, 7.2f, 7.2f);

	private static readonly Vector3 HERO_ACTOR_LOCAL_SCALE = new Vector3(8.285825f, 8.285825f, 8.285825f);

	private static readonly Vector3 HERO_LABEL_SCALE = new Vector3(8f, 8f, 8f);

	private static readonly Vector3 HERO_POWER_START_POSITION = new Vector3(0f, 0f, -0.3410472f);

	private static readonly Vector3 HERO_POWER_POSITION = new Vector3((float)Math.PI * 113f / 252f, 0f, -0.3410472f);

	private static readonly Vector3 HERO_POWER_SCALE = new Vector3(0.3419997f, 0.3419997f, 0.3419997f);

	private static readonly Vector3 DRAFTING_HERO_POWER_POSITION = new Vector3(0.9f, 0.215f, -0.164f);

	private static readonly Vector3 DRAFTING_HERO_POWER_BIG_CARD_SCALE = new Vector3(0.5f, 0.5f, 0.5f);

	private static readonly Vector3 DRAFTING_HERO_POWER_SCALE = new Vector3(5f, 5f, 5f);

	private static readonly Vector3 HERO_POWER_TOOLTIP_POSITION = new Vector3(-16.3f, 0.3f, -12.5f);

	private static readonly Vector3 HERO_POWER_TOOLTIP_SCALE = new Vector3(7f, 7f, 7f);

	private static readonly Vector3 CHOICE_ACTOR_LOCAL_SCALE_PHONE = new Vector3(14.5f, 14.5f, 14.5f) / DraftScene.DRAFT_SCENE_LOCAL_SCALE_INDEX_PHONE;

	private static readonly Vector3 HERO_ACTOR_LOCAL_SCALE_PHONE = new Vector3(15.5f, 15.5f, 15.5f) / DraftScene.DRAFT_SCENE_LOCAL_SCALE_INDEX_PHONE;

	private static readonly Vector3 HERO_LABEL_SCALE_PHONE = new Vector3(15f, 15f, 15f);

	private static readonly Vector3 HERO_POWER_START_POSITION_PHONE = new Vector3(1.6f, 0.3f, -0.15f);

	private static readonly Vector3 HERO_POWER_POSITION_PHONE = new Vector3(1.07f, 0.3f, -0.15f);

	private static readonly Vector3 HERO_POWER_SCALE_PHONE = new Vector3(0.5f, 0.5f, 0.5f) / DraftScene.DRAFT_SCENE_LOCAL_SCALE_INDEX_PHONE;

	private static readonly Vector3 DRAFTING_HERO_POWER_SCALE_PHONE = new Vector3(8f, 8f, 8f) / DraftScene.DRAFT_SCENE_LOCAL_SCALE_INDEX_PHONE;

	private static readonly Vector3 HERO_POWER_TOOLTIP_POSITION_PHONE = new Vector3(-6.7f, 5f, -5f);

	private static readonly Vector3 HERO_POWER_TOOLTIP_SCALE_PHONE = new Vector3(15f, 15f, 15f) / DraftScene.DRAFT_SCENE_LOCAL_SCALE_INDEX_PHONE;

	private static DraftDisplay s_instance;

	private DraftManager m_draftManager;

	private List<DraftChoice> m_choices = new List<DraftChoice>();

	private Actor[] m_heroPowerCardActors = new Actor[3];

	private Actor[] m_mythicPowerCardActors = new Actor[3];

	private CornerReplacementSpellType[] m_mythicHeroTypes = new CornerReplacementSpellType[3];

	private DefLoader.DisposableFullDef[] m_heroPowerDefs = new DefLoader.DisposableFullDef[3];

	private DefLoader.DisposableFullDef[] m_subClassHeroPowerDefs = new DefLoader.DisposableFullDef[3];

	private DraftMode m_currentMode;

	private NormalButton m_confirmButton;

	private Actor m_heroPower;

	private Actor m_defaultHeroPowerSkin;

	private Actor m_goldenHeroPowerSkin;

	private bool m_netCacheReady;

	private Actor m_chosenHero;

	private Actor m_inPlayHeroPowerActor;

	private bool m_animationsComplete = true;

	private List<HeroLabel> m_currentLabels = new List<HeroLabel>();

	private CardSoundSpell[] m_heroEmotes = new CardSoundSpell[3];

	private bool m_skipHeroEmotes;

	private bool m_isHeroAnimating;

	private DraftCardVisual m_zoomedHero;

	private bool m_wasDrafting;

	private bool m_firstTimeIntroComplete;

	private DialogBase m_firstTimeDialog;

	private bool m_fxActive;

	private bool m_inPositionAndShowChoices;

	private List<Actor> m_subclassHeroClones = new List<Actor>();

	private Actor[] m_subclassHeroPowerActors = new Actor[3];

	private ScreenEffectsHandle m_screenEffectsHandle;

	public Actor ChosenHero => m_chosenHero;

	private void Awake()
	{
		s_instance = this;
		m_draftManager = DraftManager.Get();
		AssetLoader.Get().InstantiatePrefab("DraftHeroChooseButton.prefab:7640de5f1d8e50e4caf8dccc55f28c6a", OnConfirmButtonLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
		AssetLoader.Get().InstantiatePrefab("History_HeroPower.prefab:e73edf8ccea2b11429093f7a448eef53", LoadHeroPowerCallback, null, AssetLoadingOptions.IgnorePrefabPosition);
		AssetLoader.Get().InstantiatePrefab(ActorNames.GetNameWithPremiumType(ActorNames.ACTOR_ASSET.HISTORY_HERO_POWER, TAG_PREMIUM.GOLDEN), LoadGoldenHeroPowerCallback, null, AssetLoadingOptions.IgnorePrefabPosition);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			AssetLoader.Get().InstantiatePrefab("BackButton_phone.prefab:08de22f2aa1facd42812215422eba8c7", OnPhoneBackButtonLoaded);
		}
		m_draftManager.RegisterDisplayHandlers();
		SceneMgr.Get().RegisterScenePreUnloadEvent(OnScenePreUnload);
		string sceneHeadline = m_draftManager.GetSceneHeadlineText();
		if (string.IsNullOrEmpty(sceneHeadline))
		{
			sceneHeadline = GameStrings.Get("GLUE_TOOLTIP_BUTTON_FORGE_HEADLINE");
		}
		m_forgeLabel.Text = sceneHeadline;
		m_instructionText.Text = string.Empty;
		m_pickArea.enabled = false;
		if (DemoMgr.Get().ArenaIs1WinMode())
		{
			Options.Get().SetBool(Option.HAS_SEEN_FORGE_PLAY_MODE, val: false);
			Options.Get().SetBool(Option.HAS_SEEN_FORGE_CARD_CHOICE, val: false);
			Options.Get().SetBool(Option.HAS_SEEN_FORGE, val: true);
			Options.Get().SetBool(Option.HAS_SEEN_FORGE_HERO_CHOICE, val: true);
			Options.Get().SetBool(Option.HAS_SEEN_FORGE_CARD_CHOICE2, val: false);
		}
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	private void OnDestroy()
	{
		DefLoader.DisposableFullDef[] heroPowerDefs = m_heroPowerDefs;
		for (int i = 0; i < heroPowerDefs.Length; i++)
		{
			heroPowerDefs[i]?.Dispose();
		}
		heroPowerDefs = m_subClassHeroPowerDefs;
		for (int i = 0; i < heroPowerDefs.Length; i++)
		{
			heroPowerDefs[i]?.Dispose();
		}
		FadeEffectsOut();
		s_instance = null;
	}

	private void Start()
	{
		Navigation.Push(OnNavigateBack);
		NetCache.Get().RegisterScreenForge(OnNetCacheReady);
		SetupRetireButton();
		m_playButton.AddEventListener(UIEventType.RELEASE, PlayButtonPress);
		m_manaCurve.GetComponent<PegUIElement>().AddEventListener(UIEventType.ROLLOVER, ManaCurveOver);
		m_manaCurve.GetComponent<PegUIElement>().AddEventListener(UIEventType.ROLLOUT, ManaCurveOut);
		m_playButton.SetText(GameStrings.Get("GLOBAL_PLAY"));
		ShowPhonePlayButton(show: false);
		if (!UniversalInputManager.UsePhoneUI)
		{
			SetupBackButton();
		}
		Network.Get().RequestDraftChoicesAndContents();
		MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_Arena);
		StartCoroutine(NotifySceneLoadedWhenReady());
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_draftDeckTray.gameObject.SetActive(value: true);
		}
	}

	private void Update()
	{
		Network.Get().ProcessNetwork();
	}

	public static DraftDisplay Get()
	{
		return s_instance;
	}

	public void OnOpenRewardsComplete()
	{
		ExitDraftScene();
	}

	public void OnApplicationPause(bool pauseStatus)
	{
		if (GameMgr.Get().IsFindingGame())
		{
			CancelFindGame();
		}
	}

	public void Unload()
	{
		Box.Get().SetToIgnoreFullScreenEffects(ignoreEffects: false);
		if (m_confirmButton != null)
		{
			UnityEngine.Object.Destroy(m_confirmButton.gameObject);
		}
		if (m_heroPower != null)
		{
			m_heroPower.Destroy();
		}
		if (m_chosenHero != null)
		{
			m_chosenHero.Destroy();
		}
		foreach (Actor actor in m_subclassHeroClones)
		{
			if (actor != null)
			{
				actor.Destroy();
			}
		}
		m_subclassHeroClones.Clear();
		Actor[] subclassHeroPowerActors = m_subclassHeroPowerActors;
		foreach (Actor actor2 in subclassHeroPowerActors)
		{
			if (actor2 != null)
			{
				actor2.Destroy();
			}
		}
		m_currentLabels.Clear();
		m_draftManager.UnregisterDisplayHandlers();
		m_draftManager = null;
		DraftInputManager.Get().Unload();
	}

	public void AcceptNewChoices(List<NetCache.CardDefinition> choices)
	{
		DestroyOldChoices();
		UpdateInstructionText();
		StartCoroutine(WaitForAnimsToFinishAndThenDisplayNewChoices(choices));
	}

	public void OnChoiceSelected(int chosenIndex)
	{
		DraftChoice draftChoice = m_choices[chosenIndex - 1];
		Actor choiceActor = draftChoice.m_actor;
		if (!choiceActor.GetEntityDef().IsHeroSkin() && !choiceActor.GetEntityDef().IsHeroPower())
		{
			AddCardToManaCurve(choiceActor.GetEntityDef());
			m_draftDeckTray.GetCardsContent().UpdateCardList(draftChoice.m_cardID, updateHighlight: true, choiceActor);
		}
	}

	private IEnumerator WaitForAnimsToFinishAndThenDisplayNewChoices(List<NetCache.CardDefinition> choices)
	{
		while (!m_animationsComplete)
		{
			yield return null;
		}
		while (m_isHeroAnimating)
		{
			yield return null;
		}
		m_choices.Clear();
		for (int i = 0; i < choices.Count; i++)
		{
			NetCache.CardDefinition cardDefinition = choices[i];
			DraftChoice newChoice = new DraftChoice
			{
				m_cardID = cardDefinition.Name,
				m_premium = cardDefinition.Premium,
				m_actor = null
			};
			m_choices.Add(newChoice);
		}
		if (m_draftManager.GetSlotType() != DraftSlotType.DRAFT_SLOT_HERO)
		{
			while (m_chosenHero == null)
			{
				yield return null;
			}
		}
		m_skipHeroEmotes = false;
		for (int j = 0; j < m_choices.Count; j++)
		{
			DraftChoice choice = m_choices[j];
			if (choice.m_cardID != null)
			{
				ChoiceCallback callbackInfo = new ChoiceCallback();
				callbackInfo.choiceID = j + 1;
				callbackInfo.slot = m_draftManager.GetSlot();
				callbackInfo.premium = choice.m_premium;
				DefLoader.Get().LoadFullDef(choice.m_cardID, OnFullDefLoaded, callbackInfo);
			}
		}
	}

	public void SetDraftMode(DraftMode mode)
	{
		bool num = m_currentMode != mode;
		m_currentMode = mode;
		if (num)
		{
			Log.Arena.Print("SetDraftMode - " + m_currentMode);
			StartCoroutine(InitializeDraftScreen());
		}
	}

	public DraftMode GetDraftMode()
	{
		return m_currentMode;
	}

	public void CancelFindGame()
	{
		GameMgr.Get().CancelFindGame();
		HandleGameStartupFailure();
	}

	public void ZoomHeroCard(Actor hero, bool isDraftingHeroPower)
	{
		SoundManager.Get().LoadAndPlay("tournament_screen_select_hero.prefab:2b9bdf587ac07084b8f7d5c4bce33ecf");
		m_isHeroAnimating = true;
		hero.SetUnlit();
		hero.UpdateCustomFrameDiamondMaterial();
		iTween.MoveTo(hero.gameObject, m_bigHeroBone.position, 0.25f);
		iTween.ScaleTo(hero.gameObject, m_bigHeroBone.localScale, 0.25f);
		SoundManager.Get().LoadAndPlay("forge_hero_portrait_plate_rises.prefab:bffebffeb579074418432f59870e854e");
		FadeEffectsIn();
		LayerUtils.SetLayer(hero.gameObject, GameLayer.IgnoreFullScreenEffects);
		UniversalInputManager.Get().SetGameDialogActive(active: true);
		m_confirmButton.gameObject.SetActive(value: true);
		m_confirmButton.m_button.GetComponent<PlayMakerFSM>().SendEvent("Birth");
		m_confirmButton.AddEventListener(UIEventType.RELEASE, OnConfirmButtonClicked);
		m_heroClickCatcher.AddEventListener(UIEventType.RELEASE, OnCancelButtonClicked);
		m_heroClickCatcher.gameObject.SetActive(value: true);
		hero.TurnOffCollider();
		hero.SetActorState(ActorStateType.CARD_IDLE);
		if (isDraftingHeroPower)
		{
			Actor[] subclassHeroPowerActors = m_subclassHeroPowerActors;
			for (int i = 0; i < subclassHeroPowerActors.Length; i++)
			{
				subclassHeroPowerActors[i].Hide();
			}
		}
		if (isDraftingHeroPower || !m_draftManager.HasSlotType(DraftSlotType.DRAFT_SLOT_HERO_POWER))
		{
			StartCoroutine(ShowHeroPowerWhenDefIsLoaded(isDraftingHeroPower));
		}
	}

	public void OnHeroClicked(int heroChoice)
	{
		Actor chosenHeroActor = null;
		bool isDraftingHeroPower = false;
		if (m_draftManager.GetSlotType() == DraftSlotType.DRAFT_SLOT_HERO)
		{
			chosenHeroActor = m_choices[heroChoice - 1].m_actor;
		}
		else if (m_draftManager.GetSlotType() == DraftSlotType.DRAFT_SLOT_HERO_POWER)
		{
			isDraftingHeroPower = true;
			chosenHeroActor = m_subclassHeroClones[heroChoice - 1];
			Actor chosenHeroPowerActor = m_heroPowerCardActors[heroChoice - 1];
			m_heroPower = chosenHeroPowerActor;
			m_heroPower.Hide();
		}
		if (chosenHeroActor != null)
		{
			m_zoomedHero = chosenHeroActor.GetCollider().gameObject.GetComponent<DraftCardVisual>();
			ZoomHeroCard(chosenHeroActor, isDraftingHeroPower);
		}
		else
		{
			Log.Arena.PrintWarning("DraftDisplay.OnHeroClicked: ChosenHeroActor is null! HeroChoice={0}", heroChoice);
		}
		bool wasEmoteAlreadyReady = true;
		if (!isDraftingHeroPower)
		{
			wasEmoteAlreadyReady = IsHeroEmoteSpellReady(heroChoice - 1);
			StartCoroutine(WaitForSpellToLoadAndPlay(heroChoice - 1));
		}
		if (CanAutoDraft() && wasEmoteAlreadyReady)
		{
			OnConfirmButtonClicked(null);
		}
	}

	private void MakeHeroPowerGoldenIfPremium(DefLoader.DisposableFullDef heroPowerDef)
	{
		EntityDef heroEntityDef = heroPowerDef.EntityDef;
		TAG_PREMIUM premium = CollectionManager.Get().GetHeroPremium(heroEntityDef.GetClass());
		int zoomedHeroIdx = m_zoomedHero.GetChoiceNum() - 1;
		if (m_mythicHeroTypes[zoomedHeroIdx] != 0)
		{
			m_heroPower = m_mythicPowerCardActors[zoomedHeroIdx];
		}
		else
		{
			m_heroPower = ((premium == TAG_PREMIUM.GOLDEN) ? m_goldenHeroPowerSkin : m_defaultHeroPowerSkin);
		}
		m_heroPower.SetCardDef(heroPowerDef.DisposableCardDef);
		m_heroPower.SetEntityDef(heroEntityDef);
		m_heroPower.SetPremium(premium);
		m_heroPower.UpdateAllComponents();
	}

	private bool IsHeroEmoteSpellReady(int index)
	{
		if (!(m_heroEmotes[index] != null))
		{
			return m_skipHeroEmotes;
		}
		return true;
	}

	private IEnumerator WaitForSpellToLoadAndPlay(int index)
	{
		bool wasEmoteAlreadyReady = IsHeroEmoteSpellReady(index);
		while (!IsHeroEmoteSpellReady(index))
		{
			yield return null;
		}
		if (!m_skipHeroEmotes)
		{
			m_heroEmotes[index].Reactivate();
		}
		if (CanAutoDraft() && !wasEmoteAlreadyReady)
		{
			OnConfirmButtonClicked(null);
		}
	}

	public void ClickConfirmButton()
	{
		OnConfirmButtonClicked(null);
	}

	private void OnConfirmButtonClicked(UIEvent e)
	{
		if (GameUtils.IsAnyTransitionActive())
		{
			return;
		}
		EnableBackButton(buttonEnabled: false);
		m_choices.ForEach(delegate(DraftChoice choice)
		{
			if (choice.m_actor != null)
			{
				choice.m_actor.TurnOffCollider();
			}
		});
		if (m_draftManager.GetSlotType() == DraftSlotType.DRAFT_SLOT_HERO_POWER)
		{
			m_subclassHeroClones.ForEach(delegate(Actor choice)
			{
				choice.TurnOffCollider();
			});
		}
		DoHeroSelectAnimation();
	}

	private void OnCancelButtonClicked(UIEvent e)
	{
		if (IsInHeroSelectMode())
		{
			DoHeroCancelAnimation();
		}
		else
		{
			Navigation.GoBack();
		}
	}

	private void RemoveListeners()
	{
		m_confirmButton.RemoveEventListener(UIEventType.RELEASE, OnConfirmButtonClicked);
		m_confirmButton.m_button.GetComponent<PlayMakerFSM>().SendEvent("Death");
		m_confirmButton.gameObject.SetActive(value: false);
		m_heroClickCatcher.RemoveEventListener(UIEventType.RELEASE, OnCancelButtonClicked);
		m_heroClickCatcher.gameObject.SetActive(value: false);
	}

	private void FadeEffectsIn()
	{
		if (!m_fxActive)
		{
			m_fxActive = true;
			ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignetteDesaturatePerspective;
			screenEffectParameters.Time = 0.4f;
			screenEffectParameters.Blur = new BlurParameters(1f, 1f);
			screenEffectParameters.Desaturate = new DesaturateParameters(0f);
			m_screenEffectsHandle.StartEffect(screenEffectParameters);
		}
	}

	private void FadeEffectsOut()
	{
		if (m_fxActive)
		{
			m_fxActive = false;
			m_screenEffectsHandle.StopEffect(0f, OnFadeFinished);
		}
	}

	private void OnFadeFinished()
	{
		if (!(m_chosenHero == null))
		{
			LayerUtils.SetLayer(m_chosenHero.gameObject, GameLayer.Default);
		}
	}

	public void DoHeroCancelAnimation()
	{
		RemoveListeners();
		m_heroPower.Hide();
		Actor chosenHero;
		if (m_draftManager.GetSlotType() == DraftSlotType.DRAFT_SLOT_HERO)
		{
			chosenHero = m_choices[m_zoomedHero.GetChoiceNum() - 1].m_actor;
		}
		else
		{
			chosenHero = m_subclassHeroClones[m_zoomedHero.GetChoiceNum() - 1];
			Actor[] subclassHeroPowerActors = m_subclassHeroPowerActors;
			foreach (Actor obj in subclassHeroPowerActors)
			{
				obj.Show();
				Spell heroPowerFadeIn = obj.GetComponentInChildren<Spell>();
				if (heroPowerFadeIn != null)
				{
					heroPowerFadeIn.Deactivate();
					heroPowerFadeIn.Activate();
				}
			}
		}
		LayerUtils.SetLayer(chosenHero.gameObject, GameLayer.Default);
		chosenHero.TurnOnCollider();
		FadeEffectsOut();
		UniversalInputManager.Get().SetGameDialogActive(active: false);
		m_isHeroAnimating = false;
		m_pickArea.enabled = true;
		iTween.MoveTo(chosenHero.gameObject, GetCardPosition(m_zoomedHero.GetChoiceNum() - 1, isHeroSkin: true), 0.25f);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			iTween.ScaleTo(chosenHero.gameObject, HERO_ACTOR_LOCAL_SCALE_PHONE, 0.25f);
		}
		else
		{
			iTween.ScaleTo(chosenHero.gameObject, HERO_ACTOR_LOCAL_SCALE, 0.25f);
		}
		m_pickArea.enabled = false;
		m_zoomedHero = null;
	}

	public bool IsInHeroSelectMode()
	{
		return m_zoomedHero != null;
	}

	private void DoHeroSelectAnimation()
	{
		bool isDraftingHeroPower = m_draftManager.GetSlotType() == DraftSlotType.DRAFT_SLOT_HERO_POWER;
		RemoveListeners();
		m_heroPower.transform.parent = null;
		if (!isDraftingHeroPower)
		{
			m_heroPower.Hide();
		}
		FadeEffectsOut();
		UniversalInputManager.Get().SetGameDialogActive(active: false);
		m_chosenHero = (isDraftingHeroPower ? m_zoomedHero.GetSubActor() : m_zoomedHero.GetActor());
		m_zoomedHero.SetChosenFlag(bOn: true);
		m_draftManager.MakeChoice(m_zoomedHero.GetChoiceNum(), m_chosenHero.GetPremium());
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			Actor heroActor = null;
			if (!isDraftingHeroPower)
			{
				heroActor = m_zoomedHero.GetActor();
			}
			else
			{
				heroActor = m_zoomedHero.GetSubActor();
				m_inPlayHeroPowerActor = m_subclassHeroPowerActors[m_zoomedHero.GetChoiceNum() - 1];
				Actor heroPowerActor = m_zoomedHero.GetActor();
				heroPowerActor.transform.parent = m_socketHeroPowerBone;
				iTween.MoveTo(heroPowerActor.gameObject, iTween.Hash("position", Vector3.zero, "time", 0.25f, "islocal", true, "easetype", iTween.EaseType.easeInCubic, "oncomplete", "PhoneHeroPowerAnimationFinished", "oncompletetarget", base.gameObject));
				iTween.ScaleTo(heroPowerActor.gameObject, iTween.Hash("scale", Vector3.one, "time", 0.25f, "easetype", iTween.EaseType.easeInCubic));
			}
			heroActor.transform.parent = m_socketHeroBone;
			iTween.MoveTo(heroActor.gameObject, iTween.Hash("position", Vector3.zero, "time", 0.25f, "islocal", true, "easetype", iTween.EaseType.easeInCubic, "oncomplete", "PhoneHeroAnimationFinished", "oncompletetarget", base.gameObject));
			iTween.ScaleTo(heroActor.gameObject, iTween.Hash("scale", Vector3.one, "time", 0.25f, "easetype", iTween.EaseType.easeInCubic));
		}
		else
		{
			m_zoomedHero.GetActor().ActivateSpellBirthState(SpellType.CONSTRUCT);
			m_zoomedHero = null;
			m_isHeroAnimating = false;
		}
		SoundManager.Get().LoadAndPlay("forge_hero_portrait_plate_descend_and_impact.prefab:371e56744a872fc45a4bb3c043e684aa");
		ShowInnkeeperInstructions();
	}

	private void PhoneHeroAnimationFinished()
	{
		Log.Arena.Print("Phone Hero animation complete");
		m_zoomedHero = null;
		m_isHeroAnimating = false;
	}

	private void PhoneHeroPowerAnimationFinished()
	{
		Log.Arena.Print("Phone Hero Power animation complete");
		m_inPlayHeroPowerActor.transform.parent = m_socketHeroPowerBone;
		m_inPlayHeroPowerActor.transform.localPosition = Vector3.zero;
		m_inPlayHeroPowerActor.transform.localScale = Vector3.one;
		m_inPlayHeroPowerActor.Show();
	}

	public void AddCardToManaCurve(EntityDef entityDef)
	{
		if (m_manaCurve == null)
		{
			Debug.LogWarning($"DraftDisplay.AddCardToManaCurve({entityDef}) - m_manaCurve is null");
		}
		else
		{
			m_manaCurve.AddCardToManaCurve(entityDef);
		}
	}

	public List<DraftCardVisual> GetCardVisuals()
	{
		List<DraftCardVisual> cardVisuals = new List<DraftCardVisual>();
		foreach (DraftChoice choice in m_choices)
		{
			if (choice.m_actor == null)
			{
				return null;
			}
			DraftCardVisual cardVis = choice.m_actor.GetCollider().gameObject.GetComponent<DraftCardVisual>();
			if (cardVis != null)
			{
				cardVisuals.Add(cardVis);
				continue;
			}
			if (choice.m_subActor == null)
			{
				return null;
			}
			cardVis = choice.m_subActor.GetCollider().gameObject.GetComponent<DraftCardVisual>();
			if (cardVis != null)
			{
				cardVisuals.Add(cardVis);
				continue;
			}
			return null;
		}
		return cardVisuals;
	}

	public void HandleGameStartupFailure()
	{
		m_playButton.Enable();
		ShowPhonePlayButton(show: true);
		if (PresenceMgr.Get().CurrentStatus == Global.PresenceStatus.ARENA_QUEUE)
		{
			PresenceMgr.Get().SetPrevStatus();
		}
	}

	public void DoDeckCompleteAnims()
	{
		SoundManager.Get().LoadAndPlay("forge_commit_deck.prefab:1e3ef554bb2848b48816f336f2f91569");
		if (!UniversalInputManager.UsePhoneUI)
		{
			m_DeckCompleteSpell.Activate();
			if (m_draftDeckTray != null)
			{
				m_draftDeckTray.GetCardsContent().ShowDeckCompleteEffects();
			}
		}
	}

	public bool DraftAnimationIsComplete()
	{
		return m_animationsComplete;
	}

	private IEnumerator NotifySceneLoadedWhenReady()
	{
		while (m_confirmButton == null)
		{
			yield return null;
		}
		while (m_heroPower == null)
		{
			yield return null;
		}
		while (m_currentMode == DraftMode.INVALID)
		{
			yield return null;
		}
		while (!m_netCacheReady)
		{
			yield return null;
		}
		while (!AchieveManager.Get().IsReady())
		{
			yield return null;
		}
		InitManaCurve();
		m_draftDeckTray.Initialize();
		PegUIElement component = m_draftDeckTray.GetTooltipZone().gameObject.GetComponent<PegUIElement>();
		component.AddEventListener(UIEventType.ROLLOVER, DeckHeaderOver);
		component.AddEventListener(UIEventType.ROLLOUT, DeckHeaderOut);
		SceneMgr.Get().NotifySceneLoaded();
	}

	private IEnumerator InitializeDraftScreen()
	{
		while (!ArenaTrayDisplay.Get().IsReady())
		{
			yield return null;
		}
		if (!m_firstTimeIntroComplete && !Options.Get().GetBool(Option.HAS_SEEN_FORGE, defaultVal: false) && UserAttentionManager.CanShowAttentionGrabber("DraftDisplay.InitializeDraftScreen:" + Option.HAS_SEEN_FORGE))
		{
			while (SceneMgr.Get().IsTransitioning())
			{
				yield return null;
			}
			PresenceMgr.Get().SetStatus(Global.PresenceStatus.ARENA_PURCHASE);
			m_firstTimeIntroComplete = true;
			DoFirstTimeIntro();
			yield break;
		}
		switch (m_currentMode)
		{
		case DraftMode.NO_ACTIVE_DRAFT:
		{
			while (SceneMgr.Get().IsTransitioning())
			{
				yield return null;
			}
			int numTicketsOwned = m_draftManager.GetNumTicketsOwned();
			if (StoreManager.Get().HasOutstandingPurchaseNotices(ProductType.PRODUCT_TYPE_DRAFT))
			{
				ShowPurchaseScreen();
				break;
			}
			if (numTicketsOwned > 0)
			{
				ShowOutstandingTicketScreen(numTicketsOwned);
				break;
			}
			PresenceMgr.Get().SetStatus(Global.PresenceStatus.ARENA_PURCHASE);
			ShowPurchaseScreen();
			break;
		}
		case DraftMode.DRAFTING:
			if (StoreManager.Get().HasOutstandingPurchaseNotices(ProductType.PRODUCT_TYPE_DRAFT))
			{
				while (SceneMgr.Get().IsTransitioning())
				{
					yield return null;
				}
				ShowPurchaseScreen();
			}
			PresenceMgr.Get().SetStatus(Global.PresenceStatus.ARENA_FORGE);
			if (m_draftManager.ShouldShowFreeArenaWinScreen())
			{
				ShowFreeArenaWinScreen();
			}
			else
			{
				ShowCurrentlyDraftingScreen();
			}
			break;
		case DraftMode.ACTIVE_DRAFT_DECK:
			PresenceMgr.Get().SetStatus(Global.PresenceStatus.ARENA_IDLE);
			StartCoroutine(ShowActiveDraftScreen());
			break;
		case DraftMode.IN_REWARDS:
			PresenceMgr.Get().SetStatus(Global.PresenceStatus.ARENA_REWARD);
			ShowDraftRewardsScreen();
			break;
		default:
			Debug.LogError($"DraftDisplay.InitializeDraftScreen(): don't know how to handle m_currentMode = {m_currentMode}");
			break;
		}
	}

	private void OnConfirmButtonLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_confirmButton = go.GetComponent<NormalButton>();
		m_confirmButton.SetText(GameStrings.Get("GLUE_CHOOSE"));
		m_confirmButton.gameObject.SetActive(value: false);
		LayerUtils.SetLayer(go, GameLayer.IgnoreFullScreenEffects);
	}

	private void OnPhoneBackButtonLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogError("Phone Back Button failed to load!");
			return;
		}
		go.transform.SetParent(base.transform, worldPositionStays: true);
		m_backButton = go.GetComponent<UIBButton>();
		m_backButton.transform.parent = m_PhoneBackButtonBone;
		m_backButton.transform.position = m_PhoneBackButtonBone.position;
		m_backButton.transform.localScale = m_PhoneBackButtonBone.localScale;
		m_backButton.transform.rotation = Quaternion.identity;
		LayerUtils.SetLayer(go, GameLayer.Default);
		SetupBackButton();
	}

	private void OnHeroPowerActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogWarning($"DeckPickerTrayDisplay.OnHeroPowerActorLoaded() - FAILED to load actor \"{assetRef}\"");
			return;
		}
		go.transform.SetParent(base.transform, worldPositionStays: true);
		m_inPlayHeroPowerActor = go.GetComponent<Actor>();
		if (m_inPlayHeroPowerActor == null)
		{
			Debug.LogWarning($"DeckPickerTrayDisplay.OnHeroPowerActorLoaded() - ERROR actor \"{assetRef}\" has no Actor component");
			return;
		}
		m_inPlayHeroPowerActor.SetUnlit();
		m_inPlayHeroPowerActor.Hide();
	}

	private void LoadHeroPowerCallback(Actor actor)
	{
		if (actor == null)
		{
			Debug.LogWarning("DeckPickerTrayDisplay.LoadHeroPowerCallback() - ERROR actor null.");
			return;
		}
		actor.transform.SetParent(base.transform, worldPositionStays: true);
		actor.TurnOffCollider();
		LayerUtils.SetLayer(actor.gameObject, GameLayer.IgnoreFullScreenEffects);
		m_heroPower = actor;
		actor.Hide();
	}

	private void LoadHeroPowerCallback(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogWarning($"DeckPickerTrayDisplay.LoadHeroPowerCallback() - FAILED to load actor \"{assetRef}\"");
			return;
		}
		go.transform.SetParent(base.transform, worldPositionStays: true);
		Actor actor = go.GetComponent<Actor>();
		if (actor == null)
		{
			Debug.LogWarning($"DeckPickerTrayDisplay.LoadHeroPowerCallback() - ERROR actor \"{assetRef}\" has no Actor component");
			return;
		}
		actor.TurnOffCollider();
		LayerUtils.SetLayer(actor.gameObject, GameLayer.IgnoreFullScreenEffects);
		m_defaultHeroPowerSkin = actor;
		m_heroPower = actor;
		actor.Hide();
	}

	private void LoadGoldenHeroPowerCallback(AssetReference assetRef, GameObject go, object callbackData)
	{
		go.transform.SetParent(base.transform, worldPositionStays: true);
		m_goldenHeroPowerSkin = go.GetComponent<Actor>();
	}

	private void ShowHeroPowerBigCard(bool isDraftingHeroPower)
	{
		if (!(m_heroPower == null))
		{
			LayerUtils.SetLayer(m_heroPower.gameObject, GameLayer.IgnoreFullScreenEffects);
			Actor m_actorToParent = m_zoomedHero.GetSubActor();
			if (m_actorToParent == null)
			{
				m_actorToParent = m_zoomedHero.GetActor();
			}
			m_heroPower.gameObject.transform.SetParent(m_actorToParent.transform);
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				m_heroPower.gameObject.transform.localPosition = HERO_POWER_START_POSITION_PHONE;
				m_heroPower.gameObject.transform.localScale = HERO_POWER_SCALE_PHONE;
			}
			else if (!isDraftingHeroPower)
			{
				m_heroPower.gameObject.transform.localPosition = HERO_POWER_START_POSITION;
				m_heroPower.gameObject.transform.localScale = HERO_POWER_SCALE;
			}
			else
			{
				m_heroPower.gameObject.transform.localPosition = HERO_POWER_START_POSITION;
				m_heroPower.gameObject.transform.localScale = DRAFTING_HERO_POWER_BIG_CARD_SCALE;
			}
		}
	}

	private void ShowHeroPower(Actor actor)
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_heroPower.gameObject.transform.localPosition = HERO_POWER_TOOLTIP_POSITION_PHONE;
			m_heroPower.gameObject.transform.localScale = HERO_POWER_TOOLTIP_SCALE_PHONE;
		}
		else
		{
			m_heroPower.gameObject.transform.localPosition = HERO_POWER_TOOLTIP_POSITION;
			m_heroPower.gameObject.transform.localScale = HERO_POWER_TOOLTIP_SCALE;
		}
		m_heroPower.SetFullDefFromActor(actor);
		m_heroPower.UpdateAllComponents();
		m_heroPower.Show();
	}

	private IEnumerator ShowHeroPowerWhenDefIsLoaded(bool isDraftingHeroPower = false)
	{
		if (m_zoomedHero == null)
		{
			yield break;
		}
		if (!isDraftingHeroPower)
		{
			while (m_heroPowerDefs[m_zoomedHero.GetChoiceNum() - 1] == null)
			{
				yield return null;
			}
			int zoomedHeroIdx = m_zoomedHero.GetChoiceNum() - 1;
			if (m_mythicHeroTypes[zoomedHeroIdx] != 0)
			{
				while (m_mythicPowerCardActors[zoomedHeroIdx] == null)
				{
					yield return null;
				}
				m_heroPower = m_mythicPowerCardActors[zoomedHeroIdx];
			}
			DefLoader.DisposableFullDef def = m_heroPowerDefs[zoomedHeroIdx];
			MakeHeroPowerGoldenIfPremium(def);
			if (!GameUtils.IsVanillaHero(m_zoomedHero.GetActor().GetEntityDef().GetCardId()))
			{
				def.CardDef.m_AlwaysRenderPremiumPortrait = true;
			}
		}
		m_heroPower.Show();
		ShowHeroPowerBigCard(isDraftingHeroPower);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			iTween.MoveTo(m_heroPower.gameObject, iTween.Hash("position", HERO_POWER_POSITION_PHONE, "islocal", true, "time", 0.5f));
		}
		else if (!isDraftingHeroPower)
		{
			iTween.MoveTo(m_heroPower.gameObject, iTween.Hash("position", HERO_POWER_POSITION, "islocal", true, "time", 0.5f));
		}
		else
		{
			iTween.MoveTo(m_heroPower.gameObject, iTween.Hash("position", DRAFTING_HERO_POWER_POSITION, "islocal", true, "time", 0.5f));
		}
	}

	private IEnumerator WaitAndPositionHeroPower()
	{
		yield return new WaitForSeconds(0.35f);
		m_inPlayHeroPowerActor = m_subclassHeroPowerActors[m_draftManager.ChosenIndex - 1];
		m_inPlayHeroPowerActor.transform.localPosition = m_socketHeroPowerBone.transform.localPosition;
		m_inPlayHeroPowerActor.transform.localScale = m_socketHeroPowerBone.transform.localScale;
		SetupToDisplayHeroPowerTooltip(m_inPlayHeroPowerActor);
		Spell inPlayHeroPowerSummonIn = m_inPlayHeroPowerActor.GetComponentInChildren<Spell>();
		if (inPlayHeroPowerSummonIn != null)
		{
			inPlayHeroPowerSummonIn.Activate();
		}
		m_inPlayHeroPowerActor.Show();
		DraftCardVisual draftCardVisual = m_inPlayHeroPowerActor.GetComponentInChildren<DraftCardVisual>();
		if (draftCardVisual != null)
		{
			UnityEngine.Object.Destroy(draftCardVisual);
		}
	}

	private void DestroyOldChoices()
	{
		m_animationsComplete = false;
		for (int index = 1; index < m_choices.Count + 1; index++)
		{
			DraftChoice draftChoice = m_choices[index - 1];
			Actor choiceActor = draftChoice.m_actor;
			if (choiceActor == null)
			{
				continue;
			}
			Actor choiceSubActor = draftChoice.m_subActor;
			choiceActor.TurnOffCollider();
			Spell raritySpell = choiceActor.GetSpell(GetSpellTypeForRarity(choiceActor.GetEntityDef().GetRarity()));
			if (index == m_draftManager.ChosenIndex)
			{
				if (choiceActor.GetEntityDef().IsHeroSkin())
				{
					foreach (HeroLabel currentLabel in m_currentLabels)
					{
						currentLabel.FadeOut();
					}
				}
				else if (choiceActor.GetEntityDef().IsHeroPower())
				{
					choiceActor.transform.parent = null;
					LayerUtils.SetLayer(choiceActor.gameObject, GameLayer.IgnoreFullScreenEffects);
					if (!UniversalInputManager.UsePhoneUI)
					{
						m_heroPower = choiceActor.Clone();
						m_heroPower.Hide();
						Spell componentInChildren = choiceActor.GetComponentInChildren<Spell>();
						componentInChildren.AddFinishedCallback(CleanupChoicesOnSpellFinish_HeroPower, choiceActor);
						choiceActor.Show();
						componentInChildren.Activate();
						StartCoroutine(WaitAndPositionHeroPower());
					}
					else
					{
						Actor[] subclassHeroPowerActors = m_subclassHeroPowerActors;
						for (int i = 0; i < subclassHeroPowerActors.Length; i++)
						{
							subclassHeroPowerActors[i].Hide();
						}
						SetupToDisplayHeroPowerTooltip(m_inPlayHeroPowerActor);
						m_heroPower.Hide();
					}
					foreach (HeroLabel label in m_currentLabels)
					{
						if (label != null)
						{
							label.FadeOut();
						}
					}
				}
				else
				{
					Spell summonOutSpell = choiceActor.GetSpell(SpellType.SUMMON_OUT_FORGE);
					if (summonOutSpell == null)
					{
						Debug.LogError("DraftDisplay.DestroyOldChoices: The SUMMON_OUT_FORGE spell is missing from the spell table for this card.");
						continue;
					}
					summonOutSpell.AddFinishedCallback(DestroyChoiceOnSpellFinish, choiceActor);
					choiceActor.ActivateSpellBirthState(SpellType.SUMMON_OUT_FORGE);
					raritySpell.ActivateState(SpellStateType.DEATH);
					SoundManager.Get().LoadAndPlay("forge_select_card_1.prefab:b770cd64bb913f0409902629f975421e");
				}
			}
			else
			{
				SoundManager.Get().LoadAndPlay("unselected_cards_dissipate.prefab:a68b6959b8e9ed4408bf2475f37fd97d");
				Spell burnSpell = choiceActor.GetSpell(SpellType.BURN);
				if (burnSpell != null)
				{
					burnSpell.AddFinishedCallback(DestroyChoiceOnSpellFinish, choiceActor);
					choiceActor.ActivateSpellBirthState(SpellType.BURN);
				}
				burnSpell = ((choiceSubActor == null) ? null : choiceSubActor.GetSpell(SpellType.BURN));
				if (burnSpell != null)
				{
					burnSpell.AddFinishedCallback(DestroyChoiceOnSpellFinish, choiceSubActor);
					choiceSubActor.ActivateSpellBirthState(SpellType.BURN);
				}
				if (raritySpell != null)
				{
					raritySpell.ActivateState(SpellStateType.DEATH);
				}
			}
		}
		StartCoroutine(CompleteAnims());
		m_inPositionAndShowChoices = false;
	}

	private void SetupToDisplayHeroPowerTooltip(Actor actor)
	{
		if (actor == null)
		{
			Log.Arena.PrintWarning("DraftDisplay.SetupToDisplayHeroPowerTooltip: Actor is null!");
			return;
		}
		PegUIElement peg = actor.gameObject.GetComponent<PegUIElement>();
		if (peg == null)
		{
			peg = actor.gameObject.AddComponent<PegUIElement>();
			peg.gameObject.AddComponent<BoxCollider>();
		}
		peg.AddEventListener(UIEventType.ROLLOVER, OnMouseOverHeroPower);
		peg.AddEventListener(UIEventType.ROLLOUT, OnMouseOutHeroPower);
		actor.Show();
	}

	private IEnumerator CompleteAnims()
	{
		yield return new WaitForSeconds(0.5f);
		m_animationsComplete = true;
	}

	private void CleanupChoicesOnSpellFinish_HeroPower(Spell spell, object actorObject)
	{
		foreach (Actor subclassHeroClone in m_subclassHeroClones)
		{
			subclassHeroClone.Hide();
		}
		Actor[] subclassHeroPowerActors = m_subclassHeroPowerActors;
		foreach (Actor heroPower in subclassHeroPowerActors)
		{
			if (heroPower != m_inPlayHeroPowerActor)
			{
				heroPower.Hide();
			}
		}
		DestroyChoiceOnSpellFinish(spell, actorObject);
	}

	private void DestroyChoiceOnSpellFinish(Spell spell, object actorObject)
	{
		Actor actor = (Actor)actorObject;
		StartCoroutine(DestroyObjectAfterDelay(actor.gameObject));
	}

	private IEnumerator DestroyObjectAfterDelay(GameObject gameObjectToDestroy)
	{
		yield return new WaitForSeconds(5f);
		UnityEngine.Object.Destroy(gameObjectToDestroy);
	}

	private void OnFullDefLoaded(string cardID, DefLoader.DisposableFullDef def, object userData)
	{
		using (def)
		{
			if (def == null)
			{
				Debug.LogErrorFormat("Unable to load FullDef for cardID={0}", cardID);
				return;
			}
			ChoiceCallback callbackInfo = (ChoiceCallback)userData;
			callbackInfo.fullDef = def;
			if (def.EntityDef.IsHeroSkin())
			{
				CornerReplacementSpellType mythicHeroType = callbackInfo.fullDef.EntityDef?.GetTag<CornerReplacementSpellType>(GAME_TAG.CORNER_REPLACEMENT_TYPE) ?? CornerReplacementSpellType.NONE;
				m_mythicHeroTypes[callbackInfo.choiceID - 1] = mythicHeroType;
				if (mythicHeroType != 0)
				{
					string actorName = CornerReplacementConfig.Get().GetActor(mythicHeroType, ActorNames.ACTOR_ASSET.HISTORY_HERO_POWER, callbackInfo.premium);
					if (string.IsNullOrEmpty(actorName))
					{
						Debug.LogWarningFormat("Unable to find replacement History Hero Power Actor for {0}, defaulting to normal flow", mythicHeroType);
						m_mythicHeroTypes[callbackInfo.choiceID - 1] = CornerReplacementSpellType.NONE;
					}
					else
					{
						AssetLoader.Get().InstantiatePrefab(actorName, OnMythicHeroPowerLoaded, callbackInfo.Copy(), AssetLoadingOptions.IgnorePrefabPosition);
					}
				}
				AssetLoader.Get().InstantiatePrefab(ActorNames.GetZoneActor(def.EntityDef, TAG_ZONE.PLAY), OnActorLoaded, callbackInfo.Copy(), AssetLoadingOptions.IgnorePrefabPosition);
				DefLoader.Get().LoadCardDef(def.EntityDef.GetCardId(), OnCardDefLoaded, callbackInfo.choiceID);
				string heroPowerID = GameUtils.GetHeroPowerCardIdFromHero(def.EntityDef.GetCardId());
				DefLoader.Get().LoadFullDef(heroPowerID, OnHeroPowerFullDefLoaded, callbackInfo.choiceID);
			}
			else if (def.EntityDef.IsHeroPower())
			{
				AssetLoader.Get().InstantiatePrefab(ActorNames.GetHandActor(def.EntityDef, callbackInfo.premium), OnActorLoaded, callbackInfo.Copy(), AssetLoadingOptions.IgnorePrefabPosition);
				AssetLoader.Get().InstantiatePrefab(ActorNames.GetZoneActor(def.EntityDef, TAG_ZONE.PLAY, callbackInfo.premium), OnSubClassActorLoaded, callbackInfo.Copy(), AssetLoadingOptions.IgnorePrefabPosition);
			}
			else
			{
				AssetLoader.Get().InstantiatePrefab(ActorNames.GetHandActor(def.EntityDef, callbackInfo.premium), OnActorLoaded, callbackInfo.Copy(), AssetLoadingOptions.IgnorePrefabPosition);
			}
		}
	}

	private void OnHeroPowerFullDefLoaded(string cardID, DefLoader.DisposableFullDef def, object userData)
	{
		int choiceID = (int)userData;
		m_heroPowerDefs[choiceID - 1]?.Dispose();
		m_heroPowerDefs[choiceID - 1] = def;
	}

	public void ShowInnkeeperInstructions()
	{
		if (m_draftManager.GetSlotType() == DraftSlotType.DRAFT_SLOT_HERO && !Options.Get().GetBool(Option.HAS_SEEN_FORGE_HERO_CHOICE, defaultVal: false) && UserAttentionManager.CanShowAttentionGrabber("DraftDisplay.UpdateInstructionText:" + Option.HAS_SEEN_FORGE_HERO_CHOICE))
		{
			if (!m_draftManager.HasSlotType(DraftSlotType.DRAFT_SLOT_HERO_POWER))
			{
				NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, GameStrings.Get("VO_INNKEEPER_FORGE_INST1_19"), "VO_INNKEEPER_FORGE_INST1_19.prefab:a0e06e90b545b274290dad8e442e83d0", 3f);
				Options.Get().SetBool(Option.HAS_SEEN_FORGE_HERO_CHOICE, val: true);
			}
		}
		else if (m_draftManager.GetSlotType() == DraftSlotType.DRAFT_SLOT_CARD && !Options.Get().GetBool(Option.HAS_SEEN_FORGE_CARD_CHOICE, defaultVal: false) && UserAttentionManager.CanShowAttentionGrabber("DraftDisplay.DoHeroSelectAnimation:" + Option.HAS_SEEN_FORGE_CARD_CHOICE))
		{
			Options.Get().SetBool(Option.HAS_SEEN_FORGE_HERO_CHOICE, val: true);
			NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, GameStrings.Get("VO_INNKEEPER_FORGE_INST2_20"), "VO_INNKEEPER_FORGE_INST2_20.prefab:242b6a30031534e47b1f8ddd69370eac", 3f);
			Options.Get().SetBool(Option.HAS_SEEN_FORGE_CARD_CHOICE, val: true);
		}
		else if (m_draftManager.GetSlotType() == DraftSlotType.DRAFT_SLOT_CARD && !Options.Get().GetBool(Option.HAS_SEEN_FORGE_CARD_CHOICE2, defaultVal: false) && UserAttentionManager.CanShowAttentionGrabber("DraftDisplay.UpdateInstructionText:" + Option.HAS_SEEN_FORGE_CARD_CHOICE2))
		{
			NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, GameStrings.Get("VO_INNKEEPER_FORGE_INST3_21"), "VO_INNKEEPER_FORGE_INST3_21.prefab:06182dd3360965d4ea48952a6dd4a720", 3f);
			Options.Get().SetBool(Option.HAS_SEEN_FORGE_CARD_CHOICE2, val: true);
		}
	}

	public void SetInstructionText()
	{
		switch (m_draftManager.GetSlotType())
		{
		case DraftSlotType.DRAFT_SLOT_CARD:
			m_instructionText.Text = GameStrings.Get("GLUE_DRAFT_INSTRUCTIONS");
			m_instructionDetailText.Text = "";
			break;
		case DraftSlotType.DRAFT_SLOT_HERO:
			m_instructionText.Text = GameStrings.Get("GLUE_DRAFT_HERO_INSTRUCTIONS");
			m_instructionDetailText.Text = "";
			break;
		case DraftSlotType.DRAFT_SLOT_HERO_POWER:
			m_instructionText.Text = GameStrings.Get("GLUE_DRAFT_HERO_POWER_INSTRUCTIONS_TITLE");
			m_instructionDetailText.Text = GameStrings.Get("GLUE_DRAFT_HERO_POWER_INSTRUCTIONS_DETAIL");
			break;
		default:
			m_instructionText.Text = GameStrings.Get("GLUE_DRAFT_INSTRUCTIONS");
			m_instructionDetailText.Text = "";
			break;
		}
	}

	private void UpdateInstructionText()
	{
		if (GetDraftMode() == DraftMode.DRAFTING)
		{
			ShowInnkeeperInstructions();
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				switch (m_draftManager.GetSlotType())
				{
				case DraftSlotType.DRAFT_SLOT_HERO:
					m_PhoneDeckControl.SetMode(ArenaPhoneControl.ControlMode.ChooseHero);
					return;
				case DraftSlotType.DRAFT_SLOT_HERO_POWER:
					m_PhoneDeckControl.SetMode(ArenaPhoneControl.ControlMode.ChooseHeroPower);
					return;
				}
				if (m_draftManager.GetDraftDeck().GetTotalCardCount() > 0)
				{
					m_PhoneDeckControl.SetMode(ArenaPhoneControl.ControlMode.CardCountViewDeck);
				}
				else
				{
					m_PhoneDeckControl.SetMode(ArenaPhoneControl.ControlMode.ChooseCard);
				}
			}
			else
			{
				SetInstructionText();
			}
		}
		else if (GetDraftMode() == DraftMode.ACTIVE_DRAFT_DECK)
		{
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				m_PhoneDeckControl.SetMode(ArenaPhoneControl.ControlMode.ViewDeck);
			}
			else
			{
				m_instructionText.Text = GameStrings.Get("GLUE_DRAFT_MATCH_PROG");
			}
		}
		else
		{
			m_instructionText.Text = "";
		}
	}

	private void DoFirstTimeIntro()
	{
		Box.Get().SetToIgnoreFullScreenEffects(ignoreEffects: true);
		if (!UniversalInputManager.UsePhoneUI)
		{
			m_playButton.Disable();
		}
		ShowPhonePlayButton(show: false);
		m_retireButton.Disable();
		if ((bool)m_manaCurve)
		{
			m_manaCurve.ResetBars();
		}
		if (!UniversalInputManager.UsePhoneUI)
		{
			StoreManager.Get().StartArenaTransaction(OnStoreBackButtonPressed, null, isTotallyFake: true);
		}
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_ARENA_1ST_TIME_HEADER");
		info.m_text = GameStrings.Get("GLUE_ARENA_1ST_TIME_DESC");
		info.m_showAlertIcon = false;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		info.m_responseCallback = OnFirstTimeIntroOkButtonPressed;
		info.m_id = "arena_first_time";
		DialogManager.Get().ShowPopup(info, delegate(DialogBase dialog, object userData)
		{
			m_firstTimeDialog = dialog;
			return true;
		});
		SoundManager.Get().LoadAndPlay("VO_INNKEEPER_ARENA_INTRO2.prefab:40f8c705d6df66445937a3ded7460725");
	}

	private void OnFirstTimeIntroOkButtonPressed(AlertPopup.Response response, object userData)
	{
		StoreManager.Get().HideStore(ShopType.ARENA_STORE);
		m_draftManager.RequestDraftBegin();
		Options.Get().SetBool(Option.HAS_SEEN_FORGE, val: true);
	}

	private void ShowFreeArenaWinScreen()
	{
		Box.Get().SetToIgnoreFullScreenEffects(ignoreEffects: true);
		if (!UniversalInputManager.UsePhoneUI)
		{
			m_playButton.Disable();
		}
		ShowPhonePlayButton(show: false);
		m_retireButton.Disable();
		if ((bool)m_manaCurve)
		{
			m_manaCurve.ResetBars();
		}
		StoreManager.Get().HideStore(ShopType.ARENA_STORE);
		FreeArenaWinDialog.Info info = new FreeArenaWinDialog.Info();
		info.m_callbackOnHide = OnFreeArenaWinOkButtonPress;
		info.m_winCount = m_draftManager.GetWins();
		DialogManager.Get().ShowFreeArenaWinPopup(UserAttentionBlocker.NONE, info);
	}

	private void ShowOutstandingTicketScreen(int numTicketsOwned)
	{
		Box.Get().SetToIgnoreFullScreenEffects(ignoreEffects: true);
		if (!UniversalInputManager.UsePhoneUI)
		{
			m_playButton.Disable();
		}
		ShowPhonePlayButton(show: false);
		m_retireButton.Disable();
		if ((bool)m_manaCurve)
		{
			m_manaCurve.ResetBars();
		}
		OutstandingDraftTicketDialog.Info info = new OutstandingDraftTicketDialog.Info();
		info.m_callbackOnEnter = OnOutstandingTicketEnterButtonPress;
		info.m_callbackOnCancel = OnOutstandingTicketCancelButtonPress;
		info.m_outstandingTicketCount = numTicketsOwned;
		DialogManager.Get().ShowOutstandingDraftTicketPopup(UserAttentionBlocker.NONE, info);
	}

	private void ShowPurchaseScreen()
	{
		Box.Get().SetToIgnoreFullScreenEffects(ignoreEffects: true);
		if (!UniversalInputManager.UsePhoneUI)
		{
			m_playButton.Disable();
		}
		ShowPhonePlayButton(show: false);
		m_retireButton.Disable();
		if ((bool)m_manaCurve)
		{
			m_manaCurve.ResetBars();
		}
		if (DemoMgr.Get().ArenaIs1WinMode())
		{
			Network.Get().PurchaseViaGold(1, ProductType.PRODUCT_TYPE_DRAFT, 0);
		}
		else
		{
			StoreManager.Get().StartArenaTransaction(OnStoreBackButtonPressed, null, isTotallyFake: false);
		}
	}

	private void ShowCurrentlyDraftingScreen()
	{
		m_wasDrafting = true;
		ArenaTrayDisplay.Get().ShowPlainPaperBackground();
		StoreManager.Get().HideStore(ShopType.ARENA_STORE);
		UpdateInstructionText();
		m_retireButton.Disable();
		if (!UniversalInputManager.UsePhoneUI)
		{
			m_playButton.Disable();
		}
		ShowPhonePlayButton(show: false);
		LoadAndPositionHeroCard();
		NarrativeManager.Get().OnArenaDraftStarted();
	}

	private IEnumerator ShowActiveDraftScreen()
	{
		StoreManager.Get().HideStore(ShopType.ARENA_STORE);
		int losses = m_draftManager.GetLosses();
		DestroyOldChoices();
		m_retireButton.Enable();
		m_playButton.Enable();
		ShowPhonePlayButton(show: true);
		UpdateInstructionText();
		LoadAndPositionHeroCard();
		if (m_wasDrafting)
		{
			yield return new WaitForSeconds(0.3f);
		}
		ArenaTrayDisplay.Get().UpdateTray();
		if (!UserAttentionManager.CanShowAttentionGrabber("DraftDisplay.ShowActiveDraftScreen"))
		{
			yield break;
		}
		if (!Options.Get().GetBool(Option.HAS_SEEN_FORGE_PLAY_MODE, defaultVal: false))
		{
			if (m_draftManager.GetWins() == 0 && losses == 0)
			{
				NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, GameStrings.Get("VO_INNKEEPER_FORGE_COMPLETE_22"), "VO_INNKEEPER_ARENA_COMPLETE.prefab:d0c3736823e5a47479bc204abb7a6e71");
				Options.Get().SetBool(Option.HAS_SEEN_FORGE_PLAY_MODE, val: true);
			}
		}
		else if (losses == 2 && !Options.Get().GetBool(Option.HAS_SEEN_FORGE_2LOSS, defaultVal: false))
		{
			NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, GameStrings.Get("VO_INNKEEPER_FORGE_2LOSS_25"), "VO_INNKEEPER_FORGE_2LOSS_25.prefab:82e4f0325619e9d4e9a7fb384b6f7e47", 3f);
			Options.Get().SetBool(Option.HAS_SEEN_FORGE_2LOSS, val: true);
		}
		else if (m_draftManager.GetWins() == 1 && !Options.Get().GetBool(Option.HAS_SEEN_FORGE_1WIN, defaultVal: false))
		{
			while (GameToastMgr.Get().AreToastsActive())
			{
				yield return null;
			}
			NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, new Vector3(133.1f, NotificationManager.DEPTH, 54.2f), GameStrings.Get("VO_INNKEEPER_FORGE_1WIN"), "VO_INNKEEPER_ARENA_1WIN.prefab:31bb13e800c74c0439ee1a7bfc1e3499");
			Options.Get().SetBool(Option.HAS_SEEN_FORGE_1WIN, val: true);
		}
	}

	private void ShowDraftRewardsScreen()
	{
		if (!UniversalInputManager.UsePhoneUI)
		{
			m_playButton.Disable();
		}
		ShowPhonePlayButton(show: false);
		EnableBackButton(buttonEnabled: false);
		m_retireButton.Disable();
		if (DemoMgr.Get().ArenaIs1WinMode())
		{
			StartCoroutine(RestartArena());
			return;
		}
		if (m_draftManager.ShouldActivateKey())
		{
			int maxWins = m_draftManager.GetMaxWins();
			if (m_draftManager.GetWins() >= maxWins && !Options.Get().GetBool(Option.HAS_SEEN_FORGE_MAX_WIN, defaultVal: false) && UserAttentionManager.CanShowAttentionGrabber("DraftDisplay.ShowDraftRewardsScreen:" + Option.HAS_SEEN_FORGE_MAX_WIN))
			{
				NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, GameStrings.Get("VO_INNKEEPER_MAX_ARENA_WINS_04"), "VO_INNKEEPER_MAX_ARENA_WINS_04.prefab:cdf8e488f2d17604499f2cc358cb35f6");
				Options.Get().SetBool(Option.HAS_SEEN_FORGE_MAX_WIN, val: true);
			}
			ArenaTrayDisplay.Get().UpdateTray(showNewKey: false);
			ArenaTrayDisplay.Get().ActivateKey();
			if (m_PhoneDeckControl != null)
			{
				m_PhoneDeckControl.SetMode(ArenaPhoneControl.ControlMode.Rewards);
			}
		}
		else
		{
			ArenaTrayDisplay.Get().ShowRewardsOpenAtStart();
		}
		LoadAndPositionHeroCard();
	}

	private IEnumerator RestartArena()
	{
		Debug.LogWarning("Restarting");
		int wins = m_draftManager.GetWins();
		if (wins < 5)
		{
			DemoMgr.Get().CreateDemoText(GameStrings.Get("GLUE_BLIZZCON2013_ARENA_NO_PRIZE"), unclickable: true);
		}
		else if (wins < 9)
		{
			DemoMgr.Get().CreateDemoText(GameStrings.Get("GLUE_BLIZZCON2013_ARENA_PRIZE"), unclickable: true);
		}
		else if (wins == 9)
		{
			DemoMgr.Get().CreateDemoText(GameStrings.Get("GLUE_BLIZZCON2013_ARENA_GRAND_PRIZE"), unclickable: true);
		}
		AssetLoader.Get().InstantiatePrefab("NumberLabel.prefab:597544d5ed24b994f95fe56e28584992", LastArenaWinsLabelLoaded, wins, AssetLoadingOptions.IgnorePrefabPosition);
		m_currentLabels = new List<HeroLabel>();
		yield return new WaitForSeconds(6f);
		SetDraftMode(DraftMode.NO_ACTIVE_DRAFT);
		yield return new WaitForSeconds(2f);
		Network.Get().AckDraftRewards(m_draftManager.GetDraftDeck().ID, m_draftManager.GetSlot());
		yield return new WaitForSeconds(1f);
		ArenaTrayDisplay.Get().UpdateTray();
		if (m_chosenHero != null)
		{
			UnityEngine.Object.Destroy(m_chosenHero.gameObject);
		}
		yield return new WaitForSeconds(1f);
		Network.Get().PurchaseViaGold(1, ProductType.PRODUCT_TYPE_DRAFT, 0);
		yield return new WaitForSeconds(15f);
		if (wins >= 5)
		{
			DemoMgr.Get().MakeDemoTextClickable(clickable: true);
			DemoMgr.Get().NextDemoTipIsNewArenaMatch();
		}
		else
		{
			DemoMgr.Get().RemoveDemoTextDialog();
			DemoMgr.Get().CreateDemoText(GameStrings.Get("GLUE_BLIZZCON2013_ARENA"), unclickable: false, shouldDoArenaInstruction: true);
		}
	}

	private void LastArenaWinsLabelLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		int wins = (int)callbackData;
		go.GetComponent<UberText>().Text = "Last Arena: " + wins + " Wins";
		go.transform.position = new Vector3(11.40591f, 1.341853f, 29.28797f);
		go.transform.localScale = new Vector3(15f, 15f, 15f);
	}

	private void LoadAndPositionHeroCard()
	{
		if (m_chosenHero != null)
		{
			return;
		}
		CollectionDeck deck = m_draftManager.GetDraftDeck();
		if (deck == null)
		{
			Log.All.Print("bug 8052, null exception");
			return;
		}
		TAG_PREMIUM heroPremium = CollectionManager.Get().GetHeroPremium(deck.GetClass());
		GameUtils.LoadAndPositionCardActor("Card_Play_Hero.prefab:42cbbd2c4969afb46b3887bb628de19d", deck.HeroCardID, heroPremium, OnHeroActorLoaded);
		string playHeroPowerActorToLoad;
		if (heroPremium == TAG_PREMIUM.GOLDEN)
		{
			playHeroPowerActorToLoad = "Card_Play_HeroPower_Premium.prefab:015ad985f9ec49e4db327d131fd79901";
			GameUtils.LoadAndPositionCardActor("History_HeroPower_Premium.prefab:081da807b95b8495e9f16825c5164787", deck.HeroPowerCardID, heroPremium, LoadHeroPowerCallback);
		}
		else
		{
			playHeroPowerActorToLoad = "Card_Play_HeroPower.prefab:a3794839abb947146903a26be13e09af";
		}
		GameUtils.LoadAndPositionCardActor(playHeroPowerActorToLoad, deck.HeroPowerCardID, heroPremium, OnHeroPowerActorLoaded);
	}

	private void OnNetCacheReady()
	{
		NetCache.Get().UnregisterNetCacheHandler(OnNetCacheReady);
		if (!NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Games.Forge)
		{
			if (!SceneMgr.Get().IsModeRequested(SceneMgr.Mode.HUB))
			{
				SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
				Error.AddWarningLoc("GLOBAL_FEATURE_DISABLED_TITLE", "GLOBAL_FEATURE_DISABLED_MESSAGE_FORGE");
			}
		}
		else
		{
			m_netCacheReady = true;
		}
	}

	private void PositionAndShowChoices()
	{
		if (m_inPositionAndShowChoices)
		{
			return;
		}
		m_inPositionAndShowChoices = true;
		m_pickArea.enabled = true;
		for (int i = 0; i < m_choices.Count; i++)
		{
			DraftChoice choice = m_choices[i];
			if (choice.m_actor == null)
			{
				Debug.LogWarning($"DraftDisplay.PositionAndShowChoices(): WARNING found choice with null actor (cardID = {choice.m_cardID}). Skipping...");
				continue;
			}
			bool isHeroSkin = choice.m_actor.GetEntityDef().IsHeroSkin();
			bool isHeroPower = choice.m_actor.GetEntityDef().IsHeroPower();
			TAG_RARITY rarity = TAG_RARITY.COMMON;
			Actor heroCopyActor = null;
			Actor heroPowerActor = null;
			if (isHeroPower)
			{
				LayerUtils.SetLayer(m_chosenHero.gameObject, GameLayer.Default);
				heroCopyActor = m_chosenHero.Clone();
				UberShaderController[] uberShaderControllers = heroCopyActor.GetComponentsInChildren<UberShaderController>(includeInactive: true);
				if (uberShaderControllers != null)
				{
					foreach (UberShaderController controller in uberShaderControllers)
					{
						if (controller.UberShaderAnimation != null)
						{
							controller.UberShaderAnimation = UnityEngine.Object.Instantiate(controller.UberShaderAnimation);
						}
					}
				}
				heroCopyActor.transform.position = GetCardPosition(i, isHeroSkin: true);
				heroCopyActor.Show();
				heroCopyActor.ActivateSpellBirthState(SpellType.SUMMON_IN_FORGE);
				rarity = heroCopyActor.GetEntityDef().GetRarity();
				heroCopyActor.ActivateSpellBirthState(GetSpellTypeForRarity(rarity));
				m_subclassHeroClones.Add(heroCopyActor);
				DraftCardVisual cardVis = heroCopyActor.GetCollider().gameObject.GetComponent<DraftCardVisual>();
				if (cardVis == null)
				{
					cardVis = heroCopyActor.GetCollider().gameObject.AddComponent<DraftCardVisual>();
				}
				cardVis.SetChoiceNum(i + 1);
				cardVis.SetActor(choice.m_actor);
				cardVis.SetSubActor(heroCopyActor);
				choice.m_subActor = heroCopyActor;
				heroCopyActor.TurnOnCollider();
				heroPowerActor = m_subclassHeroPowerActors[i];
				heroPowerActor.transform.position = m_heroPowerBones[i].position;
				if ((bool)UniversalInputManager.UsePhoneUI)
				{
					heroPowerActor.transform.localScale = DRAFTING_HERO_POWER_SCALE_PHONE;
				}
				else
				{
					heroPowerActor.transform.localScale = DRAFTING_HERO_POWER_SCALE;
					SpellUtils.SetCustomSpellParent(SpellManager.Get().GetSpell(m_heroPowerChosenFadeOut), choice.m_actor);
				}
				cardVis = heroPowerActor.GetCollider().gameObject.AddComponent<DraftCardVisual>();
				cardVis.SetChoiceNum(i + 1);
				cardVis.SetActor(choice.m_actor);
				cardVis.SetSubActor(heroCopyActor);
				heroPowerActor.TurnOnCollider();
				DefLoader.DisposableFullDef def = m_subClassHeroPowerDefs[i];
				heroPowerActor.SetPremium(choice.m_premium);
				heroPowerActor.SetCardDef(def.DisposableCardDef);
				heroPowerActor.SetEntityDef(def.EntityDef);
				heroPowerActor.UpdateAllComponents();
				heroPowerActor.Hide();
				Spell inPlayHeroPowerSummonIn = SpellManager.Get().GetSpell(m_heroPowerChosenFadeIn);
				SpellUtils.SetCustomSpellParent(inPlayHeroPowerSummonIn, heroPowerActor);
				inPlayHeroPowerSummonIn.transform.localPosition = new Vector3(inPlayHeroPowerSummonIn.transform.localPosition.x, inPlayHeroPowerSummonIn.transform.localPosition.y + 0.5f, inPlayHeroPowerSummonIn.transform.localPosition.z);
			}
			else
			{
				choice.m_actor.transform.position = GetCardPosition(i, isHeroSkin);
				choice.m_actor.Show();
				choice.m_actor.ActivateSpellBirthState(SpellType.SUMMON_IN_FORGE);
				rarity = choice.m_actor.GetEntityDef().GetRarity();
				choice.m_actor.ActivateSpellBirthState(GetSpellTypeForRarity(rarity));
			}
			switch (rarity)
			{
			case TAG_RARITY.COMMON:
			case TAG_RARITY.FREE:
				SoundManager.Get().LoadAndPlay("forge_normal_card_appears.prefab:3e1223a4e6503f2469fb0090db8da67e");
				break;
			case TAG_RARITY.RARE:
			case TAG_RARITY.EPIC:
			case TAG_RARITY.LEGENDARY:
				SoundManager.Get().LoadAndPlay("forge_rarity_card_appears.prefab:4ecbc5de846e50746986849690c01e6a");
				break;
			}
			if (isHeroSkin)
			{
				if (i == 0 && DemoMgr.Get().ArenaIs1WinMode())
				{
					DemoMgr.Get().CreateDemoText(GameStrings.Get("GLUE_BLIZZCON2013_ARENA"), unclickable: false, shouldDoArenaInstruction: true);
				}
				choice.m_actor.GetHealthObject().Hide();
				GameObject newHeroLabelObj = UnityEngine.Object.Instantiate(m_heroLabel);
				newHeroLabelObj.transform.position = choice.m_actor.GetMeshRenderer().transform.position;
				HeroLabel newLabel = newHeroLabelObj.GetComponent<HeroLabel>();
				if ((bool)UniversalInputManager.UsePhoneUI)
				{
					choice.m_actor.transform.localScale = HERO_ACTOR_LOCAL_SCALE_PHONE;
					newHeroLabelObj.transform.localScale = HERO_LABEL_SCALE_PHONE;
				}
				else
				{
					choice.m_actor.transform.localScale = HERO_ACTOR_LOCAL_SCALE;
					newHeroLabelObj.transform.localScale = HERO_LABEL_SCALE;
				}
				Color heroLabelTextColorOverride = Color.white;
				if (m_draftManager.GetDraftPaperTextColorOverride(ref heroLabelTextColorOverride))
				{
					newLabel.SetColor(heroLabelTextColorOverride);
				}
				newHeroLabelObj.transform.SetParent(base.transform, worldPositionStays: true);
				newLabel.UpdateText(choice.m_actor.GetEntityDef().GetName(), GameStrings.GetClassName(choice.m_actor.GetEntityDef().GetClass()).ToUpper());
				m_currentLabels.Add(newLabel);
			}
			else if (isHeroPower)
			{
				heroCopyActor.GetHealthObject().Hide();
				GameObject newHeroLabelObj2 = UnityEngine.Object.Instantiate(m_heroLabel);
				newHeroLabelObj2.transform.position = heroCopyActor.GetMeshRenderer().transform.position;
				HeroLabel newLabel2 = newHeroLabelObj2.GetComponent<HeroLabel>();
				newLabel2.m_nameText.Hide();
				newLabel2.m_classText.Hide();
				heroCopyActor.GetSpell(SpellType.SUMMON_IN_FORGE).AddSpellEventCallback(delegate(string eventName, object eventData, object userData)
				{
					if (eventName == SummonInForge.ACTOR_VISIBLE_EVENT)
					{
						heroPowerActor.Show();
						newLabel2.m_classText.Show();
					}
				});
				if ((bool)UniversalInputManager.UsePhoneUI)
				{
					heroCopyActor.transform.localScale = HERO_ACTOR_LOCAL_SCALE_PHONE;
					newHeroLabelObj2.transform.localScale = HERO_LABEL_SCALE_PHONE;
				}
				else
				{
					heroCopyActor.transform.localScale = HERO_ACTOR_LOCAL_SCALE;
					newHeroLabelObj2.transform.localScale = HERO_LABEL_SCALE;
				}
				Color heroLabelTextColorOverride2 = Color.white;
				if (m_draftManager.GetDraftPaperTextColorOverride(ref heroLabelTextColorOverride2))
				{
					newLabel2.SetColor(heroLabelTextColorOverride2);
				}
				newHeroLabelObj2.transform.SetParent(base.transform, worldPositionStays: true);
				string doubleHeroName = GameStrings.GetClassName(heroCopyActor.GetEntityDef().GetClass()).ToUpper() + "-" + GameStrings.GetClassName(choice.m_actor.GetEntityDef().GetClass()).ToUpper();
				newLabel2.UpdateText(m_chosenHero.GetEntityDef().GetName(), doubleHeroName);
				newLabel2.m_classText.CharacterSize = 5f;
				m_currentLabels.Add(newLabel2);
			}
			else if ((bool)UniversalInputManager.UsePhoneUI)
			{
				choice.m_actor.transform.localScale = CHOICE_ACTOR_LOCAL_SCALE_PHONE;
			}
			else
			{
				choice.m_actor.transform.localScale = CHOICE_ACTOR_LOCAL_SCALE;
			}
		}
		EnableBackButton(buttonEnabled: true);
		StartCoroutine(RunAutoDraftCheat());
		m_pickArea.enabled = false;
	}

	private bool CanAutoDraft()
	{
		if (!HearthstoneApplication.IsInternal())
		{
			return false;
		}
		if (!Vars.Key("Arena.AutoDraft").GetBool(def: false))
		{
			return false;
		}
		return true;
	}

	public IEnumerator RunAutoDraftCheat()
	{
		if (!CanAutoDraft())
		{
			yield break;
		}
		int frameStart = Time.frameCount;
		while (GameUtils.IsAnyTransitionActive() && Time.frameCount - frameStart < 120)
		{
			yield return null;
		}
		List<DraftCardVisual> draftChoices = GetCardVisuals();
		if (draftChoices != null && draftChoices.Count > 0)
		{
			int pickedIndex = UnityEngine.Random.Range(0, draftChoices.Count - 1);
			DraftCardVisual visual = draftChoices[pickedIndex];
			frameStart = Time.frameCount;
			while (visual.GetActor() == null && Time.frameCount - frameStart < 120)
			{
				yield return null;
			}
			if (visual.GetActor() != null)
			{
				string info = $"autodraft'ing {visual.GetActor().GetEntityDef().GetName()}\nto stop, use cmd 'autodraft off'";
				UIStatus.Get().AddInfo(info, 2f);
				draftChoices[pickedIndex].ChooseThisCard();
			}
		}
	}

	private Vector3 GetCardPosition(int cardChoice, bool isHeroSkin)
	{
		float num = m_pickArea.bounds.center.x - m_pickArea.bounds.extents.x;
		float spaceBetweenChoices = m_pickArea.bounds.size.x / 3f;
		float offset = ((m_choices.Count == 2) ? 0f : ((0f - spaceBetweenChoices) / 2f));
		float zOffset = 0f;
		if (isHeroSkin)
		{
			zOffset = 1f;
		}
		return new Vector3(num + (float)(cardChoice + 1) * spaceBetweenChoices + offset, m_pickArea.transform.position.y, m_pickArea.transform.position.z + zOffset);
	}

	public static SpellType GetSpellTypeForRarity(TAG_RARITY rarity)
	{
		return rarity switch
		{
			TAG_RARITY.RARE => SpellType.BURST_RARE, 
			TAG_RARITY.EPIC => SpellType.BURST_EPIC, 
			TAG_RARITY.LEGENDARY => SpellType.BURST_LEGENDARY, 
			_ => SpellType.BURST_COMMON, 
		};
	}

	private void OnHeroActorLoaded(Actor actor)
	{
		actor.transform.SetParent(base.transform, worldPositionStays: true);
		m_chosenHero = actor;
		m_chosenHero.transform.parent = m_socketHeroBone;
		m_chosenHero.transform.localPosition = Vector3.zero;
		m_chosenHero.transform.localScale = Vector3.one;
		m_chosenHero.transform.localRotation = Quaternion.identity;
	}

	private void OnHeroPowerActorLoaded(Actor actor)
	{
		actor.transform.SetParent(base.transform, worldPositionStays: true);
		m_inPlayHeroPowerActor = actor;
		SetupToDisplayHeroPowerTooltip(m_inPlayHeroPowerActor);
		m_inPlayHeroPowerActor.transform.parent = m_socketHeroPowerBone;
		m_inPlayHeroPowerActor.transform.localPosition = Vector3.zero;
		m_inPlayHeroPowerActor.transform.localScale = Vector3.one;
		m_inPlayHeroPowerActor.transform.localRotation = Quaternion.identity;
	}

	private void OnMouseOverHeroPower(UIEvent uiEvent)
	{
		if (m_inPlayHeroPowerActor != null)
		{
			ShowHeroPower(m_inPlayHeroPowerActor);
		}
	}

	private void OnMouseOutHeroPower(UIEvent uiEvent)
	{
		if (m_heroPower != null)
		{
			m_heroPower.Hide();
		}
	}

	private void OnMythicHeroPowerLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogWarning($"DraftDisplay.OnActorLoaded() - FAILED to load actor \"{assetRef}\"");
			return;
		}
		go.transform.SetParent(base.transform, worldPositionStays: true);
		Actor actor = go.GetComponent<Actor>();
		if (actor == null)
		{
			Debug.LogWarning($"DraftDisplay.OnActorLoaded() - ERROR actor \"{assetRef}\" has no Actor component");
			return;
		}
		ChoiceCallback callbackInfo = (ChoiceCallback)callbackData;
		m_mythicPowerCardActors[callbackInfo.choiceID - 1] = actor;
	}

	private void OnActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		ChoiceCallback callbackInfo = (ChoiceCallback)callbackData;
		DefLoader.DisposableFullDef fullDef = callbackInfo?.fullDef;
		try
		{
			if (go == null)
			{
				Debug.LogWarning($"DraftDisplay.OnActorLoaded() - FAILED to load actor \"{assetRef}\"");
				return;
			}
			go.transform.SetParent(base.transform, worldPositionStays: true);
			Actor actor = go.GetComponent<Actor>();
			if (actor == null)
			{
				Debug.LogWarning($"DraftDisplay.OnActorLoaded() - ERROR actor \"{assetRef}\" has no Actor component");
				return;
			}
			DraftChoice choice = m_choices.Find((DraftChoice obj) => obj.m_cardID != null && obj.m_cardID.Equals(fullDef.EntityDef.GetCardId()));
			if (choice == null)
			{
				Debug.LogWarningFormat("DraftDisplay.OnActorLoaded(): Could not find draft choice {0} (cardID = {1}) in m_choices.", fullDef.EntityDef.GetName(), fullDef.EntityDef.GetCardId());
				UnityEngine.Object.Destroy(go);
				return;
			}
			choice.m_actor = actor;
			choice.m_actor.SetPremium(choice.m_premium);
			choice.m_actor.SetEntityDef(fullDef.EntityDef);
			choice.m_actor.SetCardDef(fullDef.DisposableCardDef);
			choice.m_actor.UpdateAllComponents();
			choice.m_actor.gameObject.name = fullDef.CardDef.name + "_actor";
			choice.m_actor.ContactShadow(visible: true);
			if (choice.m_actor.GetEntityDef().IsHeroPower())
			{
				m_heroPowerCardActors[callbackInfo.choiceID - 1] = choice.m_actor;
				if (HaveActorsForAllChoices() && HaveAllSubclassHeroPowerDefs())
				{
					PositionAndShowChoices();
				}
				else
				{
					choice.m_actor.Hide();
				}
				return;
			}
			Collider choiceCollider = choice.m_actor.GetCollider();
			if (choiceCollider != null)
			{
				DraftCardVisual draftCardVisual = choiceCollider.gameObject.AddComponent<DraftCardVisual>();
				draftCardVisual.SetActor(choice.m_actor);
				draftCardVisual.SetChoiceNum(callbackInfo.choiceID);
			}
			if (HaveActorsForAllChoices())
			{
				PositionAndShowChoices();
			}
			else
			{
				choice.m_actor.Hide();
			}
		}
		finally
		{
			if (fullDef != null)
			{
				((IDisposable)fullDef).Dispose();
			}
		}
	}

	private void OnSubClassActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		ChoiceCallback callbackInfo = (ChoiceCallback)callbackData;
		using DefLoader.DisposableFullDef fullDef = callbackInfo.fullDef;
		if (go == null)
		{
			Debug.LogWarning($"DraftDisplay.OnDualClassActorLoaded() - FAILED to load actor \"{assetRef}\"");
			return;
		}
		go.transform.SetParent(base.transform, worldPositionStays: true);
		Actor actor = go.GetComponent<Actor>();
		if (actor == null)
		{
			Debug.LogWarning($"DraftDisplay.OnDualClassActorLoaded() - ERROR actor \"{assetRef}\" has no Actor component");
			return;
		}
		m_subClassHeroPowerDefs[callbackInfo.choiceID - 1]?.Dispose();
		m_subClassHeroPowerDefs[callbackInfo.choiceID - 1] = fullDef.Share();
		m_subclassHeroPowerActors[callbackInfo.choiceID - 1] = actor;
		if (HaveActorsForAllChoices() && HaveAllSubclassHeroPowerDefs())
		{
			PositionAndShowChoices();
		}
	}

	private void OnCardDefLoaded(string cardId, DefLoader.DisposableCardDef def, object callbackData)
	{
		using (def)
		{
			if (def == null)
			{
				return;
			}
			foreach (EmoteEntryDef emoteDef in def?.CardDef.m_EmoteDefs)
			{
				if (emoteDef.m_emoteType == EmoteType.PICKED)
				{
					AssetLoader.Get().InstantiatePrefab(emoteDef.m_emoteSoundSpellPath, OnStartEmoteLoaded, callbackData);
				}
			}
		}
	}

	private void OnStartEmoteLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		CardSoundSpell spell = null;
		if (go != null)
		{
			spell = go.GetComponent<CardSoundSpell>();
			go.transform.SetParent(base.transform, worldPositionStays: true);
		}
		m_skipHeroEmotes |= spell == null;
		if (m_skipHeroEmotes)
		{
			UnityEngine.Object.Destroy(go);
			return;
		}
		int choiceID = (int)callbackData;
		m_heroEmotes[choiceID - 1] = spell;
	}

	private bool HaveActorsForAllChoices()
	{
		foreach (DraftChoice choice in m_choices)
		{
			if (!string.IsNullOrEmpty(choice.m_cardID) && choice.m_actor == null)
			{
				return false;
			}
		}
		return true;
	}

	private bool HaveAllSubclassHeroPowerDefs()
	{
		DefLoader.DisposableFullDef[] subClassHeroPowerDefs = m_subClassHeroPowerDefs;
		for (int i = 0; i < subClassHeroPowerDefs.Length; i++)
		{
			if (subClassHeroPowerDefs[i] == null)
			{
				return false;
			}
		}
		return true;
	}

	private void InitManaCurve()
	{
		CollectionDeck draftDeck = m_draftManager.GetDraftDeck();
		if (draftDeck == null)
		{
			return;
		}
		foreach (CollectionDeckSlot slot in draftDeck.GetSlots())
		{
			EntityDef entityDef = DefLoader.Get().GetEntityDef(slot.CardID);
			for (int i = 0; i < slot.Count; i++)
			{
				AddCardToManaCurve(entityDef);
			}
		}
	}

	private void OnStoreBackButtonPressed(bool authorizationBackButtonPressed, object userData)
	{
		ExitDraftScene();
	}

	private bool OnNavigateBack()
	{
		if (IsInHeroSelectMode())
		{
			DoHeroCancelAnimation();
			return false;
		}
		if (ArenaTrayDisplay.Get() == null)
		{
			return false;
		}
		ArenaTrayDisplay.Get().KeyFXCancel();
		ExitDraftScene();
		return true;
	}

	private void BackButtonPress(UIEvent e)
	{
		Navigation.GoBack();
	}

	private void ExitDraftScene()
	{
		GameMgr.Get().CancelFindGame();
		if (!UniversalInputManager.UsePhoneUI)
		{
			m_playButton.Disable();
		}
		ShowPhonePlayButton(show: false);
		StoreManager.Get().HideStore(ShopType.ARENA_STORE);
		SceneMgr.Get().SetNextMode(SceneMgr.Mode.GAME_MODE, SceneMgr.TransitionHandlerType.NEXT_SCENE);
		Box.Get().SetToIgnoreFullScreenEffects(ignoreEffects: false);
	}

	private void PlayButtonPress(UIEvent e)
	{
		if (!SetRotationManager.Get().CheckForSetRotationRollover() && (PlayerMigrationManager.Get() == null || !PlayerMigrationManager.Get().CheckForPlayerMigrationRequired()))
		{
			if (!UniversalInputManager.UsePhoneUI)
			{
				m_playButton.Disable();
			}
			ShowPhonePlayButton(show: false);
			m_draftManager.FindGame();
			PresenceMgr.Get().SetStatus(Global.PresenceStatus.ARENA_QUEUE);
		}
	}

	private void RetireButtonPress(UIEvent e)
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_FORGE_RETIRE_WARNING_HEADER");
		info.m_text = GameStrings.Get("GLUE_FORGE_RETIRE_WARNING_DESC");
		info.m_showAlertIcon = false;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
		info.m_responseCallback = OnRetirePopupResponse;
		DialogManager.Get().ShowPopup(info);
	}

	private void OnFreeArenaWinOkButtonPress(DialogBase dialog, object userData)
	{
		Options.Get().SetBool(Option.HAS_SEEN_FREE_ARENA_WIN_DIALOG_THIS_DRAFT, val: true);
		ShowCurrentlyDraftingScreen();
	}

	private void OnOutstandingTicketEnterButtonPress()
	{
		m_draftManager.RequestDraftBegin();
		Options.Get().SetBool(Option.HAS_SEEN_FORGE, val: true);
	}

	private void OnOutstandingTicketCancelButtonPress()
	{
		ExitDraftScene();
	}

	private void OnRetirePopupResponse(AlertPopup.Response response, object userData)
	{
		if (response != AlertPopup.Response.CANCEL)
		{
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				m_draftDeckTray.gameObject.GetComponent<SlidingTray>().HideTray();
			}
			m_retireButton.Disable();
			EnableBackButton(buttonEnabled: false);
			Network.Get().DraftRetire(m_draftManager.GetDraftDeck().ID, m_draftManager.GetSlot(), m_draftManager.CurrentSeasonId);
		}
	}

	private void ManaCurveOver(UIEvent e)
	{
		m_manaCurve.GetComponent<TooltipZone>().ShowTooltip(GameStrings.Get("GLUE_FORGE_MANATIP_HEADER"), GameStrings.Get("GLUE_FORGE_MANATIP_DESC"), TooltipPanel.FORGE_SCALE);
	}

	private void ManaCurveOut(UIEvent e)
	{
		m_manaCurve.GetComponent<TooltipZone>().HideTooltip();
	}

	private void DeckHeaderOver(UIEvent e)
	{
		m_draftDeckTray.GetTooltipZone().ShowTooltip(GameStrings.Get("GLUE_ARENA_DECK_TOOLTIP_HEADER"), GameStrings.Get("GLUE_ARENA_DECK_TOOLTIP"), TooltipPanel.FORGE_SCALE);
	}

	private void DeckHeaderOut(UIEvent e)
	{
		m_draftDeckTray.GetTooltipZone().HideTooltip();
	}

	private void SetupBackButton()
	{
		if (DemoMgr.Get().CantExitArena())
		{
			m_backButton.SetText("");
			return;
		}
		m_backButton.SetText(GameStrings.Get("GLOBAL_BACK"));
		m_backButton.AddEventListener(UIEventType.RELEASE, BackButtonPress);
	}

	private void EnableBackButton(bool buttonEnabled)
	{
		if (buttonEnabled != m_backButton.IsEnabled())
		{
			m_backButton.Flip(buttonEnabled);
		}
		m_backButton.SetEnabled(buttonEnabled);
		if (m_PhoneBackButtonBone != null)
		{
			m_PhoneBackButtonBone.gameObject.SetActive(buttonEnabled);
		}
	}

	private void SetupRetireButton()
	{
		if (DemoMgr.Get().CantExitArena())
		{
			m_retireButton.SetText("");
			return;
		}
		m_retireButton.SetText(GameStrings.Get("GLUE_DRAFT_RETIRE_BUTTON"));
		m_retireButton.AddEventListener(UIEventType.RELEASE, RetireButtonPress);
	}

	private void ShowPhonePlayButton(bool show)
	{
		if (!(m_PhonePlayButtonTray == null))
		{
			SlidingTray tray = m_PhonePlayButtonTray.GetComponent<SlidingTray>();
			if (!(tray == null))
			{
				tray.ToggleTraySlider(show);
			}
		}
	}

	private void OnScenePreUnload(SceneMgr.Mode prevMode, PegasusScene prevScene, object userData)
	{
		if (prevMode == SceneMgr.Mode.DRAFT)
		{
			StoreManager.Get().HideStore(ShopType.ARENA_STORE);
			DialogManager.Get().RemoveUniquePopupRequestFromQueue("arena_first_time");
			if (m_firstTimeDialog != null)
			{
				m_firstTimeDialog.Hide();
			}
			if (IsInHeroSelectMode())
			{
				m_zoomedHero.gameObject.SetActive(value: false);
				m_heroPower.gameObject.SetActive(value: false);
				m_confirmButton.gameObject.SetActive(value: false);
				UniversalInputManager.Get().SetGameDialogActive(active: false);
			}
		}
	}
}
