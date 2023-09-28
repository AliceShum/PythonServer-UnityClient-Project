using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : Singleton<ObjectPool>
{
    private const int maxCount = 128;
    private Dictionary<string, List<GameObject>> pool = new Dictionary<string, List<GameObject>>();

    public GameObject GetObj(GameObject perfab)
    {
        //池子中有
        GameObject result = null;
        if (pool.ContainsKey(perfab.name))
        {
            if (pool[perfab.name].Count > 0)
            {
                result = pool[perfab.name][0];
                result.SetActive(true);
                pool[perfab.name].Remove(result);
                return result;
            }
        }
        //池子中缺少
        result = Object.Instantiate(perfab);
        result.name = perfab.name;
        RecycleObj(result);
        GetObj(result);
        return result;
    }

    public GameObject GetObj(GameObject perfab, Transform parent)
    {
        var result = GetObj(perfab);
        var localPos = result.transform.localPosition;
        result.transform.SetParent(parent);
        result.transform.localPosition = localPos;
        return result;
    }

    public void RecycleObj(GameObject obj)
    {
        var par = Camera.main;
        var localPos = obj.transform.localPosition;
        obj.transform.SetParent(par.transform);
        obj.transform.localPosition = localPos;
        obj.SetActive(false);

        if (pool.ContainsKey(obj.name))
        {
            if (pool[obj.name].Count < maxCount)
            {
                pool[obj.name].Add(obj);
            }
        }
        else
        {
            pool.Add(obj.name, new List<GameObject>() { obj });
        }
    }

    public void RecycleAllChildren(GameObject parent)
    {
        if (parent == null) return;
        for (; parent.transform.childCount > 0;)
        {
            var tar = parent.transform.GetChild(0).gameObject;
            RecycleObj(tar);
        }
    }

    public void Clear()
    {
        pool.Clear();
    }

    //获取全部生成的子物体
    public List<Transform> GetAllChildren(Transform parent)
    {
        List<Transform> childList = new List<Transform>();
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            childList.Add(parent.transform.GetChild(i));
        }
        return childList;
    }
}