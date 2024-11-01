using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreUserresponse
{
    [JsonProperty("user_id")]
    public int UserID { get; set; }

    [JsonProperty("token")]
    public string  Token { get; set; }
}
