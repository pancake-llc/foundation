using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pancake;
using Pancake.ExLibEditor;
using Unity.SharpZipLib.Zip;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace PancakeEditor
{
    public static class UtilitiesOdinDrawer
    {
        public static UnityWebRequest webRequest;
        private static string password = "";

        public static void OnInspectorGUI()
        {

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Password: ", GUILayout.MaxWidth(100));
            password = GUILayout.PasswordField(password, '*', EditorStyles.textField);
            EditorGUILayout.EndHorizontal();
            GUI.enabled = !EditorApplication.isCompiling && !string.IsNullOrEmpty(password);
            GUI.backgroundColor = Uniform.Pink;
            if (GUILayout.Button("Install Odin", GUILayout.Height(40)))
            {
                EditorCoroutine.Start(IeOdinDownload());
            }

            GUI.backgroundColor = Color.white;
            GUI.enabled = true;
        }

        private static void OnDownloadOdinProgress(float progress, bool done)
        {
            // Download is complete. Clear progress bar.
            if (done)
            {
                EditorUtility.ClearProgressBar();
            }
            // Download is in progress, update progress bar.
            else
            {
                if (EditorUtility.DisplayCancelableProgressBar("Odin", string.Format("Downloading odin..."), progress))
                {
                    webRequest?.Abort();
                    EditorUtility.ClearProgressBar();
                }
            }
        }

        private static IEnumerator IeOdinDownload()
        {
            string pathFile = Path.Combine(Application.temporaryCachePath, "odin.zip");
            const string urlDownload = "https://github.com/pancake-llc/unitypackage/releases/download/1.0.0/odin.zip";
            var downloadHandler = new DownloadHandlerFile(pathFile);
            Debug.Log(pathFile);
            webRequest = new UnityWebRequest(urlDownload) {method = UnityWebRequest.kHttpVerbGET, downloadHandler = downloadHandler};
            var operation = webRequest.SendWebRequest();

            while (!operation.isDone)
            {
                yield return new WaitForSeconds(0.1f);
                OnDownloadOdinProgress(operation.progress, operation.isDone);
            }

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(webRequest.error);
            }
            else
            {
                string folderUnZip = Path.Combine(Application.temporaryCachePath, "UnZip");
                if (!Directory.Exists(folderUnZip)) Directory.CreateDirectory(folderUnZip);
                EditorUnZip.UnZip(folderUnZip, File.ReadAllBytes(pathFile), password);
                AssetDatabase.ImportPackage(Path.Combine(folderUnZip, "odin.unitypackage"), false);
            }
        }
    }
}