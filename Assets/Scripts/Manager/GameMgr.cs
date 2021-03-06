﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMgr : UnitySingleton<GameMgr>
{
    private bool init = false;
    private Transform m_cameraRoot;
    private Transform m_playerRoot;
    private Transform m_itemRoot;

    private Entity m_MainEntity; //主角
    private Dictionary<string, int> downBinInfoDic;
    private CharacterController m_characController; //主角控制器
    private ARPGCameraController m_cameraController; //相机跟随控制
    private ARPGAnimatorController m_animController; //主角动作控制器
    private Dictionary<int, MapInfo> m_dicMapInfo;
    

    public ARPGAnimatorController ARPGAnimatController
    {
        get
        {
            if (null == m_animController)
            {
                m_animController = MainEntity.CacheModel.GetComponent<ARPGAnimatorController>();
            }
            return m_animController;
        }

        set
        {
            m_animController = value;
        }
    }

    public ARPGCameraController CameraController
    {
        get
        {
            if (null == m_cameraController)
            {
                m_cameraController = m_cameraRoot.Find("MainCamera").GetComponent<ARPGCameraController>();
            }
            return m_cameraController;
        }
    }

    public Entity MainEntity
    {
        get
        {
            if (null == m_MainEntity)
            {
                m_MainEntity = m_playerRoot.Find("Charactor").GetComponent<Entity>();
            }
            return m_MainEntity;
        }
    }

    public CharacterController CharacController
    {
        get
        {
            if (null == m_characController)
            {
                if (null != MainEntity)
                {
                    m_characController = MainEntity.CacheModel.GetComponent<CharacterController>();
                }
                else
                {
                    Debuger.LogError("MainEntity is Null!");
                }
            }
            return m_characController;
        }

        set
        {
            m_characController = value;
        }
    }

    public Dictionary<string, int> DownBinInfoDic
    {
        get
        {
            if (null == downBinInfoDic)
            {
                downBinInfoDic = new Dictionary<string, int>();
            }
            return downBinInfoDic;
        }
        set
        {
            downBinInfoDic = value;
        }
    }

    public void Awake()
    {
        Init();
    }

    void Init()
    {
        if(init)
        {
            return;
        }
        init = true;
        Transform m_self = transform;
        m_cameraRoot = m_self.Find("Camera");
        m_playerRoot = m_self.Find("Player");
        m_itemRoot = m_self.Find("ItemRoot");
        LoadTemplate();
    }

    //加载模板数据
    void LoadTemplate()
    {
        if (IsNewVersion)
        {
            StartCoroutine(DownFiles(AppConst.AppContentPath, AppConst.AppPersistentPath));
        }
        else
        {
            InitTemplate();
        }
    }

    //加载配置表数据
    void InitTemplate()
    {
        string path = new System.Text.StringBuilder().Append(AppConst.AppPersistentPath).Append('/').Append(AppConst.TextDir).Append('/').ToString();
        Util.Init<HeroInfo>(path);
        Util.Init<EffectInfo>(path);
        Util.Init<ParticleInfo>(path);
        Util.Init<SkillInfo>(path);
        Util.Init<LanSurInfo>(path);
        Util.Init<LanTxtInfo>(path);
        Util.Init<ConstInfo>(path);
        Util.Init<OccupationInfo>(path);
        Util.Init<ItemInfo>(path);
        Util.Init<LevelInfo>(path);
        Util.Init<ItemEffectInfo>(path);

        Util.InitMap(path + "map.bin");
        CreateEntity(1);
        ItemDropMgr.Instance.InitMapDrop(2);
        UIManager.Instance.ShowWindow(WindowID.WindowID_MainUI);
    }


    //保存版本号
    void SaveVersion()
    {
        PlayerPrefs.SetString(AppConst.VersionKey, AppConst.Version);
        PlayerPrefs.Save();
    }

    //判断当前是否最新版本
    bool IsNewVersion
    {
        get
        {
            string version = PlayerPrefs.GetString(AppConst.VersionKey);
            return !Util.Equals(version, AppConst.Version);
        }
    }

    public Dictionary<int, MapInfo> DicMapInfo
    {
        get
        {
            if(null == m_dicMapInfo)
            {
                m_dicMapInfo = new Dictionary<int, MapInfo>();
            }
            return m_dicMapInfo;
        }

        set
        {
            m_dicMapInfo = value;
        }
    }

    public Transform ItemRoot
    {
        get
        {
            if(null == m_itemRoot)
            {
                m_itemRoot = transform.Find("ItemRoot");
            }
            return m_itemRoot;
        }

        set
        {
            m_itemRoot = value;
        }
    }

    IEnumerator DownFiles(string from, string dest)
    {
        yield return StartCoroutine(DownBinFileInfo(from + AppConst.FileBin));

        yield return StartCoroutine(PersistFiles(from + AppConst.TextDir + "/", dest + "/" + AppConst.TextDir + "/"));

        DownBinInfoDic.Clear();

        SaveVersion();

        InitTemplate();
    }

    IEnumerator DownBinFileInfo(string path)
    {
        using (WWW www = new WWW(path))
        {
            yield return www;
            if (string.IsNullOrEmpty(www.error))
            {
                if (www.isDone)
                {
                    string text = www.text;
                    string[] lines = text.Split('\r', '\n');
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (string.IsNullOrEmpty(lines[i]))
                        {
                            continue;
                        }
                        string[] contents = lines[i].Split(AppConst.Separate);
                        if (contents.Length > 1)
                        {
                            DownBinInfoDic[contents[0]] = int.Parse(contents[1]);
                        }
                    }
                }
            }
            else
            {
                Debuger.LogError(www.error);
            }
        }
    }

    IEnumerator PersistFiles(string from, string dest)
    {
        if (!System.IO.Directory.Exists(dest))
        {
            System.IO.Directory.CreateDirectory(dest);
        }

        foreach (KeyValuePair<string, int> kv in DownBinInfoDic)
        {
            string fromFile = from + kv.Key;
            string destFile = dest + kv.Key;

            using (WWW www = new WWW(fromFile))
            {
                yield return www;

                if (string.IsNullOrEmpty(www.error))
                {
                    if (www.isDone)
                    {
                        System.IO.File.WriteAllBytes(destFile, www.bytes);
                    }
                }
                else
                {
                    Debuger.LogError(www.error);
                }

                yield return null;
            }
        }
        yield return new WaitForEndOfFrame();
    }


    #region 资源模型加载
    //主角模型加载
    void CreateEntity(int heroId)
    {
        Debuger.LogError("CreateEntity 主角模型");
        HeroInfo heroInfo = InfoMgr<HeroInfo>.Instance.GetInfo(heroId);
        //GameObject prefab = ResourcesMgr.Instance.LoadResource<GameObject>(ResourceType.RESOURCE_ENTITY, heroInfo.model);
        //if (null == prefab)
        //{
        //    Debuger.LogError("角色模型不存在Id:" + heroId);
        //    return;
        //}
        GameObject go = ResourcesMgr.Instance.Spawner(heroInfo.model, ResourceType.RESOURCE_ENTITY, transform);// ResourcesMgr.Instance.Instantiate(prefab);
        if (null == go)
        {
            Debuger.LogError("角色模型不存在Id:" + heroId);
            return;
        }
        MainEntity.InitCharactor(go);
        MainEntity.InitEntityAttribute(heroInfo);
    }

    #endregion
}
