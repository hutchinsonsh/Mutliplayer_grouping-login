using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using UnityEngine.SceneManagement;

public class ObjectHit : NetworkBehaviour
{
    public string RoomName;

    // this runs on every client when one client hits the 'door'
    // idea: Find client that actually hit door, have them change rooms/send message to server; all other clients do nothing
    private void OnTriggerEnter(Collider other)
    {
        // if 0 -> is server; don't want server calling server
        if (NetworkManager.Singleton.LocalClientId != 0)
        {
            // get uniqueID of the client running this; sees if it is the same client that hit the 'door'
            ulong temp = NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<PlayerNameStats>().getClientUniqueID();
            if (temp == other.GetComponent<PlayerNameStats>().getClientUniqueID())
            {
                StartCoroutine(changePlayerScene(other));
                GoToNewSceneServerRpc(NetworkManager.Singleton.LocalClientId);
            }
        }
    }

    // changes scene on client side; adds new scene/deletes old scene (as long as old scene isn't lobby)
    // TODO: delete old scenes that aren't lobby
    public IEnumerator changePlayerScene(Collider other)
    {
        //string oldRoom = NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<PlayerNameStats>().Room.Value;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(RoomName, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        SceneManager.MoveGameObjectToScene(other.gameObject, SceneManager.GetSceneByName(RoomName));
        other.transform.position = new Vector3(0, 0, 100);

        /*
        Debug.Log("old room: " + oldRoom);
        if (!oldRoom.Equals("LogInScene"))
        {
            SceneManager.UnloadSceneAsync(oldRoom);
        }
        */
    }

    // server call
    // adds new room if it doesn't already exist/transfer client to new room/updates clients stats
    [ServerRpc(RequireOwnership = false)]
    public void GoToNewSceneServerRpc(ulong hitUniqueID)
    {
        // get old room name and set new room name
        //string oldRoom = NetworkManager.Singleton.ConnectedClients[hitUniqueID].PlayerObject.GetComponent<PlayerNameStats>().Room.Value;
        NetworkManager.Singleton.ConnectedClients[hitUniqueID].PlayerObject.GetComponent<PlayerNameStats>().Room.Value = RoomName;


        /*
         * 
         * THIS SECTION MAY NOT BE NEEDED; why move client on server side? Then server has to have all rooms available; may only exist on client side
         * 
         * 
         * 
        // sees if scene already exists; if it doesn't --> adds scene; also moves player object to new room
        Scene sceneToLoad = SceneManager.GetSceneByName(RoomName);
        if (sceneToLoad.name == null)
        {
            SceneManager.LoadSceneAsync(RoomName, LoadSceneMode.Additive);
        }

        // moves new player to new scene
        foreach (PlayerData temp in NetworkManager.GetComponent<NetworkData>().playerDataList)
        {
            if (temp.GetPlayerUniqueID() == hitUniqueID)
            {
                temp.SetPlayerRoom(RoomName);
                SceneManager.MoveGameObjectToScene(NetworkManager.Singleton.ConnectedClients[hitUniqueID].PlayerObject.gameObject, SceneManager.GetSceneByName(RoomName));
                break;
            }
        }
        */

        /*
         * For removing old scene if no one no longer in it
        Debug.Log("old room: " + oldRoom);
        if(!oldRoom.Equals("LogInScene"))
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            bool someoneThere = false;
            foreach (GameObject player in players)
            {
                if (player.GetComponent<PlayerNameStats>().Room.Value.Equals(oldRoom))
                    someoneThere = true;
            }
            if(!someoneThere)
            {
                SceneManager.UnloadSceneAsync(oldRoom);
            }
        }
        */
        GoToNewSceneClientRpc();
    }


    [ClientRpc]
    /*
     * sees which clients are in which room; for each client -> renders other clients that are in the same room/disables clients not in the same room
     */
    public void GoToNewSceneClientRpc()
    {
        string currentRoom = NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<PlayerNameStats>().Room.Value;
        ulong currentID = NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<PlayerNameStats>().getClientUniqueID();
        Debug.Log("CurrentRoom: " + currentRoom + currentID);
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            // skips itself; only runs for other clients
            if (player.GetComponent<PlayerNameStats>().getClientUniqueID() != currentID)
            {
                Debug.Log("ID: " + player.GetComponent<PlayerNameStats>().getClientUniqueID());
                MeshRenderer[] Renderers = player.GetComponentsInChildren<MeshRenderer>();
                Collider[] Colliders = player.GetComponentsInChildren<Collider>();
                if (player.GetComponent<PlayerNameStats>().Room.Value.Equals(currentRoom))
                {
                    Debug.Log("In the same room");
                    foreach (MeshRenderer rend in Renderers)
                    {
                        rend.enabled = true;
                    }
                    foreach (Collider coll in Colliders)
                    {
                        coll.enabled = true;
                    }
                }
                else
                {
                    Debug.Log("In a different room");
                    foreach (MeshRenderer rend in Renderers)
                    {
                        rend.enabled = false;
                    }
                    foreach (Collider coll in Colliders)
                    {
                        coll.enabled = false;
                    }
                }
            }
        }
    }
}