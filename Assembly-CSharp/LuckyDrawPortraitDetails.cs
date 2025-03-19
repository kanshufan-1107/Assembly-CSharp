using Hearthstone.UI;

public class LuckyDrawPortraitDetails : BaconCollectionDetails
{
	protected override string DebugTextValue => "Debug Text";

	public override void AssignDataModels(IDataModel dataModel, IDataModel pageDataModel)
	{
	}

	protected override void Start()
	{
		base.Start();
		base.gameObject.SetActive(value: true);
	}

	protected override void ClearDataModels()
	{
	}

	protected override void DetailsEventListener(string eventName)
	{
	}

	protected override bool ValidateDataModels(IDataModel dataModel, IDataModel pageDataModel)
	{
		return true;
	}
}
