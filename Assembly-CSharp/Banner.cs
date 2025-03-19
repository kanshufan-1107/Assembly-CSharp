using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core.Utils;
using UnityEngine;

public class Banner : MonoBehaviour
{
	[Header("Default Banner")]
	public UberText m_headline;

	public UberText m_caption;

	public GameObject m_bannerDefault;

	public UberText m_headlineDefault;

	public UberText m_captionDefault;

	public GameObject m_glowObject;

	[Header("Murloc Holmes Banner")]
	public GameObject m_bannerMurlocHolmes;

	public UberText m_headlineMurlocHolmes;

	public UberText m_captionMurlocHolmes;

	[Header("Suspicious Banner")]
	public GameObject m_bannerSuspicious;

	public UberText m_headlineSuspicious;

	public UberText m_captionSuspicious;

	[Header("Bacon Anomaly Banner")]
	public GameObject m_bannerBaconAnomaly;

	public UberText m_headlineBaconAnomaly;

	public UberText m_captionBaconAnomaly;

	public GameObject m_handBaconAnomaly;

	public GameObject m_playBaconAnomaly;

	public PlayMakerFSM m_fsmBaconAnomaly;

	private bool m_initializedBGAnomalies;

	[Header("Bacon Buddies Banner")]
	public GameObject m_bannerBaconBuddies;

	[Header("Bacon Quests Banner")]
	public GameObject m_bannerBaconQuests;

	[Header("Bacon Trinkets Banner")]
	public GameObject m_bannerBaconTrinkets;

	[Header("HearthStone Anomaly Settings")]
	public AnomalyMedallion m_medallionHearthstoneAnomaly;

	public PlayMakerFSM m_fsmHearthstoneAnomaly;

	public List<GameObject> m_singleHearthstoneAnomalyBone = new List<GameObject>();

	public List<GameObject> m_doubleHearthstoneAnomalyBones = new List<GameObject>();

	[Header("Algalon Settings")]
	public GameObject m_AlgalonsVisionContent;

	public Transform m_boneLeftCardAlgalonsVision;

	private const string MURLOC_HOLMES = "REV_022";

	private const string SUSPICOUS_CARD = "REV_000e";

	private const string MURLOC_HOMES_BACON_HEROPOWER = "BG23_HERO_303p2";

	private const string ALGALONS_VISION = "TTN_717t";

	private int m_pendingLoadingAnomalyCards;

	public void SetText(string headline)
	{
		m_headline.Text = headline;
		m_caption.gameObject.SetActive(value: false);
	}

	public void SetText(string headline, string caption)
	{
		m_headline.Text = headline;
		m_caption.gameObject.SetActive(value: true);
		m_caption.Text = caption;
	}

	public void MoveGlowForBottomPlacement()
	{
		m_glowObject.transform.localPosition = new Vector3(m_glowObject.transform.localPosition.x, m_glowObject.transform.localPosition.y, 0f);
	}

