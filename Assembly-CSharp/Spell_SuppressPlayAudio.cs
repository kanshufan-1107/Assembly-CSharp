using UnityEngine;

public class Spell_SuppressPlayAudio : Spell
{
	public override void SetSource(GameObject go)
	{
		Source = go;
		if (!(Source == null))
		{
			Card sourceCard = Source.GetComponent<Card>();
			if (sourceCard != null)
			{
				sourceCard.SuppressPlaySounds(suppress: true);
			}
		}
	}
}
