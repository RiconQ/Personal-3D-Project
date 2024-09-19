public class PooledTrail : PooledMonobehaviour
{
	public TrailScript trail;

	protected override void Awake()
	{
		base.Awake();
		trail = GetComponentInChildren<TrailScript>();
	}

	protected override void OnActualEnable()
	{
		base.OnActualEnable();
		trail.Play();
	}
}
