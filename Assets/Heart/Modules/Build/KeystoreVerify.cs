using Pancake;
using Pancake.Common;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public class KeystoreVerify : IVerifyBuildProcess
    {
        public bool OnVerify()
        {
            var s = AndroidBuildPipelineSettings.Instance;

            if (s.environment == AndroidBuildPipelineSettings.Environment.Development && s.customKeystore ||
                s.environment == AndroidBuildPipelineSettings.Environment.Production)
            {
                if (string.IsNullOrEmpty(s.keystorePath) || string.IsNullOrEmpty(s.alias) || string.IsNullOrEmpty(s.password) || string.IsNullOrEmpty(s.aliasPassword))
                {
                    EditorUtility.DisplayDialog("Keystore", "Keystore infomation can not be empty!\nPlease enter correct and try again!", "Ok");
                    return false;
                }
            }

            return true;
        }

        public void OnComplete(bool status)
        {
            Debug.Log(status ? "[Keystore] Verify Success".SetColor(Uniform.Success) : "[Keystore] Verify Failure".SetColor(Uniform.Error));
        }
    }
}