using System;
using UnityEngine;

public class TheSkull : MonoBehaviour
{
	public static Action OnEvent = delegate
	{
	};

	public static TheSkullSequence overrideSequence;

	public static string overrideSceneToLoad = "";

	public Transform tCam;

	public Transform tSkull;

	public Transform tMesh;

	public TextAnimator anim;

	public TheSkullSequence sequence;

	public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public AnimationCurve camCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public AudioSource source;

	public PerlinShake perlinShake;

	public CanvasGroup cg;

	public KeyboardInputs inputs;

	private bool leaving;

	private int index = -1;

	private float timer;

	private float camTimer;

	private float holdTimer;

	private Vector3 startPos;

	private Vector3 pos;

	private Vector3 shake;

	private Vector3 angles;

	private Quaternion rot;

	private Vector3 aPos = new Vector3(0f, 0f, 0f);

	private Vector3 bPos = new Vector3(0f, 0f, -1f);

	private void Start()
	{
		if (overrideSequence != null)
		{
			sequence = overrideSequence;
			overrideSequence = null;
		}
		if (overrideSceneToLoad.Length == 0)
		{
			overrideSceneToLoad = LevelsData.instance.hubs[1].levels[0].sceneReference.ScenePath;
		}
		NextLine();
		tCam.localPosition = bPos;
		camTimer = 0f;
		timer = 0f;
	}

	private void NextLine()
	{
		if (anim.isPlaying && !anim.LastCharReached())
		{
			return;
		}
		int num = index;
		num++;
		if (num < sequence.lines.Length)
		{
			perlinShake.Shake();
			timer = 0f;
			index = num;
			startPos = tSkull.position;
			anim.text.text = sequence.lines[index].message;
			anim.ResetAndPlay();
			source.volume = 1f;
			if (sequence.lines[index].triggerEvent && OnEvent != null)
			{
				OnEvent();
			}
		}
		else if (!leaving)
		{
			leaving = true;
			Game.fading.Fade(1f, 4f);
			Game.instance.LoadLevel(overrideSceneToLoad);
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(inputs.playerKeys[6].key) || Input.GetButtonDown("Accept") || Input.GetButtonDown("Jump") || Input.GetKeyDown(inputs.playerKeys[8].key) || InputsManager.rTriggerPressed)
		{
			NextLine();
		}
		if (Input.GetKey(inputs.playerKeys[6].key) || Input.GetButton("Accept") || Input.GetButton("Jump") || Input.GetKey(inputs.playerKeys[8].key) || InputsManager.rTriggerHolded)
		{
			if (!leaving)
			{
				holdTimer = Mathf.MoveTowards(holdTimer, 1f, Time.deltaTime * 2f);
				if (holdTimer == 1f)
				{
					leaving = true;
					Game.fading.Fade(1f, 4f);
					Game.instance.LoadLevel(overrideSceneToLoad);
				}
			}
		}
		else
		{
			holdTimer = 0f;
		}
		cg.alpha = Mathf.Lerp(cg.alpha, ((index == 0) & !anim.isPlaying) ? 1 : 0, Time.unscaledDeltaTime * 4f);
		shake.x = 0.5f - Mathf.PerlinNoise(Time.time * 1.5f, 0f) * 2f;
		shake.y = 0.5f - Mathf.PerlinNoise(0.25f, Time.time * 3f) * 2f;
		shake.z = 0.5f - Mathf.PerlinNoise((0f - Time.time) * 2f, 0.5f) * 2f;
		if (index > -1 && sequence.lines[index].mood != 0)
		{
			switch (sequence.lines[index].mood)
			{
			case SkullSpeech.Mood.Positive:
				angles.x = Mathf.LerpAngle(angles.x, Mathf.Sin(Time.time * 3f) * 6f, Time.deltaTime * 4f);
				angles.y = Mathf.LerpAngle(angles.y, 0f, Time.deltaTime);
				angles.z = Mathf.LerpAngle(angles.z, 0f, Time.deltaTime);
				break;
			case SkullSpeech.Mood.Negative:
				angles.x = Mathf.LerpAngle(angles.x, 0f, Time.deltaTime);
				angles.y = Mathf.LerpAngle(angles.y, Mathf.Sin(Time.time * 3f) * 6f, Time.deltaTime * 4f);
				angles.z = Mathf.LerpAngle(angles.z, 0f, Time.deltaTime);
				break;
			case SkullSpeech.Mood.Surprised:
				angles.x = Mathf.LerpAngle(angles.x, 0f, Time.deltaTime);
				angles.y = Mathf.LerpAngle(angles.y, 0f, Time.deltaTime);
				angles.z = Mathf.LerpAngle(angles.z, 15f, Time.deltaTime * 2f);
				break;
			}
		}
		else
		{
			angles.x = Mathf.LerpAngle(angles.x, 0f, Time.deltaTime);
			angles.y = Mathf.LerpAngle(angles.y, 0f, Time.deltaTime);
			angles.z = Mathf.LerpAngle(angles.z, 0f, Time.deltaTime);
		}
		tMesh.localEulerAngles = shake + angles;
		tCam.rotation = Quaternion.Slerp(tCam.rotation, Quaternion.LookRotation(tCam.position.DirTo(tSkull.position)), Time.deltaTime * 4f);
		tCam.position = Vector3.Lerp(tCam.position, leaving ? bPos : aPos, Time.unscaledDeltaTime * 4f);
		tMesh.localPosition = new Vector3(0f, Mathf.Sin(Time.time) * 0.05f, 0f);
		if (index > -1)
		{
			tSkull.rotation = Quaternion.Lerp(tSkull.rotation, Quaternion.LookRotation(tSkull.position.DirTo(sequence.lines[index].lookAtPos)), Time.deltaTime * 4f);
			if (timer != 1f)
			{
				timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 0.5f);
				pos = Vector3.LerpUnclamped(startPos, sequence.lines[index].pos, curve.Evaluate(timer));
				pos.y -= Mathf.Sin(curve.Evaluate(timer) * (float)Math.PI) * 0.1f;
				tSkull.position = pos;
			}
		}
		source.volume = Mathf.Lerp(source.volume, 1f - timer, Time.deltaTime * 0.5f);
	}
}
