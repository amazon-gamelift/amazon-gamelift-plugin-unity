#if (UNITY_EDITOR || UNITY_SERVER)
using log4net.Config;
using UnityEngine;
using System.IO;

namespace Aws.GameLift.Unity
{
    public class LoggingConfiguration
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Configure()
        {
            // Configure log4net with the provided xml file that supports console and file output
            // Note: this overrides the basic configuration that executes first on "AfterAssembliesLoaded"
            var configPathname = $"{Application.dataPath}/Samples/Amazon GameLift Server SDK/5.0.0/Log4Net Config/log4net.config";
            XmlConfigurator.Configure(new FileInfo(configPathname));
        }
    }
}
#endif
