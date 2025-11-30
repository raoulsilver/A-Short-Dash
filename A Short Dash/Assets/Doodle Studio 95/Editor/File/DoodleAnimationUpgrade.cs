using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace DoodleStudio95 {

public class DoodleAnimationUpgrade : AssetPostprocessor
{
    private const string OLD_GUID = "ab85ac808950aa04ea8320fc273cd8c0";

    static List<string> _ls;

    private void OnPreprocessAsset()
    {
        if (Path.GetExtension(assetPath).ToLower() == ".asset" && IsOldAnimationFile(assetPath))
        {
            (string newFileID, string newFileGUID) = FindDoodleAnimationFileScript();
            if (newFileID == null || newFileGUID == null)
            {
                // Debug.LogError("DoodleAnimationFile ScriptableObject not found.");
                return;
            }
            UpdateYAML(assetPath, newFileID, newFileGUID);
        }
    }

    [MenuItem("Tools/Doodle Studio 95/Upgrade Selected Animations")]
    private static void UpgradeSelectedAnimations()
    {
        // Get the selected assets
        string[] selectedAssetsGUIDs = Selection.assetGUIDs;

        (string newFileID, string newFileGUID) = FindDoodleAnimationFileScript();
        if (newFileID == null || newFileGUID == null)
        {
            // Debug.LogError("DoodleAnimationFile ScriptableObject not found.");
            return;
        }

        _ls = new List<string>();
        foreach (string assetGUID in selectedAssetsGUIDs)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
            if (Path.GetExtension(assetPath).ToLower() == ".asset" && IsOldAnimationFile(assetPath))
            {
                _ls.Add(assetGUID);
            }
        }
        
        Selection.objects = new Object[0];

        foreach(var assetGUID in _ls)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
            UpdateYAML(assetPath, newFileID, newFileGUID);
        }

        AssetDatabase.Refresh();

        Selection.objects = _ls.Select(guid => AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guid))).ToArray();

        if (_ls.Count > 0)
            EditorUtility.DisplayDialog("Upgrade Completed", $"Upgraded {_ls.Count} files.", "OK");
        else
            EditorUtility.DisplayDialog("Upgrade Skipped", $"No files to upgrade.", "OK");
    }

    private static (string fileID, string guid) FindDoodleAnimationFileScript()
    {
        string[] guids = AssetDatabase.FindAssets("t:Script DoodleAnimationFile");
        if (guids.Length == 0)
        {
            return (null, null);
        }

        string assetPath = "";
        string assetGUID = "";
        foreach (string guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) == "DoodleAnimationFile")
            {
                assetPath = path;
                assetGUID = guid;
                break;
            }
        }
        if (string.IsNullOrEmpty(assetPath))
        {
            // Debug.LogError("DoodleAnimationFile ScriptableObject not found.");
            return (null, null);
        }

        string metaPath = AssetDatabase.GetTextMetaFilePathFromAssetPath(assetPath);
        string metaContent = File.ReadAllText(metaPath);

        Regex guidRegex = new Regex(@"guid:\s*(-?\d+),");
        Match guidMatch = guidRegex.Match(metaContent);
        if (guidMatch.Success)
            assetGUID = guidMatch.Groups[1].Value;

        // Get the fileID for the target ScriptableObject
        string targetFileID = AssetDatabase.GetAssetDependencyHash(assetPath).ToString();
        var obj = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out assetGUID, out long fileID);

        // Debug.Log("assetGUID: " + assetGUID + " fileID: " + fileID.ToString());
        return (fileID.ToString(), assetGUID);
    }

    private static bool IsOldAnimationFile(string assetPath)
    {
        string yamlPath = assetPath;
        if (!File.Exists(yamlPath))
        {
            return false;
        }

        string content = File.ReadAllText(yamlPath);
        string pattern = @"m_Script:\s*\{fileID:\s*-?\d+,\s*guid:\s*" + OLD_GUID + @",\s*type:\s*3\}";
        return Regex.IsMatch(content, pattern);
    }

    private static void UpdateYAML(string assetPath, string fileID, string guid)
    {
        string yamlPath = assetPath;
        if (!File.Exists(yamlPath))
        {
            return;
        }

        string content = File.ReadAllText(yamlPath);
        bool updated = false;

        string pattern = @"m_Script:\s*\{fileID:\s*-?\d+,\s*guid:\s*[0-9a-fA-F]+,\s*type:\s*3\}";
        Match match = Regex.Match(content, pattern);
        if (match.Success)
        {
            string replacement = $"m_Script: {{fileID: {fileID}, guid: {guid}, type: 3}}";
            content = Regex.Replace(content, pattern, replacement);
            updated = true;
        }

        if (updated)
        {
            File.WriteAllText(yamlPath, content);
            Debug.Log($"[Doodle Studio 95!] Upgraded old animation file: {assetPath}");
        }
    }
}
}