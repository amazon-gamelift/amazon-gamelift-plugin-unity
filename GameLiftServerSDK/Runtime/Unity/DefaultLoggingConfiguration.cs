#if (UNITY_EDITOR || UNITY_SERVER)
using log4net.Config;
using UnityEngine;

namespace Aws.GameLift.Unity
{
    public class DefaultLoggingConfiguration
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Configure()
        {
            if (!log4net.LogManager.GetRepository().Configured)
            {
                // Configure log4net to support the default console output
                BasicConfigurator.Configure();
            }
        }
    }
}
#endif
