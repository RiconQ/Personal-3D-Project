using System;
using System.Collections.Generic;
using UnityEngine;

public class PortalsManager : MonoBehaviour
{
	public static PortalsManager instance;

	public List<PortalPoint> points = new List<PortalPoint>(4);

	public List<SimplePortal> portals = new List<SimplePortal>(4);

	private void Awake()
	{
		instance = this;
		Loading.OnLoadingStart = (Action)Delegate.Combine(Loading.OnLoadingStart, new Action(Reset));
	}

	private void OnDestroy()
	{
		Loading.OnLoadingStart = (Action)Delegate.Remove(Loading.OnLoadingStart, new Action(Reset));
	}

	public void AddPoint(PortalPoint newPoint)
	{
		int num = -1;
		for (int i = 0; i < portals.Count; i++)
		{
			if (portals[i].channel == newPoint.channel)
			{
				portals[i].Setup(newPoint);
				num = i;
				break;
			}
		}
		if (num < 0)
		{
			points.Add(newPoint);
		}
		else
		{
			portals.RemoveAt(num);
		}
	}

	public void AddPortal(SimplePortal newPortal)
	{
		int num = -1;
		for (int i = 0; i < points.Count; i++)
		{
			if (points[i].channel == newPortal.channel)
			{
				newPortal.Setup(points[i]);
				num = i;
				break;
			}
		}
		if (num < 0)
		{
			portals.Add(newPortal);
		}
		else
		{
			points.RemoveAt(num);
		}
	}

	private void Reset()
	{
		portals.Clear();
		points.Clear();
	}
}
