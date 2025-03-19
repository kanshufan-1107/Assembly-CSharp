using UnityEngine;

public class CheatUI : MonoBehaviour
{
	public GameObject m_CheatManagerMenu;

	private void Start()
	{
		m_CheatManagerMenu.SetActive(value: false);
	}

	public void CloseCheatMenu()
	{
		m_CheatManagerMenu.SetActive(value: false);
	}

	private void Update()
	{
		if (InputCollection.GetKey(KeyCode.LeftControl) && InputCollection.GetKey(KeyCode.LeftAlt) && InputCollection.GetKey(KeyCode.LeftShift) && InputCollection.GetKeyDown(KeyCode.C))
		{
			m_CheatManagerMenu.SetActive(!m_CheatManagerMenu.activeSelf);
			SetActiveTabOnOpen();
		}
	}

	private void SetActiveTabOnOpen()
	{
		string OpeningFrom = "Level";
		if (!(OpeningFrom == "Match"))
		{
			if (OpeningFrom == "ClosedBox")
			{
				m_CheatManagerMenu.GetComponent<CheatMenu>().SetAsActiveTab(3);
			}
			else
			{
				m_CheatManagerMenu.GetComponent<CheatMenu>().SetAsActiveTab(3);
			}
		}
		else
		{
			m_CheatManagerMenu.GetComponent<CheatMenu>().SetAsActiveTab(0);
		}
	}
}
