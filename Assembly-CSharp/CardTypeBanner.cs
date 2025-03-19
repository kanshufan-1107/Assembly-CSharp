using UnityEngine;

public class CardTypeBanner : MonoBehaviour
{
	public GameObject m_root;

	public UberText m_text;

	public GameObject m_banner;

	private static CardTypeBanner s_instance;

	private Card m_card;

	private readonly Color MINION_COLOR = new Color(13f / 85f, 0.1254902f, 3f / 85f);

	private readonly Color HERO_COLOR = new Color(13f / 85f, 0.1254902f, 3f / 85f);

	private readonly Color SPELL_COLOR = new Color(0.8745098f, 67f / 85f, 0.5254902f);

	private readonly Color REWARD_COLOR = new Color(0.8745098f, 67f / 85f, 0.5254902f);

	private readonly Color BACON_HEROBUDDY_COLOR = new Color(0.8745098f, 67f / 85f, 0.5254902f);

	private readonly Color WEAPON_COLOR = new Color(0.8745098f, 67f / 85f, 0.5254902f);

	private readonly Color LOCATION_COLOR = new Color(0.8745098f, 67f / 85f, 0.5254902f);

	public bool HasCardDef
	{
		get
		{
			if (!(m_card != null))
			{
				return false;
			}
			return m_card.HasCardDef;
		}
	}

	private void Awake()
	{
		s_instance = this;
	}

	private void OnDestroy()
	{
		s_instance = null;
	}

	private void Update()
	{
		if (m_card != null)
		{
			if (m_card.GetActor().IsShown())
			{
				UpdatePosition();
			}
			else
			{
				Hide();
			}
		}
	}

	public static CardTypeBanner Get()
	{
		return s_instance;
	}

	public bool IsShown()
	{
		return m_card;
	}

	public void Show(Card card)
	{
		GameEntityOptions gameOptions = GameState.Get()?.GetGameEntity()?.GetGameOptions();
		if (gameOptions == null || !gameOptions.GetBooleanOption(GameEntityOption.DISABLE_CARD_TYPE_BANNER))
		{
			m_card = card;
			ShowImpl();
		}
	}

	public void Hide()
	{
		m_card = null;
		HideImpl();
	}

	public void Hide(Card card)
	{
		if (m_card == card)
		{
			Hide();
		}
	}

	public DefLoader.DisposableCardDef ShareDisposableCardDef()
	{
		return m_card?.ShareDisposableCardDef();
	}

	private void ShowImpl()
	{
		m_root.gameObject.SetActive(value: true);
		TAG_CARDTYPE cardType = m_card.GetEntity().GetCardType();
		m_text.gameObject.SetActive(value: true);
		m_text.Text = GameStrings.GetCardTypeName(cardType);
		switch (cardType)
		{
		case TAG_CARDTYPE.SPELL:
			m_text.TextColor = SPELL_COLOR;
			break;
		case TAG_CARDTYPE.BATTLEGROUND_SPELL:
			m_text.TextColor = SPELL_COLOR;
			break;
		case TAG_CARDTYPE.MINION:
			m_text.TextColor = MINION_COLOR;
			break;
		case TAG_CARDTYPE.HERO:
			m_text.TextColor = HERO_COLOR;
			break;
		case TAG_CARDTYPE.WEAPON:
			m_text.TextColor = WEAPON_COLOR;
			break;
		case TAG_CARDTYPE.LOCATION:
			m_text.TextColor = LOCATION_COLOR;
			break;
		case TAG_CARDTYPE.BATTLEGROUND_QUEST_REWARD:
			m_text.TextColor = REWARD_COLOR;
			break;
		case TAG_CARDTYPE.BATTLEGROUND_HERO_BUDDY:
			m_text.TextColor = BACON_HEROBUDDY_COLOR;
			break;
		}
		m_banner.SetActive(value: true);
		UpdatePosition();
	}

	private void HideImpl()
	{
		m_root.gameObject.SetActive(value: false);
	}

	private void UpdatePosition()
	{
		GameObject anchor = m_card.GetActor().GetCardTypeBannerAnchor();
		m_root.transform.position = anchor.transform.position;
	}

	public bool HasSameCardDef(CardDef cardDef)
	{
		if (!(m_card != null))
		{
			return false;
		}
		return m_card.HasSameCardDef(cardDef);
	}
}
