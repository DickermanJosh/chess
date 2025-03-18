using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> actions = new Queue<Action>();

    private static UnityMainThreadDispatcher _instance;
    public static UnityMainThreadDispatcher Instance => _instance;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(_instance);
            _instance = this;
        }

        DontDestroyOnLoad(gameObject);
    }

    public static void Enqueue(Action action)
    {
        lock (actions)
        {
            actions.Enqueue(action);
        }
    }

    void Update()
    {
        lock (actions)
        {
            while (actions.Count > 0)
            {
                Action a = actions.Dequeue();
                a?.Invoke();
            }
        }
    }
}
