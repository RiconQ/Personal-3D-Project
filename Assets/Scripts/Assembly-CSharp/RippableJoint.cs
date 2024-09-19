using System;
using UnityEngine;

[Serializable]
public class RippableJoint
{
	public int index;

	public Joint joint;

	public Vector3 localPos;

	public Rigidbody connectedBody;

	public Rigidbody freeBody;
}
