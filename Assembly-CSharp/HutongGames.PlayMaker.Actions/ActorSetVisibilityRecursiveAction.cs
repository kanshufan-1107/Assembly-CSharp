using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Sets the visibility on a game object and its children. Will properly Show/Hide Actors in the hierarchy.")]
public class ActorSetVisibilityRecursiveAction : FsmStateAction
{
	public FsmOwnerDefault m_GameObject;

	[Tooltip("Should objects be set to visible or invisible?")]
	public FsmBool m_Visible;

	[Tooltip("Don't touch the Actor's SpellTable when setting visibility")]
	public FsmBool m_IgnoreSpells;

	[Tooltip("Resets to the initial visibility once it leaves the state")]
	public bool m_ResetOnExit;

	[Tooltip("Should children of the Game Object be affected?")]
	public bool m_IncludeChildren;

	[Tooltip("Should parents of the Game Object be affected?")]
	public bool m_IncludeParents;

	private Map<GameObject, bool> m_initialVisibility = new Map<GameObject, bool>();

	public override void Reset()
	{
		m_GameObject = null;
		m_Visible = false;
		m_IgnoreSpells = false;
		m_ResetOnExit = false;
		m_IncludeChildren = true;
		m_initialVisibility.Clear();
	}

	public override void OnEnter()
	{
		GameObject go = base.Fsm.GetOwnerDefaultTarget(m_GameObject);
		if (go != null)
		{
			if (m_ResetOnExit)
			{
				SaveInitialVisibility(go);
			}
			SetVisibility(go);
		}
		Finish();
	}

	public override void OnExit()
	{
		if (m_ResetOnExit)
		{
			RestoreInitialVisibility();
		}
	}

	private void SaveInitialVisibility(GameObject go)
	{
		Actor actor;
		if (m_IncludeParents && go.transform.parent != null)
		{
			Transform t = go.transform.parent;
			while (t != null)
			{
				SaveInitialVisibilityImp(t.gameObject, out actor);
				if (actor != null)
				{
					m_initialVisibility.Clear();
					m_initialVisibility[t.gameObject] = actor.IsShown();
					return;
				}
				t = t.transform.parent;
			}
		}
		SaveInitialVisibilityImp(go, out actor);
		if (actor != null || !m_IncludeChildren)
		{
			return;
		}
		foreach (Transform child in go.transform)
		{
			SaveInitialVisibility(child.gameObject);
		}
	}

	private bool SaveInitialVisibilityImp(GameObject go, out Actor actor)
	{
		bool show = false;
		actor = go.GetComponent<Actor>();
		if (actor != null)
		{
			show = (m_initialVisibility[go] = actor.IsShown());
		}
		else
		{
			Renderer r = go.GetComponent<Renderer>();
			if (r != null)
			{
				show = (m_initialVisibility[go] = r.enabled);
			}
		}
		return show;
	}

	private void RestoreInitialVisibility()
	{
		foreach (KeyValuePair<GameObject, bool> pair in m_initialVisibility)
		{
			GameObject go = pair.Key;
			bool visible = pair.Value;
			Actor actor = go.GetComponent<Actor>();
			if (actor != null)
			{
				if (visible)
				{
					ShowActor(actor);
				}
				else
				{
					HideActor(actor);
				}
			}
			else
			{
				go.GetComponent<Renderer>().enabled = visible;
			}
		}
	}

	private void SetVisibility(GameObject go)
	{
		Actor actor;
		if (m_IncludeParents && go.transform.parent != null)
		{
			Transform t = go.transform.parent;
			while (t != null)
			{
				SetVisibilityImp(t.gameObject, out actor);
				if (actor != null)
				{
					return;
				}
				t = t.transform.parent;
			}
		}
		SetVisibilityImp(go, out actor);
		if (actor != null || !m_IncludeChildren)
		{
			return;
		}
		foreach (Transform child in go.transform)
		{
			SetVisibilityInChildren(child.gameObject);
		}
	}

	private void SetVisibilityImp(GameObject go, out Actor actor)
	{
		actor = go.GetComponent<Actor>();
		if (actor != null)
		{
			if (m_Visible.Value)
			{
				ShowActor(actor);
			}
			else
			{
				HideActor(actor);
			}
			return;
		}
		Renderer r = go.GetComponent<Renderer>();
		if (r != null)
		{
			r.enabled = m_Visible.Value;
		}
	}

	private void SetVisibilityInChildren(GameObject go)
	{
		SetVisibilityImp(go, out var actor);
		if (actor != null || !m_IncludeChildren)
		{
			return;
		}
		foreach (Transform child in go.transform)
		{
			SetVisibilityInChildren(child.gameObject);
		}
	}

	private void ShowActor(Actor actor)
	{
		actor.Show(m_IgnoreSpells.Value);
	}

	private void HideActor(Actor actor)
	{
		actor.Hide(m_IgnoreSpells.Value);
	}
}
