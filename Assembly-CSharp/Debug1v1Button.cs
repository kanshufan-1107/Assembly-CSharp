using Blizzard.T5.MaterialService.Extensions;
using PegasusShared;
using UnityEngine;

public class Debug1v1Button : PegUIElement
{
	public int m_missionId;

	public GameObject m_heroImage;

	public UberText m_name;

	private GameObject m_heroPowerObject;

	public static bool HasUsedDebugMenu { get; set; }

	private void Start()
	{
		ScenarioDbfRecord missionRec = GameDbf.Scenario.GetRecord(m_missionId);
		if (missionRec != null)
		{
			string dbfName = missionRec.ShortName;
			if (m_name != null && !string.IsNullOrEmpty(dbfName))
			{
				m_name.Text = dbfName;
			}
		}
	}

	private void OnCardDefLoaded(string cardID, CardDef cardDef, object userData)
	{
		m_heroImage.GetComponent<Renderer>().GetMaterial().mainTexture = cardDef.GetPortraitTexture(TAG_PREMIUM.NORMAL);
	}

	protected override void OnRelease()
	{
		base.OnRelease();
		long deckID = DeckPickerTrayDisplay.Get().GetSelectedDeckID();
		HasUsedDebugMenu = true;
		GameMgr.Get().FindGame(GameType.GT_TAVERNBRAWL, FormatType.FT_WILD, m_missionId, 0, deckID, null, null, restoreSavedGameState: false, null, null, 0L);
		Object.Destroy(base.transform.parent.gameObject);
	}
}
