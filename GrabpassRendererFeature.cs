using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GrabpassRendererFeature : ScriptableRendererFeature
{
	class GrabpassRenderPass : ScriptableRenderPass
	{
		const string textureName = "_Grabpass";
        
		Material material;
		RTHandle tempTexture;
		RTHandle sourceTexture;

		public GrabpassRenderPass(Material material, RenderPassEvent renderPassEvent) : base()
		{
			this.material = material;
			this.renderPassEvent = renderPassEvent;
		}

		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			sourceTexture = renderingData.cameraData.renderer.cameraColorTargetHandle;
			tempTexture = RTHandles.Alloc(new RenderTargetIdentifier(textureName), textureName);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			CommandBuffer commandBuffer = CommandBufferPool.Get("FullScreenRenderFeature");
			RenderTextureDescriptor targetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
			targetDescriptor.depthBufferBits = 0;
			commandBuffer.GetTemporaryRT(Shader.PropertyToID(tempTexture.name), targetDescriptor, FilterMode.Point);
			Blit(commandBuffer, sourceTexture, tempTexture, material);
			Blit(commandBuffer, tempTexture, sourceTexture);
			context.ExecuteCommandBuffer(commandBuffer);
			CommandBufferPool.Release(commandBuffer);
		}

		public override void OnCameraCleanup(CommandBuffer cmd)
		{
			tempTexture.Release();
		}
	}

    	[SerializeField] Material material;
	[SerializeField] RenderPassEvent renderPassEvent;
	GrabpassRenderPass grabpassRenderPass;

	public override void Create()
	{
		grabpassRenderPass = new GrabpassRenderPass(material, renderPassEvent);
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		renderer.EnqueuePass(grabpassRenderPass);
	}
}
