using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public sealed class LightTrailsRenderer : PostProcessEffectRenderer<LightTrails>
{
	private Shader shader;

	private RenderTexture tex;

	private float timer;

	public override void Init()
	{
		shader = Shader.Find("Hidden/ClearFlagsImageEffect");
		tex = new RenderTexture(Screen.width / 4, Screen.height / 4, 16);
		base.Init();
	}

	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(shader);
		propertySheet.properties.SetTexture("_prevFrame", tex);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
		if (Time.time > timer)
		{
			context.command.Blit(RenderTexture.active, tex);
			timer += 0.01f;
		}
	}
}
