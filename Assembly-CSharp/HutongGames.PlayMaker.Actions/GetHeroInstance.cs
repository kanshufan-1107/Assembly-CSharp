using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Find a hero instance on the board.")]
public class GetHeroInstance : FsmStateAction
{
	private enum HeroType
	{
		Friendly,
		Opponent
	}

	private enum TargetObject
	{
		CardActor,
		Parent,
		RootChild
	}

	[UIHint(UIHint.FsmEnum)]
	[ObjectType(typeof(HeroType))]
	[Tooltip("Which hero are we looking for?")]
	public FsmEnum type;

	[Tooltip("Which object within the hero are we looking for?")]
	[ObjectType(typeof(TargetObject))]
	[UIHint(UIHint.FsmEnum)]
	public FsmEnum targetObject;

	[RequiredField]
	[Tooltip("Store the hero in this variable.")]
	[UIHint(UIHint.Variable)]
	public FsmGameObject output;

	public override void Reset()
	{
		type = new FsmEnum
		{
			Value = HeroType.Opponent
		};
		targetObject = new FsmEnum
		{
			Value = TargetObject.CardActor
		};
		output = new FsmGameObject
		{
			UseVariable = true
		};
	}

	public override void OnEnter()
	{
		DoAction();
		Finish();
	}

	private void DoAction()
	{
		if (output.IsNone || !output.UseVariable)
		{
			Debug.LogWarning("No output selected.");
			return;
		}
		PlayerLeaderboardMainCardActor[] actors = Object.FindObjectsOfType<PlayerLeaderboardMainCardActor>();
		int typeInt = type.ToInt();
		int index = -1;
		for (int i = 0; i < actors.Length; i++)
		{
			string name = actors[i].name.ToLower();
			switch (typeInt)
			{
			case 0:
				if (!name.Contains("friendly"))
				{
					continue;
				}
				index = i;
				break;
			case 1:
				if (!name.Contains("opponent") && !name.Contains("enemy"))
				{
					continue;
				}
				index = i;
				break;
			default:
				continue;
			}
			break;
		}
		if (index >= 0)
		{
			switch (targetObject.ToInt())
			{
			case 1:
			{
				Transform parent = actors[index].transform.parent;
				if (parent != null)
				{
					output.Value = parent.gameObject;
					break;
				}
				Debug.LogError("Hero instance does not have a parent.");
				output.Value = null;
				break;
			}
			case 2:
			{
				for (int j = 0; j < actors[index].transform.childCount; j++)
				{
					if (actors[index].transform.GetChild(j).name.ToLower().Contains("root"))
					{
						output.Value = actors[index].transform.GetChild(j).gameObject;
						return;
					}
				}
				Debug.LogError("Hero instance does not have a root child.");
				output.Value = null;
				break;
			}
			default:
				output.Value = actors[index].gameObject;
				break;
			}
		}
		else
		{
			Debug.LogError("Could not find hero instance.");
			output.Value = null;
		}
	}
}
