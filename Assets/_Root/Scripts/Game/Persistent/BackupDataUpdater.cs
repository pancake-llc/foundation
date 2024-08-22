using Pancake.Common;
using Pancake.SignIn;

namespace Pancake.Game
{
    using UnityEngine;

    [EditorIcon("icon_default")]
    public class BackupDataUpdater : GameUnit
    {
        private float _time;

        private void Start()
        {
            if (GameServiceSettings.EnableAutoBackup && GameServiceSettings.ByApplicationQuit) App.AddQuitCallback(ApplicationQuitCallback);
        }

        private static async void ApplicationQuitCallback()
        {
            // todo silent backup data
            await BackupDataHelper.Backup();
        }

        public override async void OnUpdate()
        {
            if (GameServiceSettings.EnableAutoBackup && GameServiceSettings.ByTime)
            {
                _time += Time.deltaTime;
                if (_time >= GameServiceSettings.BackupTimeInterval)
                {
                    _time = 0f;
                    // todo silent backup data
                    // can also display a loading icon during backup in the corner of the screen
                    await BackupDataHelper.Backup();
                }
            }
        }
    }
}