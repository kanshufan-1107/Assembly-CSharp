using System.Text;
using Hearthstone.Game.AssetProfiler.Snapshots;

namespace Hearthstone.Game.AssetProfiler.Events;

public class AssetUnreferencedEvent : IProfilingEvent, IProfilingSnapshot
{
	public string Name { get; } = "AssetUnreferencedEvent";

	public string AssetAddress { get; }

	public TimeSnapshot Time { get; } = new TimeSnapshot();

	public ApplicationMemorySnapshot Memory { get; } = new ApplicationMemorySnapshot();

	public AssetManagerSnapshot AssetManager { get; } = new AssetManagerSnapshot();

	public AssetUnreferencedEvent(string assetAddress)
	{
		AssetAddress = assetAddress;
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
		output.Append("AssetAddress");
		output.Append(',');
		Memory.ToCsvHeader(output);
		AssetManager.ToCsvHeader(output, appendNewline);
	}

	public void ToCsvLine(StringBuilder output, bool appendNewline = false)
	{
		Time.ToCsvLine(output);
		output.Append(AssetAddress);
		output.Append(',');
		Memory.ToCsvLine(output);
		AssetManager.ToCsvLine(output, appendNewline);
	}
}
