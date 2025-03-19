using System.Collections;
using System.Collections.Generic;
using PegasusShared;
using UnityEngine;

public class RAFFrame : UIBPopup
{
	private enum Display
	{
		NONE,
		HERO,
		PROGRESS
	}

	private static RAFFrame s_Instance;

	public RAFHeroContentDisplay m_heroDisplay;

	public UIBButton m_recruitFriendsButton;

	public HighlightState m_recruitFriendsButtonGlow;

	public UIBButton m_infoButton;

	public GameObject m_frame;

	public GameObject m_heroFrame;

	public GameObject m_progressFrame;

	public RAFLinkFrame m_linkFrame;

	public RAFInfo m_infoFrame;

	public List<RAFRecruitBar> m_recruitContainerList;

	public GameObject m_recruitCount;

	public UberText m_recruitCountText;

	public List<RAFChest> m_chestList;

	public GameObject m_heroRewardChestTooltip;

	public UberText m_heroRewardChestTooltipText;

	public Transform m_heroRewardChestTooltipHeroBone;

	public Transform m_heroRewardChestTooltipHeroPowerBone;

	public GameObject m_packRewardChestTooltip;

	public UberText m_packRewardChestTooltipText;

	public GameObject m_packRewardContainer;

	public UnopenedPack m_packReward;

	public GameObject m_totalResultLabel;

	public GameObject m_totalResult;

	private RAFChest m_heroChest;

	private Actor m_heroActor;

	private Actor m_heroPowerActor;

	private bool m_showHeroRewardChestTooltip;

	private PegUIElement m_inputBlockerPegUIElement;

	private bool m_isHeroDisplaySetup;

	private bool m_isHeroKeyArtShowing = true;

	private CollectionHeroDef m_collectionHeroDef;

	private MusicPlaylistType m_prevMusicPlaylist;

	private Display m_shownDisplay;

	private ScreenEffectsHandle m_screenEffectsHandle;

	protected override void Awake()
	{
		base.Awake();
		s_Instance = this;
		m_recruitFriendsButton.AddEventListener(UIEventType.RELEASE, OnRecruitFriendsButtonReleased);
		m_infoButton.AddEventListener(UIEventType.RELEASE, OnInfoButtonReleased);
		m_heroDisplay.m_previewToggle.AddEventListener(UIEventType.RELEASE, OnHeroPreviewToggle);
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	protected override void Start()
	{
		base.Start();
		m_heroChest = m_chestList[0];
		m_heroChest.AddEventListener(UIEventType.ROLLOVER, ShowHeroRewardTooltip);
		m_heroChest.AddEventListener(UIEventType.ROLLOUT, HideHeroRewardTooltip);
		for (int i = 1; i < m_chestList.Count; i++)
		{
			m_chestList[i].AddEventListener(UIEventType.ROLLOVER, ShowPackRewardTooltip);
			m_chestList[i].AddEventListener(UIEventType.ROLLOUT, HidePackRewardTooltip);
		}
		m_packReward.SetCount(1);
		if (m_shownDisplay == Display.NONE)
		{
			ShowHeroFrame();
		}
		UpdateRecruitFriendsButtonGlow();
		BnetBar bnetBar = BnetBar.Get();
		if (bnetBar != null)
		{
			bnetBar.OnMenuOpened += OnMenuOpened;
		}
	}

	private void OnDestroy()
	{
		Hide(animate: true);
		BnetBar bnetBar = BnetBar.Get();
		if (bnetBar != null)
		{
			bnetBar.OnMenuOpened -= OnMenuOpened;
		}
		s_Instance = null;
	}

	public static RAFFrame Get()
	{
		return s_Instance;
	}

	public override void Show()
	{
		if (!base.IsShown())
		{
			Navigation.Push(OnNavigateBack);
			ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignetteDesaturatePerspective;
			screenEffectParameters.Time = 0.1f;
			m_screenEffectsHandle.StartEffect(screenEffectParameters);
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				BnetBar.Get().HideCurrencyFrames();
			}
			base.transform.parent = BaseUI.Get().transform;
			Camera camera = CameraUtils.FindFirstByLayer(base.gameObject.layer);
			if (m_inputBlockerPegUIElement != null)
			{
				Object.Destroy(m_inputBlockerPegUIElement.gameObject);
				m_inputBlockerPegUIElement = null;
			}
			GameObject inputBlockerObject = CameraUtils.CreateInputBlocker(camera, "RAFInputBlocker");
			LayerUtils.SetLayer(inputBlockerObject, base.gameObject.layer, null);
			m_inputBlockerPegUIElement = inputBlockerObject.AddComponent<PegUIElement>();
			m_inputBlockerPegUIElement.transform.parent = base.transform;
			m_inputBlockerPegUIElement.transform.localPosition = new Vector3(0f, -1f, 0f);
			m_inputBlockerPegUIElement.AddEventListener(UIEventType.RELEASE, OnInputBlockerRelease);
			base.Show(useOverlayUI: false);
			TransformUtil.SetPosY(this, base.transform.position.y + 100f);
			Options.Get().SetBool(Option.HAS_SEEN_RAF, val: true);
			FriendListFrame friendListFrame = ChatMgr.Get().FriendListFrame;
			if (friendListFrame != null)
			{
				friendListFrame.UpdateRAFButtonGlow();
			}
		}
	}

