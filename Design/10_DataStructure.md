# 数据结构与配置文件

## 数据架构概述

### 设计原则

**数据驱动开发：**
- 游戏逻辑与数据分离
- 配置文件易于修改和平衡
- 支持热更新和模组
- 便于团队协作

**数据格式选择：**
- ScriptableObject：Unity原生，可视化编辑
- JSON：通用格式，易于版本控制
- CSV：表格数据，批量编辑
- XML：复杂结构（可选）

**数据组织：**
- 按功能模块分类
- 清晰的命名规范
- 版本控制友好
- 易于查找和维护

---

## 核心数据结构

### 1. 玩家数据（PlayerData）

**基础信息：**
```csharp
[System.Serializable]
public class PlayerData
{
    // 基础属性
    public string playerName;
    public int level;
    public int currentExp;
    public int expToNextLevel;
    
    // 六维属性
    public AttributeData attributes;
    
    // 战斗属性
    public int maxHealth;
    public int currentHealth;
    public int maxStamina;
    public int currentStamina;
    public int attack;
    public int defense;
    public float moveSpeed;
    
    // 位置信息
    public Vector3 position;
    public string currentScene;
    
    // 时间信息
    public int currentDay;
    public int currentHour;
    public int currentMinute;
    
    // 货币
    public int gold;
    public int contributionPoints;
    
    // 统计数据
    public PlayerStats stats;
}
```

**属性数据：**
```csharp
[System.Serializable]
public class AttributeData
{
    public AttributeInfo strength;      // 力量
    public AttributeInfo skill;         // 技巧
    public AttributeInfo intelligence;  // 智慧
    public AttributeInfo charisma;      // 魅力
    public AttributeInfo courage;       // 勇气
    public AttributeInfo wealth;        // 财富
}

[System.Serializable]
public class AttributeInfo
{
    public int level;
    public int currentExp;
    public int expToNextLevel;
    
    public void AddExp(int exp)
    {
        currentExp += exp;
        while (currentExp >= expToNextLevel)
        {
            LevelUp();
        }
    }
    
    private void LevelUp()
    {
        level++;
        currentExp -= expToNextLevel;
        expToNextLevel = CalculateExpForNextLevel(level);
    }
    
    private int CalculateExpForNextLevel(int level)
    {
        return 100 + (level * 50); // 示例公式
    }
}
```

**统计数据：**
```csharp
[System.Serializable]
public class PlayerStats
{
    // 战斗统计
    public int totalEnemiesKilled;
    public int totalDamageDealt;
    public int totalDamageTaken;
    public int totalDeaths;
    
    // 任务统计
    public int totalQuestsCompleted;
    public int mainQuestsCompleted;
    public int sideQuestsCompleted;
    
    // 社交统计
    public int totalDialogues;
    public int totalGiftsGiven;
    public int highestAffection;
    
    // 探索统计
    public int totalDistanceTraveled;
    public int locationsDiscovered;
    public int itemsCollected;
    
    // 时间统计
    public int totalDaysPlayed;
    public float totalPlayTime;
}
```

---

### 2. 物品数据（ItemData）

**基础物品：**
```csharp
[CreateAssetMenu(fileName = "New Item", menuName = "Game/Item/Base")]
public class ItemData : ScriptableObject
{
    [Header("基础信息")]
    public string itemId;
    public string itemName;
    public Sprite icon;
    [TextArea(3, 5)]
    public string description;
    
    [Header("分类")]
    public ItemType type;
    public ItemRarity rarity;
    public ItemCategory category;
    
    [Header("属性")]
    public int maxStack = 99;
    public int baseValue;
    public float weight;
    public bool isQuestItem;
    public bool canDrop;
    public bool canSell;
    
    [Header("标签")]
    public string[] tags;
}

public enum ItemType
{
    Consumable,     // 消耗品
    Equipment,      // 装备
    Material,       // 材料
    QuestItem,      // 任务物品
    Misc            // 杂物
}

public enum ItemRarity
{
    Common,         // 普通（灰色）
    Uncommon,       // 不常见（绿色）
    Rare,           // 稀有（蓝色）
    Epic,           // 史诗（紫色）
    Legendary       // 传说（金色）
}

public enum ItemCategory
{
    Weapon,
    Armor,
    Accessory,
    Food,
    Medicine,
    Material,
    Tool,
    Book,
    Gift,
    Other
}
```

