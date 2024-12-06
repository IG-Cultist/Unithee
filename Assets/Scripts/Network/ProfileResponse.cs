using Newtonsoft.Json;


public class ProfileResponse
{
    /// <summary>
    /// ユーザIDのプロパティ
    /// </summary>
    [JsonProperty("user_id")]
    public int UserID { get; set; }

    /// <summary>
    /// ポイントプロパティ
    /// </summary>
    [JsonProperty("point")]
    public int Point { get; set; }
}
