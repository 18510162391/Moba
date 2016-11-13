using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FrameWork.Common;

namespace FrameWork.Resource
{
    class ResourceUpdateManager : UnitySingleton<ResourceUpdateManager>
    {
        void OnAwake()
        {
        }

        void OnStart()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR || SKIP_SDK
            OnUpdateComplete();
#endif
        }
        private void OnUpdateComplete()
        {

        }
    }
}
