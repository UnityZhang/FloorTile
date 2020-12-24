using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用的扩展，类的扩展
/// </summary>
public static class ClassExtension
{
    /// <summary>
    /// 功能：判断是否为空
    /// 示例：
    /// <code>
    /// var simpleObject = new object();
    ///
    /// if (simpleObject.IsNull()) // 等价于 simpleObject == null
    /// {
    ///     // do sth
    /// }
    /// </code>
    /// </summary>
    /// <param name="selfObj">判断对象(this)</param>
    /// <typeparam name="T">对象的类型（可不填）</typeparam>
    /// <returns>是否为空</returns>
    public static bool IsNull<T>(this T selfObj) where T: class
    {
        return null == selfObj;
    }

    /// <summary>
    /// 功能：判断不是为空
    /// 示例：
    /// <code>
    /// var simpleObject = new object();
    ///
    /// if (simpleObject.IsNotNull()) // 等价于 simpleObject != null
    /// {
    ///    // do sth
    /// }
    /// </code>
    /// </summary>
    /// <param name="selfObj">判断对象（this)</param>
    /// <typeparam name="T">对象的类型（可不填）</typeparam>
    /// <returns>是否不为空</returns>
    public static bool IsNotNull<T>(this T selfObj) where T : class
    {
        return null != selfObj;
    }
}

/// <summary>
/// 字符串扩展
/// </summary>
public static class StringExtension {

    /// <summary>
    /// Check Whether string is null or empty
    /// </summary>
    /// <param name="selfStr"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty(this string selfStr) {
        return string.IsNullOrEmpty(selfStr);
    }    
}

/// <summary>
/// 列表扩展
/// </summary>
public static class ListExtension
{
    /// <summary>
    /// Check Whether string is null or empty
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="selfList"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty<T>(this List<T> selfList)
    {
        return selfList.IsNull() || selfList.Count <= 0;
    }
}

/// <summary>
/// 字典扩展
/// </summary>
public static class DictinatryExtension
{
    public static bool IsNullOrEmpty<T1, T2>(this Dictionary<T1, T2> selfDict) {
        return selfDict.IsNull() || selfDict.Count <= 0;
    }
}
