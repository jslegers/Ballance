﻿using Ballance2.Config;
using Ballance2.Sys.Res;
using Ballance2.Sys.Services;
using SLua;
using UnityEngine;
using System.IO;
using Ballance2.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Ballance2.Sys.Debug;

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
* 
* 
* 2021-4-17 Add BaseInfo for GameSystemPackage
*
*/

namespace Ballance2.Sys.Package
{
    class GameSystemPackage : GameZipPackage
    {
        public override async Task<bool> LoadInfo(string filePath)
        {
            PackageFilePath = filePath;
#if !UNITY_EDITOR
            GameErrorChecker.SetLastErrorAndLog(GameError.OnlyCanUseInEditor, TAG, "This package can only use in editor.");
            await base.LoadPackage();
            return false;
#else
            if(disableLoadFileInUnity) {
                return await base.LoadInfo(filePath);
            } else {
                string defPath = filePath + "/PackageDef.xml";
                if (!File.Exists(defPath))
                {
                    GameErrorChecker.SetLastErrorAndLog(GameError.PackageDefNotFound, TAG, "PackageDef.xml not found");
                    LoadError = "模块并不包含 PackageDef.xml";
                    return await base.LoadPackage();
                } 
                else
                {
                    try{
                        PackageDef = new XmlDocument();
                        PackageDef.Load(defPath);
                    }  catch(System.Exception e) {
                        GameErrorChecker.SetLastErrorAndLog(GameError.PackageIncompatible, TAG, "Format error in PackageDef.xml : " + e);
                        return false;
                    }
                    UpdateTime = File.GetLastWriteTime(defPath);
                    return ReadInfo(PackageDef);
                }
            }
#endif
        }
        public override async Task<bool> LoadPackage()
        {
            if(disableLoadFileInUnity) {
                return await base.LoadPackage();
            } else {
                if(!string.IsNullOrEmpty(BaseInfo.Logo))
                    LoadLogo(PackageFilePath + "/" + BaseInfo.Logo);
                
                DoSearchScriptNames();
                SystemPackageSetInitFinished();

                disableZipLoad = true;
                return await base.LoadPackage();
            }
        }
        private void LoadLogo(string path)
        {
            try
            {
                Texture2D texture2D = new Texture2D(128, 128);
                texture2D.LoadImage(File.ReadAllBytes(path));

                BaseInfo.LogoTexture = Sprite.Create(texture2D, 
                    new Rect(Vector2.zero, new Vector2(texture2D.width, texture2D.height)), 
                    new Vector2(0.5f, 0.5f));
            }
            catch (System.Exception e)
            {
                BaseInfo.LogoTexture = null;
                Log.E(TAG, "在加载模块的 Logo {0} 失败\n错误信息：{1}", path, e.ToString());
            }
        }

        private Dictionary<string, string> packageCodeAsset = new Dictionary<string, string>();

        public override void Destroy() {
            packageCodeAsset.Clear();
            base.Destroy();
        }
        
        private bool disableLoadFileInUnity = false;

        public void SetDisableLoadFileInUnity() {
            disableLoadFileInUnity = true;
        }

        private void DoSearchScriptNames() {
#if UNITY_EDITOR
            //构建一下所有脚本名称和路径的列表
            DirectoryInfo dir = new DirectoryInfo(ConstStrings.EDITOR_SYSTEMPACKAGE_LOAD_SCRIPT_PATH);  
            FileInfo[] fi = dir.GetFiles("*.lua", SearchOption.AllDirectories);
            foreach(var f in fi) {
                string path = f.FullName.Replace("\\", "/");
                int index = path.IndexOf("Assets/");
                if(index > 0)
                    path = path.Substring(index);
                packageCodeAsset.Add(f.Name, path);
            }
            packageCodeAsset.Add("PackageEntry.lua", ConstStrings.EDITOR_SYSTEMPACKAGE_LOAD_ASSET_PATH + "PackageEntry.lua");
#endif       
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
#if UNITY_EDITOR
            if(disableLoadFileInUnity) {
                return base.GetAsset<T>(pathorname);
            } else {
                if(pathorname.StartsWith("Assets"))
                    return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(pathorname);       
                return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(ConstStrings.EDITOR_SYSTEMPACKAGE_LOAD_ASSET_PATH + pathorname);
            }
#else
            return base.GetAsset<T>(pathorname);
#endif
        }
        public override string GetCodeLuaAsset(string pathorname, out string realPath)
        {
#if UNITY_EDITOR
            if(disableLoadFileInUnity) {
                return base.GetCodeLuaAsset(pathorname, out realPath);
            } else {
                //绝对路径
                if(GamePathManager.IsAbsolutePath(pathorname) || pathorname.StartsWith("Assets")) {
                    realPath = pathorname;
                    return new StreamReader(pathorname, System.Text.Encoding.UTF8).ReadToEnd();
                }
                //直接拼接路径
                var scriptPath = ConstStrings.EDITOR_SYSTEMPACKAGE_LOAD_SCRIPT_PATH + pathorname;
                if(File.Exists(scriptPath)) {
                    realPath = scriptPath;
                    return new StreamReader(scriptPath, System.Text.Encoding.UTF8).ReadToEnd();
                }
                //尝试使用路径列表里的路径
                if(packageCodeAsset.TryGetValue(pathorname, out var path)) {
                    realPath = path;
                    return new StreamReader(path, System.Text.Encoding.UTF8).ReadToEnd();
                }
                
                realPath = "";
                return null;
            } 
#else
            return base.GetCodeLuaAsset(pathorname);
#endif
        }
    }
}
