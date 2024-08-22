using Pancake.Common;

namespace Pancake.Game
{
    using UnityEngine;

    /// <summary>
    /// this script runs only once in application lifetime
    /// </summary>
    [EditorIcon("icon_one")]
    public class FirstOpenStartup : MonoBehaviour
    {
        private void Start()
        {
            if (!UserData.GetFirstOpen())
            {
                UserData.SetFirstOpen(true);

                string userId = UserData.UserId;
                Data.Save(Constant.User.KEY_ID, userId);
            }
        }
    }
}