using System.Text;
using Hearthstone.Game.AssetProfiler.Snapshots;

namespace Hearthstone.Game.AssetProfiler.Events;

public abstract class AssetBundleChangedEvent : IProfilingEvent, IProfilingSnapshot
{
	public abstract string Name { get; }

	public string AssetBundleName { get; }

	public TimeSnapshot Time { get; } = new TimeSnapshot();

	public ApplicationMemorySnapshot Memory { get; } = new ApplicationMemorySnapshot();

	public AssetManagerSnapshot AssetManager { get; } = new AssetManagerSnapshot();

	protected AssetBundleChangedEvent(string assetBundleName)
	{
		AssetBundleName = assetBundleName;
	}

	public void Populate()
	{
		Time.Populate();
		Memory.Populate();
		AssetManager.Populate();
	}

	public void ToCsvHeader(StringBuilder output, bool appendNewline = false)
	{
		Time.ToCsvHeader(output);
		output.Append("AssetBundleName");
		output.Append(',');
		Memory.ToCsvHeader(output);
		AssetManager.ToCsvHeader(output, appendNewline);
	}

	public void ToCsvLine(StringBuilder output, bool appendNewline = false)
	{
		Time.ToCsvLine(output);
		output.Append(AssetBundleName);
		output.Append(',');
		Memory.ToCsvLine(output);
		AssetManager.ToCsvLine(output, appendNewline);
	}
}
