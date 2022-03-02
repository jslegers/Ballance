﻿using System.IO;
using UnityEditor;
using UnityEngine;
using ICSharpCode.SharpZipLib.Checksum;
using Ballance2.Utils;
using ICSharpCode.SharpZipLib.Zip;
using System.Xml;
using System.Collections.Generic;
using Ballance2.Editor.Lua;

namespace Ballance2.Editor.Modding
{
  class PackagePacker
  {
    private static string projPath = "";
    private static string projModDefFile = "";
    private static string projModDirPath = "";
    private static string projLogoFile = "";
    private static string packPackageName = "";
    private static string packLogoName = "";
    private static bool packShouldCompile = true;
    private static bool packContainCSharp = false;

    private static List<string> allAssetsPath = new List<string>();
    private static List<string> allLuaPath = new List<string>();
    private static List<string> allCsPath = new List<string>();

    public static string DoPackPackage(BuildTarget packTarget, TextAsset packDefFile, string sourceName, string targetDir)
    {
      string targetPath = targetDir + "/" + sourceName + ".ballance";
      if (!string.IsNullOrEmpty(targetPath))
      {
        packShouldCompile = true;
        packContainCSharp = false;

        DoSolvePackageDef(packDefFile);

        if (string.IsNullOrEmpty(packPackageName))
          return "PackageDef.xml 必须填写包名 packageName";

        allAssetsPath.Clear();
        allLuaPath.Clear();
        allCsPath.Clear();

        bool isCore = packPackageName == "core";
        string dirTargetPath = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrEmpty(projModDirPath))
        {
          //遍历文件夹的内容
          if (Directory.Exists(projModDirPath))
          {
            DirectoryInfo direction = new DirectoryInfo(projModDirPath);
            FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
              string filesPath = files[i].Name.Replace("\\", "/");

              if (filesPath.EndsWith(".meta")) continue;
              if (filesPath.Contains("NoPackage")) continue;
              if (isCore && filesPath.Contains("Scripts/Native")) continue;

              //将cs代码取出来，等待后续编译
              if (filesPath.EndsWith(".cs")) {
                allCsPath.Add(filesPath.Replace(projPath, ""));
                continue;
              }
              //将lua代码取出来，打包到zip中
              if (filesPath.EndsWith(".lua"))
              {
                allLuaPath.Add(filesPath.Replace(projPath, ""));
                continue;
              }

              allAssetsPath.Add(filesPath.Replace(projPath, ""));
            }
          }

          string name = Path.GetFileNameWithoutExtension(targetPath);

          //打包
          AssetBundleBuild assetBundleBuild = new AssetBundleBuild();
          assetBundleBuild.assetBundleName = packPackageName;
          assetBundleBuild.assetBundleVariant = "assetbundle";
          assetBundleBuild.assetNames = allAssetsPath.ToArray();

          //打包
          BuildPipeline.BuildAssetBundles(dirTargetPath, new AssetBundleBuild[]{
                        assetBundleBuild
                    }, BuildAssetBundleOptions.None, packTarget);

          EditorUtility.DisplayProgressBar("正在打包", "正在打包，请稍后...", 0.6f);

          //ballance 包处理
          DoSolveBallancePack(dirTargetPath, dirTargetPath + "/" + name, targetPath);

          EditorUtility.ClearProgressBar();
        }
        else return "PackageDef.xml 不在本项目中 ";
      }
      return "";
    }
    
    private static void DoSolvePackageDef(TextAsset packDefFile)
    {
      projPath = Directory.GetCurrentDirectory().Replace("\\", "/") + "/";
      projModDefFile = projPath + AssetDatabase.GetAssetPath(packDefFile);
      projModDirPath = projPath + Path.GetDirectoryName(AssetDatabase.GetAssetPath(packDefFile));

      //模块信息处理
      XmlDocument packDefXmlDoc = new XmlDocument();
      packDefXmlDoc.LoadXml(packDefFile.text);
      XmlNode nodePackage = packDefXmlDoc.SelectSingleNode("Package");

      if (nodePackage.Attributes["name"] != null)
        packPackageName = nodePackage.Attributes["name"].Value;

      foreach (XmlNode node in nodePackage.ChildNodes)
      {
        if (node.Name == "BaseInfo")
        {
          foreach (XmlAttribute attribute in node.Attributes)
          {
            if (attribute.Name == "packageName")
              packPackageName = attribute.Value;
          }
          foreach (XmlNode nodec in node.ChildNodes)
          {
            if (nodec.Name == "Logo")
            {
              packLogoName = nodec.InnerText;
              projLogoFile = projModDirPath + Path.DirectorySeparatorChar + nodec.InnerText;
              break;
            }
          }
          break;
        }
        else if (node.Name == "CompileLuaCode")
        {
          bool.TryParse(node.InnerText, out packShouldCompile);
        }
        else if (node.Name == "ContainCSharp")
        {
          bool.TryParse(node.InnerText, out packContainCSharp);
        }
      }
    }
    private static void DoSolveBallancePack(string dirTargetPath, string bundlePath, string targetPath)
    {
      Crc32 crc = new Crc32();
      ZipOutputStream zipStream = ZipUtils.CreateZipFile(targetPath);
      string basePath = projModDirPath.Replace(projPath, "").Replace("\\", "/");

      //添加到包里
      ZipUtils.AddFileToZip(zipStream, bundlePath + ".assetbundle", "/assets/" + Path.GetFileName(bundlePath) + ".assetbundle", ref crc);
      ZipUtils.AddFileToZip(zipStream, bundlePath + ".assetbundle.manifest", "/assets/" + Path.GetFileName(bundlePath) + ".assetbundle.manifest", ref crc);
      ZipUtils.AddFileToZip(zipStream, projModDefFile, projModDirPath.Length, ref crc);

      //添加logo图片
      if (File.Exists(projLogoFile)) ZipUtils.AddFileToZip(zipStream, projLogoFile, projModDirPath.Length, ref crc);
      else Debug.LogWarning("模块的 Logo 没有找到：" + projLogoFile);

      //添加lua代码
      int i = 0, len = allLuaPath.Count;
      foreach (string path in allLuaPath)
      {
        if (packShouldCompile)
        { //编译为字节码
          var outPath = "";
          if (LuaCompiler.CompileLuaFile(path, true, out outPath))
          {
            EditorUtility.DisplayProgressBar("正在打包", path, i / (float)len);
            ZipUtils.AddFileToZip(zipStream, outPath, "/class" + path.Substring(basePath.Length, path.Length - basePath.Length - 4) + ".luac", ref crc);
            File.Delete(outPath);
          }
          else
          {
            Debug.LogError("编译 " + path + " 失败, 将lua文件原样打包至zip中。");
            ZipUtils.AddFileToZip(zipStream, path, "/class" + path.Substring(basePath.Length), ref crc);
          }
        }
        else ZipUtils.AddFileToZip(zipStream, path, "/class" + path.Substring(basePath.Length), ref crc);
        i++;
      }
      //编译C#代码
      if(packContainCSharp && allCsPath.Count > 0) {
        //TODO: 编译C#代码
      }

      zipStream.Finish();
      zipStream.Close();
    }
  }
}
