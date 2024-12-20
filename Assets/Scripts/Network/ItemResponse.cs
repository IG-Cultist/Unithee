using Newtonsoft.Json;

public class ItemResponse 
{
    /// <summary>
    /// アイテムIDのプロパティ
    /// </summary>
    [JsonProperty("id")]
    public int ItemID { get; set; }

    /// <summary>
    /// アイテム名のプロパティ
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// アイテム効果値のプロパティ
    /// </summary>
    [JsonProperty("effect")]
    public int Effect { get; set; }

    /// <summary>
    /// 適応アイテム名のプロパティ
    /// </summary>
    [JsonProperty("bestItem_name")]
    public string BestItemName { get; set; }

    /// <summary>
    /// アイテム説明のプロパティ
    /// </summary>
    [JsonProperty("explain")]
    public string Explain { get; set; }
}
