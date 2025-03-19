using System;
using UnityEngine;

[Serializable]
public class CardTargetInfo
{
	public enum TargetOrigin
	{
		CARD,
		OPPONENTS_DECK,
		OPPONENTS_HAND,
		PLAYERS_DECK,
		PLAYERS_HAND
	}

	[SerializeField]
	private ZoneTransitionStyle m_transitionStyle = ZoneTransitionStyle.VERY_SLOW;

	[SerializeField]
	private TargetOrigin m_origin;

	public ZoneTransitionStyle TransitionStyle => m_transitionStyle;

	public TargetOrigin Origin => m_origin;
}
