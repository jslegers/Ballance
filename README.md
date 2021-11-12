# Ballance

这是2004年Arail 发布的Ballance游戏的开源重制版（制作中）。

## 先睹为快

[先看看演示视频](https://www.bilibili.com/video/BV1Dg411P7xp/)

## 简介

作者是一个Ballance忠实粉丝，从最初为原版Ballance作图，到后面开发相关的小工具，最后又想让这个老游戏重新焕发生机。
这个项目从2018年就开始了，当时还在B吧里发布过一个测试版本，可惜太烂。中间又高考，停了好长时间，一直到大学快毕业才又想起来，一直想把它做好，可是因为天生拖延症，一拖再拖。到现在工作了，才终于有动力做，但想做又没有太多时间了。

这个项目目前由作者工作之余一个人做，大约5天更新一次。**非常欢迎想与我一起开发的小伙伴加入我一起做呀**。

原版游戏是使用Virtools制作的，仅可在WindowsX86平台运行。我因此使用Unity开发，希望可以将这个老游戏运行到手机平台上。

游戏主要流程大都使用Lua作为语言（为了兼顾MOD与热更），游戏框架使用C#写。

本项目从2019年开始开发，中间断断续续，到现在积攒大约3-4万行代码（大部分是游戏框架的）。

---

![Demo](https://imengyu.top/assets/images/demo.png)

## 目标

* 与原版物理必须差不多（正在努力调试中）
* **Havok物理引擎或是ivp物理引擎**
* 支持MOD和自定义关卡加载（已完成）
* 支持用Lua来开发MOD（已完成）
* 发布至Steam并建设游戏的创意工坊
* 发布Android和IOS端手游

## 开发状态

目前游戏主体架构已经开发的差不多了。整体流程已经可以运行了。
可以加载关卡，加载机关，游戏UI基本完成。
目前仅剩物理参数的调试。

物理引擎暂时使用Havok，但是我找到了ivp的源代码，通过反编译virtools的physics.dll，我发现virtools物理引擎就是这个源代码编译出来的，里面的字符串一模一样，因此假如用这个物理引擎，**可以达到和原版一模一样的物理效果**。目前正在努力调试这个物理引擎。。

## TODO: 项目待完成内容

* ✅ 已完成
* ❎ 完成能用但存在问题
* 🅿 功能有计划但目前暂停开发
* 🔙 功能回退旧版本
* 🅾 正在开发未完成
* 🈹 功能被割舍或不完全并暂停开发

---

* ✅ 基础系统
* ✅ 事件系统
* ✅ 操作与数据系统
* ✅ 基础系统
* ✅ 模组加载卸载
* ✅ 模组管理器
* ✅ Lua代码动态载入
* ✅ 模组包功能逻辑
* ✅ 调试命令管理器
* ✅ Lua调试功能
* ✅ 模组包打包功能
* 🈹 关卡包打包功能
* ✅ 逻辑场景
* ✅ Intro进入动画
* ✅ MenuLevel场景
* 🅿 MenuLevel的那个滚球动画
* ✅ 主菜单与设置菜单
* ✅ 关于菜单
* ✅ I18N
* ✅ 调试日志输出到unity
* ✅ core主模块独立打包装载
* ✅ BallLightningSphere球闪电动画
* ✅ BallManager球管理器主逻辑
* ✅ TranfoAminControl变球器动画逻辑
* ✅ 球碎片管理器主逻辑
* ✅ CamManager摄像机管理器主逻辑
* ✅ 关于菜单
* ✅ luac代码编译功能
* ✅ LUA 安全性
* ✅ LUA 按包鉴别
* ✅ 模块包安全系研究
* ✅ 修复物理坐标问题
* ✅ 修复物理约束碰撞问题
* ✅ 物理弹簧
* ✅ 物理滑动约束
* ✅ LevelEnd
* ✅ LevelBuilder
* ✅ 机关逻辑
* ✅ 简单机关
* ✅ 生命球和分数球机关
* ✅ SectorManager节逻辑
* ✅ GameManager相关逻辑
* ✅ 背景音乐相关逻辑
* ✅ 分数相关逻辑
* ✅ 自动归组
* ✅ 复杂机关 01
* 🅾 复杂机关 03
* 🅾 复杂机关 08
* 🅾 复杂机关 17
* ✅ 复杂机关 18
* 🅾 复杂机关 19
* 🅾 复杂机关 25
* 🅾 复杂机关 26
* 🅾 复杂机关 29
* 🅾 复杂机关 30
* 🅾 复杂机关 37
* 🅾 复杂机关 41
* ✅ 第1关
* ✅ 第2关
* 🅾 第3关
* 🅾 第4关
* 🅾 第5关
* 🅾 第6关
* 🅾 第7关
* 🅾 第8关
* 🅾 第9关
* 🅾 第10关
* 🅾 第11关
* 🅾 第12关
* 🅾 第13关
* 🅾 迷你机关调试环境
* 🅾 关卡管理菜单
* 🅾 模组管理菜单
* 🅾 菜单的键盘逻辑
* 🅾 第一关的教程
* 🅾 更新服务器与联网更新功能
* 🅾 手机端适配
* 🅾 Android and ios 物理模块调试
* 🅾 steam接入
* 🅾 最终整体调试
* 🅾 制作魔脓空间站的转译版本地图并测试整体系统功能
* 🅾 发布steam
* 🅾 发布其他平台

## 联系我

微信: brave_imengyu

## 项目运行步骤

提示：*(目前暂无Mac版本的物理引擎文件，请使用Win版本的Unity进行调试)*

1. 请下载 Unity 2021.2.0+ 版本打开项目。
2. 点击菜单“SLua”>“All”>“Make”以生成Lua相关文件。
3. 打开 Assets/Scenes/Game.unity 场景。
4. 选择 GameEntry 对象，设置“Debug Type”为“FullDebug”。
5. 点击运行
