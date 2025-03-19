using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.UI;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
	public enum PopupTextType
	{
		BASIC,
		FANCY
	}

	public enum TutorialPopupType
	{
		BASIC,
		IMPORTANT,
		GRAPHIC
	}

	public enum VisualEmoteType
	{
		NONE,
		HOT_STREAK,
		TRIPLE,
		TECH_UP_01,
		TECH_UP_02,
		TECH_UP_03,
		TECH_UP_04,
		TECH_UP_05,
		TECH_UP_06,
		BATTLEGROUNDS_01,
		BATTLEGROUNDS_02,
		BATTLEGROUNDS_03,
		BATTLEGROUNDS_04,
		BATTLEGROUNDS_05,
		BATTLEGROUNDS_06,
		BANANA,
		HERO_BUDDY,
		DOUBLE_HERO_BUDDY,
		COLLECTIBLE_BATTLEGROUNDS_EMOTE,
		QUEST_COMPLETE,
		STORE,
		TECH_UP_07
	}

	public class SpeechBubbleOptions
	{
		public string speechText = "";

		public Notification.SpeechBubbleDirection direction = Notification.SpeechBubbleDirection.BottomLeft;

		public Actor actor;

		public bool destroyWhenNewCreated = true;

		public bool parentToActor = true;

		public float bubbleScale;

		public VisualEmoteType visualEmoteType;

		public int speechBubbleGroup;

		public Action<int> finishCallback;

		public float emoteDuration;

		public int battlegroundsEmoteId;

		public SpeechBubbleOptions WithSpeechText(string speechText)
		{
			this.speechText = speechText;
			return this;
		}

		public SpeechBubbleOptions WithSpeechBubbleDirection(Notification.SpeechBubbleDirection direction)
		{
			this.direction = direction;
			return this;
		}

		public SpeechBubbleOptions WithActor(Actor actor)
		{
			this.actor = actor;
			return this;
		}

		public SpeechBubbleOptions WithParentToActor(bool parentToActor)
		{
			this.parentToActor = parentToActor;
			return this;
		}

		public SpeechBubbleOptions WithDestroyWhenNewCreated(bool destroyWhenNewCreated)
		{
			this.destroyWhenNewCreated = destroyWhenNewCreated;
			return this;
		}

		public SpeechBubbleOptions WithBubbleScale(float bubbleScale)
		{
			this.bubbleScale = bubbleScale;
			return this;
		}

		public SpeechBubbleOptions WithVisualEmoteType(VisualEmoteType visualEmoteType)
		{
			this.visualEmoteType = visualEmoteType;
			return this;
		}

		public SpeechBubbleOptions WithSpeechBubbleGroup(int speechBubbleGroup)
		{
			this.speechBubbleGroup = speechBubbleGroup;
			return this;
		}

		public SpeechBubbleOptions WithFinishCallback(Action<int> finishCallback)
		{
			this.finishCallback = finishCallback;
			return this;
		}

		public SpeechBubbleOptions WithEmoteDuration(float emoteDuration)
		{
			this.emoteDuration = emoteDuration;
			return this;
		}

		public SpeechBubbleOptions WithBattlegroundsEmoteId(int id)
		{
			battlegroundsEmoteId = id;
			return this;
		}
	}

	private class QuoteSoundCallbackData
	{
		public Notification m_quote;

		public float m_durationSeconds;

		public bool m_persistCharacter;
	}

	public const string KT_PREFAB_PATH = "KT_Quote.prefab:7ad118a1a10e9ab409ade82268a378f5";

	public const string TIRION_PREFAB_PATH = "Tirion_Quote.prefab:2f88f08e8896841429c972fc5c4c7088";

	public const string NORMAL_NEFARIAN_PREFAB_PATH = "NormalNefarian_Quote.prefab:708840e536eb141479a23b632ebcc913";

	public const string ZOMBIE_NEFARIAN_PREFAB_PATH = "NefarianDragon_Quote.prefab:179fec888df7e4c02b8de3b7ad109a23";

	public const string RAGNAROS_PREFAB_PATH = "Ragnaros_Quote.prefab:c9e0154894cd1a946b90ebefeb481a51";

	public const string MAJORDOMO_PREFAB_PATH = "Majordomo_Quote.prefab:72286f87e5b724c21aa1d92d04426614";

	public const string RENO_PREFAB_PATH = "Reno_Quote.prefab:0a2e34fa6782a0747b4f5d5574d1331a";

	public const string RENO_BIG_PREFAB_PATH = "Reno_BigQuote.prefab:63a25676d5e84264a9eb9c3d5c7e0921";

	public const string CARTOGRAPHER_PREFAB_PATH = "Cartographer_Quote.prefab:c6056bfb8c0025a458553adabc8ed537";

	public const string ELISE_BIG_PREFAB_PATH = "Elise_BigQuote.prefab:932bc9e74bb49e047ae8dd480492db26";

	public const string FINLEY_BIG_PREFAB_PATH = "Finley_BigQuote.prefab:1c1c332cf5009194cb7dd7316c465aee";

	public const string BRANN_BIG_PREFAB_PATH = "Brann_BigQuote.prefab:a03dd286404083c439e371ba84d7a82b";

	public const string RAFAAM_WRAP_PREFAB_PATH = "Rafaam_wrap_Quote.prefab:d7100015bf618604ea93bad6b9f54f8b";

	public const string RAFAAM_WRAP_BIG_PREFAB_PATH = "Rafaam_wrap_BigQuote.prefab:ee7dbbb027adc1947b64b05f31d4c124";

	public const string RAFAAM_BIG_PREFAB_PATH = "Rafaam_BigQuote.prefab:ff1fd65bf3d8ba748b144b805fca871f";

	public const string RAFAAM_PREFAB_PATH = "Rafaam_Quote.prefab:d27a824bbfd6bd94185fe10e594f0014";

	public const string BRANN_PREFAB_PATH = "Brann_Quote.prefab:2c11651ab7740924189734944b8d7089";

	public const string BLAGGH_PREFAB_PATH = "Blaggh_Quote.prefab:f5d1e7053e6368e4a930ca3906cff53a";

	public const string MEDIVH_PREFAB_PATH = "Medivh_Quote.prefab:423c4a6b7e7a7f643bf0b2992ad3d31b";

	public const string MEDIVH_BIG_PREFAB_PATH = "Medivh_BigQuote.prefab:78e18a627031f6c48aef27a0fa1123c1";

	public const string MEDIVAS_BIG_PREFAB_PATH = "Medivas_BigQuote.prefab:ad677b060790a304fa6caed25f19bf88";

	public const string MOROES_PREFAB_PATH = "Moroes_Quote.prefab:ea3a21837aab2b0448ce4090103724cf";

	public const string MOROES_BIG_PREFAB_PATH = "Moroes_BigQuote.prefab:321274c1b67d79a4ba421a965bbc9e6d";

	public const string CURATOR_PREFAB_PATH = "Curator_Quote.prefab:ab58be80382875e4cbaa766fda73cd39";

	public const string CURATOR_BIG_PREFAB_PATH = "Curator_BigQuote.prefab:f01875528133988418925bd870aa7b81";

	public const string BARNES_PREFAB_PATH = "Barnes_Quote.prefab:2e7e9f28b5bc37149a12b2e5feaa244a";

	public const string BARNES_BIG_PREFAB_PATH = "Barnes_BigQuote.prefab:15c396b2577ab09449f3721d23da3dba";

	public const string AYA_BIG_PREFAB_PATH = "Aya_BigQuote.prefab:26a19c2632327c14dbf648b96f8751d1";

	public const string HANCHO_BIG_PREFAB_PATH = "HanCho_BigQuote.prefab:0b24275caed054c45b2ebcb91fd9112d";

	public const string KAZAKUS_BIG_PREFAB_PATH = "Kazakus_BigQuote.prefab:b0007ae4277fc5a40a8c6f8c774ab823";

	public const string LICHKING_PREFAB_PATH = "LichKing_Quote.prefab:59d5b461e0b2bbe479b7db63e0962d30";

	public const string TIRION_BIG_PREFAB_PATH = "Tirion_BigQuote.prefab:878fcebc1cddaf24f828c44edb07f7f8";

	public const string AHUNE_BIG_PREFAB_PATH = "Ahune_BigQuote.prefab:00dd8f83adda33345ac291cc76241482";

	public const string RAGNAROS_BIG_PREFAB_PATH = "Ragnaros_BigQuote.prefab:843c4fab946192943a909b026f755505";

	public const string DEMON_HUNTER_ILLIDAN_PREFAB_PATH = "DemonHunter_Illidan_Popup_Banner.prefab:c2b08a2b89af02e4bb9e80b08526df7a";

	public static readonly float DEPTH = -15f;

	public static readonly Vector3 LEFT_OF_FRIENDLY_HERO = new Vector3(-1f, 0f, 1f);

	public static readonly Vector3 RIGHT_OF_FRIENDLY_HERO = new Vector3(-6f, 0f, 1f);

	public static readonly Vector3 LEFT_OF_ENEMY_HERO = new Vector3(-1f, 0f, -3.5f);

	public static readonly Vector3 RIGHT_OF_ENEMY_HERO = new Vector3(-6f, 0f, -3f);

	public static readonly Vector3 DEFAULT_CHARACTER_POS = new Vector3(100f, DEPTH, 24.7f);

	public static readonly Vector3 DEFAULT_BANNER_POS = new Vector3(0f, DEPTH, 0f);

	public static readonly Vector3 CHARACTER_POS_ABOVE_QUEST_TOAST = new Vector3(100f, 50f, 24.7f);

	public static readonly Vector3 ALT_ADVENTURE_SCREEN_POS = new Vector3(104.8f, DEPTH, 131.1f);

	public static readonly Vector3 PHONE_CHARACTER_POS = new Vector3(124.1f, DEPTH, 24.7f);

	public static readonly float PHONE_OVERLAY_UI_CHARACTER_X_OFFSET = -0.5f;

	public static readonly float DEFAULT_BANNER_OFFSET_Z = 24.7f;

	public static readonly float DEFAULT_BANNER_OFFSET_X = 50f;

	public GameObject speechBubblePrefab;

	public GameObject speechIndicatorPrefab;

	public GameObject bounceArrowPrefab;

	public GameObject fadeArrowPrefab;

	public GameObject popupTextPrefab;

	public GameObject fancyPopupTextPrefab;

	public GameObject dialogBoxPrefab;

	public GameObject innkeeperQuotePrefab;

	public AsyncReference tutorialTooltipPrefab;

	[SerializeField]
	private GameObject m_battlegroundsEmoteNotificationPrefab;

	[SerializeField]
	private GameObject m_storeNotificationPrefab;

	private static NotificationManager s_instance;

	private Map<int, List<Notification>> notificationsToDestroyUponNewNotifier;

	private Map<int, List<Notification>> speechBubbleNotToDestoryUponNewNotifier;

	private List<Notification> arrows;

	private List<Notification> popUpTexts;

	private Notification popUpDialog;

	private Notification m_quote;

	private List<string> m_quotesThisSession;

	private const float DEFAULT_QUOTE_DURATION = 8f;

	private Vector3 NOTIFICATION_SCALE = 0.163f * Vector3.one;

	private Vector3 NOTIFICATION_SCALE_PHONE = 0.326f * Vector3.one;

	private Widget m_tutorialNotificationWidget;

	private Notification m_tutorialNotification;

	public static Vector3 NOTIFICATITON_WORLD_SCALE
	{
		get
		{
			if (!UniversalInputManager.UsePhoneUI)
			{
				return 18f * Vector3.one;
			}
			return 25f * Vector3.one;
		}
	}

	public bool IsQuotePlaying => m_quote != null;

	public static Vector3 GetDefaultDialogueBannerPos(CanvasAnchor anchor)
	{
		Vector3 pos = DEFAULT_BANNER_POS;
		switch (anchor)
		{
		case CanvasAnchor.BOTTOM:
		case CanvasAnchor.BOTTOM_LEFT:
		case CanvasAnchor.BOTTOM_RIGHT:
			pos += Vector3.forward * DEFAULT_BANNER_OFFSET_Z;
			break;
		case CanvasAnchor.TOP:
		case CanvasAnchor.TOP_LEFT:
		case CanvasAnchor.TOP_RIGHT:
			pos -= Vector3.forward * DEFAULT_BANNER_OFFSET_Z;
			break;
		}
		switch (anchor)
		{
		case CanvasAnchor.LEFT:
		case CanvasAnchor.BOTTOM_LEFT:
		case CanvasAnchor.TOP_LEFT:
			pos += Vector3.right * DEFAULT_BANNER_OFFSET_X;
			break;
		case CanvasAnchor.RIGHT:
		case CanvasAnchor.BOTTOM_RIGHT:
		case CanvasAnchor.TOP_RIGHT:
			pos -= Vector3.right * DEFAULT_BANNER_OFFSET_X;
			break;
		}
		return pos;
	}

	private void Awake()
	{
		s_instance = this;
		m_quotesThisSession = new List<string>();
		tutorialTooltipPrefab.RegisterReadyListener<Widget>(OnTutorialTooltipPrefabReady);
	}

	private void OnDestroy()
	{
		s_instance = null;
	}

	private void Start()
	{
		notificationsToDestroyUponNewNotifier = new Map<int, List<Notification>>();
		speechBubbleNotToDestoryUponNewNotifier = new Map<int, List<Notification>>();
		arrows = new List<Notification>();
		popUpTexts = new List<Notification>();
	}

	public static NotificationManager Get()
	{
		return s_instance;
	}

	public Notification CreatePopupDialog(string headlineText, string bodyText, string yesOrOKButtonText, string noButtonText)
	{
		return CreatePopupDialog(headlineText, bodyText, yesOrOKButtonText, noButtonText, new Vector3(0f, 0f, 0f));
	}

	public Notification CreatePopupDialog(string headlineText, string bodyText, string yesOrOKButtonText, string noButtonText, Vector3 offset)
	{
		if (popUpDialog != null)
		{
			UnityEngine.Object.Destroy(popUpDialog.gameObject);
		}
		GameObject popupDialogObject = UnityEngine.Object.Instantiate(dialogBoxPrefab);
		Vector3 cameraPosition = Camera.main.transform.position;
		popupDialogObject.transform.position = cameraPosition + new Vector3(-0.07040818f, -16.10709f, 1.79612f) + offset;
		popUpDialog = popupDialogObject.GetComponent<Notification>();
		popUpDialog.ChangeDialogText(headlineText, bodyText, yesOrOKButtonText, noButtonText);
		popUpDialog.PlayBirth();
		UniversalInputManager.Get().SetGameDialogActive(active: true);
		return popUpDialog;
	}

	public Notification CreateSpeechBubble(string speechText, Actor actor)
	{
		return CreateSpeechBubble(speechText, Notification.SpeechBubbleDirection.BottomLeft, actor, bDestroyWhenNewCreated: false);
	}

	public Notification CreateSpeechBubble(string speechText, Actor actor, bool bDestroyWhenNewCreated)
	{
		return CreateSpeechBubble(speechText, Notification.SpeechBubbleDirection.BottomLeft, actor, bDestroyWhenNewCreated);
	}

	public Notification CreateSpeechBubble(string speechText, Notification.SpeechBubbleDirection direction, Actor actor)
	{
		return CreateSpeechBubble(speechText, direction, actor, bDestroyWhenNewCreated: false);
	}

	public Notification CreateSpeechBubble(string speechText, Notification.SpeechBubbleDirection direction, Actor actor, bool bDestroyWhenNewCreated, bool parentToActor = true, float bubbleScale = 0f)
	{
		SpeechBubbleOptions options = new SpeechBubbleOptions().WithSpeechText(speechText).WithSpeechBubbleDirection(direction).WithActor(actor)
			.WithDestroyWhenNewCreated(bDestroyWhenNewCreated)
			.WithParentToActor(parentToActor)
			.WithBubbleScale(bubbleScale);
		return CreateSpeechBubble(options);
	}

	public bool HasNonDestroyableSpeechBubbleExisting(Notification.SpeechBubbleDirection direction, int speechBubbleGroup)
	{
		if (speechBubbleNotToDestoryUponNewNotifier.Count == 0 || !speechBubbleNotToDestoryUponNewNotifier.ContainsKey(speechBubbleGroup) || speechBubbleNotToDestoryUponNewNotifier[speechBubbleGroup] == null)
		{
			return false;
		}
		for (int i = 0; i < speechBubbleNotToDestoryUponNewNotifier[speechBubbleGroup].Count; i++)
		{
			if (!(speechBubbleNotToDestoryUponNewNotifier[speechBubbleGroup][i] == null) && speechBubbleNotToDestoryUponNewNotifier[speechBubbleGroup][i].GetSpeechBubbleDirection() == direction)
			{
				return true;
			}
		}
		return false;
	}

	public Notification CreateSpeechBubble(SpeechBubbleOptions options)
	{
		if (options.destroyWhenNewCreated && HasNonDestroyableSpeechBubbleExisting(options.direction, options.speechBubbleGroup))
		{
			return null;
		}
		DestroyOtherNotifications(options.direction, options.speechBubbleGroup);
		Notification speechBubble;
		if (options.speechText == "" && options.visualEmoteType == VisualEmoteType.NONE)
		{
			speechBubble = UnityEngine.Object.Instantiate(speechIndicatorPrefab).GetComponent<Notification>();
			speechBubble.PlaySmallBirthForFakeBubble();
			speechBubble.SetPositionForSmallBubble(options.actor);
			if (!Cheats.Get().IsSpeechBubbleEnabled())
			{
				speechBubble.SetPosition(Cheats.Get().SPEECH_BUBBLE_HIDDEN_POSITION);
			}
		}
		else if (options.visualEmoteType == VisualEmoteType.COLLECTIBLE_BATTLEGROUNDS_EMOTE)
		{
			speechBubble = UnityEngine.Object.Instantiate(m_battlegroundsEmoteNotificationPrefab).GetComponent<Notification>();
			if (speechBubble is BattlegroundsEmoteNotification battlegroundsEmoteNotification)
			{
				battlegroundsEmoteNotification.BindEmoteDataModel(options.battlegroundsEmoteId);
			}
			else
			{
				Debug.LogError("NotificationManager: Could not find BattlegroundsEmoteNotification component on emote notification prefab.");
			}
			speechBubble.SetPosition(options.actor, options.direction);
			speechBubble.PlayBirth();
		}
		else if (options.visualEmoteType == VisualEmoteType.STORE)
		{
			speechBubble = UnityEngine.Object.Instantiate(m_storeNotificationPrefab).GetComponent<Notification>();
			speechBubble.ChangeText(options.speechText);
			speechBubble.FaceDirection(options.direction);
			speechBubble.SetPosition(options.actor, options.direction);
			speechBubble.PlayBirth();
		}
		else
		{
			speechBubble = UnityEngine.Object.Instantiate(speechBubblePrefab).GetComponent<Notification>();
			if (options.visualEmoteType == VisualEmoteType.NONE)
			{
				speechBubble.ChangeText(options.speechText);
				speechBubble.ChangeEmote(VisualEmoteType.NONE);
			}
			else
			{
				speechBubble.ChangeText("");
				speechBubble.ChangeEmote(options.visualEmoteType);
			}
			speechBubble.FaceDirection(options.direction);
			speechBubble.PlayBirth();
			speechBubble.SetPosition(options.actor, options.direction);
			if (!Cheats.Get().IsSpeechBubbleEnabled() && options.visualEmoteType == VisualEmoteType.NONE)
			{
				speechBubble.SetPosition(Cheats.Get().SPEECH_BUBBLE_HIDDEN_POSITION);
			}
			if (!Mathf.Approximately(options.bubbleScale, 0f))
			{
				GameObject speechScaleOffset = new GameObject();
				speechScaleOffset.transform.SetParent(options.actor.transform);
				TransformUtil.Identity(speechScaleOffset);
				speechBubble.SetParentOffsetObject(speechScaleOffset);
				speechScaleOffset.transform.localScale = new Vector3(options.bubbleScale, options.bubbleScale, options.bubbleScale);
			}
		}
		if (options.destroyWhenNewCreated)
		{
			if (!notificationsToDestroyUponNewNotifier.ContainsKey(options.speechBubbleGroup))
			{
				notificationsToDestroyUponNewNotifier.Add(options.speechBubbleGroup, new List<Notification>());
			}
			notificationsToDestroyUponNewNotifier[options.speechBubbleGroup].Add(speechBubble);
		}
		else
		{
			if (!speechBubbleNotToDestoryUponNewNotifier.ContainsKey(options.speechBubbleGroup))
			{
				speechBubbleNotToDestoryUponNewNotifier.Add(options.speechBubbleGroup, new List<Notification>());
			}
			speechBubbleNotToDestoryUponNewNotifier[options.speechBubbleGroup].Add(speechBubble);
		}
		if (options.parentToActor)
		{
			speechBubble.transform.parent = options.actor.transform;
		}
		if (options.finishCallback != null)
		{
			Notification notification = speechBubble;
			notification.OnFinishDeathState = (Action<int>)Delegate.Combine(notification.OnFinishDeathState, options.finishCallback);
		}
		if (options.emoteDuration > 0f)
		{
			DestroyNotification(speechBubble, options.emoteDuration);
		}
		speechBubble.notificationGroup = options.speechBubbleGroup;
		return speechBubble;
	}

	public Notification CreateBouncingArrow(UserAttentionBlocker blocker, bool addToList)
	{
		bool num = SceneMgr.Get().IsInGame();
		bool canShow = UserAttentionManager.CanShowAttentionGrabber(blocker, "NotificationManger.CreateBouncingArrow");
		if (!num && !canShow)
		{
			Log.All.PrintDebug($"CreateBouncingArrow returning null because: SceneMgr.Get().IsInGame(): {SceneMgr.Get().IsInGame()} && UserAttentionManager.CanShowAttentionGrabber(blocker, \"NotificationManger.CreateBouncingArrow\"):{canShow} ");
			return null;
		}
		if (bounceArrowPrefab == null)
		{
			Log.All.PrintDebug("CreateBouncingArrow returning null because: bounceArrowPrefab is null");
			return null;
		}
		Notification arrow = UnityEngine.Object.Instantiate(bounceArrowPrefab).GetComponent<Notification>();
		arrow.PlayBirth();
		if (addToList)
		{
			arrows.Add(arrow);
		}
		return arrow;
	}

	public Notification CreateBouncingArrow(UserAttentionBlocker blocker, Vector3 position, Vector3 rotation)
	{
		return CreateBouncingArrow(blocker, position, rotation, addToList: true);
	}

	public Notification CreateBouncingArrow(UserAttentionBlocker blocker, Vector3 position, Vector3 rotation, bool addToList, float scaleFactor = 1f)
	{
		Notification notification = CreateBouncingArrow(blocker, addToList);
		notification.transform.position = position;
		notification.transform.localEulerAngles = rotation;
		notification.transform.localScale = Vector3.one * scaleFactor;
		return notification;
	}

	public Notification CreateFadeArrow(bool addToList)
	{
		Notification arrow = UnityEngine.Object.Instantiate(fadeArrowPrefab).GetComponent<Notification>();
		arrow.PlayBirth();
		if (addToList)
		{
			arrows.Add(arrow);
		}
		return arrow;
	}

	public Notification CreateFadeArrow(Vector3 position, Vector3 rotation)
	{
		return CreateFadeArrow(position, rotation, addToList: true);
	}

	public Notification CreateFadeArrow(Vector3 position, Vector3 rotation, bool addToList)
	{
		Notification notification = CreateFadeArrow(addToList);
		notification.transform.position = position;
		notification.transform.localEulerAngles = rotation;
		return notification;
	}

	public Notification CreatePopupText(UserAttentionBlocker blocker, Transform bone, string text, bool convertLegacyPosition = true, PopupTextType popupTextType = PopupTextType.BASIC)
	{
		if (convertLegacyPosition)
		{
			return CreatePopupText(blocker, bone.position, bone.localScale, text, convertLegacyPosition, popupTextType);
		}
		return CreatePopupText(blocker, bone.localPosition, bone.localScale, text, convertLegacyPosition, popupTextType);
	}

	public Notification CreatePopupText(UserAttentionBlocker blocker, Vector3 position, Vector3 scale, string text, bool convertLegacyPosition = true, PopupTextType popupTextType = PopupTextType.BASIC)
	{
		if (!SceneMgr.Get().IsInGame() && !UserAttentionManager.CanShowAttentionGrabber(blocker, "NotificationManager.CreatePopupText"))
		{
			return null;
		}
		Vector3 popupPosition = position;
		if (convertLegacyPosition)
		{
			Camera unityCamera = ((SceneMgr.Get().GetMode() != SceneMgr.Mode.GAMEPLAY) ? Box.Get().GetBoxCamera().GetComponent<Camera>() : BoardCameras.Get().GetComponentInChildren<Camera>());
			popupPosition = OverlayUI.Get().GetRelativePosition(position, unityCamera, OverlayUI.Get().m_heightScale.m_Center);
		}
		GameObject popupTextObject = UnityEngine.Object.Instantiate((popupTextType == PopupTextType.BASIC) ? popupTextPrefab : fancyPopupTextPrefab);
		LayerUtils.SetLayer(popupTextObject, GameLayer.UI);
		popupTextObject.transform.localPosition = popupPosition;
		popupTextObject.transform.localScale = scale;
		OverlayUI.Get().AddGameObject(popupTextObject);
		Notification popupText = popupTextObject.GetComponent<Notification>();
		popupText.ChangeText(text);
		popupText.PlayBirth();
		popupText.OnDestroyCallback = (Action<Notification>)Delegate.Combine(popupText.OnDestroyCallback, new Action<Notification>(OnPopupTextDestroy));
		popUpTexts.Add(popupText);
		return popupText;
	}

	public Notification CreateInnkeeperQuote(UserAttentionBlocker blocker, string text, string soundPath, float durationSeconds = 0f, Action<int> finishCallback = null, bool clickToDismiss = false)
	{
		return CreateInnkeeperQuote(blocker, DEFAULT_CHARACTER_POS, text, soundPath, durationSeconds, finishCallback, clickToDismiss);
	}

	public Notification CreateInnkeeperQuote(UserAttentionBlocker blocker, string text, string soundPath, Action<int> finishCallback, bool clickToDismiss = false)
	{
		return CreateInnkeeperQuote(blocker, DEFAULT_CHARACTER_POS, text, soundPath, 0f, finishCallback, clickToDismiss);
	}

	public Notification CreateInnkeeperQuote(UserAttentionBlocker blocker, Vector3 position, string text, string soundPath, float durationSeconds = 0f, Action<int> finishCallback = null, bool clickToDismiss = false, CanvasAnchor anchor = CanvasAnchor.BOTTOM_LEFT)
	{
		if (!SceneMgr.Get().IsInGame() && !UserAttentionManager.CanShowAttentionGrabber(blocker, "NotificationManager.CreateInnkeeperQuote"))
		{
			finishCallback?.Invoke(0);
			return null;
		}
		GameObject obj = UnityEngine.Object.Instantiate(innkeeperQuotePrefab);
		obj.GetComponentInChildren<BoxCollider>().enabled = clickToDismiss;
		Notification quote = obj.GetComponent<Notification>();
		quote.ignoreAudioOnDestroy = clickToDismiss;
		if (finishCallback != null)
		{
			quote.OnFinishDeathState = (Action<int>)Delegate.Combine(quote.OnFinishDeathState, finishCallback);
		}
		PlayCharacterQuote(quote, position, text, soundPath, durationSeconds, anchor);
		return quote;
	}

	public Notification CreateKTQuote(string stringTag, string soundPath, bool allowRepeatDuringSession = true)
	{
		return CreateKTQuote(DEFAULT_CHARACTER_POS, stringTag, soundPath, allowRepeatDuringSession);
	}

	public Notification CreateKTQuote(Vector3 position, string stringTag, string soundPath, bool allowRepeatDuringSession = true)
	{
		return CreateCharacterQuote("KT_Quote.prefab:7ad118a1a10e9ab409ade82268a378f5", position, GameStrings.Get(stringTag), soundPath, allowRepeatDuringSession);
	}

	public Notification CreateZombieNefarianQuote(Vector3 position, string stringTag, string soundPath, bool allowRepeatDuringSession)
	{
		return CreateCharacterQuote("NefarianDragon_Quote.prefab:179fec888df7e4c02b8de3b7ad109a23", position, GameStrings.Get(stringTag), soundPath, allowRepeatDuringSession);
	}

	public void PlayBundleInnkeeperLineForClass(TAG_CLASS cardClass)
	{
		bool clickToDismiss = UniversalInputManager.UsePhoneUI;
		string quote = string.Empty;
		string audioName = string.Empty;
		switch (cardClass)
		{
		case TAG_CLASS.DRUID:
			quote = GameStrings.Get("GLUE_INKEEPER_RANDOM_CARD_DECK_RECIPE_DRUID");
			audioName = "VO_INKEEPER_Male_Dwarf_ClassLegendaryDruid_01.prefab:2c4672cdfe2a96a45a7ac4f29c17d5b7";
			break;
		case TAG_CLASS.HUNTER:
			quote = GameStrings.Get("GLUE_INKEEPER_RANDOM_CARD_DECK_RECIPE_HUNTER");
			audioName = "VO_INKEEPER_Male_Dwarf_ClassLegendaryHunter_01.prefab:77302a32e0268f845a97992117241577";
			break;
		case TAG_CLASS.MAGE:
			quote = GameStrings.Get("GLUE_INKEEPER_RANDOM_CARD_DECK_RECIPE_MAGE");
			audioName = "VO_INKEEPER_Male_Dwarf_ClassLegendaryMage_01.prefab:2059ede4ae6efab489ecb4240a08d5bb";
			break;
		case TAG_CLASS.PALADIN:
			quote = GameStrings.Get("GLUE_INKEEPER_RANDOM_CARD_DECK_RECIPE_PALADIN");
			audioName = "VO_INKEEPER_Male_Dwarf_ClassLegendaryPaladin_01.prefab:21b7870188f66714b9707961d833b26a";
			break;
		case TAG_CLASS.PRIEST:
			quote = GameStrings.Get("GLUE_INKEEPER_RANDOM_CARD_DECK_RECIPE_PRIEST");
			audioName = "VO_INKEEPER_Male_Dwarf_ClassLegendaryPriest_01.prefab:fe9cd14401fd7f14f80950fb99864ce7";
			break;
		case TAG_CLASS.ROGUE:
			quote = GameStrings.Get("GLUE_INKEEPER_RANDOM_CARD_DECK_RECIPE_ROGUE");
			audioName = "VO_INKEEPER_Male_Dwarf_ClassLegendaryRogue_01.prefab:aa4c71ab99a240a4885e4a8d034adb1b";
			break;
		case TAG_CLASS.SHAMAN:
			quote = GameStrings.Get("GLUE_INKEEPER_RANDOM_CARD_DECK_RECIPE_SHAMAN");
			audioName = "VO_INKEEPER_Male_Dwarf_ClassLegendaryShaman_01.prefab:1101d9f890551164791f277babaa25d9";
			break;
		case TAG_CLASS.WARLOCK:
			quote = GameStrings.Get("GLUE_INKEEPER_RANDOM_CARD_DECK_RECIPE_WARLOCK");
			audioName = "VO_INKEEPER_Male_Dwarf_ClassLegendaryWarlock_01.prefab:5eaf5c883b0310e4d91bcfd3debc6eff";
			break;
		case TAG_CLASS.WARRIOR:
			quote = GameStrings.Get("GLUE_INKEEPER_RANDOM_CARD_DECK_RECIPE_WARRIOR");
			audioName = "VO_INKEEPER_Male_Dwarf_ClassLegendaryWarrior_01.prefab:41b4581beb2dae945843ed164a6ec710";
			break;
		}
		if (!string.IsNullOrEmpty(quote))
		{
			Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, quote, audioName, null, clickToDismiss);
		}
	}

	public Notification CreateCharacterQuote(string prefabPath, string text, string soundPath, bool allowRepeatDuringSession = true, float durationSeconds = 0f, CanvasAnchor anchorPoint = CanvasAnchor.BOTTOM_LEFT, bool blockAllOtherInput = false)
	{
		return CreateCharacterQuote(prefabPath, DEFAULT_CHARACTER_POS, text, soundPath, allowRepeatDuringSession, durationSeconds, null, anchorPoint, blockAllOtherInput);
	}

	public Notification CreateCharacterQuote(string prefabPath, Vector3 position, string text, string soundPath, bool allowRepeatDuringSession = true, float durationSeconds = 0f, Action<int> finishCallback = null, CanvasAnchor anchorPoint = CanvasAnchor.BOTTOM_LEFT, bool blockAllOtherInput = false)
	{
		if (!allowRepeatDuringSession && m_quotesThisSession.Contains(soundPath))
		{
			return null;
		}
		m_quotesThisSession.Add(soundPath);
		Notification quote = GameUtils.LoadGameObjectWithComponent<Notification>(prefabPath);
		if (quote == null)
		{
			return null;
		}
		quote.ShowWithExistingPopups = true;
		quote.PrefabPath = prefabPath;
		quote.SetClickBlockerActive(blockAllOtherInput);
		if (finishCallback != null)
		{
			quote.OnFinishDeathState = (Action<int>)Delegate.Combine(quote.OnFinishDeathState, finishCallback);
		}
		PlayCharacterQuote(quote, position, text, soundPath, durationSeconds, anchorPoint);
		return quote;
	}

	public Notification CreateBigCharacterQuoteWithGameString(string prefabPath, Vector3 position, string soundPath, string bubbleGameStringID, bool allowRepeatDuringSession = true, float durationSeconds = 0f, Action<int> finishCallback = null, bool useOverlayUI = false, Notification.SpeechBubbleDirection bubbleDir = Notification.SpeechBubbleDirection.None, bool persistCharacter = false, bool altPosition = false)
	{
		if (!allowRepeatDuringSession && m_quotesThisSession.Contains(bubbleGameStringID))
		{
			return null;
		}
		m_quotesThisSession.Add(bubbleGameStringID);
		return CreateBigCharacterQuoteWithText(prefabPath, position, soundPath, GameStrings.Get(bubbleGameStringID), durationSeconds, finishCallback, useOverlayUI, bubbleDir, persistCharacter, altPosition);
	}

	public Notification CreateBigCharacterQuoteWithText(string prefabPath, Vector3 position, string soundPath, string bubbleText, float durationSeconds = 0f, Action<int> finishCallback = null, bool useOverlayUI = false, Notification.SpeechBubbleDirection bubbleDir = Notification.SpeechBubbleDirection.None, bool persistCharacter = false, bool altPosition = false)
	{
		bool reusingQuote = false;
		Notification quote;
		if (prefabPath != null && m_quote != null && m_quote.PersistCharacter && prefabPath.Equals(m_quote.PrefabPath))
		{
			quote = m_quote;
			reusingQuote = true;
		}
		else
		{
			quote = GameUtils.LoadGameObjectWithComponent<Notification>(prefabPath);
		}
		if (quote == null)
		{
			return null;
		}
		quote.PrefabPath = prefabPath;
		quote.PersistCharacter = persistCharacter;
		quote.ShowWithExistingPopups = true;
		if (bubbleDir != 0)
		{
			quote.RepositionSpeechBubbleAroundBigQuote(bubbleDir, reusingQuote);
		}
		if (finishCallback != null)
		{
			Notification notification = quote;
			notification.OnFinishDeathState = (Action<int>)Delegate.Combine(notification.OnFinishDeathState, finishCallback);
		}
		PlayBigCharacterQuote(quote, bubbleText, soundPath, durationSeconds, position, useOverlayUI, persistCharacter, altPosition);
		return quote;
	}

	public void ForceAddSoundToPlayedList(string soundPath)
	{
		m_quotesThisSession.Add(soundPath);
	}

	public void ForceRemoveSoundFromPlayedList(string soundPath)
	{
		m_quotesThisSession.Remove(soundPath);
	}

	public bool HasSoundPlayedThisSession(string soundPath)
	{
		return m_quotesThisSession.Contains(soundPath);
	}

	public void ResetSoundsPlayedThisSession()
	{
		m_quotesThisSession.Clear();
	}

	private void PlayBigCharacterQuote(Notification quote, string text, string soundPath, float durationSeconds, Vector3 position, bool useOverlayUI = false, bool persistCharacter = false, bool altPosition = false)
	{
		bool playBirth = true;
		if ((bool)m_quote)
		{
			if (m_quote == quote)
			{
				playBirth = false;
			}
			else
			{
				UnityEngine.Object.Destroy(m_quote.gameObject);
			}
		}
		m_quote = quote;
		m_quote.ChangeText(text);
		if (useOverlayUI)
		{
			string boneName = (altPosition ? "OffScreenSpeaker2" : "OffScreenSpeaker1");
			TransformUtil.AttachAndPreserveLocalTransform(m_quote.transform, OverlayUI.Get().FindBone(boneName));
		}
		else
		{
			TransformUtil.AttachAndPreserveLocalTransform(m_quote.transform, Board.Get().FindBone("OffScreenSpeaker1"));
		}
		Vector3 quotePos = Vector3.zero;
		if (position != DEFAULT_CHARACTER_POS)
		{
			quotePos = position;
		}
		if (useOverlayUI && (bool)UniversalInputManager.UsePhoneUI)
		{
			quotePos.x += PHONE_OVERLAY_UI_CHARACTER_X_OFFSET;
		}
		m_quote.transform.localPosition = quotePos;
		m_quote.transform.localEulerAngles = Vector3.zero;
		if (!useOverlayUI && m_quote.rotate180InGameplay)
		{
			m_quote.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
		}
		if (playBirth)
		{
			m_quote.transform.localScale = Vector3.one * 0.01f;
		}
		if (!string.IsNullOrEmpty(soundPath) && AssetLoader.Get().IsAssetAvailable(soundPath))
		{
			QuoteSoundCallbackData soundCallbackData = new QuoteSoundCallbackData();
			soundCallbackData.m_quote = m_quote;
			soundCallbackData.m_durationSeconds = durationSeconds;
			soundCallbackData.m_persistCharacter = persistCharacter;
			SoundLoader.LoadSound(soundPath, OnBigQuoteSoundLoaded, soundCallbackData, SoundManager.Get().GetPlaceholderSound());
			return;
		}
		m_quote.PlayBirthWithForcedScale(Vector3.one);
		if (durationSeconds > 0f)
		{
			if (persistCharacter)
			{
				DestroySpeechBubble(m_quote, durationSeconds);
			}
			else
			{
				DestroyNotification(m_quote, durationSeconds);
			}
		}
	}

	private void PlayCharacterQuote(Notification quote, Vector3 position, string text, string soundPath, float durationSeconds, CanvasAnchor anchorPoint)
	{
		if ((bool)m_quote)
		{
			UnityEngine.Object.Destroy(m_quote.gameObject);
		}
		m_quote = quote;
		m_quote.ChangeText(text);
		m_quote.transform.position = position;
		m_quote.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
		OverlayUI.Get().AddGameObject(m_quote.gameObject, anchorPoint);
		if (!string.IsNullOrEmpty(soundPath) && AssetLoader.Get().IsAssetAvailable(soundPath))
		{
			QuoteSoundCallbackData soundCallbackData = new QuoteSoundCallbackData();
			soundCallbackData.m_quote = m_quote;
			soundCallbackData.m_durationSeconds = durationSeconds;
			SoundLoader.LoadSound(soundPath, OnQuoteSoundLoaded, soundCallbackData, SoundManager.Get().GetPlaceholderSound());
		}
		else
		{
			PlayQuoteWithoutSound(durationSeconds, text);
		}
	}

	private void PlayQuoteWithoutSound(float durationSeconds, string text = null)
	{
		m_quote.PlayBirthWithForcedScale(UniversalInputManager.UsePhoneUI ? NOTIFICATION_SCALE_PHONE : NOTIFICATION_SCALE);
		if (durationSeconds <= 0f && text != null)
		{
			durationSeconds = ClipLengthEstimator.StringToReadTime(text);
		}
		DestroyNotification(m_quote, durationSeconds);
	}

	private void OnQuoteSoundLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		QuoteSoundCallbackData quoteCallbackData = (QuoteSoundCallbackData)callbackData;
		if (!quoteCallbackData.m_quote)
		{
			UnityEngine.Object.Destroy(go);
			return;
		}
		AudioSource source = null;
		if ((bool)go)
		{
			source = go.GetComponent<AudioSource>();
			if ((bool)source && !source.clip)
			{
				source = null;
			}
		}
		if (!source)
		{
			Log.Asset.PrintInfo("Quote Sound failed to load!");
			PlayQuoteWithoutSound((quoteCallbackData.m_durationSeconds > 0f) ? quoteCallbackData.m_durationSeconds : 8f);
			return;
		}
		m_quote.AssignAudio(source);
		SoundManager.Get().PlayPreloaded(source);
		m_quote.PlayBirthWithForcedScale(UniversalInputManager.UsePhoneUI ? NOTIFICATION_SCALE_PHONE : NOTIFICATION_SCALE);
		float destroySeconds = Mathf.Max(quoteCallbackData.m_durationSeconds, source.clip.length);
		DestroyNotification(m_quote, destroySeconds);
		if (m_quote.clickOff != null)
		{
			m_quote.clickOff.SetData(m_quote);
			m_quote.clickOff.AddEventListener(UIEventType.PRESS, ClickNotification);
		}
	}

	private void OnBigQuoteSoundLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		QuoteSoundCallbackData quoteCallbackData = (QuoteSoundCallbackData)callbackData;
		if (!quoteCallbackData.m_quote)
		{
			UnityEngine.Object.Destroy(go);
			return;
		}
		AudioSource source = null;
		if ((bool)go)
		{
			source = go.GetComponent<AudioSource>();
			if ((bool)source && !source.clip)
			{
				source = null;
			}
		}
		if (!source)
		{
			Log.Asset.PrintInfo("Quote Sound failed to load!");
			PlayQuoteWithoutSound((quoteCallbackData.m_durationSeconds > 0f) ? quoteCallbackData.m_durationSeconds : 8f);
			return;
		}
		m_quote.AssignAudio(source);
		SoundManager.Get().PlayPreloaded(source);
		m_quote.PlayBirthWithForcedScale(Vector3.one);
		float destroySeconds = Mathf.Max(quoteCallbackData.m_durationSeconds, source.clip.length);
		Log.Notifications.Print("Destroying notification or speech bubble after {0} seconds. durationSeconds: {1} source.clip.length: {2} persistCharacter? {3}", destroySeconds, quoteCallbackData.m_durationSeconds, source.clip.length, quoteCallbackData.m_persistCharacter);
		if (quoteCallbackData.m_persistCharacter)
		{
			DestroySpeechBubble(m_quote, destroySeconds);
		}
		else
		{
			DestroyNotification(m_quote, destroySeconds);
		}
		if (m_quote.clickOff != null)
		{
			m_quote.clickOff.SetData(m_quote);
			m_quote.clickOff.AddEventListener(UIEventType.PRESS, ClickNotification);
		}
	}

	public void DestroyAllArrows()
	{
		if (arrows.Count == 0)
		{
			return;
		}
		for (int i = 0; i < arrows.Count; i++)
		{
			if (arrows[i] != null)
			{
				NukeNotificationWithoutPlayingAnim(arrows[i]);
			}
		}
	}

	public void DestroyAllPopUps()
	{
		if (popUpTexts.Count == 0)
		{
			return;
		}
		for (int i = 0; i < popUpTexts.Count; i++)
		{
			if (!(popUpTexts[i] == null))
			{
				NukeNotification(popUpTexts[i]);
			}
		}
		popUpTexts.Clear();
	}

	private void DestroyOtherNotifications(Notification.SpeechBubbleDirection direction, int speechBubbleGroup)
	{
		if (notificationsToDestroyUponNewNotifier.Count == 0 || !notificationsToDestroyUponNewNotifier.ContainsKey(speechBubbleGroup) || notificationsToDestroyUponNewNotifier[speechBubbleGroup] == null)
		{
			return;
		}
		for (int i = 0; i < notificationsToDestroyUponNewNotifier[speechBubbleGroup].Count; i++)
		{
			if (!(notificationsToDestroyUponNewNotifier[speechBubbleGroup][i] == null) && notificationsToDestroyUponNewNotifier[speechBubbleGroup][i].GetSpeechBubbleDirection() == direction)
			{
				NukeNotificationWithoutPlayingAnim(notificationsToDestroyUponNewNotifier[speechBubbleGroup][i]);
			}
		}
	}

	public void DestroyNotification(Notification notification, float delaySeconds)
	{
		if (!(notification == null))
		{
			if (delaySeconds == 0f)
			{
				NukeNotification(notification);
			}
			else
			{
				StartCoroutine(WaitAndThenDestroyNotification(notification, delaySeconds));
			}
		}
	}

	public void DestroySpeechBubble(Notification notification, float delaySeconds)
	{
		if (!(notification == null))
		{
			if (delaySeconds == 0f)
			{
				NukeSpeechBubble(notification);
			}
			else
			{
				StartCoroutine(WaitAndThenDestroySpeechBubble(notification, delaySeconds));
			}
		}
	}

	private void OnPopupTextDestroy(Notification notification)
	{
		popUpTexts.Remove(notification);
	}

	public void DestroyNotificationWithText(string text, float delaySeconds = 0f)
	{
		Notification notification = null;
		for (int i = 0; i < popUpTexts.Count; i++)
		{
			if (!(popUpTexts[i] == null) && popUpTexts[i].speechUberText.Text == text)
			{
				notification = popUpTexts[i];
			}
		}
		DestroyNotification(notification, delaySeconds);
	}

	private void ClickNotification(UIEvent e)
	{
		Notification quote = (Notification)e.GetElement().GetData();
		NukeNotification(quote);
		quote.clickOff.RemoveEventListener(UIEventType.PRESS, ClickNotification);
	}

	private void OnTutorialTooltipPrefabReady(Widget widget)
	{
		m_tutorialNotification = widget.GetComponentInChildren<Notification>();
		if (m_tutorialNotification != null)
		{
			m_tutorialNotificationWidget = m_tutorialNotification.GetComponent<Widget>();
			m_tutorialNotificationWidget.Hide();
		}
	}

	public void DestroyAllNotificationsNowWithNoAnim()
	{
		if ((bool)popUpDialog)
		{
			NukeNotificationWithoutPlayingAnim(popUpDialog);
		}
		if ((bool)m_quote)
		{
			NukeNotificationWithoutPlayingAnim(m_quote);
		}
		foreach (List<Notification> notificationsToDestroyGroup in notificationsToDestroyUponNewNotifier.Values)
		{
			for (int i = 0; i < notificationsToDestroyGroup.Count; i++)
			{
				Notification notification = notificationsToDestroyGroup[i];
				if (!(notification == null))
				{
					NukeNotificationWithoutPlayingAnim(notification);
				}
			}
		}
		DestroyAllArrows();
		DestroyAllPopUps();
	}

	public void DestroyQuote(Notification notification, float delaySeconds, bool ignoreAudio = false)
	{
		if (!(notification == null))
		{
			if (ignoreAudio)
			{
				notification.ignoreAudioOnDestroy = true;
			}
			if (delaySeconds == 0f)
			{
				NukeNotification(notification);
			}
			else
			{
				StartCoroutine(WaitAndThenDestroyNotification(notification, delaySeconds));
			}
		}
	}

	public void DestroyActiveQuote(float delaySeconds, bool ignoreAudio = false)
	{
		DestroyQuote(m_quote, delaySeconds, ignoreAudio);
	}

	public void DestroyNotificationNowWithNoAnim(Notification notification)
	{
		if (!(notification == null))
		{
			NukeNotificationWithoutPlayingAnim(notification);
		}
	}

	private IEnumerator WaitAndThenDestroyNotification(Notification notification, float amountSeconds)
	{
		yield return new WaitForSeconds(amountSeconds);
		if (notification != null)
		{
			NukeNotification(notification);
		}
	}

	private void NukeNotification(Notification notification)
	{
		if (notification == null)
		{
			Log.All.PrintWarning("Attempting to Nuke a Notification that does not exist!");
			return;
		}
		foreach (List<Notification> notificationsToDestroyGroup in notificationsToDestroyUponNewNotifier.Values)
		{
			if (notificationsToDestroyGroup.Contains(notification))
			{
				notificationsToDestroyGroup.Remove(notification);
			}
		}
		foreach (List<Notification> notificationsNotToDestroyGroup in speechBubbleNotToDestoryUponNewNotifier.Values)
		{
			if (notificationsNotToDestroyGroup.Contains(notification))
			{
				notificationsNotToDestroyGroup.Remove(notification);
			}
		}
		if (!notification.IsDying())
		{
			notification.PlayDeath();
			UniversalInputManager.Get()?.SetGameDialogActive(active: false);
		}
	}

	private void NukeNotificationWithoutPlayingAnim(Notification notification)
	{
		foreach (List<Notification> notificationsToDestroyGroup in notificationsToDestroyUponNewNotifier.Values)
		{
			if (notificationsToDestroyGroup.Contains(notification))
			{
				notificationsToDestroyGroup.Remove(notification);
			}
		}
		foreach (List<Notification> notificationsNotToDestroyGroup in speechBubbleNotToDestoryUponNewNotifier.Values)
		{
			if (notificationsNotToDestroyGroup.Contains(notification))
			{
				notificationsNotToDestroyGroup.Remove(notification);
			}
		}
		UnityEngine.Object.Destroy(notification.gameObject);
		UniversalInputManager.Get()?.SetGameDialogActive(active: false);
	}

	private IEnumerator WaitAndThenDestroySpeechBubble(Notification notification, float amountSeconds)
	{
		yield return new WaitForSeconds(amountSeconds);
		if (notification != null)
		{
			NukeSpeechBubble(notification);
		}
	}

	private void NukeSpeechBubble(Notification notification)
	{
		if (notification == null)
		{
			Log.All.PrintWarning("Attempting to Nuke a Speech Bubble for a Notification that does not exist!");
		}
		else if (!notification.IsDying())
		{
			notification.PlaySpeechBubbleDeath();
		}
	}

	public TutorialNotification CreateTutorialDialog(string headlineGameString, string bodyTextGameString, string buttonGameString, UIEvent.Handler buttonHandler, Vector2 materialOffset, bool swapMaterial = false)
	{
		GameObject actorObject = AssetLoader.Get().InstantiatePrefab("TutorialIntroDialog.prefab:2d189389d0be2f2428bf37ace33e85b1");
		if (actorObject == null)
		{
			Debug.LogError("Unable to load tutorial dialog TutorialIntroDialog prefab.");
			return null;
		}
		TutorialNotification notification = actorObject.GetComponent<TutorialNotification>();
		if (notification == null)
		{
			Debug.LogError("TutorialNotification component does not exist on TutorialIntroDialog prefab.");
			return null;
		}
		TransformUtil.AttachAndPreserveLocalTransform(actorObject.transform, OverlayUI.Get().m_heightScale.m_Center);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			actorObject.transform.localScale = 1.5f * actorObject.transform.localScale;
		}
		popUpDialog = notification;
		notification.headlineUberText.Text = GameStrings.Get(headlineGameString);
		notification.speechUberText.Text = GameStrings.Get(bodyTextGameString);
		notification.m_ButtonStart.SetText(GameStrings.Get(buttonGameString));
		if (swapMaterial)
		{
			notification.artOverlay.SetMaterial(notification.swapMaterial);
		}
		notification.artOverlay.GetMaterial().mainTextureOffset = materialOffset;
		notification.m_ButtonStart.AddEventListener(UIEventType.RELEASE, delegate(UIEvent e)
		{
			if (buttonHandler != null)
			{
				buttonHandler(e);
			}
			notification.m_ButtonStart.ClearEventListeners();
			DestroyNotification(notification, 0f);
		});
		popUpDialog.PlayBirth();
		UniversalInputManager.Get().SetGameDialogActive(active: true);
		return notification;
	}

	public bool ShowTutorialNotification(UserAttentionBlocker blocker, Vector3 position, Vector3 scale, string text, bool convertLegacyPosition = true, TutorialPopupType tooltipType = TutorialPopupType.BASIC, Notification.PopUpArrowDirection arrowDirection = Notification.PopUpArrowDirection.None)
	{
		if (!SceneMgr.Get().IsInGame() && !UserAttentionManager.CanShowAttentionGrabber(blocker, "NotificationManager.CreateTutorialNotificationText"))
		{
			return false;
		}
		if (m_tutorialNotificationWidget == null || m_tutorialNotification == null)
		{
			return false;
		}
		switch (arrowDirection)
		{
		case Notification.PopUpArrowDirection.None:
			m_tutorialNotificationWidget.TriggerEvent("DEFAULT_HIDE_ARROW");
			break;
		case Notification.PopUpArrowDirection.Right:
			m_tutorialNotificationWidget.TriggerEvent("ARROW_RIGHT");
			break;
		case Notification.PopUpArrowDirection.Left:
			m_tutorialNotificationWidget.TriggerEvent("ARROW_LEFT");
			break;
		case Notification.PopUpArrowDirection.Down:
			m_tutorialNotificationWidget.TriggerEvent("ARROW_BOTTOM");
			break;
		case Notification.PopUpArrowDirection.Up:
			m_tutorialNotificationWidget.TriggerEvent("ARROW_TOP");
			break;
		case Notification.PopUpArrowDirection.LeftDown:
			m_tutorialNotificationWidget.TriggerEvent("ARROW_BOTTOM_LEFT");
			break;
		case Notification.PopUpArrowDirection.RightDown:
			m_tutorialNotificationWidget.TriggerEvent("ARROW_BOTTOM_RIGHT");
			break;
		case Notification.PopUpArrowDirection.RightUp:
			m_tutorialNotificationWidget.TriggerEvent("ARROW_TOP_RIGHT");
			break;
		case Notification.PopUpArrowDirection.LeftUp:
			m_tutorialNotificationWidget.TriggerEvent("ARROW_TOP_LEFT");
			break;
		}
		m_tutorialNotificationWidget.Show();
		switch (tooltipType)
		{
		case TutorialPopupType.BASIC:
			m_tutorialNotificationWidget.TriggerEvent("DEFAULT_BACKER");
			break;
		case TutorialPopupType.IMPORTANT:
			m_tutorialNotificationWidget.TriggerEvent("IMPORTANT_BACKER");
			break;
		case TutorialPopupType.GRAPHIC:
			m_tutorialNotificationWidget.TriggerEvent("GRAPHIC_BACKER");
			break;
		}
		Vector3 popupPosition = position;
		if (convertLegacyPosition)
		{
			Camera unityCamera = ((SceneMgr.Get().GetMode() != SceneMgr.Mode.GAMEPLAY) ? Box.Get().GetBoxCamera().GetComponent<Camera>() : BoardCameras.Get().GetComponentInChildren<Camera>());
			popupPosition = OverlayUI.Get().GetRelativePosition(position, unityCamera, OverlayUI.Get().m_heightScale.m_Center);
		}
		m_tutorialNotificationWidget.transform.localPosition = popupPosition;
		m_tutorialNotificationWidget.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
		m_tutorialNotification.ChangeText(text);
		m_tutorialNotification.PlayBirthWithForcedScale(scale);
		return true;
	}

	public bool ShowExistingTutorialNotification(Vector3 scale)
	{
		if (m_tutorialNotificationWidget == null || m_tutorialNotification == null)
		{
			return false;
		}
		m_tutorialNotificationWidget.Show();
		m_tutorialNotification.PlayBirthWithForcedScale(scale);
		return true;
	}

	private IEnumerator HideTutorialNotificationWithDelay(float delay, float animationDuration)
	{
		if (m_tutorialNotification == null)
		{
			yield return null;
		}
		yield return new WaitForSeconds(delay);
		HideTutorialNotification(animationDuration);
	}

	public void HideTutorialNotification(float delay, float animationDuration)
	{
		StartCoroutine(HideTutorialNotificationWithDelay(delay, animationDuration));
	}

	public void HideTutorialNotification(float animationDuration)
	{
		if (m_tutorialNotification != null)
		{
			m_tutorialNotification.PlayDeathNoDestroy(animationDuration);
		}
	}
}
