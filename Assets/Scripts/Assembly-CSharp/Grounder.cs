using System;
using System.Collections.Generic;
using UnityEngine;

public class Grounder : MonoBehaviour
{
	public Action OnGrounded = delegate
	{
	};

	public Action OnFakeGrounded = delegate
	{
	};

	public Action OnUngrounded = delegate
	{
	};

	public Action OnColliderChanged = delegate
	{
	};

	public float maxGroundAngle = 40f;

	public float maxGapFixAngle = 10f;

	public LayerMask groundMask;

	private int groundContactCount;

	private int stepSinceLastUngrounded;

	private float minGroundDotProduct;

	private float minGapFixDotProduct;

	private float delay = 3f;

	private float yLastBodyPosition;

	private Vector3 dir;

	private Vector3 project;

	private Vector3 tempGroundNormal;

	private List<Vector3> gPoints = new List<Vector3>(5);

	private Transform t;

	private Rigidbody rb;

	private RaycastHit hit;

	private ContactPoint cp;

	private ContactPoint[] contactPoints = new ContactPoint[4];

	public bool onSlope { get; private set; }

	public bool fakeGrounded { get; private set; }

	public bool grounded { get; private set; }

	public float jumpHeight { get; private set; }

	public Vector3 gNormal { get; private set; }

	public Vector3 wNormal { get; private set; }

	public Vector3 gPoint { get; private set; }

	public Collider gCollider { get; private set; }

	public float maxFallVelocity { get; private set; }

	public float highestPoint { get; private set; }

	private void Awake()
	{
		t = base.transform;
		rb = GetComponent<Rigidbody>();
		gNormal = Vector3.up;
		highestPoint = t.position.y;
		jumpHeight = 0f;
		OnValidate();
	}

	private void OnEnable()
	{
		highestPoint = t.position.y;
		jumpHeight = 0f;
	}

	private void OnValidate()
	{
		minGroundDotProduct = Mathf.Cos(maxGroundAngle * ((float)Math.PI / 180f));
		minGapFixDotProduct = Mathf.Cos(maxGapFixAngle * ((float)Math.PI / 180f));
	}

	public void Reset()
	{
		highestPoint = t.position.y;
		jumpHeight = 0f;
		maxFallVelocity = 0f;
	}

	public void Grounded(bool forced = false)
	{
		grounded = true;
		if (!forced)
		{
			Debug.DrawRay(t.position, Vector3.up, Color.yellow, 4f);
			if (OnGrounded != null)
			{
				OnGrounded();
			}
		}
	}

	public void Ungrounded(bool forced = false)
	{
		delay = 5f;
		if (grounded)
		{
			gNormal = Vector3.up;
			grounded = false;
			fakeGrounded = false;
			highestPoint = t.position.y;
			maxFallVelocity = 0f;
			groundContactCount = 0;
			tempGroundNormal = Vector3.zero;
			if (!forced && OnUngrounded != null)
			{
				OnUngrounded();
			}
		}
	}

	public bool CheckWithRaycast(float dot = 0f)
	{
		if (dot == 0f)
		{
			return Physics.Raycast(t.position, Vector3.down, out hit, 1.2f + (1f - gNormal.y), groundMask);
		}
		Physics.Raycast(t.position, Vector3.down, out hit, 1.2f + (1f - gNormal.y), groundMask);
		return hit.normal.y > dot;
	}

	private void Update()
	{
		if (grounded)
		{
			return;
		}
		if (rb.velocity.y < maxFallVelocity)
		{
			maxFallVelocity = rb.velocity.y;
		}
		if (!fakeGrounded && groundContactCount > 0)
		{
			fakeGrounded = true;
			if (OnFakeGrounded != null)
			{
				OnFakeGrounded();
			}
		}
		if (rb.isKinematic)
		{
			highestPoint = t.position.y;
		}
		else if (t.position.y > highestPoint)
		{
			highestPoint = t.position.y;
		}
		jumpHeight = highestPoint - t.position.y;
		Debug.DrawRay(t.position.With(null, highestPoint), Vector3.up);
	}

	private void FixedUpdate()
	{
		if (delay > 0f)
		{
			delay -= 1f;
			ClearState();
		}
		else
		{
			UpdateState();
			Debug.DrawRay(gPoint, gNormal * 0.5f, Color.yellow);
			ClearState();
		}
	}

