using System;
using UnityEngine;

public class DashPoint : DashableObject, IInteractable
{
	public Weapon[] weapons;

	public bool used;

	public Vector3 dir;

	public Vector3 pos;

	public Transform tMesh;

	public MeshFilter mf;

	private void Start()
	{
		if (weapons.Length != 0)
		{
			mf.sharedMesh = weapons[0].mesh;
		}
		if (!WeaponsControl.allWeapons.Contains(base.t))
		{
			WeaponsControl.allWeapons.Add(base.t);
		}
		Grounder grounder = Game.player.grounder;
		grounder.OnGrounded = (Action)Delegate.Combine(grounder.OnGrounded, new Action(Reset));
		QuickmapScene.OnEditMode = (Action)Delegate.Combine(QuickmapScene.OnEditMode, new Action(Reset));
	}

	private void OnDestroy()
	{
		Grounder grounder = Game.player.grounder;
		grounder.OnGrounded = (Action)Delegate.Remove(grounder.OnGrounded, new Action(Reset));
		QuickmapScene.OnEditMode = (Action)Delegate.Remove(QuickmapScene.OnEditMode, new Action(Reset));
		if (WeaponsControl.allWeapons.Contains(base.t))
		{
			WeaponsControl.allWeapons.Remove(base.t);
		}
	}

	private void Update()
	{
		pos.y = Mathf.Sin(Time.time) * 0.25f;
		tMesh.localPosition = pos;
		tMesh.Rotate(0f, 90f * Time.deltaTime, 0f);
	}

	private void Reset()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			used = false;
			base.gameObject.SetActive(value: true);
		}
	}

	public void Interact()
	{
	}

	public override void PreDash()
	{
		dir = Game.player.tHead.position.DirTo(base.t.position);
		Debug.DrawRay(base.t.position, dir, Color.green, 2f);
	}

	public override void Dash()
	{
		if (!used)
		{
			used = true;
			if (weapons.Length != 0)
			{
				Game.player.weapons.Pick(weapons[0].index);
			}
		}
		Game.player.airControlBlock = 0.3f;
		QuickEffectsPool.Get("Dash Point FX", base.t.position, Quaternion.LookRotation(dir)).Play();
		base.gameObject.SetActive(value: false);
	}
}
