using System.Collections;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

public class RoomNodeSO : ScriptableObject
{
    public string id;
    public List<string> parentRoomNodeIDList = new List<string>();
    public List<string> childRoomNodeIDList = new List<string>();
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph;
    public RoomNodeTypeSO roomNodeType;
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;

    #region Editor Code

#if UNITY_EDITOR
    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickDragging = false;
    [HideInInspector] public bool isSelected = false;
    public void Initialise(Rect rect, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeType)
    {
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.roomNodeGraph = nodeGraph;
        this .roomNodeType = roomNodeType;
        //Load room node type list
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    public void Draw(GUIStyle nodeStyle)
    {
        GUILayout.BeginArea(rect, nodeStyle);

        EditorGUI.BeginChangeCheck();

        if (parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance)
        {
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else
        {
            //ѡ���б������͵�����
            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);
            //�û�ѡ���Ժ������
            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());
            //ȷ���û�ѡ���Ժ������׼����Ⱦ
            roomNodeType = roomNodeTypeList.list[selection];

            if (//���Ȳ����л�����������Ķ���
                roomNodeTypeList.list[selected].isCorridor && !roomNodeTypeList.list[selection].isCorridor 
                //�����������ȵĻ������л�������
                || !roomNodeTypeList.list[selected].isCorridor && roomNodeTypeList.list[selection].isCorridor 
                //��������boss���䲻���л���boss����
                || !roomNodeTypeList.list[selected].isBossRoom && roomNodeTypeList.list[selection].isBossRoom
                )
            {
            //��������ǲ�����������,��ô��ɾ���ýڵ���ӽڵ������,set them free
                    if ( childRoomNodeIDList.Count > 0)
                    {
                        for (int i = childRoomNodeIDList.Count - 1; i >= 0; i--)
                        {
                            RoomNodeSO childRoomNode = roomNodeGraph.GetRoomNode(childRoomNodeIDList[i]);

                            if (childRoomNode != null)
                            {
                                RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);

                                childRoomNode.RemoveParentRoomNodeIDFromRoomNode(id);

                            }
                        }
                    }
                }
        }
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(this);

        }
        GUILayout.EndArea();
    }
    public string[] GetRoomNodeTypesToDisplay()
    {
        string[] roomArray = new string[roomNodeTypeList.list.Count];

        List<string> allRoomNames = roomNodeTypeList.list
        .Where(x => x.displayInNodeGraphEditor)  // ֻѡȡ��Щ������ʾ�ķ�������
        .Select(x => x.name)
        .ToList();

        // �ҵ�����ǰ׺
        string commonPrefix = HelperUtilities.GetCommonPrefix(allRoomNames);
        for (int i = 0; i < roomNodeTypeList.list.Count; i++)
        {
            //  if (roomNodeTypeList.list[i].diaplayInNodeGraphEditor)
            //  {
            //    roomArray[i] = roomNodeTypeList.list[i].name; 
            //  }

            string roomName = roomNodeTypeList.list[i].name;
            // ������������й���ǰ׺��ȥ������ǰ׺
            if (roomName.StartsWith(commonPrefix))
            {
                roomArray[i] = roomName.Substring(commonPrefix.Length);
            }
            else
            {
                roomArray[i] = roomName;
            }
        }
        return roomArray;
    }
    public void ProcessEvents(Event currentEvent)
    {
        switch(currentEvent.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;
            default : break;
        }
    }
    //0��������갴ť��1��������갴ť
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        if(currentEvent.button == 0)
        {
            ProcessLeftClickDownEvent();
        }
        else if(currentEvent.button == 1)
        {
            ProcessRightClickDownEvent(currentEvent);
        }
    }
    private void ProcessLeftClickDownEvent()
    {
        //Selection.activeObject = this;

        if(isSelected == true)
        {
            isSelected = false;
        }
        else
        {
            isSelected = true;
        }
    }
    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
    }
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if(currentEvent.button == 0)
        {
            ProcessLeftClickUpEvent();
        }
    }

    private void ProcessLeftClickUpEvent()
    {
        if(isLeftClickDragging)
        {
            isLeftClickDragging = false;
        }
    }

    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if(currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent);
        }
    }

    private void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;

        DragNode(currentEvent.delta);
        GUI.changed = true;
    }
    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    public bool AddChildRoomIdToRoomNode(string childID)
    {
        if (IsChildRoomValid(childID))
        {
            childRoomNodeIDList.Add(childID);
            return true;
        }
        return false;
    }

    public bool IsChildRoomValid(string childID)
    {
        bool isConnectedBossRoomAlready = false;

        foreach(RoomNodeSO roomNode in roomNodeGraph.roomNodeList)
        {
            if(roomNodeType.isBossRoom && roomNode.parentRoomNodeIDList.Count > 0)
            {
                isConnectedBossRoomAlready = true;
            }
        }
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isBossRoom && isConnectedBossRoomAlready)
        {
            return false;
        }

        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isNone)
        {
            return false;
        }

        if(childRoomNodeIDList.Contains(childID))
        {
            return false;
        }

        if(id == childID)
        {
            return false;
        }

        if(parentRoomNodeIDList.Contains(childID))
        {
            return false;
        }

        if(roomNodeGraph.GetRoomNode(childID).parentRoomNodeIDList.Count > 0)
        {
            return false;
        }
        //���Ȳ�����������
        if(roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && roomNodeType.isCorridor)
        {
            return false ;

        }
        //���䲻��ֱ�����ӷ���
        if(!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && !roomNodeType.isCorridor)
        {
            return false ;
        }

        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count >= Settings.maxchildCorridor)
        {
            return false;
        }

        if(roomNodeGraph.GetRoomNode(childID).roomNodeType.isEntrance)
        {
            return false;
        }

        if(!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count > 0)
        {
            return false;
        }

        return true;
    }

    public bool AddParentRoomIdToRoomNode(string child)
    {
        parentRoomNodeIDList.Add(child);
        return true;
    }
    //һ��remove һ��childroomnode ���� parentroomnode
    public bool RemoveChildRoomNodeIDFromRoomNode(string childID)
    {
        if (childRoomNodeIDList.Contains(childID))
        {
            childRoomNodeIDList.Remove(childID);
            return true;
        }
        return false;
    }

    public bool RemoveParentRoomNodeIDFromRoomNode(string ParentID)
    {
        if (parentRoomNodeIDList.Contains(ParentID))
        {
            parentRoomNodeIDList.Remove(ParentID);
            return true;
        }
        return false;
    }
#endif

    #endregion Editor Code
}
