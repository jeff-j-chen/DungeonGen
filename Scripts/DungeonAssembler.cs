﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class RoomData {
    public int x;
    public int y;
    public string needsEntranceAt;
    public string cameFrom;
    public RoomData(int x, int y, string needsEntranceAt, string cameFrom) {
        this.x = x;
        this.y = y;
        this.needsEntranceAt = needsEntranceAt;
        this.cameFrom = cameFrom;
    }
}

public class DungeonAssembler : MonoBehaviour {
    [SerializeField] private List<GameObject> colliderList = new List<GameObject>();
    // list of colliders to attach to a room
    [SerializeField] private Sprite startSprite;
    [SerializeField] private Sprite northEntranceClosing;
    [SerializeField] private Sprite eastEntranceClosing;
    [SerializeField] private Sprite southEntranceClosing;
    [SerializeField] private Sprite westEntranceClosing;
    [SerializeField] private List<Sprite> northSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> eastSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> southSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> westSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> allSprites = new List<Sprite>();
    // lists of sprites for rooms to use
    [SerializeField] private GameObject roomPrefab;
    // gameobject to instantiate
    [SerializeField] private float roomOffset;
    // how much to offset each room by
    [SerializeField] private int desiredRooms;
    // the # of rooms to create
    [SerializeField] private List<Vector2> roomPositions = new List<Vector2>();
    // list of positions of chunks
    [SerializeField] private List<GameObject> createdRooms = new List<GameObject>();
    // list of created chunks
    [SerializeField] private Stack<RoomData> northRoomStack = new Stack<RoomData>();
    [SerializeField] private Stack<RoomData> eastRoomStack = new Stack<RoomData>();
    [SerializeField] private Stack<RoomData> southRoomStack = new Stack<RoomData>();
    [SerializeField] private Stack<RoomData> westRoomStack = new Stack<RoomData>();
    [SerializeField] private int stackFlipChance;
    private Dictionary<string, string> oppositeOf = new Dictionary<string, string>() {
        {"n", "s"},
        {"e", "w"},
        {"s", "n"},
        {"w", "e"}
    };
    private WaitForSeconds quickDelay = new WaitForSeconds(0.01f);

    private void Start() {
        StartCoroutine(CreateRoomCoro(0, 0, "starter", "starter"));
        StartCoroutine(CreateRoomCoro(0, 1, "s", "n"));
        StartCoroutine(CreateRoomCoro(1, 0, "w", "e"));
        StartCoroutine(CreateRoomCoro(0, -1, "n", "s"));
        StartCoroutine(CreateRoomCoro(-1, 0, "e", "w"));
        StartCoroutine(CreateAllRoomsCoro());
        // StartCoroutine(FixRooms());
    }

