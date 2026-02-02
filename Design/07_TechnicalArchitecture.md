# 技术架构设计

## 技术栈概览

### 核心技术

**游戏引擎：** Unity 2022.3 LTS
- 稳定的长期支持版本
- 2D工具链完善
- 跨平台支持
- 丰富的资源和社区

**编程语言：** C#
- Unity原生支持
- 强类型语言，易于维护
- 丰富的库和工具

**版本控制：** Git + GitHub
- 分布式版本控制
- 代码托管和协作
- Issue追踪和项目管理

### 开发工具

**IDE：** Visual Studio 2022 / Rider
**美术工具：** Aseprite（像素绘制）
**地图工具：** Unity Tilemap
**对话系统：** Yarn Spinner
**音频工具：** Audacity
**文档工具：** Markdown

---

## 项目结构

### Unity项目目录结构

```
DayAndNight/
├── Assets/
│   ├── _Project/                    # 项目核心资源
│   │   ├── Scripts/                 # 所有脚本
│   │   │   ├── Core/               # 核心系统
│   │   │   ├── Gameplay/           # 游戏玩法
│   │   │   ├── UI/                 # UI系统
│   │   │   ├── Data/               # 数据结构
│   │   │   └── Utilities/          # 工具类
│   │   │
│   │   ├── Scenes/                 # 场景文件
│   │   │   ├── Main.unity          # 主场景
│   │   │   ├── Town/               # 小镇场景
│   │   │   └── Combat/             # 战斗场景
│   │   │
│   │   ├── Prefabs/                # 预制体
│   │   │   ├── Characters/         # 角色
│   │   │   ├── Items/              # 物品
│   │   │   ├── UI/                 # UI元素
│   │   │   └── Environment/        # 环境物体
│   │   │
│   │   ├── Art/                    # 美术资源
│   │   │   ├── Sprites/            # 精灵图
│   │   │   ├── Animations/         # 动画
│   │   │   ├── Tilesets/           # 瓦片集
│   │   │   └── UI/                 # UI图片
│   │   │
│   │   ├── Audio/                  # 音频资源
│   │   │   ├── Music/              # 音乐
│   │   │   ├── SFX/                # 音效
│   │   │   └── Ambient/            # 环境音
│   │   │
│   │   ├── Data/                   # 数据文件
│   │   │   ├── Config/             # 配置文件
│   │   │   ├── Dialogues/          # 对话文件
│   │   │   └── Localization/       # 本地化
│   │   │
│   │   └── Resources/              # 动态加载资源
│   │
│   ├── Plugins/                    # 第三方插件
│   └── Settings/                   # 项目设置
│
├── Packages/                       # Package Manager包
├── ProjectSettings/                # Unity项目设置
└── UserSettings/                   # 用户设置（不提交）
```

---

## 核心架构设计

### 架构模式

采用 **MVC + ECS 混合架构**：

**MVC（Model-View-Controller）：**
- Model：数据层，管理游戏状态
- View：表现层，UI和渲染
- Controller：逻辑层，处理输入和业务逻辑

**ECS（Entity-Component-System）：**
- 用于游戏对象管理
- 高性能的数据驱动设计
- 易于扩展和维护

### 系统分层

```
┌─────────────────────────────────────┐
│         表现层 (Presentation)        │
│    UI / 动画 / 音效 / 粒子效果        │
└─────────────────────────────────────┘
              ↕
┌─────────────────────────────────────┐
│         逻辑层 (Logic)               │
│    游戏系统 / 状态机 / AI / 物理      │
└─────────────────────────────────────┘
              ↕
┌─────────────────────────────────────┐
│         数据层 (Data)                │
│    数据模型 / 配置 / 存档 / 资源      │
└─────────────────────────────────────┘
```

---

## 核心系统设计


### 1. 游戏管理器（GameManager）

**职责：**
- 游戏生命周期管理
- 场景切换
- 全局状态管理
- 系统初始化

**单例模式：**
```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public GameState CurrentState { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Initialize()
    {
        // 初始化各个系统
        TimeManager.Instance.Initialize();
        AttributeManager.Instance.Initialize();
        // ...
    }
}
```

### 2. 时间管理器（TimeManager）

**职责：**
- 游戏时间流逝
- 昼夜循环
- 时间事件触发
- 时间缩放

