using UnityEngine;

/// <summary>
/// 游戏图层常量类 - 集中管理所有物理碰撞图层
/// 放置位置：Assets/_Project/Scripts/Core/GameLayers.cs
/// 
/// 使用方法：
/// LayerMask mask = GameLayers.PlayerLayer;
/// gameObject.layer = GameLayers.Ground;
/// </summary>
public static class GameLayers
{
    // ========== 玩家图层 ==========
    /// <summary>玩家图层（整数：6）</summary>
    public const int Player = 6;
    /// <summary>玩家图层名称</summary>
    public const string PlayerLayerName = "Player";
    /// <summary>玩家图层掩码</summary>
    public static readonly LayerMask PlayerLayer = 1 << Player;

    // ========== 敌人图层 ==========
    /// <summary>敌人图层（整数：7）</summary>
    public const int Enemy = 7;
    /// <summary>敌人图层名称</summary>
    public const string EnemyLayerName = "Enemy";
    /// <summary>敌人图层掩码</summary>
    public static readonly LayerMask EnemyLayer = 1 << Enemy;

    // ========== 环境图层 ==========
    /// <summary>地面图层（整数：8）</summary>
    public const int Ground = 8;
    /// <summary>地面图层名称</summary>
    public const string GroundLayerName = "Ground";
    /// <summary>地面图层掩码</summary>
    public static readonly LayerMask GroundLayer = 1 << Ground;

    /// <summary>墙壁图层（整数：9）</summary>
    public const int Wall = 9;
    /// <summary>墙壁图层名称</summary>
    public const string WallLayerName = "Wall";
    /// <summary>墙壁图层掩码</summary>
    public static readonly LayerMask WallLayer = 1 << Wall;

    /// <summary>平台图层（整数：10）</summary>
    public const int Platform = 10;
    /// <summary>平台图层名称</summary>
    public const string PlatformLayerName = "Platform";
    /// <summary>平台图层掩码</summary>
    public static readonly LayerMask PlatformLayer = 1 << Platform;

    /// <summary>水图层（整数：11）</summary>
    public const int Water = 11;
    /// <summary>水图层名称</summary>
    public const string WaterLayerName = "Water";
    /// <summary>水图层掩码</summary>
    public static readonly LayerMask WaterLayer = 1 << Water;

    /// <summary>岩浆图层（整数：12）</summary>
    public const int Lava = 12;
    /// <summary>岩浆图层名称</summary>
    public const string LavaLayerName = "Lava";
    /// <summary>岩浆图层掩码</summary>
    public static readonly LayerMask LavaLayer = 1 << Lava;

    // ========== 物品图层 ==========
    /// <summary>可收集物品图层（整数：13）</summary>
    public const int Collectible = 13;
    /// <summary>可收集物品图层名称</summary>
    public const string CollectibleLayerName = "Collectible";
    /// <summary>可收集物品图层掩码</summary>
    public static readonly LayerMask CollectibleLayer = 1 << Collectible;

    /// <summary>金币图层（整数：14）</summary>
    public const int Coin = 14;
    /// <summary>金币图层名称</summary>
    public const string CoinLayerName = "Coin";
    /// <summary>金币图层掩码</summary>
    public static readonly LayerMask CoinLayer = 1 << Coin;

    // ========== 特效图层 ==========
    /// <summary>投射物图层（整数：15）</summary>
    public const int Projectile = 15;
    /// <summary>投射物图层名称</summary>
    public const string ProjectileLayerName = "Projectile";
    /// <summary>投射物图层掩码</summary>
    public static readonly LayerMask ProjectileLayer = 1 << Projectile;

    /// <summary>陷阱图层（整数：16）</summary>
    public const int Trap = 16;
    /// <summary>陷阱图层名称</summary>
    public const string TrapLayerName = "Trap";
    /// <summary>陷阱图层掩码</summary>
    public static readonly LayerMask TrapLayer = 1 << Trap;

