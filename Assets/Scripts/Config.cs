using UnityEngine;

public class Config : Singleton<Config>
{
    [SerializeField] private UserConfig userConfig;
    public UserConfig UserConfig => userConfig;
}