**核心功能：**
```csharp
public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }
    
    // 游戏时间
    public int CurrentDay { get; private set; }
    public int CurrentHour { get; private set; }
    public int CurrentMinute { get; private set; }
    
    // 时间流速（1现实秒 = X游戏分钟）
    public float TimeScale = 1.0f;
    
    // 事件
    public event Action OnDayChanged;
    public event Action OnHourChanged;
    public event Action<bool> OnDayNightChanged; // true=白天, false=夜晚
    
    // 时间控制
    public void AdvanceTime(int minutes) { }
    public void PauseTime() { }
    public void ResumeTime() { }
    public bool IsDay() { return CurrentHour >= 6 && CurrentHour < 18; }
}
```

### 3. 属性管理器（AttributeManager）

**职责：**
- 管理六维属性
- 属性增长和计算
- 属性效果应用
- 等级系统

**数据结构：**
```csharp
public class AttributeManager : MonoBehaviour
{
    public static AttributeManager Instance { get; private set; }
    
    // 六维属性
    public Attribute Strength { get; private set; }
    public Attribute Skill { get; private set; }
    public Attribute Intelligence { get; private set; }
    public Attribute Charisma { get; private set; }
    public Attribute Courage { get; private set; }
    public Attribute Wealth { get; private set; }
    
    // 属性变化事件
    public event Action<AttributeType, int> OnAttributeChanged;
    
    // 方法
    public void AddAttributeExp(AttributeType type, int exp) { }
    public int GetAttributeLevel(AttributeType type) { }
    public bool CheckAttributeRequirement(AttributeType type, int required) { }
}

[System.Serializable]
public class Attribute
{
    public int Level;
    public int CurrentExp;
    public int ExpToNextLevel;
    
    public void AddExp(int exp) { }
    public float GetEffectMultiplier() { }
}
```

### 4. 战斗管理器（CombatManager）

**职责：**
- 战斗流程控制
- 伤害计算
- 战斗状态管理
- 敌人生成

**核心功能：**
```csharp
public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }
    
    // 战斗状态
    public bool IsInCombat { get; private set; }
    public List<Enemy> ActiveEnemies { get; private set; }
    
    // 战斗事件
    public event Action OnCombatStart;
    public event Action OnCombatEnd;
    public event Action<Enemy> OnEnemyDefeated;
    
    // 方法
    public void StartCombat(CombatConfig config) { }
    public void EndCombat() { }
    public void SpawnEnemy(EnemyData data, Vector3 position) { }
    public int CalculateDamage(AttackData attack, DefenseData defense) { }
}
```

### 5. NPC管理器（NPCManager）

**职责：**
- NPC生命周期管理
- 好感度系统
- NPC日程管理
- 对话触发

**核心功能：**
```csharp
public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance { get; private set; }
    
    // NPC数据
    private Dictionary<string, NPCData> npcDatabase;
    private Dictionary<string, int> npcAffection; // 好感度
    
    // 方法
    public NPCData GetNPC(string npcId) { }
    public void AddAffection(string npcId, int amount) { }
    public int GetAffection(string npcId) { }
    public NPCSchedule GetCurrentSchedule(string npcId) { }
    public Vector3 GetNPCLocation(string npcId) { }
}
```

### 6. 任务管理器（QuestManager）

**职责：**
- 任务状态管理
- 任务进度追踪
- 任务奖励发放
- 任务触发条件检查

**核心功能：**
```csharp
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }
    
    // 任务数据
    private Dictionary<string, Quest> activeQuests;
    private List<string> completedQuests;
    
    // 事件
    public event Action<Quest> OnQuestAccepted;
    public event Action<Quest> OnQuestCompleted;
    public event Action<Quest, QuestObjective> OnObjectiveCompleted;
    
    // 方法
    public void AcceptQuest(string questId) { }
    public void CompleteQuest(string questId) { }
    public void UpdateObjective(string questId, string objectiveId, int progress) { }
    public bool IsQuestAvailable(string questId) { }
    public List<Quest> GetAvailableQuests() { }
}
```

### 7. 物品管理器（InventoryManager）

**职责：**
- 背包系统
- 物品增删
- 装备管理
- 物品使用

