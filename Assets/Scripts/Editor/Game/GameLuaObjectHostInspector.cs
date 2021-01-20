﻿using Ballance2.System.Bridge.LuaWapper;
using Ballance2.System.Bridge.LuaWapper.GameLuaWapperEvents;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(GameLuaObjectHost), true)]
class GameLuaObjectHostInspector : Editor
{
    private List<GameLuaObjectEventCaller> events = new List<GameLuaObjectEventCaller>();
    private GameLuaObjectEventWarps currentAddEventType = GameLuaObjectEventWarps.Unknow;
    private GameLuaObjectHost myScript;

    private SerializedProperty pLuaInitialVars;
    private SerializedProperty pName;
    private SerializedProperty pLuaClassName;
    private SerializedProperty pLuaPackageName;
    private SerializedProperty pExecuteOrder;
    private SerializedProperty pCreateStore;
    private SerializedProperty pCreateActionStore;

    private ReorderableList reorderableList;
    private GUIStyle styleHighlight = null;
    private GUIStyle styleCNBox = null;

    private bool bDrawVarsInspector = false;
    private bool bDrawEventCallerInspector = false;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        myScript = (GameLuaObjectHost)target;
        myScript.GetComponents(events);

        if(styleCNBox == null)
            styleCNBox = GUI.skin.GetStyle("CN Box");

        EditorGUI.BeginChangeCheck();

        DrawMinInspector();

        bDrawVarsInspector = EditorGUILayout.Foldout(bDrawVarsInspector, "Lua 引入参数", true);
        if (bDrawVarsInspector)
            DrawVarsInspector();

        bDrawEventCallerInspector = EditorGUILayout.Foldout(bDrawEventCallerInspector, "Lua 类 On * 事件接收器", true);
        if (bDrawEventCallerInspector)
            DrawEventCallerInspector();
        

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
    private void OnEnable()
    {
        bDrawVarsInspector = EditorPrefs.GetBool("GameLuaObjectHostInspector_bDrawVarsInspector", false);
        bDrawEventCallerInspector = EditorPrefs.GetBool("GameLuaObjectHostInspector_bDrawEventCallerInspector", false);

        pName = serializedObject.FindProperty("Name");
        pLuaClassName = serializedObject.FindProperty("LuaClassName");
        pLuaPackageName = serializedObject.FindProperty("LuaPackageName");
        pLuaInitialVars = serializedObject.FindProperty("LuaInitialVars");
        pExecuteOrder = serializedObject.FindProperty("ExecuteOrder");
        pCreateStore = serializedObject.FindProperty("CreateStore");
        pCreateActionStore = serializedObject.FindProperty("CreateActionStore");

        InitVarsList();
    }
    private void OnDisable()
    {
        EditorPrefs.SetBool("GameLuaObjectHostInspector_bDrawVarsInspector", bDrawVarsInspector);
        EditorPrefs.SetBool("GameLuaObjectHostInspector_bDrawEventCallerInspector", bDrawEventCallerInspector);
        styleCNBox = null;
        styleHighlight = null;
    }

