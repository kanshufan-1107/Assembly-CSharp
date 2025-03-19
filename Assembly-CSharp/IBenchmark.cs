public abstract class IBenchmark
{
	public abstract bool Setup();

	public abstract void Run();

	public abstract void Cleanup();
}
