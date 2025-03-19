using Hearthstone.Game.AssetProfiler.Snapshots;

namespace Hearthstone.Game.AssetProfiler.Events;

public interface IProfilingEvent : IProfilingSnapshot
{
	string Name { get; }
}
