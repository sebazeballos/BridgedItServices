using System;
namespace BridgetItService.Settings
{
	public class AWSConfig
	{
        public string LogGroup { get; set; }
        public bool IncludeLogLevel { get; set; }
        public bool IncludeCategory { get; set; }
        public bool IncludeNewline { get; set; }
        public bool IncludeException { get; set; }
        public bool IncludeEventId { get; set; }
        public bool IncludeScopes { get; set; }
        public AWSCredentialsConfig AwsCredentials { get; set; }

        public AWSConfig(IConfigurationSection section)
        {
            section.Bind(this);
        }
    }

    public class LogLevelConfig
    {
        public string Default { get; set; }
        public string System { get; set; }
        public string Microsoft { get; set; }
        public string AWS { get; set; }
    }

    public class AWSCredentialsConfig
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
    }
}

