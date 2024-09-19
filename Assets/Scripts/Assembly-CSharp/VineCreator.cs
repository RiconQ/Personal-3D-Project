using System.Collections.Generic;
using UnityEngine;

public class VineCreator : MonoBehaviour
{
	public List<VinePoint> newPoints = new List<VinePoint>();

	public List<Vector3> splinePoints = new List<Vector3>();

	public MeshFilter filter;

	public MeshRenderer rend;

	public AnimationCurve widthCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private Vector3[] profile = new Vector3[4]
	{
		new Vector3(0f, 1f, 0f),
		new Vector3(1f, 0f, 0f),
		new Vector3(0f, -1f, 0f),
		new Vector3(-1f, 0f, 0f)
	};

	public float detalisation = 1f;

	public void CreateSpline()
	{
		splinePoints.Clear();
		for (int i = 0; i < newPoints.Count; i++)
		{
			if (i == 0)
			{
				newPoints[i].tangent = newPoints[i].normal;
			}
			else if (i == newPoints.Count - 1)
			{
				newPoints[i].tangent = newPoints[i - 1].point.DirTo(newPoints[i].point);
			}
			else
			{
				newPoints[i].tangent = (newPoints[i - 1].point.DirTo(newPoints[i].point) + newPoints[i].point.DirTo(newPoints[i + 1].point)).normalized;
			}
		}
		for (int j = 0; j < newPoints.Count - 1; j++)
		{
			float magnitude = (newPoints[j].point - newPoints[j + 1].point).magnitude;
			newPoints[j].desnity = Mathf.Clamp(Mathf.RoundToInt(magnitude * detalisation), 2, 24);
			for (int k = 0; k < newPoints[j].desnity; k++)
			{
				splinePoints.Add(MegaHelp.CalculateCubicBezierPoint((float)k / (float)newPoints[j].desnity, newPoints[j].point, newPoints[j].point + newPoints[j].tangent * magnitude / 3f, newPoints[j + 1].point - newPoints[j + 1].tangent * magnitude / 3f, newPoints[j + 1].point));
			}
		}
	}

	public void CreateMesh()
	{
		profile = new Vector3[6];
		Vector3 up = Vector3.up;
		for (int i = 0; i < 6; i++)
		{
			profile[i] = Quaternion.Euler(0f, 0f, 60 * i) * up;
		}
		List<Vector3> list = new List<Vector3>();
		List<int> list2 = new List<int>();
		new List<Vector2>();
		for (int j = 0; j < newPoints.Count; j++)
		{
			if (j < newPoints.Count - 1)
			{
				newPoints[j].rotation = Quaternion.LookRotation(newPoints[j].point.DirTo(newPoints[j + 1].point), newPoints[j].normal);
			}
			else
			{
				newPoints[j].rotation = Quaternion.LookRotation(newPoints[j - 1].point.DirTo(newPoints[j].point), newPoints[j].normal);
			}
			for (int k = 0; k < 6; k++)
			{
				Vector3 item = newPoints[j].point - base.transform.position + newPoints[j].rotation * profile[k];
				list.Add(item);
			}
			if (j < newPoints.Count - 1)
			{
				for (int l = 0; l < profile.Length; l++)
				{
					int num = j * profile.Length + l;
					int item2 = ((l == profile.Length - 1) ? (j * profile.Length) : (num + 1));
					int num2 = num + profile.Length;
					int item3 = ((l == profile.Length - 1) ? (j * profile.Length + profile.Length) : (num2 + 1));
					list2.Add(num);
					list2.Add(num2);
					list2.Add(item2);
					list2.Add(item2);
					list2.Add(num2);
					list2.Add(item3);
				}
			}
		}
		Mesh mesh = new Mesh();
		mesh.SetVertices(list);
		mesh.SetTriangles(list2, 0);
		mesh.RecalculateNormals();
		filter.sharedMesh = mesh;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		if (splinePoints.Count > 1)
		{
			for (int i = 0; i < splinePoints.Count - 1; i++)
			{
				Gizmos.DrawLine(splinePoints[i], splinePoints[i + 1]);
			}
		}
		if (newPoints.Count <= 0)
		{
			return;
		}
		for (int j = 0; j < newPoints.Count; j++)
		{
			Gizmos.color = Color.white;
			Gizmos.DrawRay(newPoints[j].point, newPoints[j].tangent);
			if (j > 0)
			{
				Gizmos.DrawLine(newPoints[j].point, newPoints[j - 1].point);
			}
			Gizmos.color = Color.red;
			Gizmos.DrawRay(newPoints[j].point, newPoints[j].normal);
		}
	}
}
