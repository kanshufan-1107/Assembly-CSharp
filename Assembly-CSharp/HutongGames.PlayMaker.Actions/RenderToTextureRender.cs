using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Triggers a render to texture to render.")]
public class RenderToTextureRender : FsmStateAction
{
	[RequiredField]
	public FsmOwnerDefault r2tObject;

	public bool now;

	public override void Reset()
	{
		r2tObject = null;
		now = false;
	}

	public override void OnEnter()
	{
		GameObject go = base.Fsm.GetOwnerDefaultTarget(r2tObject);
		if (go != null)
		{
			RenderToTexture r2t = go.GetComponent<RenderToTexture>();
			if (r2t != null)
			{
				if (now)
				{
					r2t.RenderNow();
				}
				else
				{
					r2t.Render();
				}
			}
		}
		Finish();
	}
}
