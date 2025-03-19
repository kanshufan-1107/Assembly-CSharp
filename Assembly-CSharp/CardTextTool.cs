using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Blizzard.T5.Configuration.PreferencesManager;
using Blizzard.T5.Fonts;
using UnityEngine;
using UnityEngine.UI;

public class CardTextTool : MonoBehaviour
{
	[Serializable]
	public class LocalizedFont
	{
		public Locale m_Locale;

		public FontDefinition m_FontDef;
	}

	private const string PREFS_LOCALE = "CARD_TEXT_LOCALE";

	private const string PREFS_NAME = "CARD_TEXT_NAME";

	private const string PREFS_DESCRIPTION = "CARD_TEXT_DESCRIPTION";

	public GameObject m_CardsRoot;

	public Actor m_AbilityActor;

	public Actor m_AllyActor;

	public Actor m_WeaponActor;

	public Actor m_HeroActor;

	public Actor m_HeroPowerActor;

	public Actor m_BossCardActor;

	public Actor m_MercenariesAbilityActor;

	public Actor m_MercenariesEquipmentActor;

	public Actor m_MercenaryActor;

	public Actor m_LocationActor;

	public Texture2D m_AbilityPortraitTexture;

	public Texture2D m_AllyPortraitTexture;

	public Texture2D m_WeaponPortraitTexture;

	public Texture2D m_HeroPortraitTexture;

	public Texture2D m_HeroPowerPortraitTexture;

	public Texture2D m_BossPortraitTexture;

	public Texture2D m_MercenariesAbilityPortraitTexture;

	public Texture2D m_MercenariesEquipmentPortraitTexture;

	public Texture2D m_MercenaryPortraitTexture;

	public Texture2D m_LocationPortraitTexture;

	public UberText m_AbilityCardDescription;

	public UberText m_AllyCardDescription;

	public UberText m_WeaponCardDescription;

	public UberText m_HeroCardDescription;

	public UberText m_HeroPowerCardDescription;

	public UberText m_BossCardDescription;

	public UberText m_MercenariesAbilityCardDescription;

	public UberText m_MercenariesEquipmentCardDescription;

	public UberText m_MercenaryCardDescription;

	public UberText m_LocationCardDescription;

	public InputField m_DescriptionInputField;

	public UberText m_AbilityCardName;

	public UberText m_AllyCardName;

	public UberText m_WeaponCardName;

	public UberText m_HeroCardName;

	public UberText m_HeroPowerName;

	public UberText m_BossName;

	public UberText m_MercenariesAbilityCardName;

	public UberText m_MercenariesEquipmentCardName;

	public UberText m_MercenaryCardName;

	public UberText m_LocationCardName;

	public InputField m_NameInputField;

	public Button m_LocaleDropDownMainButton;

	public Button m_LocaleDropDownSelectionButton;

	public List<LocalizedFont> m_LocalizedFontCollection;

	private Locale m_locale;

	private string[] m_csvStrings;

	private int m_indexIntoCSVList;

	private void Awake()
	{
		Initialize();
	}

	private void OnApplicationQuit()
	{
		PreferencesManager.SetString("CARD_TEXT_NAME", m_NameInputField.text);
		PreferencesManager.SetString("CARD_TEXT_DESCRIPTION", m_DescriptionInputField.text);
		PreferencesManager.Save();
	}

	public void UpdateDescriptionText()
	{
		string text = m_DescriptionInputField.text;
		text = Regex.Replace(text, "(\\$|#)", "");
		text = FixedNewline(text);
		m_AbilityCardDescription.Text = text;
		m_AllyCardDescription.Text = text;
		m_WeaponCardDescription.Text = text;
		m_HeroCardDescription.Text = text;
		m_HeroPowerCardDescription.Text = text;
		m_BossCardDescription.Text = text;
		m_MercenariesAbilityCardDescription.Text = text;
		m_MercenariesEquipmentCardDescription.Text = text;
		m_MercenaryCardDescription.Text = text;
		m_LocationCardDescription.Text = text;
	}

	public void UpdateNameText()
	{
		string text = m_NameInputField.text;
		m_AbilityCardName.Text = text;
		m_AllyCardName.Text = text;
		m_WeaponCardName.Text = text;
		m_HeroCardName.Text = text;
		m_HeroPowerName.Text = text;
		m_BossName.Text = text;
		m_MercenariesAbilityCardName.Text = text;
		m_MercenariesEquipmentCardName.Text = text;
		m_MercenaryCardName.Text = text;
		m_LocationCardName.Text = text;
	}

