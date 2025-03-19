using System.Text;
using Hearthstone.Game.AssetProfiler.Snapshots;

namespace Hearthstone.Game.AssetProfiler.Events;

public abstract class ModeChangedEvent : IProfilingEvent, IProfilingSnapshot
{
	public abstract string Name { get; }

	public string PreviousMode { get; }

	public string NextMode { get; }

	public TimeSnapshot Time { get; } = new TimeSnapshot();

	public ApplicationMemorySnapshot Memory { get; } = new ApplicationMemorySnapshot();

	public AssetManagerSnapshot AssetManager { get; } = new AssetManagerSnapshot();

	protected ModeChangedEvent(string previousMode, string nextMode)
	{
		PreviousMode = previousMode;
		NextMode = nextMode;
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
		output.Append("Scene");
		output.Append(',');
		output.Append("Mode");
		output.Append(',');
		Memory.ToCsvHeader(output);
		AssetManager.ToCsvHeader(output, appendNewline);
	}

	public void ToCsvLine(StringBuilder output, bool appendNewline = false)
	{
		Time.ToCsvLine(output);
		output.Append(PreviousMode);
		output.Append(',');
		output.Append(NextMode);
		output.Append(',');
		Memory.ToCsvLine(output);
		AssetManager.ToCsvLine(output, appendNewline);
	}
}
