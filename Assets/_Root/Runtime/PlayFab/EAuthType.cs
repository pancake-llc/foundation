#if PANCAKE_PLAYFAB
namespace Pancake.GameService
{
    public enum EAuthType
    {
        None = 0,
        Silent = 1,
        UsernameAndPassword = 2,
        EmailAndPassword = 3,
        RegisterPlayFabAccount = 4,
        Facebook = 5,
        Google = 6,
        Apple = 7,
    }
}
#endif