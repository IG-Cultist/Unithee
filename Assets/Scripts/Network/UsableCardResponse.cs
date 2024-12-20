using Newtonsoft.Json;

public class UsableCardResponse
{
    /// <summary>
    /// 使用可能カードIDのプロパティ
    /// </summary>
    [JsonProperty("id")]
    public int CardID { get; set; }

    /// <summary>
    /// 使用可能カード名のプロパティ
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// 使用可能カード枚数のプロパティ
    /// </summary>
    [JsonProperty("stack")]
    public string Stack { get; set; }

    /// <summary>
    /// 使用可能カードの種別のプロパティ
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; }
}
