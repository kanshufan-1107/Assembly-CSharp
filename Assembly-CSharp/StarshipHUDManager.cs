using System.Collections.Generic;
using UnityEngine;

public class StarshipHUDManager : MonoBehaviour
{
	[SerializeField]
	private PlayButton m_launchButton;

	[SerializeField]
	private PlayButton m_abortLaunchButton;

	[SerializeField]
	private UIBButton m_pageLeftButton;

	[SerializeField]
	private UIBButton m_pageRightButton;

	[SerializeField]
	private GameObject m_buttonDashboard;

	[SerializeField]
	private Transform m_showThreeStarshipPiecesBone;

	[SerializeField]
	private Transform m_showTwoStarshipPiecesBone;

	[SerializeField]
	private Transform m_showOneStarshipPieceBone;

	[SerializeField]
	private float m_cardShowScale = 1f;

	[SerializeField]
	private float m_adjacentCardXOffset = 0.75f;

	[Header("Launch Button Inactive State Refrences")]
	[SerializeField]
	private GameObject m_launchButtonHighlight;

	[SerializeField]
	private GameObject m_launchButtonInactiveMesh;

	[SerializeField]
	private GameObject m_launchButtonActiveMesh;

	[SerializeField]
	private GameObject m_manaGem;

	[SerializeField]
	private UberText m_manaText;

	[SerializeField]
	[Header("HUD Mobile Only Variables")]
	private Vector3 m_showThreeStarshipPiecesBoneOffsetMobile;

	[SerializeField]
	private Vector3 m_showTwoStarshipPiecesBoneOffsetMobile;

	[SerializeField]
	private Vector3 m_showOneStarshipPieceBoneOffsetMobile;

	[SerializeField]
	private Vector3 m_dashboardPositionOffsetMobile;

	[SerializeField]
	private Vector3 m_launchButtonPositionOffsetMobile;

	[SerializeField]
	private Vector3 m_abortLaunchButtonPositionOffsetMobile;

	[SerializeField]
	private Vector3 m_pageLeftButtonPositionOffsetMobile;

	[SerializeField]
	private Vector3 m_pageRightButtonPositionOffsetMobile;

	[SerializeField]
	private Vector3 m_dashboardScaleMobile = Vector3.one;

	[SerializeField]
	private Vector3 m_launchButtonScaleMobile = Vector3.one;

	[SerializeField]
	private Vector3 m_abortLaunchButtonScaleMobile = Vector3.one;

	[SerializeField]
	private Vector3 m_pageLeftButtonScaleMobile = Vector3.one;

	[SerializeField]
	private Vector3 m_pageRightButtonScaleMobile = Vector3.one;

	[SerializeField]
	private float m_cardShowScaleMobile = 1f;

	[SerializeField]
	private float m_adjacentCardXOffsetMobile = 0.75f;

	[Header("HUD Animation References")]
	[SerializeField]
	private Spell m_OpenCloseHudSpell;

	[SerializeField]
	private Spell m_RevealCardsHUDSpell;

	private List<Actor> m_starshipPiecesToShow;

	private int m_pageCurrentIndex;

	private float m_cardScaleOnShow;

	private float m_adjacentCardOffset;

	private static string m_StarshipHUDPrefabRef = "StarshipHUD.prefab:818950bfabf5b4d47991c0924df4b1f0";

	private static StarshipHUDManager m_instance;

	private const int PAGE_STARTING_INDEX = 0;

	private static readonly Vector3 INVISIBLE_SCALE = new Vector3(0.0001f, 0.0001f, 0.0001f);

	private const int MAX_CARDS_PER_PAGE = 3;

	private bool m_isBiengDestroyed = true;

	public static StarshipHUDManager Get()
	{
		if (m_instance == null)
		{
			GameObject StarshipHUDObject = AssetLoader.Get().InstantiatePrefab(m_StarshipHUDPrefabRef, AssetLoadingOptions.IgnorePrefabPosition);
			if (StarshipHUDObject != null)
			{
				m_instance = StarshipHUDObject.GetComponent<StarshipHUDManager>();
			}
		}
		return m_instance;
	}

	private void Awake()
	{
		SetupPageLeftRightButtons();
		m_pageCurrentIndex = 0;
		m_starshipPiecesToShow = new List<Actor>();
		m_cardScaleOnShow = m_cardShowScale;
		m_adjacentCardOffset = m_adjacentCardXOffset;
		UpdateValuesForMobile();
	}

	public bool IsWaitingOnDestroy()
	{
		return m_isBiengDestroyed;
	}

