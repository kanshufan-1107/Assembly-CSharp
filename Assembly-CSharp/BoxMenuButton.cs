using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class BoxMenuButton : PegUIElement
{
	public UberText m_TextMesh;

	public MeshRenderer m_ButtonMeshRenderer;

	public Spell m_Spell;

	public HighlightState m_HighlightState;

	public Box.ButtonType m_buttonType;

	private bool m_enablePressSpell = true;

	private bool m_enableHoverSpell = true;

	public void DisableHoverSpell()
	{
		m_enableHoverSpell = false;
	}

	public void EnableHoverSpell()
	{
		m_enableHoverSpell = true;
	}

	public void DisablePressSpell()
	{
		m_enablePressSpell = false;
	}

	public void EnablePressSpell()
	{
		m_enablePressSpell = true;
	}

	public string GetText()
	{
		return m_TextMesh.Text;
	}

	public void SetText(string text)
	{
		m_TextMesh.Text = text;
	}

	public void SetButtonMaterial(Material material)
	{
		if (!(m_ButtonMeshRenderer == null) && !(material == null))
		{
			m_ButtonMeshRenderer.SetMaterial(material);
		}
	}

	protected override void OnOver(InteractionState oldState)
	{
		if (!(m_Spell == null) && m_enableHoverSpell)
		{
			m_Spell.ActivateState(SpellStateType.BIRTH);
		}
	}

	protected override void OnOut(InteractionState oldState)
	{
		if (!(m_Spell == null) && m_enableHoverSpell)
		{
			m_Spell.ActivateState(SpellStateType.DEATH);
		}
	}

	protected override void OnPress()
	{
		if (!(m_Spell == null) && m_enablePressSpell && !DialogManager.Get().ShowingDialog())
		{
			m_Spell.ActivateState(SpellStateType.IDLE);
		}
	}

	protected override void OnRelease()
	{
		if (!(m_Spell == null))
		{
			m_Spell.ActivateState(SpellStateType.ACTION);
		}
	}
}
