namespace Hearthstone.APIGateway;

internal struct OAuthConfiguration
{
	public string ClientID { get; set; }

	public string AuthEndpointURL { get; set; }

	public bool IsValid
	{
		get
		{
			if (!string.IsNullOrEmpty(ClientID))
			{
				return !string.IsNullOrEmpty(AuthEndpointURL);
			}
			return false;
		}
	}
}
