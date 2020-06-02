﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class RoomData {
    public Vector2 pos;
    public string needsEntranceAt;
    public RoomData(Vector2 pos, string needsEntranceAt) {
        this.pos = pos;
        this.needsEntranceAt = needsEntranceAt;
    }
}

public class DungeonAssembler : MonoBehaviour {
    [SerializeField] private List<GameObject> colliderList = new List<GameObject>();
    // list of colliders to attach to a room
    [SerializeField] private Sprite startSprite;
    // the sprite for starting out with
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
    [SerializeField] private List<RoomData> roomsToCreate = new List<RoomData>();
    // list of roomDatas which we take from
    private WaitForSeconds shortDelay = new WaitForSeconds(0f);
    // just to make it animated/cool

    private void Start() {
        CreateRoom(new Vector2(0, 0), "starter");
        CreateRoom(new Vector2(0, 1), "s");
        CreateRoom(new Vector2(1, 0), "w");
        CreateRoom(new Vector2(0, -1), "n");
        CreateRoom(new Vector2(-1, 0), "e");
        // create the starter rooms
        StartCoroutine(CreateAllRoomsCoro());
        // create all the rooms witht the coro
    }

    /// <summary>
    /// A coroutine to create all the rooms required, as well as fixing bad entrances.
    /// </summary>
    private IEnumerator CreateAllRoomsCoro() {
        while (createdRooms.Count < desiredRooms) {
            // while there are less rooms than wanted
            int rand = Random.Range(0, roomsToCreate.Count);
            // get a random number to pick with
            RoomData roomData = roomsToCreate[rand];
            // get the data from that location
            roomsToCreate.RemoveAt(rand);
            // and then remove it
            if (!(roomPositions.Contains(roomData.pos))) {
                // as long as the position isn't already taken
                yield return shortDelay;
                // quick delay (only use for visual effects)
                CreateRoom(roomData.pos, roomData.needsEntranceAt);
                // make a room based on the data we grabbed
            }
        }
        StartCoroutine(FixRoomsCoro());
        // after all the rooms are created, fix em up
    }

    /// <summary>
    /// Create a room at the designated location and add the necessary rooms to the roomsToCreate list.
    /// </summary>
    /// <param name="pos">The x position vector2 to create the room at.</param>
    /// <param name="roomNeedsEntranceAt">Which cardinal direction the room must connect to.</param>
    private void CreateRoom(Vector2 pos, string roomNeedsEntranceAt) {
        if (createdRooms.Count >= desiredRooms) { return; }
        // instantly break out if we have exceeded the max #
        if (roomPositions.Contains(pos)) { return; }
        roomPositions.Add(pos);
        // if the position is already created, break out, REDUNDANT BUT NEEDED FOR SOME REASON
        GameObject created = Instantiate(roomPrefab, new Vector3(pos.x * roomOffset, pos.y * roomOffset, 0), Quaternion.identity);
        createdRooms.Add(created);
        // create a gameobject from the prefab at the designated location, and add it to the list
        int rand = Random.Range(0, 9);
        // create a random number, 0-9
        GenerateSprite(created, roomNeedsEntranceAt, rand, desiredRooms);
        // based on the needed room, assign the sprite
        string spriteName = created.GetComponent<SpriteRenderer>().sprite.name;
        // get the name of the sprite, e.g. 'ew' for east and west entrances
        if (spriteName.Contains("n")) {
            // if the sprite has an entrance pointing north
            ParentColliderTo("north path collider", created);
            // assign the correct collider to that hallway
            roomsToCreate.Add(new RoomData(new Vector2(pos.x, pos.y + 1), "s"));
            // add a roomData object to the list, specifying it's position and required entrance
        }
        else { ParentColliderTo("north wall collider", created); }
        // no entrance pointing north, so use the correct collider
        // same process for all other directions
        if (spriteName.Contains("e")) {
            ParentColliderTo("east path collider", created);
            roomsToCreate.Add(new RoomData(new Vector2(pos.x + 1, pos.y), "w"));
        }
        else { ParentColliderTo("east wall collider", created); }
        if (spriteName.Contains("s")) {
            ParentColliderTo("south path collider", created);
            roomsToCreate.Add(new RoomData(new Vector2(pos.x, pos.y - 1), "n"));
        }
        else { ParentColliderTo("south wall collider", created); }
        if (spriteName.Contains("w")) {
            ParentColliderTo("west path collider", created);
            roomsToCreate.Add(new RoomData(new Vector2(pos.x - 1, pos.y), "e"));
        }
        else { ParentColliderTo("west wall collider", created); }
    }

