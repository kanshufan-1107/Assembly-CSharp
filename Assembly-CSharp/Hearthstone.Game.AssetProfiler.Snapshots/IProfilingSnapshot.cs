using System.Text;

namespace Hearthstone.Game.AssetProfiler.Snapshots;

public interface IProfilingSnapshot
{
	void Populate();

	void ToCsvHeader(StringBuilder output, bool appendNewline = false);

	void ToCsvLine(StringBuilder output, bool appendNewline = false);
}
