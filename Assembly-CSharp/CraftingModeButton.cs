using System;
using System.Collections;
using System.Threading;
using Blizzard.T5.MaterialService.Extensions;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CraftingModeButton : UIBButton
{
	public GameObject m_dustBottle;

	public GameObject m_activeGlow;

	public ParticleSystem m_dustShower;

	public Vector3 m_jarJiggleRotation = new Vector3(0f, 30f, 0f);

	public GameObject m_textObject;

	public MeshRenderer m_mainMesh;

	public Material m_enabledMaterial;

	public Material m_disabledMaterial;

	public bool m_shouldHideWholeButton;

	private bool m_isGlowEnabled;

	private bool m_showDustBottle;

	private bool m_hasStartedJiggleAnimation;

	private bool m_isDestroyed;

	private CancellationTokenSource m_jiggleTokenSource;

	public void ShowActiveGlow(bool show)
	{
		m_isGlowEnabled = show;
		m_activeGlow.SetActive(show);
	}

	public void ShowDustBottle(bool show, bool forceMobileActive)
	{
		m_showDustBottle = show;
		m_dustBottle.SetActive(show || (forceMobileActive && (bool)UniversalInputManager.UsePhoneUI));
		if (show)
		{
			StartBottleJiggle();
		}
	}

	private void StartBottleJiggle()
	{
		if (!m_hasStartedJiggleAnimation)
		{
			BottleJiggle();
		}
	}

	private void BottleJiggle()
	{
		if (m_isDestroyed)
		{
			TelemetryManager.Client().SendLiveIssue("Collections_CraftingModeButton_BottleJiggle", "BottleJiggle called on CraftingModeButton after object was destroyed");
			return;
		}
		if (m_jiggleTokenSource == null)
		{
			m_jiggleTokenSource = new CancellationTokenSource();
		}
		try
		{
			Jiggle(m_jiggleTokenSource.Token).Forget();
		}
		catch (Exception ex)
		{
			TelemetryManager.Client().SendLiveIssue("Collections_CraftingModeButton_BottleJiggle", "Caught exception '" + ex.Message + "'. Has the token somehow been disposed without destroying the gameobject?");
		}
	}

	private async UniTaskVoid Jiggle(CancellationToken token)
	{
		m_hasStartedJiggleAnimation = true;
		await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
		m_dustShower.Play();
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("amount", m_jarJiggleRotation);
		args.Add("time", 0.5f);
		args.Add("oncomplete", "BottleJiggle");
		args.Add("oncompletetarget", base.gameObject);
		iTween.PunchRotation(m_dustBottle.gameObject, args);
	}

	public void Enable(bool enabled)
	{
		if (IsEnabled() == enabled)
		{
			return;
		}
		SetEnabled(enabled);
		if (m_shouldHideWholeButton)
		{
			Flip(enabled);
			return;
		}
		m_activeGlow.SetActive(enabled && m_isGlowEnabled);
		m_dustShower.gameObject.SetActive(enabled);
		m_dustBottle.SetActive(enabled && (m_showDustBottle || (bool)UniversalInputManager.UsePhoneUI));
		if (m_textObject != null)
		{
			m_textObject.SetActive(enabled);
		}
		if (m_mainMesh != null)
		{
			m_mainMesh.SetSharedMaterial(enabled ? m_enabledMaterial : m_disabledMaterial);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		m_hasStartedJiggleAnimation = false;
		m_jiggleTokenSource?.Cancel();
		m_jiggleTokenSource?.Dispose();
		m_jiggleTokenSource = null;
		m_isDestroyed = true;
	}
}
