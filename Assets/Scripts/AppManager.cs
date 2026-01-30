using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class AppManager : MonoBehaviour
{
    private static AppManager m_instance;

    private void OnEnable()
    {
        if (m_instance != null)
        {
            throw new SystemException("AppManager already exists");
        }

        m_instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
