using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Sets the visibility on UberText objects and its UberText children.")]
[ActionCategory("Pegasus")]
public class SetUberTextVisibilityRecursiveAction : FsmStateAction
{
	[RequiredField]
	public FsmOwnerDefault gameObject;

	[Tooltip("Should the objects be set to visible or invisible?")]
	public FsmBool visible;

	[Tooltip("Resets to the initial visibility once\nit leaves the state")]
	public bool resetOnExit;

	public bool includeChildren;

	private Map<UberText, bool> m_initialVisibility = new Map<UberText, bool>();

	public override void Reset()
	{
		gameObject = null;
		visible = false;
		resetOnExit = false;
		includeChildren = false;
		m_initialVisibility.Clear();
	}

	public override void OnEnter()
	{
		GameObject go = base.Fsm.GetOwnerDefaultTarget(gameObject);
		if (go == null)
		{
			Finish();
			return;
		}
		if (includeChildren)
		{
			UberText[] uberTextObjs = go.GetComponentsInChildren<UberText>();
			if (uberTextObjs != null)
			{
				UberText[] array = uberTextObjs;
				foreach (UberText ut in array)
				{
					m_initialVisibility[ut] = !ut.isHidden();
					if (visible.Value)
					{
						ut.Show();
					}
					else
					{
						ut.Hide();
					}
				}
			}
		}
		else
		{
			UberText ut2 = go.GetComponent<UberText>();
			if (ut2 != null)
			{
				m_initialVisibility[ut2] = !ut2.isHidden();
				if (visible.Value)
				{
					ut2.Show();
				}
				else
				{
					ut2.Hide();
				}
			}
		}
		Finish();
	}

	public override void OnExit()
	{
		if (!resetOnExit)
		{
			return;
		}
		foreach (KeyValuePair<UberText, bool> pair in m_initialVisibility)
		{
			UberText ut = pair.Key;
			if (!(ut == null))
			{
				if (pair.Value)
				{
					ut.Show();
				}
				else
				{
					ut.Hide();
				}
			}
		}
	}
}
