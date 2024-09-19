using UnityEngine;

public class EnemyMaterial : MonoBehaviour
{
	private bool stunned;

	private MeshRenderer[] rends;

	private MaterialPropertyBlock block;

	public float rimFadeSpeed = 1f;

	private int i;

	private int count;

	private Color stunColor = new Color(0.75f, 0.75f, 0.75f);

	public float blinkTimer { get; private set; }

	public void Setup()
	{
		rends = GetComponentsInChildren<MeshRenderer>();
		count = rends.Length;
		block = new MaterialPropertyBlock();
		rends[0].GetPropertyBlock(block);
	}

	public void Blink(float value = 1f)
	{
		blinkTimer = value;
		block.SetFloat("_Blink", value);
		for (i = 0; i < count; i++)
		{
			rends[i].SetPropertyBlock(block);
		}
	}

	public void SetFloatByName(string name, float value)
	{
		value = Mathf.Clamp01(value);
		block.SetFloat(name, value);
		for (i = 0; i < count; i++)
		{
			rends[i].SetPropertyBlock(block);
		}
	}

	public void SetColorByName(string name, Color value)
	{
		block.SetColor(name, value);
		for (i = 0; i < count; i++)
		{
			rends[i].SetPropertyBlock(block);
		}
	}

	public void ResetBlink()
	{
		blinkTimer = 0f;
		block.SetFloat("_Blink", 0f);
		for (i = 0; i < count; i++)
		{
			rends[i].SetPropertyBlock(block);
		}
	}

	public void Stun()
	{
		stunned = true;
	}

	public void Unstun()
	{
		stunned = false;
		ResetBlink();
	}

	public void Dissolve(float value)
	{
		block.SetFloat("_Glow", Mathf.Clamp01(value * 20f));
		block.SetFloat("_Dissolve", value);
		for (i = 0; i < count; i++)
		{
			rends[i].SetPropertyBlock(block);
		}
	}

	private void Update()
	{
		if (blinkTimer != -0.1f || stunned)
		{
			if (stunned)
			{
				blinkTimer = Mathf.Sin(Time.time * 12f) * 0.25f + 0.25f;
			}
			else
			{
				blinkTimer = Mathf.MoveTowards(blinkTimer, -0.1f, Time.deltaTime * 2f);
			}
			block.SetFloat("_Blink", Mathf.Clamp(blinkTimer * 2f, -0.1f, rimFadeSpeed));
			block.SetColor("_BlinkColor", stunned ? stunColor : Color.red);
			for (i = 0; i < count; i++)
			{
				rends[i].SetPropertyBlock(block);
			}
		}
	}
}
