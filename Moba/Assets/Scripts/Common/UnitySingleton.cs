using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FrameWork.Common
{
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
            if (_instance == null)
            {
                _instance = this as T;
            }
            else
            {
                Destroy(this.gameObject);
            }
        }
    }
}
