using System.Collections;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class LuckyDrawHammerSlot : MonoBehaviour
{
	[SerializeField]
	private PlayMakerFSM m_hammerPlaymaker;

	[SerializeField]
	private PlayMakerFSM m_firstTimeHammerPlaymaker;

	[SerializeField]
	private VisualController m_firstHammerVisualController;

	private Widget m_widget;

	private const string kFirstHammerAnimStartEventName = "First_Hammer_Claimed";

	private bool m_rewardTargetReceived;

	private const float kHammerAnimationTimeoutTime = 15f;

	public PlayMakerFSM HammerPlaymaker => m_hammerPlaymaker;

	private void Awake()
	{
		m_widget = GetComponent<Widget>();
		if (m_widget == null)
		{
			Error.AddDevWarning("UI Error", "[LuckyDrawHammerSlot] Awake() m_widget was null!");
			return;
		}
		m_widget.RegisterEventListener(HandleEvent);
		if (m_hammerPlaymaker == null)
		{
			Error.AddDevWarning("UI Error", "[LuckyDrawHammerSlot] Awake() m_hammerPlaymaker was null!");
		}
		else if (m_firstTimeHammerPlaymaker == null)
		{
			Error.AddDevWarning("UI Error", "[LuckyDrawHammerSlot] Awake() m_firstTimeHammerPlaymaker was null!");
		}
	}

	private void HandleEvent(string eventName)
	{
		EventDataModel eventData = m_widget.GetDataModel<EventDataModel>();
		switch (eventName)
		{
		case "CODE_INITIALIZE_HAMMER":
			if (eventData == null)
			{
				Error.AddDevWarning("Error", "[LuckyDrawHammerSlot] HandleEvent() event payload was null from {0} event", eventName);
			}
			else
			{
				InitializeHammerFSMVariables(eventData);
			}
			break;
		case "CODE_HAMMER_SMASH_READY":
			if (eventData == null)
			{
				Error.AddDevWarning("Error", "[LuckyDrawHammerSlot] HandleEvent() event payload was null from {0} event", eventName);
			}
			else
			{
				SetupHammerSmashTarget(eventData);
			}
			break;
		case "CODE_DO_FIRST_HAMMER_CLAIM_ANIMATION":
			DoFirstHammerClaimAnim();
			break;
		case "CODE_ANTICIPATION_FINISHED":
			FlagAnticipationAnimationComplete();
			break;
		}
	}

	public void DisplayFirstHammer()
	{
		if (m_firstHammerVisualController == null)
		{
			Error.AddDevWarning("UI Error", "[LuckyDrawHammerSlot] DisplayFirstHammer() m_firstHammerVisualController was null!");
			return;
		}
		if (LuckyDrawManager.Get().GetLuckyDrawButtonDataModel().ClaimedFirstHammer)
		{
			m_firstHammerVisualController.SetState("INACTIVE");
			return;
		}
		m_firstHammerVisualController.SetState("ACTIVE");
		NarrativeManager.Get().OnLuckyDrawEntered();
	}

	private void InitializeHammerFSMVariables(EventDataModel eventPayload)
	{
		m_rewardTargetReceived = false;
		if (eventPayload == null)
		{
			Error.AddDevWarning("UI Error", "[LuckyDrawHammerSlot] InitializeHammerFSMVariables() eventPayload was null!");
			LuckyDrawUtils.ShowErrorAndReturnToLobby();
			return;
		}
		Vector3 hammerUpAmount = m_hammerPlaymaker.FsmVariables.GetFsmVector3("HammerUpPosition").Value;
		Vector3 anticipationOffset = m_hammerPlaymaker.FsmVariables.GetFsmVector3("HammerAnticipationOffset").Value;
		Vector3 hammerIdlePosition = m_hammerPlaymaker.transform.localPosition;
		m_hammerPlaymaker.FsmVariables.GetFsmVector3("HammerIdlePosition").Value = hammerIdlePosition;
		m_hammerPlaymaker.FsmVariables.GetFsmVector3("Calculated_HammerUpPosition").Value = hammerIdlePosition + hammerUpAmount;
		Vector3 hammerFrameCenterTarget = (Vector3)eventPayload.Payload + anticipationOffset;
		m_hammerPlaymaker.FsmVariables.GetFsmVector3("Calculated_HammerAnticipationPosition").Value = new Vector3(hammerFrameCenterTarget.x, hammerUpAmount.y + anticipationOffset.y, hammerFrameCenterTarget.z);
	}

	private void FlagAnticipationAnimationComplete()
	{
		StartCoroutine(WaitForTargetInfo());
	}

	private IEnumerator WaitForTargetInfo()
	{
		float cancelTime = Time.time + 15f;
		while (!m_rewardTargetReceived)
		{
			if (Time.time > cancelTime)
			{
				LuckyDrawManager.Get().LogError("Error [LuckyDrawHammerSlot] WaitForTargetInfo() timeout triggered while waiting for rewardTarget");
				LuckyDrawUtils.ShowErrorAndReturnToLobby();
				yield break;
			}
			yield return null;
		}
		m_hammerPlaymaker.SendEvent("Smash_Reward_Tile");
	}

	private void SetupHammerSmashTarget(EventDataModel eventPayload)
	{
		if (eventPayload == null)
		{
			Error.AddDevWarning("Error", "[LuckyDrawHammerSlot] PerformHammerSmashAnim() eventPayload was null!");
			LuckyDrawUtils.ShowErrorAndReturnToLobby();
			return;
		}
		Vector3 targetOffset = m_hammerPlaymaker.FsmVariables.GetFsmVector3("HammerTargetOffset").Value;
		Vector3 hammerDownOffset = m_hammerPlaymaker.FsmVariables.GetFsmVector3("HammerDownOffset").Value;
		Vector3 hammerUpAmount = m_hammerPlaymaker.FsmVariables.GetFsmVector3("HammerUpPosition").Value;
		Vector3 targetVector = (Vector3)eventPayload.Payload;
		m_hammerPlaymaker.FsmVariables.GetFsmVector3("TileWorldPosition").Value = targetVector;
		Vector3 hammerTargetHoverVector = targetVector + targetOffset;
		m_hammerPlaymaker.FsmVariables.GetFsmVector3("Calculated_TargetPosition").Value = new Vector3(hammerTargetHoverVector.x, hammerUpAmount.y + targetOffset.y, hammerTargetHoverVector.z);
		m_hammerPlaymaker.FsmVariables.GetFsmVector3("Calculated_HammerDownPosition").Value = new Vector3(hammerTargetHoverVector.x, 0f, hammerTargetHoverVector.z) + hammerDownOffset;
		m_rewardTargetReceived = true;
	}

	private void DoFirstHammerClaimAnim()
	{
		m_firstTimeHammerPlaymaker.SendEvent("First_Hammer_Claimed");
	}
}
