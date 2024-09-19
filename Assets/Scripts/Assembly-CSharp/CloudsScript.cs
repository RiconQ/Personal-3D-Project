using UnityEngine;

public class CloudsScript : MonoBehaviour
{
	private void Update()
	{
		base.transform.Rotate(Vector3.forward * Time.deltaTime);
	}
}
