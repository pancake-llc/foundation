using Pancake.Localization;
using VContainer.Unity;

namespace Pancake.SceneFlow
{
    public class RequireInitalizeLocale : IInitializable
    {
        public void Initialize()
        {
            UserData.LoadLanguageSetting(LocaleSettings.DetectDeviceLanguage); // Do not add any other load process here to avoid slow loading
        }
    }
}