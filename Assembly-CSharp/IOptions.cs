public interface IOptions
{
	bool GetBool(Option option, bool defaultVal);

	void SetBool(Option option, bool val);
}
