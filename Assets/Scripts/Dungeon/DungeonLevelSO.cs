using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DungeonLevelSO_", menuName = "Scriptable Objects/Dungeon/Dungeon Level")]
public class DungeonLevelSO : ScriptableObject
{
    #region Header BASIC LEVEL DETAILS
    [Space(10)]
    [Header("BASIC LEVEL DETATLS")]
    #endregion Header BASIC LEVEL DETATLS

    #region Tooltip
    [Header("The name of the level")]
    #endregion Tooltip

    public string levelName;

    #region Header ROOM TEMPLATES FOR LEVEL
    [Space(10)]
    [Header("ROOM TEMPLATES FOR LEVEL")]
    #endregion Header ROOM TEMPLATES FOR LEVEL
    #region Tooltip
    [Tooltip("����б�,ȷ������ģ������������йؿ�����ڵ�ͼ��ָ���ķ�������")]
    #endregion

    public List<RoomTemplateSO> roomTemplateList;

    #region Header ROOM NODE GRAPH FOR LEVEL
    [Space(10)]
    [Header("ROOM NODE GRAPH FOR LEVEL")]
    #endregion Header ROOM NODE GRAPH FOR LEVEL
    #region Tooltip
    [Tooltip("�ùؿ�����ѡ��ķ���ڵ�ͼ����б�")]
    #endregion

    public List<RoomNodeGraphSO> roomNodeGraphList;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidataCheckEmptyString(this, nameof(levelName), levelName);
        if (HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomTemplateList), roomTemplateList))
            return;
        if (HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomNodeGraphList), roomNodeGraphList))
            return;
        
        bool isCorridorEW = false;
        bool isCorridorNS = false;
        bool isEntrance = false;
        foreach (RoomTemplateSO roomTemplateSO in roomTemplateList)
        {
            if (roomTemplateSO == null)
                return;

            if(roomTemplateSO.roomNodeType.isCorridorEW == true)
                isCorridorEW= true;
            if (roomTemplateSO.roomNodeType.isCorridorNS == true)
                isCorridorNS = true;
            if (roomTemplateSO.roomNodeType.isEntrance == true)
                isEntrance = true;
        }

        if(isCorridorEW == false)
        {
            Debug.Log("In" + this.name.ToString() + ": No E/W Corridor Room Type Specified");
        }

        if (isCorridorNS == false)
        {
            Debug.Log("In" + this.name.ToString() + ": No N/S Corridor Room Type Specified");
        }

        if (isEntrance== false)
        {
            Debug.Log("In" + this.name.ToString() + ": No E/W Entrance Room Type Specified");
        }

        foreach(RoomNodeGraphSO roomNodeGraph in roomNodeGraphList )
        {
            if (roomNodeGraph == null)
                return;
            foreach(RoomNodeSO roomNodeSO in roomNodeGraph.roomNodeList)
            {
                if(roomNodeSO == null) continue;

                if (roomNodeSO.roomNodeType.isCorridorEW || roomNodeSO.roomNodeType.isCorridorNS || roomNodeSO.roomNodeType.isEntrance ||
                    roomNodeSO.roomNodeType.isCorridor || roomNodeSO.roomNodeType.isNone)
                    continue;

                bool isRoomNodeTypeFound = false;

                foreach(RoomTemplateSO roomTemplateSO in roomTemplateList )
                {
                    if( roomTemplateSO == null) continue;

                    if(roomTemplateSO.roomNodeType == roomNodeSO.roomNodeType )
                    {
                        isRoomNodeTypeFound=true; 
                        break;
                    }
                }

                if(!isRoomNodeTypeFound)
                    Debug.Log("In" + this.name.ToString() + ": No room template " + roomNodeSO.roomNodeType.name.ToString() + 
                        "found for node graph" + roomNodeGraph.name.ToString());
            }
        }

      
    }

#endif
    #endregion
}