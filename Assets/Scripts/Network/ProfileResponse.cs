using Newtonsoft.Json;


public class ProfileResponse
{
    /// <summary>
    /// ユーザIDのプロパティ
    /// </summary>
    [JsonProperty("user_id")]
    public int UserID { get; set; }
}
