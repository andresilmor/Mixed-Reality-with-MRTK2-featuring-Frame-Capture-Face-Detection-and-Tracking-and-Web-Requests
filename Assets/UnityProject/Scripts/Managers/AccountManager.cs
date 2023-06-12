using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Realms;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Linq;
using Realms.Sync;
using UnityEditor;
using System.Diagnostics.Contracts;
using Microsoft.MixedReality.Toolkit.UI;
using QRTracking;

using Debug = MRDebug;
using BestHTTP.WebSocket;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Diagnostics;
using Newtonsoft.Json;
using Microsoft.MixedReality.Toolkit.Examples.Demos.EyeTracking;

public static class AccountManager {
    public static string ActiveUserEmail { get; set; }
    public static string Token { get; set; }

    private static bool _isLogged = false;
    public static bool IsLogged {
        get { return _isLogged; }
        set {
            if (_isLogged == value) { return; }
            _isLogged = value;
            OnLoggedStatusChange?.Invoke(value);
            if (_isLogged) {
                return;

                // EXECUTE in LOGIN
                foreach (MemberOf memberOf in RealmManager.realm.Find<UserEntity>(ActiveUserEmail).MemberOf) {
                    NotificationsManager.SetupMedicationAlerts(memberOf.Institution.UUID);

                }

            }
            //OnLoggedStatusChange(IsLogged);
        }
    }



    public delegate void OnVariableChangeDelegate(bool newValue);
    public static event OnVariableChangeDelegate OnLoggedStatusChange;


    private static bool requesting = false;
    private static int _qrTrackingCounter = 0;
    private static QRInfo _lastQR = null;

    #region Login
    public static void OnSucessfullLogin(bool logged) {
        UIManager.Instance.HandMenu.ToggleHomeButton(logged);
        UIManager.Instance.HandMenu.ToggleLogoutButton(logged);

        UIManager.Instance.LoginMenu.gameObject.SetActive(false);

        UIManager.Instance.HomeMenu.gameObject.SetActive(true);

        Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * UIManager.Instance.WindowDistance;

        position.y += UIManager.Instance.AxisYOffset;

        UIManager.Instance.HomeMenu.gameObject.transform.position = position;
        UIManager.Instance.HomeMenu.gameObject.transform.LookAt(Camera.main.transform.position);

        UIManager.Instance.LoginMenu.ClearLoginPage();
    }

    public static bool LoginQR() {
     
     

        requesting = true;


        WebSocket authChannel = APIManager.CreateWebSocketConnection(APIManager.QRRoute + APIManager.QRAuth, (WebSocket ws, string response) => {
            JObject res = JObject.Parse(@response);

            Debug.Log(res); 
            Debug.Log(res["confirmation"].Type); 

            if ((bool)res["confirmation"] == true) {
                Debug.Log("HERE");
                IsLogged = true;
                Debug.Log("Saving User");
                SaveUser(res["authToken"].ToString());

            } else {
                Debug.Log("WTF");
                UIManager.Instance.LoginMenu.ResetButtons();
     

            }

            requesting = false;
            Debug.Log("Im gonna close the mothafucker");
            APIManager.GetWebSocket(APIManager.QRRoute + APIManager.QRAuth).Close();

        }, null, (WebSocket ws) => {

            QRCodeReaderManager.DetectQRCodes(DetectionMode.OneShot, (List<QRCodeDecodeReply.Detection> qrCodes) => {
                Debug.Log("ON OPEN CALLED");
                if (qrCodes != null && qrCodes.Count > 0) {
                    try {
                        UIManager.Instance.LoginMenu.QRCodeText.text = "Validating...";

                        foreach (QRCodeDecodeReply.Detection detection in qrCodes) {
                            if (!requesting)
                                break;
                            //Debug.Log("Detection: " + detection.content.ToString());
                            string channel = detection.content.ToString();
                            if (channel.Contains('.')) {
                                channel = channel.Split('.')[1];
                                //Debug.Log("----- Channel 1: " + channel);

                                if (channel[channel.Length - 1] != '=')
                                    channel += "=";

                                channel = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(channel));
                                //Debug.Log("----- Channel 2: " + channel);
                                //Debug.Log("----- Object2: " + JObject.Parse(channel).ToString());
                                channel = JObject.Parse(channel)["context"]["channel"].ToString();
                                //Debug.Log("----- Channel 3: " + channel);

                                ws.Send(JObject.Parse("{ \"channel\": \"" + channel + "\", \"confirmation\": " + false.ToString().ToLower() + " }").ToString());

                            }


                            //Debug.Log(detection.content.ToString());

                        }

                        return;

                    } catch (Exception ex) {
                        Debug.Log("A: " + ex.Message, LogType.Exception);
                        UIManager.Instance.LoginMenu.ResetButtons();

                        APIManager.GetWebSocket(APIManager.QRRoute + APIManager.QRAuth).Close();

                    }

                } else {
                    UIManager.Instance.LoginMenu.QRCodeText.text = "No QRCode Found";
                    requesting = false;
                    UIManager.Instance.LoginMenu.ResetButtons();
                    APIManager.GetWebSocket(APIManager.QRRoute + APIManager.QRAuth).Close();

                }
                //Debug.Log("----- NOP Founded xd");

            });

        });

