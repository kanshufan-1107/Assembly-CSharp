using System;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class QuestRewardWidget : MonoBehaviour
{
	public MeshRenderer m_portraitMesh;

	public int portraitIndex;

	public float m_tilingX = 0.69f;

	public float m_tilingY = 0.69f;

	public float m_offsetX = 0.17f;

	public float m_offsetY = 0.2f;

	private bool m_warningSent;

	private Actor m_actor;

	private void Awake()
	{
		m_actor = base.gameObject.GetComponent<Actor>();
		if (m_actor != null)
		{
			Actor actor = m_actor;
			actor.OnPortraitMaterialUpdated = (Action)Delegate.Combine(actor.OnPortraitMaterialUpdated, new Action(OnPortraitMaterialUpdated));
		}
		else
		{
			Debug.LogWarning("QuestRewardWidget - Is missing an Actor Component");
		}
	}

	private void OnDestroy()
	{
		if (m_actor != null)
		{
			Actor actor = m_actor;
			actor.OnPortraitMaterialUpdated = (Action)Delegate.Remove(actor.OnPortraitMaterialUpdated, new Action(OnPortraitMaterialUpdated));
		}
	}

	private void OnPortraitMaterialUpdated()
	{
		using DefLoader.DisposableCardDef cardDef = m_actor.GetCard()?.ShareDisposableCardDef();
		UpdatePortrait(cardDef);
	}

	private void UpdatePortrait(DefLoader.DisposableCardDef disposableCardDef)
	{
		if (!m_actor.IsShown() || disposableCardDef == null)
		{
			return;
		}
		Material questRewardMat = disposableCardDef.CardDef.GetBattlegroundsQuestRewardPortraitMaterial();
		Material portraitMaterial = m_portraitMesh.GetMaterials()[portraitIndex];
		if (questRewardMat == null)
		{
			if (!m_warningSent)
			{
				Debug.LogWarning("QuestRewardWidget.UpdatePortrait() - Missing quest reward Mat");
				m_warningSent = true;
			}
			SetupDefaultPortraitMaterial(portraitMaterial);
		}
		else
		{
			portraitMaterial.mainTexture = questRewardMat.mainTexture;
			Texture shadowTexture = portraitMaterial.GetTexture("_SecondTex");
			portraitMaterial.CopyPropertiesFromMaterial(questRewardMat);
			portraitMaterial.SetTexture("_SecondTex", shadowTexture);
		}
	}

	private void SetupDefaultPortraitMaterial(Material portraitMaterial)
	{
		portraitMaterial.SetTextureOffset("_MainTex", new Vector2(m_offsetX, m_offsetY));
		portraitMaterial.SetTextureScale("_MainTex", new Vector2(m_tilingX, m_tilingY));
	}
}
