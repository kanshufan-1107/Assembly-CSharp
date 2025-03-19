using UnityEngine;

public class HistoryChildCard : HistoryItem
{
	public void SetCardInfo(Entity entity, DefLoader.DisposableCardDef cardDef, HistoryInfo info)
	{
		m_entity = entity;
		m_portraitTexture = cardDef.CardDef.GetPortraitTexture(m_entity.GetPremiumType());
		m_portraitGoldenMaterial = cardDef.CardDef.GetPremiumPortraitMaterial();
		SetCardDef(cardDef);
		m_splatAmount = info.GetSplatAmount();
		m_dead = info.HasDied();
		m_burned = info.m_isBurnedCard;
		m_isPoisonous = info.m_isPoisonous;
		m_isCriticalHit = info.m_isCriticalHit;
		m_splatType = info.m_splatType;
	}

	public void LoadMainCardActor()
	{
		string prefabPath = ActorNames.GetHistoryActor(m_entity, HistoryInfoType.NONE);
		GameObject actorObject = AssetLoader.Get().InstantiatePrefab(prefabPath, AssetLoadingOptions.IgnorePrefabPosition);
		if (actorObject == null)
		{
			Debug.LogWarningFormat("HistoryChildCard.LoadActorCallback() - FAILED to load actor \"{0}\"", prefabPath);
			return;
		}
		Actor actor = actorObject.GetComponent<Actor>();
		if (actor == null)
		{
			Debug.LogWarningFormat("HistoryChildCard.LoadActorCallback() - ERROR actor \"{0}\" has no Actor component", prefabPath);
			return;
		}
		m_mainCardActor = actor;
		m_mainCardActor.SetPremium(m_entity.GetPremiumType());
		m_mainCardActor.SetWatermarkCardSetOverride(m_entity.GetWatermarkCardSetOverride());
		m_mainCardActor.SetHistoryItem(this);
		m_mainCardActor.UpdateAllComponents();
		LayerUtils.SetLayer(m_mainCardActor.gameObject, GameLayer.Tooltip);
		m_mainCardActor.Hide();
	}
}
