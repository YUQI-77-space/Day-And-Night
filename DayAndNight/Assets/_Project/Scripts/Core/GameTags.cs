using UnityEngine;

/// <summary>
/// 游戏标签常量类 - 集中管理所有游戏对象的Tag
/// 放置位置：Assets/_Project/Scripts/Core/GameTags.cs
/// 
/// 使用方法：
/// if (.CompareTag(GameTags.Player)) { ... }
/// </summary>
public static class GameTags
{
    // ========== 玩家相关 ==========
    /// <summary>玩家</summary>
    public const string Player = "Player";
    
    /// <summary>玩家身体部位（用于伤害检测）</summary>
    public const string PlayerBody = "PlayerBody";
    
    /// <summary>玩家武器</summary>
    public const string PlayerWeapon = "PlayerWeapon";

    // ========== 敌人相关 ==========
    /// <summary>敌人</summary>
    public const string Enemy = "Enemy";
    
    /// <summary>BOSS敌人</summary>
    public const string Boss = "Boss";
    
    /// <summary>小怪敌人</summary>
    public const string Minion = "Minion";

    // ========== 环境相关 ==========
    /// <summary>地面</summary>
    public const string Ground = "Ground";
    
    /// <summary>墙壁</summary>
    public const string Wall = "Wall";
    
    /// <summary>天花板</summary>
    public const string Ceiling = "Ceiling";
    
    /// <summary>平台</summary>
    public const string Platform = "Platform";
    
    /// <summary>水</summary>
    public const string Water = "Water";
    
    /// <summary>岩浆</summary>
    public const string Lava = "Lava";

    // ========== 物品相关 ==========
    /// <summary>可收集物品</summary>
    public const string Collectible = "Collectible";
    
    /// <summary>金币</summary>
    public const string Coin = "Coin";
    
    /// <summary>生命恢复道具</summary>
    public const string HealthItem = "HealthItem";
    
    /// <summary>魔法恢复道具</summary>
    public const string ManaItem = "ManaItem";
    
    /// <summary>武器</summary>
    public const string Weapon = "Weapon";
    
    /// <summary>防具</summary>
    public const string Armor = "Armor";

    // ========== 交互相关 ==========
    /// <summary>可交互物体</summary>
    public const string Interactable = "Interactable";
    
    /// <summary>商店</summary>
    public const string Shop = "Shop";
    
    /// <summary>宝箱</summary>
    public const string Chest = "Chest";
    
    /// <summary>传送门</summary>
    public const string Portal = "Portal";
    
    /// <summary>NPC</summary>
    public const string NPC = "NPC";

    // ========== 特效相关 ==========
    /// <summary>特效</summary>
    public const string Effect = "Effect";
    
    /// <summary>粒子系统</summary>
    public const string Particle = "Particle";
    
    /// <summary>投射物</summary>
    public const string Projectile = "Projectile";

    // ========== 陷阱相关 ==========
    /// <summary>陷阱</summary>
    public const string Trap = "Trap";
    
    /// <summary>尖刺</summary>
    public const string Spike = "Spike";
    
    /// <summary>地雷</summary>
    public const string LandMine = "LandMine";

    // ========== UI相关 ==========
    /// <summary>UI元素</summary>
    public const string UI = "UI";
    
    /// <summary>可点击UI</summary>
    public const string ClickableUI = "ClickableUI";

    // ========== 车辆/坐骑 ==========
    /// <summary>坐骑</summary>
    public const string Mount = "Mount";
    
    /// <summary>车辆</summary>
    public const string Vehicle = "Vehicle";

    // ========== 公开方法 ==========
    /// <summary>
    /// 检查标签是否属于玩家相关
    /// </summary>
    public static bool IsPlayerTag(string tag)
    {
        return tag == Player || tag == PlayerBody || tag == PlayerWeapon;
    }

    /// <summary>
    /// 检查标签是否属于敌人
    /// </summary>
    public static bool IsEnemyTag(string tag)
    {
        return tag == Enemy || tag == Boss || tag == Minion;
    }

    /// <summary>
    /// 检查标签是否属于环境
    /// </summary>
    public static bool IsEnvironmentTag(string tag)
    {
        return tag == Ground || tag == Wall || tag == Ceiling || 
               tag == Platform || tag == Water || tag == Lava;
    }

    /// <summary>
    /// 检查标签是否属于危险物体（会造成伤害）
    /// </summary>
    public static bool IsDangerousTag(string tag)
    {
        return tag == Enemy || tag == Boss || tag == Trap || 
               tag == Spike || tag == LandMine || tag == Lava;
    }

    /// <summary>
    /// 获取所有玩家相关标签的数组
    /// </summary>
    public static string[] GetPlayerTags()
    {
        return new string[] { Player, PlayerBody, PlayerWeapon };
    }

    /// <summary>
    /// 获取所有敌人标签的数组
    /// </summary>
    public static string[] GetEnemyTags()
    {
        return new string[] { Enemy, Boss, Minion };
    }

    /// <summary>
    /// 获取所有环境碰撞标签的数组
    /// </summary>
    public static string[] GetEnvironmentTags()
    {
        return new string[] { Ground, Wall, Ceiling, Platform };
    }
}
