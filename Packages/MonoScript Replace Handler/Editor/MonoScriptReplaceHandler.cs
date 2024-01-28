using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public class MonoScriptReplaceHandler
{
    [MenuItem("CONTEXT/Component/Replace")]
    public static void OnScriptsReloaded(MenuCommand command)
    {
        var window = EditorWindow.CreateInstance<ReplaceHandler>();
        window.serializedObject = new SerializedObject(command.context);
        window.ShowUtility();
    }
    [MenuItem("CONTEXT/Component/Replace", validate = true)]
    public static bool ValidateComponent(MenuCommand command) => command.context is not Transform;
}

public class ReplaceHandler : EditorWindow
{
    [Header("用于替换的脚本："), Tooltip("使用指定的脚本替换当前组件")]
    public MonoScript source;
    SerializedProperty property;
    SerializedObject origin_so;
    static GameObject origin_go;
    MonoScript origin_script;
    static bool replaceOccured;
    public SerializedObject serializedObject
    {
        get => origin_so;
        set
        {
            origin_so = value;
            origin_go = (value.targetObject as Behaviour).gameObject;
            origin_script = value.FindProperty("m_Script").objectReferenceValue as MonoScript;
            properties_origin = CollectProperty(value);
        }
    }

    #region Preview
    private List<Property> properties_origin; //property of the origin one
    private Vector2 scrollPosition;
    #endregion
    struct Property
    {
        public bool isCollection; //Array 和 List 等效
        public string name;
        public string type; // 如果是 Collection 则是元素类型
        public PropState state; // 确认在替换脚本后字段的存续状态
        public Property(SerializedProperty property)
        {
            state = PropState.None;
            name = property.name;
            isCollection = property.propertyPath.Contains("Array");
            type = isCollection ? property.arrayElementType : property.type;
        }
        public override readonly bool Equals(object obj)
        {
            return isCollection == ((Property)obj).isCollection
                && name == ((Property)obj).name
                && type == ((Property)obj).type;
        }
        public override readonly int GetHashCode() => base.GetHashCode();
        public override readonly string ToString() => $"{name}({type}) - {isCollection}:{state}";
    }
    enum PropState
    {
        None,//未确认
        Same, //相同
        Mismatch,//字段存在但类型不匹配
        Missing, // 字段将会消失
    }

    List<Property> CollectProperty(SerializedObject target)
    {
        var properties = new List<Property>();
        var property = target.GetIterator();
        property.NextVisible(true);
        while (property.NextVisible(false))
        {
            if (property.name == "m_Script")
                continue;
            properties.Add(new Property(property));
        }
        return properties;
    }

    private List<Property> CollectSourceProperty(MonoScript source)
    {
        var preview = new GameObject();
        preview.SetActive(false); // should not polution the scene in case of  case null exception
        preview.hideFlags = HideFlags.HideAndDontSave;
        var component = preview.AddComponent(source.GetClass());
        using var so = new SerializedObject(component);
        var properties = CollectProperty(so);
        DestroyImmediate(preview);
        return properties;
    }

    void RecalculatePropState(ref List<Property> target, List<Property> source)
    {
        for (int i = 0; i < target.Count; i++)
        {
            var item = target[i];
            item.state = PropState.None;
            var index = source.FindIndex(v => v.name == item.name); //首要条件是 name 相同
            if (index != -1)
            {
                if (item.Equals(source[index]))
                {
                    item.state = PropState.Same;
                }
                else
                {
                    item.state = PropState.Mismatch;
                }
                source.RemoveAt(index);
            }
            else
            {
                item.state = PropState.Missing;
            }
            target[i] = item;
        }
    }

    private Color GetColor(SerializedProperty property, Color fallback)
    {
        var info = properties_origin.Find((item) => item.name == property.name);
        return info.state switch
        {
            PropState.Same => Color.green,
            PropState.Mismatch => Color.yellow,
            PropState.Missing => Color.red,
            _ => fallback,
        };
    }

