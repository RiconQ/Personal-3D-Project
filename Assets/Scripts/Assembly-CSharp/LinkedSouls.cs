using System;
using UnityEngine;

public class LinkedSouls : MonoBehaviour
{
	public BaseEnemy enemyA;

	public BaseEnemy enemyB;

	public DamageType typeA;

	public DamageType typeB;

	public Vector3 posA;

	public Vector3 posB;

	public Transform tParticleA;

	public Transform tParticleB;

	private Vector3[] positions;

	public ParticleSystem ps;

	public LineRenderer line;

	private GradientAlphaKey[] alphaKeys = new GradientAlphaKey[3];

	private GradientColorKey[] keys = new GradientColorKey[2];

	private Gradient gradient = new Gradient();

	public bool destroyed;

	public float timer;

	public float deactivation;

	private void Awake()
	{
		if ((bool)enemyA && (bool)enemyB)
		{
			ps.transform.SetParent(null);
			enemyA.linkedSouls = this;
			enemyB.linkedSouls = this;
			positions = new Vector3[9];
			PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
		}
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		typeA = null;
		typeB = null;
		deactivation = (timer = 0f);
		destroyed = false;
		RefreshGradient();
		base.gameObject.SetActive(value: true);
	}

	public void SetDamage(BaseEnemy e, DamageType type)
	{
		if (destroyed)
		{
			return;
		}
		if (e == enemyA)
		{
			if (typeA == null)
			{
				typeA = type;
			}
		}
		else if (typeB == null)
		{
			typeB = type;
		}
		timer = 2.5f;
		if (typeA != null && typeB != null)
		{
			destroyed = true;
			if (typeA != typeB)
			{
				destroyed = true;
			}
			else
			{
				typeA = null;
				typeB = null;
			}
		}
		RefreshGradient();
	}

	private void RefreshGradient()
	{
		alphaKeys[0].alpha = 0.5f;
		alphaKeys[0].time = 0f;
		alphaKeys[1].alpha = 1f;
		alphaKeys[1].time = 0.5f;
		alphaKeys[2].alpha = 0.5f;
		alphaKeys[2].time = 1f;
		keys[0].time = 0.25f;
		keys[0].color = (typeA ? Color.yellow : Color.white);
		keys[1].time = 0.75f;
		keys[1].color = (typeB ? Color.yellow : Color.white);
		gradient.SetKeys(keys, alphaKeys);
		line.colorGradient = gradient;
	}

	private void LateUpdate()
	{
		if (!enemyA || !enemyB)
		{
			return;
		}
		if (!destroyed)
		{
			if (timer != 0f)
			{
				timer = Mathf.MoveTowards(timer, 0f, Time.deltaTime);
				if (timer == 0f)
				{
					typeA = null;
					typeB = null;
					RefreshGradient();
				}
			}
		}
		else
		{
			deactivation = Mathf.MoveTowards(deactivation, 1f, Time.deltaTime * 2f);
			if (deactivation == 1f)
			{
				Mesh mesh = new Mesh();
				line.BakeMesh(mesh, useTransform: true);
				ParticleSystem.ShapeModule shape = ps.shape;
				shape.mesh = mesh;
				ps.Play();
				base.gameObject.SetActive(value: false);
				DamageData damageData = new DamageData();
				damageData.newType = Game.player.weapons.kickController.dmg_SlideKick;
				damageData.amount = 10f;
				damageData.knockdown = true;
				damageData.dir = posA.DirTo(posB) + Vector3.up;
				enemyA.Damage(damageData);
				damageData.dir = posB.DirTo(posA) + Vector3.up * 2f;
				enemyB.Damage(damageData);
				CameraController.shake.Shake(2);
				Game.time.SlowMotion(0.1f, 0.3f, 0.1f);
			}
		}
		Debug.DrawLine(enemyA.GetActualPosition(), enemyB.GetActualPosition());
		posA = enemyA.GetActualPosition() + Vector3.up / 2f;
		posB = enemyB.GetActualPosition() + Vector3.up / 2f;
		tParticleA.position = posA;
		tParticleB.position = posB;
		for (int i = 0; i < positions.Length; i++)
		{
			float num = (float)i / (float)(positions.Length - 1);
			positions[i] = Vector3.Lerp(posA, posB, num);
			positions[i].y -= Mathf.Sin(num * (float)Math.PI) / 2f;
		}
		line.SetPositions(positions);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.cyan;
		if ((bool)enemyA && (bool)enemyB)
		{
			Gizmos.DrawLine(enemyA.transform.position, enemyB.transform.position);
		}
		else if ((bool)enemyA)
		{
			Gizmos.DrawWireSphere(enemyA.transform.position, 2f);
		}
		else if ((bool)enemyB)
		{
			Gizmos.DrawWireSphere(enemyB.transform.position, 2f);
		}
	}
}
