public class ArmorSpell : Spell
{
	public UberText m_ArmorText;

	private int m_armor;

	public int GetArmor()
	{
		return m_armor;
	}

	public void SetArmor(int armor)
	{
		m_armor = armor;
		UpdateArmorText();
	}

	private void UpdateArmorText()
	{
		if (!(m_ArmorText == null))
		{
			string armorText = m_armor.ToString();
			if (m_armor == 0)
			{
				armorText = "";
			}
			m_ArmorText.Text = armorText;
		}
	}
}
