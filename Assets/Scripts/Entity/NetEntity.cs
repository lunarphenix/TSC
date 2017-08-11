﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetEntity : IEntity
{
    private int id;
    private NPCControl m_npcControl;
    //private float flee_dis = 10; //逃离距离
    //角色属性
    private EntityAttribute attribute;
    private Transform cacheAccelParticleTran; //加速特效
    private Transform cacheSkillParticleTran; //技能特效
    private Transform cacheParticleParent; //模型特效父节点
    //子节点，角色模型
    private Transform roleModel;
    //当前节点模型Entity
    private Transform cacheModel;

    private SkillInfo skill1;
    private EffectInfo effect1;
    private SkillInfo skill2;
    private EffectInfo effect2;

    private ARPGAnimatorController m_arpgAnimatContorller;
    private CharacterController m_characterController;

    private int m_heroId;

    private bool init = false;

    private OccpType occupation = OccpType.NONE;

    private StateType m_state = StateType.NONE;

    private CollisionType collision = CollisionType.NONE;

    public EntityAttribute Attribute
    {
        get
        {
            if (null == attribute)
            {
                attribute = new EntityAttribute();
            }
            return attribute;
        }
        set
        {
            attribute = value;
        }
    }

    public Transform RoleModel
    {
        get
        {
            return roleModel;
        }
        set
        {
            roleModel = value;
        }
    }

    //当前Entity脚本所在的transform
    public Transform CacheModel
    {
        get
        {
            if (null == cacheModel)
            {
                cacheModel = transform;
            }
            return cacheModel;
        }
        private set
        {
            cacheModel = value;
        }
    }

    public Transform CacheAccelParticleTran
    {
        get
        {
            return cacheAccelParticleTran;
        }

        set
        {
            cacheAccelParticleTran = value;
        }
    }

    public Transform CacheSkillParticleTran
    {
        get
        {
            return cacheSkillParticleTran;
        }

        set
        {
            cacheSkillParticleTran = value;
        }
    }

    public Transform CacheParticleParent
    {
        get
        {
            if (null == cacheParticleParent)
            {
                if (null != CacheModel)
                {
                    cacheParticleParent = CacheModel.Find("p");
                }
            }
            return cacheParticleParent;
        }

        set
        {
            cacheParticleParent = value;
        }
    }

    public StateType State
    {
        get
        {
            return m_state;
        }

        set
        {
            m_state = value;
        }
    }

    public int HeroId
    {
        get
        {
            return m_heroId;
        }

        set
        {
            m_heroId = value;
        }
    }

    public OccpType Occupation
    {
        get
        {
            return occupation;
        }

        set
        {
            occupation = value;
        }
    }

    public ARPGAnimatorController ArpgAnimatContorller
    {
        get
        {
            if (null == m_arpgAnimatContorller)
            {
                m_arpgAnimatContorller = CacheModel.GetComponent<ARPGAnimatorController>();
            }
            return m_arpgAnimatContorller;
        }

        set
        {
            m_arpgAnimatContorller = value;
        }
    }

    public CharacterController CharaController
    {
        get
        {
            if (null == m_characterController)
            {
                m_characterController = CacheModel.GetComponent<CharacterController>();
            }
            return m_characterController;
        }

        set
        {
            m_characterController = value;
        }
    }

    public bool IsAlive
    {
        get
        {
            return Attribute.Hp > 0;
        }
    }

    public int Id
    {
        get
        {
            return id;
        }

        set
        {
            id = value;
        }
    }

    public NPCControl NpcControl
    {
        get
        {
            if (null == m_npcControl)
            {
                m_npcControl = Util.AddComponent<NPCControl>(gameObject);
            }
            return m_npcControl;
        }

        set
        {
            m_npcControl = value;
        }
    }

    public void InitEntity(OccpType occp, int heroId)
    {
        ChangeOccp(occp, heroId);
        HeroInfo heroInfo = InfoMgr<HeroInfo>.Instance.GetInfo(heroId);
        InitEntityAttribute(heroInfo);
        InitSkill();
        init = true;
    }

    void InitSkill()
    {
        skill1 = InfoMgr<SkillInfo>.Instance.GetInfo(1);
        effect1 = InfoMgr<EffectInfo>.Instance.GetInfo(skill1.effectID);
        skill2 = InfoMgr<SkillInfo>.Instance.GetInfo(2);
        effect2 = InfoMgr<EffectInfo>.Instance.GetInfo(skill2.effectID);
    }

    GameObject SpawnerGO(int heroId, Vector3 v)
    {
        HeroInfo heroInfo = InfoMgr<HeroInfo>.Instance.GetInfo(heroId);
        GameObject go = ResourcesMgr.Instance.Spawner(heroInfo.model, v, ResourceType.RESOURCE_ENTITY, transform);// ResourcesMgr.Instance.Instantiate(prefab);
        return go;
    }

    void ChangeOccp(OccpType occp, int heroId)
    {
        Occupation = occp;
        HeroId = heroId;
        HeroInfo heroInfo = InfoMgr<HeroInfo>.Instance.GetInfo(heroId);
        GameObject go = ResourcesMgr.Instance.Spawner(heroInfo.model, ResourceType.RESOURCE_ENTITY, CacheModel);// ResourcesMgr.Instance.Instantiate(prefab);
        if (null == go)
        {
            return;
        }
        ResourcesMgr.Instance.Despawner(ResourceType.RESOURCE_ENTITY, RoleModel);
        InitCharactor(go);
    }

    //初始化主角
    void InitCharactor(GameObject go)
    {
        if (null == go)
        {
            return;
        }
        RoleModel = go.transform;
        RoleModel.parent = CacheModel;
        RoleModel.localScale = Vector3.one;
        RoleModel.localPosition = Vector3.zero;
        RoleModel.localRotation = Quaternion.identity;
        ArpgAnimatContorller = CacheModel.GetComponent<ARPGAnimatorController>();
        ArpgAnimatContorller.Init(RoleModel);
        //GameMgr.Instance.CameraController.SetTarget(CacheModel);
    }

    void InitEntityAttribute(HeroInfo info)
    {
        Attribute.BaseSpeed = AppConst.BaseSpeed;
        Attribute.Speed = Attribute.BaseSpeed;
        Attribute.RebornTime = AppConst.RebornTime;
        Attribute.Basedis = AppConst.Basedis;
        Attribute.Atkdis = Attribute.Basedis;
        Attribute.MaxPhy = AppConst.MaxPhy;
        Attribute.CurPhy = Attribute.MaxPhy;
        Attribute.CostPhySpeed = AppConst.CostPhySpeed;
        Attribute.MaxHp = AppConst.MaxHp;
        Attribute.Hp = Attribute.MaxHp;

        OccupationInfo occpInfo = InfoMgr<OccupationInfo>.Instance.GetInfo(info.occupationId);
        Attribute.Skills = occpInfo.skillId;
        Attribute.Score = 0;
        Attribute.Level = 0;
    }

    public void DespawnerParticle(EffectType type)
    {
        switch (type)
        {
            case EffectType.ACCELERATE:
                DespawnerAccelerateParticle();
                break;
            case EffectType.WALKINSTANT:
                DespawnerWalkInstanceParticle();
                break;
        }
    }

    //特效加载
    public void SpawnerParticle(int id, EffectType type, Vector3 v, Transform parent)
    {
        DespawnerParticle(type);
        GameObject inst = ParticleMgr.Instance.Spawner(id, v, parent);
        if (null != inst)
        {
            switch (type)
            {
                case EffectType.ACCELERATE:
                    CacheAccelParticleTran = inst.transform;
                    break;
                case EffectType.WALKINSTANT:
                    CacheSkillParticleTran = inst.transform;
                    break;
            }
        }
    }

    void DespawnerWalkInstanceParticle()
    {
        if (null != CacheSkillParticleTran)
        {
            ParticleMgr.Instance.Despawner(ResourceType.RESOURCE_PARTICLE, CacheSkillParticleTran);
            CacheSkillParticleTran.parent = GameMgr.Instance.ParticleRoot;
            CacheSkillParticleTran = null;
        }
    }

    void DespawnerAccelerateParticle()
    {
        if (null != CacheAccelParticleTran)
        {
            ParticleMgr.Instance.Despawner(ResourceType.RESOURCE_PARTICLE, CacheAccelParticleTran);
            CacheAccelParticleTran.parent = GameMgr.Instance.ParticleRoot;
            CacheAccelParticleTran = null;
        }
    }

    //积分变换，体力变换
    public void EnergyUpdate(ItemEffectInfo effect)
    {
        Attribute.Score = Attribute.Score + effect.param1;
        Attribute.CurPhy = Attribute.CurPhy + effect.param2;
        if (Attribute.CurPhy > Attribute.MaxPhy)
        {
            Attribute.CurPhy = Attribute.MaxPhy;
        }
        Attribute.Level = GetCurrentLevel();
    }

    //等级变化
    public int GetCurrentLevel()
    {
        LevelInfo info = InfoMgr<LevelInfo>.Instance.GetInfo(Attribute.Level);
        Dictionary<int, LevelInfo> level = InfoMgr<LevelInfo>.Instance.Dict;
        int lev = 0;
        foreach (KeyValuePair<int, LevelInfo> kv in level)
        {
            if (Attribute.Score >= kv.Value.score)
            {
                lev = Mathf.Max(lev, kv.Value.id);
            }
            else
            {
                break;
            }
        }
        Debuger.LogError("Level:" + lev);
        return lev;
    }

    public void UpdateState(StateType stateType, ItemInfo item, ItemEffectInfo effect)
    {
        this.State = stateType;
        switch (stateType)
        {
            case StateType.STATE_MARK:
                //TurnedBody(item);
                break;
            case StateType.STATE_MAGNET:
                Magnet(item, effect);
                break;
            case StateType.STATE_TRANSFERGATE:
                Transfer();
                break;
            case StateType.STATE_SPEED:
                ChangeSpeed(item, effect);
                break;
            case StateType.STATE_PROTECT:
                Protect();
                break;
        }
    }

    //取消变身
    void BackBody(Timer.TimerData data)
    {
        Debug.LogError("返回变身效果");
        HeroInfo heroInfo = InfoMgr<HeroInfo>.Instance.GetInfo(HeroId);
        GameObject go = ResourcesMgr.Instance.Spawner(heroInfo.model, ResourceType.RESOURCE_ENTITY, CacheModel);// ResourcesMgr.Instance.Instantiate(prefab);
        if (null == go)
        {
            return;
        }
        ResourcesMgr.Instance.Despawner(ResourceType.RESOURCE_ENTITY, RoleModel);
        InitCharactor(go);
    }


    void TurnedBody(ItemInfo item)
    {
        Debug.LogError("变身效果");
        HeroInfo heroInfo = InfoMgr<HeroInfo>.Instance.GetInfo(4);
        GameObject go = ResourcesMgr.Instance.Spawner(heroInfo.model, ResourceType.RESOURCE_ENTITY, CacheModel);// ResourcesMgr.Instance.Instantiate(prefab);
        if (null == go)
        {
            return;
        }
        ResourcesMgr.Instance.Despawner(ResourceType.RESOURCE_ENTITY, RoleModel);
        InitCharactor(go);
        Timer.Instance.AddTimer(item.timer, 1, true, BackBody);
    }

    //吸鐵石
    void Magnet(ItemInfo item, ItemEffectInfo effect)
    {
        Debug.LogError("吸铁石");
        float rate = (float)effect.param1 / AppConst.factor;
        this.Attribute.Atkdis = this.Attribute.Basedis * rate;
    }

    void Transfer()
    {
        Debug.LogError("传送门");
    }

    void ChangeSpeed(ItemInfo item, ItemEffectInfo effect)
    {
        Debug.LogError("速度变化");
        //加速道具 停止使用加速功能
        this.Attribute.Speed = Attribute.BaseSpeed * effect.param1 / AppConst.factor;
        Timer.Instance.AddTimer(item.timer, 1, true, BackSpeed);
    }

    void BackSpeed(Timer.TimerData data)
    {
        //发送事件 可以使用加速功能
        this.Attribute.Speed = Attribute.BaseSpeed;
    }

    void Protect()
    {
        Debug.LogError("保护时间");
    }

    void DetectItem()
    {
        foreach (KeyValuePair<int, DropItemInfo> kv in TSCData.Instance.DropItemDic)
        {
            if (kv.Value.IsLock)
            {
                continue;
            }
            if (Util.PtInCircleArea(kv.Value.Cache, CacheModel, Attribute.Atkdis))
            {
                kv.Value.FlyToEntity(this);
            }
        }
    }

    void DetectObstacle()
    {
        foreach (KeyValuePair<int, ObstacleEntity> kv in TSCData.Instance.ObstacleDic)
        {
            if (Util.PtInRectArea(CacheModel, kv.Value.Cache, kv.Value.Width + Attribute.Atkdis, kv.Value.Height + Attribute.Atkdis))
            {
                if (null != ArpgAnimatContorller)
                {
                    if (ArpgAnimatContorller.Skill == 1)
                    {
                        StopSkill(CollisionType.Collision_OBSTACLE);
                    }
                }
                Vector3 dir =  CacheModel.position - kv.Value.Cache.position;
                CacheModel.forward = dir;//CacheModel.forward * -1;
            }
        }
    }

    public void StopSkill(CollisionType type)
    {
        collision = type;
        Timer.Instance.RemoveTimer(TimerWalkInstantHandler);
        TimerWalkInstantHandler(null);
        collision = CollisionType.NONE;
    }


    void TimerWalkInstantHandler(Timer.TimerData data)
    {
        Attribute.Speed = Attribute.BaseSpeed;
        ArpgAnimatContorller.Skill = 0;
        DespawnerParticle(EffectType.WALKINSTANT);

        switch (collision)
        {
            case CollisionType.Collision_NET:
                Debug.LogError("碰到玩家了");
                break;
            case CollisionType.Collision_ITEM:
                Debug.LogError("碰到道具了");
                break;
            case CollisionType.Collision_OBSTACLE:
                Debug.LogError("碰到障碍物了");
                break;
            case CollisionType.NONE:
                Debug.LogError("自己停止了");
                OccpType occp = Util.GetNextOccp(occupation);
                int id = Util.GetHeroIdByOccp(occp);
                ChangeOccp(occp, id);
                break;
        }
    }

    public void StopAccelerate()
    {
        Attribute.Speed = GameMgr.Instance.MainEntity.Attribute.BaseSpeed;
        DespawnerParticle(EffectType.ACCELERATE);
    }

    //加速
    public void Accelerate()
    {
        if (null == skill1 || null == effect1)
        {
            return;
        }
        //加速停止恢复能量条
        Attribute.Speed = Attribute.BaseSpeed * effect1.param / AppConst.factor;
        SpawnerParticle(skill1.particleID, EffectType.ACCELERATE, CacheParticleParent.position, CacheParticleParent);
    }

    //冲锋
    public void Walkinstant()
    {
        if (null == skill2 || null == effect2)
        {
            return;
        }

        ArpgAnimatContorller.Skill = 1;
        Attribute.Speed = Attribute.BaseSpeed * effect2.param / AppConst.factor;
        SpawnerParticle(skill2.particleID, EffectType.WALKINSTANT, CacheParticleParent.position, CacheParticleParent);
        Timer.Instance.AddTimer((float)effect2.keeptime / AppConst.factor, 1, true, TimerWalkInstantHandler);
    }

    public void BeKilled()
    {
        Debug.LogError("BeKilled...");
        NpcControl.SetTransition(Transition.Dead, this);
    }

    public void Dead()
    {
        Attribute.Hp = 0;
        StopAccelerate();
        StopSkill(CollisionType.Collision_NOTHING);
        ArpgAnimatContorller.Reset();
        ArpgAnimatContorller.Die = true;
        TSCData.Instance.EntityDic.Remove(Id);
        Timer.Instance.AddTimer(2, 1, true, RemoveBody);
    }

    void RemoveBody(Timer.TimerData data)
    {
        ResourcesMgr.Instance.Despawner(ResourceType.RESOURCE_ENTITY, RoleModel);
        ResourcesMgr.Instance.Despawner(ResourceType.RESOURCE_NET, CacheModel);
    }


    //停止逃跑到walk
    void EndFleeStateToWalk(Timer.TimerData data)
    {
        if(IsAlive)
        {
            NpcControl.SetTransition(Transition.FreeWalk, this);
        }
    }

    //加速技能使用间隔10S
    private float lastAcceTime = 10;
    private float lastSkillTime = 10;
    private float m_judgeTime = 0;
    private float m_judgeInter = 2;
    private void JudgeEntity()
    {
        m_judgeTime += Time.deltaTime;
        lastAcceTime += Time.deltaTime;
        if (m_judgeTime < m_judgeInter)
        {
            return;
        }

        m_judgeTime = 0;

        if(!GameMgr.Instance.MainEntity.IsAlive)
        {
            return;
        }


        //判断主角是否能KILL AI
        bool kill = Util.CanKillBody(GameMgr.Instance.MainEntity.Occupation, Occupation);
        if (kill)
        {
            if (Util.GetEntityDistance(GameMgr.Instance.MainEntity.CacheModel, CacheModel) < AppConst.FleeDis)
            {
                Vector3 dir = CacheModel.position - GameMgr.Instance.MainEntity.CacheModel.position;
                CacheModel.forward = dir;

                if (lastAcceTime > AppConst.AcctInterval)
                {
                    Debug.LogError(Id +"逃离");
                    lastAcceTime = 0;
                    NpcControl.SetTransition(Transition.Acct, this);
                    int accTime = Random.Range(3, 8);
                    Timer.Instance.AddTimer(accTime, 1, true, EndFleeStateToWalk);
                }
            }
        }
        else
        {
            //if(Util.CanKillBody(Occupation, GameMgr.Instance.MainEntity.Occupation))
            //{
            //    if (Util.GetEntityDistance(GameMgr.Instance.MainEntity.CacheModel, CacheModel) < AppConst.ChaseDis)
            //    {
            //        //能被主角杀死，逃离
            //        Vector3 dir = CacheModel.position - GameMgr.Instance.MainEntity.CacheModel.position;
            //        CacheModel.forward = dir;

            //        if (lastAcceTime > AppConst.AcctInterval)
            //        {
            //            lastAcceTime = 0;
            //            NpcControl.SetTransition(Transition.Acct, this);
            //            int accTime = Random.Range(3, 8);
            //            Timer.Instance.AddTimer(accTime, 1, true, EndFleeStateToWalk);
            //        }
            //    }
            //}else
            //{

            //}


            foreach (KeyValuePair<int, NetEntity> kv in TSCData.Instance.EntityDic)
            {
                if(kv.Value.Id == Id)
                {
                    continue;
                }

                if(!kv.Value.IsAlive)
                {
                    continue;
                }

                bool k = Util.CanKillBody(kv.Value.Occupation, Occupation);
                if (k)
                {
                    if (Util.GetEntityDistance(kv.Value.CacheModel, CacheModel) < AppConst.FleeDis)
                    {
                        //能被主角杀死，逃离
                        Vector3 dir = CacheModel.position - GameMgr.Instance.MainEntity.CacheModel.position;
                        CacheModel.forward = dir;
                        if (lastAcceTime > AppConst.AcctInterval)
                        {
                            Debug.LogError("逃离");
                            lastAcceTime = 0;
                            NpcControl.SetTransition(Transition.Acct, this);
                            int accTime = Random.Range(3, 8);
                            Timer.Instance.AddTimer(accTime, 1, true, EndFleeStateToWalk);
                        }
                        break;
                    }
                }
            }
        }
    }


    //else
    //            {
    //                if(Util.CanKillBody(Occupation, kv.Value.Occupation))
    //                {
    //                    if (Util.GetEntityDistance(kv.Value.CacheModel, CacheModel) < AppConst.ChaseDis)
    //                    {
    //                        //专项追击释放技能
    //                        Vector3 dir = kv.Value.CacheModel.position - CacheModel.position;
    //CacheModel.forward = dir;

    //                        if (lastSkillTime > AppConst.SkillInterval)
    //                        {
    //                            lastSkillTime = 0;
    //                            NpcControl.SetTransition(Transition.Skill, this);
    //}
    //                        break;
    //                    }
    //                }
    //            }

    private void OnDestroy()
    {
        DespawnerAccelerateParticle();
        DespawnerWalkInstanceParticle();
        TSCData.Instance.EntityDic.Remove(Id);
    }


    //先用物理引擎去检测碰撞,效率低再优化
    float mTotalTime = 0;
    void Update()
    {
        if (!init)
        {
            return;
        }

        if(!IsAlive)
        {
            return;
        }

        JudgeEntity();

        mTotalTime += Time.deltaTime;
        if (mTotalTime <= 0.2f)
        {
            return;
        }

        mTotalTime = 0;

        DetectItem();

        DetectObstacle();

    }
}
