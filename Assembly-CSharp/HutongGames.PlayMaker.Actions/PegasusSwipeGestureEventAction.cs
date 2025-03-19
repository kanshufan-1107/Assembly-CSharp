using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Sends an event when a swipe is detected.")]
[ActionCategory("Pegasus")]
public class PegasusSwipeGestureEventAction : FsmStateAction
{
	[Tooltip("How far a touch has to travel to be considered a swipe. Uses normalized distance (e.g. 1 = 1 screen diagonal distance). Should generally be a very small number.")]
	public FsmFloat minSwipeDistance;

	[Tooltip("Event to send when swipe left detected.")]
	public FsmEvent swipeLeftEvent;

	[Tooltip("Event to send when swipe right detected.")]
	public FsmEvent swipeRightEvent;

	[Tooltip("Event to send when swipe up detected.")]
	public FsmEvent swipeUpEvent;

	[Tooltip("Event to send when swipe down detected.")]
	public FsmEvent swipeDownEvent;

	[Tooltip("If checked, accept mouse gestures as touch input.")]
	public FsmBool mouseSupport;

	private float screenDiagonalSize;

	private float minSwipeDistancePixels;

	private bool touchStarted;

	private Vector2 touchStartPos;

	public override void Reset()
	{
		minSwipeDistance = 0.1f;
		swipeLeftEvent = null;
		swipeRightEvent = null;
		swipeUpEvent = null;
		swipeDownEvent = null;
	}

	public override void OnEnter()
	{
		if (!UniversalInputManager.Get().IsTouchMode() && !mouseSupport.Value)
		{
			Finish();
			return;
		}
		screenDiagonalSize = Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height);
		minSwipeDistancePixels = minSwipeDistance.Value * screenDiagonalSize;
	}

	public override void OnUpdate()
	{
		Touch touch;
		if (Input.touchCount > 0)
		{
			touch = Input.touches[0];
		}
		else
		{
			if (mouseSupport == null || !mouseSupport.Value)
			{
				return;
			}
			touch = default(Touch);
			touch.position = Input.mousePosition;
			if (Input.GetMouseButtonDown(0))
			{
				touch.phase = TouchPhase.Began;
			}
			else
			{
				if (!touchStarted || !Input.GetMouseButtonUp(0))
				{
					return;
				}
				touch.phase = TouchPhase.Ended;
			}
		}
		switch (touch.phase)
		{
		case TouchPhase.Began:
		{
			UniversalInputManager inputManager = UniversalInputManager.Get();
			if (inputManager != null)
			{
				inputManager.GetInputHitInfoByRenderPass(out var hitInfo, out var _);
				if (!(hitInfo.transform == null) && hitInfo.transform.TryGetComponent<PlaymakerSwipeable>(out var _))
				{
					touchStarted = true;
					touchStartPos = touch.position;
				}
			}
			break;
		}
		case TouchPhase.Ended:
			if (touchStarted)
			{
				TestForSwipeGesture(touch);
				touchStarted = false;
			}
			break;
		case TouchPhase.Canceled:
			touchStarted = false;
			break;
		case TouchPhase.Moved:
		case TouchPhase.Stationary:
			break;
		}
	}

	private void TestForSwipeGesture(Touch touch)
	{
		Vector2 lastPos = touch.position;
		if (Vector2.Distance(lastPos, touchStartPos) > minSwipeDistancePixels)
		{
			float dy = lastPos.y - touchStartPos.y;
			float dx = lastPos.x - touchStartPos.x;
			float angle = 57.29578f * Mathf.Atan2(dx, dy);
			angle = (360f + angle - 45f) % 360f;
			Debug.Log(angle);
			if (angle < 90f)
			{
				base.Fsm.Event(swipeRightEvent);
			}
			else if (angle < 180f)
			{
				base.Fsm.Event(swipeDownEvent);
			}
			else if (angle < 270f)
			{
				base.Fsm.Event(swipeLeftEvent);
			}
			else
			{
				base.Fsm.Event(swipeUpEvent);
			}
		}
	}
}
