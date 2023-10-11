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

        public static void SetCurrentSkinHat(string id) { Data.Save(Constant.CHARACTER_SKIN_HAT, id); }

        public static string GetCurrentSkinHat() { return Data.Load(Constant.CHARACTER_SKIN_HAT, ""); }

        public static void SetCurrentSkinShirt(string id) { Data.Save(Constant.CHARACTER_SKIN_SHIRT, id); }

        public static string GetCurrentSkinShirt() { return Data.Load(Constant.CHARACTER_SKIN_SHIRT, ""); }

        public static void SetCurrentSkinShoes(string id) { Data.Save(Constant.CHARACTER_SKIN_SHOES, id); }

        public static string GetCurrentSkinShoes() { return Data.Load(Constant.CHARACTER_SKIN_SHOES, ""); }
    }
}