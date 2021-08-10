﻿using Ballance2.Config;
using Ballance2.Sys.Debug;
using Ballance2.Sys.Services;
using Ballance2.Sys.UI.Utils;
using Ballance2.UI.Parts;
using SubjectNerd.Utilities;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

/*
* Copyright(c) 2021  mengyu
*
* 模块名：     
* GameEntry.cs
* 
* 用途：
* 整个游戏的入口
* 以及用户协议对话框的显示
*
* 作者：
* mengyu
*
* 
* 
* 2021-4-12 mengyu 修改调试相关配置
*/

namespace Ballance2.Sys.Entry
{        
    class GameEntry : MonoBehaviour
    {
        #region 全局静态配置属性

        [Tooltip("启用调试模式。Editor下默认为调试模式")]
        public bool DebugMode = false;
        [Tooltip("目标帧率")]
        public int DebugTargetFrameRate = 60;
        [Tooltip("是否设置固定帧率")]
        public bool DebugSetFrameRate = true;
        [Tooltip("是否启用Lua调试器")]
        public bool DebugEnableLuaDebugger = true;
        [Tooltip("调试类型")]
        public GameDebugType DebugType = GameDebugType.NoDebug;
        [Reorderable("DebugInitPackages", true, "PackageName")]
        [Tooltip("当前调试中需要初始化的包名")]
        public System.Collections.Generic.List<GameDebugPackageInfo> DebugInitPackages = null;
        [Tooltip("自定义调试用例入口事件名称。进入调试之后会发送一个指定的全局事件，自定义调试用例可以根据这个事件作为调试入口。")]
        public string DebugCustomEntryEvent = "DebugEntry";
        [Tooltip("是否在系统或自定义调试模式中加载用户自定义模块包")]
        public bool DebugLoadCustomPackages = true;

        #endregion

        #region 静态引入

        public static GameEntry Instance { get; private set;}

        public Camera GameBaseCamera = null;
        public RectTransform GameCanvas = null;

        public GameGlobalErrorUI GameGlobalErrorUI = null;
        public Text GameDebugBeginStats;

        public GameObject GlobalGamePermissionTipDialog = null;
        public GameObject GlobalGameUserAgreementTipDialog = null;
        public Button ButtonUserAgreementAllow = null;
        public Toggle CheckBoxAllowUserAgreement = null;
        public GameObject LinkPrivacyPolicy = null;
        public GameObject LinkUserAgreement = null;

        private bool GlobalGamePermissionTipDialogClosed = false;
        private bool GlobalGameUserAgreementTipDialogClosed = false;

        #endregion

        void Start()
        {
            Instance = this;
            GameDebugBeginStats.text = string.Format("Ballance Version {0} ({1})", GameConst.GameVersion, GameConst.GameBulidDate);

            InitBaseSettings();
            InitCommandLine();
            InitUI();

            if (DebugMode && DebugSetFrameRate) Application.targetFrameRate = DebugTargetFrameRate;

            StartCoroutine(InitMain());
        }
        private void OnDestroy()
        {
            GameSystem.Destroy();
        }
        public static void Destroy() {
            UnityEngine.Object.Destroy(Instance.gameObject);
        }

        #region 用户许可相关

        /// <summary>
        /// 显示许可对话框
        /// </summary>
        /// <returns></returns>
        private bool ShowUserArgeement()
        {
            if (PlayerPrefs.GetInt("UserAgreementAgreed", 0) == 0)
            {
                GlobalGameUserAgreementTipDialog.SetActive(true);
                GlobalGameUserAgreementTipDialog.GetComponent<Animator>().Play("GlobalTipDialogShowAnimation");
                return true;
            }
            return false;
        }
        /// <summary>
        /// 检查android权限是否申请
        /// </summary>
        /// <returns></returns>
        private bool TestAndroidPermission()
        {
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
                return true;
#endif
            return false;
        }

