using Newtonsoft.Json;

public class StoreProfileRequest
{
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
