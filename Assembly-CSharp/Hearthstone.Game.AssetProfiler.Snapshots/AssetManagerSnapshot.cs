using System.Text;
using Blizzard.T5.AssetManager;

namespace Hearthstone.Game.AssetProfiler.Snapshots;

public class AssetManagerSnapshot : IProfilingSnapshot
{
	private readonly PopulateAssetManagerDebugStatsHandler m_onPopulateStats;

	public AssetManagerDebugStats DebugStats { get; set; } = new AssetManagerDebugStats();

	public AssetManagerSnapshot(PopulateAssetManagerDebugStatsHandler onPopulateStats = null)
	{
		m_onPopulateStats = onPopulateStats;
	}

	public void Populate()
	{
		(m_onPopulateStats ?? new PopulateAssetManagerDebugStatsHandler(PopulateDebugStatsFromAssetLoader))(DebugStats);
	}

	private void PopulateDebugStatsFromAssetLoader(AssetManagerDebugStats debugStats)
	{
		AssetLoader.Get().PopulateDebugStats(debugStats, AssetManagerDebugStats.DataFields.UniqueOpenBundles | AssetManagerDebugStats.DataFields.TotalBundleRefs | AssetManagerDebugStats.DataFields.UniqueOpenAssets | AssetManagerDebugStats.DataFields.TotalOutstandingHandles);
	}

	public void ToCsvHeader(StringBuilder output, bool appendNewline = false)
	{
		output.Append("UniqueOpenBundles");
		output.Append(',');
		output.Append("TotalBundleRefs");
		output.Append(',');
		output.Append("UniqueOpenAssets");
		output.Append(',');
		output.Append("TotalOutstandingHandles");
		output.Append(',');
		if (appendNewline)
		{
			output.AppendLine();
		}
	}

	public void ToCsvLine(StringBuilder output, bool appendNewline = false)
	{
		output.Append(DebugStats.uniqueOpenBundles);
		output.Append(',');
		output.Append(DebugStats.totalBundleRefs);
		output.Append(',');
		output.Append(DebugStats.uniqueOpenAssets);
		output.Append(',');
		output.Append(DebugStats.totalOutstandingHandles);
		output.Append(',');
		if (appendNewline)
		{
			output.AppendLine();
		}
	}
}