	private void UpdateValuesForMobile()
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			if (m_launchButton != null)
			{
				m_launchButton.transform.position += m_launchButtonPositionOffsetMobile;
				m_launchButton.transform.localScale = m_launchButtonScaleMobile;
			}
			if (m_abortLaunchButton != null)
			{
				m_abortLaunchButton.transform.position += m_abortLaunchButtonPositionOffsetMobile;
				m_abortLaunchButton.transform.localScale = m_abortLaunchButtonScaleMobile;
			}
			if (m_buttonDashboard != null)
			{
				m_buttonDashboard.transform.position += m_dashboardPositionOffsetMobile;
				m_buttonDashboard.transform.localScale = m_dashboardScaleMobile;
			}
			if (m_pageLeftButton != null)
			{
				m_pageLeftButton.transform.position += m_pageLeftButtonPositionOffsetMobile;
				m_pageLeftButton.transform.localScale = m_pageLeftButtonScaleMobile;
			}
			if (m_pageRightButton != null)
			{
				m_pageRightButton.transform.position += m_pageRightButtonPositionOffsetMobile;
				m_pageRightButton.transform.localScale = m_pageRightButtonScaleMobile;
			}
			if (m_showThreeStarshipPiecesBone != null)
			{
				m_showThreeStarshipPiecesBone.transform.position += m_showThreeStarshipPiecesBoneOffsetMobile;
			}
			if (m_showTwoStarshipPiecesBone != null)
			{
				m_showTwoStarshipPiecesBone.transform.position += m_showTwoStarshipPiecesBoneOffsetMobile;
			}
			if (m_showOneStarshipPieceBone != null)
			{
				m_showOneStarshipPieceBone.transform.position += m_showOneStarshipPieceBoneOffsetMobile;
			}
			m_cardScaleOnShow = m_cardShowScaleMobile;
			m_adjacentCardOffset = m_adjacentCardXOffsetMobile;
		}
	}

	private void OnDisable()
	{
		if (m_pageLeftButton != null)
		{
			m_pageLeftButton.ClearEventListeners();
		}
		if (m_pageRightButton != null)
		{
			m_pageRightButton.ClearEventListeners();
		}
		if (m_launchButton != null)
		{
			m_launchButton.ClearEventListeners();
		}
		if (m_launchButton != null)
		{
			m_launchButton.ClearEventListeners();
		}
		m_pageCurrentIndex = 0;
		DisableLaunchAndAbortButtons();
	}

	public void SetupLaunchAndAbortButtons(UIEvent.Handler OnLaunchReleaseHandler, Player player)
	{
		if (m_buttonDashboard == null)
		{
			Debug.LogError("StarshipHUDManager.SetupLaunchAndAbortButtons() - m_buttonDashboard prefab not assigned");
			return;
		}
		m_buttonDashboard.SetActive(value: true);
		if (m_launchButton == null)
		{
			Debug.LogError("StarshipHUDManager.SetupLaunchAndAbortButtons() - launchButton prefab not assigned");
			return;
		}
		m_launchButton.gameObject.SetActive(value: true);
		UpdateLaunchButton(player);
		m_launchButton.AddEventListener(UIEventType.RELEASE, OnLaunchReleaseHandler);
		if (m_abortLaunchButton == null)
		{
			Debug.LogError("StarshipHUDManager.SetupLaunchAndAbortButtons() - abortLaunchButton prefab not assigned");
			return;
		}
		m_abortLaunchButton.gameObject.SetActive(value: true);
		m_abortLaunchButton.AddEventListener(UIEventType.RELEASE, delegate
		{
			OnAbortLaunchClicked();
		});
	}

	public void UpdateLaunchButton(Player player)
	{
		if (player != null)
		{
			int launchCost = GameUtils.StarshipLaunchCost(player);
			if (player.GetNumAvailableResources() < launchCost)
			{
				m_launchButton.SetText(GameStrings.Get("GAMEPLAY_STARSHIP_HUD_ENOUGH_MANA"));
				SetLaunchButtonState(active: false);
			}
			else
			{
				m_launchButton.SetText(GameStrings.Get("GAMEPLAY_STARSHIP_HUD_CONFIRM"));
				SetLaunchButtonState(active: true);
			}
		}
		if (m_manaText != null)
		{
			int cost = GameUtils.StarshipLaunchCost(player);
			int cardTagValue = GameUtils.GetCardTagValue(GameUtils.STARSHIP_LAUNCH_CARD_ID, GAME_TAG.COST);
			m_manaText.Text = Mathf.Max(0, cost).ToString();
			if (cardTagValue > cost)
			{
				m_manaText.TextColor = Color.green;
			}
		}
	}

	public void SetupButtonsOpponentStarship()
	{
		if (m_abortLaunchButton == null)
		{
			Debug.LogError("StarshipHUDManager.SetupLaunchAndAbortButtons() - abortLaunchButton prefab not assigned");
			return;
		}
		m_abortLaunchButton.gameObject.SetActive(value: true);
		m_abortLaunchButton.AddEventListener(UIEventType.RELEASE, delegate
		{
			OnAbortLaunchClicked();
		});
		if (m_launchButton == null)
		{
			Debug.LogError("StarshipHUDManager.SetupLaunchAndAbortButtons() - launchButton prefab not assigned");
			return;
		}
		m_launchButton.SetText("");
		m_launchButton.Disable();
		m_manaGem.SetActive(value: false);
		SetLaunchButtonState(active: false);
	}

	public void SetupSubcards(List<int> subcardEntityIds)
	{
		if (subcardEntityIds == null)
		{
			Log.Gameplay.PrintError("StarshipHUDManager: list is null");
			return;
		}
		if (subcardEntityIds.Count == 0)
		{
			Log.Gameplay.PrintError("StarshipHUDManager: no subcards in list");
			return;
		}
		GameState gamestate = GameState.Get();
		if (gamestate == null)
		{
			Debug.LogError("StarshipHUDManager: gamestate was null!");
			return;
		}
		for (int i = 0; i < subcardEntityIds.Count; i++)
		{
			Entity potentialStarshipPieceEnt = gamestate.GetEntity(subcardEntityIds[i]);
			Card card = potentialStarshipPieceEnt.GetCard();
			if (potentialStarshipPieceEnt != null && potentialStarshipPieceEnt.HasTag(GAME_TAG.STARSHIP_PIECE) && !card.GetActor())
			{
				TAG_PREMIUM premium = (TAG_PREMIUM)potentialStarshipPieceEnt.GetTag(GAME_TAG.PREMIUM);
				card.UpdateActor(forceIfNullZone: true, ActorNames.GetHandActorByTags(potentialStarshipPieceEnt, premium));
				card.GetActor().Hide();
			}
		}
		InitializeAndShowStarshipPieces(subcardEntityIds);
	}

	private void InitializeAndShowStarshipPieces(List<int> subcardEntityIds)
	{
		GameState gamestate = GameState.Get();
		if (gamestate == null)
		{
			Debug.LogError("StarshipHUDManager: gamestate was null!");
			return;
		}
		for (int i = 0; i < subcardEntityIds.Count; i++)
		{
			Entity potentialStarshipPieceEnt = gamestate.GetEntity(subcardEntityIds[i]);
			if (potentialStarshipPieceEnt != null && potentialStarshipPieceEnt.HasTag(GAME_TAG.STARSHIP_PIECE))
			{
				Actor cardActor = potentialStarshipPieceEnt.GetCard().GetActor();
				m_starshipPiecesToShow.Add(cardActor);
				cardActor.Hide();
			}
		}
		ShowCards(0);
	}

	private void OnAbortLaunchClicked()
	{
		InputManager inputMgr = InputManager.Get();
		if (inputMgr != null)
		{
			inputMgr.CancelSubOptionMode();
			inputMgr.HidePlayerStarshipUI();
		}
	}

	public void PageLeft(UIEvent e)
	{
		if (m_pageCurrentIndex - 3 >= 0)
		{
			ShowCards(m_pageCurrentIndex - 3);
		}
	}

	public void PageRight(UIEvent e)
	{
		if (m_pageCurrentIndex + 3 <= m_starshipPiecesToShow.Count - 1)
		{
			ShowCards(m_pageCurrentIndex + 3);
		}
	}

	public void DisableLaunchAndAbortButtons()
	{
		DisableLaunchButton();
		if (m_abortLaunchButton != null)
		{
			m_abortLaunchButton.gameObject.SetActive(value: false);
		}
		if (m_buttonDashboard != null)
		{
			m_buttonDashboard.SetActive(value: false);
		}
	}

	public void DisableLaunchButton()
	{
		if (m_launchButton != null)
		{
			m_launchButton.gameObject.SetActive(value: false);
		}
	}

	public void AnimateAndDestroyHUD()
	{
		if (!(m_OpenCloseHudSpell == null))
		{
			DisablePageLeftAndRightButtons();
			DisableLaunchAndAbortButtons();
			HideAllCards();
			m_isBiengDestroyed = true;
			m_OpenCloseHudSpell.AddStateFinishedCallback(OnCloseHUDSpellStateFinished);
			m_OpenCloseHudSpell.ActivateState(SpellStateType.DEATH);
		}
	}

	private void OnCloseHUDSpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (prevStateType == SpellStateType.DEATH)
		{
			m_isBiengDestroyed = false;
			Object.Destroy(base.gameObject);
		}
	}

	private void SetupPageLeftRightButtons()
	{
		if (m_pageLeftButton == null)
		{
			Debug.LogError("launchButton prefab not assigned");
			return;
		}
		if (m_pageRightButton == null)
		{
			Debug.LogError("abortLaunchButton prefab not assigned");
			return;
		}
		m_pageLeftButton.AddEventListener(UIEventType.RELEASE, PageLeft);
		m_pageRightButton.AddEventListener(UIEventType.RELEASE, PageRight);
		m_pageLeftButton.gameObject.SetActive(value: false);
	}

	private void ShowCards(int startingIndex)
	{
		if (m_showThreeStarshipPiecesBone == null || m_showTwoStarshipPiecesBone == null || m_showOneStarshipPieceBone == null)
		{
			Log.Gameplay.PrintDebug("StarshipHUDManager: one or more bones not specified for showing starship pieces");
			return;
		}
		if (m_OpenCloseHudSpell != null)
		{
			m_OpenCloseHudSpell.ActivateState(SpellStateType.ACTION);
		}
		if (m_RevealCardsHUDSpell != null)
		{
			m_RevealCardsHUDSpell.RemoveAllVisualTargets();
		}
		HideAllCards();
		int numCardsToShow = Mathf.Min(m_starshipPiecesToShow.Count - startingIndex, 3);
		Transform boneToUse = m_showThreeStarshipPiecesBone;
		switch (numCardsToShow)
		{
		case 2:
			boneToUse = m_showTwoStarshipPiecesBone;
			break;
		case 1:
			boneToUse = m_showOneStarshipPieceBone;
			break;
		}
		for (int i = 0; i < 3 && startingIndex + i <= m_starshipPiecesToShow.Count - 1; i++)
		{
			Actor cardActorToShow = m_starshipPiecesToShow[startingIndex + i];
			Transform parentTransform = cardActorToShow.transform.parent;
			if (parentTransform != null)
			{
				parentTransform.position = Vector3.zero;
				parentTransform.localRotation = Quaternion.identity;
				parentTransform.localScale = Vector3.one;
			}
			cardActorToShow.transform.position = boneToUse.position;
			cardActorToShow.transform.localScale = INVISIBLE_SCALE;
			float startingXPos = boneToUse.position.x;
			Vector3 pos = default(Vector3);
			pos.x = startingXPos + (float)i * m_adjacentCardOffset;
			pos.y = boneToUse.position.y;
			pos.z = boneToUse.position.z;
			cardActorToShow.transform.position = pos;
			Vector3 scale = Vector3.one * m_cardScaleOnShow;
			cardActorToShow.transform.localScale = scale;
			m_RevealCardsHUDSpell.AddTarget(cardActorToShow.gameObject);
		}
		if (m_RevealCardsHUDSpell != null)
		{
			m_RevealCardsHUDSpell.ActivateState(SpellStateType.ACTION);
		}
		m_pageCurrentIndex = startingIndex;
		bool shouldShowPageRightButton = startingIndex + 3 < m_starshipPiecesToShow.Count;
		bool shouldShowPageLeftButton = startingIndex - 3 >= 0;
		m_pageRightButton.gameObject.SetActive(shouldShowPageRightButton);
		m_pageLeftButton.gameObject.SetActive(shouldShowPageLeftButton);
	}

	private void OnDestroy()
	{
		if (m_OpenCloseHudSpell != null)
		{
			m_OpenCloseHudSpell.RemoveStateFinishedCallback(OnCloseHUDSpellStateFinished);
		}
		if (m_abortLaunchButton != null)
		{
			m_abortLaunchButton.ClearEventListeners();
		}
		if (m_launchButton != null)
		{
			m_launchButton.ClearEventListeners();
		}
		for (int i = 0; i < m_starshipPiecesToShow.Count; i++)
		{
			m_starshipPiecesToShow[i].Hide();
		}
		m_starshipPiecesToShow.Clear();
		OnAbortLaunchClicked();
	}

	private void HideAllCards()
	{
		for (int i = 0; i < m_starshipPiecesToShow.Count; i++)
		{
			if (m_starshipPiecesToShow[i] != null)
			{
				m_starshipPiecesToShow[i].Hide();
			}
		}
	}

	private void DisablePageLeftAndRightButtons()
	{
		if (m_pageLeftButton != null)
		{
			m_pageLeftButton.gameObject.SetActive(value: false);
		}
		if (m_pageRightButton != null)
		{
			m_pageRightButton.gameObject.SetActive(value: false);
		}
	}

	private void SetLaunchButtonState(bool active)
	{
		m_launchButtonHighlight.SetActive(active);
		m_launchButtonInactiveMesh.SetActive(!active);
		m_launchButtonActiveMesh.SetActive(active);
	}
}
