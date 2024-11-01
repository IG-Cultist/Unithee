using Newtonsoft.Json;

public class DeckResponse
{
    /// <summary>
    /// デッキカードIDのプロパティ
    /// </summary>
    [JsonProperty("card_id")]
    public int CardID { get; set; }
}
