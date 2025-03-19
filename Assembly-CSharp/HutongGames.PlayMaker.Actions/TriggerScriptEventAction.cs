using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Send an event based on an Actor's Card's elite flag.")]
public class TriggerScriptEventAction : FsmStateAction
{
	public FsmGameObject m_ScriptObject;

	public FsmString m_EventName;

	public override void Reset()
	{
		m_ScriptObject = null;
		m_EventName = null;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (m_ScriptObject == null || m_ScriptObject.Value == null)
		{
			Debug.LogWarning("Script Object is null");
			Finish();
		}
		else if (m_EventName == null || m_EventName.Value == null)
		{
			Debug.LogWarning("Event Name is null");
			Finish();
		}
		else
		{
			m_ScriptObject.Value.GetComponent<IScriptEventHandler>()?.HandleEvent(m_EventName.Value);
			Finish();
		}
	}
}
