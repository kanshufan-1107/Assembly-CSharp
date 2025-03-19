public class ReactiveNetCacheObject<T> : ReactiveObject<T> where T : new()
{
	private ReactiveNetCacheObject()
	{
	}

	public static ReactiveNetCacheObject<T> CreateInstance()
	{
		ReactiveObject<T> obj = ReactiveObject<T>.GetExistingInstance();
		if (obj == null)
		{
			obj = new ReactiveNetCacheObject<T>();
		}
		return obj as ReactiveNetCacheObject<T>;
	}

	protected override T FetchValue()
	{
		NetCache netCache = NetCache.Get();
		if (netCache == null)
		{
			return default(T);
		}
		return netCache.GetNetObject<T>();
	}

	protected override bool RegisterChangeCallback()
	{
		NetCache netCache = NetCache.Get();
		if (netCache == null)
		{
			return false;
		}
		netCache.RegisterUpdatedListener(typeof(T), base.OnObjectChanged);
		return true;
	}
}
