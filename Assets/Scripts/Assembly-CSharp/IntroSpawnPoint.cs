using System;
using System.Collections;
using UnityEngine;

public class IntroSpawnPoint : SavePoint
{
	public Vector3 aPos;

	public Vector3 bPos;

	public Vector3 bob;

	public Animation anim;

	public Animation animB;

	public Transform tDagger;

	public Transform waypoint;

	public Transform lookpoint;

	public AnimationCurve curveFOV = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public AnimationCurve posCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public SceneData levelToLoad;

	public override void Launch()
	{
		StartCoroutine(Showing());
	}

	private IEnumerator Showing()
	{
		yield return new WaitForEndOfFrame();
		Game.player.rb.isKinematic = true;
		Game.player.Deactivate();
		Game.wideMode.Show();
		aPos = base.transform.position;
		bPos = waypoint.position;
		anim.Play();
		yield return null;
		float timer2 = 0f;
		while (timer2 != 1f)
		{
			bob.z = Mathf.Sin(timer2 * 20f * (float)Math.PI) * 0.1f;
			bob.y = Mathf.Cos(timer2 * 20f * (float)Math.PI * 2f) * 0.1f;
			timer2.MoveTowards(1f, 0.05f);
			Game.player.t.position = Vector3.LerpUnclamped(aPos, bPos, timer2);
			Game.player.t.position += bob * posCurve.Evaluate(timer2);
			Game.player.mouseLook.LookInDir(Vector3.Lerp(base.t.forward, Game.player.tHead.position.DirTo(lookpoint.position), curve.Evaluate(timer2)));
			yield return null;
		}
		animB.Play();
		CameraController.shake.Shake(2);
		float fov = Camera.main.fieldOfView;
		float fovB = 50f;
		float targetTime = animB.clip.length;
		timer2 = 0f;
		while (animB.isPlaying)
		{
			timer2 = Mathf.MoveTowards(timer2, targetTime, Time.deltaTime);
			Game.player.mouseLook.LookInDirSmooth(Game.player.tHead.position.DirTo(tDagger.position));
			Camera.main.fieldOfView = Mathf.LerpUnclamped(fov, fovB, curveFOV.Evaluate(timer2 / targetTime));
			yield return null;
		}
		CameraController.shake.Shake();
		yield return new WaitForSeconds(1f);
		Game.instance.LoadLevel(levelToLoad.sceneReference.ScenePath, quickLoad: true);
		Game.fading.InstantFade(1f);
	}
}
