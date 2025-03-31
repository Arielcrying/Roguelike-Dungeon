using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;


[DisallowMultipleComponent]
public class DungeonBuilder : SingletonMonobehaviour<DungeonBuilder>
{
    public Dictionary<string, Room> dungeonBuilderRoomDictionary = new Dictionary<string, Room>();
    private Dictionary<string, RoomTemplateSO> roomTemplateDictionary = new Dictionary<string, RoomTemplateSO>();
    private List<RoomTemplateSO>  roomTemplateList = null;
    private RoomNodeTypeListSO roomNodeTypeList;
    private bool dungeonBuildSuccessful;

    private void OnEnable()
    {
        // Set dimmed material to off
        GameResources.Instance.dimmedMaterial.SetFloat("Alpha_Slider", 0f);
    }

    private void OnDisable()
    {
        // Set dimmed material to fully visible
        GameResources.Instance.dimmedMaterial.SetFloat("Alpha_Slider", 1f);
    }
    protected override void Awake()
    {
        base.Awake();

        LoadRoomNodeTypeList();

        //���õ���ģʽ���ÿ��Ʋ��ʵ�͸����,1fͨ������ȫ��͸��
       // GameResources.Instance.dimmedMaterial.SetFloat("Alpha_Slider", 1f);
    }

