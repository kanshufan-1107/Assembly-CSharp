using UnityEngine;

public class LettuceMercNominationToastTextPicker : MonoBehaviour
{
	public enum TextState
	{
		Nominate,
		Replace,
		Reorder
	}

	public TextState GetToastTextState()
	{
		GameState gameState = GameState.Get();
		if (gameState == null)
		{
			return TextState.Nominate;
		}
		GameEntity gameEntity = gameState.GetGameEntity();
		if (gameEntity == null)
		{
			return TextState.Nominate;
		}
		if (gameEntity.GetTag(GAME_TAG.TURN) > 1)
		{
			Player player = gameState.GetLocalSidePlayer();
			if (player != null)
			{
				if (player.HasTag(GAME_TAG.LETTUCE_MERCENARIES_TO_NOMINATE))
				{
					return TextState.Replace;
				}
				return TextState.Reorder;
			}
		}
		return TextState.Nominate;
	}
}
