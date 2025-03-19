using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class CraftingButton : PegUIElement
{
	public enum CraftingState
	{
		Disabled,
		Create,
		Disenchant,
		Undo,
		CreateUpgrade,
		Upgrade
	}

	public Material undoMaterial;

	public Material disabledMaterial;

	public Material enabledMaterial;

	public Material upgradeMaterial;

	public Material disabledDustJarMaterial;

	public Material enabledDustJarMaterial;

	public UberText labelText;

	public UberText buttonDisabledLabelText;

	public UberText costText;

	public MeshRenderer buttonRenderer;

	public MeshRenderer dustJarRenderer;

	public GameObject m_costObject;

	public Transform m_disabledCostBone;

	public Transform m_enabledCostBone;

	public GameObject costBarMesh;

	public GameObject buttonMesh;

	private CraftingState m_craftingState;

	private CraftingState m_previousCraftingState;

	[SerializeField]
	private Color disabledTextColorOnButton;

	[SerializeField]
	private Color disabledTextColorOnCostBar;

	[SerializeField]
	private Color enabledCostColorOnPhone;

	public virtual void DisableButton(string buttonText = "")
	{
		OnEnabled(enable: false);
		SetCraftingState(CraftingState.Disabled);
		buttonRenderer.SetMaterial(disabledMaterial);
		if (dustJarRenderer != null && disabledDustJarMaterial != null)
		{
			dustJarRenderer.SetMaterial(disabledDustJarMaterial);
		}
		if (costText != null)
		{
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				costText.TextColor = disabledTextColorOnButton;
			}
			else
			{
				costText.TextColor = disabledTextColorOnCostBar;
			}
		}
		if (this is DisenchantButton)
		{
			SetLabelText("");
			return;
		}
		if (labelText != null)
		{
			labelText.TextColor = disabledTextColorOnButton;
		}
		if (buttonDisabledLabelText != null)
		{
			buttonDisabledLabelText.TextColor = disabledTextColorOnButton;
		}
		SetLabelText(buttonText);
	}

	public virtual void EnterUndoMode()
	{
		OnEnabled(enable: true);
		SetCraftingState(CraftingState.Undo);
		buttonRenderer.SetMaterial(undoMaterial);
		SetLabelText(GameStrings.Get("GLUE_CRAFTING_UNDO"));
		if (dustJarRenderer != null && enabledDustJarMaterial != null)
		{
			dustJarRenderer.SetMaterial(enabledDustJarMaterial);
		}
	}

	public virtual void EnableButton()
	{
		OnEnabled(enable: true);
		Actor shownActor = CraftingManager.Get().GetShownActor();
		if ((m_craftingState == CraftingState.Upgrade || m_craftingState == CraftingState.CreateUpgrade || (shownActor != null && shownActor.GetPremium() == TAG_PREMIUM.GOLDEN)) && upgradeMaterial != null)
		{
			buttonRenderer.SetMaterial(upgradeMaterial);
		}
		else
		{
			buttonRenderer.SetMaterial(enabledMaterial);
		}
	}

	public virtual void SetLabelText(string text)
	{
		if (labelText != null)
		{
			labelText.Text = text;
		}
		if (buttonDisabledLabelText != null)
		{
			buttonDisabledLabelText.Text = text;
		}
	}

	public bool IsButtonEnabled()
	{
		return base.gameObject.activeSelf;
	}

	public CraftingState GetCraftingState()
	{
		return m_craftingState;
	}

	public CraftingState GetPreviousCraftingState()
	{
		return m_previousCraftingState;
	}

	public void SetCraftingState(CraftingState state)
	{
		m_previousCraftingState = m_craftingState;
		m_craftingState = state;
		if (costText != null && state != 0)
		{
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				costText.TextColor = enabledCostColorOnPhone;
			}
			else
			{
				costText.TextColor = Color.white;
			}
			if (dustJarRenderer != null && enabledDustJarMaterial != null)
			{
				dustJarRenderer.SetMaterial(enabledDustJarMaterial);
			}
			if (labelText != null)
			{
				labelText.TextColor = Color.white;
			}
		}
	}

	private void OnEnabled(bool enable)
	{
		if (!UniversalInputManager.UsePhoneUI)
		{
			if (enable && labelText != null)
			{
				labelText.TextColor = Color.white;
			}
			if (buttonMesh != null)
			{
				buttonMesh.SetActive(enable);
			}
			if (costBarMesh != null)
			{
				costBarMesh.SetActive(enable);
			}
			else if (m_costObject != null)
			{
				m_costObject.SetActive(enable);
			}
			if (labelText != null)
			{
				labelText.gameObject.SetActive(enable);
			}
			if (buttonDisabledLabelText != null)
			{
				buttonDisabledLabelText.gameObject.SetActive(!enable);
			}
		}
		else if (m_costObject != null)
		{
			if (m_enabledCostBone != null && m_disabledCostBone != null)
			{
				m_costObject.transform.position = (enable ? m_enabledCostBone.position : m_disabledCostBone.position);
			}
			else
			{
				m_costObject.SetActive(enable);
			}
		}
		base.gameObject.GetComponent<Collider>().enabled = enable;
	}
}
