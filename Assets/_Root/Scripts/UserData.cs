namespace Pancake.SceneFlow
{
    public struct UserData
    {
        /// <summary>
        /// Add <paramref name="amount"/> into current coin of user
        /// </summary>
        /// <param name="amount"></param>
        public static void AddCoin(int amount) => Data.Save(Constant.CURRENT_COIN, Data.Load<int>(Constant.CURRENT_COIN) + amount);

        /// <summary>
        /// Minus <paramref name="amount"/> into current coin of user
        /// </summary>
        /// <param name="amount"></param>
        public static void MinusCoin(int amount) => Data.Save(Constant.CURRENT_COIN, Data.Load<int>(Constant.CURRENT_COIN) - amount);

        public static int GetCurrentCoin() => Data.Load<int>(Constant.CURRENT_COIN);

        public static void SetCurrentSkinHat(string id) => Data.Save(Constant.CHARACTER_SKIN_HAT, id);
        public static string GetCurrentSkinHat() => Data.Load(Constant.CHARACTER_SKIN_HAT, "");

        public static void SetCurrentSkinShirt(string id) => Data.Save(Constant.CHARACTER_SKIN_SHIRT, id);
        public static string GetCurrentSkinShirt() => Data.Load(Constant.CHARACTER_SKIN_SHIRT, "");

        public static void SetCurrentSkinShoes(string id) => Data.Save(Constant.CHARACTER_SKIN_SHOES, id);
        public static string GetCurrentSkinShoes() => Data.Load(Constant.CHARACTER_SKIN_SHOES, "");

        public static int GetCurrentWeekDailyReward() => Data.Load(Constant.WEEK_DAILY_REWARD, 1);
        public static void SetCurrentWeekDailyReward(int value) => Data.Save(Constant.WEEK_DAILY_REWARD, value);
        public static void NextWeekDailyReward() => Data.Save(Constant.WEEK_DAILY_REWARD, GetCurrentWeekDailyReward() + 1);

        public static int GetCurrentDayDailyReward() => Data.Load(Constant.CURRENT_DAY_DAILY_REWARD, 1);
        public static void SetCurrentDayDailyReward(int day) => Data.Save(Constant.CURRENT_DAY_DAILY_REWARD, day.Min(7));
        public static void NextDayDailyReward() => Data.Save(Constant.CURRENT_DAY_DAILY_REWARD, (GetCurrentDayDailyReward() + 1).Min(7));
    }
}