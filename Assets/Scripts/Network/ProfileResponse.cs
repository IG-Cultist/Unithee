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

    /// <summary>
    /// ディスプレイネームのプロパティ
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// アイコン名のプロパティ
    /// </summary>
    [JsonProperty("icon_name")]
    public string IconName { get; set; }
}
