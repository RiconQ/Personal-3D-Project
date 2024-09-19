using System;
using UnityEngine;

public class DaggerController : MonoBehaviour
{
	public static Action OnDaggerThrow = delegate
	{
	};

	public Animator animator;

	public GameObject daggerPrefab;

	public LineRenderer line;

	public ParticleSystem chainDustTrail;

	public Transform t;

	public Transform tPivot;

	public AnimationCurve pullCurve;

	public AnimationCurve widthCurve;

	[Header("DamageTypes")]
	public DamageType dmg_Pull;

	public DamageType dmg_Slam;

	public DamageType dmg_Pound;

	[Header("Audio")]
	public AudioSource source;

	public AudioClip sfxThrow;

	public AudioClip sfxPull;

	public AudioClip sfxReturn;

	public AudioClip sfxLaunch;

	private PlayerWeapons manager;

	private PooledMonobehaviour pooledDagger;

	private float dist;

	private float amp;

	private float timer;

	private Vector3 offset;

	private Vector3[] chainPositions = new Vector3[24];

	private Vector3 posA;

	private Vector3 posB;

	public ThrowedDagger dagger { get; private set; }

	public int state { get; private set; }

	public float holding { get; private set; }

	public bool isCooling => cooldown > 0f;

	public float cooldown { get; private set; }

	private void Awake()
	{
		t = base.transform;
		tPivot = t.Find("Pivot");
		line.positionCount = chainPositions.Length;
		line.enabled = false;
		chainDustTrail.transform.SetParent(null);
		manager = GetComponentInParent<PlayerWeapons>();
		dagger = UnityEngine.Object.Instantiate(daggerPrefab, Vector3.zero, Quaternion.identity).GetComponent<ThrowedDagger>();
		dagger.gameObject.SetActive(value: false);
		BaseEnemy.OnAnyEnemyDie = (Action)Delegate.Combine(BaseEnemy.OnAnyEnemyDie, new Action(Restore));
		PlayerController.OnParkourMove = (Action)Delegate.Combine(PlayerController.OnParkourMove, new Action(Restore));
		PlayerSlide.OnSlideBash = (Action)Delegate.Combine(PlayerSlide.OnSlideBash, new Action(Restore));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		BaseEnemy.OnAnyEnemyDie = (Action)Delegate.Remove(BaseEnemy.OnAnyEnemyDie, new Action(Restore));
		PlayerController.OnParkourMove = (Action)Delegate.Remove(PlayerController.OnParkourMove, new Action(Restore));
		PlayerSlide.OnSlideBash = (Action)Delegate.Remove(PlayerSlide.OnSlideBash, new Action(Restore));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Restore()
	{
		if (state == 0)
		{
			cooldown = 0f;
			Reset();
		}
	}

	private void Reset()
	{
		line.enabled = false;
		float num2 = (cooldown = 0f);
		timer = num2;
		state = 0;
		if ((bool)dagger && dagger.CheckState())
		{
			dagger.Reset();
		}
	}

	private void OnDisable()
	{
		cooldown = 0f;
		Reset();
	}

	private void StopDagger()
	{
		line.enabled = false;
		dagger.Reset();
		state = 0;
	}

	public void ThrowDagger(BaseBreakable target = null)
	{
		dagger.Activate(tPivot);
		animator.SetTrigger("Attack");
		source.PlayClip(sfxThrow, 0.5f);
		holding = 0f;
		state = 1;
		Game.player.sway.Sway(-2f, 0f, 3f, 2.5f);
		if (OnDaggerThrow != null)
		{
			OnDaggerThrow();
		}
	}

	public void Sway()
	{
		Game.player.sway.Sway(0f, 0f, -5f, 3f);
	}

	public void Tick()
	{
		switch (state)
		{
		case 0:
			if (cooldown != 0f)
			{
				cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime * (float)((!manager.p.slide.isSliding) ? 1 : 8));
			}
			else if (Game.player.AltPressed() && !Game.player.AttackHolded())
			{
				ThrowDagger();
			}
			break;
		case 1:
			if (!dagger.CheckState())
			{
				animator.SetTrigger("Catch");
				source.PlayClip(sfxReturn);
				StopDagger();
				break;
			}
			if (!dagger.hoockedClldr)
			{
				posA = tPivot.position;
				posB = dagger.tChainPivot.position;
				offset = (t.up + t.right).normalized;
				for (int j = 0; j < chainPositions.Length; j++)
				{
					dist = (float)j / (float)(chainPositions.Length - 1);
					chainPositions[j] = Vector3.Lerp(posA, posB, dist);
					chainPositions[j] += offset * Mathf.Sin(dist * (float)Math.PI * 2f) * Mathf.Sin(Time.time * 8f);
				}
				line.widthMultiplier = 0.25f;
				line.SetPositions(chainPositions);
				if (!line.enabled)
				{
					line.enabled = true;
				}
			}
			else
			{
				if (line.enabled)
				{
					line.enabled = false;
				}
				if (holding == 1f)
				{
					chainDustTrail.transform.SetPositionAndRotation(tPivot.position, Quaternion.LookRotation(tPivot.position.DirTo(dagger.t.position)));
					chainDustTrail.Play();
					StopDagger();
					break;
				}
				dagger.AlignChainMesh(tPivot);
				holding = Mathf.MoveTowards(holding, 1f, Time.deltaTime * 0.5f);
			}
			if ((!Game.player.AltHolded() || Game.player.SlidePressed()) && (bool)dagger.hoockedClldr)
			{
				cooldown = (((bool)dagger.targetEnemy & manager.p.grounder.grounded) ? 1 : 0);
				QuickEffectsPool.Get("Poof", dagger.t.position, dagger.t.rotation).Play();
				source.PlayClip(sfxPull);
				dagger.Pull();
				state++;
				timer = 0f;
			}
			break;
		case 2:
		{
			if (!dagger.CheckState())
			{
				animator.SetTrigger("Catch");
				source.PlayClip(sfxReturn);
				Reset();
				break;
			}
			timer = Mathf.MoveTowards(timer, 0.25f, Time.deltaTime);
			dagger.UpdateTargetPos();
			posA = tPivot.position;
			posB = dagger.targetPos;
			offset = (t.up - t.right).normalized;
			amp = timer / 0.25f;
			for (int i = 0; i < chainPositions.Length; i++)
			{
				dist = (float)i / (float)(chainPositions.Length - 1);
				chainPositions[i] = Vector3.Lerp(posA, posB, dist);
				chainPositions[i] += offset * Mathf.LerpUnclamped(2f * Mathf.Sin(dist * (float)Math.PI), 0f, pullCurve.Evaluate(amp));
			}
			line.widthMultiplier = Mathf.Sin(timer / 0.25f * (float)Math.PI) * 0.35f;
			line.SetPositions(chainPositions);
			if (!line.enabled)
			{
				line.enabled = true;
			}
			if (timer == 0.25f)
			{
				line.enabled = false;
				state = 0;
				dagger.Reset();
				CameraController.shake.Shake(1);
			}
			break;
		}
		}
	}
}
