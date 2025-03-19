using System.Text;
using Hearthstone.Game.AssetProfiler.Snapshots;

namespace Hearthstone.Game.AssetProfiler.Events;

public class AssetLoadedEvent : IProfilingEvent, IProfilingSnapshot
{
	public string Name => "AssetLoadedEvent";

	public string AssetAddress { get; }

	public string AssetPath { get; }

	public TimeSnapshot Time { get; } = new TimeSnapshot();

	public ApplicationMemorySnapshot Memory { get; } = new ApplicationMemorySnapshot();

	public AssetManagerSnapshot AssetManager { get; } = new AssetManagerSnapshot();

	public AssetLoadedEvent(string assetAddress, string assetPath)
	{
		AssetAddress = assetAddress;
		AssetPath = assetPath;
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
		output.Append("AssetPath");
		output.Append(',');
		Memory.ToCsvHeader(output);
		AssetManager.ToCsvHeader(output, appendNewline);
	}

	public void ToCsvLine(StringBuilder output, bool appendNewline = false)
	{
		Time.ToCsvLine(output);
		output.Append(AssetAddress);
		output.Append(',');
		output.Append(AssetPath);
		output.Append(',');
		Memory.ToCsvLine(output);
		AssetManager.ToCsvLine(output, appendNewline);
	}
}
