namespace Pancake.LevelSystem
{
    public static class LevelHelper
    {
        // check if all the prefabs are not null and with a specified ID
        public static bool IsValid(LevelSystemSetting setting)
        {
            if (setting == null || setting.Units == null || setting.Units.Length == 0) return false;
            foreach (var unit in setting.Units)
            {
                if (string.IsNullOrEmpty(unit.id) || unit.prefab == null) return false;
            }

            return true;
        }
    }
}