    /// <summary>
    /// Create a room at the designated location, and use recursion to create all nearby rooms.
    /// </summary>
    /// <param name="xPos">The x position to create the room at.</param>
    /// <param name="yPos">The y position to create the room at.</param>
    /// <param name="roomNeedsEntranceAt">Which cardinal direction the room must connect to.</param>
    private IEnumerator CreateRoomCoro(int xPos, int yPos, string roomNeedsEntranceAt, string cameFrom) {
        if (createdRooms.Count >= desiredRooms) { yield break; }
        // instantly break out if we have exceeded the max #
        Vector2 curPos = new Vector2(xPos, yPos);
        if (roomPositions.Contains(curPos)) { yield break; }
        roomPositions.Add(curPos);
        // if the position is already created, break out
        GameObject created = Instantiate(roomPrefab, new Vector3(xPos * roomOffset, yPos * roomOffset, 0), Quaternion.identity);
        createdRooms.Add(created);
        // create a gameobject from the prefab at the designated location.
        int rand = Random.Range(0, 9);
        // create a random number, 0-6
        // based on the needed room, assign the sprite
        GenerateSprite(created, roomNeedsEntranceAt, rand, desiredRooms);
        string spriteName = created.GetComponent<SpriteRenderer>().sprite.name;
        // get the name of the sprite, e.g. 'ew' for east and west entrances
        if (spriteName.Contains("n")) {
            ParentColliderTo("north path collider", created);
            if (cameFrom == "starter") { northRoomStack.Push(new RoomData(xPos, yPos + 1, "s", "n")); }
            else { 
                if (cameFrom == "n") { northRoomStack.Push(new RoomData(xPos, yPos + 1, "s", cameFrom)); }
                else if (cameFrom == "e") { eastRoomStack.Push(new RoomData(xPos, yPos + 1, "s", cameFrom)); }
                else if (cameFrom == "s") { southRoomStack.Push(new RoomData(xPos, yPos + 1, "s", cameFrom)); }
                else if (cameFrom == "w") { westRoomStack.Push(new RoomData(xPos, yPos + 1, "s", cameFrom)); }
            }
        }
        else { ParentColliderTo("north wall collider", created); }
        if (spriteName.Contains("e")) {
            ParentColliderTo("east path collider", created);
            if (cameFrom == "starter") { eastRoomStack.Push(new RoomData(xPos + 1, yPos, "w", "e")); }
            else { 
                if (cameFrom == "n") { northRoomStack.Push(new RoomData(xPos + 1, yPos, "w", cameFrom)); }
                else if (cameFrom == "e") { eastRoomStack.Push(new RoomData(xPos + 1, yPos, "w", cameFrom)); }
                else if (cameFrom == "s") { southRoomStack.Push(new RoomData(xPos + 1, yPos, "w", cameFrom)); }
                else if (cameFrom == "w") { westRoomStack.Push(new RoomData(xPos + 1, yPos, "w", cameFrom)); }
            }
        }
        else { ParentColliderTo("east wall collider", created); }
        if (spriteName.Contains("s")) {
            ParentColliderTo("south path collider", created);
            if (cameFrom == "starter") { southRoomStack.Push(new RoomData(xPos, yPos - 1, "n", "s")); }
            else { 
                if (cameFrom == "n") { northRoomStack.Push(new RoomData(xPos, yPos - 1, "n", cameFrom)); }
                else if (cameFrom == "e") { eastRoomStack.Push(new RoomData(xPos, yPos - 1, "n", cameFrom)); }
                else if (cameFrom == "s") { southRoomStack.Push(new RoomData(xPos, yPos - 1, "n", cameFrom)); }
                else if (cameFrom == "w") { westRoomStack.Push(new RoomData(xPos, yPos - 1, "n", cameFrom)); }
            }
        }
        else { ParentColliderTo("south wall collider", created); }
        if (spriteName.Contains("w")) {
            ParentColliderTo("west path collider", created);
            if (cameFrom == "starter") { westRoomStack.Push(new RoomData(xPos - 1, yPos, "e", "w")); }
            else { 
                if (cameFrom == "n") { northRoomStack.Push(new RoomData(xPos - 1, yPos, "e", cameFrom)); }
                else if (cameFrom == "e") { eastRoomStack.Push(new RoomData(xPos - 1, yPos, "e", cameFrom)); }
                else if (cameFrom == "s") { southRoomStack.Push(new RoomData(xPos - 1, yPos, "e", cameFrom)); }
                else if (cameFrom == "w") { westRoomStack.Push(new RoomData(xPos - 1, yPos, "e", cameFrom)); }
            }
        }
        else { ParentColliderTo("west wall collider", created); }
        // based on the sprite name, create the needed rooms to connect to it
    }

    private IEnumerator CreateAllRoomsCoro() {
        while (createdRooms.Count < desiredRooms) {
            yield return quickDelay;
            RoomData roomData;
            if (Random.Range(0, 100) <= stackFlipChance) {
                northRoomStack.Reverse();
                eastRoomStack.Reverse();
                southRoomStack.Reverse();
                westRoomStack.Reverse();
            }
            if (northRoomStack.Count > 0) {
                roomData = northRoomStack.Pop();
                StartCoroutine(CreateRoomCoro(roomData.x, roomData.y, roomData.needsEntranceAt, roomData.cameFrom));
            }
            if (eastRoomStack.Count > 0) {
                roomData = eastRoomStack.Pop();
                StartCoroutine(CreateRoomCoro(roomData.x, roomData.y, roomData.needsEntranceAt, roomData.cameFrom));
            }
            if (southRoomStack.Count > 0) {
                roomData = southRoomStack.Pop();
                StartCoroutine(CreateRoomCoro(roomData.x, roomData.y, roomData.needsEntranceAt, roomData.cameFrom));
            }
            if (westRoomStack.Count > 0) {
                roomData = westRoomStack.Pop();
                StartCoroutine(CreateRoomCoro(roomData.x, roomData.y, roomData.needsEntranceAt, roomData.cameFrom));
            }
        }
        yield return quickDelay;
        print("now fixing rooms!");
        StartCoroutine(FixRooms());
        yield return quickDelay;
        print("round two");
        StartCoroutine(FixRooms());
    }