	private void UpdateState()
	{
		if (wNormal.sqrMagnitude > 0f)
		{
			wNormal.Normalize();
			float num = wNormal.y.InDegrees();
			onSlope = num >= maxGroundAngle && num < 80f;
		}
		else
		{
			onSlope = false;
		}
		if (groundContactCount > 0)
		{
			if (groundContactCount > 1)
			{
				tempGroundNormal.Normalize();
				gPoint /= (float)groundContactCount;
			}
			if (gNormal != tempGroundNormal)
			{
				float num2 = 0f;
				float num3 = 0f;
				for (int i = 1; i < gPoints.Count; i++)
				{
					num2 = Vector3.Distance(gPoints[i], gPoints[i - 1]);
					if (num3 < num2)
					{
						num3 = num2;
					}
				}
				if (num3 < 0.125f)
				{
					CheckWithRaycast(minGapFixDotProduct);
					gNormal = ((hit.distance == 0f) ? tempGroundNormal : hit.normal);
				}
				else
				{
					gNormal = tempGroundNormal;
				}
			}
			if (!grounded)
			{
				Debug.DrawRay(t.position, Vector3.up, Color.green, 4f);
				grounded = true;
				stepSinceLastUngrounded = 0;
				if (OnGrounded != null)
				{
					OnGrounded();
				}
				if (!fakeGrounded)
				{
					fakeGrounded = true;
					if (OnFakeGrounded != null)
					{
						OnFakeGrounded();
					}
				}
			}
		}
		else
		{
			if (stepSinceLastUngrounded < 3 && CheckWithRaycast(minGapFixDotProduct))
			{
				if (!grounded)
				{
					grounded = true;
					stepSinceLastUngrounded = 0;
					Debug.DrawRay(t.position, Vector3.up, Color.magenta, 4f);
					if (OnGrounded != null)
					{
						OnGrounded();
					}
					if (!fakeGrounded)
					{
						fakeGrounded = true;
						if (OnFakeGrounded != null)
						{
							OnFakeGrounded();
						}
					}
				}
				tempGroundNormal = hit.normal;
				project = Vector3.ProjectOnPlane(rb.velocity, hit.normal).normalized;
				if (!rb.isKinematic && rb.velocity.normalized.y > project.y)
				{
					rb.velocity = project * rb.velocity.magnitude;
					Debug.DrawLine(hit.point, rb.position, Color.red, 2f);
				}
			}
			else
			{
				if (grounded)
				{
					grounded = false;
					fakeGrounded = false;
					highestPoint = t.position.y;
					maxFallVelocity = 0f;
					if (OnUngrounded != null)
					{
						OnUngrounded();
					}
				}
				tempGroundNormal = Vector3.up;
			}
			if (gNormal != tempGroundNormal)
			{
				gNormal = tempGroundNormal;
			}
		}
		if (!grounded)
		{
			stepSinceLastUngrounded++;
		}
	}

	private void ClearState()
	{
		groundContactCount = 0;
		tempGroundNormal.x = (tempGroundNormal.y = (tempGroundNormal.z = 0f));
		wNormal = Vector3.zero;
		gPoint = Vector3.zero;
		gPoints.Clear();
	}

	private void HandleCollision(Collision c)
	{
		if (delay > 0f || rb.isKinematic)
		{
			return;
		}
		for (int i = 0; i < c.contactCount; i++)
		{
			cp = c.GetContact(i);
			if (cp.normal.y > minGroundDotProduct)
			{
				if (gCollider != c.collider)
				{
					gCollider = c.collider;
					if (OnColliderChanged != null)
					{
						OnColliderChanged();
					}
				}
				tempGroundNormal += cp.normal;
				gPoint += cp.point;
				gPoints.Add(cp.point);
				groundContactCount++;
				Debug.DrawRay(cp.point, cp.normal * 0.2f, Color.green);
			}
			else
			{
				wNormal += cp.normal;
				Debug.DrawRay(cp.point, cp.normal * 0.2f, Color.red);
			}
		}
	}

	private void OnCollisionEnter(Collision c)
	{
		if ((int)groundMask == ((int)groundMask | (1 << c.gameObject.layer)))
		{
			HandleCollision(c);
		}
	}

	private void OnCollisionStay(Collision c)
	{
		if ((int)groundMask == ((int)groundMask | (1 << c.gameObject.layer)))
		{
			HandleCollision(c);
		}
	}
}
