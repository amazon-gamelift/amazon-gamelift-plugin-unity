using Amazon.GameLift;

namespace AmazonGameLift.Editor
{
    public class ManagedEC2FleetParameters
    {
        public string GameName;
        public string FleetName;
        public string BuildName;
        public string LaunchParameters;
        public OperatingSystem OperatingSystem;
        public string GameServerFolder;
        public string GameServerFile;
    }
}