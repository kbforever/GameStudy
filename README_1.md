# Study_Project_6 - Unity项目索引

这是一个使用Unity 2021+开发的3D项目，采用Universal Render Pipeline (URP)渲染管线。

## 项目结构

### 📁 Assets/ - 主要资源文件夹
- **NewBehaviourScript.cs** - 基础行为脚本（测试用，包含Start方法中的调试输出）
- **Scenes/** - 场景文件
  - SampleScene.unity - 主示例场景
- **Settings/** - URP渲染设置
  - URP-Balanced - 平衡质量设置
  - URP-HighFidelity - 高保真设置
  - URP-Performant - 高性能设置
- **TutorialInfo/** - 教程信息和编辑器扩展
  - **Scripts/** - 脚本文件
    - Readme.cs - Readme系统脚本
    - **Editor/** - 编辑器扩展
      - ReadmeEditor.cs - Readme编辑器界面
  - **Icons/** - 图标资源
    - URP.png - URP相关图标

### 📁 ProjectSettings/ - 项目设置
包含26个配置文件，包括项目设置、输入管理、标签和图层等。

### 📁 Packages/ - 包管理
- manifest.json - 包清单文件
- packages-lock.json - 包锁定文件

主要依赖包：
- **com.unity.render-pipelines.universal** (14.0.8) - URP渲染管线
- **com.boxqkrtm.ide.cursor** - Cursor IDE集成
- **com.unity.textmeshpro** (3.0.6) - 文本网格专业版
- **com.unity.timeline** (1.7.5) - 时间轴系统
- **com.unity.visualscripting** (1.9.0) - 视觉脚本

### 📁 Library/ - Unity生成文件
包含编译后的资源、着色器缓存、脚本程序集等（通常不需要手动编辑）

### 📁 Logs/ - 日志文件
Unity编辑器运行日志

### 📁 Temp/ - 临时文件
构建过程中的临时文件

## 开发环境

- **Unity版本**: 2021+ (基于URP 14.0.8)
- **渲染管线**: Universal Render Pipeline
- **IDE集成**: Cursor, Rider, Visual Studio, VS Code

## 🎮 大富翁游戏Demo

我已经为您创建了一个完整的单机版大富翁游戏demo！

### 📁 新增游戏文件
```
Assets/MonopolyGame/
├── Scripts/           # 10个核心脚本文件
│   ├── Player.cs           # 基础玩家类
│   ├── HumanPlayer.cs      # 人类玩家类
│   ├── AIPlayer.cs         # AI玩家类（智能决策）
│   ├── Property.cs         # 房产系统
│   ├── BoardManager.cs     # 棋盘管理器
│   ├── GameManager.cs      # 游戏管理器
│   ├── DiceManager.cs      # 骰子系统
│   ├── UIManager.cs        # UI界面管理
│   ├── MonopolyDemo.cs     # 游戏入口点
│   └── GameDemo.cs         # 演示脚本
├── Scenes/            # 游戏场景
│   └── MonopolyGameScene.unity
├── Prefabs/           # 预制体
│   ├── PlayerPrefab.prefab (需要手动创建)
│   └── README.md           # 预制体创建指南
└── README.md          # 详细游戏说明
```

### 🎯 游戏特色
- **完整的单机体验**: 1个人类玩家 + 3个智能AI玩家
- **真实的游戏规则**: 包含所有经典大富翁元素
- **智能AI系统**: AI会评估风险、计算收益，做出合理决策
- **完整的UI界面**: 直观的游戏界面和交互
- **40个棋盘格子**: 包含街道、铁路、公共事业等

### 🎮 核心功能
- ✅ 玩家创建和管理（人类+AI）
- ✅ 完整的房产买卖系统
- ✅ 垄断和房屋建造机制
- ✅ 租金支付和破产处理
- ✅ 监狱和特殊事件系统
- ✅ 回合制游戏流程
- ✅ 智能AI决策算法

### 🚀 如何运行大富翁游戏
1. **打开Unity项目**
2. **加载游戏场景**: `Assets/MonopolyGame/Scenes/MonopolyGameScene.unity`
3. **点击运行** - 游戏会自动初始化
4. **开始游戏** - 点击"开始游戏"按钮
5. **享受游戏** - 进行大富翁对战！

### 🎲 操作说明
- **掷骰子**: 点击按钮投骰子移动
- **决策制定**: 根据提示购买房产、建造房屋
- **ESC键**: 查看所有玩家信息
- **R键**: 重新开始游戏

**详情请查看**: `Assets/MonopolyGame/README.md`

## 脚本说明

### NewBehaviourScript.cs
- 位置: Assets/NewBehaviourScript.cs
- 功能: 基础MonoBehaviour类
- 当前状态: 测试脚本，输出调试信息

## 快速开始

1. 使用Unity打开Study_Project_6.sln或直接打开项目文件夹
2. 打开Assets/Scenes/SampleScene.unity场景
3. 运行项目查看效果

## 注意事项

- Library文件夹由Unity自动生成，请勿手动修改
- .meta文件用于Unity资源管理，请勿删除
- 项目使用URP渲染管线，确保Unity版本兼容性