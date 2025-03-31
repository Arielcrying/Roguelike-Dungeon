# Roguelike Dungeon - 随机地下城生成系统  

## General

一个采用**节点编辑器设计**的随机地牢生成系统，包含6个渐进式关卡。

- 自定义Unity编辑器工具链

- 基于节点图的房间布局系统

- 状态机驱动的玩家行为

- Cinemachine智能摄像机跟随

- URP渲染管线 + 像素完美适配

 

## Feature

#### Node Editor

继承`EditorWindow`的可视化编辑工具，创建了一个节点编辑器来构建每层地牢关卡的布局，以及相关的一个unity的编辑器窗口，内部可以创建节点图的scriptable object添加节点创建房间的scriptable object，通过添加节点间的连线设置节点间的父子关系（布局顺序），删除节点和通过删除节点间的连线取消节点之间的联系

 

#### Dungeon  Generate System

- **房间ScriptableObject**: 使用`GUID`维护父子房间关系

- **Unity原生随机算法**: 基于`Random.Range`和`Random.RandomState`

- **房间布局系统**: 使用Tilemap的`RuleTile`实现智能拼接

- **动态地图重置**: `R键`即时重新生成当前层级

- **渐进式关卡**: 6层地牢难度递增设计

- **分层渲染**: Sorting Layers + Pixel Perfect

 

#### Player System

玩家在地牢的基本行为：

- 包括asdw在地牢中移动，鼠标右键控制翻滚，武器追随光标瞄准，
- 根据玩家状态机控制的切换状态时ui文字的展现，
- 玩家和场景中房间交互打开门和点亮房间，
- 玩家控制地图的刷新，关卡间的切换，摄像机追随玩家和光标

## Display

<img src="Image\0.png" alt="0" style="zoom:50%;" />

<img src="Image\1.png" alt="1" style="zoom:50%;" />

<img src="Image\2.png" alt="2" style="zoom:50%;" />

<img src="Image\3.jpg" alt="3" style="zoom:50%;" />