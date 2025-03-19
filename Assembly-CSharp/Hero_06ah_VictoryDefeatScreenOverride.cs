using UnityEngine;

[CustomEditClass]
public class Hero_06ah_VictoryDefeatScreenOverride : MonoBehaviour
{
	[CustomEditField(T = EditType.GAME_OBJECT)]
	public string VictoryScreenPrefab;

	[CustomEditField(T = EditType.GAME_OBJECT)]
	public string DefeatScreenPrefab;

	private string m_victoryScreenDefaultPrefabPath;

	private string m_defeatScreenDefaultPrefabPath;

	private Actor m_heroActor;

	private void OnEnable()
	{
		GameState gameState = GameState.Get();
		if (gameState == null)
		{
			return;
		}
		GameEntity gameEntity = gameState.GetGameEntity();
		if (gameEntity == null)
		{
			return;
		}
		m_heroActor = LegendaryUtil.FindLegendaryHeroActor(base.gameObject);
		if (m_heroActor == null)
		{
			return;
		}
		Card card = m_heroActor.GetCard();
		if ((object)card != null && card.GetControllerSide() == Player.Side.FRIENDLY)
		{
			if (!string.IsNullOrEmpty(VictoryScreenPrefab))
			{
				m_victoryScreenDefaultPrefabPath = gameEntity.GetGameOptions().GetStringOption(GameEntityOption.VICTORY_SCREEN_PREFAB_PATH);
				gameEntity.GetGameOptions().SetStringOption(GameEntityOption.VICTORY_SCREEN_PREFAB_PATH, VictoryScreenPrefab);
			}
			if (!string.IsNullOrEmpty(DefeatScreenPrefab))
			{
				m_defeatScreenDefaultPrefabPath = gameEntity.GetGameOptions().GetStringOption(GameEntityOption.DEFEAT_SCREEN_PREFAB_PATH);
				gameEntity.GetGameOptions().SetStringOption(GameEntityOption.DEFEAT_SCREEN_PREFAB_PATH, DefeatScreenPrefab);
			}
		}
	}

	private void OnDisable()
	{
		GameState gameState = GameState.Get();
		if (gameState == null)
		{
			return;
		}
		GameEntity gameEntity = gameState.GetGameEntity();
		if (gameEntity == null || m_heroActor == null)
		{
			return;
		}
		Card card = m_heroActor.GetCard();
		if ((object)card != null && card.GetControllerSide() == Player.Side.FRIENDLY)
		{
			if (!string.IsNullOrEmpty(m_victoryScreenDefaultPrefabPath))
			{
				gameEntity.GetGameOptions().SetStringOption(GameEntityOption.VICTORY_SCREEN_PREFAB_PATH, m_victoryScreenDefaultPrefabPath);
			}
			if (!string.IsNullOrEmpty(m_defeatScreenDefaultPrefabPath))
			{
				gameEntity.GetGameOptions().SetStringOption(GameEntityOption.DEFEAT_SCREEN_PREFAB_PATH, m_defeatScreenDefaultPrefabPath);
			}
		}
	}
}
