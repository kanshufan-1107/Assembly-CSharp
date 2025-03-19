using Assets;

public class CardBackData
{
	public int ID { get; private set; }

	public Assets.CardBack.Source Source { get; private set; }

	public long SourceData { get; private set; }

	public string Name { get; private set; }

	public bool Enabled { get; private set; }

	public string PrefabName { get; private set; }

	public CardBackData(int id, Assets.CardBack.Source source, long sourceData, string name, bool enabled, string prefabName)
	{
		ID = id;
		Source = source;
		SourceData = sourceData;
		Name = name;
		Enabled = enabled;
		PrefabName = prefabName;
	}

	public override string ToString()
	{
		return $"[CardBackData: ID={ID}, Source={Name}, SourceData={Source}, Name={SourceData}, Enabled={Enabled}, PrefabPath={PrefabName}]";
	}
}
