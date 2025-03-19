using HutongGames.PlayMaker;
using UnityEngine;

[HutongGames.PlayMaker.Tooltip("Get the material instance from an object with RenderToTexture")]
[ActionCategory("Pegasus")]
public class GetRenderToTextureMaterial : FsmStateAction
{
	[RequiredField]
	[CheckForComponent(typeof(RenderToTexture))]
	public FsmOwnerDefault gameObject;

	[RequiredField]
	[UIHint(UIHint.Variable)]
	public FsmMaterial material;

	[HutongGames.PlayMaker.Tooltip("Get the material instance from an object with RenderToTexture. This is used to get the material of the procedurally generated render plane.")]
	public override void Reset()
	{
		gameObject = null;
		material = null;
	}

	public override void OnEnter()
	{
		DoGetMaterial();
		Finish();
	}

	private void DoGetMaterial()
	{
		GameObject go = base.Fsm.GetOwnerDefaultTarget(gameObject);
		if (!(go == null))
		{
			RenderToTexture r2t = go.GetComponent<RenderToTexture>();
			if (r2t == null)
			{
				LogError("Missing RenderToTexture component!");
			}
			else
			{
				material.Value = r2t.GetRenderMaterial();
			}
		}
	}
}
