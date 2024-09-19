using UnityEngine;

public class PooledMonobehaviour : MonoBehaviour
{
	private int poolIndex = -1;

	public int maxCount = 10;

	private bool initialEnable = true;

	public Transform t { get; private set; }

	public Rigidbody rb { get; private set; }

	protected virtual void Awake()
	{
		t = base.transform;
		rb = GetComponentInChildren<Rigidbody>();
	}

	private void OnEnable()
	{
		if (initialEnable)
		{
			initialEnable = false;
		}
		else
		{
			OnActualEnable();
		}
	}

	protected virtual void OnActualEnable()
	{
	}

	protected virtual void OnDisable()
	{
		if (poolIndex != -1)
		{
			QuickPool.instance.ReturnToPool(poolIndex, this);
		}
	}

	public void SetPoolIndex(int i)
	{
		poolIndex = i;
	}
}