    private IEnumerator FixRooms() {
        for (int i = 0; i < createdRooms.Count; i++) {
            yield return quickDelay;
            string unwantedEntrances = "";
            GameObject curRoom = createdRooms[i];
            Vector2 checkPosition;
            string spriteName = curRoom.GetComponent<SpriteRenderer>().sprite.name;
            if (curRoom.GetComponent<SpriteRenderer>().sprite.name.Contains("n")) {
                checkPosition = new Vector2(curRoom.transform.position.x, curRoom.transform.position.y + 1);
                if (roomPositions.Contains(checkPosition)) {
                    if(!createdRooms[roomPositions.IndexOf(checkPosition)].GetComponent<SpriteRenderer>().sprite.name.Contains("s")) {
                        // no south entrance to match the north one
                        unwantedEntrances += "n";
                    }
                }
                else {
                    unwantedEntrances += "n";
                }
            }
            if (curRoom.GetComponent<SpriteRenderer>().sprite.name.Contains("e")) {
                checkPosition = new Vector2(curRoom.transform.position.x + 1, curRoom.transform.position.y);
                if (roomPositions.Contains(checkPosition)) {
                    if(!createdRooms[roomPositions.IndexOf(checkPosition)].GetComponent<SpriteRenderer>().sprite.name.Contains("w")) {
                        unwantedEntrances += "e";
                    }
                }
                else {
                    unwantedEntrances += "e";
                }
            }
            if (curRoom.GetComponent<SpriteRenderer>().sprite.name.Contains("s")) {
                checkPosition = new Vector2(curRoom.transform.position.x, curRoom.transform.position.y - 1);
                if (roomPositions.Contains(checkPosition)) {
                    if(!createdRooms[roomPositions.IndexOf(checkPosition)].GetComponent<SpriteRenderer>().sprite.name.Contains("n")) {
                        unwantedEntrances += "s";
                    }
                }
                else {
                    unwantedEntrances += "s";
                }
            }
            if (curRoom.GetComponent<SpriteRenderer>().sprite.name.Contains("w")) {
                checkPosition = new Vector2(curRoom.transform.position.x - 1, curRoom.transform.position.y);
                if (roomPositions.Contains(checkPosition)) {
                    if(!createdRooms[roomPositions.IndexOf(checkPosition)].GetComponent<SpriteRenderer>().sprite.name.Contains("e")) {
                        unwantedEntrances += "w";
                    }
                }
                else {
                    unwantedEntrances += "w";
                }
            }
            string wantedEntrances = curRoom.GetComponent<SpriteRenderer>().sprite.name;
            for (int k = 0; k < unwantedEntrances.Length; k++) {
               wantedEntrances = string.Join("", wantedEntrances.Split(unwantedEntrances[k]));
            }
            if (wantedEntrances == "") {
                // Debug.LogError($"big error at {curRoom.transform.position.x}, {curRoom.transform.position.y}, whose unwanted entrances were {unwantedEntrances} from spriteName {curRoom.GetComponent<SpriteRenderer>().sprite.name}");
            }
            else {
                curRoom.GetComponent<SpriteRenderer>().sprite = allSprites[(from sprite in allSprites select sprite.name).ToList().IndexOf(wantedEntrances)];
            }
            foreach (Transform child in curRoom.transform) {
                Destroy(child.gameObject);
            }
            if (curRoom.GetComponent<SpriteRenderer>().sprite.name.Contains("n")) {
                ParentColliderTo("north path collider", curRoom);
            }
            else { ParentColliderTo("north wall collider", curRoom); }
            if (curRoom.GetComponent<SpriteRenderer>().sprite.name.Contains("e")) {
                ParentColliderTo("east path collider", curRoom);
            }
            else { ParentColliderTo("east wall collider", curRoom); }
            if (curRoom.GetComponent<SpriteRenderer>().sprite.name.Contains("s")) {
                ParentColliderTo("south path collider", curRoom);
            }
            else { ParentColliderTo("south wall collider", curRoom); }
            if (curRoom.GetComponent<SpriteRenderer>().sprite.name.Contains("w")) {
                ParentColliderTo("west path collider", curRoom);
            }
        }
    }

    /// <summary>
    /// Child a collider with a given name to the given gameobject.
    /// </summary>
    /// <param name="colliderName">The name of the collider to match to.</param>
    /// <param name="parentTo">The gameobject to chil the collider to.</param>
    private void ParentColliderTo(string colliderName, GameObject parentTo) {
        GameObject collider = Instantiate(colliderList[(from col in colliderList select col.name).ToList().IndexOf(colliderName)], new Vector3(0, 0, 0), Quaternion.identity);
        // get the collider based on its name
        collider.transform.parent = parentTo.transform;
        // child the collider to the gameobject
        collider.transform.localPosition = new Vector2(0, 0);
        // set the collider's localposition to be normal
    }

    private void GenerateSprite(GameObject created, string roomNeedsEntranceAt, int rand, int desiredRooms) {
        if (roomNeedsEntranceAt == "starter") {
            // if creating a starting room
            created.GetComponent<SpriteRenderer>().sprite = startSprite;
            created.GetComponent<SpriteRenderer>().color = Color.green;
            created.GetComponent<SpriteRenderer>().sortingOrder = 1;
            // set the sprite, make it green, make it go forwards
        }
        else if (roomNeedsEntranceAt == "n") { created.GetComponent<SpriteRenderer>().sprite = northSprites[rand]; }
        else if (roomNeedsEntranceAt == "e") { created.GetComponent<SpriteRenderer>().sprite = eastSprites[rand]; }
        else if (roomNeedsEntranceAt == "s") { created.GetComponent<SpriteRenderer>().sprite = southSprites[rand]; }
        else if (roomNeedsEntranceAt == "w") { created.GetComponent<SpriteRenderer>().sprite = westSprites[rand]; }
        else { print("big error"); }
    }
}