**消耗品数据：**
```csharp
[CreateAssetMenu(fileName = "New Consumable", menuName = "Game/Item/Consumable")]
public class ConsumableData : ItemData
{
    [Header("消耗品效果")]
    public ConsumableEffect[] effects;
    public float cooldown;
    public bool consumeOnUse = true;
    
    public void Use(PlayerController player)
    {
        foreach (var effect in effects)
        {
            effect.Apply(player);
        }
    }
}

[System.Serializable]
public class ConsumableEffect
{
    public EffectType type;
    public float value;
    public float duration;
}

public enum EffectType
{
    RestoreHealth,
    RestoreStamina,
    IncreaseAttack,
    IncreaseDefense,
    IncreaseSpeed,
    RemoveDebuff,
    GrantBuff
}
```

**装备数据：**
```csharp
[CreateAssetMenu(fileName = "New Equipment", menuName = "Game/Item/Equipment")]
public class EquipmentData : ItemData
{
    [Header("装备信息")]
    public EquipmentSlot slot;
    public int requiredLevel;
    public AttributeRequirement[] attributeRequirements;
    
    [Header("装备属性")]
    public int attack;
    public int defense;
    public float attackSpeed;
    public float criticalChance;
    public float criticalDamage;
    
    [Header("属性加成")]
    public AttributeBonus[] attributeBonuses;
    
    [Header("特殊效果")]
    public EquipmentEffect[] specialEffects;
    
    [Header("升级")]
    public int currentLevel;
    public int maxLevel = 10;
    public UpgradeRequirement[] upgradeRequirements;
}

public enum EquipmentSlot
{
    Weapon,
    Armor,
    Accessory
}

[System.Serializable]
public class AttributeRequirement
{
    public AttributeType type;
    public int requiredLevel;
}

[System.Serializable]
public class AttributeBonus
{
    public AttributeType type;
    public int bonusValue;
}

[System.Serializable]
public class EquipmentEffect
{
    public string effectName;
    public string effectDescription;
    public EffectTrigger trigger;
    public float chance;
}

public enum EffectTrigger
{
    OnEquip,
    OnAttack,
    OnHit,
    OnKill,
    Passive
}
```

---

### 3. NPC数据（NPCData）

**NPC基础数据：**
```csharp
[CreateAssetMenu(fileName = "New NPC", menuName = "Game/NPC")]
public class NPCData : ScriptableObject
{
    [Header("基础信息")]
    public string npcId;
    public string npcName;
    public int age;
    public Gender gender;
    public Sprite portrait;
    public Sprite sprite;
    
    [Header("性格")]
    public string[] personalityTags;
    [TextArea(5, 10)]
    public string background;
    public NPCPersonality personality;
    
    [Header("喜好")]
    public ItemPreference[] itemPreferences;
    
    [Header("日程")]
    public NPCScheduleData scheduleData;
    
    [Header("对话")]
    public DialogueData dialogueData;
    
    [Header("关系")]
    public NPCRelationship[] relationships;
    
    [Header("任务")]
    public string[] questIds;
    
    [Header("商店（如果是商人）")]
    public ShopData shopData;
    
    [Header("战斗（如果可战斗）")]
    public CombatData combatData;
}

public enum Gender
{
    Male,
    Female,
    Other
}

[System.Serializable]
public class NPCPersonality
{
    [Range(0, 100)] public int friendliness;
    [Range(0, 100)] public int openness;
    [Range(0, 100)] public int courage;
    [Range(0, 100)] public int optimism;
}

[System.Serializable]
public class ItemPreference
{
    public ItemData item;
    public PreferenceLevel level;
    public int affectionChange;
}

public enum PreferenceLevel
{
    Love,       // +15
    Like,       // +8
    Neutral,    // +1
    Dislike     // -5
}
```

**NPC日程数据：**
```csharp
[CreateAssetMenu(fileName = "New Schedule", menuName = "Game/NPC/Schedule")]
public class NPCScheduleData : ScriptableObject
{
    public ScheduleEntry[] weekdaySchedule;
    public ScheduleEntry[] weekendSchedule;
    public ScheduleEntry[] specialSchedule; // 节日等
}

[System.Serializable]
public class ScheduleEntry
{
    public int startHour;
    public int startMinute;
    public int endHour;
    public int endMinute;
    
    public string locationId;
    public Vector3 position;
    
    public NPCActivity activity;
    public string activityDescription;
    
    public bool isInteractable;
}

public enum NPCActivity
{
    Sleeping,
    Working,
    Eating,
    Socializing,
    Shopping,
    Exercising,
    Reading,
    Wandering,
    Custom
}
```

**NPC关系数据：**
```csharp
[System.Serializable]
public class NPCRelationship
{
    public string targetNpcId;
    public RelationType relationType;
    public int relationshipStrength; // 0-100
    [TextArea(2, 4)]
    public string relationshipDescription;
}

public enum RelationType
{
    Family,
    Friend,
    Rival,
    Romantic,
    Colleague,
    Mentor,
    Student,
    Enemy,
    Neutral
}
```