	public void PasteClipboard()
	{
		m_DescriptionInputField.text = GUIUtility.systemCopyBuffer;
		UpdateDescriptionText();
	}

	public void CopyToClipboard()
	{
		GUIUtility.systemCopyBuffer = m_DescriptionInputField.text;
	}

	public void LeftArrowClicked()
	{
		HandleArrowClicked(-2);
	}

	public void RightArrowClicked()
	{
		HandleArrowClicked(2);
	}

	private void HandleArrowClicked(int offset)
	{
		m_indexIntoCSVList += offset;
		if (m_indexIntoCSVList < 0)
		{
			m_indexIntoCSVList = m_csvStrings.Length - 2;
		}
		if (m_indexIntoCSVList > m_csvStrings.Length - 2)
		{
			m_indexIntoCSVList = 0;
		}
		m_NameInputField.text = m_csvStrings[m_indexIntoCSVList];
		m_DescriptionInputField.text = m_csvStrings[m_indexIntoCSVList + 1];
		UpdateNameText();
		UpdateDescriptionText();
	}

	public void LoadCSV()
	{
		string clipboardContents = GUIUtility.systemCopyBuffer;
		m_csvStrings = CsvParser(clipboardContents);
		m_indexIntoCSVList = 0;
		m_NameInputField.text = ((m_csvStrings.Length != 0) ? m_csvStrings[m_indexIntoCSVList] : string.Empty);
		m_DescriptionInputField.text = ((m_csvStrings.Length > 1) ? m_csvStrings[m_indexIntoCSVList + 1] : string.Empty);
		UpdateNameText();
		UpdateDescriptionText();
	}

	private string[] CsvParser(string csvText)
	{
		List<string> tokens = new List<string>();
		int last = -1;
		int current = 0;
		bool inText = false;
		for (csvText = csvText.Replace("\r", string.Empty).Replace("\n", ","); current < csvText.Length; current++)
		{
			switch (csvText[current])
			{
			case '"':
				inText = !inText;
				break;
			case ',':
				if (!inText)
				{
					string toAdd = csvText.Substring(last + 1, current - last).Trim(' ', ',');
					toAdd = toAdd.Trim('"').Replace("\"\"\"", "\"").Replace("\"\"", "\"");
					tokens.Add(toAdd);
					last = current;
				}
				break;
			}
		}
		if (last != csvText.Length - 1)
		{
			tokens.Add(csvText.Substring(last + 1).Trim());
		}
		return tokens.ToArray();
	}

