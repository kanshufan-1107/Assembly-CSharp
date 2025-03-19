using UnityEngine;
using UnityEngine.UI;

public class Ragnaros_Input_Test : MonoBehaviour
{
	[SerializeField]
	private RagnarosController m_controller;

	[SerializeField]
	private Transform m_testTarget;

	[SerializeField]
	private Text m_text;

	private bool m_armed;

	private void CheckInput()
	{
		if (MouseDown())
		{
			m_controller.HandleEvent("ATTACK_BIRTH");
			m_armed = true;
		}
		else if (MouseUp() && m_armed)
		{
			m_controller.HandleEvent("ATTACK");
			m_armed = false;
		}
	}

	private bool MouseDown()
	{
		return Input.GetMouseButtonDown(0);
	}

	private bool MouseUp()
	{
		return Input.GetMouseButtonUp(0);
	}

	private Vector3 Raycast()
	{
		float offset = Mathf.Abs(Camera.main.transform.position.y - 0.5f);
		return Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, offset));
	}

	private void Update()
	{
		CheckInput();
		Vector3 target = Raycast();
		m_testTarget.position = target;
		m_controller.ReticlePosition = target;
		m_text.text = m_testTarget.localPosition.ToString();
	}
}