---

### 4. 任务数据（QuestData）

**任务基础数据：**
```csharp
[CreateAssetMenu(fileName = "New Quest", menuName = "Game/Quest")]
public class QuestData : ScriptableObject
{
    [Header("基础信息")]
    public string questId;
    public string questName;
    [TextArea(5, 10)]
    public string description;
    public Sprite icon;
    
    [Header("分类")]
    public QuestType type;
    public QuestDifficulty difficulty;
    public int recommendedLevel;
    
    [Header("任务给予者")]
    public string questGiverId;
    public string questGiverName;
    
    [Header("前置条件")]
    public QuestRequirement[] requirements;
    public string[] prerequisiteQuestIds;
    
    [Header("目标")]
    public QuestObjective[] objectives;
    
    [Header("奖励")]
    public QuestReward rewards;
    
    [Header("时间限制")]
    public bool hasTimeLimit;
    public int timeLimitInMinutes;
    
    [Header("失败条件")]
    public QuestFailCondition[] failConditions;
    
    [Header("后续任务")]
    public string[] followUpQuestIds;
}

public enum QuestType
{
    Main,           // 主线
    Side,           // 支线
    Daily,          // 日常
    Emergency,      // 紧急
    Personal,       // 个人剧情
    Community       // 社区
}

public enum QuestDifficulty
{
    Easy,
    Normal,
    Hard,
    VeryHard,
    Extreme
}
```

**任务条件：**
```csharp
[System.Serializable]
public class QuestRequirement
{
    public RequirementType type;
    public string targetId;
    public int requiredValue;
    public ComparisonOperator comparison;
}

public enum RequirementType
{
    PlayerLevel,
    AttributeLevel,
    NPCAffection,
    QuestCompleted,
    ItemOwned,
    LocationDiscovered,
    TimeOfDay,
    DayOfWeek,
    TownReputation
}

public enum ComparisonOperator
{
    Equal,
    GreaterThan,
    LessThan,
    GreaterOrEqual,
    LessOrEqual
}
```

**任务目标：**
```csharp
[System.Serializable]
public class QuestObjective
{
    public string objectiveId;
    public string description;
    public ObjectiveType type;
    
    public string targetId;
    public int requiredCount;
    public int currentCount;
    
    public bool isOptional;
    public bool isHidden; // 隐藏目标
    public bool isCompleted;
    
    public Vector3 targetLocation; // 用于地图标记
}

public enum ObjectiveType
{
    KillEnemy,
    CollectItem,
    TalkToNPC,
    ReachLocation,
    DefendArea,
    EscortNPC,
    CraftItem,
    UseItem,
    WaitTime,
    Custom
}
```

**任务奖励：**
```csharp
[System.Serializable]
public class QuestReward
{
    [Header("经验奖励")]
    public AttributeExpReward[] attributeExp;
    
    [Header("货币奖励")]
    public int gold;
    public int contributionPoints;
    
    [Header("物品奖励")]
    public ItemReward[] items;
    
    [Header("关系奖励")]
    public NPCAffectionReward[] affectionRewards;
    
    [Header("解锁内容")]
    public string[] unlockedLocations;
    public string[] unlockedRecipes;
    public string[] unlockedSkills;
    
    [Header("声望奖励")]
    public int townReputation;
    public FactionReputationReward[] factionReputation;
}

[System.Serializable]
public class AttributeExpReward
{
    public AttributeType type;
    public int expAmount;
}

[System.Serializable]
public class ItemReward
{
    public ItemData item;
    public int count;
    public bool isOptional; // 可选奖励
}

[System.Serializable]
public class NPCAffectionReward
{
    public string npcId;
    public int affectionChange;
}

[System.Serializable]
public class FactionReputationReward
{
    public string factionId;
    public int reputationChange;
}
```

---

### 5. 敌人数据（EnemyData）


