using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LegendaryHeroAnimController : MonoBehaviour
{
	[Serializable]
	private struct AnimatorEntry
	{
		public Animator m_animator;

		public float m_transitionMultiplier;

		public AnimatorEntry(Animator animator, float transitionMultiplier)
		{
			m_animator = animator;
			m_transitionMultiplier = transitionMultiplier;
		}
	}

	[Flags]
	public enum SupportedFeatures
	{
		Attack = 1,
		Damage = 2,
		Emotes = 4,
		IntoPlay = 8,
		Spell = 0x10,
		Summon = 0x20,
		EquipWeapon = 0x40,
		HeroPower = 0x80,
		HeroPowerBirth = 0x100,
		IntoPlayExit = 0x200,
		Strikes = 0x400
	}

	public enum InternalState
	{
		Unknown,
		Idle,
		IdleFidget,
		Emote_Greetings,
		Emote_Oops,
		Emote_Thanks,
		Emote_Threaten,
		Emote_WellPlayed,
		Emote_Wow,
		EquipWeapon,
		Spell,
		SummonMinion,
		Victory,
		Defeat,
		AttackStart,
		AttackComplete,
		DamageStart,
		DamageComplete,
		IntroStart,
		IntroComplete,
		HeroPowerStart,
		HeroPowerComplete,
		HeroPowerCompleteAlt,
		SocketIn,
		VictoryStrike,
		DefeatStrike
	}

	public delegate void RequestedAnimTransitionCallback(InternalState state);

	public class AnimationEventHandler : MonoBehaviour
	{
		private LegendaryHeroAnimController m_controller;

		public void SetController(LegendaryHeroAnimController controller)
		{
			m_controller = controller;
		}

		public void RaiseAnimEvent(int internalEvent)
		{
			m_controller.RaiseAnimEvent((InternalState)internalEvent);
		}
	}

	[Header("Supported Features")]
	public SupportedFeatures Features = (SupportedFeatures)(-1);

	public bool voStartsComeIntoPlay;

	public bool HasPreAttackAnimation;

	public bool HasIdleFidgetAnimation;

	[Min(0f)]
	[Header("Timers")]
	public float DamageTimer = 2.3f;

	[Min(0f)]
	public float AttackDelay = 1f;

	[Min(0f)]
	public float HeroPowerDelay = 2.5f;

	[Min(0f)]
	public float MinIdleFidgetTime = 5f;

	[Min(0f)]
	public float MaxIdleFidgetTime = 10f;

	public bool AllowSummonInterruption = true;

	[SerializeField]
	[HideInInspector]
	private float[] m_transitionTimes = Enumerable.Repeat(0.25f, Enum.GetValues(typeof(InternalState)).Length).ToArray();

	private Animator m_masterAnimator;

	private List<AnimatorEntry> m_animators = new List<AnimatorEntry>();

	private Card m_card;

	[NonSerialized]
	public float DamageCountdown;

	[NonSerialized]
	public float AttackCountdown;

	[NonSerialized]
	public float HeroPowerCountdown;

	private float IdleFidgetCountdown;

	private Dictionary<LegendaryHeroAnimations, float> m_ignoreAnimRequests = new Dictionary<LegendaryHeroAnimations, float>();

	private InternalState m_requestedState;

	private InternalState m_lastRequestedState = InternalState.Idle;

	private static readonly Dictionary<InternalState, int> s_animatorStates = new Dictionary<InternalState, int>
	{
		{
			InternalState.Idle,
			Animator.StringToHash("Base Layer.Idle")
		},
		{
			InternalState.IdleFidget,
			Animator.StringToHash("Base Layer.Idle Fidget")
		},
		{
			InternalState.AttackComplete,
			Animator.StringToHash("Base Layer.Attack")
		},
		{
			InternalState.IntroComplete,
			Animator.StringToHash("Base Layer.Intro Sequence")
		},
		{
			InternalState.DamageComplete,
			Animator.StringToHash("Base Layer.Damage")
		},
		{
			InternalState.HeroPowerComplete,
			Animator.StringToHash("Base Layer.Hero Power")
		},
		{
			InternalState.HeroPowerCompleteAlt,
			Animator.StringToHash("Base Layer.Hero Power Alt")
		},
		{
			InternalState.Spell,
			Animator.StringToHash("Base Layer.Spell")
		},
		{
			InternalState.HeroPowerStart,
			Animator.StringToHash("Base Layer.Hero Power Intro")
		},
		{
			InternalState.SummonMinion,
			Animator.StringToHash("Base Layer.Summon")
		},
		{
			InternalState.EquipWeapon,
			Animator.StringToHash("Base Layer.Equip Weapon")
		},
		{
			InternalState.Emote_Thanks,
			Animator.StringToHash("Base Layer.Emote Thanks")
		},
		{
			InternalState.Emote_WellPlayed,
			Animator.StringToHash("Base Layer.Emote Well Played")
		},
		{
			InternalState.Emote_Greetings,
			Animator.StringToHash("Base Layer.Emote Greetings")
		},
		{
			InternalState.Emote_Wow,
			Animator.StringToHash("Base Layer.Emote Wow")
		},
		{
			InternalState.Emote_Oops,
			Animator.StringToHash("Base Layer.Emote Oops")
		},
		{
			InternalState.Emote_Threaten,
			Animator.StringToHash("Base Layer.Emote Threaten")
		},
		{
			InternalState.AttackStart,
			Animator.StringToHash("Base Layer.Attack Intro")
		},
		{
			InternalState.IntroStart,
			Animator.StringToHash("Base Layer.Intro Sequence Intro")
		},
		{
			InternalState.DamageStart,
			Animator.StringToHash("Base Layer.Damage Intro")
		},
		{
			InternalState.Victory,
			Animator.StringToHash("Base Layer.Victory Intro")
		},
		{
			InternalState.Defeat,
			Animator.StringToHash("Base Layer.Defeat Intro")
		},
		{
			InternalState.SocketIn,
			Animator.StringToHash("Base Layer.Socket In")
		},
		{
			InternalState.VictoryStrike,
			Animator.StringToHash("Base Layer.Victory Strike")
		},
		{
			InternalState.DefeatStrike,
			Animator.StringToHash("Base Layer.Defeat Strike")
		}
	};

	public event RequestedAnimTransitionCallback OnRequestAnimTransition;

	private void Awake()
	{
		m_masterAnimator = base.gameObject.GetComponentInChildren<Animator>();
		AnimatorEntry masterAnimator = new AnimatorEntry(m_masterAnimator, 1f);
		m_animators.Add(masterAnimator);
		m_requestedState = InternalState.Unknown;
		if (HasIdleFidgetAnimation)
		{
			SetupIdleFidget();
		}
	}

	private void SetupIdleFidget()
	{
		m_masterAnimator.gameObject.AddComponent<AnimationEventHandler>().SetController(this);
		AnimatorOverrideController obj = m_masterAnimator.runtimeAnimatorController as AnimatorOverrideController;
		List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
		obj.GetOverrides(overrides);
		KeyValuePair<AnimationClip, AnimationClip> anim = overrides.Find((KeyValuePair<AnimationClip, AnimationClip> x) => x.Key.name == "Idle");
		if (anim.Value == null)
		{
			HasIdleFidgetAnimation = false;
		}
		AnimationEvent animEvent = new AnimationEvent();
		animEvent.functionName = "RaiseAnimEvent";
		animEvent.intParameter = 2;
		animEvent.time = 0f;
		anim.Value.AddEvent(animEvent);
		SetIdleFidgetCountdown();
	}

	public void OnAttachedToActor(Actor actor)
	{
		m_card = actor.GetComponentInParent<Card>();
		if (base.enabled)
		{
			OnEnable();
		}
	}

	private void OnEnable()
	{
		if (m_card != null)
		{
			m_card.OnEmotePlayCallback += EmotePlayCallback;
		}
		DamageCountdown = 0f;
	}

	private void OnDisable()
	{
		if (m_card != null)
		{
			m_card.OnEmotePlayCallback -= EmotePlayCallback;
		}
	}

	private void Update()
	{
		UpdateCountdownTimers();
		CheckAndTriggerTransitionListenersForIdle();
		if (!StateSupported(m_requestedState))
		{
			m_requestedState = InternalState.Unknown;
		}
		bool resetStateTime = UpdateCountdownResets();
		UpdateIdleFidget();
		CheckForSummonInterruption();
		UpdateIgnoreAnimRequests();
		CrossFadeToRequestedState(resetStateTime);
	}

	private void UpdateCountdownTimers()
	{
		UpdateTimer(ref AttackCountdown);
		UpdateTimer(ref HeroPowerCountdown);
		if (UpdateTimer(ref DamageCountdown))
		{
			m_requestedState = InternalState.DamageComplete;
		}
	}

	private bool UpdateCountdownResets()
	{
		if (m_requestedState == InternalState.AttackComplete)
		{
			AttackCountdown = AttackDelay;
		}
		else if (m_requestedState == InternalState.HeroPowerComplete)
		{
			HeroPowerCountdown = HeroPowerDelay;
		}
		return UpdateDamageCountdownReset();
	}

	private void UpdateIdleFidget()
	{
		if (!HasIdleFidgetAnimation)
		{
			return;
		}
		if (m_requestedState == InternalState.IdleFidget)
		{
			if (IdleFidgetCountdown > 0f)
			{
				m_requestedState = InternalState.Unknown;
			}
			else
			{
				SetIdleFidgetCountdown();
			}
		}
		else if (m_requestedState == InternalState.Idle)
		{
			SetIdleFidgetCountdown();
		}
		else if (m_requestedState == InternalState.Unknown && m_masterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Idle"))
		{
			UpdateTimer(ref IdleFidgetCountdown);
		}
	}

	private bool UpdateDamageCountdownReset()
	{
		if (m_requestedState != InternalState.DamageStart)
		{
			return false;
		}
		if (DamageTimer == 0f)
		{
			m_requestedState = InternalState.DamageComplete;
			return false;
		}
		DamageCountdown = DamageTimer;
		return true;
	}

	private void CheckForSummonInterruption()
	{
		if (!AllowSummonInterruption && m_requestedState == InternalState.SummonMinion)
		{
			string summonStateName = "Base Layer.Summon";
			if (m_masterAnimator.GetCurrentAnimatorStateInfo(0).IsName(summonStateName) || m_masterAnimator.GetNextAnimatorStateInfo(0).IsName(summonStateName))
			{
				m_requestedState = InternalState.Unknown;
			}
		}
	}

	private void CrossFadeToRequestedState(bool resetStateTime)
	{
		if (m_requestedState == InternalState.Unknown)
		{
			return;
		}
		if (s_animatorStates.TryGetValue(m_requestedState, out var stateNameHash))
		{
			foreach (AnimatorEntry animatorEntry in m_animators)
			{
				float transitionTime = m_transitionTimes[(int)m_requestedState] * animatorEntry.m_transitionMultiplier;
				if (resetStateTime)
				{
					animatorEntry.m_animator.CrossFadeInFixedTime(stateNameHash, transitionTime, -1, 0f);
				}
				else
				{
					animatorEntry.m_animator.CrossFadeInFixedTime(stateNameHash, transitionTime * animatorEntry.m_transitionMultiplier);
				}
			}
			this.OnRequestAnimTransition?.Invoke(m_requestedState);
			m_lastRequestedState = m_requestedState;
		}
		m_requestedState = InternalState.Unknown;
	}

	private void SetIdleFidgetCountdown()
	{
		IdleFidgetCountdown = UnityEngine.Random.Range(MinIdleFidgetTime, MaxIdleFidgetTime);
	}

	private bool UpdateTimer(ref float timer)
	{
		if (timer > 0f)
		{
			timer -= Time.deltaTime;
			if (timer <= 0f)
			{
				return true;
			}
		}
		return false;
	}

	public void EmotePlayCallback(EmoteType emoteType)
	{
		if (TryGetStateFromEvent(emoteType, out var newState))
		{
			m_requestedState = newState;
		}
	}

	public void RaiseAnimEvent(LegendaryHeroAnimations animation)
	{
		InternalState newState;
		if (m_ignoreAnimRequests.ContainsKey(animation))
		{
			Debug.Log("Ignoring animation " + animation.ToString() + " as ignore request was specified");
		}
		else if (TryGetStateFromEvent(animation, out newState))
		{
			m_requestedState = newState;
		}
	}

	public void RaiseAnimEvent(InternalState newState)
	{
		if (newState != InternalState.IdleFidget || m_requestedState == InternalState.Unknown)
		{
			m_requestedState = newState;
		}
	}

	public void AddSlaveAnimator(Animator animator, float transitionMultiplier)
	{
		if (!(m_animators.Find((AnimatorEntry x) => x.m_animator == animator).m_animator == animator))
		{
			m_animators.Add(new AnimatorEntry(animator, transitionMultiplier));
		}
	}

	public void RemoveSlaveAnimator(Animator animator)
	{
		m_animators.RemoveAll((AnimatorEntry x) => x.m_animator == animator);
	}

	internal bool TryGetStateFromEvent(LegendaryHeroAnimations animation, out InternalState state)
	{
		switch (animation)
		{
		case LegendaryHeroAnimations.Emote_Thanks:
			state = InternalState.Emote_Thanks;
			return true;
		case LegendaryHeroAnimations.Emote_WellPlayed:
			state = InternalState.Emote_WellPlayed;
			return true;
		case LegendaryHeroAnimations.Emote_Greetings:
			state = InternalState.Emote_Greetings;
			return true;
		case LegendaryHeroAnimations.Emote_Wow:
			state = InternalState.Emote_Wow;
			return true;
		case LegendaryHeroAnimations.Emote_Oops:
			state = InternalState.Emote_Oops;
			return true;
		case LegendaryHeroAnimations.Emote_Threaten:
			state = InternalState.Emote_Threaten;
			return true;
		case LegendaryHeroAnimations.FriendlyAnnounceVO:
			state = InternalState.IntroStart;
			return true;
		case LegendaryHeroAnimations.OpponentAnnounceVO:
			state = InternalState.IntroStart;
			return true;
		case LegendaryHeroAnimations.Defeat:
			state = InternalState.Defeat;
			return true;
		case LegendaryHeroAnimations.Victory:
			state = InternalState.Victory;
			return true;
		case LegendaryHeroAnimations.SummonMinion:
			state = InternalState.SummonMinion;
			return true;
		case LegendaryHeroAnimations.Damage:
			state = InternalState.DamageStart;
			return true;
		case LegendaryHeroAnimations.SpellCard:
			state = InternalState.Spell;
			return true;
		case LegendaryHeroAnimations.HeroPower:
			state = InternalState.HeroPowerComplete;
			return true;
		case LegendaryHeroAnimations.IntroSequenceBegin:
			state = InternalState.IntroStart;
			return true;
		case LegendaryHeroAnimations.IntroSequenceEnd:
			state = InternalState.IntroComplete;
			return true;
		case LegendaryHeroAnimations.Attack:
			state = InternalState.AttackComplete;
			return true;
		case LegendaryHeroAnimations.AttackBirth:
			state = InternalState.AttackStart;
			return true;
		case LegendaryHeroAnimations.AttackCancel:
			state = InternalState.Idle;
			return true;
		case LegendaryHeroAnimations.HeroPowerBirth:
			state = InternalState.HeroPowerStart;
			return true;
		case LegendaryHeroAnimations.HeroPowerCancel:
			state = InternalState.Idle;
			return true;
		case LegendaryHeroAnimations.HeroPowerAlt:
			state = InternalState.HeroPowerCompleteAlt;
			return true;
		case LegendaryHeroAnimations.SocketInTriggered:
			state = InternalState.SocketIn;
			return true;
		case LegendaryHeroAnimations.WeaponCardPlayed:
			state = InternalState.EquipWeapon;
			return true;
		case LegendaryHeroAnimations.VictoryStrike:
			state = InternalState.VictoryStrike;
			return true;
		case LegendaryHeroAnimations.DefeatStrike:
			state = InternalState.DefeatStrike;
			return true;
		default:
			state = InternalState.Idle;
			return false;
		}
	}

	private bool TryGetStateFromEvent(EmoteType emote, out InternalState state)
	{
		switch (emote)
		{
		case EmoteType.THANKS:
			state = InternalState.Emote_Thanks;
			return true;
		case EmoteType.WELL_PLAYED:
			state = InternalState.Emote_WellPlayed;
			return true;
		case EmoteType.GREETINGS:
			state = InternalState.Emote_Greetings;
			return true;
		case EmoteType.MIRROR_GREETINGS:
			state = InternalState.Emote_Greetings;
			return true;
		case EmoteType.WOW:
			state = InternalState.Emote_Wow;
			return true;
		case EmoteType.OOPS:
			state = InternalState.Emote_Oops;
			return true;
		case EmoteType.THREATEN:
			state = InternalState.Emote_Threaten;
			return true;
		default:
			state = InternalState.Idle;
			return false;
		}
	}

	public bool StateSupported(InternalState state)
	{
		if (AttackCountdown > 0f || HeroPowerCountdown > 0f)
		{
			return false;
		}
		if (DamageCountdown > 0f && state != InternalState.DamageStart)
		{
			return false;
		}
		switch (state)
		{
		case InternalState.Idle:
		case InternalState.SocketIn:
			return true;
		case InternalState.IdleFidget:
			return HasIdleFidgetAnimation;
		case InternalState.Emote_Greetings:
		case InternalState.Emote_Oops:
		case InternalState.Emote_Thanks:
		case InternalState.Emote_Threaten:
		case InternalState.Emote_WellPlayed:
		case InternalState.Emote_Wow:
			return Features.HasFlag(SupportedFeatures.Emotes);
		case InternalState.EquipWeapon:
			return Features.HasFlag(SupportedFeatures.EquipWeapon);
		case InternalState.HeroPowerStart:
			return Features.HasFlag(SupportedFeatures.HeroPowerBirth);
		case InternalState.HeroPowerComplete:
		case InternalState.HeroPowerCompleteAlt:
			return Features.HasFlag(SupportedFeatures.HeroPower);
		case InternalState.Spell:
			return Features.HasFlag(SupportedFeatures.Spell);
		case InternalState.SummonMinion:
			return Features.HasFlag(SupportedFeatures.Summon);
		case InternalState.Victory:
		case InternalState.Defeat:
			return true;
		case InternalState.AttackStart:
			if (HasPreAttackAnimation)
			{
				return Features.HasFlag(SupportedFeatures.Attack);
			}
			return false;
		case InternalState.AttackComplete:
			return Features.HasFlag(SupportedFeatures.Attack);
		case InternalState.DamageStart:
		case InternalState.DamageComplete:
			return Features.HasFlag(SupportedFeatures.Damage);
		case InternalState.IntroStart:
			if (voStartsComeIntoPlay)
			{
				return Features.HasFlag(SupportedFeatures.IntoPlay);
			}
			return false;
		case InternalState.IntroComplete:
			return Features.HasFlag(SupportedFeatures.IntoPlayExit);
		case InternalState.VictoryStrike:
		case InternalState.DefeatStrike:
			return Features.HasFlag(SupportedFeatures.Strikes);
		default:
			return false;
		}
	}

	public void UpdateIgnoreAnimRequests()
	{
		List<LegendaryHeroAnimations> requestsToRemove = new List<LegendaryHeroAnimations>();
		float deltaTime = Time.deltaTime;
		foreach (KeyValuePair<LegendaryHeroAnimations, float> request in m_ignoreAnimRequests.ToList())
		{
			float value = request.Value;
			value -= Time.deltaTime;
			if (value - deltaTime <= 0f)
			{
				requestsToRemove.Add(request.Key);
			}
			else
			{
				m_ignoreAnimRequests[request.Key] = value;
			}
		}
		foreach (LegendaryHeroAnimations request2 in requestsToRemove)
		{
			m_ignoreAnimRequests.Remove(request2);
		}
	}

	public void IgnoreAnim(LegendaryHeroAnimations anim, float time)
	{
		Debug.Log("Adding ignore anim request for " + anim);
		m_ignoreAnimRequests[anim] = time;
	}

	public float GetTransitionTime(InternalState state)
	{
		if ((int)state >= m_transitionTimes.Length)
		{
			float[] transitionTimes = m_transitionTimes;
			m_transitionTimes = Enumerable.Repeat(0.25f, Enum.GetValues(typeof(InternalState)).Length).ToArray();
			transitionTimes.CopyTo(m_transitionTimes, 0);
		}
		return m_transitionTimes[(int)state];
	}

	public void SetTransitionTime(InternalState state, float transitionTime)
	{
		if ((int)state >= m_transitionTimes.Length)
		{
			float[] transitionTimes = m_transitionTimes;
			m_transitionTimes = Enumerable.Repeat(0.25f, Enum.GetValues(typeof(InternalState)).Length).ToArray();
			transitionTimes.CopyTo(m_transitionTimes, 0);
		}
		m_transitionTimes[(int)state] = transitionTime;
	}

	public float GetCurrentAnimTime()
	{
		if (m_lastRequestedState == InternalState.Unknown)
		{
			return 0f;
		}
		AnimatorStateInfo stateInfo = m_masterAnimator.GetCurrentAnimatorStateInfo(0);
		if (stateInfo.fullPathHash != s_animatorStates[m_lastRequestedState])
		{
			stateInfo = m_masterAnimator.GetNextAnimatorStateInfo(0);
			if (stateInfo.fullPathHash != s_animatorStates[m_lastRequestedState])
			{
				return 0f;
			}
		}
		float normalizedTime = stateInfo.normalizedTime;
		if (stateInfo.loop)
		{
			normalizedTime -= (float)(int)normalizedTime;
		}
		return normalizedTime * stateInfo.length;
	}

	public void CheckAndTriggerTransitionListenersForIdle()
	{
		if (!m_masterAnimator.IsInTransition(0) && m_masterAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash == s_animatorStates[InternalState.Idle] && m_lastRequestedState != InternalState.Idle)
		{
			m_lastRequestedState = InternalState.Idle;
			this.OnRequestAnimTransition?.Invoke(InternalState.Idle);
		}
	}
}
