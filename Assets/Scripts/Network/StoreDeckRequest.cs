using Newtonsoft.Json;

public class StoreDeckRequest
{
    [JsonProperty("card_id")]
    public int CardID { get; set; }
}