**敌人基础数据：**
```csharp
[CreateAssetMenu(fileName = "New Enemy", menuName = "Game/Enemy")]
public class EnemyData : ScriptableObject
{
    [Header("基础信息")]
    public string enemyId;
    public string enemyName;
    public Sprite sprite;
    public RuntimeAnimatorController animator;
    
    [Header("分类")]
    public EnemyType type;
    public EnemyRank rank;
    public int level;
    
    [Header("战斗属性")]
    public int maxHealth;
    public int attack;
    public int defense;
    public float moveSpeed;
    public float attackSpeed;
    public float attackRange;
    public float detectionRange;
    
    [Header("抗性")]
    public DamageResistance[] resistances;
    
    [Header("AI行为")]
    public EnemyAIType aiType;
    public AIBehaviorData behaviorData;
    
    [Header("技能")]
    public EnemySkill[] skills;
    
    [Header("掉落")]
    public LootTable lootTable;
    public int expReward;
    public int goldReward;
    
    [Header("特殊")]
    public bool isBoss;
    public string[] weaknesses;
    public string[] immunities;
}

public enum EnemyType
{
    Infected,       // 感染者
    Mutant,         // 变异体
    Animal,         // 动物
    Robot,          // 机器人
    Human,          // 人类敌人
    Boss            // Boss
}

public enum EnemyRank
{
    Normal,         // 普通
    Elite,          // 精英
    Champion,       // 冠军
    Boss,           // Boss
    WorldBoss       // 世界Boss
}

[System.Serializable]
public class DamageResistance
{
    public DamageType damageType;
    [Range(-100, 100)]
    public float resistancePercent; // 负数表示弱点
}

public enum DamageType
{
    Physical,
    Fire,
    Ice,
    Electric,
    Poison,
    Explosive
}
```

**AI行为数据：**
```csharp
[CreateAssetMenu(fileName = "New AI Behavior", menuName = "Game/Enemy/AI")]
public class AIBehaviorData : ScriptableObject
{
    [Header("基础行为")]
    public float idleTime = 2f;
    public float patrolRadius = 10f;
    public float chaseSpeed = 5f;
    public float retreatHealthPercent = 20f;
    
    [Header("攻击行为")]
    public float attackCooldown = 2f;
    public bool canComboAttack;
    public int maxComboCount = 3;
    
    [Header("群体行为")]
    public bool callForHelp;
    public float helpCallRadius = 15f;
    public bool fleeWhenAlone;
    
    [Header("特殊行为")]
    public AISpecialBehavior[] specialBehaviors;
}

[System.Serializable]
public class AISpecialBehavior
{
    public string behaviorName;
    public AIBehaviorTrigger trigger;
    public float triggerValue;
    public AIAction action;
}

public enum AIBehaviorTrigger
{
    HealthBelow,
    HealthAbove,
    TimeElapsed,
    AllyDied,
    PlayerDistance,
    Random
}

public enum AIAction
{
    UseSkill,
    Flee,
    CallReinforcement,
    Enrage,
    Heal,
    ChangePhase
}
```

**掉落表：**
```csharp
[CreateAssetMenu(fileName = "New Loot Table", menuName = "Game/Loot Table")]
public class LootTable : ScriptableObject
{
    public LootEntry[] lootEntries;
    public int minDrops = 1;
    public int maxDrops = 3;
    
    public List<ItemStack> GenerateLoot(float luckModifier = 1f)
    {
        List<ItemStack> drops = new List<ItemStack>();
        int dropCount = Random.Range(minDrops, maxDrops + 1);
        
        for (int i = 0; i < dropCount; i++)
        {
            LootEntry entry = SelectRandomEntry(luckModifier);
            if (entry != null && Random.value <= entry.dropChance * luckModifier)
            {
                int count = Random.Range(entry.minCount, entry.maxCount + 1);
                drops.Add(new ItemStack(entry.item, count));
            }
        }
        
        return drops;
    }
    
    private LootEntry SelectRandomEntry(float luckModifier)
    {
        float totalWeight = 0f;
        foreach (var entry in lootEntries)
        {
            totalWeight += entry.weight;
        }
        
        float randomValue = Random.value * totalWeight;
        float currentWeight = 0f;
        
        foreach (var entry in lootEntries)
        {
            currentWeight += entry.weight;
            if (randomValue <= currentWeight)
            {
                return entry;
            }
        }
        
        return null;
    }
}

[System.Serializable]
public class LootEntry
{
    public ItemData item;
    [Range(0f, 1f)]
    public float dropChance;
    public float weight = 1f;
    public int minCount = 1;
    public int maxCount = 1;
    public ItemRarity rarity;
}

[System.Serializable]
public class ItemStack
{
    public ItemData item;
    public int count;
    
    public ItemStack(ItemData item, int count)
    {
        this.item = item;
        this.count = count;
    }
}
```

---

### 6. 对话数据（DialogueData）

