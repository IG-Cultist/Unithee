using Newtonsoft.Json;

public class StoreDeckRequest
{
    [JsonProperty("card1")]
    public int CardID_1 { get; set; }

    [JsonProperty("card2")]
    public int CardID_2 { get; set; }

    [JsonProperty("card3")]
    public int CardID_3 { get; set; }

    [JsonProperty("card4")]
    public int CardID_4 { get; set; }
}
