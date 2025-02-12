﻿using System.IO;
using UnityEditor;
using UnityEngine;
using VCIGLTF;

namespace VCI
{
    public static class VCIObjectExporterMenu
    {
        #region Export NonHumanoid

        private const string CONVERT_OBJECT_KEY = VCIVersion.MENU + "/Export VCI";

        [MenuItem(CONVERT_OBJECT_KEY)]
        public static void ExportObject()
        {
            var errorMessage = "";
            if (!Validate(out errorMessage))
            {
                Debug.LogAssertion(errorMessage);
                EditorUtility.DisplayDialog("Error", errorMessage, "OK");
                return;
            }

            // save dialog
            var root = Selection.activeObject as GameObject;
            var path = EditorUtility.SaveFilePanel(
                "Save " + VCIVersion.EXTENSION,
                null,
                root.name + VCIVersion.EXTENSION,
                VCIVersion.EXTENSION.Substring(1));
            if (string.IsNullOrEmpty(path)) return;

            // export
            var gltf = new glTF();
            using (var exporter = new VCIExporter(gltf))
            {
                exporter.Prepare(root);
                exporter.Export();
            }

            var bytes = gltf.ToGlbBytes();
            File.WriteAllBytes(path, bytes);

            if (path.StartsWithUnityAssetPath())
            {
                AssetDatabase.ImportAsset(path.ToUnityRelativePath());
                AssetDatabase.Refresh();
            }

            // Show the file in the explorer.
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                System.Diagnostics.Process.Start("explorer.exe", " /e,/select," + path.Replace("/", "\\"));
            }
        }

        private static bool Validate(out string errorMessage)
        {
            var selectedGameObjects = Selection.gameObjects;
            if (selectedGameObjects.Length == 0)
            {
                errorMessage = "VCIObjectがアタッチされたGameObjectを選択して下さい。";
                return false;
            }

            if (2 <= selectedGameObjects.Length)
            {
                errorMessage = "VCIObjectがアタッチされたGameObjectを1つ選択して下さい。";
                return false;
            }

            var vciObject = selectedGameObjects[0].GetComponent<VCIObject>();
            if (vciObject == null)
            {
                errorMessage = "VCIObjectがアタッチされたGameObjectを選択して下さい。";
                return false;
            }

            var isValid = VCIMetaValidator.Validate(vciObject, out errorMessage);
            return isValid;
        }

        #endregion
    }
}