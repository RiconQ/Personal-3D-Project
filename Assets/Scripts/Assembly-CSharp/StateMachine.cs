using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
	private Dictionary<Type, BaseState> states;

	public BaseState current { get; private set; }

	public event Action<BaseState> OnStateChanged;

	public void SetStates(Dictionary<Type, BaseState> states)
	{
		this.states = states;
	}

	public void SwitchState(Type nextState)
	{
		if (current != null)
		{
			current.LastCall();
		}
		current = states[nextState];
		current.FirstCall();
		this.OnStateChanged?.Invoke(current);
	}

	public bool CurrentIs(Type state)
	{
		return current.GetType() == state;
	}

	private void Update()
	{
		if (current == null)
		{
			current = Enumerable.First(states.Values);
		}
		Type type = current?.Tick();
		if (type != null && type != current?.GetType())
		{
			SwitchState(type);
		}
	}
}
