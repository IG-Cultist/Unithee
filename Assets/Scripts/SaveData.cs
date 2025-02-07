using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveData
{
    public string Name { get; set; }
    public int UserID { get; set; }
    public List<int> StageList { get; set; }
    public string Token { get; set; }
    public string IconName { get; set; }
    public string DisplayName { get; set; }
}
