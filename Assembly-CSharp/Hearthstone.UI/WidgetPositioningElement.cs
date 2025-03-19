using System.Collections;
using UnityEngine;

namespace Hearthstone.UI;

public abstract class WidgetPositioningElement : MonoBehaviour
{
	private bool m_refreshQueued;

	private Coroutine m_refreshCoroutine;

	protected virtual void OnEnable()
	{
		if (m_refreshQueued)
		{
			Refresh();
			m_refreshQueued = false;
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_refreshCoroutine != null)
		{
			StopCoroutine(m_refreshCoroutine);
		}
	}

	public void Refresh()
	{
		if (!base.isActiveAndEnabled)
		{
			m_refreshQueued = true;
		}
		else
		{
			InternalRefresh();
		}
	}

	public void RefreshWithDelay(float seconds = 0f)
	{
		if (!base.isActiveAndEnabled)
		{
			m_refreshQueued = true;
			return;
		}
		if (m_refreshCoroutine != null)
		{
			StopCoroutine(m_refreshCoroutine);
		}
		m_refreshCoroutine = StartCoroutine(UpdateElementDelay(seconds));
	}

	protected abstract void InternalRefresh();

	private IEnumerator UpdateElementDelay(float seconds)
	{
		if (seconds <= 0f)
		{
			yield return null;
		}
		else
		{
			yield return new WaitForSeconds(seconds);
		}
		Refresh();
	}
}
