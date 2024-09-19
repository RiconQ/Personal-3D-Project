using System.Collections;
using UnityEngine;

public class ResultsScreen : MonoBehaviour
{
	public Transform tLetter;

	public Transform tCamRoot;

	public StyleRanksInfo ranksInfo;

	public AnimationCurve curve;

	public AnimationCurve blinkCurve;

	public Vector3 aPos;

	public Vector3 bPos;

	public Vector3 aRot;

	public Vector3 bRot;

	public Vector3 aRootRot;

	public Vector3 bRootRot;

	public Vector2 FOVrange = new Vector2(100f, 75f);

	public PerlinShake shake;

	public MissionResultsUI results;

	public Camera cam;

	public ParticleSystem particleRank1;

	public ParticleSystem particleRank2;

	private int rank;

	private float timer;

	private MeshFilter mfLetter;

	private MeshRenderer rend;

	private MaterialPropertyBlock block;

	public AudioClip sfxRankStartMoving;

	public AudioClip sfxRankSlam;

	private void Awake()
	{
		rank = Game.mission.rawResults.rank;
		mfLetter = tLetter.GetComponentInChildren<MeshFilter>();
		mfLetter.sharedMesh = ranksInfo.ranks[rank].mesh;
		rend = tLetter.GetComponentInChildren<MeshRenderer>();
		block = new MaterialPropertyBlock();
		rend.GetPropertyBlock(block);
		block.SetFloat("_Blink", 0f);
		rend.SetPropertyBlock(block);
		tLetter.localPosition = aPos;
		tLetter.localEulerAngles = aRot;
		tCamRoot.localEulerAngles = aRootRot;
		results.gameObject.SetActive(value: false);
	}

	private void Start()
	{
		Game.fading.InstantFade(1f);
		StartCoroutine(Showing());
	}

	private IEnumerator Showing()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		shake.Shake();
		cam.fieldOfView = 100f;
		particleRank1.Play();
		Game.fading.InstantFade(0f);
		Game.sounds.PlayClip(sfxRankStartMoving);
		bool shakePlayed = false;
		while (timer != 1f)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime);
			tLetter.localPosition = Vector3.LerpUnclamped(aPos, bPos, curve.Evaluate(timer));
			tLetter.localEulerAngles = Vector3.LerpUnclamped(aRot, bRot, curve.Evaluate(timer));
			tCamRoot.localEulerAngles = Vector3.LerpUnclamped(aRootRot, bRootRot, curve.Evaluate(timer));
			cam.fieldOfView = Mathf.LerpUnclamped(FOVrange.x, FOVrange.y, curve.Evaluate(timer));
			block.SetFloat("_Blink", blinkCurve.Evaluate(timer) * 2f);
			rend.SetPropertyBlock(block);
			if (!shakePlayed && timer >= 0.9f)
			{
				results.gameObject.SetActive(value: true);
				Game.sounds.PlayClip(sfxRankSlam);
				shake.Shake();
				particleRank2.Play();
				shakePlayed = true;
			}
			yield return null;
		}
	}

	private void Update()
	{
	}
}
