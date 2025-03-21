using System.Collections;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory(ActionCategory.GameObject)]
[Tooltip("Remove a list of GameObjects after a delay")]
public class RemoveAfterDelayAction : FsmStateAction
{
	[Tooltip("GameObjects to remove")]
	[ArrayEditor(VariableType.GameObject, "", 0, 0, 65536)]
	[UIHint(UIHint.Variable)]
	public FsmArray gameObjectList;

	[Tooltip("Wait time before removal")]
	[RequiredField]
	public FsmFloat waitTime;

	private MonoBehaviour coroutineParent;

	public override void OnEnter()
	{
		if (gameObjectList.Length != 0)
		{
			GameObject dummy = new GameObject();
			coroutineParent = dummy.AddComponent<EmptyScript>();
			coroutineParent.StartCoroutine(WaitAndRemove());
		}
	}

	private IEnumerator WaitAndRemove()
	{
		yield return new WaitForSecondsRealtime(waitTime.Value);
		object[] values = gameObjectList.Values;
		for (int i = 0; i < values.Length; i++)
		{
			Object.Destroy((GameObject)values[i]);
		}
		Object.Destroy(coroutineParent.gameObject);
	}
}
