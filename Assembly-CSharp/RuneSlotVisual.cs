using System.Collections.Generic;
using PegasusShared;
using UnityEngine;

public class RuneSlotVisual : MonoBehaviour
{
	public GameObject m_deckRunesContainer;

	public List<Rune> m_deckRuneSlots;

	public void Show(RuneType[] runes)
	{
		m_deckRunesContainer.gameObject.SetActive(value: true);
		UpdateRuneSlots(runes, RuneState.Uninit);
	}

	public void Show(RuneType[] runes, RuneState runeState)
	{
		m_deckRunesContainer.gameObject.SetActive(value: true);
		UpdateRuneSlots(runes, runeState);
	}

	public void Show(RunePattern runes)
	{
		Show(runes, RuneState.Uninit);
	}

	public void Show(RunePattern runes, RuneState runeState)
	{
		m_deckRunesContainer.gameObject.SetActive(value: true);
		UpdateRuneSlots(runes, runeState);
	}

	public void SetState(RuneState runeState)
	{
		foreach (Rune deckRuneSlot in m_deckRuneSlots)
		{
			deckRuneSlot.SetState(runeState);
		}
	}

	public void Hide()
	{
		m_deckRunesContainer.gameObject.SetActive(value: false);
	}

	private void UpdateRuneSlots(RunePattern runes, RuneState runeState)
	{
		if (runes.CombinedValue <= 0)
		{
			return;
		}
		RuneType[] runesArray = new RuneType[runes.CombinedValue];
		int index = 0;
		RuneType[] validRuneTypes = RunePattern.ValidRuneTypes;
		foreach (RuneType runeType in validRuneTypes)
		{
			int amount = runes.GetCost(runeType);
			for (int j = 0; j < amount; j++)
			{
				runesArray[index] = runeType;
				index++;
			}
		}
		UpdateRuneSlots(runesArray, runeState);
	}

	private void UpdateRuneSlots(RuneType[] runes, RuneState runeState)
	{
		if (runes == null)
		{
			return;
		}
		int index = 0;
		foreach (Rune runeSlot in m_deckRuneSlots)
		{
			if (index >= runes.Length)
			{
				runeSlot.ShowRune(RuneType.RT_NONE, RuneState.Empty);
				continue;
			}
			runeSlot.ShowRune(runes[index], runeState);
			index++;
		}
	}
}
