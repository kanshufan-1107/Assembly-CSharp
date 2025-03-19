namespace Hearthstone.Login;

public struct LoginStrategyParameters
{
	public IMobileAuth MobileAuth { get; }

	public string ChallengeUrl { get; }
}
