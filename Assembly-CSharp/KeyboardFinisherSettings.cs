using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Prototyping/Keyboard Finisher Settings")]
public class KeyboardFinisherSettings : ScriptableObject
{
	public enum DamageLevel
	{
		Small,
		Large
	}

	public enum LethalLevel
	{
		Nonlethal,
		Lethal,
		FirstPlaceVictory
	}

	[Serializable]
	public class KeyAndFinisherTriggerPair
	{
		public KeyCode KeyboardKey;

		public FinisherGameplaySettings Finisher;

		public DamageLevel DamageLevel;

		public LethalLevel LethalLevel;

		public int ImpactDamage = 1;

		public FinisherAuthoringList AllFinishers;
	}

	[Tooltip("A list of the bindings from keyboard keys to finisher data.")]
	public List<KeyAndFinisherTriggerPair> Settings = new List<KeyAndFinisherTriggerPair>();

	public KeyAndFinisherTriggerPair this[int idx]
	{
		get
		{
			return Settings[idx];
		}
		set
		{
			Settings[idx] = value;
		}
	}

	public int Count => Settings.Count;
}
