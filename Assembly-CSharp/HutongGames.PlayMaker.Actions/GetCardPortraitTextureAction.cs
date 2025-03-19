using Blizzard.T5.Core.Utils;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Initialize a spell state, setting variables that reference the parent actor and its contents.")]
public class GetCardPortraitTextureAction : FsmStateAction
{
	[UIHint(UIHint.Variable)]
	[Title("StoreTexture")]
	[Tooltip("Store the portrait texture in a variable.")]
	[RequiredField]
	public FsmTexture storedTexture;

	public override void Reset()
	{
	}

	public override void OnEnter()
	{
		Actor actor = GameObjectUtils.FindComponentInThisOrParents<Actor>(base.Owner);
		if (actor == null)
		{
			Debug.LogWarningFormat("Failed to find Actor in GetCardPortraitTextureAction: {0}", GameObjectUtils.FindComponentInThisOrParents<Card>(base.Owner));
			Finish();
			return;
		}
		if (!actor.HasCardDef)
		{
			Debug.LogWarningFormat("Failed to get CardDef in GetCardPortraitTextureAction: {0}", GameObjectUtils.FindComponentInThisOrParents<Card>(base.Owner));
			Finish();
			return;
		}
		Texture portraitTexture = actor.PortraitTexture;
		if (portraitTexture == null)
		{
			Debug.LogWarningFormat("Failed to get GetPortraitTexture in GetCardPortraitTextureAction: {0}", GameObjectUtils.FindComponentInThisOrParents<Card>(base.Owner));
			Finish();
		}
		else
		{
			storedTexture.Value = portraitTexture;
			Finish();
		}
	}
}
