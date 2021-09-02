using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;

/*
 * This script stores client data on client side
 * Calls RPC to store same data on server side
 */

public class PlayerNameStats : NetworkBehaviour
{
    // so that all clients can view all other clients current wereabouts (used for multi-scene)
    [SerializeField] private NetworkVariableString c_Room = new NetworkVariableString("Room_Name");
    public NetworkVariableString Room => c_Room;

    // so that all clients can keep track of all other player's unique ID (used for multi-scene)
    [SerializeField] private NetworkVariableULong c_ID = new NetworkVariableULong(0);
    public NetworkVariableULong UID => c_ID;

    //for testing purposes
    public string clientName;
    public string clientType;
    private string clientPassword;
    public ulong clientUniqueID;

    // sets usernames
    // used in LogInConnect when client connects
    public void SetClientName(string newName)
    {
        clientName = newName;
    }

    // set whichType
    // teacher, student, or explorer
    // used in LogInConnect when client connects
    public void SetClientType(string newType)
    {
        clientType = newType;
    }

    public void SetClientPassword(string pass)
    {
        clientPassword = pass;
    }

    public void SetClientUniqueID(ulong randNum)
    {
        clientUniqueID = randNum;
    }

    // for changing between scenes
    public void DontDestroy()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    public string getClientName()
    {
        return clientName;
    }

    public ulong getClientUniqueID()
    {
        return clientUniqueID;
    }

    // send data to the server to store in structs
    public void SetData()
    {
        SetDataServerRpc(clientName, clientType, clientPassword, clientUniqueID);
    }

    // server call
    [ServerRpc]
    public void SetDataServerRpc(string name, string type, string password, ulong id)
    {
        NetworkManager.Singleton.ConnectedClients[id].PlayerObject.GetComponent<PlayerNameStats>().UID.Value = id;
        NetworkManager.Singleton.ConnectedClients[id].PlayerObject.GetComponent<PlayerNameStats>().Room.Value = "LogInScene";
        NetworkManager.GetComponent<NetworkData>().createNewEntry(name, type, password, id);
    }


    /*
    void OnEnable()
    {
        Room.OnValueChanged += OnRoomChanged;
    }

    void OnDisable()
    {
        Room.OnValueChanged -= OnRoomChanged;
    }

    void OnRoomChanged(string oldValue, string newValue)
    {
        // Update UI, if this a client instance and it's the owner of the object
        if (IsOwner && IsClient)
        {
            Debug.Log("Room name changed");
        }
    }
    */

    /*
    public void SendEmail()
    {
        Debug.Log("sending email");
        MailMessage mail = new MailMessage();
        SmtpClient SmtpServer = new SmtpClient("smtp.mail.me.com");
        mail.From = new MailAddress("s.hills8@icloud.com");
        mail.To.Add(clientName);
        mail.Subject = "Unity Email Verification";
        mail.Body = "This is a test for email verification on unity";

        SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
        SmtpServer.Port = 587;
        //SmtpServer.Credentials = (ICredentialsByHost)CredentialCache.DefaultNetworkCredentials;
        SmtpServer.Credentials = (ICredentialsByHost) new NetworkCredential("s.hills8@icloud.com", "cmau-pszo-toix-odpg");
        SmtpServer.EnableSsl = true;
        SmtpServer.Timeout = 20000;
        try
        {
            SmtpServer.Send(mail);
            Debug.Log("email sent");
        }
        catch (SmtpException myEx)
        {
            Debug.Log("Expection: \n" + myEx.ToString());
        }


    }
    */
}
