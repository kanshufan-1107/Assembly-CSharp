using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class CardListPanel : MonoBehaviour
{
	[CustomEditField(Sections = "Object Links")]
	public NestedPrefab m_leftArrowNested;

	[CustomEditField(Sections = "Object Links")]
	public NestedPrefab m_rightArrowNested;

	[SerializeField]
	private float m_CardSpacing = 2.3f;

	private UIBButton m_leftArrow;

	private UIBButton m_rightArrow;

	private const int MAX_CARDS_PER_PAGE = 3;

	private int m_numPages = 1;

	private int m_pageNum;

	private List<int> m_cards = new List<int>();

	private List<Actor> m_cardActors = new List<Actor>();

	private Dictionary<int, List<CardChangeDbfRecord>> m_changes = new Dictionary<int, List<CardChangeDbfRecord>>();

	[CustomEditField(Sections = "Variables")]
	public float CardSpacing
	{
		get
		{
			return m_CardSpacing;
		}
		set
		{
			m_CardSpacing = value;
			UpdateCardPositions();
		}
	}

	private void Awake()
	{
		m_leftArrowNested.gameObject.SetActive(value: false);
		m_rightArrowNested.gameObject.SetActive(value: false);
	}

	public void Show(List<int> cards, Dictionary<int, List<CardChangeDbfRecord>> changes = null)
	{
		if (cards != null)
		{
			m_cards = cards;
		}
		if (changes != null)
		{
			m_changes = changes;
		}
		SetupPagingArrows();
		m_numPages = (m_cards.Count + 3 - 1) / 3;
		ShowPage(0);
	}

	private void ShowPage(int pageNum)
	{
		if (pageNum < 0 || pageNum >= m_numPages)
		{
			Log.All.PrintWarning("CardListPanel.ShowPage: attempting to show invalid pageNum=" + pageNum + " numPages=" + m_numPages);
			return;
		}
		m_pageNum = pageNum;
		StopCoroutine("TransitionPage");
		StartCoroutine("TransitionPage");
	}

	private IEnumerator TransitionPage()
	{
		if (m_leftArrow != null)
		{
			m_leftArrow.gameObject.SetActive(value: false);
		}
		if (m_rightArrow != null)
		{
			m_rightArrow.gameObject.SetActive(value: false);
		}
		List<Spell> activeSpells = new List<Spell>();
		foreach (Actor cardActor in m_cardActors)
		{
			Object.Destroy(cardActor.gameObject);
		}
		m_cardActors.Clear();
		activeSpells.Clear();
		int startingCardIndex = m_pageNum * 3;
		int cardsToShow = Mathf.Min(3, m_cards.Count - startingCardIndex);
		for (int i = 0; i < cardsToShow; i++)
		{
			int card = m_cards[startingCardIndex + i];
			using DefLoader.DisposableFullDef fullDef = DefLoader.Get().GetFullDef(GameUtils.TranslateDbIdToCardId(card));
			Actor actor = AssetLoader.Get().InstantiatePrefab(ActorNames.GetHandActor(fullDef.EntityDef, TAG_PREMIUM.NORMAL), AssetLoadingOptions.IgnorePrefabPosition).GetComponent<Actor>();
			actor.SetCardDef(fullDef.DisposableCardDef);
			actor.SetEntityDef(fullDef.EntityDef);
			GameUtils.SetParent(actor, base.gameObject);
			LayerUtils.SetLayer(actor, base.gameObject.layer);
			List<CardChangeDbfRecord> cardChanges = m_changes[card];
			GameObject cardChangeGlow = AssetLoader.Get().InstantiatePrefab(ActorNames.GetNerfGlowsActor(fullDef.EntityDef.GetCardType()));
			if (cardChangeGlow != null)
			{
				CardNerfGlows nerfGlows = cardChangeGlow.GetComponent<CardNerfGlows>();
				if (nerfGlows != null)
				{
					TransformUtil.AttachAndPreserveLocalTransform(cardChangeGlow.transform, actor.transform);
					LayerUtils.SetLayer(nerfGlows, actor.gameObject.layer);
					nerfGlows.SetGlowsForCard(cardChanges);
				}
				else
				{
					Debug.LogError("CardListPanel.cs: Nerf Glows GameObject " + cardChangeGlow?.ToString() + " does not have a CardNerfGlows script attached.");
				}
			}
			m_cardActors.Add(actor);
		}
		UpdateCardPositions();
		foreach (Actor actor2 in m_cardActors)
		{
			activeSpells.Add(actor2.ActivateSpellBirthState(SpellType.DEATHREVERSE));
			actor2.ContactShadow(visible: true);
		}
		SoundManager.Get().LoadAndPlay("collection_manager_card_move_invalid_or_click.prefab:777caa6f44f027747a03f3d85bcc897c");
		yield return new WaitForSeconds(0.2f);
		if (m_leftArrow != null)
		{
			m_leftArrow.gameObject.SetActive(m_pageNum != 0);
		}
		if (m_rightArrow != null)
		{
			m_rightArrow.gameObject.SetActive(m_pageNum < m_numPages - 1);
		}
	}

	private void UpdateCardPositions()
	{
		int numCards = m_cardActors.Count;
		for (int i = 0; i < numCards; i++)
		{
			Actor actor = m_cardActors[i];
			Vector3 position = Vector3.zero;
			float offset = ((float)i - (float)(numCards - 1) / 2f) * m_CardSpacing;
			position.x += offset;
			actor.transform.localPosition = position;
		}
	}

	private void SetupPagingArrows()
	{
		if (m_cards.Count > 3)
		{
			m_leftArrowNested.gameObject.SetActive(value: true);
			m_rightArrowNested.gameObject.SetActive(value: true);
			GameObject go = m_leftArrowNested.PrefabGameObject();
			LayerUtils.SetLayer(go, m_leftArrowNested.gameObject.layer, null);
			m_leftArrow = go.GetComponent<UIBButton>();
			m_leftArrow.AddEventListener(UIEventType.RELEASE, delegate
			{
				TurnPage(right: false);
			});
			go = m_rightArrowNested.PrefabGameObject();
			LayerUtils.SetLayer(go, m_rightArrowNested.gameObject.layer, null);
			m_rightArrow = go.GetComponent<UIBButton>();
			m_rightArrow.AddEventListener(UIEventType.RELEASE, delegate
			{
				TurnPage(right: true);
			});
			HighlightState highlight = m_rightArrow.GetComponentInChildren<HighlightState>();
			if ((bool)highlight)
			{
				highlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
			}
		}
		else
		{
			m_leftArrowNested.gameObject.SetActive(value: false);
			m_rightArrowNested.gameObject.SetActive(value: false);
		}
	}

	private void TurnPage(bool right)
	{
		HighlightState highlight = m_rightArrow.GetComponentInChildren<HighlightState>();
		if ((bool)highlight)
		{
			highlight.ChangeState(ActorStateType.NONE);
		}
		ShowPage(m_pageNum + (right ? 1 : (-1)));
	}
}
