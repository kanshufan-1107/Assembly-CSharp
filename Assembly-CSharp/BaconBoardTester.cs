using System;
using System.Collections;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.UI;

public class BaconBoardTester : MonoBehaviour
{
	public enum BaconBoardTesterPlatform
	{
		PC,
		PHONE
	}

	public KeyboardFSMSettings m_keyboardStateSettings;

	public GameObject m_pcAuthoringRoot;

	public GameObject m_pcCombatSkinsRoot;

	public GameObject m_pcTavernSkinsRoot;

	public GameObject m_phoneAuthoringRoot;

	public GameObject m_phoneCombatSkinsRoot;

	public GameObject m_phoneTavernSkinsRoot;

	public GameObject m_controlsRoot;

	public GameObject m_turnIndicatorRoot;

	public GameObject m_turnIndicatorPhoneRoot;

	public List<PlayMakerFSM> m_boardStateChangingObjects;

	public Dropdown m_platformSelection;

	public Dropdown m_combatSkinSelectionPC;

	public Dropdown m_combatSkinSelectionPhone;

	public Dropdown m_tavernSkinSelectionPC;

	public Dropdown m_tavernSkinSelectionPhone;

	public Dropdown m_fsmStateSelection;

	private BaconBoardTesterPlatform m_selectedPlatform;

	private BaconBoardSkinBehaviour m_selectedPCCombatSkin;

	private BaconBoardSkinBehaviour m_selectedPCTavernSkin;

	private BaconBoardSkinBehaviour m_prevPCCombatSkin;

	private BaconBoardSkinBehaviour m_prevPCTavernSkin;

	private BaconBoardSkinBehaviour m_selectedPhoneCombatSkin;

	private BaconBoardSkinBehaviour m_selectedPhoneTavernSkin;

	private BaconBoardSkinBehaviour m_prevPhoneCombatSkin;

	private BaconBoardSkinBehaviour m_prevPhoneTavernSkin;

	private string m_selectedState = "SHOP";

	private TAG_BOARD_VISUAL_STATE m_prevState;

	private BaconBoardSkinBehaviour[] m_pcCombatSkins;

	private BaconBoardSkinBehaviour[] m_pcTavernSkins;

	private BaconBoardSkinBehaviour[] m_phoneCombatSkins;

	private BaconBoardSkinBehaviour[] m_phoneTavernSkins;

	private void Start()
	{
		StartCoroutine(WaitForSoundManagerThenLoad());
	}

	public IEnumerator WaitForSoundManagerThenLoad()
	{
		while (SoundManager.Get() == null)
		{
			yield return new WaitForSeconds(1f);
		}
		InitObjects();
	}

	private void InitObjects()
	{
		m_pcCombatSkins = m_pcCombatSkinsRoot.GetComponentsInChildren<BaconBoardSkinBehaviour>(includeInactive: true);
		m_pcTavernSkins = m_pcTavernSkinsRoot.GetComponentsInChildren<BaconBoardSkinBehaviour>(includeInactive: true);
		m_phoneCombatSkins = m_phoneCombatSkinsRoot.GetComponentsInChildren<BaconBoardSkinBehaviour>(includeInactive: true);
		m_phoneTavernSkins = m_phoneTavernSkinsRoot.GetComponentsInChildren<BaconBoardSkinBehaviour>(includeInactive: true);
		if (m_phoneAuthoringRoot.activeSelf && !m_pcAuthoringRoot.activeSelf)
		{
			m_selectedPlatform = BaconBoardTesterPlatform.PHONE;
		}
		else
		{
			m_selectedPlatform = BaconBoardTesterPlatform.PC;
		}
		m_platformSelection.SetValueWithoutNotify((int)m_selectedPlatform);
		m_selectedPCCombatSkin = GetActiveSkin(m_pcCombatSkins);
		m_selectedPCTavernSkin = GetActiveSkin(m_pcTavernSkins);
		m_selectedPhoneCombatSkin = GetActiveSkin(m_phoneCombatSkins);
		m_selectedPhoneTavernSkin = GetActiveSkin(m_phoneTavernSkins);
		InitDropdown(m_combatSkinSelectionPC, Array.ConvertAll(m_pcCombatSkins, (BaconBoardSkinBehaviour skin) => skin.gameObject.name), m_selectedPCCombatSkin.gameObject.name);
		InitDropdown(m_tavernSkinSelectionPC, Array.ConvertAll(m_pcTavernSkins, (BaconBoardSkinBehaviour skin) => skin.gameObject.name), m_selectedPCTavernSkin.gameObject.name);
		InitDropdown(m_combatSkinSelectionPhone, Array.ConvertAll(m_phoneCombatSkins, (BaconBoardSkinBehaviour skin) => skin.gameObject.name), m_selectedPhoneCombatSkin.gameObject.name);
		InitDropdown(m_tavernSkinSelectionPhone, Array.ConvertAll(m_phoneTavernSkins, (BaconBoardSkinBehaviour skin) => skin.gameObject.name), m_selectedPhoneTavernSkin.gameObject.name);
		InitDropdown(m_fsmStateSelection, m_keyboardStateSettings.Settings.ConvertAll((KeyboardFSMSettings.KeyAndAnimationTriggerPair setting) => setting.PlaymakerState + " ( " + setting.KeyboardKey.ToString() + ")").ToArray(), "SHOP");
		ActivateSelection();
	}

