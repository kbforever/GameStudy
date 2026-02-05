using UnityEditor;
using UnityEngine;

public static class CreateYellowMaterial
{
    [MenuItem("Tools/Materials/Create Yellow Material")]
    public static void Create()
    {
        const string folderPath = "Assets/Materials";
        const string assetPath = "Assets/Materials/Yellow.mat";

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }

        // 优先 URP Lit；找不到则回退 Standard，保证“基础 3D 物体”可用
        Shader shader =
            Shader.Find("Universal Render Pipeline/Lit") ??
            Shader.Find("Standard");

        if (shader == null)
        {
            Debug.LogError("未找到可用 Shader（URP Lit / Standard 都没找到）。请确认项目渲染管线配置。");
            return;
        }

        var mat = new Material(shader);

        // URP Lit: _BaseColor；Standard: _Color
        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", Color.yellow);
        }
        if (mat.HasProperty("_Color"))
        {
            mat.SetColor("_Color", Color.yellow);
        }

        // 生成/覆盖资源
        AssetDatabase.DeleteAsset(assetPath);
        AssetDatabase.CreateAsset(mat, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = mat;
        EditorGUIUtility.PingObject(mat);
        Debug.Log($"已创建黄色材质：{assetPath}（Shader: {shader.name}）");
    }
}

