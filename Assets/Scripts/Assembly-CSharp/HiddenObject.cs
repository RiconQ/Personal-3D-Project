using System;
using System.Collections;
using UnityEngine;

public class HiddenObject : MonoBehaviour
{
	public static Action OnPick = delegate
	{
	};

	public AudioSource source;

	public AudioClip pickSound;

	public Transform tMesh;

	public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public MeshRenderer rend;

	private MaterialPropertyBlock block;

	private void Start()
	{
		if (LevelsData.currentPlayableLevel.results.secret != 1)
		{
			block = new MaterialPropertyBlock();
			rend.GetPropertyBlock(block);
			source.volume = 1f;
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
	}

	private void OnTriggerStay(Collider other)
	{
		if (Game.player.grounder.grounded && Game.player.IsActive())
		{
			StartCoroutine(Picking());
		}
	}

	private IEnumerator Picking()
	{
		GetComponent<Collider>().enabled = false;
		Vector3 aPos = tMesh.localPosition;
		Vector3 bPos = new Vector3(0f, 2f, 0f);
		Game.player.SetKinematic(value: true);
		Game.player.Deactivate();
		Game.player.weapons.gameObject.SetActive(value: false);
		Game.player.mouseLook.enabled = false;
		Game.wideMode.Show();
		float timer = 0f;
		while (timer != 1f)
		{
			source.volume = 1f - timer;
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 0.3f);
			tMesh.localPosition = Vector3.LerpUnclamped(aPos, bPos, curve.Evaluate(timer));
			tMesh.rotation = Quaternion.Slerp(tMesh.rotation, Quaternion.LookRotation(tMesh.position.DirTo(Game.player.t.position)), Time.deltaTime * 10f);
			Game.player.camController.Angle(Mathf.Sin(curve.Evaluate(timer) * (float)Math.PI) * 10f);
			Game.player.mouseLook.LookAtSmooth(tMesh.position);
			block.SetFloat("_Dissolve", Mathf.Clamp01(-0.5f + timer * 1.5f));
			rend.SetPropertyBlock(block);
			yield return null;
		}
		if (OnPick != null)
		{
			OnPick();
		}
		Game.player.SetKinematic(value: false);
		Game.player.Activate();
		Game.player.weapons.gameObject.SetActive(value: true);
		Game.player.mouseLook.enabled = true;
		Game.wideMode.Hide();
	}
}
