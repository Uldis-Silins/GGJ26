using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class AppManager : MonoBehaviour
{
    private static AppManager s_instance;

    private void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private void OnEnable()
    {
        if (s_instance != null)
        {
            throw new SystemException("AppManager already exists");
        }

        s_instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
