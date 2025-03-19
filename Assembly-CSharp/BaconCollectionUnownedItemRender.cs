using System.Collections;
using Hearthstone.UI;
using UnityEngine;

public class BaconCollectionUnownedItemRender : MonoBehaviour
{
	[SerializeField]
	private RenderToTexture TextureRenderer;

	[SerializeField]
	private UberText NameText;

	private void Start()
	{
		base.gameObject.GetComponent<Widget>().RegisterDoneChangingStatesListener(DoneChangingStates);
	}

	private void DoneChangingStates(object unused)
	{
		if (TextureRenderer != null && TextureRenderer.gameObject.activeInHierarchy)
		{
			if (NameText != null && NameText.gameObject.activeInHierarchy)
			{
				StartCoroutine(RenderOnceTextReady());
			}
			else
			{
				TextureRenderer.RenderNow();
			}
		}
	}

	private IEnumerator RenderOnceTextReady()
	{
		while (!NameText.IsDone())
		{
			yield return null;
		}
		TextureRenderer.RenderNow();
	}
}
