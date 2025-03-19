using Blizzard.T5.Game.Spells;
using Hearthstone.UI;
using UnityEngine;

public class PackOpeningCoin : MonoBehaviour
{
	public GameObject m_root;

	public AsyncReference m_HiddenCoinWidgetReference;

	private ISpellCallbackHandler<Spell>.FinishedCallback m_finishedCallback;

	private ISpellCallbackHandler<Spell>.StateFinishedCallback m_stateFinishedCallback;

	private void Start()
	{
		m_HiddenCoinWidgetReference.RegisterReadyListener<Widget>(OnHiddenCoinWidgetReady);
	}

	private void OnHiddenCoinWidgetReady(Widget widget)
	{
		RecursiveSetVisibility(widget.gameObject, visible: false);
	}

	private void RecursiveSetVisibility(GameObject go, bool visible)
	{
		Renderer r = go.GetComponent<Renderer>();
		if (r != null)
		{
			r.enabled = visible;
		}
		foreach (Transform child in go.transform)
		{
			RecursiveSetVisibility(child.gameObject, visible);
		}
	}

	public void ActivateDeathVisuals(ISpellCallbackHandler<Spell>.FinishedCallback finishedCallback, ISpellCallbackHandler<Spell>.StateFinishedCallback stateFinishedCallback)
	{
		m_finishedCallback = finishedCallback;
		m_stateFinishedCallback = stateFinishedCallback;
		SendEventUpwardStateAction.SendEventUpward(base.gameObject, "FADE_OUT_COIN");
	}

	public void OnDeathVisualsFadedIn()
	{
		m_finishedCallback(null, null);
		m_finishedCallback = null;
	}

	public void OnDeathVisualsFinished()
	{
		m_stateFinishedCallback(null, SpellStateType.NONE, null);
		m_stateFinishedCallback = null;
	}

	public void SetActive(bool active)
	{
		m_root.SetActive(active);
	}
}
