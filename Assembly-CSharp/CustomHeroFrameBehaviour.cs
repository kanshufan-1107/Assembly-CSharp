using System.Collections;
using System.Collections.Generic;
using Hearthstone.UI.Core;
using UnityEngine;

[RequireComponent(typeof(Actor))]
public class CustomHeroFrameBehaviour : MonoBehaviour, IPopupRendering
{
	[SerializeField]
	private GameObject m_defaultFrame;

	private GameObject m_frameMesh;

	private Actor m_actor;

	private IPopupRoot m_popupRoot;

	private HashSet<IPopupRendering> m_popupRenderingComponents = new HashSet<IPopupRendering>();

	private bool m_isCoroutineRunning;

	private bool m_shouldRestartCoroutine;

	private void Awake()
	{
		UpdateFrame();
	}

	private void OnDisable()
	{
		if (m_isCoroutineRunning)
		{
			m_shouldRestartCoroutine = true;
		}
	}

	private void OnEnable()
	{
		if (m_shouldRestartCoroutine)
		{
			StartCoroutine(WaitForCardDef());
			m_shouldRestartCoroutine = false;
		}
	}

	public GameObject GetFrame()
	{
		return m_frameMesh;
	}

	public void UpdateFrame()
	{
		m_actor = GetComponent<Actor>();
		if (m_actor != null && m_actor.HasCardDef)
		{
			LoadFrameMesh();
			return;
		}
		InstantiateFrameMesh(m_defaultFrame);
		StartCoroutine(WaitForCardDef());
	}

	private IEnumerator WaitForCardDef()
	{
		m_isCoroutineRunning = true;
		while (m_actor != null && !m_actor.HasCardDef)
		{
			yield return null;
		}
		LoadFrameMesh();
		m_isCoroutineRunning = false;
	}

	private void LoadFrameMesh()
	{
		if (m_actor == null)
		{
			return;
		}
		using DefLoader.DisposableCardDef cardDef = m_actor.ShareDisposableCardDef();
		if (cardDef != null && cardDef.CardDef.m_FrameMeshOverride != null)
		{
			InstantiateFrameMesh(cardDef.CardDef.m_FrameMeshOverride);
		}
		else
		{
			InstantiateFrameMesh(m_defaultFrame);
		}
	}

	private void InstantiateFrameMesh(GameObject frameObject)
	{
		if (m_frameMesh != null)
		{
			Object.Destroy(m_frameMesh);
		}
		m_frameMesh = Object.Instantiate(frameObject, m_actor.m_portraitMesh.transform);
		m_frameMesh.transform.localPosition = new Vector3(0f, 0.1f, 0f);
		LayerUtils.SetLayer(m_frameMesh, m_actor.m_portraitMesh.layer, null);
		if (m_popupRoot != null)
		{
			m_popupRoot.ApplyPopupRendering(m_frameMesh.transform, m_popupRenderingComponents);
		}
	}

	public GameObject GetMeshObject()
	{
		return m_frameMesh;
	}

	public void EnablePopupRendering(IPopupRoot popupRoot)
	{
		if (m_popupRoot != popupRoot && m_frameMesh != null)
		{
			popupRoot.ApplyPopupRendering(m_frameMesh.transform, m_popupRenderingComponents, overrideLayer: true, base.gameObject.layer);
		}
		m_popupRoot = popupRoot;
	}

	public void DisablePopupRendering()
	{
		if (m_popupRoot != null)
		{
			m_popupRoot.CleanupPopupRendering(m_popupRenderingComponents);
			m_popupRoot = null;
		}
	}

	public bool HandlesChildPropagation()
	{
		return false;
	}
}
