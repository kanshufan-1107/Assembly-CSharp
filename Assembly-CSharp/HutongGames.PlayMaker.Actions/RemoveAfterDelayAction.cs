using System.Collections;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Remove a list of GameObjects after a delay")]
[ActionCategory(ActionCategory.GameObject)]
public class RemoveAfterDelayAction : FsmStateAction
{
	[ArrayEditor(VariableType.GameObject, "", 0, 0, 65536)]
	[Tooltip("GameObjects to remove")]
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