        Debug.Log("Number of methods of : " + authChannel.OnOpen.GetInvocationList().Length);
        authChannel.Open();

        return true;

    }

    async private static void LoginQRCode(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> code) {
   
        QRInfo newQR = new QRInfo(code.Data);
        if (_lastQR != null && _lastQR.Id == newQR.Id)
            return;

        _lastQR = newQR;

        QRCodesPlugin.Instance.StopQRTracking();
        QRCodesPlugin.Instance.QRCodeAdded -= LoginQRCode;
        
        Debug.Log(newQR.Data.ToString());

        JObject qrMessage = JObject.Parse(@newQR.Data.ToString());

        LoginWithCredentials(qrMessage["email"].ToString(), qrMessage["password"].ToString());

    }

    async public static void LoginWithCredentials(string email, string password) {
        if (requesting)
            return;

        requesting = true;
        await ValidateLogin(email, password);


    }

    async private static Task ValidateLogin(string email, string password) {

        Debug.Log("ValidateLogin");
        GraphQL.Type queryOperation = new GraphQL.Type(
        "MemberLogin", new GraphQL.Type[] {new GraphQL.Type("loginCredentials", new GraphQL.Params[] {
            new GraphQL.Params("email", "\"" + email + "\""),
            new GraphQL.Params("password", "\"" + password + "\""),
        }) });


        await APIManager.ExecuteRequest("", queryOperation,
            (message, succeed) => {
                try {
                    if (succeed) {
                        JObject response = JObject.Parse(@message);
                        if (response.HasValues && response["data"] != null && response["data"]["MemberLogin"]["message"] == null) {
                            Debug.Log(response.ToString());
                            UIManager.Instance.LoginMenu.ValidatingLogin = false;
                            SaveUser(response);
                            AccountManager.IsLogged = true;
                            requesting = false;

                        } else {
                            Debug.Log("No Response");
                            UIManager.Instance.LoginMenu.ShowLoginErrorMessage("Invalid Credentials");
                            UIManager.Instance.LoginMenu.ValidatingLogin = false;
                            requesting = false;
                            UIManager.Instance.LoginMenu.ResetButtons();

                        }

                    }

                } catch (Exception e) {


                    Debug.Log("Error (ValidateLogin/ExecuteRequest): " + e.Message);
                    UIManager.Instance.LoginMenu.ValidatingLogin = false;
                    requesting = false;
                    UIManager.Instance.LoginMenu.ResetButtons();

                }

            },
            new GraphQL.Type[] {
                new GraphQL.Type("... on Member", new GraphQL.Type[] {
                    new GraphQL.Type("token"),
                    new GraphQL.Type("uuid"),
                    new GraphQL.Type("name"),
                    new GraphQL.Type("email"),
                    new GraphQL.Type("MemberOf", new GraphQL.Type[] {
                        new GraphQL.Type("role"),
                        new GraphQL.Type("institution", new GraphQL.Type[] {
                            new GraphQL.Type("uuid")
                        })
                    }),
                }),
                new GraphQL.Type("... on Error", new GraphQL.Type[] {
                    new GraphQL.Type("message"),
                
                }),
            });

        requesting = false;
        UIManager.Instance.LoginMenu.ValidatingLogin = false;
        UIManager.Instance.LoginMenu.ResetButtons();
        Debug.Log("Returning Value");

    }

    #endregion

    #region Logged Account Persistence

    private static void SaveUser(JObject response) {
        AccountManager.Token = response["data"]["MemberLogin"]["token"].Value<string>();
        if (RealmManager.CreateUpdateUser(response, response["data"]["MemberLogin"]["email"].Value<string>()))
            ActiveUserEmail = response["data"]["MemberLogin"]["email"].Value<string>();

        SetRelationshipInstitution(response);

    }

    private static void SaveUser(string token) {
        

        string tokenClaim = token.Split('.')[1];

        if (tokenClaim[tokenClaim.Length - 1] != '=')
            tokenClaim += "=";

        tokenClaim = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(tokenClaim));
        string userEmail = JObject.Parse(tokenClaim)["sub"].ToString();
        Debug.Log("----- Sub Test: " + userEmail.ToString());
        Debug.Log("----- Token Test: " + token.ToString());
        AccountManager.Token = token;

        RealmManager.CreateUpdateUser("", token, userEmail);

    
    }



    private static void SetRelationshipInstitution(JObject response) {
        UserEntity currentUser = RealmManager.realm.Find<UserEntity>(ActiveUserEmail);

        foreach (var relationship in response["data"]["MemberLogin"]["MemberOf"])
            RealmManager.CreateUpdateUserMembership(currentUser, relationship);


    }

    #endregion



    public static void Logout() {
        RealmManager.LogoutUser(AccountManager.ActiveUserEmail);
        IsLogged = false;


    }


}
