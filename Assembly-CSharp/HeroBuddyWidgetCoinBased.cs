using UnityEngine;

public class HeroBuddyWidgetCoinBased : HeroBuddyWidgetBase
{
	public Mesh m_Stage2Mesh;

	public Material m_Stage2FrameMaterial;

	public Texture2D m_Stage2HighlightSihouette;

	public int m_portraitFrameMaterialIndex;

	public HighlightState m_highlightState;

	private bool m_initialized;

	private void Init()
	{
		if (m_initialized)
		{
			return;
		}
		GameState gameState = GameState.Get();
		if (gameState == null)
		{
			return;
		}
		Actor actor = base.gameObject.GetComponent<Actor>();
		if (!(actor == null) && actor.GetEntity() != null)
		{
			int numBuddiesGained = ((actor.GetEntity().IsControlledByOpposingSidePlayer() ? gameState.GetOpposingSidePlayer() : gameState.GetFriendlySidePlayer())?.GetHero())?.GetTag(GAME_TAG.BACON_PLAYER_NUM_HERO_BUDDIES_GAINED) ?? 0;
			if (numBuddiesGained == 1)
			{
				EnterStage2();
			}
			else if (numBuddiesGained > 1)
			{
				Debug.LogWarning("HeroBuddyWidgetCoinBased - Initialized into a state ready to be destroyed");
				actor.Destroy();
			}
			m_initialized = true;
		}
	}

	protected override void LateUpdate()
	{
		Init();
		base.LateUpdate();
	}

	public void EnterStage2()
	{
		Actor actor = base.gameObject.GetComponent<Actor>();
		if (actor == null || !actor.IsShown())
		{
			return;
		}
		MeshFilter meshFilter = m_portraitMesh.GetComponent<MeshFilter>();
		if (meshFilter == null || m_Stage2Mesh == null)
		{
			Debug.LogWarning("[HeroBuddyWidgetCoinBased.EnterStage2] - MeshFilter is null");
			return;
		}
		if (meshFilter != null)
		{
			meshFilter.SetMesh(m_Stage2Mesh);
		}
		actor.SetPortraitMaterial(m_Stage2FrameMaterial, m_portraitFrameMaterialIndex);
		if (m_highlightState == null)
		{
			Debug.LogWarning("[HeroBuddyWidgetCoinBased.EnterStage2] - Highlight state is null");
			return;
		}
		m_highlightState.m_StaticSilouetteTexture = m_Stage2HighlightSihouette;
		m_highlightState.ForceUpdate();
	}
}
