using System.Collections.Generic;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Assign an actor's portrait mesh to children of a specified actor (used for Hero Portrait effects)")]
public class AssignActorPortraitToChildrenAction : FsmStateAction
{
	[Tooltip("Provide an actor to apply the portrait to. If no actor is provided for m_ActorToAssignPortraitFrom, this actor's parent will be used")]
	public FsmGameObject m_ActorToAssignPortraitTo;

	[Tooltip("Provide an actor to specifically use for the portrait, otherwise we'll use a parent of m_ActorToAssignPortraitTo")]
	public FsmGameObject m_ActorToAssignPortraitFrom;

	public override void Reset()
	{
		m_ActorToAssignPortraitFrom = null;
		m_ActorToAssignPortraitTo = null;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (m_ActorToAssignPortraitTo == null)
		{
			Finish();
			return;
		}
		Actor actorToCopy = null;
		actorToCopy = ((!m_ActorToAssignPortraitFrom.Value) ? GameObjectUtils.FindComponentInThisOrParents<Actor>(m_ActorToAssignPortraitTo.Value) : GameObjectUtils.FindComponentInThisOrParents<Actor>(m_ActorToAssignPortraitFrom.Value));
		if (actorToCopy == null || actorToCopy.m_portraitMesh == null)
		{
			Finish();
			return;
		}
		List<Material> portraitMats = actorToCopy.m_portraitMesh.GetComponent<Renderer>().GetMaterials();
		if (portraitMats.Count == 0 || actorToCopy.m_portraitMatIdx < 0)
		{
			Finish();
			return;
		}
		Texture portraitTex = portraitMats[actorToCopy.m_portraitMatIdx].mainTexture;
		Renderer[] componentsInChildren = m_ActorToAssignPortraitTo.Value.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			foreach (Material mat in componentsInChildren[i].GetMaterials())
			{
				if (mat.name.Contains("portrait"))
				{
					mat.mainTexture = portraitTex;
				}
			}
		}
		Finish();
	}
}
