using System;
using System.Collections.Generic;
using System.Threading;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.MaterialService.Extensions;
using Blizzard.T5.Services;
using Cysharp.Threading.Tasks;
using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.Serialization;

public class HighlightState : MonoBehaviour
{
	private readonly string HIGHLIGHT_SHADER_NAME = "Custom/Selection/Highlight";

	private const string FSM_BIRTH_STATE = "Birth";

	private const string FSM_IDLE_STATE = "Idle";

	private const string FSM_DEATH_STATE = "Death";

	private const string FSM_BIRTHTRANSITION_STATE = "BirthTransition";

	private const string FSM_IDLETRANSITION_STATE = "IdleTransition";

	private const string FSM_DEATHTRANSITION_STATE = "DeathTransition";

	public GameObject m_RenderPlane;

	public HighlightStateType m_highlightType;

	[NonSerialized]
	public Texture2D m_StaticSilouetteOverride;

	[Header("Silhouettes")]
	public Texture2D m_StaticSilouetteTexture;

	public Texture2D m_StaticSilouetteTextureUnique;

	[FormerlySerializedAs("m_TriClassBannerStaticSilouetteTexture")]
	[FormerlySerializedAs("m_MultiClassStaticSilouetteTexture")]
	public Texture2D m_BannerStaticSilouetteTexture;

	[FormerlySerializedAs("m_TriClassBannerStaticSilouetteTextureUnique")]
	[FormerlySerializedAs("m_MultiClassStaticSilouetteTextureUnique")]
	public Texture2D m_BannerStaticSilouetteTextureUnique;

	public Texture2D m_MulticlassStaticSilouetteTexture;

	public Texture2D m_MulticlassStaticSilouetteTextureUnique;

	public Texture2D m_MulticlassBannerStaticSilouetteTexture;

	public Texture2D m_MulticlassBannerStaticSilouetteTextureUnique;

	public Texture2D m_BattlegroundQuestSiloutteTexture;

	public Texture2D m_DeathKnightRuneBannerSilhouetteTexture;

	public Texture2D m_DeathKnightRuneBannerSilhouetteTextureUnique;

	public Texture2D m_MulticlassDeathKnightRuneBannerSilhouetteTexture;

	public Texture2D m_MulticlassDeathKnightRuneBannerSilhouetteTextureUnique;

	[Header("Settings")]
	public Vector3 m_HistoryTranslation = new Vector3(0f, -0.1f, 0f);

	public int m_RenderQueue;

	public int m_RenderQueueOffset = 3000;

	public List<HighlightRenderState> m_HighlightStates;

	public ActorStateType m_debugState;

	protected ActorStateType m_PreviousState;

	protected ActorStateType m_CurrentState;

	protected PlayMakerFSM m_FSM;

	private string m_sendEvent;

	private bool m_isDirty;

	private bool m_forceRerender;

	private string m_BirthTransition = "None";

	private string m_SecondBirthTransition = "None";

	private string m_IdleTransition = "None";

	private string m_DeathTransition = "None";

	private bool m_Hide;

	private bool m_VisibilityState;

	private float m_seed;

	private Material m_Material;

	private Renderer m_renderer;

	private HighlightRender m_highlightRender;

	private bool m_RenderersInitialized;

	private CancellationTokenSource m_tokenSource;

	private IGraphicsManager m_graphicsManager;

	public ActorStateType CurrentState => m_CurrentState;

	private void Awake()
	{
		m_graphicsManager = ServiceManager.Get<IGraphicsManager>();
		TryInitRenderers();
		if (m_FSM != null)
		{
			m_FSM.enabled = true;
		}
		if (m_highlightType == HighlightStateType.NONE)
		{
			Transform parent = base.transform.parent;
			if (parent != null)
			{
				if ((bool)parent.GetComponent<ActorStateMgr>())
				{
					m_highlightType = HighlightStateType.CARD;
				}
				else
				{
					m_highlightType = HighlightStateType.HIGHLIGHT;
				}
			}
		}
		if (m_highlightType == HighlightStateType.NONE)
		{
			Debug.LogError("m_highlightType is not set!");
			base.enabled = false;
		}
		Setup();
		if (m_tokenSource == null)
		{
			m_tokenSource = new CancellationTokenSource();
		}
	}

