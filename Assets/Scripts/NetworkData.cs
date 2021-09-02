using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This script deals w/ storing user data on the server side
 * stores: name, password, GroupID, and type
 * 
 * 
 */

// class for storing user data
[System.Serializable]
public class PlayerData
{
    [SerializeField] private string PlayerName;
    [SerializeField] private string PlayerType;
    [SerializeField] private string PlayerPass;
    [SerializeField] private List<string> PlayerRooms = new List<string>(); 
    [SerializeField] private int PlayerGroupID;
    [SerializeField] private ulong PlayerUniqueID;

    public PlayerData(string name, string type, string password, string room, int groupID, ulong uniqueID)
    {
        PlayerName = name;
        PlayerType = type;
        PlayerPass = password;
        PlayerRooms.Add(room);
        PlayerGroupID = groupID;
        PlayerUniqueID = uniqueID;
    }

    public string GetPlayerName()
    {
        return PlayerName;
    }

    public string GetPlayerType()
    {
        return PlayerType;
    }

    public List<string> GetPlayerRoom()
    {
        return PlayerRooms;
    }

    public void SetPlayerRoom(string room)
    {
        PlayerRooms.Add(room);
    }

    public int GetPlayerGroupID()
    {
        return PlayerGroupID;
    }

    public ulong GetPlayerUniqueID()
    {
        return PlayerUniqueID;
    }

    public string GetPlayerPass()
    {
        return PlayerPass;
    }

}

// main method
public class NetworkData : MonoBehaviour
{
    // for keeping track of GroupID; increases w/ each teacher added
    // TODO: create a better method of grouping
    [SerializeField] public int idNum = 1;
    [SerializeField] public List<PlayerData> playerDataList = new List<PlayerData>();

    // creates new PlayerData object;
    public void createNewEntry(string name, string type, string password, ulong id)
    {
        // for getting group ID
        int groupID = -2;
        if (type.Equals("explorer"))
            groupID = 0;
        else if (type.Equals("teacher"))
        {
            groupID = idNum;
            idNum += 1;
        }
        else if (type.Equals("student"))
        {
            string tempPass = password + "****";
            foreach (PlayerData temp in playerDataList)
            {
                if (temp.GetPlayerType().Equals("teacher"))
                {
                    if (temp.GetPlayerPass().Equals(tempPass))
                        groupID = temp.GetPlayerGroupID();
                }
            }
            if (groupID == -2)
            {
                Debug.Log("Student doesn't have matching password");
            }
        }
        else
            Debug.Log("Type off; from network data script");

        playerDataList.Add(new PlayerData(name, type, password, "LogInScene", groupID, id));
    }
}