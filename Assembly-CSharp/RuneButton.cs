using PegasusShared;
using UnityEngine;

public class RuneButton : UIBButton
{
	private static RuneType[] m_runeOrder = new RuneType[4]
	{
		RuneType.RT_NONE,
		RuneType.RT_BLOOD,
		RuneType.RT_FROST,
		RuneType.RT_UNHOLY
	};

	[CustomEditField(Sections = "Button Objects")]
	public GameObject m_runeBlood;

	[CustomEditField(Sections = "Button Objects")]
	public GameObject m_runeFrost;

	[CustomEditField(Sections = "Button Objects")]
	public GameObject m_runeUnholy;

	[CustomEditField(Sections = "Button Objects")]
	public GameObject m_runeEmpty;

	[CustomEditField(Sections = "Button Objects")]
	public PlayMakerFSM m_runeFSM;

	[CustomEditField(Sections = "Button Objects")]
	public PlayMakerFSM m_runeHighlightFSM;

	[CustomEditField(Sections = "Button Objects")]
	public GameObject m_emptyRune;

	[CustomEditField(Sections = "Button Objects")]
	public GameObject m_emptyRuneHighlight;

	private const string BloodSpawnIn = "Blood_SpawnIn";

	private const string BloodSpawnOut = "Blood_SpawnOut";

	private const string FrostSpawnIn = "Frost_SpawnIn";

	private const string FrostSpawnOut = "Frost_SpawnOut";

	private const string UnholySpawnIn = "Unholy_SpawnIn";

	private const string UnholySpawnOut = "Unholy_SpawnOut";

	private const string HoverOn = "Hover";

	private const string HoverOff = "Hover_Off";

	private int m_runeIndex;

	public RuneType RuneType => (RuneType)m_runeIndex;

	public int ButtonIndex { get; private set; }

	public void SetRune(RuneType runeType, bool animate)
	{
		ShowRune(runeType, animate);
		m_runeIndex = (int)runeType;
	}

	private void ShowRune(RuneType runeType, bool animate)
	{
		switch (runeType)
		{
		case RuneType.RT_BLOOD:
			if (animate)
			{
				m_runeFSM.SendEvent("Blood_SpawnIn");
			}
			else
			{
				m_runeBlood.SetActive(value: true);
				m_runeFrost.SetActive(value: false);
				m_runeUnholy.SetActive(value: false);
			}
			SetEmptyRuneVisible(visible: false);
			break;
		case RuneType.RT_FROST:
			if (animate)
			{
				m_runeFSM.SendEvent("Frost_SpawnIn");
			}
			else
			{
				m_runeBlood.SetActive(value: false);
				m_runeFrost.SetActive(value: true);
				m_runeUnholy.SetActive(value: false);
			}
			SetEmptyRuneVisible(visible: false);
			break;
		case RuneType.RT_UNHOLY:
			if (animate)
			{
				m_runeFSM.SendEvent("Unholy_SpawnIn");
			}
			else
			{
				m_runeBlood.SetActive(value: false);
				m_runeFrost.SetActive(value: false);
				m_runeUnholy.SetActive(value: true);
			}
			SetEmptyRuneVisible(visible: false);
			break;
		case RuneType.RT_NONE:
			HideCurrentRune(animate);
			break;
		}
	}

	private void SetEmptyRuneVisible(bool visible)
	{
		if (visible)
		{
			m_emptyRune.SetActive(value: true);
			return;
		}
		m_emptyRuneHighlight.SetActive(value: false);
		m_emptyRune.SetActive(value: false);
	}

	private void HideCurrentRune(bool animate)
	{
		if (animate)
		{
			switch (RuneType)
			{
			case RuneType.RT_BLOOD:
				m_runeFSM.SendEvent("Blood_SpawnOut");
				break;
			case RuneType.RT_FROST:
				m_runeFSM.SendEvent("Frost_SpawnOut");
				break;
			case RuneType.RT_UNHOLY:
				m_runeFSM.SendEvent("Unholy_SpawnOut");
				break;
			default:
				m_runeBlood.SetActive(value: false);
				m_runeFrost.SetActive(value: false);
				m_runeUnholy.SetActive(value: false);
				break;
			}
		}
		else
		{
			m_runeBlood.SetActive(value: false);
			m_runeFrost.SetActive(value: false);
			m_runeUnholy.SetActive(value: false);
		}
		m_runeHighlightFSM.SendEvent("Hover_Off");
		SetEmptyRuneVisible(visible: true);
	}

	public void ShowNextRune()
	{
		if (m_runeIndex == m_runeOrder.Length - 1)
		{
			HideCurrentRune(animate: true);
			m_runeIndex = 0;
		}
		else
		{
			m_runeIndex++;
			RuneType runeType = (RuneType)m_runeIndex;
			ShowRune(runeType, animate: true);
		}
	}

	public void Initialize(int buttonIndex, RuneType rune)
	{
		ButtonIndex = buttonIndex;
		SetRune(rune, animate: true);
	}

	public void SetHighlighted(bool highlighted)
	{
		if (RuneType == RuneType.RT_NONE)
		{
			m_emptyRuneHighlight.SetActive(highlighted);
		}
		else
		{
			m_runeHighlightFSM.SendEvent(highlighted ? "Hover" : "Hover_Off");
		}
	}

	public void PlayDragEffect()
	{
		m_runeFSM.SendEvent("PickUp");
	}

	public void StopDragEffect()
	{
		m_runeFSM.SendEvent("START");
	}
}