	private void Update()
	{
		if (m_debugState != 0)
		{
			ChangeState(m_debugState);
			ForceUpdate();
		}
		if (m_Hide)
		{
			if (!(m_RenderPlane == null))
			{
				m_renderer.enabled = false;
			}
		}
		else if (m_isDirty && !(m_RenderPlane == null) && m_renderer.enabled)
		{
			UpdateSilouette();
			m_isDirty = false;
		}
	}

	private void OnApplicationFocus(bool state)
	{
		m_isDirty = true;
		m_forceRerender = true;
	}

	protected void OnDestroy()
	{
		if ((bool)m_Material)
		{
			UnityEngine.Object.Destroy(m_Material);
		}
		m_tokenSource?.Cancel();
		m_tokenSource?.Dispose();
	}

	private void Setup()
	{
		m_seed = UnityEngine.Random.value;
		m_CurrentState = ActorStateType.CARD_IDLE;
		Renderer component = m_RenderPlane.GetComponent<Renderer>();
		component.enabled = false;
		m_VisibilityState = false;
		if (m_Material == null)
		{
			Shader highlightShader = ShaderUtils.FindShader(HIGHLIGHT_SHADER_NAME);
			if (!highlightShader)
			{
				Debug.LogError("Failed to load Highlight Shader: " + HIGHLIGHT_SHADER_NAME);
				base.enabled = false;
			}
			m_Material = new Material(highlightShader);
		}
		component.SetSharedMaterial(m_Material);
	}

	public void Show()
	{
		m_Hide = false;
		if (m_renderer != null && m_VisibilityState && !m_renderer.enabled)
		{
			m_renderer.enabled = true;
		}
	}

	public void Hide()
	{
		m_Hide = true;
		if (!(m_renderer == null))
		{
			m_renderer.enabled = false;
		}
	}

	public void SetDirty()
	{
		m_isDirty = true;
	}

	public void ForceUpdate()
	{
		m_isDirty = true;
		m_forceRerender = true;
	}

	public void ContinuousUpdate(float updateTime)
	{
		ContinuousSilouetteRender(updateTime, m_tokenSource.Token).Forget();
	}

	public bool IsReady()
	{
		return m_Material != null;
	}

	public bool ChangeState(ActorStateType stateType)
	{
		if (stateType == m_CurrentState)
		{
			return true;
		}
		m_PreviousState = m_CurrentState;
		m_CurrentState = stateType;
		TryInitRenderers();
		if (m_renderer == null)
		{
			m_VisibilityState = false;
			return true;
		}
		switch (stateType)
		{
		case ActorStateType.NONE:
			m_renderer.enabled = false;
			m_VisibilityState = false;
			return true;
		case ActorStateType.CARD_IDLE:
		case ActorStateType.HIGHLIGHT_OFF:
			if (m_FSM == null)
			{
				m_renderer.enabled = false;
				m_VisibilityState = false;
				return true;
			}
			m_DeathTransition = m_PreviousState.ToString();
			SendDataToPlaymaker();
			SendPlaymakerDeathEvent();
			return true;
		default:
			foreach (HighlightRenderState hlrs in m_HighlightStates)
			{
				if (hlrs.m_StateType != stateType)
				{
					continue;
				}
				if (hlrs.m_Material != null && m_Material != null)
				{
					m_Material.CopyPropertiesFromMaterial(hlrs.m_Material);
					m_renderer.SetSharedMaterial(m_Material);
					m_renderer.GetSharedMaterial().SetFloat("_Seed", m_seed);
					bool result = RenderSilouette();
					if (stateType == ActorStateType.CARD_HISTORY)
					{
						base.transform.localPosition = m_HistoryTranslation;
					}
					else
					{
						base.transform.localPosition = hlrs.m_Offset;
					}
					if (m_FSM == null)
					{
						if (!m_Hide)
						{
							m_renderer.enabled = true;
						}
						m_VisibilityState = true;
					}
					else
					{
						m_BirthTransition = stateType.ToString();
						m_SecondBirthTransition = m_PreviousState.ToString();
						m_IdleTransition = m_BirthTransition;
						SendDataToPlaymaker();
						SendPlaymakerBirthEvent();
					}
					return result;
				}
				m_renderer.enabled = false;
				m_VisibilityState = false;
				return true;
			}
			if (m_highlightType == HighlightStateType.CARD || m_highlightType == HighlightStateType.CARD_PLAY)
			{
				m_CurrentState = ActorStateType.CARD_IDLE;
			}
			else if (m_highlightType == HighlightStateType.HIGHLIGHT)
			{
				m_CurrentState = ActorStateType.HIGHLIGHT_OFF;
			}
			m_DeathTransition = m_PreviousState.ToString();
			SendDataToPlaymaker();
			SendPlaymakerDeathEvent();
			m_renderer.enabled = false;
			m_VisibilityState = false;
			return false;
		}
	}

