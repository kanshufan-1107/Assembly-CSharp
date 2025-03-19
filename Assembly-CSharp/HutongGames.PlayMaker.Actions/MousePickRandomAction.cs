using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Raycast from camera and send events. Mouse Down can have a rendom chance to send event")]
public class MousePickRandomAction : FsmStateAction
{
	[CheckForComponent(typeof(Collider))]
	public FsmOwnerDefault GameObject;

	[Tooltip("Additional Colliders for mouse pick")]
	public FsmGameObject[] additionalColliders;

	[Tooltip("Min number of clicks before random gate open")]
	public FsmInt RandomGateClicksMin = 0;

	[Tooltip("Max number of clicks before random gate open")]
	public FsmInt RandomGateClicksMax = 0;

	[Tooltip("Resets count to 0 once triggered")]
	public FsmBool ResetOnOpen = false;

	[Tooltip("Mouse Down event. Random Gate open (true)")]
	public FsmEvent mouseDownGateOpen;

	[Tooltip("Mouse Down event. Random Gate is closed (false)")]
	public FsmEvent mouseDownGateClosed;

	[Tooltip("Mouse Over event")]
	public FsmEvent mouseOver;

	[Tooltip("Mouse Up event")]
	public FsmEvent mouseUp;

	[Tooltip("Mouse Off event")]
	public FsmEvent mouseOff;

	[Tooltip("Check for clicks as soon as the state machine enters this state.")]
	public bool checkFirstFrame;

	[Tooltip("Repeat every frame.")]
	public bool everyFrame;

	[Tooltip("Stop processing after an event is triggered.")]
	public bool oneShot;

	[Tooltip("Click Count")]
	public FsmInt ClickCount = 0;

	private int m_RandomValue;

	private bool m_opened = true;

	public override void Reset()
	{
		GameObject = null;
		additionalColliders = new FsmGameObject[0];
		RandomGateClicksMin = 0;
		RandomGateClicksMax = 0;
		ResetOnOpen = false;
		mouseDownGateOpen = null;
		mouseDownGateClosed = null;
		mouseOver = null;
		mouseUp = null;
		mouseOff = null;
		checkFirstFrame = true;
		everyFrame = true;
		oneShot = false;
		ClickCount = 0;
	}

	public override void OnEnter()
	{
		if (RandomGateClicksMin.Value > RandomGateClicksMax.Value)
		{
			RandomGateClicksMin = RandomGateClicksMax;
		}
		if (m_opened)
		{
			m_RandomValue = Random.Range(RandomGateClicksMin.Value, RandomGateClicksMax.Value);
			m_opened = false;
		}
		if (checkFirstFrame)
		{
			DoMousePickEvent();
		}
		if (!everyFrame)
		{
			Finish();
		}
	}

	public override void OnUpdate()
	{
		DoMousePickEvent();
	}

	private void DoMousePickEvent()
	{
		bool num = mouseOver != null || mouseOff != null;
		bool hasMouseUpEvent = mouseUp != null;
		bool hasMouseDownEvent = mouseDownGateOpen != null || mouseDownGateClosed != null;
		if (!num && (!hasMouseUpEvent || !InputCollection.GetMouseButtonUp(0)) && (!hasMouseDownEvent || !InputCollection.GetMouseButtonDown(0)))
		{
			return;
		}
		GameObject testObject = ((GameObject.OwnerOption == OwnerDefaultOption.UseOwner) ? base.Owner : GameObject.GameObject.Value);
		if (!InputUtil.IsPlayMakerMouseInputAllowed(testObject))
		{
			return;
		}
		bool isMouseOver = UniversalInputManager.Get().InputIsOver(testObject.gameObject);
		if (!isMouseOver && additionalColliders.Length != 0)
		{
			for (int i = 0; i < additionalColliders.Length; i++)
			{
				GameObject addTestObj = additionalColliders[i].Value;
				if (!(addTestObj == null))
				{
					isMouseOver = UniversalInputManager.Get().InputIsOver(addTestObj);
					if (isMouseOver)
					{
						break;
					}
				}
			}
		}
		if (isMouseOver)
		{
			if (InputCollection.GetMouseButtonDown(0))
			{
				ClickCount.Value += 1;
				if (ClickCount.Value >= m_RandomValue)
				{
					m_opened = true;
					if (ResetOnOpen.Value)
					{
						ClickCount.Value = 0;
					}
					if (mouseDownGateOpen != null)
					{
						base.Fsm.Event(mouseDownGateOpen);
					}
				}
				else if (mouseDownGateClosed != null)
				{
					base.Fsm.Event(mouseDownGateClosed);
				}
				if (oneShot)
				{
					Finish();
				}
			}
			if (mouseOver != null)
			{
				base.Fsm.Event(mouseOver);
			}
			if (mouseUp != null && InputCollection.GetMouseButtonUp(0))
			{
				base.Fsm.Event(mouseUp);
				if (oneShot)
				{
					Finish();
				}
			}
		}
		else if (mouseOff != null)
		{
			base.Fsm.Event(mouseOff);
		}
	}

	public override string ErrorCheck()
	{
		return "";
	}
}
