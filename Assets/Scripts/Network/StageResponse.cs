using Newtonsoft.Json;

public class StageResponse
{
    /// <summary>
    /// ステージIDのプロパティ
    /// </summary>
    [JsonProperty("id")]
    public int StageID { get; set; }

    /// <summary>
    /// ステージクリア判定のプロパティ
    /// </summary>
    [JsonProperty("clear")]
    public int Clear { get; set; }

    /// <summary>
    /// ステージ完全クリア判定のプロパティ
    /// </summary>
    [JsonProperty("perfect")]
    public int Perfect { get; set; }
}
