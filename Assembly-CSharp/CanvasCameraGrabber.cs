using UnityEngine;

public class CanvasCameraGrabber : MonoBehaviour
{
	private void Awake()
	{
		Canvas canvas = GetComponent<Canvas>();
		if ((bool)canvas)
		{
			canvas.worldCamera = CameraUtils.FindFirstByLayer(GameLayer.BattleNet);
		}
	}
}