**核心功能：**
```csharp
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    
    // 背包数据
    public int MaxSlots = 50;
    private List<ItemStack> items;
    
    // 装备槽
    public Equipment EquippedWeapon { get; private set; }
    public Equipment EquippedArmor { get; private set; }
    public Equipment EquippedAccessory { get; private set; }
    
    // 事件
    public event Action<ItemStack> OnItemAdded;
    public event Action<ItemStack> OnItemRemoved;
    public event Action<Equipment> OnEquipmentChanged;
    
    // 方法
    public bool AddItem(ItemData item, int count = 1) { }
    public bool RemoveItem(string itemId, int count = 1) { }
    public void UseItem(string itemId) { }
    public void EquipItem(Equipment equipment) { }
    public int GetItemCount(string itemId) { }
}
```

### 8. 存档管理器（SaveManager）

**职责：**
- 游戏存档
- 游戏读档
- 自动保存
- 多存档槽

**核心功能：**
```csharp
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    
    // 存档数据
    private SaveData currentSave;
    
    // 方法
    public void SaveGame(int slot) { }
    public void LoadGame(int slot) { }
    public void AutoSave() { }
    public bool HasSaveData(int slot) { }
    public SaveData GetSaveData(int slot) { }
    
    // 存档数据结构
    [System.Serializable]
    public class SaveData
    {
        public int saveSlot;
        public string saveName;
        public DateTime saveTime;
        
        // 玩家数据
        public PlayerData playerData;
        public AttributeData attributeData;
        public InventoryData inventoryData;
        
        // 世界数据
        public TimeData timeData;
        public NPCData[] npcData;
        public QuestData[] questData;
        public WorldStateData worldData;
    }
}
```

### 9. 事件管理器（EventManager）

**职责：**
- 全局事件系统
- 事件订阅和发布
- 解耦系统间通信

**核心功能：**
```csharp
public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }
    
    private Dictionary<string, Action<object>> eventDictionary;
    
    public void Subscribe(string eventName, Action<object> listener) { }
    public void Unsubscribe(string eventName, Action<object> listener) { }
    public void Publish(string eventName, object data = null) { }
}

// 使用示例
// 订阅：EventManager.Instance.Subscribe("OnPlayerDeath", HandlePlayerDeath);
// 发布：EventManager.Instance.Publish("OnPlayerDeath", playerData);
```

### 10. 音频管理器（AudioManager）

**职责：**
- 音乐播放
- 音效播放
- 音量控制
- 音频淡入淡出

**核心功能：**
```csharp
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    // 音频源
    private AudioSource musicSource;
    private AudioSource ambientSource;
    private List<AudioSource> sfxSources;
    
    // 音量设置
    public float MasterVolume { get; set; }
    public float MusicVolume { get; set; }
    public float SFXVolume { get; set; }
    
    // 方法
    public void PlayMusic(AudioClip clip, bool loop = true) { }
    public void PlaySFX(AudioClip clip, float volume = 1.0f) { }
    public void PlaySFXAtPosition(AudioClip clip, Vector3 position) { }
    public void FadeMusic(float targetVolume, float duration) { }
    public void StopMusic() { }
}
```

---

## 数据驱动设计

### ScriptableObject 数据配置

**优势：**
- 可视化编辑
- 易于维护
- 支持热更新
- 减少硬编码

### 核心数据类型

#### 1. 物品数据（ItemData）

```csharp
[CreateAssetMenu(fileName = "New Item", menuName = "Game/Item")]
public class ItemData : ScriptableObject
{
    [Header("基础信息")]
    public string itemId;
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;
    
    [Header("属性")]
    public ItemType type;
    public ItemRarity rarity;
    public int maxStack = 99;
    public int value;
    
    [Header("效果")]
    public ItemEffect[] effects;
    
    public virtual void Use() { }
}
```

#### 2. 敌人数据（EnemyData）

```csharp
[CreateAssetMenu(fileName = "New Enemy", menuName = "Game/Enemy")]
public class EnemyData : ScriptableObject
{
    [Header("基础信息")]
    public string enemyId;
    public string enemyName;
    public Sprite sprite;
    
    [Header("属性")]
    public int maxHealth;
    public int attack;
    public int defense;
    public float moveSpeed;
    
    [Header("行为")]
    public EnemyAIType aiType;
    public float detectionRange;
    public float attackRange;
    
    [Header("掉落")]
    public LootTable lootTable;
    public int expReward;
}
```

