using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "UserConfig",
    menuName = "Config/User Config",
    order = 0
)]
[Serializable]
public class UserConfig : ScriptableObject
{
    [Header("API Keys")]
    public string openaiApiKey = "";
    public string sunoApiKey = "";
}