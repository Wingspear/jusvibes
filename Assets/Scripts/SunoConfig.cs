using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "SunoConfig",
    menuName = "Config/Suno Config",
    order = 0
)]
[Serializable]
public class SunoConfig : ScriptableObject
{
    [Header("Suno API Key, DONT PUSH")]
    public string sunoApiKey = "";
}