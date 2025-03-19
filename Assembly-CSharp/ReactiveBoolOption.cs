public class ReactiveBoolOption : ReactiveOption<bool>
{
	private ReactiveBoolOption(Option val)
		: base(val)
	{
	}

	public static ReactiveBoolOption CreateInstance(Option opt)
	{
		ReactiveObject<bool> obj = ReactiveObject<bool>.GetExistingInstance(ReactiveOption<bool>.GetOptionId(opt));
		if (obj == null)
		{
			obj = new ReactiveBoolOption(opt);
		}
		return obj as ReactiveBoolOption;
	}

	protected override bool DoFetchValue()
	{
		return Options.Get().GetBool(m_option);
	}

	public override void Set(bool newValue)
	{
		Options.Get().SetBool(m_option, newValue);
	}
}
