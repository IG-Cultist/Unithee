using Newtonsoft.Json;

public class RivalResponse 
{
    /// <summary>
    /// ユーザIDのプロパティ
    /// </summary>
    [JsonProperty("user_id")]
    public int UserID { get; set; }

    /// <summary>
    /// カードIDのプロパティ
    /// </summary>
    [JsonProperty("card_id")]
    public int CardID { get; set; }
}