	public void OnCombatBoardChangedPC(int index)
	{
		m_selectedPCCombatSkin = m_pcCombatSkins[index];
		ActivateSelection();
	}

	public void OnCombatBoardChangedPhone(int index)
	{
		m_selectedPhoneCombatSkin = m_phoneCombatSkins[index];
		ActivateSelection();
	}

	public void OnTavernBoardChangedPC(int index)
	{
		m_selectedPCTavernSkin = m_pcTavernSkins[index];
		ActivateSelection();
	}

	public void OnTavernBoardChangedPhone(int index)
	{
		m_selectedPhoneTavernSkin = m_phoneTavernSkins[index];
		ActivateSelection();
	}

	public void OnPlatformChange(int value)
	{
		m_selectedPlatform = (BaconBoardTesterPlatform)value;
		ActivateSelection();
	}

	public void OnFSMStateChange(int index)
	{
		m_selectedState = m_keyboardStateSettings[index].PlaymakerState;
		ActivateSelection();
	}

	public void OnUseBlurToggled(bool value)
	{
		m_turnIndicatorRoot.SetActive(value);
		m_turnIndicatorPhoneRoot.SetActive(value);
	}

	private void InitDropdown(Dropdown dropdown, string[] items, string selection)
	{
		dropdown.AddOptions(new List<string>(items));
		dropdown.SetValueWithoutNotify(dropdown.options.FindIndex((Dropdown.OptionData option) => option.text.StartsWith(selection)));
	}

	private BaconBoardSkinBehaviour GetActiveSkin(BaconBoardSkinBehaviour[] skins)
	{
		foreach (BaconBoardSkinBehaviour skin in skins)
		{
			if (skin.gameObject.activeSelf)
			{
				return skin;
			}
		}
		skins[0].gameObject.SetActive(value: true);
		return skins[0];
	}

	private void SetStateOnFsms(string stateName)
	{
		foreach (PlayMakerFSM fsm in m_boardStateChangingObjects)
		{
			if (FsmContainsState(fsm, stateName))
			{
				fsm.SetState(stateName);
			}
		}
	}

	private bool FsmContainsState(PlayMakerFSM fsm, string stateName)
	{
		FsmState[] fsmStates = fsm.FsmStates;
		foreach (FsmState state in fsmStates)
		{
			if (stateName.Equals(state.Name))
			{
				return true;
			}
		}
		return false;
	}

	public void AddStateChangingPlaymaker(GameObject container)
	{
		PlayMakerFSM playmaker = container.GetComponentInChildren<PlayMakerFSM>();
		if (playmaker != null)
		{
			m_boardStateChangingObjects.Add(playmaker);
		}
	}

