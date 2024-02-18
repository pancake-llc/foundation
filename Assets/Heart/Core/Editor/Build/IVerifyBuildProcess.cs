namespace PancakeEditor
{
    public interface IVerifyBuildProcess
    {
        bool OnVerify();
        void OnComplete(bool status);
    }
}