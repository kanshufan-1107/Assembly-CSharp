using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CorpseCounter : MonoBehaviour
{
	private static UnityEvent s_initializeEvent;

	private static UnityEvent s_updateEvent;

	private static UnityEvent s_hidePhoneManaTrayEvent;

	private static UnityEvent s_showPhoneManaTrayEvent;

	public UberText m_textbox;

	public GameObject m_symbol;

	public Player.Side m_side;

	private bool m_shown;

	private int m_numOfCorpses;

	private const int NUM_CAP = 99;

	private Vector3 m_startingScale;

	private bool m_initialized;

	private PlayMakerFSM m_FSMComponent;

	static CorpseCounter()
	{
		if (s_initializeEvent == null)
		{
			s_initializeEvent = new UnityEvent();
		}
		if (s_updateEvent == null)
		{
			s_updateEvent = new UnityEvent();
		}
		if (s_hidePhoneManaTrayEvent == null)
		{
			s_hidePhoneManaTrayEvent = new UnityEvent();
		}
		if (s_showPhoneManaTrayEvent == null)
		{
			s_showPhoneManaTrayEvent = new UnityEvent();
		}
	}

	private void OnEnable()
	{
		m_FSMComponent = GetComponent<PlayMakerFSM>();
		s_initializeEvent.AddListener(DelayThenInitialize);
		s_updateEvent.AddListener(UpdateText);
		s_hidePhoneManaTrayEvent.AddListener(PhoneManaTrayHideFX);
		s_showPhoneManaTrayEvent.AddListener(PhoneManaTrayShowFX);
	}

	private void OnDisable()
	{
		m_FSMComponent = null;
		s_initializeEvent.RemoveListener(DelayThenInitialize);
		s_updateEvent.RemoveListener(UpdateText);
		s_hidePhoneManaTrayEvent.RemoveListener(PhoneManaTrayHideFX);
		s_showPhoneManaTrayEvent.RemoveListener(PhoneManaTrayShowFX);
	}

	public static void InitializeAll()
	{
		if (s_initializeEvent != null)
		{
			s_initializeEvent.Invoke();
		}
	}

	public static void UpdateTextAll()
	{
		if (s_updateEvent != null)
		{
			s_updateEvent.Invoke();
		}
	}

	public static void HidePhoneManaTray()
	{
		if (s_hidePhoneManaTrayEvent != null)
		{
			s_hidePhoneManaTrayEvent.Invoke();
		}
	}

	public static void ShowPhoneManaTray()
	{
		if (s_showPhoneManaTrayEvent != null)
		{
			s_showPhoneManaTrayEvent.Invoke();
		}
	}

	public bool IsShown()
	{
		return m_shown;
	}

	private IEnumerator DelayedInitialization()
	{
		yield return new WaitForSeconds(1f);
		Initialize();
	}

	private void DelayThenInitialize()
	{
		StartCoroutine(DelayedInitialization());
	}

	private void Initialize()
	{
		if (!m_initialized)
		{
			m_startingScale = m_textbox.gameObject.transform.localScale;
			m_initialized = true;
			UpdateText();
		}
	}

	private void UpdateText()
	{
		Initialize();
		if (m_textbox == null)
		{
			Debug.LogWarningFormat("UpdateText() is called with no textbox set.");
		}
		else if (m_symbol == null)
		{
			Debug.LogWarningFormat("UpdateText() is called with no symbol set.");
		}
		else if (ShouldShowCorpseCounter())
		{
			int corpses = GetPlayer().GetNumAvailableCorpses();
			if (corpses > m_numOfCorpses)
			{
				CorpseCountIncreaseFX();
			}
			else if (corpses < m_numOfCorpses)
			{
				CorpseCountDecreaseFX();
			}
			if (corpses != m_numOfCorpses)
			{
				Jiggle();
			}
			m_numOfCorpses = corpses;
			m_symbol.SetActive(value: true);
			m_textbox.Text = ((m_numOfCorpses <= 99) ? m_numOfCorpses.ToString() : (99 + "+"));
			m_shown = true;
		}
		else
		{
			m_symbol.SetActive(value: false);
			m_textbox.Text = "";
			m_shown = false;
		}
	}

	private Player GetPlayer()
	{
		if (m_side != Player.Side.FRIENDLY)
		{
			return GameState.Get().GetOpposingSidePlayer();
		}
		return GameState.Get().GetFriendlySidePlayer();
	}

	private bool IsDeathKnightEntity(Entity entity)
	{
		if (entity != null)
		{
			if (entity.HasClass(TAG_CLASS.DEATHKNIGHT))
			{
				return !entity.IsMultiClass();
			}
			return false;
		}
		return false;
	}

	private bool ShouldShowCorpseCounter()
	{
		if (GameMgr.Get() != null && GameMgr.Get().IsBattlegrounds())
		{
			return false;
		}
		Player currentPlayer = GetPlayer();
		if (currentPlayer == null)
		{
			return false;
		}
		if (IsDeathKnightEntity(currentPlayer.GetHero()))
		{
			return true;
		}
		if (IsDeathKnightEntity(currentPlayer.GetHeroPower()))
		{
			return true;
		}
		foreach (Card currentCard in currentPlayer.GetBattlefieldZone().GetCards())
		{
			if (IsDeathKnightEntity(currentCard.GetEntity()))
			{
				return true;
			}
		}
		foreach (Card currentCard2 in currentPlayer.GetSecretZone().GetCards())
		{
			if (IsDeathKnightEntity(currentCard2.GetEntity()))
			{
				return true;
			}
		}
		Card weapon = currentPlayer.GetWeaponCard();
		if (weapon != null && IsDeathKnightEntity(weapon.GetEntity()))
		{
			return true;
		}
		if (m_side == Player.Side.FRIENDLY)
		{
			foreach (Card currentCard3 in currentPlayer.GetHandZone().GetCards())
			{
				if (IsDeathKnightEntity(currentCard3.GetEntity()))
				{
					return true;
				}
			}
			if (ChoiceCardMgr.Get().IsFriendlyShown())
			{
				foreach (Card currentCard4 in ChoiceCardMgr.Get().GetFriendlyCards())
				{
					if (IsDeathKnightEntity(currentCard4.GetEntity()))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private void Jiggle()
	{
		iTween.Stop(m_textbox.gameObject);
		m_textbox.gameObject.transform.localScale = m_startingScale;
		iTween.PunchScale(m_textbox.gameObject, Vector3.one, 1f);
	}

	private void CorpseCountIncreaseFX()
	{
		if (m_FSMComponent != null)
		{
			m_FSMComponent.SendEvent("Increase");
		}
	}

	private void CorpseCountDecreaseFX()
	{
		if (m_FSMComponent != null)
		{
			m_FSMComponent.SendEvent("Decrease");
		}
	}

	private void PhoneManaTrayHideFX()
	{
		if (m_FSMComponent != null)
		{
			m_FSMComponent.SendEvent("HideManaTray");
		}
	}

	private void PhoneManaTrayShowFX()
	{
		if (m_FSMComponent != null)
		{
			m_FSMComponent.SendEvent("ShowManaTray");
		}
	}
}
