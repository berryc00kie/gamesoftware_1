using UnityEditor;
using UnityEngine;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

//Author: Alexey "DataGreed" Strelkov
//Date: 2019-05-31
//Title: Grouping Tool

/// <summary>
/// Custom menu items for grouping game objects from GameObject menu
/// or Hierarchy pane
/// Place in Assets/{whatever}/Editor/ or just Assets/Editor
/// </summary>
public class GroupObjectsEditor : MonoBehaviour
{
    internal const string version = "1.0.0"; 

    private static Transform[] lastSelectedTransforms;
    private static int selectedItemsLeft;
    private static Dictionary<Transform, int> hierarchyIndexes;

    internal static string templateGroupEditorPrefName = "pro.datagreed.grouptool.GroupPrefab."+Application.productName;
    private static GameObject _templateGroupObject;

    /// <summary>
    /// Holds template for creating groups with custom template set as parent.
    /// Automatically saves object to EditorPrefs and Loads it.
    /// </summary>
    /// <value>The template group object.</value>
    public static GameObject templateGroupObject
    {
        get
        {
            //try returning cached object
            if (_templateGroupObject) return _templateGroupObject;

            //-1 is a special case indicating that there is no value
            int instanceID =  EditorPrefs.GetInt(templateGroupEditorPrefName, -1);

            if (instanceID<0)
            {
                return null;
            }
            else
            {
                UnityEngine.Object obj = EditorUtility.InstanceIDToObject(instanceID);
                //if (!obj)
                //{
                //    Debug.LogError("No object could be found with instance id: " + instanceID);
                //}

                return (GameObject)obj;
            }

        }

        set
        {
            //if object is the same as cached, ignore it to avoid unnecessary 
            //disk I/O
            if (value == _templateGroupObject) return;

            //cache value
            _templateGroupObject = value;


            if (value)
            {
                EditorPrefs.SetInt(templateGroupEditorPrefName, value.GetInstanceID());
            }
            else
            {
                EditorPrefs.SetInt(templateGroupEditorPrefName, -1);
            }
        }
    }


    
    [MenuItem("GameObject/Group Selected  %g", false, 0)]
    static void GroupObjectsCommand(MenuCommand menuCommand)
    {
        //group object with default empty object
        GroupObjects(menuCommand);
    }

    // Add a menu item to create group with custom object set as parent
    // Priority 1 ensures it is grouped with the other menu items of the same kind
    // and propagated to the hierarchy dropdown and hierarchy context menus.
    //see https://docs.unity3d.com/ScriptReference/MenuItem.html for docs
    //also https://blog.redbluegames.com/guide-to-extending-unity-editors-menus-b2de47a746db
    [MenuItem("GameObject/Group With Template  %#g", false, 0)]
    static void GroupObjectsWithCustomTemplatCommand(MenuCommand menuCommand)
    {
        if(!templateGroupObject)
        {
            Debug.LogWarning("Template group object not set. Please set it in \"Menu > Window > Grouping Tool\" and try again");
            //show window to make it ore obvious where to find the setting
            GropToolPrefsWindow.ShowWindow();
            return;
        }
        //group objects with an object from template
        GroupObjects(menuCommand, templateGroupObject);
    }