#### 3. 任务数据（QuestData）

```csharp
[CreateAssetMenu(fileName = "New Quest", menuName = "Game/Quest")]
public class QuestData : ScriptableObject
{
    [Header("基础信息")]
    public string questId;
    public string questName;
    [TextArea] public string description;
    
    [Header("类型")]
    public QuestType type;
    public QuestDifficulty difficulty;
    
    [Header("条件")]
    public QuestRequirement[] requirements;
    public QuestObjective[] objectives;
    
    [Header("奖励")]
    public QuestReward[] rewards;
    
    [Header("关联")]
    public string questGiver;
    public string[] prerequisiteQuests;
}
```

#### 4. NPC数据（NPCData）

```csharp
[CreateAssetMenu(fileName = "New NPC", menuName = "Game/NPC")]
public class NPCData : ScriptableObject
{
    [Header("基础信息")]
    public string npcId;
    public string npcName;
    public Sprite portrait;
    public int age;
    
    [Header("性格")]
    public string[] personalityTags;
    [TextArea] public string background;
    
    [Header("喜好")]
    public ItemData[] favoriteItems;
    public ItemData[] dislikedItems;
    
    [Header("日程")]
    public NPCSchedule[] weekdaySchedule;
    public NPCSchedule[] weekendSchedule;
    
    [Header("关系")]
    public NPCRelationship[] relationships;
    
    [Header("任务")]
    public QuestData[] personalQuests;
}
```

---

## 性能优化策略

### 1. 对象池（Object Pooling）

**用途：**
- 敌人生成
- 子弹和特效
- UI元素
- 音效播放器

**实现：**
```csharp
public class ObjectPool<T> where T : Component
{
    private T prefab;
    private Queue<T> pool;
    private Transform parent;
    
    public ObjectPool(T prefab, int initialSize, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;
        pool = new Queue<T>();
        
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewObject();
        }
    }
    
    private T CreateNewObject()
    {
        T obj = Object.Instantiate(prefab, parent);
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
        return obj;
    }
    
    public T Get()
    {
        if (pool.Count == 0)
        {
            CreateNewObject();
        }
        
        T obj = pool.Dequeue();
        obj.gameObject.SetActive(true);
        return obj;
    }
    
    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}
```

### 2. 资源异步加载

**使用 Addressables 系统：**
- 按需加载资源
- 减少初始加载时间
- 支持热更新
- 自动管理内存

### 3. 场景分割

**策略：**
- 主场景：核心系统和管理器
- 子场景：具体的游戏区域
- 异步加载和卸载
- 使用 Additive 模式

### 4. 渲染优化

**2D优化：**
- Sprite Atlas 打包
- 批处理（Batching）
- 遮挡剔除
- 相机视锥体剔除
- LOD（如果需要）

### 5. 代码优化

**最佳实践：**
- 避免在 Update 中频繁调用
- 使用对象池
- 缓存组件引用
- 使用事件而非轮询
- 合理使用协程

---

## UI系统架构

### UI框架设计

**层级结构：**
```
Canvas (Screen Space - Overlay)
├── HUD Layer (游戏内UI)
│   ├── 属性显示
│   ├── 时间显示
│   ├── 任务追踪
│   └── 快捷栏
│
├── Menu Layer (菜单UI)
│   ├── 主菜单
│   ├── 暂停菜单
│   ├── 设置菜单
│   └── 背包界面
│
├── Dialog Layer (对话UI)
│   ├── 对话框
│   ├── 选择框
│   └── NPC头像
│
└── Popup Layer (弹窗UI)
    ├── 提示信息
    ├── 确认对话框
    └── 教程提示
```

### UI管理器

```csharp
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    // UI面板
    private Dictionary<string, UIPanel> panels;
    private Stack<UIPanel> panelStack;
    
    // 方法
    public void ShowPanel(string panelName) { }
    public void HidePanel(string panelName) { }
    public void HideAllPanels() { }
    public T GetPanel<T>() where T : UIPanel { }
}

public abstract class UIPanel : MonoBehaviour
{
    public virtual void Show() { }
    public virtual void Hide() { }
    public virtual void Refresh() { }
}
```

---

## 输入系统

### 使用 Unity Input System

