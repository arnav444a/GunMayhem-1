using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
#endif

public static partial class Core
{
    public interface ISerializeData
    {
        bool Load(string path);
        void Save(string path);
    }

    public class SerializeDataHandler<T> : ISerializeData where T : new()
    {
        T data = new T();
        public SerializeDataHandler()
        {
        }

        public T Data
        {
            get
            {
                return data;
            }
        }

        public bool Load(string path)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using var stream = new FileStream(path, FileMode.Open);
                data = (T)serializer.Deserialize(stream);
                return data != null;
            }
            catch (UnityException e)
            {
                Debug.Log(e.Message);
                return false;
            }
        }

        public void Save(string path)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    serializer.Serialize(stream, data);
                }
            }
            catch (UnityException e)
            {
                Debug.Log(e.Message);
            }
        }
    }

    const string INDENT_STRING = "    ";
    public static string FormatJson(string str)
    {
        var indent = 0;
        var quoted = false;
        var sb = new StringBuilder();
        for (var i = 0; i < str.Length; i++)
        {
            var ch = str[i];
            switch (ch)
            {
                case '{':
                case '[':
                    sb.Append(ch);
                    if (!quoted)
                    {
                        sb.AppendLine();
                        Enumerable.Range(0, ++indent).ForEach(item => sb.Append(INDENT_STRING));
                    }
                    break;
                case '}':
                case ']':
                    if (!quoted)
                    {
                        sb.AppendLine();
                        Enumerable.Range(0, --indent).ForEach(item => sb.Append(INDENT_STRING));
                    }
                    sb.Append(ch);
                    break;
                case '"':
                    sb.Append(ch);
                    bool escaped = false;
                    var index = i;
                    while (index > 0 && str[--index] == '\\')
                        escaped = !escaped;
                    if (!escaped)
                        quoted = !quoted;
                    break;
                case ',':
                    sb.Append(ch);
                    if (!quoted)
                    {
                        sb.AppendLine();
                        Enumerable.Range(0, indent).ForEach(item => sb.Append(INDENT_STRING));
                    }
                    break;
                case ':':
                    sb.Append(ch);
                    if (!quoted)
                        sb.Append(" ");
                    break;
                default:
                    sb.Append(ch);
                    break;
            }
        }
        return sb.ToString();
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class SerializableClassAttribute : PropertyAttribute
    {

    }

#if UNITY_EDITOR
    public class AdvancedTypePopupItem : AdvancedDropdownItem
    {
        public Type Type { get; }
        public AdvancedTypePopupItem(Type type, string name) : base(name)
        {
            Type = type;
        }
    }

    [CustomPropertyDrawer(typeof(SerializableClassAttribute))]
    public class SerializableClassDrawer : PropertyDrawer
    {
        SerializedProperty _targetProperty;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                Rect popupPosition = new Rect(position);
                popupPosition.width -= EditorGUIUtility.labelWidth;
                popupPosition.x += EditorGUIUtility.labelWidth;
                popupPosition.height = EditorGUIUtility.singleLineHeight;
                if (EditorGUI.DropdownButton(popupPosition, GetTypeName(property), FocusType.Keyboard))
                {
                    _targetProperty = property;
                    var popup = GetTypePopup(property);
                    popup.Show(popupPosition);
                }
                EditorGUI.PropertyField(position, property, label, true);
            }
            else
            {
                EditorGUI.LabelField(position, label, new GUIContent("The property type is not manage reference."));
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, true);
        }

        AdvancedTypePopup GetTypePopup(SerializedProperty property)
        {
            string managedReferenceFieldTypename = property.managedReferenceFieldTypename;
            AdvancedTypePopup result = null;
            var state = new AdvancedDropdownState();

            Type baseType = GetTypeFromName(managedReferenceFieldTypename);

            if (Attribute.IsDefined(baseType, typeof(SerializableAttribute))
                || baseType.IsInterface)
            {
                var list = TypeCache.GetTypesDerivedFrom(baseType).Where(p =>
                        (p.IsPublic || p.IsNestedPublic)
                        && !p.IsAbstract && !p.IsGenericType
                        /*&& !typeof(UnityEngine.Object).IsAssignableFrom(p)*/);
                var popup = new AdvancedTypePopup(list, 13, state);
                popup.OnItemSelected += item =>
                {
                    Type type = item.Type;
                    object obj = (type != null) ? Activator.CreateInstance(type) : null;
                    _targetProperty.managedReferenceValue = obj;
                    _targetProperty.isExpanded = (obj != null);
                    _targetProperty.serializedObject.ApplyModifiedProperties();
                    _targetProperty.serializedObject.Update();
                };

                result = popup;
            }
            return result;
        }

        static Type GetTypeFromName(string typeName)
        {
            int splitIndex = typeName.IndexOf(' ');
            var assembly = System.Reflection.Assembly.Load(typeName[..splitIndex]);
            return assembly.GetType(typeName[(splitIndex + 1)..]);
        }

        static GUIContent GetTypeName(SerializedProperty property)
        {
            string managedReferenceFullTypename = property.managedReferenceFullTypename;

            if (string.IsNullOrEmpty(managedReferenceFullTypename))
            {
                return new GUIContent("<null>");
            }

            Type type = property.managedReferenceValue.GetType();
            string typeName = null;

            if (string.IsNullOrWhiteSpace(typeName))
            {
                typeName = ObjectNames.NicifyVariableName(type.Name);
            }


            GUIContent result = new GUIContent(typeName);
            return result;
        }

        static string[] GetBreadCrumbTypeName(Type type)
        {
            string[] splits = type.FullName.Split(new char[] { '.', '+' }, StringSplitOptions.RemoveEmptyEntries);
            return splits;
        }

        public class AdvancedTypePopup : AdvancedDropdown
        {
            Type[] _types;
            public event Action<AdvancedTypePopupItem> OnItemSelected;
            public AdvancedTypePopup(IEnumerable<Type> types, int maxLineCount, AdvancedDropdownState state) : base(state)
            {
                _types = types.ToArray();
                minimumSize = new Vector2(minimumSize.x, EditorGUIUtility.singleLineHeight * maxLineCount);
            }
            protected override AdvancedDropdownItem BuildRoot()
            {
                var root = new AdvancedDropdownItem("Select Type");
                int itemCount = 0;

                var nullItem = new AdvancedTypePopupItem(null, "<null>")
                {
                    id = itemCount++
                };
                root.AddChild(nullItem);

                foreach (Type type in _types)
                {
                    string[] typeNames = GetBreadCrumbTypeName(type);
                    if (typeNames.Length == 0) continue;

                    AdvancedDropdownItem parent = root;

                    if (typeNames.Length > 1)
                    {
                        foreach (string typeName in typeNames.SkipLast(1))
                        {
                            AdvancedDropdownItem foundItem = parent.children.FirstOrDefault(p => p.name == typeName);
                            if (foundItem != null)
                            {
                                parent = foundItem;
                            }
                            else
                            {
                                var newItem = new AdvancedDropdownItem(typeName)
                                {
                                    id = itemCount++,
                                };
                                parent.AddChild(newItem);
                                parent = newItem;
                            }
                        }
                    }

                    string nicify = ObjectNames.NicifyVariableName(typeNames[^1]);
                    var item = new AdvancedTypePopupItem(type, nicify)
                    {
                        id = itemCount++
                    };
                    parent.AddChild(item);
                }

                return root;
            }
            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                base.ItemSelected(item);
                if (item is AdvancedTypePopupItem typePopupItem)
                {
                    OnItemSelected?.Invoke(typePopupItem);
                }
            }
        }
    }
#endif
}