        /// <summary>
        /// 用户同意许可
        /// </summary>
        public void ArgeedUserArgeement()
        {
            PlayerPrefs.SetInt("UserAgreementAgreed", 1);
            GlobalGameUserAgreementTipDialogClosed = true;
            GlobalGameUserAgreementTipDialog.SetActive(false);
        }
        /// <summary>
        /// 同意选择框改变
        /// </summary>
        /// <param name=""></param>
        public void ArgeedUserArgeementChackChinged(bool check)
        {
            ButtonUserAgreementAllow.interactable = check;
        }
        /// <summary>
        /// 请求安卓权限
        /// </summary>
        public void RequestAndroidPermission()
        {
#if UNITY_ANDROID
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
#endif
            GlobalGamePermissionTipDialogClosed = true;
            GlobalGamePermissionTipDialog.SetActive(false);
        }
        /// <summary>
        /// 退出游戏
        /// </summary>
        public void QuitGame()
        {
            StopAllCoroutines();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion

        private void InitCommandLine()
        {
            string[] CommandLineArgs = Environment.GetCommandLineArgs();
            int len = CommandLineArgs.Length;
            if (len > 1)
            {
                for (int i = 0; i < len; i++)
                {
                    if (CommandLineArgs[i] == "-debug")
                        PlayerPrefs.SetInt("core.DebugMode", 1);
                }
            }
        }
        private void InitUI()
        {
            CheckBoxAllowUserAgreement.onValueChanged.AddListener(ArgeedUserArgeementChackChinged);

            EventTriggerListener.Get(LinkPrivacyPolicy).onClick += (go) =>
                Application.OpenURL(GameConst.BallancePrivacyPolicy);
            EventTriggerListener.Get(LinkUserAgreement).onClick += (go) =>
                Application.OpenURL(GameConst.BallanceUserAgreement);
        }           
        private void InitBaseSettings() {
#if UNITY_EDITOR
            DebugMode = true;
#else
            if(GameSettings.GetBool("DebugMode", false)) DebugMode = true;
            else {
                DebugMode = false;
                GameObject.Find("GameDebugBeginStats").SetActive(false);
            }
#endif
        }

        private IEnumerator InitMain()
        {
            if (TestAndroidPermission())
            {
                GlobalGamePermissionTipDialog.SetActive(true);
                GlobalGamePermissionTipDialog.GetComponent<Animator>().Play("GlobalTipDialogShowAnimation");

                yield return new WaitUntil(() => GlobalGamePermissionTipDialogClosed);
            }
            if (ShowUserArgeement())
            {
                yield return new WaitUntil(() => GlobalGameUserAgreementTipDialogClosed);
            }

            GameErrorChecker.SetGameErrorUI(GameGlobalErrorUI);
            GameSystemInit.FillStartParameters(this);

            if(DebugMode && DebugType == GameDebugType.SystemDebug)
                GameSystem.RegSysDebugProvider(GameSystemDebugTests.RequestDebug);
            else if(!DebugMode || DebugType != GameDebugType.SystemDebug) {
                GameSystem.RegSysHandler(GameSystemInit.GetSysHandler());
                GameSystem.Init();
            }
            else
                GameErrorChecker.ThrowGameError(GameError.ConfigueNotRight, "DebugMode not right.");
        }
    }
    /// <summary>
    /// 调试类型
    /// </summary>
    public enum GameDebugType {
        /// <summary>
        /// 正常运行。
        /// </summary>
        NoDebug,
        /// <summary>
        /// 完整的调试，包括系统调试和自定义调试，包含完整的游戏运行环境。
        /// </summary>
        FullDebug,
        /// <summary>
        /// 自定义调试。不包含系统测试，包含半完整的游戏运行环境。
        /// </summary>
        CustomDebug,
        /// <summary>
        /// 系统调试。此模式不会加载游戏运行环境。
        /// </summary>
        SystemDebug
    }
    [Serializable]
    public class GameDebugPackageInfo
    {
        public bool Enable;
        public string PackageName;
        public override string ToString() { return PackageName; }
    }
}