**对话系统数据：**
```csharp
[CreateAssetMenu(fileName = "New Dialogue", menuName = "Game/Dialogue")]
public class DialogueData : ScriptableObject
{
    public string dialogueId;
    public DialogueNode[] nodes;
    public DialogueVariable[] variables;
}

[System.Serializable]
public class DialogueNode
{
    public string nodeId;
    public string speakerId; // NPC ID
    public string speakerName;
    public Sprite speakerPortrait;
    
    [TextArea(3, 6)]
    public string text;
    
    public DialogueCondition[] conditions; // 显示条件
    public DialogueChoice[] choices;
    public DialogueEffect[] effects; // 对话效果
    
    public string nextNodeId; // 如果没有选择
    public float autoAdvanceDelay; // 自动前进延迟
}

[System.Serializable]
public class DialogueChoice
{
    public string choiceText;
    public string targetNodeId;
    
    public DialogueCondition[] conditions; // 选项显示条件
    public DialogueEffect[] effects; // 选择后的效果
    
    public AttributeRequirement[] attributeRequirements;
    public bool isSkillCheck; // 是否是技能检定
    public float successChance; // 成功率
}

[System.Serializable]
public class DialogueCondition
{
    public ConditionType type;
    public string targetId;
    public ComparisonOperator comparison;
    public float value;
}

public enum ConditionType
{
    AttributeLevel,
    NPCAffection,
    QuestStatus,
    ItemOwned,
    TimeOfDay,
    PlayerLevel,
    FlagSet,
    VariableValue
}

[System.Serializable]
public class DialogueEffect
{
    public EffectType type;
    public string targetId;
    public float value;
}

public enum EffectType
{
    ChangeAffection,
    GiveItem,
    RemoveItem,
    StartQuest,
    CompleteQuest,
    GiveExp,
    GiveMoney,
    SetFlag,
    SetVariable,
    TriggerEvent
}

[System.Serializable]
public class DialogueVariable
{
    public string variableName;
    public VariableType type;
    public object value;
}

public enum VariableType
{
    Boolean,
    Integer,
    Float,
    String
}
```

---

### 7. 场景数据（SceneData）

**场景配置：**
```csharp
[CreateAssetMenu(fileName = "New Scene Config", menuName = "Game/Scene")]
public class SceneConfigData : ScriptableObject
{
    [Header("基础信息")]
    public string sceneId;
    public string sceneName;
    public string sceneDescription;
    public SceneType type;
    
    [Header("加载设置")]
    public string unitySceneName;
    public LoadSceneMode loadMode;
    public bool showLoadingScreen;
    
    [Header("环境设置")]
    public EnvironmentData environment;
    
    [Header("音频")]
    public AudioClip backgroundMusic;
    public AudioClip ambientSound;
    
    [Header("敌人生成")]
    public EnemySpawnData[] enemySpawns;
    
    [Header("NPC")]
    public NPCSpawnData[] npcSpawns;
    
    [Header("可交互物体")]
    public InteractableData[] interactables;
    
    [Header("传送点")]
    public TeleportPoint[] teleportPoints;
}

public enum SceneType
{
    Town,
    Combat,
    Exploration,
    Interior,
    Special
}

[System.Serializable]
public class EnvironmentData
{
    public bool isDynamic; // 是否动态昼夜
    public Color ambientLight;
    public float fogDensity;
    public WeatherType defaultWeather;
}

public enum WeatherType
{
    Clear,
    Cloudy,
    Rain,
    Storm,
    Snow,
    Fog
}

[System.Serializable]
public class EnemySpawnData
{
    public EnemyData enemyData;
    public Vector3 spawnPosition;
    public float spawnRadius;
    public int minCount;
    public int maxCount;
    public float respawnTime;
    public SpawnCondition[] conditions;
}

[System.Serializable]
public class NPCSpawnData
{
    public string npcId;
    public Vector3 defaultPosition;
    public bool followSchedule;
}

[System.Serializable]
public class InteractableData
{
    public string interactableId;
    public InteractableType type;
    public Vector3 position;
    public string[] requiredItems;
    public string[] rewards;
}

public enum InteractableType
{
    Container,
    Door,
    Lever,
    Button,
    Collectible,
    Crafting,
    Rest,
    Custom
}

[System.Serializable]
public class TeleportPoint
{
    public string pointId;
    public Vector3 position;
    public string targetSceneId;
    public string targetPointId;
    public bool requiresUnlock;
}
```

---

### 8. 配方数据（RecipeData）

**制作配方：**
```csharp
[CreateAssetMenu(fileName = "New Recipe", menuName = "Game/Recipe")]
public class RecipeData : ScriptableObject
{
    [Header("基础信息")]
    public string recipeId;
    public string recipeName;
    public Sprite icon;
    [TextArea(2, 4)]
    public string description;
    
    [Header("分类")]
    public RecipeCategory category;
    public int requiredLevel;
    
    [Header("需求")]
    public AttributeRequirement[] attributeRequirements;
    public string requiredStation; // 需要的工作台
    
    [Header("材料")]
    public RecipeIngredient[] ingredients;
    
    [Header("产出")]
    public ItemData resultItem;
    public int resultCount = 1;
    
    [Header("制作")]
    public float craftTime; // 秒
    public int expReward;
    
    [Header("解锁")]
    public bool isUnlocked;
    public string[] unlockConditions;
}

public enum RecipeCategory
{
    Weapon,
    Armor,
    Consumable,
    Material,
    Tool,
    Furniture,
    Other
}

[System.Serializable]
public class RecipeIngredient
{
    public ItemData item;
    public int count;
}
```

