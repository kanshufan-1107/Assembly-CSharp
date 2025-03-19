using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus Audio")]
[Tooltip("Start ducking one or more category of sounds.")]
public class AudioStartDuckingAction : FsmStateAction
{
	[RequiredField]
	[Tooltip("Game Object to responsible for ducking.")]
	public FsmOwnerDefault m_GameObject;

	public SoundDuckedCategoryDef[] m_DuckedCategoryDefs;

	public override void Reset()
	{
	}

	public override void OnEnter()
	{
		GameObject go = base.Fsm.GetOwnerDefaultTarget(m_GameObject);
		if (go != null)
		{
			SoundDucker ducker = null;
			ducker = go.GetComponent<SoundDucker>();
			if (ducker == null)
			{
				ducker = go.AddComponent<SoundDucker>();
			}
			List<SoundDuckedCategoryDef> duckDefs = new List<SoundDuckedCategoryDef>(m_DuckedCategoryDefs);
			ducker.SetDuckedCategoryDefs(duckDefs);
			ducker.StartDucking();
		}
		Finish();
	}
}
