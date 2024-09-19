using System;
using UnityEngine;

internal class ProgressClass : IProgress<float>
{
	private float lastvalue;

	public void Report(float value)
	{
		if (!(lastvalue >= value))
		{
			lastvalue = value;
			Debug.Log(value);
		}
	}
}
