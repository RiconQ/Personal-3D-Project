using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basic Script for SingleTone Pattern
/// </summary>
/// <typeparam name="T">Class's Type Parameter</typeparam>
public class SingleMonobase <T> : MonoBehaviour where T : SingleMonobase<T>
{
    public static T instance;

    protected virtual void Awake()
    {
        if (instance != null)
            Debug.LogError(this + "is already in this scene.");
        instance = (T)this;
    }

    protected virtual void OnDestroy()
    {
        Destroy();
    }

    public void Destroy()
    {
        instance = null;
    }
}
