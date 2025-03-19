using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Prototyping/Finisher Authoring List")]
public class FinisherAuthoringList : ScriptableObject
{
	public List<FinisherGameplaySettings> Finishers = new List<FinisherGameplaySettings>();
}
