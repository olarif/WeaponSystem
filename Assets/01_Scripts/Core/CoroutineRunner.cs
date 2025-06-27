using System.Collections;
using UnityEngine;

/// <summary>
/// Singleton MonoBehaviour for starting and stopping coroutines from anywhere.
/// </summary>
public class CoroutineRunner : MonoBehaviour
{
    private static CoroutineRunner _instance;
    public  static CoroutineRunner Instance
    {
        get
        {
            if (_instance == null)
            {
                //Create new GameObject with this component
                var go = new GameObject("CoroutineRunner");
                _instance = go.AddComponent<CoroutineRunner>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject); //keep instance alive across scenes
    }
    
    //Start the coroutine
    public Coroutine StartRoutine(IEnumerator coroutine)
    {
        return StartCoroutine(coroutine);
    }
    
    //Stop the coroutine
    public void StopRoutine(Coroutine coroutine)
    {
        if (coroutine != null)
            StopCoroutine(coroutine);
    }
}