public class BreakablePot : BreakableB
{
	public int type;

	public PotProps[] types;

	public override void Awake()
	{
		base.Awake();
		Setup();
	}

	public void Setup()
	{
		mat.SetColorByName("_EmissionColor", types[type].color);
		_prefabOnBreak = types[type].prefab;
	}
}
