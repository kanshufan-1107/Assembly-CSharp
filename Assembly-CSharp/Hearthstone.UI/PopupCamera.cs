using Hearthstone.UI.Core;
using UnityEngine;

namespace Hearthstone.UI;

[ExecuteAlways]
[AddComponentMenu("")]
public class PopupCamera : MonoBehaviour, IPopupCamera
{
	public Camera MirroredCamera { get; set; }

	public int CullingMask { get; set; }

	public float Depth { get; set; }

	public Camera Camera { get; private set; }

	private void Awake()
	{
		Camera = CameraUtils.FindFirstByLayer(GameLayer.BattleNet);
		Camera.cullingMask = 0;
		Update();
	}

	private void Update()
	{
		if (MirroredCamera != null)
		{
			Transform obj = base.transform;
			Transform transform2 = MirroredCamera.transform;
			obj.position = transform2.position;
			obj.rotation = transform2.rotation;
			obj.localScale = transform2.localScale;
			Camera.CopyFrom(MirroredCamera);
		}
		Camera.cullingMask = CullingMask;
		Camera.clearFlags = CameraClearFlags.Depth;
		Camera.depth = Depth;
	}
}