    private void LoadRoomNodeTypeList()
    {
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    //���ɵ���,������return true
    public bool GenerateDungeon(DungeonLevelSO currentDungeonLevel)
    {
        roomTemplateList = currentDungeonLevel.roomTemplateList;

        LoadRoomTemplatesIntoDictionary();

        dungeonBuildSuccessful = false;
        int dungeonBuildAttempts = 0;

        while (!dungeonBuildSuccessful && dungeonBuildAttempts < Settings.maxDungeonBuildAttempts)
        {
            dungeonBuildAttempts++;

            RoomNodeGraphSO roomNodeGraph = SelectRandomRoomNodeGraph(currentDungeonLevel.roomNodeGraphList);

            int dungeonBuildAttemptsForRoomNodeGraph = 0;
            //dungeonBuildSuccessful = false ;

            while (!dungeonBuildSuccessful && dungeonBuildAttemptsForRoomNodeGraph <= Settings.maxDungeonRebuildAttemptsForRoomNodeGraph)
            {
                ClearDungeon();

                dungeonBuildAttemptsForRoomNodeGraph++;

                dungeonBuildSuccessful = AttemptToBuildRandomDungeon(roomNodeGraph);

            }

            if(dungeonBuildSuccessful)
            {
                //ʵ������Ϸ����
                InstantiateRoomGameObjects();
            }
            
        }
        return dungeonBuildSuccessful;
    }

    private bool AttemptToBuildRandomDungeon(RoomNodeGraphSO roomNodeGraph)
    {
        //����һ���¶���
        Queue<RoomNodeSO> openRoomNodeQueue = new Queue<RoomNodeSO>();

        //���� isEntrance ����Ϊ true ��Ԫ��
        RoomNodeSO entranceNode = roomNodeGraph.GetRoomNode(roomNodeTypeList.list.Find(x => x.isEntrance));

        if(entranceNode != null)
        {
            //Ϊ�¶��з����һ��
            openRoomNodeQueue.Enqueue(entranceNode);
        }
        else
        {
            Debug.Log("No Entrance Node");
            return false;
        }

        bool noRoomOverlaps = true;

        noRoomOverlaps = ProcessRoomsInOpenRoomNodeQueue(roomNodeGraph , openRoomNodeQueue,noRoomOverlaps);
        
        if(openRoomNodeQueue.Count == 0 && noRoomOverlaps)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    private bool ProcessRoomsInOpenRoomNodeQueue(RoomNodeGraphSO roomNodeGraph,Queue<RoomNodeSO> openRoomNodeQueue, bool noRoomOverlaps)
    {
        while(openRoomNodeQueue.Count > 0 && noRoomOverlaps == true)
        {
            //ȡ����һ��Ԫ�ؽ��д���(��һ����entrance)
            RoomNodeSO roomNode = openRoomNodeQueue.Dequeue();

            foreach(RoomNodeSO childRoomNode in roomNodeGraph.GetChildRoomNodes(roomNode))
            {
                //�����Ԫ�ص��ӽڵ�
                openRoomNodeQueue.Enqueue(childRoomNode);
            }
            //������ĸ��ڵ�
            if(roomNode.roomNodeType.isEntrance)
            {
                RoomTemplateSO roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);

                Room room = CreateRoomFromRoomTemplate(roomTemplate,roomNode);

                room.isPositioned = true;

                dungeonBuilderRoomDictionary.Add(room.id, room);
            }
            else
            {
                Room parentRoom = dungeonBuilderRoomDictionary[roomNode.parentRoomNodeIDList[0]];

                noRoomOverlaps = CanPlaceRoomWithNoOverlaps(roomNode, parentRoom);
            }

        }
        return noRoomOverlaps;
    }

    private bool CanPlaceRoomWithNoOverlaps(RoomNodeSO roomNode, Room parentRoom)
    {
        bool roomOverlaps = true;

        while(roomOverlaps)
        {
            //��ø��ڵ㻹û�����ӵĴ�������
            List<Doorway> unconnectedAvailableParentDoorway = GetUnconnectedAvailableDoorways(parentRoom.doorWayList).ToList();

            if(unconnectedAvailableParentDoorway.Count == 0)
            {
                return false;
            }
            Doorway doorwayParent = unconnectedAvailableParentDoorway[UnityEngine.Random.Range(0, unconnectedAvailableParentDoorway.Count)];

            RoomTemplateSO roomTemplate = GetRandomTemplateForRoomConsistentWithParent(roomNode, doorwayParent);

            Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

            if(PlaceTheRoom(parentRoom, doorwayParent,room))
            {
                roomOverlaps = false;

                room.isPositioned = true;

                dungeonBuilderRoomDictionary.Add(room.id, room);

            }
            else
            {
                roomOverlaps = true;
            }
        }

        return true;

    }

    private Room CheckForRoomOverlap(Room roomToList)
    {
        foreach(KeyValuePair<string, Room> keyValuePair in dungeonBuilderRoomDictionary)
        {
            Room room = keyValuePair.Value;
            if (room.id == roomToList.id || !room.isPositioned)
                continue;
            if(IsOverLappingRoom(roomToList, room))
            {
                return room;
            }
        }
        return null;
    }

    private bool IsOverLappingRoom(Room room1, Room room2)
    {
        bool isOverLappingX = IsOverLappingInterval(room1.lowerBounds.x,room1.upperBounds.x,room2.lowerBounds.x,room2.upperBounds.x);
        bool isOverLappingY = IsOverLappingInterval(room1.lowerBounds.y, room1.upperBounds.y, room2.lowerBounds.y, room2.upperBounds.y);
        if (isOverLappingX && isOverLappingY)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool IsOverLappingInterval(int imin1, int imax1,int imin2,int imax2)
    {
        if (Mathf.Max(imin1, imin2) <= Mathf.Min(imax1, imax2))
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    private RoomTemplateSO GetRandomTemplateForRoomConsistentWithParent(RoomNodeSO roomNode,Doorway doorwayParent)
    {
        RoomTemplateSO roomTemplate = null;

        if (roomNode.roomNodeType.isCorridor)
        {
            switch (doorwayParent.orientation)
            {
                case Orientation.north:
                case Orientation.south:
                    roomTemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorNS));
                    break;

                case Orientation.east:
                case Orientation.west:
                    roomTemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorEW));
                    break;

                case Orientation.none:
                    break;

                default:
                    break;
            }
        }
        else
        {
            roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
        }
        return roomTemplate;
    }

    private void LoadRoomTemplatesIntoDictionary()
    {
        roomTemplateDictionary.Clear();

        foreach(RoomTemplateSO roomTemplate in roomTemplateList)
        {
            if(!roomTemplateDictionary.ContainsKey(roomTemplate.guid))
            {
                roomTemplateDictionary.Add(roomTemplate.guid, roomTemplate);
            }
            else
            {
                Debug.Log("Duplicate Room Template Key In" + roomTemplateList);
            }
        }
    }

    private bool PlaceTheRoom(Room parentRoom, Doorway doorwayParent, Room room)
    {
        Doorway doorway = GetOppositeDoorway(doorwayParent, room.doorWayList);

        if(doorway == null)
        {
            doorwayParent.isUnavailable = true;

            return false;
        }
        //�������������¸������ŵ�λ��(����λ�����ǲ���ģ�����޴���(parentRoom.lowerBounds-parentRoom.templatelowerBounds))
        Vector2Int parentDoorwayPosition = parentRoom.lowerBounds + doorwayParent.position - parentRoom.templateLowerBounds;

        Vector2Int adjustment = Vector2Int.zero;

        switch(doorway.orientation)
        {
            case Orientation.north:
                adjustment = new Vector2Int(0, -1);
                break;
            case Orientation.east:
                adjustment = new Vector2Int(-1, 0);
                break;
            case Orientation.south:
                adjustment = new Vector2Int(0, 1);
                break;
            case Orientation.west:
                adjustment = new Vector2Int(1, 0);
                break;

            case Orientation.none:
                break;

            default:
                break;

        }
        room.lowerBounds = parentDoorwayPosition + adjustment + room.templateLowerBounds - doorway.position;
        room.upperBounds = room.lowerBounds + room.templateUpperBounds - room.templateLowerBounds;

        Room overlappingRoom = CheckForRoomOverlap(room);

        if (overlappingRoom == null)
        {
            doorway.isUnavailable = true;
            doorway.isConnected = true;

            doorwayParent.isUnavailable = true;
            doorwayParent.isConnected = true;

            return true;
        }
        else
        {
            doorwayParent.isUnavailable=true;
            return false;
        }
    }

    private Doorway GetOppositeDoorway(Doorway parentDoorway,List<Doorway> doorwayList)
    {
        foreach(Doorway doorwayToCheck in doorwayList)
        {
            if(parentDoorway.orientation == Orientation.east && doorwayToCheck.orientation == Orientation.west)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.west && doorwayToCheck.orientation == Orientation.east)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.south && doorwayToCheck.orientation == Orientation.north)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.north && doorwayToCheck.orientation == Orientation.south)
            {
                return doorwayToCheck;
            }
        }
        return null;
    }

    private RoomTemplateSO GetRandomRoomTemplate(RoomNodeTypeSO roomNodeType)
    {
        List<RoomTemplateSO> matchingRoomTemplateList = new List<RoomTemplateSO>();

        foreach(RoomTemplateSO roomTemplate in roomTemplateList)
        {
            if(roomTemplate.roomNodeType == roomNodeType)
            {
                matchingRoomTemplateList.Add(roomTemplate);
            }
        }
        if(matchingRoomTemplateList.Count == 0)
        {
            return null;
        }
        return matchingRoomTemplateList[UnityEngine.Random.Range(0, matchingRoomTemplateList.Count)];
    }

    private IEnumerable<Doorway> GetUnconnectedAvailableDoorways(List<Doorway> roomDoorwayList)
    {
        foreach(Doorway doorway in roomDoorwayList)
        {
            if(!doorway.isConnected && !doorway.isUnavailable)
            {
                yield return doorway;
            }
        }
    }
    
    private Room CreateRoomFromRoomTemplate(RoomTemplateSO roomTemplate ,RoomNodeSO roomNode)
    {
        Room room = new Room();

        room.templateID = roomTemplate.guid;
        room.id = roomNode.id;
        room.prefab = roomTemplate.prefab;
        room.roomNodeType = roomTemplate.roomNodeType;
        room.lowerBounds = roomTemplate.lowerBounds;
        room.upperBounds = roomTemplate.upperBounds;
        room.spawnPositionArray = roomTemplate.spawnPositionArray;
        room.templateLowerBounds = roomTemplate.lowerBounds;
        room.templateUpperBounds = roomTemplate.upperBounds;

        //���
        room.childRoomIDList = CopyStringList(roomNode.childRoomNodeIDList);
        room.doorWayList = CopyDoorwayList(roomTemplate.doorwayList);

        if(roomNode.parentRoomNodeIDList.Count == 0)
        {
            room.parentRoomID = "";
            room.isPreviousVisited = true;

            GameManager.Instance.SetCurrentRoom(room);
        }
        else
        {
            room.parentRoomID = roomNode.parentRoomNodeIDList[0];
        }
        return room;
    }


    private RoomNodeGraphSO SelectRandomRoomNodeGraph(List<RoomNodeGraphSO> roomNodeGraphList)
    {
        if(roomNodeGraphList.Count > 0)
        {
            return roomNodeGraphList[UnityEngine.Random.Range(0, roomNodeGraphList.Count)];
        }
        else
        {
            Debug.Log("No Room Node Graph In List");
            return null ;
        }
    }

    private List<Doorway> CopyDoorwayList(List<Doorway> oldDoorWayList)
    {
        List<Doorway> newDoorwayList = new List<Doorway>();

        foreach(Doorway doorway in oldDoorWayList)
        {
            Doorway newDoorway = new Doorway();

            newDoorway.position = doorway.position;
            newDoorway.orientation = doorway.orientation;
            newDoorway.doorPrefab = doorway.doorPrefab;
            newDoorway.isConnected = doorway.isConnected;
            newDoorway.isUnavailable = doorway.isUnavailable;
            newDoorway.doorwayStartCopyPosition = doorway.doorwayStartCopyPosition;
            newDoorway.doorwayCopyTileHeight = doorway.doorwayCopyTileHeight;
            newDoorway.doorwayCopyTileWidth = doorway.doorwayCopyTileWidth;

            newDoorwayList.Add(newDoorway);
        }
        return newDoorwayList;
    }
    private List<string> CopyStringList(List<string> oldStringList)
    {
        List<string> newStringList = new List<string>();
        foreach (string stringValue in oldStringList)
        {
            newStringList.Add(stringValue);
        }
        return newStringList;
    }

    private void InstantiateRoomGameObjects()
    {
        foreach (KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
        {
            Room room = keyvaluepair.Value;

            Vector3 roomPosition = new Vector3(room.lowerBounds.x - room.templateLowerBounds.x, room.lowerBounds.y - room.templateLowerBounds.y, 0f);

            // Instantiate room
            GameObject roomGameobject = Instantiate(room.prefab, roomPosition, Quaternion.identity, transform);

            // Get instantiated room component from instantiated prefab.
            InstantiatedRoom instantiatedRoom = roomGameobject.GetComponentInChildren<InstantiatedRoom>();

            instantiatedRoom.room = room;

            // Initialise The Instantiated Room
            instantiatedRoom.Initialise(roomGameobject);

            // Save gameobject reference.
            room.instantiatedRoom = instantiatedRoom;

            if (!room.roomNodeType.isBossRoom)
            {
                room.isClearedOfEnemies = true;
            }
        }



    }
    //ͨ��ģ��ID��÷���ģ��
    public RoomTemplateSO GetRoomTemplate(string roomTemplateID)
    {
        if (roomTemplateDictionary.TryGetValue(roomTemplateID, out RoomTemplateSO roomTemplate))
        {
            return roomTemplate;
        }
        else
        {
            return null;
        }
    }
    //ͨ������id��÷���
    public Room GetRoomByID(string roomID)
    {
        if (dungeonBuilderRoomDictionary.TryGetValue(roomID, out Room room))
        {
            return room;
        }
        else
        {
            return null;
        }
    }
    private void ClearDungeon()
    {
        if(dungeonBuilderRoomDictionary.Count > 0)
        {
            foreach(KeyValuePair<string,Room> keyValuePair in dungeonBuilderRoomDictionary)
            {
                Room room = keyValuePair.Value;
                //����Ѿ�����ʵ��������,������Ѿ����ڳ����еĶ���
                if(room.instantiatedRoom != null)
                {
                    Destroy(room.instantiatedRoom.gameObject);
                }
            }

            dungeonBuilderRoomDictionary.Clear();
        }
    }

}
