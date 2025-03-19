using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

[RequireComponent(typeof(PlayMakerFSM), typeof(Collider))]
public class BattlegroundsEmoteOption : MonoBehaviour
{
	public delegate void BattlegroundsEmoteOptionReadyCallback();

	[SerializeField]
	private PlayMakerFSM m_initialStatePlayMakerFSM;

	[SerializeField]
	private PlayMakerFSM m_mouseOverPlayMakerFSM;

	[SerializeField]
	private Widget m_widget;

	[SerializeField]
	private bool m_hasLeftSideBubble;

	[SerializeField]
	private Collider m_collider;

	[SerializeField]
	private AsyncReference m_asyncEmoteSpriteReference;

	[SerializeField]
	private AsyncReference m_asyncImageWidgetColliderReference;

	[SerializeField]
	private AsyncReference m_asyncAnimationOverrideReference;

	private int m_emoteId;

	private bool m_isSlotEmpty;

	private bool m_isAnimationLoaded;

	private const string InitializeEventName = "INITIALIZE";

	private const string EmptyEventName = "SLOT_EMPTY";

	private const string FilledEventName = "SLOT_FILLED";

	private const string MouseOverEventName = "MOUSE_OVER";

	private const string MouseOutEventName = "MOUSE_OUT";

	private const string OnCooldownBottomLeftEventName = "ON_COOLDOWN_BOTTOM_LEFT";

	private const string OnCooldownBottomRightEventName = "ON_COOLDOWN_BOTTOM_RIGHT";

	private const string OffCooldownBottomLeftEventName = "OFF_COOLDOWN_BOTTOM_LEFT";

	private const string OffCooldownBottomRightEventName = "OFF_COOLDOWN_BOTTOM_RIGHT";

	private const string SetBottomRightSpeechBubbleEventName = "GAMEPLAY_BOTTOM_RIGHT";

	private const string SetBottomLeftSpeechBubbleEventName = "GAMEPLAY_BOTTOM_LEFT";

	public GameObject EmoteSpriteGameObject { get; private set; }

	public GameObject BubbleSpriteGameObject { get; private set; }

	public GameObject ImageWidgetColliderGameObject { get; private set; }

	private event BattlegroundsEmoteOptionReadyCallback OnBattlegroundsEmoteOptionReady;

	private void Awake()
	{
		if (m_initialStatePlayMakerFSM == null || m_mouseOverPlayMakerFSM == null)
		{
			Debug.LogError("BattlegroundsEmoteOption: Missing required PlaymakerFSM component");
		}
		if (m_asyncImageWidgetColliderReference == null || m_asyncEmoteSpriteReference == null)
		{
			Debug.LogError("BattlegroundsEmoteOption: Missing a required AsyncReference to an image widget component");
		}
		if (m_widget == null)
		{
			Debug.LogError("BattlegroundsEmoteOption: Missing required Widget component");
		}
		if (m_collider == null)
		{
			Debug.LogError("BattlegroundsEmoteOption: Missing required Collider component");
		}
	}

	private void Start()
	{
		m_asyncEmoteSpriteReference.RegisterReadyListener<SpriteRenderer>(OnEmoteSpriteReady);
		m_asyncImageWidgetColliderReference.RegisterReadyListener<Collider>(OnImageWidgetColliderReady);
		m_asyncAnimationOverrideReference.RegisterReadyListener<AnimationOverrideWidgetBehaviour>(OnAnimationOverrideReady);
	}

	public void BindAndInitializeWidget(BattlegroundsEmoteDbfRecord dbfRecord)
	{
		if (dbfRecord == null)
		{
			Debug.Log("BattlegroundsEmoteHandler: No emote DBF record found for binding to " + m_widget.name + ". Slot will be empty.");
			m_isSlotEmpty = true;
		}
		else
		{
			BattlegroundsEmoteDataModel dataModel = new BattlegroundsEmoteDataModel
			{
				EmoteDbiId = dbfRecord.ID,
				Animation = dbfRecord.AnimationPath,
				IsAnimating = dbfRecord.IsAnimating,
				BorderType = dbfRecord.BorderType,
				XOffset = (float)dbfRecord.XOffset,
				ZOffset = (float)dbfRecord.ZOffset,
				Rarity = GameStrings.Get(GameStrings.GetRarityTextKey((TAG_RARITY)dbfRecord.Rarity))
			};
			m_widget.BindDataModel(dataModel);
			m_widget.TriggerEvent(m_hasLeftSideBubble ? "GAMEPLAY_BOTTOM_RIGHT" : "GAMEPLAY_BOTTOM_LEFT");
			m_emoteId = dbfRecord.ID;
			m_isSlotEmpty = false;
		}
		InvokeOrRegisterReadyListener(SetInitialImageWidgetState);
	}

