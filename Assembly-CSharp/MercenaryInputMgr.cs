using System;

public class MercenaryInputMgr : InputMgr
{
	private static MercenaryInputMgr s_instance;

	public Func<bool> MouseOverTargetEvaluator;

	protected override bool MouseIsOverDeck
	{
		get
		{
			if (MouseOverTargetEvaluator != null)
			{
				return MouseOverTargetEvaluator();
			}
			return base.MouseIsOverDeck;
		}
		set
		{
			base.MouseIsOverDeck = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		s_instance = this;
	}

	protected override void OnDestroy()
	{
		s_instance = null;
		base.OnDestroy();
	}

	public new static MercenaryInputMgr Get()
	{
		return s_instance;
	}
}
