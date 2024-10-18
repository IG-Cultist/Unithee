using Newtonsoft.Json;


public class StoreUserRequest
{
    [JsonProperty("name")]
    public string Name { get; set; }
}
