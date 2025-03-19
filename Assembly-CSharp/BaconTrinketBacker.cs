using System.Collections;
using UnityEngine;

public class BaconTrinketBacker : MonoBehaviour
{
	public UberText m_coinCost;

	public UberText m_coinText;

	public GameObject m_coinObject;

	public bool m_showCostObject;

	public PlayMakerFSM m_playmaker;

	public bool m_isTeammate;

	private Coroutine m_updateCoroutine;

	public void UpdateCoinText(int cost = 0)
	{
		if (PlatformSettings.Screen != ScreenCategory.Phone)
		{
			m_coinObject.SetActive(value: false);
			return;
		}
		m_coinObject.SetActive(value: false);
		Player friendlyPlayer = GameState.Get().GetFriendlySidePlayer();
		if (friendlyPlayer != null)
		{
			m_coinText.Text = GameStrings.Format("GAMEPLAY_MANA_COUNTER", friendlyPlayer.GetNumAvailableResources(), friendlyPlayer.GetTag(GAME_TAG.RESOURCES));
		}
		if (!m_showCostObject)
		{
			m_coinCost.gameObject.SetActive(value: false);
			return;
		}
		if (cost == 0)
		{
			m_coinCost.gameObject.SetActive(value: false);
			return;
		}
		m_coinCost.Text = (-1 * cost).ToString();
		m_coinCost.gameObject.SetActive(value: true);
	}

	private void Start()
	{
		UpdateCoinText();
	}

	public void UpdateTrinketState(bool turnLeftChanged = false, bool isPotentialTrinketChanged = false, int turnsLeftToDiscover = -1)
	{
		if (m_updateCoroutine != null)
		{
			StopCoroutine(m_updateCoroutine);
		}
		m_updateCoroutine = StartCoroutine(UpdateVFX(turnLeftChanged, isPotentialTrinketChanged, turnsLeftToDiscover));
	}

	private IEnumerator UpdateVFX(bool turnLeftChanged = false, bool isPotentialTrinketChanged = false, int turnsLeftToDiscover = -1)
	{
		yield break;
	}

	private void OnDestroy()
	{
		if ((bool)UniversalInputManager.UsePhoneUI && ManaCrystalMgr.Get() != null)
		{
			GameObject manaCounter = (m_isTeammate ? ManaCrystalMgr.Get().teammateManaCounter : ManaCrystalMgr.Get().friendlyManaCounter);
			if (manaCounter != null)
			{
				LayerUtils.SetLayer(manaCounter, GameLayer.Default);
			}
		}
	}

	public void Show(bool show)
	{
		if (!show && m_playmaker != null)
		{
			m_playmaker.SendEvent("hide");
		}
		base.gameObject.SetActive(show);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			GameObject manaCounter = (m_isTeammate ? ManaCrystalMgr.Get().teammateManaCounter : ManaCrystalMgr.Get().friendlyManaCounter);
			if (manaCounter != null)
			{
				LayerUtils.SetLayer(manaCounter, show ? GameLayer.Tooltip : GameLayer.Default);
			}
		}
		if (show && m_playmaker != null)
		{
			m_playmaker.SendEvent("show");
		}
	}
}
