using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using PortalGame;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

/// <summary>
/// <see cref="PropertyDrawer"/> que mostra no inspetor
/// um tipo de dicionario customizado.
/// </summary>
[CustomPropertyDrawer(typeof(UnityDictionary<,>), true)]
public class UnityDictionaryPropertyDrawer : PropertyDrawer {

    private VisualElement container;

    private ListView itemsListView;
    private Toggle allowDuplicatesToggle;

    public override VisualElement CreatePropertyGUI(SerializedProperty property) {
        // cria a arvore
        var vsTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Editor/UnityDictionaryTree.uxml");
        container = vsTree.CloneTree(property.propertyPath);

        var foldout = container.Q<Foldout>("Foldout");
        foldout.text = property.displayName;

        // binda a checkbox
        allowDuplicatesToggle = container.Q<Toggle>("allowDuplicatesToggle");
        var duplicatesProperty = property.FindPropertyRelative("m_duplicatesAllowed");
        allowDuplicatesToggle.BindProperty(duplicatesProperty);

        // setar a list view
        itemsListView = container.Q<ListView>("itemsListView");
        var dictProperty = property.FindPropertyRelative("m_Dictionary");

        // inicializa a listview com os itens do dicionario
        itemsListView.itemsSource = GetKeyValuePairs(dictProperty);
        itemsListView.makeItem = () => new PropertyField();
        itemsListView.bindItem = (element, i) => {
            var itemProperty = dictProperty.GetArrayElementAtIndex(i);
            (element as PropertyField).BindProperty(itemProperty);
        };

        itemsListView.itemsAdded += (indicesAdded) => {
            foreach (var index in indicesAdded) {
                Debug.Log("Index added: " + index);
                dictProperty.InsertArrayElementAtIndex(index);

                // setar o valor padrao do item adicionado
                var itemProperty = dictProperty.GetArrayElementAtIndex(index);
                var keyProperty = itemProperty.FindPropertyRelative("key");
                var valueProperty = itemProperty.FindPropertyRelative("value");

                SetDefaultValue(keyProperty);
                SetDefaultValue(valueProperty);
            }
            property.serializedObject.ApplyModifiedProperties();
        };

        itemsListView.itemsRemoved += (indicesRemoved) => {
            foreach (var index in indicesRemoved) {
                Debug.Log("Index removed: " + index);
                dictProperty.DeleteArrayElementAtIndex(index);
            }
            property.serializedObject.ApplyModifiedProperties();
        };

        return container;
    }

    private List<SerializedProperty> GetKeyValuePairs(SerializedProperty dictProperty) {
        List<SerializedProperty> keyValuePairs = new();
        for (int i = 0; i < dictProperty.arraySize; i++) {
            keyValuePairs.Add(dictProperty.GetArrayElementAtIndex(i));
        }
        return keyValuePairs;
    }

    private void SetDefaultValue(SerializedProperty property) {
        if (property.propertyType == SerializedPropertyType.String) {
            property.stringValue = string.Empty;
        } else if (property.propertyType == SerializedPropertyType.Integer) {
            property.intValue = 0;
        } else if (property.propertyType == SerializedPropertyType.Float) {
            property.floatValue = 0;
        } else if (property.propertyType == SerializedPropertyType.Boolean) {
            property.boolValue = false;
        } else if (property.propertyType == SerializedPropertyType.Enum) {
            property.enumValueIndex = 0;
        } else if (property.propertyType == SerializedPropertyType.ObjectReference) {
            property.objectReferenceValue = null;
        }
    }
}