**优势：**
- 跨平台支持
- 灵活的输入映射
- 支持多种输入设备
- 易于重新绑定

**输入配置：**
```
Player Actions:
├── Movement
│   ├── Move (WASD / Arrow Keys / Gamepad Left Stick)
│   └── Sprint (Shift / Gamepad B)
│
├── Combat
│   ├── Attack (Mouse Left / Gamepad X)
│   ├── Dodge (Space / Gamepad A)
│   └── Special (Q / Gamepad Y)
│
├── Interaction
│   ├── Interact (E / Gamepad A)
│   └── Cancel (ESC / Gamepad B)
│
└── UI
    ├── Inventory (I / Gamepad Start)
    ├── Map (M / Gamepad Select)
    └── Pause (ESC / Gamepad Start)
```

---

## 本地化系统

### 多语言支持

**支持语言：**
- 简体中文（默认）
- 英语
- 日语（可选）

**本地化内容：**
- UI文本
- 对话内容
- 物品描述
- 任务文本

**实现方式：**
```csharp
public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }
    
    private Dictionary<string, string> localizedText;
    public SystemLanguage CurrentLanguage { get; private set; }
    
    public void LoadLanguage(SystemLanguage language) { }
    public string GetText(string key) { }
    public void ChangeLanguage(SystemLanguage language) { }
}

// 使用示例
string text = LocalizationManager.Instance.GetText("ui.button.start");
```

---

## 调试和测试工具

### 开发者控制台

**功能：**
- 命令行输入
- 作弊码
- 调试信息显示
- 性能监控

**常用命令：**
```
/god - 无敌模式
/additem [itemId] [count] - 添加物品
/setattr [type] [level] - 设置属性
/tp [location] - 传送
/time [hour] - 设置时间
/spawn [enemyId] - 生成敌人
/quest [questId] - 完成任务
```

### 性能监控

**监控指标：**
- FPS
- 内存使用
- 对象数量
- 绘制调用
- 物理计算

---

## 构建和发布

### 构建配置

**Windows构建：**
- 架构：x86_64
- 压缩：LZ4
- 开发构建：包含调试信息

**macOS构建：**
- 架构：Universal (Intel + Apple Silicon)
- 代码签名：需要开发者证书

### 版本管理

**版本号格式：** Major.Minor.Patch
- Major：重大更新
- Minor：功能更新
- Patch：Bug修复

**示例：** v1.0.0, v1.1.0, v1.1.1

---

## 未来扩展

### 可能的技术升级

**联机功能：**
- 使用 Unity Netcode 或 Mirror
- 异步多人模式
- 排行榜和成就

**Mod支持：**
- 开放数据文件格式
- Mod加载系统
- 社区内容支持

**高级功能：**
- 程序生成内容
- 机器学习AI
- 云存档

---

## 开发规范

### 代码规范

**命名规范：**
- 类名：PascalCase（如 GameManager）
- 方法名：PascalCase（如 GetPlayerData）
- 变量名：camelCase（如 playerHealth）
- 常量：UPPER_CASE（如 MAX_HEALTH）
- 私有字段：_camelCase（如 _currentState）

**注释规范：**
```csharp
/// <summary>
/// 计算伤害值
/// </summary>
/// <param name="attack">攻击力</param>
/// <param name="defense">防御力</param>
/// <returns>最终伤害值</returns>
public int CalculateDamage(int attack, int defense)
{
    // 基础伤害计算
    int damage = attack - defense;
    
    // 确保最小伤害为1
    return Mathf.Max(1, damage);
}
```

### Git工作流

**分支策略：**
- main：稳定版本
- develop：开发分支
- feature/*：功能分支
- hotfix/*：紧急修复

**提交规范：**
```
feat: 添加新功能
fix: 修复bug
docs: 文档更新
style: 代码格式调整
refactor: 代码重构
test: 测试相关
chore: 构建/工具相关
```

---

## 总结

技术架构设计为项目提供了：

1. **清晰的结构**：模块化设计，易于维护和扩展
2. **高性能**：优化策略确保流畅运行
3. **可扩展性**：数据驱动，支持快速迭代
4. **开发效率**：完善的工具和规范
5. **稳定性**：完善的存档和错误处理

这个架构将支撑整个游戏的开发，确保代码质量和开发效率。
