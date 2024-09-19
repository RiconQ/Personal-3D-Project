using System.Collections.Generic;
using UnityEngine;

namespace PathCreation.Examples
{
	public class ChainPathFollower : MonoBehaviour
	{
		public PathCreator pathCreator;

		public EndOfPathInstruction endOfPathInstruction;

		public float speed = 5f;

		public float distanceTravelled;

		public float offset = 1f;

		public GameObject prefab;

		public GameObject prefabB;

		public List<Transform> objects = new List<Transform>();

		public Vector3 startPos;

		public Quaternion startRot;

		private void Start()
		{
			objects.Clear();
			startPos = pathCreator.path.GetPointAtDistance(0f);
			startRot = pathCreator.path.GetRotationAtDistance(0f);
			base.transform.SetPositionAndRotation(startPos, startRot);
			objects.Add(base.transform);
			if (pathCreator != null)
			{
				pathCreator.pathUpdated += OnPathChanged;
			}
		}

		private void Update()
		{
			if (objects.Count > 0)
			{
				if (objects.Count < 80 && Vector3.Distance(objects[objects.Count - 1].position, startPos) > offset)
				{
					objects.Add(Object.Instantiate((objects.Count % 2 == 1) ? prefabB : prefab, startPos, startRot).transform);
				}
				for (int i = 1; i < objects.Count; i++)
				{
					objects[i].SetPositionAndRotation(pathCreator.path.GetPointAtDistance(distanceTravelled - (float)i * offset, endOfPathInstruction), pathCreator.path.GetRotationAtDistance(distanceTravelled - (float)i * offset, endOfPathInstruction));
				}
			}
			if (pathCreator != null)
			{
				distanceTravelled += speed * Time.deltaTime;
				base.transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
				base.transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
			}
		}

		private void OnPathChanged()
		{
			distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(base.transform.position);
		}
	}
}
