using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Tool
{
    /// <summary>
    /// 递归查找指定名字的子节点
    /// </summary>
    public static Transform FindChildRecursive(this Transform parent, string targetName)
    {
        if (parent.name == targetName)
        {
            return parent;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            Transform result = FindChildRecursive(child, targetName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}
