using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Sets the layer on a game object and its children.")]
public class SetLayerRecursiveAction : FsmStateAction
{
	[RequiredField]
	public FsmOwnerDefault gameObject;

	[HideIf("HideEnum")]
	[Tooltip("Layer number")]
	public GameLayer layer;

	[Tooltip("Layer number")]
	[HideIf("HideInt")]
	public FsmInt layerAsInt;

	[Tooltip("Pass layer in as int instead of enum")]
	public bool asInt;

	[Tooltip("Resets to the initial layer once\nit leaves the state")]
	public bool resetOnExit;

	public bool includeChildren;

	private Map<GameObject, GameLayer> m_initialLayer = new Map<GameObject, GameLayer>();

	public bool HideInt()
	{
		return !asInt;
	}

	public bool HideEnum()
	{
		return asInt;
	}

	public override void Reset()
	{
		gameObject = null;
		layer = GameLayer.Default;
		layerAsInt = null;
		resetOnExit = true;
		includeChildren = false;
		m_initialLayer.Clear();
	}

	public override void OnEnter()
	{
		GameObject go = base.Fsm.GetOwnerDefaultTarget(gameObject);
		int useLayer = (asInt ? layerAsInt.Value : ((int)layer));
		if (go == null)
		{
			Finish();
			return;
		}
		if (includeChildren)
		{
			Transform[] xforms = go.GetComponentsInChildren<Transform>();
			if (xforms != null)
			{
				Transform[] array = xforms;
				foreach (Transform t in array)
				{
					m_initialLayer[t.gameObject] = (GameLayer)t.gameObject.layer;
					t.gameObject.layer = useLayer;
				}
			}
		}
		else
		{
			Transform t2 = go.GetComponent<Transform>();
			if (t2 != null)
			{
				m_initialLayer[t2.gameObject] = (GameLayer)t2.gameObject.layer;
				t2.gameObject.layer = useLayer;
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
		foreach (KeyValuePair<GameObject, GameLayer> pair in m_initialLayer)
		{
			GameObject go = pair.Key;
			if (!(go == null))
			{
				go.layer = (int)pair.Value;
			}
		}
	}
}
