using Newtonsoft.Json;

namespace DiscordTestBot
{
    public struct ConfigJson
    {
        [JsonProperty("token")]
        public string token { get; private set; }
        [JsonProperty("prefix")]
        public string prefix { get; private set; }

    }
}
