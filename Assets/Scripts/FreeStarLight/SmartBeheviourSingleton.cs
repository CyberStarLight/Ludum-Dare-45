using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SmartBeheviourSingleton<T> : SmartBeheviour where T : SmartBeheviour
{
    public static T Instance;

    private T mySingletonInstance;

    //use this to make the inheriting class return itself so we can set it to the static "Instance" field;
    protected abstract T GetSingletonInstance();
    protected abstract bool IsDestroyedOnLoad();
    protected abstract bool OverridePreviousSingletons();

    public override void Awake()
    {
        base.Awake();

        if (Instance != null)
        {
            if(OverridePreviousSingletons())
            {
                Destroy(Instance.gameObject);
                Instance = GetSingletonInstance();

                if(!IsDestroyedOnLoad() && Application.isPlaying)
                    DontDestroyOnLoad(gameObject);
            }
            else if(Instance.gameObject != gameObject)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Instance = GetSingletonInstance();
            if (!IsDestroyedOnLoad() && Application.isPlaying)
                DontDestroyOnLoad(gameObject);
        }
    }
    
    public override void OnDestroy()
    {
        base.OnDestroy();

        if(this == Instance)
            Instance = null;
    }
}