---

### 9. 技能数据（SkillData）

**技能配置：**
```csharp
[CreateAssetMenu(fileName = "New Skill", menuName = "Game/Skill")]
public class SkillData : ScriptableObject
{
    [Header("基础信息")]
    public string skillId;
    public string skillName;
    public Sprite icon;
    [TextArea(3, 5)]
    public string description;
    
    [Header("分类")]
    public SkillType type;
    public SkillCategory category;
    
    [Header("需求")]
    public int requiredLevel;
    public AttributeRequirement[] attributeRequirements;
    public string[] prerequisiteSkills;
    
    [Header("消耗")]
    public int staminaCost;
    public int manaCost; // 如果有魔法系统
    
    [Header("冷却")]
    public float cooldown;
    
    [Header("效果")]
    public SkillEffect[] effects;
    
    [Header("动画")]
    public string animationTrigger;
    public float animationDuration;
    
    [Header("音效")]
    public AudioClip castSound;
    public AudioClip hitSound;
    
    [Header("特效")]
    public GameObject visualEffect;
}

public enum SkillType
{
    Active,     // 主动技能
    Passive,    // 被动技能
    Toggle      // 切换技能
}

public enum SkillCategory
{
    Combat,
    Survival,
    Social,
    Crafting,
    Exploration
}

[System.Serializable]
public class SkillEffect
{
    public SkillEffectType type;
    public float value;
    public float duration;
    public GameObject effectPrefab;
}

public enum SkillEffectType
{
    Damage,
    Heal,
    Buff,
    Debuff,
    Summon,
    Teleport,
    AreaEffect,
    StatusEffect
}
```

---

### 10. 存档数据（SaveData）

**完整存档结构：**
```csharp
[System.Serializable]
public class SaveData
{
    [Header("存档信息")]
    public int saveSlot;
    public string saveName;
    public DateTime saveTime;
    public string version;
    
    [Header("玩家数据")]
    public PlayerData playerData;
    public InventoryData inventoryData;
    public EquipmentData equipmentData;
    
    [Header("世界数据")]
    public TimeData timeData;
    public WorldStateData worldData;
    
    [Header("NPC数据")]
    public NPCSaveData[] npcData;
    
    [Header("任务数据")]
    public QuestSaveData[] questData;
    
    [Header("场景数据")]
    public SceneSaveData[] sceneData;
    
    [Header("标志位")]
    public Dictionary<string, bool> flags;
    public Dictionary<string, int> variables;
    
    [Header("统计数据")]
    public PlayerStats stats;
}

[System.Serializable]
public class InventoryData
{
    public ItemStack[] items;
    public int maxSlots;
}

[System.Serializable]
public class EquipmentData
{
    public string weaponId;
    public string armorId;
    public string accessoryId;
}

[System.Serializable]
public class TimeData
{
    public int currentDay;
    public int currentHour;
    public int currentMinute;
    public float timeScale;
}

[System.Serializable]
public class WorldStateData
{
    public int townLevel;
    public int townReputation;
    public Dictionary<string, int> factionReputation;
    public string[] unlockedLocations;
    public string[] discoveredLocations;
    public WeatherType currentWeather;
}

[System.Serializable]
public class NPCSaveData
{
    public string npcId;
    public int affection;
    public Vector3 position;
    public string currentActivity;
    public bool isAlive;
    public Dictionary<string, bool> flags;
}

[System.Serializable]
public class QuestSaveData
{
    public string questId;
    public QuestStatus status;
    public QuestObjectiveProgress[] objectiveProgress;
    public float timeRemaining;
}

public enum QuestStatus
{
    NotStarted,
    Active,
    Completed,
    Failed
}

[System.Serializable]
public class QuestObjectiveProgress
{
    public string objectiveId;
    public int currentCount;
    public bool isCompleted;
}

[System.Serializable]
public class SceneSaveData
{
    public string sceneId;
    public string[] defeatedEnemies;
    public string[] collectedItems;
    public Dictionary<string, bool> interactableStates;
}
```

---

## JSON配置文件示例

### 游戏配置（GameConfig.json）

