public interface RenderToTexturePostProcess
{
	void End();

	void AddCommandBuffers();

	bool IsUsedBy(DiamondRenderToTexture r2t);
}
