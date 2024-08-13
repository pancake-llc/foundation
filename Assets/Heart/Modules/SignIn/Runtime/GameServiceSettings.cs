using UnityEngine;

namespace Pancake.SignIn
{
    [EditorIcon("so_blue_setting")]
    public sealed class GameServiceSettings : ScriptableSettings<GameServiceSettings>
    {
        [SerializeField] private bool enableAutoBackup;
        [SerializeField] private bool byTime;
        [SerializeField] private bool byApplicationQuit;
        [SerializeField] private float backupTimeInterval = 300;

        public static bool EnableAutoBackup => Instance.enableAutoBackup;
        public static bool ByTime => Instance.byTime;
        public static bool ByApplicationQuit => Instance.byApplicationQuit;
        public static float BackupTimeInterval => Instance.backupTimeInterval;
    }
}