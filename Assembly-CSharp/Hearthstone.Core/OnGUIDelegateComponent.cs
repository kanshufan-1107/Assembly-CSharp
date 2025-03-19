using System;
using UnityEngine;

namespace Hearthstone.Core;

public class OnGUIDelegateComponent : MonoBehaviour
{
	private Action m_OnGUIAction;

	public void SetGUIAction(Action newAction)
	{
		m_OnGUIAction = newAction;
	}

	private void OnGUI()
	{
		m_OnGUIAction();
	}

	public static GameObject CreateGUIDelegate(Action action, bool ddol = true)
	{
		GameObject go = new GameObject("OnGUI");
		OnGUIDelegateComponent onGUIDelegateComponent = go.AddComponent<OnGUIDelegateComponent>();
		if (ddol)
		{
			UnityEngine.Object.DontDestroyOnLoad(go);
		}
		onGUIDelegateComponent.SetGUIAction(action);
		return go;
	}
}
