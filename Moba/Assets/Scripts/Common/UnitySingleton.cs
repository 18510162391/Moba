﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitySingleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<T>();
                if (_instance == null)
                {
                    GameObject go = new GameObject();
                    go.hideFlags = HideFlags.HideAndDontSave;
                    _instance = go.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    void OnAwake()
    {
        DontDestroyOnLoad(this.gameObject);
        //if (_instance == null)
        //{
        //    _instance = this as T;
        //}
        //else
        //{
        //    Destroy(this.gameObject);
        //}
    }
}
