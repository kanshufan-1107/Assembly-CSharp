using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Prototyping/Keyboard Animation Settings")]
public class KeyboardAnimationSettings : ScriptableObject
{
	[Serializable]
	public class KeyAndAnimationTriggerPair
	{
		[Tooltip("The keyboard key to press to trigger the animation.")]
		public KeyCode KeyboardKey;

		[Tooltip("The name of the animation trigger (not the state) to set when the keyboard key is pressed.")]
		public string AnimationTrigger;
	}

	[Tooltip("A list of the bindings from keyboard keys to animation trigger names.")]
	public List<KeyAndAnimationTriggerPair> Settings = new List<KeyAndAnimationTriggerPair>();

	public KeyAndAnimationTriggerPair this[int idx]
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
