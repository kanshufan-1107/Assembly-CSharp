using System.Collections;
using UnityEngine;

public class GenericConfirmationPopup : DialogBase
{
	[SerializeField]
	private Spell m_successRingSpell;

	[SerializeField]
	private GameObject m_successRingContainer;

	[SerializeField]
	private GameObject m_searchPanel;

	[SerializeField]
	private GameObject m_successPanel;

	[SerializeField]
	private string m_searchText;

	[SerializeField]
	private string m_successText;

	private Vector3 NORMAL_SCALE;

	private Vector3 HIDDEN_SCALE;

	protected override void Awake()
	{
		base.Awake();
		NORMAL_SCALE = base.transform.localScale;
		HIDDEN_SCALE = 0.01f * NORMAL_SCALE;
	}

	public void ShowConfirmation(float showDelay = 0f, float searchTime = 1f, float confirmationTime = 3f)
	{
		StartCoroutine(OnConfirmation(showDelay, searchTime, confirmationTime));
	}

	private IEnumerator OnConfirmation(float showDelay, float searchTime, float confirmationTime)
	{
		yield return new WaitForSeconds(showDelay);
		m_searchPanel.GetComponentInChildren<UberText>().Text = m_searchText;
		m_searchPanel.SetActive(value: true);
		m_successRingContainer.SetActive(value: true);
		m_successRingSpell.ActivateState(SpellStateType.BIRTH);
		Show();
		yield return new WaitForSeconds(searchTime);
		m_searchPanel.SetActive(value: false);
		m_successPanel.GetComponentInChildren<UberText>().Text = m_successText;
		m_successPanel.SetActive(value: true);
		m_successRingSpell.ActivateState(SpellStateType.ACTION);
		yield return new WaitForSeconds(confirmationTime);
		HideConfirmation();
	}

	public override void Show()
	{
		base.Show();
		AnimationUtil.ShowWithPunch(base.gameObject, HIDDEN_SCALE, 1.1f * NORMAL_SCALE, NORMAL_SCALE, null, noFade: true);
	}

	public void HideConfirmation()
	{
		if (m_shown)
		{
			Hide();
			Object.Destroy(base.gameObject, 0.25f);
		}
	}
}
