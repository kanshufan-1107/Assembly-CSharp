using System.Text;

namespace Hearthstone.Game.AssetProfiler.Snapshots;

public class OperatingSystemMemorySnapshot : IProfilingSnapshot
{
	public long SystemUsedBytes { get; set; }

	public long SystemAvailableBytes { get; set; }

	public long AppUsedBytes { get; set; }

	public long AppAvailableBytes { get; set; }

	public void Populate()
	{
	}

	public void ToCsvHeader(StringBuilder output, bool appendNewline = false)
	{
		output.Append("OS.SystemUsedBytes");
		output.Append(',');
		output.Append("OS.SystemAvailableBytes");
		output.Append(',');
		output.Append("OS.AppUsedBytes");
		output.Append(',');
		output.Append("OS.AppAvailableBytes");
		output.Append(',');
		if (appendNewline)
		{
			output.AppendLine();
		}
	}

	public void ToCsvLine(StringBuilder output, bool appendNewline = false)
	{
		output.Append(SystemUsedBytes);
		output.Append(',');
		output.Append(SystemAvailableBytes);
		output.Append(',');
		output.Append(AppUsedBytes);
		output.Append(',');
		output.Append(AppAvailableBytes);
		output.Append(',');
		if (appendNewline)
		{
			output.AppendLine();
		}
	}
}
