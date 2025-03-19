namespace Hearthstone.Streaming;

public enum ModuleState
{
	Unknown = -1,
	NotRequested,
	Queued,
	Downloading,
	ReadyToPlay,
	FullyInstalled
}
