using System;
using System.Collections.Generic;

namespace Hearthstone.UI;

public abstract class AsyncReferenceContainerBase
{
	protected HashSet<AsyncReference> m_asyncReferenceSet = new HashSet<AsyncReference>();

	protected bool m_waitingOnReferences;

	private Action m_readyCallback;

	private int m_referenceReadyCount;

	public bool RegisterReadyListener(Action onReady)
	{
		if (m_asyncReferenceSet.Count == 0)
		{
			Log.UIFramework.PrintError("RegisterReadyListener was called but the AsyncContainer doesn't hold any references. Please call this after adding all required references.");
			return false;
		}
		m_readyCallback = onReady;
		m_waitingOnReferences = true;
		if (GetIsLoaded())
		{
			TryFireReadyCallback();
		}
		return true;
	}

	public bool GetIsLoaded()
	{
		if (m_asyncReferenceSet.Count == 0)
		{
			return false;
		}
		if (m_referenceReadyCount == m_asyncReferenceSet.Count)
		{
			return true;
		}
		return false;
	}

	public void Clear()
	{
		m_asyncReferenceSet.Clear();
		m_referenceReadyCount = 0;
		m_readyCallback = null;
		m_waitingOnReferences = false;
	}

	protected void OnReferenceReady(AsyncReference reference)
	{
		if (m_asyncReferenceSet.Contains(reference))
		{
			m_referenceReadyCount++;
			if (GetIsLoaded())
			{
				TryFireReadyCallback();
			}
		}
	}

	private void TryFireReadyCallback()
	{
		if (m_readyCallback != null)
		{
			m_readyCallback();
			m_waitingOnReferences = false;
		}
	}
}