    // ========== UI图层 ==========
    /// <summary>UI图层（整数：17）</summary>
    public const int UI = 17;
    /// <summary>UI图层名称</summary>
    public const string UILayerName = "UI";
    /// <summary>UI图层掩码</summary>
    public static readonly LayerMask UILayer = 1 << UI;

    // ========== 装饰图层 ==========
    /// <summary>背景装饰图层（整数：18）</summary>
    public const int Background = 18;
    /// <summary>背景装饰图层名称</summary>
    public const string BackgroundLayerName = "Background";
    /// <summary>背景装饰图层掩码</summary>
    public static readonly LayerMask BackgroundLayer = 1 << Background;

    /// <summary>前景装饰图层（整数：19）</summary>
    public const int Foreground = 19;
    /// <summary>前景装饰图层名称</summary>
    public const string ForegroundLayerName = "Foreground";
    /// <summary>前景装饰图层掩码</summary>
    public static readonly LayerMask ForegroundLayer = 1 << Foreground;

    // ========== 组合图层掩码 ==========
    /// <summary>所有碰撞环境图层（地面+墙壁+平台）</summary>
    public static readonly LayerMask EnvironmentLayers = GroundLayer | WallLayer | PlatformLayer;

    /// <summary>所有危险图层（水+岩浆+陷阱）</summary>
    public static readonly LayerMask DangerLayers = WaterLayer | LavaLayer | TrapLayer;

    /// <summary>所有可收集图层（物品+金币）</summary>
    public static readonly LayerMask CollectibleLayers = CollectibleLayer | CoinLayer;

    /// <summary>所有敌人图层</summary>
    public static readonly LayerMask EnemyLayers = EnemyLayer;

    /// <summary>所有玩家检测图层（用于敌人AI）</summary>
    public static readonly LayerMask PlayerDetectionLayers = PlayerLayer;

    // ========== 公开方法 ==========
    /// <summary>
    /// 将图层名称转换为整数
    /// </summary>
    public static int NameToLayer(string layerName)
    {
        return LayerMask.NameToLayer(layerName);
    }

    /// <summary>
    /// 将整数转换为图层名称
    /// </summary>
    public static string LayerToName(int layer)
    {
        return LayerMask.LayerToName(layer);
    }

    /// <summary>
    /// 检查物体是否在指定图层
    /// </summary>
    public static bool IsInLayer(GameObject obj, LayerMask layerMask)
    {
        return (layerMask & (1 << obj.layer)) != 0;
    }

    /// <summary>
    /// 获取图层的值（用于物理检测）
    /// </summary>
    public static int GetLayerValue(int layer)
    {
        return 1 << layer;
    }

    /// <summary>
    /// 创建图层掩码（支持多个图层）
    /// </summary>
    public static LayerMask CreateMask(params int[] layers)
    {
        LayerMask mask = 0;
        foreach (int layer in layers)
        {
            mask |= (1 << layer);
        }
        return mask;
    }

    /// <summary>
    /// 检查是否是玩家图层
    /// </summary>
    public static bool IsPlayerLayer(int layer)
    {
        return layer == Player;
    }

    /// <summary>
    /// 检查是否是环境图层
    /// </summary>
    public static bool IsEnvironmentLayer(int layer)
    {
        return layer == Ground || layer == Wall || layer == Platform;
    }

    /// <summary>
    /// 检查是否是危险图层
    /// </summary>
    public static bool IsDangerLayer(int layer)
    {
        return layer == Water || layer == Lava || layer == Trap;
    }

    /// <summary>
    /// 获取所有环境图层名称的数组
    /// </summary>
    public static string[] GetEnvironmentLayerNames()
    {
        return new string[] { GroundLayerName, WallLayerName, PlatformLayerName };
    }

    /// <summary>
    /// 获取所有碰撞层名称的数组（用于Physics2D检测）
    /// </summary>
    public static string[] GetCollisionLayerNames()
    {
        return new string[]
        {
            PlayerLayerName, EnemyLayerName, GroundLayerName, WallLayerName,
            PlatformLayerName, WaterLayerName, LavaLayerName, CollectibleLayerName,
            CoinLayerName, ProjectileLayerName, TrapLayerName
        };
    }
}