    private void OnEnable()
    {
        var so = new SerializedObject(this);
        property = so.FindProperty("source");
        this.titleContent = new GUIContent("MonoScript Replace Handler");
    }

    [InitializeOnLoadMethod]
    private static void RedoPerformed()
    {
        Undo.undoRedoPerformed -= OnUndoRedo;
        Undo.undoRedoPerformed += OnUndoRedo;
    }

    private static void OnUndoRedo()
    {
        if (replaceOccured && (PrefabUtility.IsPartOfPrefabAsset(origin_go) || PrefabStageUtility.GetCurrentPrefabStage()))
        {
            Selection.activeGameObject = null;
            replaceOccured = false;
        }
    }

    private void OnGUI()
    {
        property.serializedObject.Update();
        using var change = new EditorGUI.ChangeCheckScope();
        EditorGUILayout.PrefixLabel("想要替换的组件：");
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.ObjectField(origin_content, serializedObject.targetObject, origin_script.GetClass(), true);
        }
        EditorGUILayout.PropertyField(property);
        if (change.changed)
        {
            property.serializedObject.ApplyModifiedProperties();
        }
        var canConvert = source && source.GetClass()?.IsSubclassOf(typeof(MonoBehaviour)) == true;
        try
        {
            if (change.changed && canConvert)
            {
                properties_origin = CollectProperty(serializedObject);
                var prop_src = CollectSourceProperty(source);
                RecalculatePropState(ref properties_origin, prop_src);
            }
        }
        catch (Exception)
        {
            // do nothing as we just gathering serialized property info
        }

        var message = string.Empty;
        var messagetype = MessageType.Info;
        if (!canConvert)
        {
            GUILayout.FlexibleSpace();
            message = "请提供支持挂载到属性检视面板的脚本！";
            messagetype = MessageType.Warning;
            cancel_content.tooltip = comfirm_content.tooltip = "请指定正确的用于替换的源脚本！";
        }
        else
        {
            EditorGUILayout.LabelField("效果预览", EditorStyles.boldLabel);
            using var scroll = new EditorGUILayout.ScrollViewScope(scrollPosition, GUI.skin.box);
            var property = serializedObject.GetIterator();
            property.NextVisible(true);
            using var scope = new EditorGUI.DisabledScope(true);
            while (property.NextVisible(false))
            {
                if (property.name == "m_Script")
                    continue;
                var color = GUI.color;
                GUI.color = GetColor(property, color);
                EditorGUILayout.PropertyField(property, true);
                GUI.color = color;
            }
            message = "绿色：字段序列化数据可以被复用\n黄色：字段相同但类型不匹配！\n红色：字段将丢失！";
            messagetype = MessageType.Info;
            scrollPosition = scroll.scrollPosition;
            cancel_content.tooltip = comfirm_content.tooltip = "";
        }
        var richText = EditorStyles.helpBox.richText;
        EditorStyles.helpBox.richText = true;
        EditorGUILayout.HelpBox(message, messagetype);
        EditorStyles.helpBox.richText = richText;

        GUI.enabled = canConvert;
        using var hz = new EditorGUILayout.HorizontalScope();
        if (GUILayout.Button(cancel_content))
        {
            Close();
        }
        if (GUILayout.Button(comfirm_content))
        {
            ConvertMonoScript(serializedObject, source);
            EditorUtility.SetDirty(origin_go);
            Close();
            replaceOccured = true;
        }
    }

    private void ConvertMonoScript(SerializedObject so_Target, MonoScript source)
    {
        so_Target.UpdateIfRequiredOrScript();
        so_Target.FindProperty("m_Script").objectReferenceValue = source;
        so_Target.ApplyModifiedProperties();
    }

    #region GUIContent
    readonly GUIContent cancel_content = new("取消");
    readonly GUIContent comfirm_content = new("确定");
    readonly GUIContent origin_content = new("Target");
    #endregion
}
