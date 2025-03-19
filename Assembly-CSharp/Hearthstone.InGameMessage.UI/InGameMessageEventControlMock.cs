using System;
using System.Collections.Generic;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Hearthstone.Core;
using UnityEngine;

namespace Hearthstone.InGameMessage.UI;

public class InGameMessageEventControlMock : IInGameMessageEventControl, IDisposable
{
	private Map<PopupEvent, KeyCode> m_eventsKeyValues = new Map<PopupEvent, KeyCode> { [PopupEvent.OnHubScene] = KeyCode.A };

	private bool listenForInput;

	private Action<PopupEvent> OnTiggerEventCallback { get; set; }

	public void Initialize(Action<PopupEvent> onTiggerEventCallback)
	{
		OnTiggerEventCallback = onTiggerEventCallback;
		listenForInput = true;
		Processor.QueueJob("testListenForInput", ListenForInput());
	}

	public void Dispose()
	{
		listenForInput = false;
	}

	private IEnumerator<IAsyncJobResult> ListenForInput()
	{
		while (listenForInput)
		{
			yield return null;
			foreach (KeyValuePair<PopupEvent, KeyCode> val in m_eventsKeyValues)
			{
				if (Input.GetKeyDown(val.Value))
				{
					OnTiggerEventCallback?.Invoke(val.Key);
				}
			}
		}
	}
}
