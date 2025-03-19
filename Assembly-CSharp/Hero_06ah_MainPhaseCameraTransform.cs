using System.Collections;
using Hearthstone.Core;
using UnityEngine;

public class Hero_06ah_MainPhaseCameraTransform : LegendaryHeroGenericEventHandler
{
	public Camera m_Camera;

	private Vector4 m_OriginalPosition;

	private Quaternion m_OriginalRotation;

	private bool m_CachedOriginalPosition;

	private void CacheOriginalPosition()
	{
		if (!m_CachedOriginalPosition)
		{
			m_OriginalPosition = m_Camera.transform.position;
			m_OriginalRotation = m_Camera.transform.rotation;
			m_CachedOriginalPosition = true;
		}
	}

	private void Start()
	{
		CacheOriginalPosition();
		Processor.RunCoroutine(TransformCamera());
		GameState.RegisterGameStateShutdownListener(OnGameStateShutdown);
	}

	private void OnDestroy()
	{
		GameState.UnregisterGameStateShutdownListener(OnGameStateShutdown);
	}

	private IEnumerator TransformCamera()
	{
		GameState gameState = GameState.Get();
		if (gameState != null)
		{
			while (gameState.IsMulliganPhasePending())
			{
				yield return null;
			}
			SetCameraTransform();
		}
	}

	private void OnGameStateShutdown(GameState gameState, object userData)
	{
		ResetCameraTransform();
	}

	public override void HandleEvent(string eventName, object eventData)
	{
		Actor actor = eventData as Actor;
		if (!(actor == null) && actor.GetActorStateType() != ActorStateType.CARD_HISTORY)
		{
			if (eventName == LegendaryHeroGenericEvents.Cthun_Main_Phase_Camera_Transform)
			{
				SetCameraTransform();
			}
			else if (eventName == LegendaryHeroGenericEvents.Cthun_Reset_Main_Phase_Camera_Transform)
			{
				ResetCameraTransform();
			}
		}
	}

	public void SetCameraTransform()
	{
		CacheOriginalPosition();
		m_Camera.transform.position = base.gameObject.transform.position;
		m_Camera.transform.rotation = base.gameObject.transform.rotation;
	}

	public void ResetCameraTransform()
	{
		m_Camera.transform.position = m_OriginalPosition;
		m_Camera.transform.rotation = m_OriginalRotation;
	}
}
