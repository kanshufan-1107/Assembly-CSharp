using System.Collections.Generic;
using UnityEngine;

public class RadioButtonGroup : MonoBehaviour
{
	public delegate void DelButtonSelected(int buttonID, object userData);

	public delegate void DelButtonDoubleClicked(FramedRadioButton framedRadioButton);

	public struct ButtonData
	{
		public int m_id;

		public string m_text;

		public bool m_selected;

		public object m_userData;

		public ButtonData(int id, string text, object userData, bool selected)
		{
			m_id = id;
			m_text = text;
			m_userData = userData;
			m_selected = selected;
		}
	}

	public GameObject m_buttonContainer;

	public FramedRadioButton m_framedRadioButtonPrefab;

	public GameObject m_firstRadioButtonBone;

	private List<FramedRadioButton> m_framedRadioButtons = new List<FramedRadioButton>();

	private DelButtonSelected m_buttonSelectedCB;

	private DelButtonDoubleClicked m_buttonDoubleClickedCB;

	private Vector3 m_spacingFudgeFactor = Vector3.zero;

	public void ShowButtons(List<ButtonData> buttonData, DelButtonSelected buttonSelectedCallback, DelButtonDoubleClicked buttonDoubleClickedCallback)
	{
		m_buttonContainer.SetActive(value: true);
		int numButtons = buttonData.Count;
		while (m_framedRadioButtons.Count > numButtons)
		{
			FramedRadioButton obj = m_framedRadioButtons[0];
			m_framedRadioButtons.RemoveAt(0);
			Object.DestroyImmediate(obj);
		}
		bool isSingleButton = 1 == numButtons;
		Vector3 buttonContainerWorldPos = m_buttonContainer.transform.position;
		GameObject prevButton = new GameObject();
		RadioButton selectedButton = null;
		for (int i = 0; i < numButtons; i++)
		{
			FramedRadioButton framedRadioButton;
			if (m_framedRadioButtons.Count > i)
			{
				framedRadioButton = m_framedRadioButtons[i];
			}
			else
			{
				framedRadioButton = CreateNewFramedRadioButton();
				m_framedRadioButtons.Add(framedRadioButton);
			}
			FramedRadioButton.FrameType frameType = ((!isSingleButton) ? ((i == 0) ? FramedRadioButton.FrameType.MULTI_LEFT_END : ((numButtons - 1 != i) ? FramedRadioButton.FrameType.MULTI_MIDDLE : FramedRadioButton.FrameType.MULTI_RIGHT_END)) : FramedRadioButton.FrameType.SINGLE);
			ButtonData data = buttonData[i];
			framedRadioButton.Show();
			framedRadioButton.Init(frameType, data.m_text, data.m_id, data.m_userData);
			if (data.m_selected)
			{
				if (selectedButton != null)
				{
					Debug.LogWarning("RadioButtonGroup.WaitThenShowButtons(): more than one button was set as selected. Selecting the FIRST provided option.");
					framedRadioButton.m_radioButton.SetSelected(selected: false);
				}
				else
				{
					selectedButton = framedRadioButton.m_radioButton;
					selectedButton.SetSelected(selected: true);
				}
			}
			else
			{
				framedRadioButton.m_radioButton.SetSelected(selected: false);
			}
			if (i == 0)
			{
				TransformUtil.SetPoint(framedRadioButton.gameObject, Anchor.LEFT, m_firstRadioButtonBone, Anchor.LEFT);
			}
			else
			{
				TransformUtil.SetPoint(framedRadioButton.gameObject, new Vector3(0f, 1f, 0.5f), prevButton, new Vector3(1f, 1f, 0.5f), m_spacingFudgeFactor);
			}
			prevButton = framedRadioButton.m_frameFill;
		}
		buttonContainerWorldPos.x -= TransformUtil.GetBoundsOfChildren(m_buttonContainer).size.x / 2f;
		m_buttonContainer.transform.position = buttonContainerWorldPos;
		m_buttonSelectedCB = buttonSelectedCallback;
		m_buttonDoubleClickedCB = buttonDoubleClickedCallback;
		if (!(selectedButton == null) && m_buttonSelectedCB != null)
		{
			m_buttonSelectedCB(selectedButton.GetButtonID(), selectedButton.GetUserData());
		}
	}

	public void Hide()
	{
		m_buttonContainer.SetActive(value: false);
	}

	public void SetSpacingFudgeFactor(Vector3 amount)
	{
		m_spacingFudgeFactor = amount;
	}

	private FramedRadioButton CreateNewFramedRadioButton()
	{
		FramedRadioButton framedRadioButton = Object.Instantiate(m_framedRadioButtonPrefab);
		framedRadioButton.transform.parent = m_buttonContainer.transform;
		framedRadioButton.transform.localPosition = Vector3.zero;
		framedRadioButton.transform.localScale = Vector3.one;
		framedRadioButton.transform.localRotation = Quaternion.identity;
		framedRadioButton.m_radioButton.AddEventListener(UIEventType.RELEASE, OnRadioButtonReleased);
		framedRadioButton.m_radioButton.AddEventListener(UIEventType.DOUBLECLICK, OnRadioButtonDoubleClicked);
		return framedRadioButton;
	}

	private void OnRadioButtonReleased(UIEvent e)
	{
		RadioButton selectedRadioButton = e.GetElement() as RadioButton;
		if (selectedRadioButton == null)
		{
			Debug.LogWarning($"RadioButtonGroup.OnRadioButtonReleased(): UIEvent {e} source is not a RadioButton!");
			return;
		}
		bool buttonAlreadySelected = selectedRadioButton.IsSelected();
		foreach (FramedRadioButton framedRadioButton in m_framedRadioButtons)
		{
			RadioButton radioButton = framedRadioButton.m_radioButton;
			bool selected = selectedRadioButton == radioButton;
			radioButton.SetSelected(selected);
		}
		if (m_buttonSelectedCB != null)
		{
			m_buttonSelectedCB(selectedRadioButton.GetButtonID(), selectedRadioButton.GetUserData());
			if (UniversalInputManager.Get().IsTouchMode() && buttonAlreadySelected)
			{
				OnRadioButtonDoubleClicked(e);
			}
		}
	}

	private void OnRadioButtonDoubleClicked(UIEvent e)
	{
		if (m_buttonDoubleClickedCB == null)
		{
			return;
		}
		RadioButton selectedRadioButton = e.GetElement() as RadioButton;
		if (selectedRadioButton == null)
		{
			Debug.LogWarning($"RadioButtonGroup.OnRadioButtonDoubleClicked(): UIEvent {e} source is not a RadioButton!");
			return;
		}
		FramedRadioButton selectedFramedRadioButton = null;
		foreach (FramedRadioButton framedRadioButton in m_framedRadioButtons)
		{
			if (!(selectedRadioButton != framedRadioButton.m_radioButton))
			{
				selectedFramedRadioButton = framedRadioButton;
				break;
			}
		}
		if (selectedFramedRadioButton == null)
		{
			Debug.LogWarning($"RadioButtonGroup.OnRadioButtonDoubleClicked(): could not find framed radio button for radio button ID {selectedRadioButton.GetButtonID()}");
		}
		else
		{
			m_buttonDoubleClickedCB(selectedFramedRadioButton);
		}
	}
}
