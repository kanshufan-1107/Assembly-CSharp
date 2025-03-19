using Blizzard.T5.MaterialService.Extensions;
using PegasusShared;
using UnityEngine;

public class DebugMissionButton : PegUIElement
{
	public int m_missionId;

	public GameObject m_heroImage;

	public UberText m_name;

	public string m_introline;

	public string m_characterPrefabName;

	private GameObject m_heroPowerObject;

	private bool m_mousedOver;

	private DefLoader.DisposableFullDef m_heroPowerDef;

	private DefLoader.DisposableCardDef m_heroDef;

	private Actor m_heroPowerActor;

	private void Start()
	{
		ScenarioDbfRecord missionRec = GameDbf.Scenario.GetRecord(m_missionId);
		if (missionRec == null)
		{
			Error.AddDevWarning("Error", "scenario {0} does not exist in the DBF", m_missionId);
			return;
		}
		if (m_name != null)
		{
			m_name.Text = missionRec.ShortName;
		}
		string heroCardId = GameUtils.GetMissionHeroCardId(m_missionId);
		if (heroCardId != null)
		{
			DefLoader.Get().LoadCardDef(heroCardId, OnHeroCardDefLoaded);
		}
	}

	protected override void OnDestroy()
	{
		m_heroPowerDef?.Dispose();
		m_heroPowerDef = null;
		m_heroDef?.Dispose();
		m_heroDef = null;
		base.OnDestroy();
	}

	private void OnHeroCardDefLoaded(string cardID, DefLoader.DisposableCardDef cardDef, object userData)
	{
		m_heroDef?.Dispose();
		m_heroDef = cardDef;
		m_heroImage.GetComponent<Renderer>().GetMaterial().mainTexture = m_heroDef.CardDef.GetPortraitTexture(TAG_PREMIUM.NORMAL);
	}

	protected override void OnRelease()
	{
		if (!string.IsNullOrEmpty(m_introline))
		{
			string gameString = new AssetReference(m_introline).GetLegacyAssetName();
			if (string.IsNullOrEmpty(m_characterPrefabName))
			{
				NotificationManager.Get().CreateKTQuote(gameString, m_introline);
			}
			else
			{
				NotificationManager.Get().CreateCharacterQuote(m_characterPrefabName, GameStrings.Get(gameString), m_introline);
			}
		}
		base.OnRelease();
		long deckID = DeckPickerTrayDisplay.Get().GetSelectedDeckID();
		GameMgr.Get().FindGame(GameType.GT_VS_AI, FormatType.FT_WILD, m_missionId, 0, deckID, null, null, restoreSavedGameState: false, null, null, 0L);
		Object.Destroy(base.gameObject);
	}

	protected override void OnOver(InteractionState oldState)
	{
		m_mousedOver = true;
		base.OnOver(oldState);
		if (!string.IsNullOrEmpty(GameUtils.GetMissionHeroPowerCardId(m_missionId)))
		{
			DefLoader.Get().LoadFullDef(GameUtils.GetMissionHeroPowerCardId(m_missionId), OnHeroPowerDefLoaded);
		}
	}

	protected override void OnOut(InteractionState oldState)
	{
		m_mousedOver = false;
		base.OnOut(oldState);
		if ((bool)m_heroPowerActor)
		{
			Object.Destroy(m_heroPowerActor.gameObject);
		}
	}

	private void OnHeroPowerDefLoaded(string cardID, DefLoader.DisposableFullDef def, object userData)
	{
		m_heroPowerDef?.Dispose();
		m_heroPowerDef = def;
		if (m_mousedOver)
		{
			AssetReference assetReference = "History_HeroPower_Opponent.prefab:a99d23d6e8630f94b96a8e096fffb16f";
			if (!AssetLoader.Get().InstantiatePrefab(assetReference, OnHeroPowerActorLoadAttempted, null, AssetLoadingOptions.IgnorePrefabPosition))
			{
				OnHeroPowerActorLoadAttempted(assetReference, null, null);
			}
		}
	}

	private void OnHeroPowerActorLoadAttempted(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (!m_mousedOver)
		{
			Object.Destroy(go);
		}
		if ((bool)m_heroPowerActor)
		{
			Object.Destroy(m_heroPowerActor.gameObject);
		}
		if (this == null || base.gameObject == null)
		{
			Object.Destroy(go);
		}
		if (!(go == null))
		{
			m_heroPowerActor = go.GetComponent<Actor>();
			go.transform.parent = base.gameObject.transform;
			m_heroPowerActor.SetCardDef(m_heroPowerDef.DisposableCardDef);
			m_heroPowerActor.SetEntityDef(m_heroPowerDef.EntityDef);
			m_heroPowerActor.UpdateAllComponents();
			go.transform.position = base.transform.position + new Vector3(15f, 0f, 0f);
			go.transform.localScale = Vector3.one;
			iTween.ScaleTo(go, new Vector3(7f, 7f, 7f), 0.5f);
			LayerUtils.SetLayer(go, GameLayer.Tooltip);
		}
	}
}