	protected override void Hide(bool animate)
	{
		if (base.IsShown())
		{
			Navigation.RemoveHandler(OnNavigateBack);
			if (m_linkFrame != null)
			{
				m_linkFrame.Hide();
			}
			if (m_inputBlockerPegUIElement != null)
			{
				Object.Destroy(m_inputBlockerPegUIElement.gameObject);
				m_inputBlockerPegUIElement = null;
			}
			m_screenEffectsHandle.StopEffect();
			m_heroDisplay.ResetPreview();
			m_isHeroKeyArtShowing = true;
			StopHeroMusic();
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				BnetBar.Get().RefreshCurrency();
			}
			base.Hide(animate);
		}
	}

	public void ShowProgressFrame()
	{
		m_heroFrame.SetActive(value: false);
		m_progressFrame.SetActive(value: true);
		if (m_heroActor == null)
		{
			AssetLoader.Get().InstantiatePrefab("Card_Play_Hero.prefab:42cbbd2c4969afb46b3887bb628de19d", OnHeroActorLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
		}
		if (m_heroPowerActor == null)
		{
			AssetLoader.Get().InstantiatePrefab("Card_Play_HeroPower.prefab:a3794839abb947146903a26be13e09af", OnHeroPowerActorLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
		}
		m_shownDisplay = Display.PROGRESS;
	}

	public void ShowHeroFrame()
	{
		if (m_heroDisplay == null)
		{
			Log.RAF.PrintError("Failed to show hero frame as hero display was null!");
			return;
		}
		m_heroFrame.SetActive(value: true);
		if (!m_isHeroDisplaySetup)
		{
			m_heroDisplay.SetKeyArtRenderer(m_heroDisplay.m_renderQuad);
			if (m_heroDisplay.m_renderToTexture != null)
			{
				m_heroDisplay.m_renderToTexture.m_RenderToObject = m_heroDisplay.m_renderArtQuad;
			}
			else
			{
				Log.RAF.PrintError("Hero display render to texture was null! Expect broken display...");
			}
			CardHeroDbfRecord heroDbfRecord = GameDbf.CardHero.GetRecord(17);
			using (DefLoader.DisposableCardDef def = DefLoader.Get().GetCardDef(heroDbfRecord.CardId))
			{
				m_collectionHeroDef = GameUtils.LoadGameObjectWithComponent<CollectionHeroDef>(def.CardDef.m_CollectionHeroDefPath);
			}
			m_heroDisplay.UpdateFrame(heroDbfRecord, 0, m_collectionHeroDef);
			m_isHeroDisplaySetup = true;
		}
		else
		{
			m_heroDisplay.ResetPreview();
		}
		m_progressFrame.SetActive(value: false);
		m_shownDisplay = Display.HERO;
	}

	public void ResetProgressFrame()
	{
		foreach (RAFRecruitBar recruitContainer in m_recruitContainerList)
		{
			recruitContainer.SetLocked(isLocked: true);
		}
		m_recruitCount.SetActive(value: false);
		m_totalResultLabel.SetActive(value: false);
		m_totalResult.SetActive(value: false);
		foreach (RAFChest chest in m_chestList)
		{
			chest.SetOpen(isChestOpen: false);
		}
	}

	public void UpdateRecruitFriendsButtonGlow()
	{
		bool hasSeenRAFRecruitURL = Options.Get().GetBool(Option.HAS_SEEN_RAF_RECRUIT_URL);
		m_recruitFriendsButtonGlow.ChangeState((!hasSeenRAFRecruitURL) ? ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE : ActorStateType.NONE);
	}

	public void SetProgress(int numRecruits)
	{
		ResetProgressFrame();
		for (int recruitIndex = 0; recruitIndex < numRecruits && recruitIndex < 5; recruitIndex++)
		{
			RAFRecruitBar rAFRecruitBar = m_recruitContainerList[recruitIndex];
			rAFRecruitBar.SetLocked(isLocked: false);
			rAFRecruitBar.SetBattleTag("GoodKnight#1234");
			rAFRecruitBar.SetLevel(20);
			m_chestList[recruitIndex].SetOpen(isChestOpen: true);
		}
		if (numRecruits > 5)
		{
			m_recruitCount.gameObject.SetActive(value: true);
			int remainingRecruitCount = numRecruits - 5;
			m_recruitCountText.Text = GameStrings.Format("GLUE_RAF_PROGRESS_FRAME_RECRUIT_COUNT", remainingRecruitCount, remainingRecruitCount);
		}
	}

	public void SetProgressData(uint totalRecruitCount, List<RAFManager.RecruitData> topRecruits)
	{
		ResetProgressFrame();
		if (totalRecruitCount == 0)
		{
			Log.RAF.PrintError("SetProgressData() - totalRecruitCount is 0!");
			ShowHeroFrame();
			return;
		}
		if (topRecruits == null)
		{
			Log.RAF.PrintError("SetProgressData() - topRecruits is NULL!");
			ShowHeroFrame();
			return;
		}
		for (int recruitIndex = 0; recruitIndex < topRecruits.Count; recruitIndex++)
		{
			RAFRecruitBar rAFRecruitBar = m_recruitContainerList[recruitIndex];
			rAFRecruitBar.SetLocked(isLocked: false);
			RAFManager.RecruitData recruitData = topRecruits[recruitIndex];
			string battleTag = ((recruitData.m_recruitBattleTag == null) ? GameStrings.Get("GAMEPLAY_UNKNOWN_OPPONENT_NAME") : recruitData.m_recruitBattleTag);
			int level = (int)recruitData.m_recruit.Progress;
			rAFRecruitBar.SetGameAccountId(recruitData.m_recruit.GameAccountId);
			rAFRecruitBar.SetBattleTag(battleTag);
			rAFRecruitBar.SetLevel(level);
			if (level >= 20)
			{
				m_chestList[recruitIndex].SetOpen(isChestOpen: true);
			}
		}
		if (totalRecruitCount > 5)
		{
			m_recruitCount.gameObject.SetActive(value: true);
			int remainingRecruitCount = (int)(totalRecruitCount - 5);
			m_recruitCountText.Text = GameStrings.Format("GLUE_RAF_PROGRESS_FRAME_RECRUIT_COUNT", remainingRecruitCount, remainingRecruitCount);
		}
	}

	public void UpdateBattleTag(BnetId gameAccountId, string battleTag)
	{
		foreach (RAFRecruitBar recruitBar in m_recruitContainerList)
		{
			if (recruitBar.GetGameAccountId() == gameAccountId)
			{
				recruitBar.SetBattleTag(battleTag);
				break;
			}
		}
	}

	public void ShowLinkFrame(string displayURL, string fullURL)
	{
		Options.Get().SetBool(Option.HAS_SEEN_RAF_RECRUIT_URL, val: true);
		UpdateRecruitFriendsButtonGlow();
		m_linkFrame.SetURL(displayURL, fullURL);
		m_linkFrame.Show();
	}

	private bool OnNavigateBack()
	{
		Hide(animate: true);
		return true;
	}

	private void OnInputBlockerRelease(UIEvent e)
	{
		Hide(animate: true);
	}

	private void OnRecruitFriendsButtonReleased(UIEvent e)
	{
		if (!m_infoFrame.gameObject.activeInHierarchy)
		{
			string recruitURL = RAFManager.Get().GetRecruitDisplayURL();
			if (recruitURL != null)
			{
				string recruitFullURL = RAFManager.Get().GetRecruitFullURL();
				ShowLinkFrame(recruitURL, recruitFullURL);
			}
		}
	}

	private void OnInfoButtonReleased(UIEvent e)
	{
		m_infoFrame.Show();
	}

	private void OnHeroPreviewToggle(UIEvent e)
	{
		m_isHeroKeyArtShowing = !m_isHeroKeyArtShowing;
		if (m_isHeroKeyArtShowing)
		{
			StopHeroMusic();
		}
		else
		{
			PlayHeroMusic();
		}
	}

	private void PlayHeroMusic()
	{
		if (m_collectionHeroDef == null)
		{
			Log.RAF.PrintWarning("RAFFrame.PlayHeroMusic - m_collectionHeroDef is NULL!");
			return;
		}
		MusicPlaylistType heroMusic = m_collectionHeroDef.m_heroPlaylist;
		if (heroMusic != 0)
		{
			m_prevMusicPlaylist = MusicManager.Get().GetCurrentPlaylist();
			MusicManager.Get().StartPlaylist(heroMusic);
		}
	}

	private void StopHeroMusic()
	{
		if (m_prevMusicPlaylist != 0)
		{
			MusicManager.Get().StartPlaylist(m_prevMusicPlaylist);
			m_prevMusicPlaylist = MusicPlaylistType.Invalid;
		}
	}

	private void ShowHeroRewardTooltip(UIEvent e)
	{
		m_showHeroRewardChestTooltip = true;
		m_heroRewardChestTooltipText.Text = GameStrings.Get(m_heroChest.IsOpen() ? "GLUE_RAF_HERO_TOOLTIP_REDEEMED_TITLE" : "GLUE_RAF_HERO_TOOLTIP_TITLE");
		StartCoroutine(ShowHeroRewardTooltipWhenReady());
	}

	private IEnumerator ShowHeroRewardTooltipWhenReady()
	{
		while (m_heroActor == null)
		{
			yield return null;
		}
		if (m_showHeroRewardChestTooltip)
		{
			m_heroRewardChestTooltip.SetActive(value: true);
		}
	}

	private void HideHeroRewardTooltip(UIEvent e)
	{
		m_showHeroRewardChestTooltip = false;
		m_heroRewardChestTooltip.SetActive(value: false);
	}

	private void ShowPackRewardTooltip(UIEvent e)
	{
		RAFChest chest = e.GetElement() as RAFChest;
		m_packRewardChestTooltipText.Text = GameStrings.Get(chest.IsOpen() ? "GLUE_RAF_PACK_TOOLTIP_REDEEMED_TITLE" : "GLUE_RAF_PACK_TOOLTIP_TITLE");
		m_packRewardChestTooltip.transform.position = chest.m_tooltipBone.transform.position;
		m_packRewardChestTooltip.SetActive(value: true);
	}

	private void HidePackRewardTooltip(UIEvent e)
	{
		m_packRewardChestTooltip.SetActive(value: false);
	}

	private void OnHeroActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Log.RAF.PrintWarning($"RAFFrame.OnHeroActorLoaded() - FAILED to load actor \"{assetRef}\"");
			return;
		}
		m_heroActor = go.GetComponent<Actor>();
		if (m_heroActor == null)
		{
			Log.RAF.PrintWarning($"RAFFrame.OnHeroActorLoaded() - ERROR actor \"{assetRef}\" has no Actor component");
			return;
		}
		go.transform.parent = m_heroRewardChestTooltip.transform;
		go.transform.localScale = m_heroRewardChestTooltipHeroBone.localScale;
		go.transform.localPosition = m_heroRewardChestTooltipHeroBone.localPosition;
		m_heroActor.SetUnlit();
		LayerUtils.SetLayer(m_heroActor.gameObject, base.gameObject.layer, null);
		Object.Destroy(m_heroActor.m_healthObject);
		Object.Destroy(m_heroActor.m_attackObject);
		m_heroActor.Hide();
		string cardId = GameUtils.GetCardIdFromHeroDbId(17);
		DefLoader.Get().LoadFullDef(cardId, OnHeroFullDefLoaded);
	}

	private void OnHeroPowerActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogWarning($"RAFFrame.OnHeroActorLoaded() - FAILED to load actor \"{assetRef}\"");
			return;
		}
		m_heroPowerActor = go.GetComponent<Actor>();
		if (m_heroPowerActor == null)
		{
			Debug.LogWarning($"RAFFrame.OnHeroActorLoaded() - ERROR actor \"{assetRef}\" has no Actor component");
			return;
		}
		go.transform.parent = m_heroRewardChestTooltip.transform;
		go.transform.localScale = m_heroRewardChestTooltipHeroPowerBone.localScale;
		go.transform.localPosition = m_heroRewardChestTooltipHeroPowerBone.localPosition;
		m_heroPowerActor.SetUnlit();
		LayerUtils.SetLayer(m_heroPowerActor.gameObject, base.gameObject.layer, null);
		m_heroPowerActor.Hide();
		string heroPowerID = GameUtils.GetHeroPowerCardIdFromHero(GameDbf.CardHero.GetRecord(17).CardId);
		DefLoader.Get().LoadFullDef(heroPowerID, OnHeroPowerFullDefLoaded);
	}

	private void OnHeroFullDefLoaded(string cardId, DefLoader.DisposableFullDef fullDef, object userData)
	{
		using (fullDef)
		{
			m_heroActor.SetPremium(TAG_PREMIUM.GOLDEN);
			m_heroActor.SetEntityDef(fullDef.EntityDef);
			m_heroActor.SetCardDef(fullDef.DisposableCardDef);
			m_heroActor.UpdateAllComponents();
			m_heroActor.SetUnlit();
			m_heroActor.transform.Rotate(new Vector3(-90f, 0f, 0f));
			m_heroActor.Show();
		}
	}

	private void OnHeroPowerFullDefLoaded(string cardId, DefLoader.DisposableFullDef def, object userData)
	{
		using (def)
		{
			m_heroPowerActor.SetCardDef(def.DisposableCardDef);
			m_heroPowerActor.SetEntityDef(def.EntityDef);
			m_heroPowerActor.UpdateAllComponents();
			m_heroPowerActor.SetUnlit();
			def.CardDef.m_AlwaysRenderPremiumPortrait = false;
			m_heroPowerActor.UpdateMaterials();
			m_heroPowerActor.transform.Rotate(new Vector3(-90f, 0f, 0f));
			m_heroPowerActor.Show();
			StartCoroutine(UpdateHeroSkinHeroPower());
		}
	}

	private IEnumerator UpdateHeroSkinHeroPower()
	{
		while (m_heroActor == null)
		{
			yield return null;
		}
		while (!m_heroActor.HasCardDef)
		{
			yield return null;
		}
		HeroSkinHeroPower hshp = m_heroPowerActor.gameObject.GetComponentInChildren<HeroSkinHeroPower>();
		if (!(hshp == null))
		{
			string heroCardId = m_heroActor.GetEntityDef().GetCardId();
			hshp.m_Actor.AlwaysRenderPremiumPortrait = !GameUtils.IsVanillaHero(heroCardId);
			hshp.m_Actor.UpdateMaterials();
		}
	}

	private void OnMenuOpened()
	{
		if (m_shown)
		{
			Hide(animate: false);
		}
	}
}
