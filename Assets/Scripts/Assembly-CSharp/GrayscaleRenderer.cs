using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public sealed class GrayscaleRenderer : PostProcessEffectRenderer<Grayscale>
{
	private Shader shader;

	public override void Init()
	{
		shader = Shader.Find("Hidden/Custom/Grayscale");
		base.Init();
	}

	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(shader);
		propertySheet.properties.SetFloat("_Blend", base.settings.blend);
		propertySheet.properties.SetTexture("_Normal", base.settings.tex);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
