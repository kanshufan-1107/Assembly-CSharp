using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using UnityEngine;

public class SpellManager : IService
{
	private SpellCache m_spellCache;

	public static SpellManager Get()
	{
		if (!ServiceManager.TryGet<SpellManager>(out var spellManager) && !Application.isEditor)
		{
			return null;
		}
		return spellManager;
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		yield return new ServiceSoftDependency(typeof(SceneMgr), serviceLocator);
		if (serviceLocator.TryGetService<SceneMgr>(out var sceneMgr))
		{
			sceneMgr.RegisterScenePreLoadEvent(OnScenePreLoad);
		}
		m_spellCache = new SpellCache();
	}

	public Type[] GetDependencies()
	{
		return new Type[1] { typeof(IAssetLoader) };
	}

	public void Shutdown()
	{
		Clear();
	}

	public void Clear()
	{
		m_spellCache.Clear();
	}

	public Spell GetSpell(string spellAssetRef)
	{
		return CreateSpell(spellAssetRef);
	}

	public Spell GetSpell(Spell spell)
	{
		return UnityEngine.Object.Instantiate(spell);
	}

	public bool ReleaseSpell(Spell spell, bool reset = false)
	{
		if (spell == null || spell.IsDestroyed)
		{
			if (!spell.IsDestroyed)
			{
				Error.AddDevWarningNonRepeating("Spell Pooling", "Spell was null and could not be released.");
			}
			return false;
		}
		if (reset)
		{
			spell.ReleaseSpell();
		}
		DestroySpell(spell);
		return true;
	}

	public SpellTable GetSpellTable(string prefabPath)
	{
		return m_spellCache.GetSpellTable(prefabPath);
	}

	private void OnScenePreLoad(SceneMgr.Mode previousMode, SceneMgr.Mode nextMode, object userData)
	{
		if (previousMode == SceneMgr.Mode.GAMEPLAY && nextMode != SceneMgr.Mode.GAMEPLAY)
		{
			Clear();
		}
		if (nextMode == SceneMgr.Mode.HUB)
		{
			Clear();
		}
	}

	private Spell CreateSpell(string spellAssetRef)
	{
		Spell spell = null;
		if (GameUtils.IsOnVFXDenylist(spellAssetRef))
		{
			Error.AddDevWarning("Spell Manager", "Spell spellAssetRef '{0}' was on the VFX Denylist", spellAssetRef);
			return null;
		}
		GameObject gameObject = AssetLoader.Get()?.InstantiatePrefab(spellAssetRef);
		if (gameObject != null)
		{
			spell = gameObject.GetComponent<Spell>();
		}
		if (spell == null)
		{
			Error.AddDevWarning("Spell Manager", "Spell could not be found with spellAssetRef '{0}'", spellAssetRef);
			return null;
		}
		return spell;
	}

	private static void DestroySpell(Spell spell)
	{
		spell.IsDestroyed = true;
		UnityEngine.Object.Destroy(spell.gameObject);
	}
}
