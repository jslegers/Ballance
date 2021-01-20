﻿using Ballance2.System.Services;
using SLua;
using UnityEngine;

/*
* Copyright(c) 2021  mengyu
*
* 模块名：     
* GameSystemPackage.cs
* 
* 用途：
* 游戏主模块结构的声明
*
* 作者：
* mengyu
*
* 更改历史：
* 2021-1-17 创建
*
*/

namespace Ballance2.System.Package
{
    class GameSystemPackage : GamePackage
    {
        public GameSystemPackage()
        {
            PackageName = GamePackageManager.SYSTEM_PACKAGE_NAME;
            PackageFilePath = Application.dataPath;
        }

        public override LuaState PackageLuaState
        {
            get
            {
                if (GameManager.Instance != null)
                    return GameManager.Instance.GameMainLuaState;
                return base.PackageLuaState;
            }
        }

        public override T GetAsset<T>(string pathorname)
        {
            return Resources.Load<T>(pathorname);
        }
    }
}