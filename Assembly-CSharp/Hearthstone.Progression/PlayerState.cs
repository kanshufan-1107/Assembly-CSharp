using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;

namespace Hearthstone.Progression;

public class PlayerState<T> : IEnumerable<T>, IEnumerable where T : class
{
	private readonly Map<int, T> m_playerState = new Map<int, T>();

	private readonly Func<int, T> m_defaultProvider;

	public event Action<T, T> OnStateChanged = delegate
	{
	};

	public PlayerState(Func<int, T> defaultProvider = null)
	{
		m_defaultProvider = defaultProvider;
	}

	public T GetState(int id)
	{
		if (!m_playerState.TryGetValue(id, out var state))
		{
			Func<int, T> defaultProvider = m_defaultProvider;
			if (defaultProvider == null)
			{
				return null;
			}
			return defaultProvider(id);
		}
		return state;
	}

	public void UpdateState(int id, T newState)
	{
		T oldState = GetState(id);
		m_playerState[id] = newState;
		this.OnStateChanged(oldState, newState);
	}

	public void Reset()
	{
		m_playerState.Clear();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return m_playerState.Values.GetEnumerator();
	}

	public IEnumerator GetEnumerator()
	{
		return m_playerState.Values.GetEnumerator();
	}
}
