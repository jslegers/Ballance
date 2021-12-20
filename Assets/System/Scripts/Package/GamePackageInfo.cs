﻿using Ballance2.Config;
using Ballance2.Utils;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

/*
* Copyright(c) 2021  mengyu
*
* 模块名：     
* GamePackageInfo.cs
* 
* 用途：
* 游戏模块所用信息结构体和枚举声明
*
* 作者：
* mengyu
*/

namespace Ballance2.Package
{
  /// <summary>
  /// 模块代码类型
  /// </summary>
  public enum GamePackageCodeType
  {
    /// <summary>
    /// 无代码
    /// </summary>
    None,
    /// <summary>
    /// 代码类型是 Lua
    /// </summary>
    Lua,
    /// <summary>
    /// 代码类型是 C# DLL
    /// </summary>
    CSharp
  }
  /// <summary>
  /// 模块类型
  /// </summary>
  public enum GamePackageType
  {
    /// <summary>
    /// 代码包
    /// </summary>
    Module,
    /// <summary>
    /// 资源包
    /// </summary>
    Asset
  }
  /// <summary>
  /// 模块加载状态
  /// </summary>
  public enum GamePackageStatus
  {
    /// <summary>
    /// 未加载
    /// </summary>
    NotLoad = 0,
    /// <summary>
    /// 正在注册
    /// </summary>
    Registing = 1,
    /// <summary>
    /// 正在加载
    /// </summary>
    Loading = 2,
    /// <summary>
    /// 加载成功
    /// </summary>
    LoadSuccess = 3,
    /// <summary>
    /// 加载失败
    /// </summary>
    LoadFailed = 4,
    /// <summary>
    /// 正在等待卸载
    /// </summary>
    UnloadWaiting = 5,
    /// <summary>
    /// 已经注册但未加载
    /// </summary>
    Registered = 6,
  }

  /// <summary>
  /// 模块基础信息
  /// </summary>
  public class GamePackageBaseInfo
  {
    public GamePackageBaseInfo(XmlNode xmlNodeBaseInfo)
    {
      if (xmlNodeBaseInfo != null)
        for (int i = 0; i < xmlNodeBaseInfo.ChildNodes.Count; i++)
        {
          switch (xmlNodeBaseInfo.ChildNodes[i].Name)
          {
            case "Name": Name = xmlNodeBaseInfo.ChildNodes[i].InnerText; break;
            case "Author": Author = FixCdData(xmlNodeBaseInfo.ChildNodes[i].InnerText); break;
            case "Introduction": Introduction = FixCdData(xmlNodeBaseInfo.ChildNodes[i].InnerXml); break;
            case "Logo": Logo = xmlNodeBaseInfo.ChildNodes[i].InnerText; break;
            case "Link": Link = xmlNodeBaseInfo.ChildNodes[i].InnerText; break;
            case "DocLink": DocLink = xmlNodeBaseInfo.ChildNodes[i].InnerText; break;
            case "AuthorLink": AuthorLink = xmlNodeBaseInfo.ChildNodes[i].InnerText; break;
            case "Description": Description = FixCdData(xmlNodeBaseInfo.ChildNodes[i].InnerText); break;
            case "VersionName":
              VersionName = xmlNodeBaseInfo.ChildNodes[i].InnerText == "{internal.core.versionName}" ? GameConst.GameVersion :
                  FixCdData(xmlNodeBaseInfo.ChildNodes[i].InnerText);
              break;
            case "Dependencies":
              for (int j = 0, jc = xmlNodeBaseInfo.ChildNodes[i].ChildNodes.Count; j < jc; j++)
              {
                var node = xmlNodeBaseInfo.ChildNodes[i].ChildNodes[j];
                if (node.Attributes != null)
                  Dependencies.Add(new GamePackageDependencies(node));
              }
              break;
          }
        }
    }

    private string FixCdData(string x)
    {
      if (x.Length > 12 && x.StartsWith("<![CDATA[") && x.EndsWith("]]>"))
        return x.Substring(9, x.Length - 12);
      return x;
    }

    /// <summary>
    /// 模块名称
    /// </summary>
    public string Name { get; private set; }
    /// <summary>
    /// 模块作者
    /// </summary>
    public string Author { get; private set; }
    /// <summary>
    /// 模块介绍
    /// </summary>
    public string Introduction { get; private set; }
    /// <summary>
    /// 说明文字
    /// </summary>
    public string Description { get; private set; }
    /// <summary>
    /// 模块Logo
    /// </summary>
    public string Logo { get; private set; }
    /// <summary>
    /// 模块Logo Sprite
    /// </summary>
    public Sprite LogoTexture { get; set; }
    /// <summary>
    /// 模块链接
    /// </summary>
    public string Link { get; private set; }
    /// <summary>
    /// 模块作者主页URL
    /// </summary>
    public string AuthorLink { get; private set; }
    /// <summary>
    /// 模块文档URL
    /// </summary>
    public string DocLink { get; private set; }
    /// <summary>
    /// 显示给用户看的版本
    /// </summary>
    public string VersionName { get; private set; }
    /// <summary>
    /// 模块依赖
    /// </summary>
    public List<GamePackageDependencies> Dependencies { get; } = new List<GamePackageDependencies>();
  }
  /// <summary>
  /// 模块依赖信息
  /// </summary>
  public class GamePackageDependencies
  {
    public GamePackageDependencies(XmlNode xmlNode)
    {
      if (xmlNode != null)
      {
        for (int i = 0; i < xmlNode.Attributes.Count; i++)
        {
          switch (xmlNode.Attributes[i].Name)
          {
            case "name":
              Name = xmlNode.Attributes[i].Value;
              break;
            case "minVer":
              MinVersion = ConverUtils.StringToInt(xmlNode.Attributes[i].Value,
                  0, "Dependencies/minVer");
              break;
            case "mustLoad":
              MustLoad = ConverUtils.StringToBoolean(xmlNode.Attributes[i].Value,
                  false, "Dependencies/mustLoad");
              break;
          }
        }
      }
    }

    /// <summary>
    /// 依赖模块包名
    /// </summary>
    public string Name { get; private set; }
    /// <summary>
    /// 依赖模块最低版本
    /// </summary>
    public int MinVersion { get; private set; }
    /// <summary>
    /// 依赖模块是否必须加载
    /// </summary>
    public bool MustLoad { get; private set; }

  }
}