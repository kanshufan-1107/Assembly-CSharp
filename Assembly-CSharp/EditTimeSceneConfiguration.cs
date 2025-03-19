using System;
using System.Collections.Generic;
using UnityEngine;

public class EditTimeSceneConfiguration : MonoBehaviour
{
	[Serializable]
	public class SceneConfiguration
	{
		[Tooltip("A readable name for this state for the dropdown")]
		public string ConfigurationName;

		[Tooltip("The index of a state to run (zero-indexed) before running this one. -1 = don't run a state before this. Used to group common operations. This property is recursive, i.e. if that state has a FirstRunState value, that state's FirstRunState will be run first.")]
		public int FirstRunState = -1;

		[Tooltip("List of references to game objects whose active-self property will be set false.")]
		public List<GameObject> ObjectsToActivate = new List<GameObject>();

		[Tooltip("List of references to game objects whose active-self property will be set true.")]
		public List<GameObject> ObjectsToDeactivate = new List<GameObject>();

		[Tooltip("List of references to components whose enabled property will be set false.")]
		public List<Behaviour> ComponentsToActivate = new List<Behaviour>();

		[Tooltip("List of references to components whose enabled property will be set true.")]
		public List<Behaviour> ComponentsToDeactivate = new List<Behaviour>();

		[Tooltip("Check this to suppress it from appearing in the dropdown")]
		public bool Hidden;
	}

	[HideInInspector]
	public string LastSelectedConfiguration = "";

	public List<SceneConfiguration> Configurations = new List<SceneConfiguration>();

	[SerializeField]
	[Header("Quick Links")]
	private KeyboardFinisherSettings _authoringSettings;

	[SerializeField]
	public KeyboardFinisherSettings _recordingSettings;

	[SerializeField]
	public FinisherAuthoringList _allFinishers;

	public void ApplyState(int stateIndex, HashSet<int> runStates)
	{
		if (stateIndex < 0 || stateIndex >= Configurations.Count)
		{
			Log.BattlegroundsAuthoring.PrintError("EditTimeSceneConfiguration: Attempted to apply state " + stateIndex + " which is undefined.");
			return;
		}
		if (runStates == null)
		{
			runStates = new HashSet<int>();
		}
		if (runStates.Contains(stateIndex))
		{
			Log.BattlegroundsAuthoring.PrintError("EditTimeSceneConfiguration: Infinite recursion detected in state " + stateIndex + ".");
			return;
		}
		runStates.Add(stateIndex);
		if (Configurations[stateIndex].FirstRunState >= 0)
		{
			if (Configurations[stateIndex].FirstRunState >= Configurations.Count)
			{
				Log.BattlegroundsAuthoring.PrintError("EditTimeSceneConfiguration: Attempted during FirstRunState step to apply state " + stateIndex + " which is undefined.");
				return;
			}
			ApplyState(Configurations[stateIndex].FirstRunState, runStates);
		}
		for (int deactivateIndex = 0; deactivateIndex < Configurations[stateIndex].ObjectsToDeactivate.Count; deactivateIndex++)
		{
			if (Configurations[stateIndex].ObjectsToDeactivate[deactivateIndex] == null)
			{
				Log.BattlegroundsAuthoring.PrintWarning($"Attempting to Deactivate Configuration {stateIndex} Object {deactivateIndex} but it is null");
			}
			else
			{
				Configurations[stateIndex].ObjectsToDeactivate[deactivateIndex].SetActive(value: false);
			}
		}
		for (int activateIndex = 0; activateIndex < Configurations[stateIndex].ObjectsToActivate.Count; activateIndex++)
		{
			if (Configurations[stateIndex].ObjectsToActivate[activateIndex] == null)
			{
				Log.BattlegroundsAuthoring.PrintWarning($"Attempting to Activate Configuration {stateIndex} Object {activateIndex} but it is null");
			}
			else
			{
				Configurations[stateIndex].ObjectsToActivate[activateIndex].SetActive(value: true);
			}
		}
		for (int i = 0; i < Configurations[stateIndex].ComponentsToDeactivate.Count; i++)
		{
			if (Configurations[stateIndex].ComponentsToDeactivate[i] == null)
			{
				Log.BattlegroundsAuthoring.PrintWarning($"Attempting to Deactivate Configuration {stateIndex} Component {i} but it is null");
			}
			else
			{
				Configurations[stateIndex].ComponentsToDeactivate[i].enabled = false;
			}
		}
		for (int j = 0; j < Configurations[stateIndex].ComponentsToActivate.Count; j++)
		{
			if (Configurations[stateIndex].ComponentsToActivate[j] == null)
			{
				Log.BattlegroundsAuthoring.PrintWarning($"Attempting to Activate Configuration {stateIndex} Component {j} but it is null");
			}
			else
			{
				Configurations[stateIndex].ComponentsToActivate[j].enabled = true;
			}
		}
	}
}
