using UnityEngine;

public class ManaCounter : MonoBehaviour
{
	public Player.Side m_Side;

	public GameObject m_phoneGemContainer;

	public UberText m_availableManaPhone;

	public UberText m_permanentManaPhone;

	private Player m_player;

	private UberText m_textMesh;

	private GameObject m_phoneGem;

	private bool m_disableManaTextUpdate;

	private void Awake()
	{
		m_textMesh = GetComponent<UberText>();
		if (m_Side != Player.Side.FRIENDLY)
		{
			if (m_availableManaPhone != null)
			{
				string errorMessage = "The property m_availableManaPhone is set on ManaCounter for non-friendly mana crystals. This should be null.";
				SceneDebugger.Get().AddErrorMessage(errorMessage);
			}
			if (m_permanentManaPhone != null)
			{
				string errorMessage2 = "The property m_permanentManaPhone is set on ManaCounter for non-friendly mana crystals. This should be null.";
				SceneDebugger.Get().AddErrorMessage(errorMessage2);
			}
		}
	}

	private void Start()
	{
		m_textMesh.Text = GameStrings.Format("GAMEPLAY_MANA_COUNTER", "0", "0");
	}

	public void InitializeLargeResourceGameObject(string resourcePath)
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			if (m_phoneGem != null)
			{
				Object.Destroy(m_phoneGem);
			}
			m_phoneGem = AssetLoader.Get().InstantiatePrefab(resourcePath, AssetLoadingOptions.IgnorePrefabPosition);
			GameUtils.SetParent(m_phoneGem, m_phoneGemContainer, withRotation: true);
		}
	}

	public void SetPlayer(Player player)
	{
		m_player = player;
	}

	public Player GetPlayer()
	{
		return m_player;
	}

	public GameObject GetPhoneGem()
	{
		return m_phoneGem;
	}

	public void ToggleManaCrystalTextUpdate(bool enable)
	{
		m_disableManaTextUpdate = !enable;
		if (m_availableManaPhone != null)
		{
			m_availableManaPhone.enabled = enable;
		}
		if (m_permanentManaPhone != null)
		{
			m_availableManaPhone.enabled = enable;
		}
	}

	public void UpdateText(int current, int max)
	{
		if (base.gameObject.activeInHierarchy)
		{
			string text = ((!UniversalInputManager.UsePhoneUI || max < 10) ? GameStrings.Format("GAMEPLAY_MANA_COUNTER", current, max) : current.ToString());
			m_textMesh.Text = text;
			if ((bool)UniversalInputManager.UsePhoneUI && m_availableManaPhone != null && m_Side == Player.Side.FRIENDLY)
			{
				m_availableManaPhone.Text = current.ToString();
				m_permanentManaPhone.Text = max.ToString();
			}
		}
	}

	public void UpdateText()
	{
		if (base.gameObject.activeInHierarchy && !m_disableManaTextUpdate)
		{
			int permanentResources = m_player.GetTag(GAME_TAG.RESOURCES);
			if (!base.gameObject.activeInHierarchy)
			{
				base.gameObject.SetActive(value: true);
			}
			int availableResources = m_player.GetNumAvailableResources();
			string text = ((!UniversalInputManager.UsePhoneUI || permanentResources < 10) ? GameStrings.Format("GAMEPLAY_MANA_COUNTER", availableResources, permanentResources) : availableResources.ToString());
			m_textMesh.Text = text;
			if ((bool)UniversalInputManager.UsePhoneUI && m_availableManaPhone != null && m_Side == Player.Side.FRIENDLY)
			{
				m_availableManaPhone.Text = availableResources.ToString();
				m_permanentManaPhone.Text = permanentResources.ToString();
			}
		}
	}
}
