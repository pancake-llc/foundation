#define DEBUG_ENABLED

using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Sisus.Init.Internal
{
	/// <summary>
	/// Temporarily copies all link.xml file found inside Init(args)'s package folder into the project's Assets folder
	/// when a build is being created, to avoid types and members in its assemblies from being remove during the
	/// codes stripping process.
	/// <para>
	/// This is needed because Unity doesn't currently support link.xml files inside packages folders.
	/// </para>
	/// </summary>
	internal sealed class InitArgsUnityLinkerProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
	{
		private const string AssetsFolder = "Assets";
		private const string PackagesFolder = "Packages";
		private const string InitArgsPackageName = "com.sisus.init-args";
		private const string InitArgsPackagePath = PackagesFolder + "/" + InitArgsPackageName;
		private const string TemporaryFolderName = "Init(args) Temp";
		private const string TemporaryFolderPath = AssetsFolder + "/" + TemporaryFolderName;
		private const string LinkXmlFileName = "link.xml";

		public int callbackOrder => 10;

		public async void OnPreprocessBuild(BuildReport report)
		{
			var linkXmlAssetPaths = Directory.EnumerateFiles(InitArgsPackagePath, LinkXmlFileName, SearchOption.AllDirectories).ToArray();
			if(linkXmlAssetPaths.Length == 0)
			{
				#if DEV_MODE
				Debug.LogWarning( $"No link.xml files found in '{InitArgsPackagePath}'. Init(args) build stripping prevention might not work.");
				#endif
				return;
			}

			if(!AssetDatabase.IsValidFolder(TemporaryFolderPath))
			{
				AssetDatabase.CreateFolder(AssetsFolder, TemporaryFolderName);
			}

			foreach(var sourceLinkXmlPath in linkXmlAssetPaths)
			{
				string destinationFolderName = Path.GetFileName(Path.GetDirectoryName(sourceLinkXmlPath));

				string destinationFolderPath = Path.Combine(TemporaryFolderPath, destinationFolderName);
				if(!AssetDatabase.IsValidFolder(destinationFolderPath))
				{
					AssetDatabase.CreateFolder(TemporaryFolderPath, destinationFolderName);
				}

				string destinationLinkXmlPath = Path.Combine(destinationFolderPath, LinkXmlFileName);

				if(File.Exists(destinationLinkXmlPath))
				{
					#if DEV_MODE && DEBUG_ENABLED
					Debug.Log($"Won't copy link.xml from '{sourceLinkXmlPath}' to '{destinationLinkXmlPath}', because destination file already exists.");
					#endif
					continue;
				}

				#if DEV_MODE && DEBUG_ENABLED
				Debug.Log($"Copying link.xml from '{sourceLinkXmlPath}' to '{destinationLinkXmlPath}'.");
				#endif

				File.Copy(sourceLinkXmlPath, destinationLinkXmlPath);
			}

			do
			{
				await Until.UnitySafeContext();
			}
			while(BuildPipeline.isBuildingPlayer);

			// Handle deleting temporary folder even if build failed
			AssetDatabase.DeleteAsset(TemporaryFolderPath);
		}

		// Delete temporary folder after build has finished
		public void OnPostprocessBuild(BuildReport report) => AssetDatabase.DeleteAsset(TemporaryFolderPath);
	}
}