	protected void UpdateSilouette()
	{
		RenderSilouette();
	}

	private Texture2D GetBestSilouette(Texture2D defaultTexture, bool isElite, bool isBannerActive, bool isRuneBannerActive, bool isMulticlass, bool useQuestSilouette)
	{
		Texture2D silouetteTex = defaultTexture;
		if (isElite && m_StaticSilouetteTextureUnique != null)
		{
			silouetteTex = m_StaticSilouetteTextureUnique;
		}
		if (isBannerActive && m_BannerStaticSilouetteTexture != null)
		{
			silouetteTex = m_BannerStaticSilouetteTexture;
		}
		if (isElite && isBannerActive && m_BannerStaticSilouetteTextureUnique != null)
		{
			silouetteTex = m_BannerStaticSilouetteTextureUnique;
		}
		if (isRuneBannerActive && m_DeathKnightRuneBannerSilhouetteTexture != null)
		{
			silouetteTex = m_DeathKnightRuneBannerSilhouetteTexture;
		}
		if (isRuneBannerActive && isElite && m_DeathKnightRuneBannerSilhouetteTextureUnique != null)
		{
			silouetteTex = m_DeathKnightRuneBannerSilhouetteTextureUnique;
		}
		if (isMulticlass)
		{
			if (isElite && m_StaticSilouetteTextureUnique != null)
			{
				silouetteTex = m_MulticlassStaticSilouetteTextureUnique;
			}
			if (isBannerActive && m_BannerStaticSilouetteTexture != null)
			{
				silouetteTex = m_MulticlassBannerStaticSilouetteTexture;
			}
			if (isElite && isBannerActive && m_BannerStaticSilouetteTextureUnique != null)
			{
				silouetteTex = m_MulticlassBannerStaticSilouetteTextureUnique;
			}
			if (isRuneBannerActive && m_DeathKnightRuneBannerSilhouetteTexture != null)
			{
				silouetteTex = m_MulticlassDeathKnightRuneBannerSilhouetteTexture;
			}
			if (isRuneBannerActive && isElite && m_DeathKnightRuneBannerSilhouetteTextureUnique != null)
			{
				silouetteTex = m_MulticlassDeathKnightRuneBannerSilhouetteTextureUnique;
			}
		}
		if (useQuestSilouette && m_BattlegroundQuestSiloutteTexture != null)
		{
			silouetteTex = m_BattlegroundQuestSiloutteTexture;
		}
		if (m_StaticSilouetteOverride != null)
		{
			silouetteTex = m_StaticSilouetteOverride;
		}
		return silouetteTex;
	}

	private bool RenderSilouette()
	{
		m_isDirty = false;
		Texture2D staticSilouetteTexture = m_StaticSilouetteOverride ?? m_StaticSilouetteTexture;
		if (staticSilouetteTexture != null)
		{
			Texture2D silouetteTex = staticSilouetteTexture;
			Actor actor = GameObjectUtils.FindComponentInParents<Actor>(base.gameObject);
			if (actor != null)
			{
				CardSilhouetteOverride cardSilhouetteOverride = actor.CardSilhouetteOverride;
				bool isElite = actor.IsElite();
				bool hasHearthstoneFaction = actor.HasHearthstoneFaction();
				bool hasDeckAction = actor.HasDeckAction();
				bool isRuneBannerActive = actor.HasRuneCost();
				bool useQuestSilouette = actor.UseBGQuestSiloutte();
				bool isBannerActive = hasHearthstoneFaction || hasDeckAction;
				bool showMulticlassRibbon = actor.GetClasses().Count >= 3;
				switch (cardSilhouetteOverride)
				{
				case CardSilhouetteOverride.NoBanner:
					isBannerActive = false;
					break;
				case CardSilhouetteOverride.Banner:
					isBannerActive = true;
					break;
				}
				silouetteTex = GetBestSilouette(silouetteTex, isElite, isBannerActive, isRuneBannerActive, showMulticlassRibbon, useQuestSilouette);
			}
			Material sharedMaterial = m_renderer.GetSharedMaterial();
			sharedMaterial.mainTexture = silouetteTex;
			sharedMaterial.renderQueue = m_RenderQueueOffset + m_RenderQueue;
			m_forceRerender = false;
			return true;
		}
		if (m_highlightRender == null)
		{
			Debug.LogError("Unable to find HighlightRender component on m_RenderPlane");
			return false;
		}
		if (m_highlightRender.enabled)
		{
			m_highlightRender.CreateSilhouetteTexture(m_forceRerender);
			Material sharedMaterial2 = m_renderer.GetSharedMaterial();
			sharedMaterial2.mainTexture = m_highlightRender.SilhouetteTexture;
			sharedMaterial2.renderQueue = m_RenderQueueOffset + m_RenderQueue;
		}
		m_forceRerender = false;
		return true;
	}

