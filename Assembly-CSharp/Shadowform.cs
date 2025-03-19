using System;
using System.Collections;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class Shadowform : SuperSpell
{
	public Material m_ShadowformMaterial;

	public int m_MaterialIndex = 1;

	public float m_FadeInTime = 1f;

	public float m_Desaturate = 0.8f;

	public Color m_Tint = new Color(0.69140625f, 21f / 64f, 103f / 128f, 1f);

	public float m_Contrast = -0.29f;

	public float m_Intensity = 0.85f;

	public float m_FxIntensity = 4f;

	private Material m_MaterialInstance;

	private Material m_OriginalMaterial;

	private Coroutine m_StartFXCoroutine;

	protected override void OnBirth(SpellStateType prevStateType)
	{
		base.OnBirth(prevStateType);
		OnSpellFinished();
		if (!(m_ShadowformMaterial == null))
		{
			if (m_StartFXCoroutine != null)
			{
				StopCoroutine(m_StartFXCoroutine);
			}
			m_StartFXCoroutine = StartCoroutine(StartShadowformFX());
		}
	}

	private IEnumerator StartShadowformFX()
	{
		Actor actor = GameObjectUtils.FindComponentInThisOrParents<Actor>(this);
		m_OriginalMaterial = actor.GetPortraitMaterial();
		yield return null;
		actor.SetShadowform(shadowform: true);
		m_MaterialInstance = new Material(m_ShadowformMaterial);
		Texture portraitTexture = ((actor.LegendaryHeroPortrait != null) ? actor.LegendaryHeroPortrait.PortraitTexture : actor.GetStaticPortraitTexture());
		m_MaterialInstance.mainTexture = portraitTexture;
		actor.SetPortraitMaterial(m_MaterialInstance);
		actor.LegendaryHeroPortrait?.UpdateDynamicResolutionControllers();
		GameObject portraitMesh = actor.GetPortraitMesh();
		Material mat = portraitMesh.GetComponent<Renderer>().GetMaterial(actor.m_portraitMatIdx);
		Action<object> DesaturationUpdate = delegate(object desat)
		{
			mat.SetFloat("_Desaturate", (float)desat);
		};
		Hashtable desatArgs = iTweenManager.Get().GetTweenHashTable();
		desatArgs.Add("time", m_FadeInTime);
		desatArgs.Add("from", 0f);
		desatArgs.Add("to", m_Desaturate);
		desatArgs.Add("onupdate", DesaturationUpdate);
		desatArgs.Add("onupdatetarget", actor.gameObject);
		iTween.ValueTo(actor.gameObject, desatArgs);
		Action<object> colorUpdate = delegate(object col)
		{
			mat.SetColor("_Color", (Color)col);
		};
		Hashtable colorArgs = iTweenManager.Get().GetTweenHashTable();
		colorArgs.Add("time", m_FadeInTime);
		colorArgs.Add("from", Color.white);
		colorArgs.Add("to", m_Tint);
		colorArgs.Add("onupdate", colorUpdate);
		colorArgs.Add("onupdatetarget", actor.gameObject);
		iTween.ValueTo(actor.gameObject, colorArgs);
		Action<object> contrastUpdate = delegate(object desat)
		{
			mat.SetFloat("_Contrast", (float)desat);
		};
		Hashtable contrastArgs = iTweenManager.Get().GetTweenHashTable();
		contrastArgs.Add("time", m_FadeInTime);
		contrastArgs.Add("from", 0f);
		contrastArgs.Add("to", m_Contrast);
		contrastArgs.Add("onupdate", contrastUpdate);
		contrastArgs.Add("onupdatetarget", actor.gameObject);
		iTween.ValueTo(actor.gameObject, contrastArgs);
		Action<object> intesityUpdate = delegate(object desat)
		{
			mat.SetFloat("_Intensity", (float)desat);
		};
		Hashtable intensityArgs = iTweenManager.Get().GetTweenHashTable();
		intensityArgs.Add("time", m_FadeInTime);
		intensityArgs.Add("from", 1f);
		intensityArgs.Add("to", m_Intensity);
		intensityArgs.Add("onupdate", intesityUpdate);
		intensityArgs.Add("onupdatetarget", actor.gameObject);
		iTween.ValueTo(actor.gameObject, intensityArgs);
		Action<object> fxIntesityUpdate = delegate(object desat)
		{
			mat.SetFloat("_FxIntensity", (float)desat);
		};
		Hashtable fxIntensityArgs = iTweenManager.Get().GetTweenHashTable();
		fxIntensityArgs.Add("time", m_FadeInTime);
		fxIntensityArgs.Add("from", 0f);
		fxIntensityArgs.Add("to", m_FxIntensity);
		fxIntensityArgs.Add("onupdate", fxIntesityUpdate);
		fxIntensityArgs.Add("onupdatetarget", actor.gameObject);
		iTween.ValueTo(actor.gameObject, fxIntensityArgs);
	}

	protected override void OnDeath(SpellStateType prevStateType)
	{
		base.OnDeath(prevStateType);
		if (m_StartFXCoroutine != null)
		{
			StopCoroutine(m_StartFXCoroutine);
		}
		Actor actor = GameObjectUtils.FindComponentInThisOrParents<Actor>(this);
		actor.SetShadowform(shadowform: false);
		actor.UpdateAllComponents();
		actor.SetPortraitMaterial(m_OriginalMaterial);
	}
}
