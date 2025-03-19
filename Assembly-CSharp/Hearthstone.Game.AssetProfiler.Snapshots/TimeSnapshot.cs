using System.Text;
using UnityEngine;

namespace Hearthstone.Game.AssetProfiler.Snapshots;

public class TimeSnapshot : IProfilingSnapshot
{
	public long Frame { get; set; }

	public float SecondsSinceStartup { get; set; }

	public void Populate()
	{
		Frame = Time.frameCount;
		SecondsSinceStartup = Time.realtimeSinceStartup;
	}

	public void ToCsvHeader(StringBuilder output, bool appendNewline = false)
	{
		output.Append("Frame");
		output.Append(',');
		output.Append("SecondsSinceStartup");
		output.Append(',');
		if (appendNewline)
		{
			output.AppendLine();
		}
	}

	public void ToCsvLine(StringBuilder output, bool appendNewline = false)
	{
		output.Append(Frame);
		output.Append(',');
		output.Append(SecondsSinceStartup);
		output.Append(',');
		if (appendNewline)
		{
			output.AppendLine();
		}
	}
}
