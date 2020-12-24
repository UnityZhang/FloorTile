using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
{
    protected static T _classInstance;

    public static T instance
    {
        get
        {
            if (_classInstance == null)
            {
                _classInstance = FindObjectOfType<T>();
                if (_classInstance.IsNull())
                {
                    GameObject go = new GameObject("SingletonMonoBehaviour");
                    _classInstance = go.AddComponent<T>();
                }
            }
            return _classInstance;
        }
    }

    protected virtual void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        if (_classInstance == null)
        {
            _classInstance = this as T;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

/// <summary>
/// 非MonoBehaviour单例
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class Singleton<T> where T : new()
{
    private static T _instance;
    static object _lock = new object();
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new T();
                }
            }
            return _instance;
        }
    }
}
