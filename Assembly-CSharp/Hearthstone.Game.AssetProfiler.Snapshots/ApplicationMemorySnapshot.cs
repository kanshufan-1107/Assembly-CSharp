using System.Text;

namespace Hearthstone.Game.AssetProfiler.Snapshots;

public class ApplicationMemorySnapshot : IProfilingSnapshot
{
	public CSharpMemorySnapshot CSharpMemory { get; set; } = new CSharpMemorySnapshot();

	public void Populate()
	{
		CSharpMemory.Populate();
	}

	public void ToCsvHeader(StringBuilder output, bool appendNewline = false)
	{
		CSharpMemory.ToCsvHeader(output);
		if (appendNewline)
		{
			output.AppendLine();
		}
	}

	public void ToCsvLine(StringBuilder output, bool appendNewline = false)
	{
		CSharpMemory.ToCsvLine(output);
		if (appendNewline)
		{
			output.AppendLine();
		}
	}
}
