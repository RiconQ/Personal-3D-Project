using UnityEngine;

public class SmokeTrail : MonoBehaviour
{
	private LineRenderer line;

	private Transform tr;

	private Vector3[] positions;

	private Vector3[] directions;

	private int i;

	private float timeSinceUpdate;

	private Material lineMaterial;

	private float lineSegment;

	private int currentNumberOfPoints = 2;

	private bool allPointsAdded;

	public int numberOfPoints = 10;

	public float updateSpeed = 0.25f;

	public float riseSpeed = 0.25f;

	public float spread = 0.2f;

	public BaseEnemy enemy;

	private Vector3 tempVec;

	private MaterialPropertyBlock block;

	private void Start()
	{
		tr = base.transform;
		tr.SetParent(null);
		line = GetComponent<LineRenderer>();
		line.useWorldSpace = true;
		block = new MaterialPropertyBlock();
		line.GetPropertyBlock(block);
		lineSegment = 1f / (float)numberOfPoints;
		positions = new Vector3[numberOfPoints];
		directions = new Vector3[numberOfPoints];
		line.SetVertexCount(currentNumberOfPoints);
		for (i = 0; i < currentNumberOfPoints; i++)
		{
			tempVec = getSmokeVec();
			directions[i] = tempVec;
			positions[i] = enemy.GetActualTrailPivotPosition();
			line.SetPosition(i, positions[i]);
		}
	}

	private void Update()
	{
		updateSpeed = Mathf.Lerp(updateSpeed, enemy.isActiveAndEnabled ? 0.04f : 0.02f, Time.deltaTime * 8f);
		riseSpeed = Mathf.Lerp(riseSpeed, enemy.isActiveAndEnabled ? 1.25f : 0.5f, Time.deltaTime * 4f);
		spread = Mathf.Lerp(spread, enemy.isActiveAndEnabled ? 0.75f : 1f, Time.deltaTime * 2f);
		if (!enemy.dead)
		{
			block.SetColor("_TintColor", Color.Lerp(Color.black, Color.white / 1.5f, Mathf.Clamp(enemy.GetHealthPercentage(), 0.5f, 1f)));
		}
		else
		{
			block.SetColor("_TintColor", Color.Lerp(Color.white / 3f, Color.black, enemy.body.deadTimer / 5f));
		}
		line.SetPropertyBlock(block);
		timeSinceUpdate += Time.deltaTime;
		if (timeSinceUpdate > updateSpeed)
		{
			timeSinceUpdate -= updateSpeed;
			if (!allPointsAdded)
			{
				currentNumberOfPoints++;
				line.SetVertexCount(currentNumberOfPoints);
				tempVec = getSmokeVec();
				directions[0] = tempVec;
				positions[0] = enemy.GetActualTrailPivotPosition();
				line.SetPosition(0, positions[0]);
			}
			if (!allPointsAdded && currentNumberOfPoints == numberOfPoints)
			{
				allPointsAdded = true;
			}
			for (i = currentNumberOfPoints - 1; i > 0; i--)
			{
				tempVec = positions[i - 1];
				positions[i] = tempVec;
				tempVec = directions[i - 1];
				directions[i] = tempVec;
			}
			tempVec = getSmokeVec();
			directions[0] = tempVec;
		}
		for (i = 1; i < currentNumberOfPoints; i++)
		{
			tempVec = positions[i];
			tempVec += directions[i] * Time.deltaTime;
			positions[i] = tempVec;
			line.SetPosition(i, positions[i]);
		}
		positions[0] = enemy.GetActualTrailPivotPosition();
		line.SetPosition(0, enemy.GetActualTrailPivotPosition());
		_ = allPointsAdded;
	}

	private Vector3 getSmokeVec()
	{
		Vector3 result = default(Vector3);
		result.x = Random.Range(-1f, 1f);
		result.y = Random.Range(0f, 1f);
		result.z = Random.Range(-1f, 1f);
		result.Normalize();
		result -= enemy.GetActualTrailPivotTransform().up;
		result *= spread;
		result.y += riseSpeed;
		return result;
	}
}
