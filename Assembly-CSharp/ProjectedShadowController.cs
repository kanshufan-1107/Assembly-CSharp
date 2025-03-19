using System.Collections;
using Blizzard.T5.Game.Spells;
using Blizzard.T5.Services;
using UnityEngine;

public class ProjectedShadowController : MonoBehaviour
{
	private Actor actor;

	private Card card;

	private ISpell customSpawnSpell;

	private ISpell customSummonSpell;

	private IGraphicsManager graphicManager;

	private bool isRootObjectProjectedShadowEnabled;

	private float initializationTime;

	public ProjectedShadow rootObjectProjectedShadow;

	private void Awake()
	{
		graphicManager = ServiceManager.Get<IGraphicsManager>();
	}

	private void Start()
	{
		initializationTime = Time.timeSinceLevelLoad;
		isRootObjectProjectedShadowEnabled = false;
		if (graphicManager != null && graphicManager.RenderQualityLevel != GraphicsQuality.High)
		{
			StartCoroutine(getSpawnSpell());
		}
	}

	private void LateUpdate()
	{
		if (!isRootObjectProjectedShadowEnabled && rootObjectProjectedShadow.enabled)
		{
			rootObjectProjectedShadow.enabled = false;
		}
	}

	private IEnumerator getSpawnSpell()
	{
		actor = GetComponent<Actor>();
		while (card == null)
		{
			card = actor.GetCard();
			if (Time.timeSinceLevelLoad - initializationTime > 2f)
			{
				yield break;
			}
			yield return null;
		}
		customSummonSpell = card.GetCustomSummonSpell();
		if (customSummonSpell != null)
		{
			enableRootShadow();
			if (customSpawnSpell is Spell spell)
			{
				spell.AddFinishedCallback(disableRootShadow);
			}
		}
		customSpawnSpell = card.GetCustomSpawnSpellOverride();
		if (customSpawnSpell == null)
		{
			customSpawnSpell = card.GetCustomSpawnSpell();
		}
		if (customSpawnSpell != null)
		{
			enableRootShadow();
			if (customSpawnSpell is Spell spell2)
			{
				spell2.AddFinishedCallback(disableRootShadow);
			}
		}
	}

	private void enableRootShadow()
	{
		rootObjectProjectedShadow.enabled = true;
		isRootObjectProjectedShadowEnabled = true;
	}

	private void disableRootShadow(Spell spell, object userData)
	{
		rootObjectProjectedShadow.enabled = false;
		isRootObjectProjectedShadowEnabled = false;
	}
}
