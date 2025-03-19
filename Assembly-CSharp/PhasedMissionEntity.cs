using UnityEngine;

public class PhasedMissionEntity : MissionEntity
{
	public string m_PopupPrefabNameAndGUID = "PhaseProgress_Next.prefab:7013b28700033444c9f20897a59edaa0";

	public string m_PopupText = "GAMEPLAY_RESTART_PUZZLES";

	private ScreenEffectsHandle m_screenEffectsHandle;

	public override void NotifyOfGameOver(TAG_PLAYSTATE gameResult)
	{
		if (GameState.Get().GetGameEntity().GetTag(GAME_TAG.PHASED_RESTART) == 1)
		{
			PhaseComplete();
			GameState.Get().Restart();
		}
		else
		{
			base.NotifyOfGameOver(gameResult);
		}
	}

	public virtual void PhaseComplete()
	{
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
		m_screenEffectsHandle.StartEffect(ScreenEffectParameters.DesaturatePerspective);
		GameObject popup = AssetLoader.Get().InstantiatePrefab(m_PopupPrefabNameAndGUID, AssetLoadingOptions.IgnorePrefabPosition);
		UberText[] componentsInChildren = popup.GetComponentsInChildren<UberText>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SetText(GameStrings.Get(m_PopupText));
		}
		popup = AssetLoader.Get().InstantiatePrefab(m_PopupPrefabNameAndGUID, AssetLoadingOptions.IgnorePrefabPosition);
		componentsInChildren = popup.GetComponentsInChildren<UberText>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SetText(GameStrings.Get(m_PopupText));
		}
		LayerUtils.SetLayer(popup, 0, null);
	}
}
