using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Kino Image Effects/Fog")]
public class Fog : MonoBehaviour
{
	[SerializeField]
	private float _startDistance = 1f;

	public float StartDist = 50f;

	public float EndDist = 500f;

	[SerializeField]
	private bool _useRadialDistance;

	[SerializeField]
	private bool _fadeToSkybox;

	[SerializeField]
	private Shader _shader;

	[SerializeField]
	private Material _material;

	[SerializeField]
	private Camera _cam;

	public float startDistance
	{
		get
		{
			return _startDistance;
		}
		set
		{
			_startDistance = value;
		}
	}

	public bool useRadialDistance
	{
		get
		{
			return _useRadialDistance;
		}
		set
		{
			_useRadialDistance = value;
		}
	}

	public bool fadeToSkybox
	{
		get
		{
			return _fadeToSkybox;
		}
		set
		{
			_fadeToSkybox = value;
		}
	}

	private void OnEnable()
	{
	}

	[ImageEffectOpaque]
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		_startDistance = Mathf.Max(_startDistance, 0f);
		_material.SetFloat("_DistanceOffset", _startDistance);
		float startDist = StartDist;
		float endDist = EndDist;
		float num = 1f / Mathf.Max(endDist - startDist, 1E-06f);
		_material.SetFloat("_LinearGrad", 0f - num);
		_material.SetFloat("_LinearOffs", endDist * num);
		_material.DisableKeyword("FOG_EXP");
		_material.DisableKeyword("FOG_EXP2");
		if (_useRadialDistance)
		{
			_material.EnableKeyword("RADIAL_DIST");
		}
		else
		{
			_material.DisableKeyword("RADIAL_DIST");
		}
		if (_fadeToSkybox)
		{
			_material.EnableKeyword("USE_SKYBOX");
			Material skybox = RenderSettings.skybox;
			_material.SetTexture("_SkyCubemap", Shader.GetGlobalTexture("_Skybox"));
			_material.SetColor("_SkyTint", skybox.GetColor("_Tint"));
		}
		else
		{
			_material.DisableKeyword("USE_SKYBOX");
			_material.SetColor("_FogColor", RenderSettings.fogColor);
		}
		Transform obj = _cam.transform;
		float nearClipPlane = _cam.nearClipPlane;
		float farClipPlane = _cam.farClipPlane;
		float num2 = Mathf.Tan(_cam.fieldOfView * ((float)Math.PI / 180f) / 2f);
		Vector3 vector = obj.right * nearClipPlane * num2 * _cam.aspect;
		Vector3 vector2 = obj.up * nearClipPlane * num2;
		Vector3 vector3 = obj.forward * nearClipPlane - vector + vector2;
		Vector3 vector4 = obj.forward * nearClipPlane + vector + vector2;
		Vector3 vector5 = obj.forward * nearClipPlane + vector - vector2;
		Vector3 vector6 = obj.forward * nearClipPlane - vector - vector2;
		float num3 = vector3.magnitude * farClipPlane / nearClipPlane;
		RenderTexture.active = destination;
		_material.SetTexture("_MainTex", source);
		_material.SetPass(0);
		GL.PushMatrix();
		GL.LoadOrtho();
		GL.Begin(7);
		GL.MultiTexCoord2(0, 0f, 0f);
		GL.MultiTexCoord(1, vector6.normalized * num3);
		GL.Vertex3(0f, 0f, 0.1f);
		GL.MultiTexCoord2(0, 1f, 0f);
		GL.MultiTexCoord(1, vector5.normalized * num3);
		GL.Vertex3(1f, 0f, 0.1f);
		GL.MultiTexCoord2(0, 1f, 1f);
		GL.MultiTexCoord(1, vector4.normalized * num3);
		GL.Vertex3(1f, 1f, 0.1f);
		GL.MultiTexCoord2(0, 0f, 1f);
		GL.MultiTexCoord(1, vector3.normalized * num3);
		GL.Vertex3(0f, 1f, 0.1f);
		GL.End();
		GL.PopMatrix();
	}
}