    /// <summary>
    /// A coroutine to fix all the bad entrances of a room, called in CreateAllRoomsCoro.
    /// </summary>
    private IEnumerator FixRoomsCoro() {
        for (int i = 0; i < createdRooms.Count; i++) {
            // for every room created
            GameObject room = createdRooms[i];
            // get the room (faster than using foreach)
            string spriteName = room.GetComponent<SpriteRenderer>().sprite.name;
            string wantedEntrances = room.GetComponent<SpriteRenderer>().sprite.name;
            string unwantedEntrances = "";
            Vector2 northCheck = new Vector2(Mathf.RoundToInt(createdRooms[i].transform.position.x), Mathf.RoundToInt(createdRooms[i].transform.position.y + 1));
            Vector2 eastCheck = new Vector2(Mathf.RoundToInt(createdRooms[i].transform.position.x + 1), Mathf.RoundToInt(createdRooms[i].transform.position.y));
            Vector2 southCheck = new Vector2(Mathf.RoundToInt(createdRooms[i].transform.position.x), Mathf.RoundToInt(createdRooms[i].transform.position.y - 1));
            Vector2 westCheck = new Vector2(Mathf.RoundToInt(createdRooms[i].transform.position.x - 1), Mathf.RoundToInt(createdRooms[i].transform.position.y));
            // assign necessary starting variables
            if (spriteName.Contains("n") && !roomPositions.Contains(northCheck) || spriteName.Contains("n") && roomPositions.Contains(northCheck) && 
                !createdRooms[roomPositions.IndexOf(northCheck)].GetComponent<SpriteRenderer>().sprite.name.Contains("s")) {
                // if there is a north entrance
                // if there is no room at that position OR the room at that position doesn't have the right entrance
                unwantedEntrances += "n";
                // add it to the string of entrances we dont want
            }
            // repeat for all other cardinal directions
            if (spriteName.Contains("e") && !roomPositions.Contains(eastCheck) || spriteName.Contains("e") && roomPositions.Contains(eastCheck) && 
                !createdRooms[roomPositions.IndexOf(eastCheck)].GetComponent<SpriteRenderer>().sprite.name.Contains("w")) {
                unwantedEntrances += "e";
            }
            if (spriteName.Contains("s") && !roomPositions.Contains(southCheck) || spriteName.Contains("s") && roomPositions.Contains(southCheck) && 
                !createdRooms[roomPositions.IndexOf(southCheck)].GetComponent<SpriteRenderer>().sprite.name.Contains("n")) {
                unwantedEntrances += "s";
            }
            if (spriteName.Contains("w") && !roomPositions.Contains(westCheck) || spriteName.Contains("w") && roomPositions.Contains(westCheck) && 
                !createdRooms[roomPositions.IndexOf(westCheck)].GetComponent<SpriteRenderer>().sprite.name.Contains("e")) {
                unwantedEntrances += "w";
            }
            for (int k = 0; k < unwantedEntrances.Length; k++) {
                // for every character
                wantedEntrances = string.Join("", wantedEntrances.Split(unwantedEntrances[k]));
                // attempt to remove it, getting a string with only the entrances we want
            }
            room.GetComponent<SpriteRenderer>().sprite = allSprites[(from sprite in allSprites select sprite.name).ToList().IndexOf(wantedEntrances)];
            // locate the sprite by name
            foreach (Transform child in createdRooms[i].transform) {
                Destroy(child.gameObject);
                // destroy all children (colliders)
            }
            if (createdRooms[i].GetComponent<SpriteRenderer>().sprite.name.Contains("n")) {
                ParentColliderTo("north path collider", createdRooms[i]);
            }
            else { ParentColliderTo("north wall collider", createdRooms[i]); }
            if (createdRooms[i].GetComponent<SpriteRenderer>().sprite.name.Contains("e")) {
                ParentColliderTo("east path collider", createdRooms[i]);
            }
            else { ParentColliderTo("east wall collider", createdRooms[i]); }
            if (createdRooms[i].GetComponent<SpriteRenderer>().sprite.name.Contains("s")) {
                ParentColliderTo("south path collider", createdRooms[i]);
            }
            else { ParentColliderTo("south wall collider", createdRooms[i]); }
            if (createdRooms[i].GetComponent<SpriteRenderer>().sprite.name.Contains("w")) {
                ParentColliderTo("west path collider", createdRooms[i]);
            }
            // based on the name, add the necessary colliders
            yield return shortDelay;
            // quick delay (aesthetics)
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

    /// <summary>
    /// Assign a sprite fitting given requirements to the specified gameobject.
    /// </summary>
    /// <param name="created">The gameobject to assign the sprite to.</param>
    /// <param name="roomNeedsEntranceAt">Which entrance the sprite MUST have.</param>
    /// <param name="rand">The </param>
    /// <param name="desiredRooms"></param>
    private void GenerateSprite(GameObject created, string roomNeedsEntranceAt, int rand, int desiredRooms) {
        if (roomNeedsEntranceAt == "starter") {
            // if creating a starting room
            created.GetComponent<SpriteRenderer>().sprite = startSprite;
            created.GetComponent<SpriteRenderer>().color = Color.green;
            created.GetComponent<SpriteRenderer>().sortingOrder = 1;
            // set the sprite, make it green, set its sorting order forwards
        }
        else if (roomNeedsEntranceAt == "n") { created.GetComponent<SpriteRenderer>().sprite = northSprites[rand]; }
        else if (roomNeedsEntranceAt == "e") { created.GetComponent<SpriteRenderer>().sprite = eastSprites[rand]; }
        else if (roomNeedsEntranceAt == "s") { created.GetComponent<SpriteRenderer>().sprite = southSprites[rand]; }
        else if (roomNeedsEntranceAt == "w") { created.GetComponent<SpriteRenderer>().sprite = westSprites[rand]; }
        else { print("big error"); }
        // assign a sprite based on the requirement
        if (createdRooms.Count == Mathf.RoundToInt(desiredRooms / 3)) {
            created.GetComponent<SpriteRenderer>().color = Color.yellow;
        }
        else if (createdRooms.Count == Mathf.RoundToInt(2 * desiredRooms / 3)) {
            created.GetComponent<SpriteRenderer>().color = Color.yellow;
        }
        else if (createdRooms.Count == desiredRooms) {
            created.GetComponent<SpriteRenderer>().color = Color.red;
        }
        // assign a treasure room at 1/3, 2/3, and boss room as the final (in terms of room creation)
    }
}
