using Blizzard.T5.AssetManager;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Triggers an event on the FSM of the Legendary Hero object attached to an actor")]
[ActionCategory("Pegasus")]
public class TriggerHeroCardDefReplacement : FsmStateAction
{
	[Tooltip("Actors game object")]
	[RequiredField]
	[CheckForComponent(typeof(Actor))]
	public FsmOwnerDefault m_GameObject;

	public string m_NewCardDefAsset;

	public bool m_LoadCardDefTextures = true;

	public TAG_PREMIUM m_LoadCardDefTexturePremium = TAG_PREMIUM.GOLDEN;

	private DefLoader.DisposableCardDef m_NewCardDef;

	public override void Awake()
	{
		base.Awake();
		if (m_NewCardDef != null)
		{
			m_NewCardDef.Dispose();
			m_NewCardDef = null;
		}
		AssetHandle<GameObject> go = AssetLoader.Get()?.LoadAsset<GameObject>(m_NewCardDefAsset, AssetLoadingOptions.IgnorePrefabPosition);
		if ((bool)go)
		{
			m_NewCardDef = new DefLoader.DisposableCardDef(go);
			if (m_LoadCardDefTextures)
			{
				CardTextureLoader.Load(m_NewCardDef.CardDef, new CardPortraitQuality(3, m_LoadCardDefTexturePremium));
			}
		}
	}

	public override void OnEnter()
	{
		GameObject go = base.Fsm.GetOwnerDefaultTarget(m_GameObject);
		if (!go)
		{
			Finish();
			return;
		}
		Actor actor = go.GetComponent<Actor>();
		if (actor == null)
		{
			global::Log.Spells.PrintError("{0}.OnEnter() - FAILED to find actor on game object {1}", this, m_GameObject);
			Finish();
			return;
		}
		Card card = actor.GetCard();
		if (card == null)
		{
			global::Log.Spells.PrintError("{0}.OnEnter() - FAILED to find card on actor {1}", this, m_GameObject);
			Finish();
		}
		else
		{
			card.SetCardDef(m_NewCardDef, updateActor: false);
			Board.Get().UpdateCustomHeroTray(card.GetControllerSide());
		}
	}
}