    /// <summary>
    /// Actual method that groups the objects together under same parent.
    /// </summary>
    /// <param name="menuCommand">Menu command.</param>
    /// <param name="groupObjectPrefab">Group object prefab.</param>
    static void GroupObjects(MenuCommand menuCommand, GameObject groupObjectPrefab=null)
    {
        //unity executes the code for every menu item selected in Hierarchy pane context menu
        //(https://issuetracker.unity3d.com/issues/menuitem-is-executed-more-than-once-when-multiple-objects-are-selected)
        //but we have to execute it only once
        //here we protect from executing code multiple times
        if (lastSelectedTransforms!=null)
        {
            if (lastSelectedTransforms.SequenceEqual(Selection.transforms))
            {
                selectedItemsLeft--;
                if (selectedItemsLeft <=0)
                {
                    //clearing the selection
                    //so the user can actually click on menu item again
                    //and it will work
                    lastSelectedTransforms = null;
                }

                return;
            }
           
        }

        // nothing is selected; abort
        if (!Selection.activeTransform) return;

        //save number of times the context function will be called again
        //for this group of items so we will ignore it
        //we sholud save it before iterating over Selection.transforms
        //otherwise it will return zero (for some reason, probably
        //because we changed their parents)
        selectedItemsLeft = Selection.transforms.Length - 1;
        //do the grouping

        //instantiate new object that will be the parent of this group
        GameObject go;
        if (groupObjectPrefab)
        {
            go = Instantiate(groupObjectPrefab);
            go.name = $"Group {groupObjectPrefab.name}";
        }
        else
        {
            //create an empty one if no prefab specified
            go = new GameObject("Group");
        }

        //register created object in Undo stack
        Undo.RegisterCreatedObjectUndo(go, "Group Selected");


        //find the parent of the group
        Transform groupParent;
        //set fallback
        groupParent = Selection.activeTransform.parent;
        bool parentFound = false;
        //save sibling indexes of every item so we can keep their order
        hierarchyIndexes = new Dictionary<Transform, int>();
        int groupSiblingIndex = 999999999;

        //find the object in the selection that is
        //located on the highest hierarchy level
        //so we can get its parent
        foreach (Transform transform in Selection.transforms)
        {
            if (!parentFound)
            {
                if (!Array.Exists(Selection.transforms, element => element == transform.parent))
                {
                    //found transform who does not have parent inside of selection
                    //so it's highest level
                    groupParent = transform.parent;
                    parentFound = true;


                }
            }

            //save sibling index (order in hierarchy) so we can restore the order later
            hierarchyIndexes[transform] = transform.GetSiblingIndex();

            //save the lowest sibling index to inser group at the position of highest element
            groupSiblingIndex = Math.Min(groupSiblingIndex, transform.GetSiblingIndex());
        }

        //add group object in hierarchy at correct level
        go.transform.SetParent(groupParent, false);
        //insert transform in correct position (ordering) in the hierarchy
        go.transform.SetSiblingIndex(groupSiblingIndex);

        //sort selected items by there index
        IEnumerable<Transform> sortedByIndex = Selection.transforms.OrderByDescending(t => hierarchyIndexes[t]);

        //copy position of the highest selected item in list to the newly created group
        //otherwise it will appear at world origin (0,0,0)
        //and we don't want that (usually)
        go.transform.position = sortedByIndex.Last<Transform>().position;


        //set all selected objects as childs of out newly created Group object
        foreach (var transform in Selection.transforms)
        {
            Undo.SetTransformParent(transform, go.transform, "Group Selected");           
        }

        //re-sort items so they will be in the same order as before grouping
        foreach (var item in sortedByIndex)
        {
            //we cannot just copy the old sibling index values since they need 
            //to be sequential and zero-based (e.g. 0,1,2,3,4)
            item.SetAsFirstSibling();
        }

        //set editor selection to newly created group object
        Selection.activeGameObject = go;

        //free for garbage collection
        hierarchyIndexes = null;

        //if invoked from context menu
        //saved the last selected transforms so we won't run the command multiple
        //times instead of one
        if (menuCommand.context)
        {
            lastSelectedTransforms = Selection.transforms;
        }
    }


}

/// <summary>
/// Custom editor window that stores preferences for Grouping tool
/// </summary>
[Serializable]
class GropToolPrefsWindow : EditorWindow
{

    public static string windowTitle = "Grouping Tool";

    [SerializeField]
    public GameObject templateGroupObjectInputField;

    [MenuItem("Window/Grouping Tool")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(GropToolPrefsWindow), false, windowTitle);
    }

    /// <summary>
    /// Draws Editor GUI for setting preferences
    /// </summary>
    void OnGUI()
    {
        GUILayout.Label("Template Settings", EditorStyles.boldLabel);

        templateGroupObjectInputField = (GameObject)EditorGUILayout.ObjectField("Group Template", GroupObjectsEditor.templateGroupObject, typeof(GameObject), false);
        EditorGUILayout.HelpBox("Set \"Group Template\" to a prefab that you would like to use for grouping objects on the scene together.\n" +
                                "Use \"Select objects in hierarchy window and select \"Group with template\". \n" +
                                "You can still group objects with an empty GameObject by selecting \"Group Selected\".",
                                MessageType.Info);

        GroupObjectsEditor.templateGroupObject = templateGroupObjectInputField;

        GUILayout.FlexibleSpace();
        GUILayout.Label($"Version {GroupObjectsEditor.version}", EditorStyles.miniLabel);
    }
}