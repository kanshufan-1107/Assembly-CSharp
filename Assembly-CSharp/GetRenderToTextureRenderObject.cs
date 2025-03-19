using HutongGames.PlayMaker;
using UnityEngine;

[ActionCategory("Pegasus")]
[HutongGames.PlayMaker.Tooltip("Get the object being rendered to from RenderToTexture")]
public class GetRenderToTextureRenderObject : FsmStateAction
{
	[RequiredField]
	[CheckForComponent(typeof(RenderToTexture))]
	public FsmOwnerDefault gameObject;

	[RequiredField]
	[UIHint(UIHint.Variable)]
	public FsmGameObject renderObject;

	[HutongGames.PlayMaker.Tooltip("Get the object being rendered to from RenderToTexture. This is used to get the procedurally generated render plane object.")]
	public override void Reset()
	{
		gameObject = null;
		renderObject = null;
	}

	public override void OnEnter()
	{
		DoGetObject();
		Finish();
	}

	private void DoGetObject()
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
				renderObject.Value = r2t.GetRenderToObject();
			}
		}
	}
}