```json
{
  "gameVersion": "0.1.0",
  "defaultLanguage": "zh-CN",
  "targetFrameRate": 60,
  
  "gameplay": {
    "startingGold": 100,
    "startingLevel": 1,
    "maxLevel": 50,
    "timeScale": 60,
    "autosaveInterval": 300
  },
  
  "difficulty": {
    "easy": {
      "damageMultiplier": 0.7,
      "expMultiplier": 1.2,
      "lootMultiplier": 1.1
    },
    "normal": {
      "damageMultiplier": 1.0,
      "expMultiplier": 1.0,
      "lootMultiplier": 1.0
    },
    "hard": {
      "damageMultiplier": 1.5,
      "expMultiplier": 0.8,
      "lootMultiplier": 0.9
    }
  },
  
  "balance": {
    "healthRegenRate": 1.0,
    "staminaRegenRate": 5.0,
    "baseAttackSpeed": 1.0,
    "baseMoveSpeed": 5.0,
    "criticalChanceBase": 5.0,
    "criticalDamageBase": 150.0
  }
}
```

### 属性配置（AttributeConfig.json）

```json
{
  "attributes": [
    {
      "type": "Strength",
      "name": "力量",
      "description": "影响近战伤害和负重",
      "maxLevel": 100,
      "expCurve": "100 + level * 50",
      "effects": [
        {
          "type": "MeleeDamage",
          "formula": "level * 2"
        },
        {
          "type": "CarryWeight",
          "formula": "50 + level * 5"
        }
      ]
    },
    {
      "type": "Skill",
      "name": "技巧",
      "description": "影响远程伤害和制作",
      "maxLevel": 100,
      "expCurve": "100 + level * 50",
      "effects": [
        {
          "type": "RangedDamage",
          "formula": "level * 2"
        },
        {
          "type": "CraftingSpeed",
          "formula": "1.0 + level * 0.01"
        }
      ]
    }
  ]
}
```

### 物品数据库（ItemDatabase.json）

```json
{
  "items": [
    {
      "itemId": "item_potion_health_small",
      "itemName": "小型治疗药水",
      "description": "恢复50点生命值",
      "type": "Consumable",
      "rarity": "Common",
      "category": "Medicine",
      "maxStack": 99,
      "baseValue": 50,
      "weight": 0.1,
      "effects": [
        {
          "type": "RestoreHealth",
          "value": 50,
          "duration": 0
        }
      ],
      "iconPath": "Items/Potions/health_small"
    },
    {
      "itemId": "weapon_sword_iron",
      "itemName": "铁剑",
      "description": "基础的铁制长剑",
      "type": "Equipment",
      "rarity": "Common",
      "category": "Weapon",
      "maxStack": 1,
      "baseValue": 200,
      "weight": 2.0,
      "slot": "Weapon",
      "attack": 15,
      "attackSpeed": 1.0,
      "requiredLevel": 5,
      "attributeRequirements": [
        {
          "type": "Strength",
          "value": 10
        }
      ],
      "iconPath": "Items/Weapons/sword_iron"
    }
  ]
}
```

---

## CSV数据表示例

### NPC数据表（NPCs.csv）

```csv
ID,Name,Age,Gender,Personality,Affection_Start,Affection_Max,Schedule_Weekday,Schedule_Weekend,Favorite_Items,Disliked_Items
npc_001,艾琳,32,Female,温柔;坚强,30,100,schedule_erin_weekday,schedule_erin_weekend,item_medicine;item_book,item_alcohol
npc_002,马库斯,45,Male,严肃;可靠,20,100,schedule_marcus_weekday,schedule_marcus_weekend,item_weapon;item_tool,item_flower
npc_003,莉莉,19,Female,活泼;乐观,40,100,schedule_lily_weekday,schedule_lily_weekend,item_food;item_toy,item_weapon
```

### 敌人数据表（Enemies.csv）

```csv
ID,Name,Type,Rank,Level,Health,Attack,Defense,Speed,Detection_Range,Attack_Range,AI_Type,Loot_Table,Exp,Gold
enemy_001,普通感染者,Infected,Normal,1,50,10,5,3.0,10.0,1.5,Aggressive,loot_infected_common,10,5
enemy_002,快速感染者,Infected,Elite,3,40,15,3,6.0,12.0,1.0,Aggressive,loot_infected_fast,20,10
enemy_003,重型感染者,Infected,Elite,5,150,25,15,2.0,8.0,2.0,Tank,loot_infected_tank,50,25
```

### 任务数据表（Quests.csv）

