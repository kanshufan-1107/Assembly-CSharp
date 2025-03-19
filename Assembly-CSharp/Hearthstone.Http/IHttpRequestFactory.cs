namespace Hearthstone.Http;

public interface IHttpRequestFactory
{
	IHttpRequest CreateGetRequest(string uri);
}
