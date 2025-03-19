using PegasusShared;
using UnityEngine;

public class SideboardRuneIndicator : MonoBehaviour
{
	public RuneButton[] runeButtons;

	private void Start()
	{
		RuneButton[] array = runeButtons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetEnabled(enabled: false);
		}
	}

	public void UpdateRunes(RuneType[] runes)
	{
		if (runes == null)
		{
			return;
		}
		int count = 0;
		RuneButton[] array = runeButtons;
		foreach (RuneButton runeButton in array)
		{
			if (count >= runes.Length)
			{
				runeButton.SetRune(RuneType.RT_NONE, animate: false);
				continue;
			}
			runeButton.SetRune(runes[count], animate: false);
			count++;
		}
	}
}
