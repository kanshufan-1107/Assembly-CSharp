using System.Collections;
using UnityEngine;

public class NAX7_05_Spell : Spell
{
	protected override void OnBirth(SpellStateType prevStateType)
	{
		StartCoroutine(SpellEffect(prevStateType));
	}

	private IEnumerator SpellEffect(SpellStateType prevStateType)
	{
		PlayMakerFSM fsm = Board.Get().transform.Find("Board_NAX").Find("NAX_Crystal_Skinned").GetComponent<PlayMakerFSM>();
		if (fsm == null)
		{
			Debug.LogWarning("NAX7_05_Spell unable to get playmaker fsm");
			yield break;
		}
		fsm.SendEvent("ClickTop");
		base.OnBirth(prevStateType);
		OnSpellFinished();
	}
}
