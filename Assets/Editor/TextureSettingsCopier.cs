using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class TextureSettingsCopier : EditorWindow
{
    private DefaultAsset sourceFolderAsset;
    private DefaultAsset targetFolderAsset;
    private string sourceFolder = "";
    private string targetFolder = "";
    
    private Vector2 scrollPosition;
    private List<TextureMatchInfo> matchedTextures = new List<TextureMatchInfo>();
    private bool showPreview = false;

    [MenuItem("Tools/yarihcas1_lab/Texture Settings Copier")]
    public static void ShowWindow()
    {
        var window = GetWindow<TextureSettingsCopier>("Texture Settings Copier");
        window.minSize = new Vector2(600, 400);
    }
    
    private void OnGUI()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("テクスチャ設定一括コピーツール", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("同じ名前のテクスチャファイルの設定（解像度、圧縮形式など）を一括コピーします", MessageType.Info);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space();
        
        // Source folder selection
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("コピー元フォルダ（設定を取得）", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();
        sourceFolderAsset = (DefaultAsset)EditorGUILayout.ObjectField(
            "フォルダを選択", 
            sourceFolderAsset, 
            typeof(DefaultAsset), 
            false
        );
        
        if (EditorGUI.EndChangeCheck())
        {
            if (sourceFolderAsset != null)
            {
                string path = AssetDatabase.GetAssetPath(sourceFolderAsset);
                if (AssetDatabase.IsValidFolder(path))
                {
                    sourceFolder = path;
                }
                else
                {
                    sourceFolderAsset = null;
                    EditorUtility.DisplayDialog("エラー", "フォルダを選択してください", "OK");
                }
            }
            else
            {
                sourceFolder = "";
            }
            showPreview = false;
        }
        
        if (!string.IsNullOrEmpty(sourceFolder))
        {
            EditorGUILayout.LabelField("選択中: " + sourceFolder, EditorStyles.miniLabel);
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space();
        
        // Target folder selection
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("コピー先フォルダ（設定を適用）", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();
        targetFolderAsset = (DefaultAsset)EditorGUILayout.ObjectField(
            "フォルダを選択", 
            targetFolderAsset, 
            typeof(DefaultAsset), 
            false
        );
        
        if (EditorGUI.EndChangeCheck())
        {
            if (targetFolderAsset != null)
            {
                string path = AssetDatabase.GetAssetPath(targetFolderAsset);
                if (AssetDatabase.IsValidFolder(path))
                {
                    targetFolder = path;
                }
                else
                {
                    targetFolderAsset = null;
                    EditorUtility.DisplayDialog("エラー", "フォルダを選択してください", "OK");
                }
            }
            else
            {
                targetFolder = "";
            }
            showPreview = false;
        }
        
        if (!string.IsNullOrEmpty(targetFolder))
        {
            EditorGUILayout.LabelField("選択中: " + targetFolder, EditorStyles.miniLabel);
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space();
        
        // Action buttons
        EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(sourceFolder) || string.IsNullOrEmpty(targetFolder));
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("プレビュー", GUILayout.Height(30)))
        {
            PreviewMatches();
        }
        
        GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
        if (GUILayout.Button("設定を一括適用", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog(
                "確認", 
                $"{matchedTextures.Count}個のテクスチャ設定を適用しますか？", 
                "適用", 
                "キャンセル"))
            {
                ApplySettings();
            }
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.Space();
        
        // Preview list
        if (showPreview && matchedTextures.Count > 0)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"マッチしたテクスチャ: {matchedTextures.Count}個", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
            
            foreach (var match in matchedTextures)
            {
                EditorGUILayout.BeginVertical("box");
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(match.fileName, EditorStyles.boldLabel);
                
                if (match.hasChanges)
                {
                    GUIStyle changeStyle = new GUIStyle(EditorStyles.label);
                    changeStyle.normal.textColor = new Color(0.9f, 0.5f, 0.1f);
                    EditorGUILayout.LabelField("変更あり", changeStyle, GUILayout.Width(60));
                }
                else
                {
                    GUIStyle sameStyle = new GUIStyle(EditorStyles.label);
                    sameStyle.normal.textColor = new Color(0.4f, 0.7f, 0.4f);
                    EditorGUILayout.LabelField("同一", sameStyle, GUILayout.Width(60));
                }
                
                EditorGUILayout.EndHorizontal();
                
                EditorGUI.indentLevel++;
                
                // Show setting differences
                if (match.hasChanges)
                {
                    foreach (var change in match.changes)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(change, EditorStyles.miniLabel);
                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("設定は既に同じです", EditorStyles.miniLabel);
                }
                
                EditorGUI.indentLevel--;
                
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        else if (showPreview && matchedTextures.Count == 0)
        {
            EditorGUILayout.HelpBox("同じ名前のテクスチャが見つかりませんでした", MessageType.Warning);
        }
    }
    
    private void PreviewMatches()
    {
        matchedTextures.Clear();
        
        // Get all textures from source folder
        string[] sourceGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { sourceFolder });
        Dictionary<string, string> sourceTextures = new Dictionary<string, string>();
        
        foreach (string guid in sourceGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileName(path);
            sourceTextures[fileName] = path;
        }
        
        // Get all textures from target folder
        string[] targetGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { targetFolder });
        
        foreach (string guid in targetGuids)
        {
            string targetPath = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileName(targetPath);
            
            if (sourceTextures.ContainsKey(fileName))
            {
                string sourcePath = sourceTextures[fileName];
                var matchInfo = CompareTextureSettings(sourcePath, targetPath, fileName);
                matchedTextures.Add(matchInfo);
            }
        }
        
        // Sort by whether they have changes
        matchedTextures = matchedTextures.OrderByDescending(m => m.hasChanges).ToList();
        showPreview = true;
    }
    
    private TextureMatchInfo CompareTextureSettings(string sourcePath, string targetPath, string fileName)
    {
        TextureImporter sourceImporter = AssetImporter.GetAtPath(sourcePath) as TextureImporter;
        TextureImporter targetImporter = AssetImporter.GetAtPath(targetPath) as TextureImporter;
        
        var matchInfo = new TextureMatchInfo
        {
            fileName = fileName,
            sourcePath = sourcePath,
            targetPath = targetPath,
            changes = new List<string>()
        };
        
        // Compare settings
        if (sourceImporter.maxTextureSize != targetImporter.maxTextureSize)
        {
            matchInfo.changes.Add($"Max Size: {targetImporter.maxTextureSize} → {sourceImporter.maxTextureSize}");
        }
        
        if (sourceImporter.textureCompression != targetImporter.textureCompression)
        {
            matchInfo.changes.Add($"Compression: {targetImporter.textureCompression} → {sourceImporter.textureCompression}");
        }
        
        if (sourceImporter.mipmapEnabled != targetImporter.mipmapEnabled)
        {
            matchInfo.changes.Add($"Generate Mip Maps: {targetImporter.mipmapEnabled} → {sourceImporter.mipmapEnabled}");
        }
        
        if (sourceImporter.filterMode != targetImporter.filterMode)
        {
            matchInfo.changes.Add($"Filter Mode: {targetImporter.filterMode} → {sourceImporter.filterMode}");
        }
        
        if (sourceImporter.wrapMode != targetImporter.wrapMode)
        {
            matchInfo.changes.Add($"Wrap Mode: {targetImporter.wrapMode} → {sourceImporter.wrapMode}");
        }
        
        if (sourceImporter.anisoLevel != targetImporter.anisoLevel)
        {
            matchInfo.changes.Add($"Aniso Level: {targetImporter.anisoLevel} → {sourceImporter.anisoLevel}");
        }
        
        matchInfo.hasChanges = matchInfo.changes.Count > 0;
        
        return matchInfo;
    }
    
    private void ApplySettings()
    {
        if (matchedTextures.Count == 0)
        {
            PreviewMatches();
        }
        
        int appliedCount = 0;
        
        try
        {
            AssetDatabase.StartAssetEditing();
            
            foreach (var match in matchedTextures)
            {
                if (match.hasChanges)
                {
                    TextureImporter sourceImporter = AssetImporter.GetAtPath(match.sourcePath) as TextureImporter;
                    TextureImporter targetImporter = AssetImporter.GetAtPath(match.targetPath) as TextureImporter;
                    
                    if (sourceImporter != null && targetImporter != null)
                    {
                        CopyTextureSettings(sourceImporter, targetImporter);
                        targetImporter.SaveAndReimport();
                        appliedCount++;
                    }
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }
        
        EditorUtility.DisplayDialog(
            "完了", 
            $"{appliedCount}個のテクスチャ設定を適用しました", 
            "OK"
        );
        
        // Refresh preview
        PreviewMatches();
    }
    
    private void CopyTextureSettings(TextureImporter source, TextureImporter target)
    {
        // Basic settings
        target.textureType = source.textureType;
        target.textureShape = source.textureShape;
        target.sRGBTexture = source.sRGBTexture;
        target.alphaSource = source.alphaSource;
        target.alphaIsTransparency = source.alphaIsTransparency;
        
        // Advanced settings
        target.npotScale = source.npotScale;
        target.isReadable = source.isReadable;
        target.streamingMipmaps = source.streamingMipmaps;
        target.streamingMipmapsPriority = source.streamingMipmapsPriority;
        
        // Mipmap settings
        target.mipmapEnabled = source.mipmapEnabled;
        target.borderMipmap = source.borderMipmap;
        target.mipmapFilter = source.mipmapFilter;
        target.fadeout = source.fadeout;
        
        // Wrap and filter settings
        target.wrapMode = source.wrapMode;
        target.wrapModeU = source.wrapModeU;
        target.wrapModeV = source.wrapModeV;
        target.wrapModeW = source.wrapModeW;
        target.filterMode = source.filterMode;
        target.anisoLevel = source.anisoLevel;
        
        // Max texture size and compression
        target.maxTextureSize = source.maxTextureSize;
        target.textureCompression = source.textureCompression;
        target.compressionQuality = source.compressionQuality;
        target.crunchedCompression = source.crunchedCompression;
        
        // Copy platform-specific settings
        string[] platforms = new[] { "Standalone", "Android", "iPhone", "WebGL" };
        
        foreach (string platform in platforms)
        {
            TextureImporterPlatformSettings sourcePlatform = source.GetPlatformTextureSettings(platform);
            if (sourcePlatform.overridden)
            {
                TextureImporterPlatformSettings targetPlatform = new TextureImporterPlatformSettings();
                sourcePlatform.CopyTo(targetPlatform);
                target.SetPlatformTextureSettings(targetPlatform);
            }
        }
        
        // Sprite settings (if applicable)
        if (source.textureType == TextureImporterType.Sprite)
        {
            target.spriteImportMode = source.spriteImportMode;
            target.spritePixelsPerUnit = source.spritePixelsPerUnit;
            target.spritePivot = source.spritePivot;
            target.spriteBorder = source.spriteBorder;
        }
    }
    
    private class TextureMatchInfo
    {
        public string fileName;
        public string sourcePath;
        public string targetPath;
        public bool hasChanges;
        public List<string> changes;
    }
}