	public void InvokeOrRegisterReadyListener(BattlegroundsEmoteOptionReadyCallback listener)
	{
		if (IsReadyToShow())
		{
			listener?.Invoke();
		}
		else
		{
			RegisterReadyListener(listener);
		}
	}

	public void RegisterReadyListener(BattlegroundsEmoteOptionReadyCallback listener)
	{
		OnBattlegroundsEmoteOptionReady += listener;
	}

	public void UnregisterReadyListener(BattlegroundsEmoteOptionReadyCallback listener)
	{
		OnBattlegroundsEmoteOptionReady -= listener;
	}

	public void SendBattlegroundsEmote()
	{
		if (GameMgr.Get().IsBattlegroundsTutorial())
		{
			int playerId = GameState.Get().GetFriendlySidePlayer().GetPlayerId();
			GameState.Get().GetGameEntity().PlayAlternateEnemyEmote(playerId, EmoteType.COLLECTIBLE_BATTLEGROUNDS_EMOTE, m_emoteId);
		}
		else
		{
			Network.Get().SendBattlegroundsEmote(EmoteType.COLLECTIBLE_BATTLEGROUNDS_EMOTE, m_emoteId);
		}
	}

	public void SetCooldown(bool isOnCooldown)
	{
		if (isOnCooldown)
		{
			m_widget.TriggerEvent(m_hasLeftSideBubble ? "ON_COOLDOWN_BOTTOM_LEFT" : "ON_COOLDOWN_BOTTOM_RIGHT");
		}
		else
		{
			m_widget.TriggerEvent(m_hasLeftSideBubble ? "OFF_COOLDOWN_BOTTOM_LEFT" : "OFF_COOLDOWN_BOTTOM_RIGHT");
		}
	}

	public void HandleMouseOver()
	{
		m_mouseOverPlayMakerFSM.SendEvent("MOUSE_OVER");
	}

	public void HandleMouseOut()
	{
		m_mouseOverPlayMakerFSM.SendEvent("MOUSE_OUT");
	}

	private void SetInitialImageWidgetState()
	{
		InitializePlayMakers();
		m_initialStatePlayMakerFSM.SendEvent(m_isSlotEmpty ? "SLOT_EMPTY" : "SLOT_FILLED");
	}

	private void InitializePlayMakers()
	{
		m_mouseOverPlayMakerFSM.SendEvent("INITIALIZE");
		m_initialStatePlayMakerFSM.SendEvent("INITIALIZE");
	}

	private bool IsReadyToShow()
	{
		if ((m_isAnimationLoaded || m_isSlotEmpty) && m_asyncEmoteSpriteReference.IsReady)
		{
			return m_asyncImageWidgetColliderReference.IsReady;
		}
		return false;
	}

	private void NotifyListenersIfReady()
	{
		if (IsReadyToShow())
		{
			this.OnBattlegroundsEmoteOptionReady?.Invoke();
		}
	}

	private void OnEmoteSpriteReady(SpriteRenderer emoteSprite)
	{
		if (emoteSprite == null)
		{
			Debug.LogError("BattlegroundsEmoteOption: Failed to load emote sprite reference.");
			return;
		}
		EmoteSpriteGameObject = emoteSprite.gameObject;
		NotifyListenersIfReady();
	}

	private void OnImageWidgetColliderReady(Collider imageWidgetCollider)
	{
		if (imageWidgetCollider == null)
		{
			Debug.LogError("BattlegroundsEmoteOption: Failed to load image widget collider reference.");
			return;
		}
		ImageWidgetColliderGameObject = imageWidgetCollider.gameObject;
		NotifyListenersIfReady();
	}

	private void OnAnimationOverrideReady(AnimationOverrideWidgetBehaviour animationOverride)
	{
		if (animationOverride == null)
		{
			Debug.LogError("BattlegroundsEmoteOption: Failed to load animation override reference.");
			return;
		}
		animationOverride.RegisterDoneChangingStatesListener(delegate
		{
			OnAnimationLoaded();
		});
	}

	private void OnAnimationLoaded()
	{
		m_isAnimationLoaded = true;
		NotifyListenersIfReady();
	}
}
