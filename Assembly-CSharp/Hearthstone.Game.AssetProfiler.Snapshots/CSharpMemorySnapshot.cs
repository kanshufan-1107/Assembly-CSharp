using System;
using System.Text;

namespace Hearthstone.Game.AssetProfiler.Snapshots;

public class CSharpMemorySnapshot : IProfilingSnapshot
{
	public long TotalAllocatedSize { get; set; }

	public void Populate()
	{
		TotalAllocatedSize = GC.GetTotalMemory(forceFullCollection: false);
	}

	public void ToCsvHeader(StringBuilder output, bool appendNewline = false)
	{
		output.Append("C#.TotalAllocatedSize");
		output.Append(',');
		if (appendNewline)
		{
			output.AppendLine();
		}
	}

	public void ToCsvLine(StringBuilder output, bool appendNewline = false)
	{
		output.Append(TotalAllocatedSize);
		output.Append(',');
		if (appendNewline)
		{
			output.AppendLine();
		}
	}
}
