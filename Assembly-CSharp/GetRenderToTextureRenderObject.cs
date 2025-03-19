using HutongGames.PlayMaker;
using UnityEngine;

[HutongGames.PlayMaker.Tooltip("Get the object being rendered to from RenderToTexture")]
[ActionCategory("Pegasus")]
public class GetRenderToTextureRenderObject : FsmStateAction
{
	[CheckForComponent(typeof(RenderToTexture))]
	[RequiredField]
	public FsmOwnerDefault gameObject;

	[UIHint(UIHint.Variable)]
	[RequiredField]
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