```csv
ID,Name,Type,Difficulty,Quest_Giver,Description,Objectives,Rewards,Prerequisites
quest_001,初次见面,Side,Easy,npc_001,与艾琳医生初次见面,talk_to_npc:npc_001,exp_charisma:10;gold:50,
quest_002,医疗援助,Side,Normal,npc_001,帮助艾琳收集医疗用品,collect_item:item_medicine:5,exp_intelligence:20;gold:100;affection_npc_001:10,quest_001
quest_003,第一次危机,Main,Hard,npc_002,协助防御感染者袭击,kill_enemy:enemy_001:30;defend_area:town_center,exp_strength:50;exp_courage:30;gold:500;item:weapon_sword_iron,quest_002
```

---

## 数据管理系统

### 数据加载器

```csharp
public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }
    
    // 数据缓存
    private Dictionary<string, ItemData> itemDatabase;
    private Dictionary<string, NPCData> npcDatabase;
    private Dictionary<string, QuestData> questDatabase;
    private Dictionary<string, EnemyData> enemyDatabase;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllData();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void LoadAllData()
    {
        LoadItemData();
        LoadNPCData();
        LoadQuestData();
        LoadEnemyData();
    }
    
    private void LoadItemData()
    {
        itemDatabase = new Dictionary<string, ItemData>();
        ItemData[] items = Resources.LoadAll<ItemData>("Data/Items");
        foreach (var item in items)
        {
            itemDatabase[item.itemId] = item;
        }
    }
    
    // 获取数据
    public ItemData GetItem(string itemId)
    {
        return itemDatabase.ContainsKey(itemId) ? itemDatabase[itemId] : null;
    }
    
    public NPCData GetNPC(string npcId)
    {
        return npcDatabase.ContainsKey(npcId) ? npcDatabase[npcId] : null;
    }
    
    public QuestData GetQuest(string questId)
    {
        return questDatabase.ContainsKey(questId) ? questDatabase[questId] : null;
    }
    
    public EnemyData GetEnemy(string enemyId)
    {
        return enemyDatabase.ContainsKey(enemyId) ? enemyDatabase[enemyId] : null;
    }
}
```

---

## 数据验证工具

### 编辑器工具

```csharp
#if UNITY_EDITOR
using UnityEditor;

public class DataValidationTool : EditorWindow
{
    [MenuItem("Tools/Validate Game Data")]
    public static void ShowWindow()
    {
        GetWindow<DataValidationTool>("Data Validation");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Game Data Validation", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Validate All Data"))
        {
            ValidateAllData();
        }
        
        if (GUILayout.Button("Check Missing References"))
        {
            CheckMissingReferences();
        }
        
        if (GUILayout.Button("Generate Data Report"))
        {
            GenerateDataReport();
        }
    }
    
    private void ValidateAllData()
    {
        Debug.Log("Starting data validation...");
        
        ValidateItems();
        ValidateNPCs();
        ValidateQuests();
        ValidateEnemies();
        
        Debug.Log("Data validation complete!");
    }
    
    private void ValidateItems()
    {
        ItemData[] items = Resources.LoadAll<ItemData>("Data/Items");
        foreach (var item in items)
        {
            if (string.IsNullOrEmpty(item.itemId))
            {
                Debug.LogError($"Item {item.name} has no ID!");
            }
            if (item.icon == null)
            {
                Debug.LogWarning($"Item {item.itemName} has no icon!");
            }
        }
    }
    
    private void CheckMissingReferences()
    {
        // 检查所有引用是否存在
        Debug.Log("Checking for missing references...");
    }
    
    private void GenerateDataReport()
    {
        // 生成数据统计报告
        Debug.Log("Generating data report...");
    }
}
#endif
```

---

## 数据版本控制

### 版本迁移

```csharp
public class DataMigration
{
    public static SaveData MigrateSaveData(SaveData oldData, string fromVersion, string toVersion)
    {
        Debug.Log($"Migrating save data from {fromVersion} to {toVersion}");
        
        // 版本迁移逻辑
        if (fromVersion == "0.1.0" && toVersion == "0.2.0")
        {
            return MigrateFrom01To02(oldData);
        }
        
        return oldData;
    }
    
    private static SaveData MigrateFrom01To02(SaveData oldData)
    {
        // 具体的迁移逻辑
        // 例如：添加新字段、转换数据格式等
        return oldData;
    }
}
```

---

## 总结

数据结构与配置文件系统提供了：

1. **完整的数据模型**：覆盖游戏所有核心系统
2. **灵活的配置方式**：ScriptableObject + JSON + CSV
3. **易于维护**：清晰的结构和命名规范
4. **数据驱动**：逻辑与数据分离，便于平衡调整
5. **版本控制**：支持数据迁移和向后兼容
6. **开发工具**：验证工具确保数据完整性

这个数据系统将支撑整个游戏的开发，使得内容制作和平衡调整变得高效便捷。