	public void SetupBanner(int sourceEntityId, List<Card> cards, bool isSubOptionChoice)
	{
		Entity sourceEntity = GameState.Get().GetEntity(sourceEntityId);
		string cardID = sourceEntity.GetCardId();
		if (m_bannerBaconBuddies != null)
		{
			m_bannerBaconBuddies.SetActive(value: false);
		}
		if (m_bannerBaconQuests != null)
		{
			m_bannerBaconQuests.SetActive(value: false);
		}
		if (m_bannerBaconTrinkets != null)
		{
			m_bannerBaconTrinkets.SetActive(value: false);
		}
		switch (cardID)
		{
		case "REV_022":
		{
			m_bannerDefault.SetActive(value: false);
			m_bannerMurlocHolmes.SetActive(value: true);
			m_headline = m_headlineMurlocHolmes;
			m_caption = m_captionMurlocHolmes;
			string bannerText3 = "";
			int clue = sourceEntity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1);
			switch (clue)
			{
			case 1:
				bannerText3 = GameStrings.Get("GAMEPLAY_MURLOC_HOLMES_QUESTION_1");
				break;
			case 2:
				bannerText3 = GameStrings.Get("GAMEPLAY_MURLOC_HOLMES_QUESTION_2");
				break;
			case 3:
				bannerText3 = GameStrings.Get("GAMEPLAY_MURLOC_HOLMES_QUESTION_3");
				break;
			}
			string captionText3 = string.Format("{0} {1}/3", GameStrings.Get("GAMEPLAY_MURLOC_HOLMES_CLUE"), clue.ToString());
			SetText(bannerText3, captionText3);
			return;
		}
		case "REV_000e":
		{
			m_bannerDefault.SetActive(value: false);
			m_bannerSuspicious.SetActive(value: true);
			m_headline = m_headlineSuspicious;
			m_caption = m_captionSuspicious;
			string bannerText = GameStrings.Get("GAMEPLAY_SUSPICIOUS_GUESS_HEADLINE");
			string captionText = "";
			SetText(bannerText, captionText);
			return;
		}
		case "TTN_717t":
		{
			m_AlgalonsVisionContent.SetActive(value: true);
			int entityID = sourceEntity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1);
			Entity card = GameState.Get().GetEntity(entityID);
			AssetReference assetRef2 = ActorNames.GetHandActor(card.GetEntityDef(), card.GetPremiumType());
			AssetLoader.Get().InstantiatePrefab(assetRef2, delegate(AssetReference assetRef, GameObject go, object callbackData)
			{
				Actor component = go.GetComponent<Actor>();
				Entity entity = (Entity)callbackData;
				component.SetEntity(entity);
				component.SetCardDefFromEntity(entity);
				component.SetCardBackSideOverride(entity.GetControllerSide());
				component.UpdateAllComponents();
				go.transform.parent = m_AlgalonsVisionContent.transform;
				go.transform.position = m_boneLeftCardAlgalonsVision.position;
				go.transform.rotation = m_boneLeftCardAlgalonsVision.rotation;
				go.transform.localScale = m_boneLeftCardAlgalonsVision.localScale;
			}, card);
			string bannerText4 = GameStrings.Get("GAMEPLAY_CHOOSE_ONE");
			SetText(bannerText4);
			return;
		}
		case "BG23_HERO_303p2":
		{
			m_bannerDefault.SetActive(value: false);
			m_bannerMurlocHolmes.SetActive(value: true);
			m_headline = m_headlineMurlocHolmes;
			m_caption = m_captionMurlocHolmes;
			string bannerText2 = GameStrings.Get("GAMEPLAY_MURLOC_HOLMES_WARBAND_QUESTION");
			string captionText2 = GameStrings.Get("GAMEPLAY_MURLOC_HOLMES_CLUE");
			SetText(bannerText2, captionText2);
			return;
		}
		}
		if (sourceEntity.IsLaunchpad())
		{
			string bannerText5 = GameStrings.Get("GAMEPLAY_STARSHIP_PIECES");
			string captionText4 = "";
			SetText(bannerText5, captionText4);
			return;
		}
		m_bannerDefault.SetActive(value: true);
		m_bannerMurlocHolmes.SetActive(value: false);
		m_headline = m_headlineDefault;
		m_caption = m_captionDefault;
		string bannerText6 = GameState.Get().GetGameEntity().CustomChoiceBannerText();
		if (bannerText6 == null)
		{
			if (sourceEntity != null)
			{
				string customBannerTextStringId = GameDbf.GetIndex().GetCardDiscoverString(sourceEntity.GetCardId());
				if (customBannerTextStringId != null)
				{
					bannerText6 = GameStrings.Get(customBannerTextStringId);
				}
			}
			if (bannerText6 == null)
			{
				bannerText6 = ((!((sourceEntity?.IsTitan() ?? false) && isSubOptionChoice)) ? GameStrings.Get("GAMEPLAY_CHOOSE_ONE") : GameStrings.Get("GAMEPLAY_CHOOSE_AN_ABILITY"));
				foreach (Card card2 in cards)
				{
					if (null != card2 && card2.GetEntity().IsHeroPower())
					{
						bannerText6 = GameStrings.Get("GAMEPLAY_CHOOSE_ONE_HERO_POWER");
						break;
					}
				}
			}
		}
		SetText(bannerText6);
	}

	public void SetBaconBannerText(string text, string desc)
	{
		m_bannerDefault.SetActive(value: false);
		m_bannerBaconAnomaly.SetActive(value: true);
		m_headlineBaconAnomaly.Text = text;
		if (desc != null)
		{
			m_captionBaconAnomaly.Text = desc;
		}
	}

	public void SetBaconAnomalyBannerText(string text, string desc, int anomalyDBID)
	{
		m_bannerDefault.SetActive(value: false);
		m_bannerBaconAnomaly.SetActive(value: true);
		m_headlineBaconAnomaly.Text = text;
		if (desc != null)
		{
			m_captionBaconAnomaly.Text = desc;
		}
		LoadBGAnomalyActors(anomalyDBID);
	}

	public void SetBaconBuddiesBanner()
	{
		if (m_bannerBaconBuddies != null)
		{
			m_bannerBaconBuddies.SetActive(value: true);
		}
		if (m_bannerBaconQuests != null)
		{
			m_bannerBaconQuests.SetActive(value: false);
		}
		if (m_bannerBaconTrinkets != null)
		{
			m_bannerBaconTrinkets.SetActive(value: false);
		}
	}

	public void SetBaconTrinketBanner()
	{
		if (m_bannerBaconTrinkets != null)
		{
			m_bannerBaconTrinkets.SetActive(value: true);
		}
		if (m_bannerBaconBuddies != null)
		{
			m_bannerBaconBuddies.SetActive(value: false);
		}
		if (m_bannerBaconQuests != null)
		{
			m_bannerBaconQuests.SetActive(value: false);
		}
	}

	public void SetBaconQuestsBanner()
	{
		if (m_bannerBaconQuests != null)
		{
			m_bannerBaconQuests.SetActive(value: true);
		}
		if (m_bannerBaconBuddies != null)
		{
			m_bannerBaconBuddies.SetActive(value: false);
		}
		if (m_bannerBaconTrinkets != null)
		{
			m_bannerBaconTrinkets.SetActive(value: false);
		}
	}

	private void LoadBGAnomalyActors(int anomalyDBID)
	{
		if (m_initializedBGAnomalies)
		{
			return;
		}
		AssetReference assetRef1 = ActorNames.GetNameWithPremiumType(ActorNames.ACTOR_ASSET.BIG_CARD_BG_ANOMALY, TAG_PREMIUM.NORMAL);
		AssetLoader.Get().InstantiatePrefab(assetRef1, OnHandBGAnomalyActorLoaded, anomalyDBID, AssetLoadingOptions.IgnorePrefabPosition);
		m_pendingLoadingAnomalyCards++;
		using (DefLoader.DisposableFullDef anomalyCardDef = DefLoader.Get().GetFullDef(anomalyDBID))
		{
			if (anomalyCardDef != null)
			{
				int evolingCardID = anomalyCardDef.EntityDef.GetTag(GAME_TAG.BACON_EVOLUTION_CARD_ID);
				if (evolingCardID != 0)
				{
					using (DefLoader.DisposableFullDef evolvingCardDef = DefLoader.Get().GetFullDef(evolingCardID))
					{
						string evolvingCardAssetRef = ActorNames.GetHandActor(evolvingCardDef?.EntityDef, TAG_PREMIUM.NORMAL);
						AssetLoader.Get().InstantiatePrefab(evolvingCardAssetRef, OnBGAnomalyRelatedActorLoaded, evolingCardID, AssetLoadingOptions.IgnorePrefabPosition);
					}
					m_pendingLoadingAnomalyCards++;
				}
			}
		}
		AssetReference assetRef2 = ActorNames.GetNameWithPremiumType(ActorNames.ACTOR_ASSET.PLAY_BATTLEGROUND_ANOMALY, TAG_PREMIUM.NORMAL);
		AssetLoader.Get().InstantiatePrefab(assetRef2, OnPlayBGAnomalyActorLoaded, anomalyDBID, AssetLoadingOptions.IgnorePrefabPosition);
		m_pendingLoadingAnomalyCards++;
		m_initializedBGAnomalies = true;
	}

	private void OnHandBGAnomalyActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		int databaseID = (int)callbackData;
		Actor actor = go.GetComponent<Actor>();
		if (actor == null || databaseID == 0)
		{
			return;
		}
		using (DefLoader.DisposableFullDef entityfullDef = DefLoader.Get().GetFullDef(databaseID))
		{
			if (entityfullDef == null)
			{
				return;
			}
			actor.SetFullDef(entityfullDef);
		}
		actor.UpdateAllComponents();
		actor.Show();
		go.transform.parent = m_handBaconAnomaly.transform;
		go.transform.localPosition = Vector3.zero;
		LayerUtils.SetLayer(go, GameLayer.Tooltip);
		m_pendingLoadingAnomalyCards--;
	}

	private void OnBGAnomalyRelatedActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		int databaseID = (int)callbackData;
		Actor actor = go.GetComponent<Actor>();
		using (DefLoader.DisposableFullDef entityfullDef = DefLoader.Get().GetFullDef(databaseID))
		{
			actor.SetFullDef(entityfullDef);
			if (entityfullDef != null && entityfullDef.EntityDef.GetTag(GAME_TAG.TECH_LEVEL) != 0)
			{
				actor.m_manaObject.SetActive(value: false);
				Spell techLevelSpell = actor.GetSpell(SpellType.TECH_LEVEL_MANA_GEM);
				if (techLevelSpell != null)
				{
					techLevelSpell.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmInt("TechLevel").Value = actor.GetEntityDef().GetTechLevel();
					techLevelSpell.ActivateState(SpellStateType.BIRTH);
				}
			}
		}
		GameObject EvolutionVFX = GameObjectUtils.FindChildBySubstring(go, "EvolutionVFX");
		if (EvolutionVFX != null)
		{
			EvolutionVFX.SetActive(value: true);
		}
		actor.UpdateAllComponents();
		actor.Show();
		go.transform.parent = m_handBaconAnomaly.transform;
		go.transform.localPosition = new Vector3(1.4f, 0f, 0f);
		LayerUtils.SetLayer(go, GameLayer.Tooltip);
		m_pendingLoadingAnomalyCards--;
	}

	private void OnPlayBGAnomalyActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		int databaseID = (int)callbackData;
		Actor actor = go.GetComponent<Actor>();
		if (actor == null)
		{
			return;
		}
		using (DefLoader.DisposableFullDef entityfullDef = DefLoader.Get().GetFullDef(databaseID))
		{
			if (entityfullDef == null)
			{
				return;
			}
			actor.SetFullDef(entityfullDef);
		}
		actor.UpdateAllComponents();
		actor.Show();
		AnomalyWidget anomalyWidget = actor.GetComponent<AnomalyWidget>();
		if (anomalyWidget != null)
		{
			anomalyWidget.m_popup.m_targetPopup = m_handBaconAnomaly;
			anomalyWidget.m_popup.SetPopupState(enabled: false);
		}
		go.transform.parent = m_playBaconAnomaly.transform;
		go.transform.localPosition = Vector3.zero;
		m_pendingLoadingAnomalyCards--;
	}

	public void SetHearthstoneAnomaliesBannerText(string text, string desc, List<int> anomalyEntIDs)
	{
		m_headline.Text = text;
		m_caption.gameObject.SetActive(value: true);
		m_caption.Text = desc;
		m_medallionHearthstoneAnomaly.InitializeAnomalyUI(anomalyEntIDs, showSpawnSpell: false);
		if (anomalyEntIDs.Count == 1)
		{
			m_medallionHearthstoneAnomaly.SetPositionsFromBones(m_singleHearthstoneAnomalyBone);
		}
		else if (anomalyEntIDs.Count == 2)
		{
			m_medallionHearthstoneAnomaly.SetPositionsFromBones(m_doubleHearthstoneAnomalyBones);
		}
	}

	public IEnumerator ShowBaconAnomalyIntro()
	{
		m_fsmBaconAnomaly.SendEvent("Hide");
		while (m_pendingLoadingAnomalyCards > 0)
		{
			yield return new WaitForSecondsRealtime(0.1f);
		}
		m_fsmBaconAnomaly.SendEvent("Show");
		while (m_fsmBaconAnomaly.ActiveStateName != "End")
		{
			yield return new WaitForSecondsRealtime(0.1f);
		}
	}

	public IEnumerator ShowHearthstoneAnomalyIntro(List<int> anomalies)
	{
		m_medallionHearthstoneAnomaly.SetAnimating(isAnimating: true);
		while (m_medallionHearthstoneAnomaly.IsLoading())
		{
			yield return null;
		}
		if (anomalies.Count == 1)
		{
			m_fsmHearthstoneAnomaly.SendEvent("Show 1");
		}
		else if (anomalies.Count == 2)
		{
			m_fsmHearthstoneAnomaly.SendEvent("Show 2");
		}
		while (m_fsmHearthstoneAnomaly.ActiveStateName != "End")
		{
			yield return null;
		}
		m_medallionHearthstoneAnomaly.SetAnimating(isAnimating: false);
	}
}
