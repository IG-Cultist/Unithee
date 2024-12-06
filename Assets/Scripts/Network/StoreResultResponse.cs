using Newtonsoft.Json;

public class StoreResultResponse
{
    [JsonProperty("judge")]
    public int Judge { get; set; }

    [JsonProperty("battle_user_id")]
    public int RivalID { get; set; }
}