	private async UniTaskVoid ContinuousSilouetteRender(float renderTime, CancellationToken token)
	{
		if (m_RenderPlane == null || m_graphicsManager == null || m_renderer == null)
		{
			return;
		}
		if (m_graphicsManager.RenderQualityLevel == GraphicsQuality.Low)
		{
			await UniTask.Delay(TimeSpan.FromSeconds(renderTime), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
			if (m_renderer.enabled)
			{
				m_isDirty = true;
				m_forceRerender = true;
				RenderSilouette();
			}
			return;
		}
		float endTime = Time.realtimeSinceStartup + renderTime;
		while (Time.realtimeSinceStartup < endTime)
		{
			if (m_renderer.enabled)
			{
				m_isDirty = true;
				m_forceRerender = true;
				RenderSilouette();
			}
			await UniTask.Yield(PlayerLoopTiming.Update, token);
		}
	}

	private void SendDataToPlaymaker()
	{
		if (!(m_FSM == null))
		{
			FsmMaterial fsmMat = m_FSM.FsmVariables.GetFsmMaterial("HighlightMaterial");
			if (fsmMat != null)
			{
				fsmMat.Value = m_renderer.GetSharedMaterial();
			}
			FsmString fsmCurrentState = m_FSM.FsmVariables.GetFsmString("CurrentState");
			if (fsmCurrentState != null)
			{
				fsmCurrentState.Value = m_CurrentState.ToString();
			}
			FsmString fsmPreviousState = m_FSM.FsmVariables.GetFsmString("PreviousState");
			if (fsmPreviousState != null)
			{
				fsmPreviousState.Value = m_PreviousState.ToString();
			}
		}
	}

	private void SendPlaymakerDeathEvent()
	{
		if (!(m_FSM == null))
		{
			FsmString fsmDeathTransition = m_FSM.FsmVariables.GetFsmString("DeathTransition");
			if (fsmDeathTransition != null)
			{
				fsmDeathTransition.Value = m_DeathTransition;
			}
			m_FSM.SendEvent("Death");
		}
	}

	private void SendPlaymakerBirthEvent()
	{
		if (!(m_FSM == null))
		{
			FsmString fsmBirthTransition = m_FSM.FsmVariables.GetFsmString("BirthTransition");
			if (fsmBirthTransition != null)
			{
				fsmBirthTransition.Value = m_BirthTransition;
			}
			FsmString fsmSecondBirthTransition = m_FSM.FsmVariables.GetFsmString("SecondBirthTransition");
			if (fsmSecondBirthTransition != null)
			{
				fsmSecondBirthTransition.Value = m_SecondBirthTransition;
			}
			FsmString fsmIdleTransition = m_FSM.FsmVariables.GetFsmString("IdleTransition");
			if (fsmIdleTransition != null)
			{
				fsmIdleTransition.Value = m_IdleTransition;
			}
			m_FSM.SendEvent("Birth");
		}
	}

	public void OnActionFinished()
	{
	}

	private void TryInitRenderers()
	{
		if (m_RenderersInitialized)
		{
			return;
		}
		m_RenderersInitialized = true;
		if (m_RenderPlane == null)
		{
			if (!Application.isEditor)
			{
				Debug.LogError("m_RenderPlane is null!");
			}
			base.enabled = false;
		}
		else
		{
			m_renderer = m_RenderPlane.GetComponent<Renderer>();
			m_highlightRender = m_RenderPlane.GetComponent<HighlightRender>();
			m_renderer.enabled = false;
			m_VisibilityState = false;
			m_FSM = m_RenderPlane.GetComponent<PlayMakerFSM>();
		}
	}
}