	public void ActivateSelection()
	{
		BaconBoardSkinBehaviour[] pcCombatSkins = m_pcCombatSkins;
		foreach (BaconBoardSkinBehaviour skin in pcCombatSkins)
		{
			if (skin != m_selectedPCCombatSkin)
			{
				skin.gameObject.SetActive(value: false);
			}
		}
		pcCombatSkins = m_pcTavernSkins;
		foreach (BaconBoardSkinBehaviour skin2 in pcCombatSkins)
		{
			if (skin2 != m_selectedPCTavernSkin)
			{
				skin2.gameObject.SetActive(value: false);
			}
		}
		pcCombatSkins = m_phoneCombatSkins;
		foreach (BaconBoardSkinBehaviour skin3 in pcCombatSkins)
		{
			if (skin3 != m_selectedPhoneCombatSkin)
			{
				skin3.gameObject.SetActive(value: false);
			}
		}
		pcCombatSkins = m_phoneTavernSkins;
		foreach (BaconBoardSkinBehaviour skin4 in pcCombatSkins)
		{
			if (skin4 != m_selectedPhoneTavernSkin)
			{
				skin4.gameObject.SetActive(value: false);
			}
		}
		m_pcAuthoringRoot.SetActive(m_selectedPlatform == BaconBoardTesterPlatform.PC);
		m_combatSkinSelectionPC.gameObject.SetActive(m_selectedPlatform == BaconBoardTesterPlatform.PC);
		m_tavernSkinSelectionPC.gameObject.SetActive(m_selectedPlatform == BaconBoardTesterPlatform.PC);
		m_phoneAuthoringRoot.SetActive(m_selectedPlatform == BaconBoardTesterPlatform.PHONE);
		m_combatSkinSelectionPhone.gameObject.SetActive(m_selectedPlatform == BaconBoardTesterPlatform.PHONE);
		m_tavernSkinSelectionPhone.gameObject.SetActive(m_selectedPlatform == BaconBoardTesterPlatform.PHONE);
		m_selectedPCCombatSkin.gameObject.SetActive(m_selectedPlatform == BaconBoardTesterPlatform.PC);
		m_selectedPCTavernSkin.gameObject.SetActive(m_selectedPlatform == BaconBoardTesterPlatform.PC);
		m_selectedPhoneCombatSkin.gameObject.SetActive(m_selectedPlatform == BaconBoardTesterPlatform.PHONE);
		m_selectedPhoneTavernSkin.gameObject.SetActive(m_selectedPlatform == BaconBoardTesterPlatform.PHONE);
		TAG_BOARD_VISUAL_STATE newState = ((m_selectedState == "SHOP") ? TAG_BOARD_VISUAL_STATE.SHOP : TAG_BOARD_VISUAL_STATE.COMBAT);
		if (m_selectedPlatform == BaconBoardTesterPlatform.PC)
		{
			if (newState == TAG_BOARD_VISUAL_STATE.COMBAT)
			{
				m_selectedPCCombatSkin.CopyCornersFromSkin(m_selectedPCTavernSkin);
			}
			if (m_prevState != newState || m_selectedPCCombatSkin != m_prevPCCombatSkin || m_selectedState == "COMBAT")
			{
				m_selectedPCCombatSkin.SetBoardState(newState);
			}
			if (m_prevState != newState || m_selectedPCTavernSkin != m_prevPCTavernSkin)
			{
				m_selectedPCTavernSkin.SetBoardState(newState);
			}
			m_prevPCCombatSkin = m_selectedPCCombatSkin;
			m_prevPCTavernSkin = m_selectedPCTavernSkin;
			m_prevState = newState;
			if (m_selectedState != "SHOP" && m_selectedState != "COMBAT")
			{
				m_selectedPCCombatSkin.DebugTriggerFSMState(m_selectedState);
			}
		}
		else
		{
			if (newState == TAG_BOARD_VISUAL_STATE.COMBAT)
			{
				m_selectedPhoneCombatSkin.CopyCornersFromSkin(m_selectedPhoneTavernSkin);
			}
			if (m_prevState != newState || m_selectedPhoneCombatSkin != m_prevPhoneCombatSkin || m_selectedState == "COMBAT")
			{
				m_selectedPhoneCombatSkin.SetBoardState(newState);
			}
			if (m_prevState != newState || m_selectedPhoneTavernSkin != m_prevPhoneTavernSkin)
			{
				m_selectedPhoneTavernSkin.SetBoardState(newState);
			}
			m_prevPhoneCombatSkin = m_selectedPhoneCombatSkin;
			m_prevPhoneTavernSkin = m_selectedPhoneTavernSkin;
			m_prevState = newState;
			if (m_selectedState != "SHOP" && m_selectedState != "COMBAT")
			{
				m_selectedPhoneCombatSkin.DebugTriggerFSMState(m_selectedState);
			}
		}
		SetStateOnFsms(newState.ToString());
	}
}
