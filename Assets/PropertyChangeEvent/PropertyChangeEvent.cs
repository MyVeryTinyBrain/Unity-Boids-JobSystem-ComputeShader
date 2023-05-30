using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
public class PropertyChangeEventAttribute : Attribute
{
    public string CallbackName { get; private set; }
    public PropertyChangeEventAttribute(string callbackName)
    {
        CallbackName = callbackName;
    }
}

#if UNITY_EDITOR
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class AllowPropertyChangeEventTargetAttribute : UnityEditor.CustomEditor
{
    public AllowPropertyChangeEventTargetAttribute(Type inspectedType) : base(inspectedType, true) { }
    public AllowPropertyChangeEventTargetAttribute(Type inspectedType, bool editorForChildClasses) : base(inspectedType, editorForChildClasses) { }
}

[CanEditMultipleObjects]
public class AllowPropertyChangeEventEditor : UnityEditor.Editor
{
    private List<MethodInfo> callbacks = new List<MethodInfo>();
    private bool undoRedoPerformed;

    protected virtual void OnEnable()
    {
        callbacks.Clear();

        Undo.undoRedoPerformed += OnUndoRedoPerformed;
        undoRedoPerformed = false;
    }

    protected virtual void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoRedoPerformed;
    }

    private void OnUndoRedoPerformed()
    {
        undoRedoPerformed = true;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        using (SerializedProperty property = serializedObject.GetIterator())
        {
            if (property.NextVisible(true))
            {
                do
                {
                    PropertyField(property);
                }
                while (property.NextVisible(false));
            }
        }

        serializedObject.ApplyModifiedProperties();

        undoRedoPerformed = false; 

        InvokeCallbacks();
    }

    void PropertyField(SerializedProperty property)
    {
        if (property.name.Equals("m_Script", StringComparison.Ordinal))
        {
            using (new EditorGUI.DisabledScope(disabled: true))
            {
                EditorGUILayout.PropertyField(property, true);
            }
        }
        else
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(property, true);
            if (EditorGUI.EndChangeCheck() || undoRedoPerformed)
            {
                Type classType = target.GetType();
                FieldInfo propertyFieldInfo = classType.GetField(property.name, (BindingFlags)(~0));
                IEnumerable<PropertyChangeEventAttribute> attributes = propertyFieldInfo.GetCustomAttributes<PropertyChangeEventAttribute>();
                foreach (PropertyChangeEventAttribute attribute in attributes)
                {
                    MethodInfo bindingMethodInfo = classType.GetMethod(attribute.CallbackName, (BindingFlags)(~0), null, new Type[0] { }, null);
                    callbacks.Add(bindingMethodInfo);
                }
            }
        }
    }

    void InvokeCallbacks()
    {
        foreach(MethodInfo methdo in callbacks)
        {
            methdo.Invoke(target, null);
        }
        callbacks.Clear();
    }
}

public class AllowPropertyChangeEvent : AllowPropertyChangeEventEditor { }
#else
public class AllowPropertyChangeEventTargetAttribute : Attribute
{
    public AllowPropertyChangeEventTargetAttribute(Type inspectedType) { }
    public AllowPropertyChangeEventTargetAttribute(Type inspectedType, bool editorForChildClasses) { }
}
public class AllowPropertyChangeEvent { }
#endif