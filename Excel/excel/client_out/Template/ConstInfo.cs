using System;
using System.IO;
using System.Collections.Generic;
using System.Text;


// 常量表枚举类型
public enum ConstType
{
    NONE = 0,  //
    CONST_SPEED = 1,  //移动基本速度
    CONST_MAX_PHY = 2,  //体力最大值
    CONST_COST_PHY_SPEED = 3,  //体力每秒消耗值
    CONST_ATK_RADIO = 4,  //碰撞范围半径
    CONST_REBORN = 5,  //复活等待时间
};

[Serializable]
public class ConstInfo: IInfo
{
    public int id; // 编号
    public string data; // 数据

    public  void Load(BinaryReader reader)
    {
        id = reader.ReadInt32();
 
        int dataLen = reader.ReadInt32();
        data = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(dataLen));
 
    }

    public  int GetKey()
    {
        return id;
    }

}
