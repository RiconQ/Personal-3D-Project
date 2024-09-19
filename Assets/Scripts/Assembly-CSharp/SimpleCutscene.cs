using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class SimpleCutscene : CutsceneLevel
{
	public CutscenePoint[] points;

	public GameObject Effects;

	private MotionBlur _motionBlur;

	public Text Text;

	public TextAnimator Animator;

	public TextBackground TextBG;

	private void Start()
	{
		Effects.GetComponent<PostProcessVolume>().profile.TryGetSettings<MotionBlur>(out _motionBlur);
		if (points.Length != 0 && (bool)Game.player)
		{
			StartCoroutine(CutscenePlaying());
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		for (int i = 0; i < points.Length; i++)
		{
			Gizmos.DrawSphere(points[i].t.position, 0.5f);
			Gizmos.DrawRay(points[i].t.position, points[i].t.forward);
		}
	}

	private IEnumerator CutscenePlaying()
	{
		Game.fading.speed = 0.5f;
		Game.wideMode.Show();
		Game.player.SetKinematic(value: true);
		Game.player.Deactivate();
		Game.player.enabled = false;
		Game.time.SetDefaultTimeScale(0.75f);
		yield return new WaitForEndOfFrame();
		int i = 0;
		CutscenePoint[] array = points;
		foreach (CutscenePoint p in array)
		{
			float timer = 0f;
			_motionBlur.active = false;
			Game.player.mouseLook.LookInDir(p.t.forward);
			Game.player.tHead.position = p.t.position;
			p.Event.Invoke();
			Text.text = p.text.ToUpper();
			if ((bool)Animator && p.text.Length > 0)
			{
				Animator.ResetAndPlay();
			}
			yield return new WaitForEndOfFrame();
			_motionBlur.active = true;
			TextBG.Setup();
			while (timer != 1f)
			{
				timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 0.2f);
				Game.player.tHead.Translate(p.t.forward * Time.deltaTime * 0.3f, Space.World);
				yield return null;
			}
			i++;
		}
		Game.player.tHead.position = BlackBox.instance.tPlayerPosition.position;
		Game.player.mouseLook.SetRotation(BlackBox.instance.tPlayerPosition.rotation);
		Text.gameObject.SetActive(value: false);
		Game.fading.InstantFade(0f);
		CameraController.shake.Shake(2);
		BlackBox.instance.gameObject.SetActive(value: true);
		BlackBox.instance.source.Play();
		BlackBox.instance.particle.Play();
		while (BlackBox.instance.particle.isPlaying)
		{
			yield return null;
		}
		Game.instance.LoadLevel(LevelToLoad.sceneName);
	}
}
