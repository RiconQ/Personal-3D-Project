using System.Collections;
using UnityEngine;

public class GameTitle : MonoBehaviour
{
	public GameTitleLetter[] letters;

	public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public AnimationCurve rotCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public float speed = 1f;

	public float delay = 0.1f;

	public float knightDelay = 1f;

	public string text = "Press E to Continue";

	public bool subTitle;

	public ParticleSystem particle;

	public ParticleSystem particleShady;

	public PerlinShake shake;

	public MeshRenderer[] rends;

	private MaterialPropertyBlock block;

	private float alpha;

	public Vector2 xOffsetRange = new Vector2(0f, 4f);

	public Vector2 yOffsetRange = new Vector2(2f, 2.5f);

	public Vector2 zOffsetRange = new Vector2(-2f, 2f);

	public Vector2 xAngleRange = new Vector2(-15f, 15f);

	public Vector2 yAngleRange = new Vector2(-15f, 15f);

	public Vector2 zAngleRange = new Vector2(-15f, 15f);

	private WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

	private Vector3 tempPos;

	private Quaternion tempRot;

	private void Awake()
	{
		rends = GetComponentsInChildren<MeshRenderer>();
		block = new MaterialPropertyBlock();
		rends[0].GetPropertyBlock(block);
		letters = GetComponentsInChildren<GameTitleLetter>();
		for (int i = 0; i < letters.Length; i++)
		{
			letters[i].Setup(this);
		}
	}

	private void OnEnable()
	{
		alpha = -1f;
		StopAllCoroutines();
		StartCoroutine(Showing());
	}

	private IEnumerator Showing()
	{
		for (int i = 0; i < letters.Length; i++)
		{
			if (i < 5)
			{
				tempPos.x = Random.Range(xOffsetRange.x, xOffsetRange.y);
				tempPos.z = Random.Range(zOffsetRange.x, zOffsetRange.y);
				tempPos.y = Random.Range(yOffsetRange.x, yOffsetRange.y);
				letters[i].Prepare(tempPos, Quaternion.Euler(Random.Range(xAngleRange.x, xAngleRange.y), Random.Range(yAngleRange.x, yAngleRange.y), Random.Range(zAngleRange.x, zAngleRange.y)));
			}
			else
			{
				tempPos.x = 0f;
				tempPos.y = Random.Range(-0.2f, 0.2f);
				tempPos.z = 0f;
				letters[i].Prepare(tempPos, Quaternion.Euler(0f, 0f, 90f));
			}
		}
		while (Game.fading.cg.alpha > 0.5f)
		{
			yield return null;
		}
		yield return waitForEndOfFrame;
		yield return waitForEndOfFrame;
		yield return waitForEndOfFrame;
		shake.Shake();
		bool wait = true;
		float time = 0f;
		bool title = false;
		subTitle = false;
		while (wait)
		{
			time += Time.deltaTime * speed;
			for (int j = 0; j < letters.Length; j++)
			{
				float num = Mathf.Clamp01(time - ((j < 5) ? ((float)j * delay) : (knightDelay + (float)j * 0.02f)));
				letters[j].Tick(num);
				if (!title && j == 1 && num > 0f)
				{
					title = true;
					particleShady.Play();
				}
				if (!subTitle && j == 5 && num > 0f)
				{
					subTitle = true;
					particle.Play();
					shake.Shake(1);
				}
				if (j == letters.Length - 1 && num == 1f)
				{
					wait = false;
					break;
				}
			}
			yield return null;
		}
		Game.message.Show(text);
	}
}
