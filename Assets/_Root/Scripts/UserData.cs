namespace Pancake.SceneFlow
{
    public struct UserData
    {
        /// <summary>
        /// Add <paramref name="amount"/> into current coin of user
        /// </summary>
        /// <param name="amount"></param>
        public static void AddCoin(int amount) { Data.Save(Constant.CURRENT_COIN, Data.Load<int>(Constant.CURRENT_COIN) + amount); }

        /// <summary>
        /// Minus <paramref name="amount"/> into current coin of user
        /// </summary>
        /// <param name="amount"></param>
        public static void MinusCoin(int amount) { Data.Save(Constant.CURRENT_COIN, Data.Load<int>(Constant.CURRENT_COIN) - amount); }

        public static int GetCurrentCoin() => Data.Load<int>(Constant.CURRENT_COIN);
    }
}