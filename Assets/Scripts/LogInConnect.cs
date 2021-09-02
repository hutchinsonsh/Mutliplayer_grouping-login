using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using System;
using System.Text;

// for email verification
using System.Net.Mail;
using System.Net;


/*
 * This script deals w/ the login panel
 * It connects Teachers, students, and explorers if they meet their respective restrictions
 * Teachers: Join w/ username/pass -> always connect
 * Students: Join w/ username/pass -> connect if password is valid (ie- a previous teacher entered it)
 * Explorer: Join w/ username -> always connect
 * 
 * Info: on client side: stores name, password, type
 * Info: on server side: stores name, password, type, groupID
 * 
 */

public class LogInConnect : MonoBehaviour
{
    public GameObject loginPanel;

    // buttons for deciding whether teacher, student, or explorer
    public GameObject teacherButton;
    public GameObject studentButton;
    public GameObject explorerButton;

    // panels for respective buttons
    public GameObject getInfoPanel;

    // text input for password; not used for explorer
    public GameObject passwordInput;

    // textFields for respective class
    [SerializeField] private string clientName;
    [SerializeField] private string clientPassword;
    [SerializeField] private string clientType;

    [SerializeField] List<String> passwordTracker = new List<string>();

    public void Start()
    {
        getInfoPanel.SetActive(false);

        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
    }

    public void OnDestroy()
    {
        NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
    }


    //_________________________________________________________________________
    //          CONNECT/DISCONNECT SECTION: START
    //_________________________________________________________________________

    // Setup: no host, only server; this controsl ApprovalCheck
    private void HandleServerStarted()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Server works!");
        }
    }

    // when player connects, sets user data & turns off loginPanel
    private void HandleClientConnected(ulong clientID)
    {
        if (clientID == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("Connected!");
            loginPanel.SetActive(false);

            // sets username/type/groupID/password in PlayerNameStats
            NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<PlayerNameStats>().SetClientType(clientType);
            NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<PlayerNameStats>().SetClientName(clientName);
            NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<PlayerNameStats>().SetClientPassword(clientPassword);
            NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<PlayerNameStats>().SetClientUniqueID(NetworkManager.Singleton.LocalClientId);

            // sets data in struct on Server side
            NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<PlayerNameStats>().SetData();

            //NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<PlayerNameStats>().DontDestroy();
            // send verification email to user
            //NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<PlayerNameStats>().SendEmail();
        }
    }

    private void HandleClientDisconnect(ulong clientID)
    {
        if (clientID == NetworkManager.Singleton.LocalClientId)
        {
            //sendEmail(clientID);
            loginPanel.SetActive(true);
            getInfoPanel.SetActive(false);
        }
    }

    // on server side; checks if student password is valid/adds teacher password to list
    private void ApprovalCheck(byte[] ConnectionData, ulong clientId, MLAPI.NetworkManager.ConnectionApprovedDelegate callback)
    {
        string newPass = Encoding.ASCII.GetString(ConnectionData);
        bool approveConnection = false;

        if (newPass.Equals("explorer"))
        {
            approveConnection = true;
        }
        else
        {
            // if it's a teacher, add to password list
            if (newPass.Contains("****"))
            {
                string tempNewPass = newPass.Substring(0, newPass.Length - 4);
                passwordTracker.Add(tempNewPass);
                approveConnection = true;
            }
            // is student
            else
            {
                // loops through all existing passwords on server
                foreach (string temp in passwordTracker)
                {
                    if (temp.Equals(newPass))
                        approveConnection = true;
                }
            }
        }

        Debug.Log("connection: " + approveConnection);
        callback(true, null, approveConnection, null, null);
    }


    //_________________________________________________________________________
    //          CONNECT/DISCONNECT SECTION: END
    //_________________________________________________________________________


    //_________________________________________________________________________
    //          BUTTONS PRESSED SECTION: START
    //_________________________________________________________________________

    // starts client after type/name/password entered
    // happens when start button in login panel is pressed
    // TODO: default name/password, etc
    public void StartClient()
    {
        if(clientType.Equals("explorer"))
        {
            NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes("explorer");
        }
        else if(clientType.Equals("teacher"))
        {
            clientPassword += "****";
            NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(clientPassword);
        }
        else
        {
            NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(clientPassword);
        }
        NetworkManager.Singleton.StartClient();
    }

    // Starts server
    // TODO: Shouldn't have server button; 
    public void StartServer()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartServer();
    }

    // if user is teacher
    // happens when teacher button in login panel is pressed
    public void TeacherPressed()
    {
        clientType = "teacher";
        studentButton.SetActive(false);
        explorerButton.SetActive(false);
        getInfoPanel.SetActive(true);
    }

    // if user is student
    // happens when student button in login panel is pressed
    public void StudentPressed()
    {
        clientType = "student";
        teacherButton.SetActive(false);
        explorerButton.SetActive(false);
        getInfoPanel.SetActive(true);
    }

    // if user is explorer
    // happens when explorer button in login panel is pressed
    public void ExplorerPressed()
    {
        clientType = "explorer";
        studentButton.SetActive(false);
        teacherButton.SetActive(false);
        getInfoPanel.SetActive(true);
        passwordInput.SetActive(false);
    }

    // when text box for name is changed, function called
    public void setName(string nameEntered)
    {
        clientName = nameEntered;
    }

    // when text box for password is changed, function called
    public void setPassword(string passEntered)
    {
        clientPassword = passEntered;
    }

    //_________________________________________________________________________
    //          BUTTONS PRESSED SECTION: END
    //_________________________________________________________________________

    public void sendEmail(ulong clientID)
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
        SmtpServer.Credentials = (ICredentialsByHost)new NetworkCredential("s.hills8@icloud.com", "cmau-pszo-toix-odpg");
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
}