    private void InitVarsList()
    {
        reorderableList = new ReorderableList(serializedObject, pLuaInitialVars, true, true, true, true);

        reorderableList.elementHeight = 66;     
        reorderableList.drawElementCallback =
            (rect, index, isActive, isFocused) => {

                var ptargetElement = pLuaInitialVars.GetArrayElementAtIndex(index);
                if (ptargetElement != null)
                {
                    var pName = ptargetElement.FindPropertyRelative("Name");
                    var pType = ptargetElement.FindPropertyRelative("Type");

                    rect.x += 15;
                    rect.height = 18;
                    rect.width -= 30;

                    var eType = (LuaVarObjectType)pType.enumValueIndex;
                    SerializedProperty pVal = null;

                    EditorGUI.PropertyField(rect, pName); rect.y += 20;
                    EditorGUI.PropertyField(rect, pType); rect.y += 20;

                    switch (eType)
                    {
                        case LuaVarObjectType.None:
                            break;
                        case LuaVarObjectType.Vector2:
                            pVal = ptargetElement.FindPropertyRelative("vector2");
                            pVal.vector2Value = EditorGUI.Vector2Field(rect, "vector2", pVal.vector2Value);
                            break;
                        case LuaVarObjectType.Vector2Int:
                            pVal = ptargetElement.FindPropertyRelative("vector2Int");
                            pVal.vector2IntValue = EditorGUI.Vector2IntField(rect, "vector2Int", pVal.vector2IntValue);
                            break;
                        case LuaVarObjectType.Vector3:
                            pVal = ptargetElement.FindPropertyRelative("vector3");
                            pVal.vector3Value = EditorGUI.Vector3Field(rect, "vector3", pVal.vector3Value);
                            break;
                        case LuaVarObjectType.Vector3Int:
                            pVal = ptargetElement.FindPropertyRelative("vector3Int");
                            pVal.vector3IntValue = EditorGUI.Vector3IntField(rect, "vector3Int", pVal.vector3IntValue);
                            break;
                        case LuaVarObjectType.Vector4:
                            pVal = ptargetElement.FindPropertyRelative("vector4");
                            pVal.vector4Value = EditorGUI.Vector4Field(rect, "vector4", pVal.vector4Value);
                            break;
                        case LuaVarObjectType.Rect:
                            pVal = ptargetElement.FindPropertyRelative("vector4");
                            pVal.vector4Value = EditorGUI.Vector4Field(rect, "vector4", pVal.vector4Value);
                            break;
                        case LuaVarObjectType.RectInt:
                            pVal = ptargetElement.FindPropertyRelative("vector4");
                            pVal.vector4Value = EditorGUI.Vector4Field(rect, "vector4", pVal.vector4Value);
                            break;
                        case LuaVarObjectType.Gradient:
                            pVal = ptargetElement.FindPropertyRelative("gradient");
                            EditorGUI.PropertyField(rect, pVal);
                            break;
                        case LuaVarObjectType.Layer:
                            pVal = ptargetElement.FindPropertyRelative("layer");
                            pVal.intValue = EditorGUI.LayerField(rect, "layer", pVal.intValue);
                            break;
                        case LuaVarObjectType.Curve:
                            pVal = ptargetElement.FindPropertyRelative("curve");
                            pVal.animationCurveValue = EditorGUI.CurveField(rect, "curve", pVal.animationCurveValue);
                            break;
                        case LuaVarObjectType.Color:
                            pVal = ptargetElement.FindPropertyRelative("color");
                            pVal.colorValue = EditorGUI.ColorField(rect, "color", pVal.colorValue);
                            break;
                        case LuaVarObjectType.BoundsInt:
                            pVal = ptargetElement.FindPropertyRelative("boundsInt");
                            pVal.boundsIntValue = EditorGUI.BoundsIntField(rect, "boundsInt", pVal.boundsIntValue);
                            break;
                        case LuaVarObjectType.Bounds:
                            pVal = ptargetElement.FindPropertyRelative("bounds");
                            pVal.boundsValue = EditorGUI.BoundsField(rect, "bounds", pVal.boundsValue);
                            break;
                        case LuaVarObjectType.Object:
                            pVal = ptargetElement.FindPropertyRelative("objectVal");
                            EditorGUI.ObjectField(rect, pVal);
                            break;
                        case LuaVarObjectType.GameObject:
                            pVal = ptargetElement.FindPropertyRelative("gameObjectVal");
                            EditorGUI.ObjectField(rect, pVal);
                            break;
                        case LuaVarObjectType.Long:
                            pVal = ptargetElement.FindPropertyRelative("longVal");
                            pVal.longValue = EditorGUI.LongField(rect, "longVal", pVal.longValue);
                            break;
                        case LuaVarObjectType.Int:
                            pVal = ptargetElement.FindPropertyRelative("intVal");
                            pVal.intValue = EditorGUI.IntField(rect, "intVal", pVal.intValue);
                            break;
                        case LuaVarObjectType.String:
                            pVal = ptargetElement.FindPropertyRelative("stringVal");
                            pVal.stringValue = EditorGUI.TextField(rect, "stringVal", pVal.stringValue);
                            break;
                        case LuaVarObjectType.Double:
                            pVal = ptargetElement.FindPropertyRelative("doubleVal");
                            pVal.doubleValue = EditorGUI.DoubleField(rect, "doubleVal", pVal.doubleValue);
                            break;
                        case LuaVarObjectType.Bool:
                            pVal = ptargetElement.FindPropertyRelative("boolVal");
                            pVal.boolValue = EditorGUI.Toggle(rect, "boolVal", pVal.boolValue);
                            break;
                    }
                    rect.y += 20;

                    if (isFocused && EditorApplication.isPlaying)
                    {
                        float w2 = rect.width / 2;
                        rect.width -= w2;
                        if (GUI.Button(rect, "UpdateVarFromLua"))
                            myScript.UpdateVarFromLua(myScript.LuaInitialVars[index]);
                        rect.x += w2;
                        if (GUI.Button(rect, "UpdateVarToLua"))
                            myScript.UpdateVarToLua(myScript.LuaInitialVars[index]);
                    }
                }
            };

        reorderableList.drawElementBackgroundCallback = (rect, index, isActive, isFocused) => {
            if (styleHighlight == null)
                styleHighlight = GUI.skin.FindStyle("MeTransitionSelectHead");
            if (isFocused == false)
                return;
            rect.height = 66;
            GUI.Box(rect, GUIContent.none, styleHighlight);
        };
        reorderableList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, pLuaInitialVars.displayName);
    }

    private void DrawMinInspector()
    {
        EditorGUILayout.BeginVertical(styleCNBox);

        EditorGUILayout.PropertyField(pName);
        EditorGUILayout.PropertyField(pLuaClassName);
        EditorGUILayout.PropertyField(pLuaPackageName);
        EditorGUILayout.PropertyField(pExecuteOrder);
        EditorGUILayout.PropertyField(pCreateStore);
        EditorGUILayout.PropertyField(pCreateActionStore);

        if (string.IsNullOrEmpty(myScript.LuaClassName))
            EditorGUILayout.HelpBox("必须提供 Lua 类名，否则该 Lua 组件不会运行", MessageType.Warning);
        if (string.IsNullOrEmpty(myScript.LuaPackageName))
            EditorGUILayout.HelpBox("必须提供 Lua 文件所在模块，否则该 Lua 组件不会运行", MessageType.Warning);

        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }
    private void DrawVarsInspector()
    {
        EditorGUILayout.BeginVertical(styleCNBox);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("当前参数个数：" + pLuaInitialVars.arraySize);

        EditorGUILayout.HelpBox("提示：这些参数仅用于LUA对象初始化时来传递参数使用的，如果你在LUA中修改了变量值，或是在其他脚本中访问修改，" +
            "其不会自动更新，你需要手动调用 UpdateVarFromLua UpdateVarToLua 来更新对应数据。", MessageType.Info);

        EditorGUILayout.Space(5);

        reorderableList.DoLayoutList();

        if(!EditorApplication.isPlaying)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(" 提示：在运行时才能调用更新函数", MessageType.Info);
        }

        EditorGUILayout.Space(5);

        GUI.enabled = EditorApplication.isPlaying;
        if (GUILayout.Button("UpdateAllVarToLua"))
            myScript.UpdateAllVarToLua();
        if (GUILayout.Button("UpdateAllVarFromLua"))
            myScript.UpdateAllVarFromLua();
        GUI.enabled = true;

        EditorGUILayout.Space(5);

        EditorGUILayout.EndVertical();
    }
    private void DrawEventCallerInspector()
    {
        EditorGUILayout.BeginVertical(styleCNBox);
        EditorGUILayout.Space(5);

        foreach (GameLuaObjectEventCaller c in events)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.BeginHorizontal();//b Title
            EditorGUILayout.LabelField(c.GetType().Name, GUI.skin.GetStyle("AssetLabel"));
            if (GUILayout.Button("", GUI.skin.GetStyle("OL Minus")))//del
                DestroyImmediate(c);
            EditorGUILayout.EndHorizontal();//e Title

            foreach (string s in c.GetSupportEvents())
                EditorGUILayout.LabelField(s);

            EditorGUILayout.EndVertical();
        }
        if (events.Count == 0)
            EditorGUILayout.HelpBox("这里还没有接收器，可点击下方按钮添加", MessageType.None);

        EditorGUILayout.Space(13);

        EditorGUILayout.BeginHorizontal();//b添加事件接收器
        EditorGUILayout.LabelField("添加事件接收器", GUILayout.Width(100));
        currentAddEventType = (GameLuaObjectEventWarps)
            EditorGUILayout.EnumPopup(currentAddEventType, GUILayout.Width(80));
        if (GUILayout.Button("添加事件接收器"))
            AddEventCaller();

        EditorGUILayout.EndHorizontal();//e添加事件接收器

        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }

    private void AddEventCaller()
    {
        if (currentAddEventType == GameLuaObjectEventWarps.Unknow)
        {
            EditorUtility.DisplayDialog("提示", "请选择要添加的事件接收器", "好的");
            return;
        }

        foreach (GameLuaObjectEventCaller c in events)
        {
            if (c.GetEventType() == currentAddEventType)
            {
                EditorUtility.DisplayDialog("提示", "要添加的事件接收器 " + currentAddEventType + " 已经存在", "好的");
                return;
            }
        }

        switch (currentAddEventType)
        {
            case GameLuaObjectEventWarps.Physics:
                myScript.gameObject.AddComponent<GameLuaObjectPhysicsEventCaller>();
                break;
            case GameLuaObjectEventWarps.Physics2D:
                myScript.gameObject.AddComponent<GameLuaObjectPhysics2DEventCaller>();
                break;
            case GameLuaObjectEventWarps.Mouse:
                myScript.gameObject.AddComponent<GameLuaObjectMouseEventCaller>();
                break;
            case GameLuaObjectEventWarps.Animator:
                myScript.gameObject.AddComponent<GameLuaObjectAnimatorEventCaller>();
                break;
            case GameLuaObjectEventWarps.Particle:
                myScript.gameObject.AddComponent<GameLuaObjectParticleEventCaller>();
                break;
            case GameLuaObjectEventWarps.Other:
                myScript.gameObject.AddComponent<GameLuaObjectOtherEventCaller>();
                break;
        }
    }
}