	private void Initialize()
	{
		if (PreferencesManager.HasKey("CARD_TEXT_LOCALE"))
		{
			m_locale = (Locale)PreferencesManager.GetInt("CARD_TEXT_LOCALE");
		}
		UberTextSetup.SetConfig(new UberTextConfig(28, new UberTextRenderTextureTracker(), new UberTextShhadetUtil(), Log.UberText));
		Localization.SetLocale(m_locale);
		HearthstoneLocalization.Initialize();
		SetupLocaleDropDown();
		SetLocale();
		m_AbilityActor.SetPortraitTexture(m_AbilityPortraitTexture);
		m_AllyActor.SetPortraitTexture(m_AllyPortraitTexture);
		m_WeaponActor.SetPortraitTexture(m_WeaponPortraitTexture);
		m_HeroActor.SetPortraitTexture(m_HeroPortraitTexture);
		m_HeroPowerActor.SetPortraitTexture(m_HeroPowerPortraitTexture);
		m_BossCardActor.SetPortraitTexture(m_BossPortraitTexture);
		m_MercenariesAbilityActor.SetPortraitTexture(m_MercenariesAbilityPortraitTexture);
		m_MercenariesEquipmentActor.SetPortraitTexture(m_MercenariesEquipmentPortraitTexture);
		m_MercenaryActor.SetPortraitTexture(m_MercenaryPortraitTexture);
		m_LocationActor.SetPortraitTexture(m_LocationPortraitTexture);
		if (PreferencesManager.HasKey("CARD_TEXT_NAME"))
		{
			m_NameInputField.text = PreferencesManager.GetString("CARD_TEXT_NAME");
		}
		if (PreferencesManager.HasKey("CARD_TEXT_DESCRIPTION"))
		{
			m_DescriptionInputField.text = PreferencesManager.GetString("CARD_TEXT_DESCRIPTION");
		}
		UberText[] componentsInChildren = m_CardsRoot.GetComponentsInChildren<UberText>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Cache = false;
		}
		UpdateDescriptionText();
		UpdateNameText();
	}

	private string FixedNewline(string text)
	{
		if (text.Length < 2)
		{
			return text;
		}
		StringBuilder fixedText = new StringBuilder();
		for (int i = 0; i < text.Length; i++)
		{
			if (i + 1 < text.Length && text[i] == '\\' && text[i + 1] == 'n')
			{
				fixedText.Append('\n');
				i++;
			}
			else
			{
				fixedText.Append(text[i]);
			}
		}
		return fixedText.ToString();
	}

	private void SetupLocaleDropDown()
	{
		GameObject panel = m_LocaleDropDownSelectionButton.transform.parent.gameObject;
		panel.SetActive(value: true);
		foreach (Locale loc in Enum.GetValues(typeof(Locale)))
		{
			if (loc != Locale.UNKNOWN)
			{
				GameObject obj = UnityEngine.Object.Instantiate(m_LocaleDropDownSelectionButton.gameObject);
				obj.transform.parent = m_LocaleDropDownSelectionButton.transform.parent;
				Button component = obj.GetComponent<Button>();
				component.GetComponentInChildren<Text>().text = loc.ToString();
				Locale locSet = loc;
				component.onClick.AddListener(delegate
				{
					OnClick_LocaleSetButton(locSet);
				});
			}
		}
		UnityEngine.Object.Destroy(m_LocaleDropDownSelectionButton.gameObject);
		SetLocaleButtonText(m_locale);
		panel.SetActive(value: false);
	}

	private void OnClick_LocaleSetButton(Locale locale)
	{
		m_LocaleDropDownMainButton.GetComponentInChildren<Text>().text = locale.ToString();
		m_locale = locale;
		SaveLocale(m_locale);
		SetLocale();
	}

	private void SetLocaleButtonText(Locale loc)
	{
		m_LocaleDropDownMainButton.GetComponentInChildren<Text>().text = loc.ToString();
	}

	private void SaveLocale(Locale loc)
	{
		PreferencesManager.SetInt("CARD_TEXT_LOCALE", (int)m_locale);
		PreferencesManager.Save();
	}

	private void SetLocale()
	{
		StartCoroutine(SetLocaleCoroutine());
	}

	private IEnumerator SetLocaleCoroutine()
	{
		Localization.SetLocale(m_locale);
		yield return null;
		UpdateCardFonts(Locale.enUS);
		UpdateCardFonts(m_locale);
	}

	private void UpdateCardFonts(Locale loc)
	{
		foreach (LocalizedFont locFont in m_LocalizedFontCollection)
		{
			if (locFont.m_Locale == loc)
			{
				if (locFont.m_FontDef.name == "FranklinGothic")
				{
					m_AbilityCardDescription.SetFontWithoutLocalization(locFont.m_FontDef);
					m_AllyCardDescription.SetFontWithoutLocalization(locFont.m_FontDef);
					m_WeaponCardDescription.SetFontWithoutLocalization(locFont.m_FontDef);
					m_HeroCardDescription.SetFontWithoutLocalization(locFont.m_FontDef);
					m_HeroPowerCardDescription.SetFontWithoutLocalization(locFont.m_FontDef);
					m_BossCardDescription.SetFontWithoutLocalization(locFont.m_FontDef);
					m_MercenariesAbilityCardDescription.SetFontWithoutLocalization(locFont.m_FontDef);
					m_MercenariesEquipmentCardDescription.SetFontWithoutLocalization(locFont.m_FontDef);
					m_MercenaryCardDescription.SetFontWithoutLocalization(locFont.m_FontDef);
					m_LocationCardDescription.SetFontWithoutLocalization(locFont.m_FontDef);
				}
				if (locFont.m_FontDef.name == "Belwe_Outline")
				{
					m_AbilityCardName.SetFontWithoutLocalization(locFont.m_FontDef);
					m_AllyCardName.SetFontWithoutLocalization(locFont.m_FontDef);
					m_WeaponCardName.SetFontWithoutLocalization(locFont.m_FontDef);
					m_HeroCardName.SetFontWithoutLocalization(locFont.m_FontDef);
					m_HeroPowerName.SetFontWithoutLocalization(locFont.m_FontDef);
					m_BossName.SetFontWithoutLocalization(locFont.m_FontDef);
					m_MercenariesAbilityCardName.SetFontWithoutLocalization(locFont.m_FontDef);
					m_MercenariesEquipmentCardName.SetFontWithoutLocalization(locFont.m_FontDef);
					m_MercenaryCardName.SetFontWithoutLocalization(locFont.m_FontDef);
					m_LocationCardName.SetFontWithoutLocalization(locFont.m_FontDef);
				}
			}
		}
	}
}
