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

public static class AccountManager {
    public static string currentUserUUID { get; private set; }

    private static bool _isLogged = false;
    public static bool IsLogged {
        get { return _isLogged; }
        private set {
            if (_isLogged == value) { return; }
            _isLogged = value;
            if (_isLogged) {
                return;

                // EXECUTE in LOGIN
                foreach (MemberOf memberOf in RealmManager.realm.Find<UserEntity>(currentUserUUID).MemberOf) {
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

    public static bool LoginQR() {
     
     

        /*
        Debug.Log("Starting");
        QRCodeReaderManager.DetectQRCodes((List<QRCodeReaderManager.QRCodeDetected> results) => {
            Debug.Log("Invoked");
        }, 1.5f, () => {
            Debug.Log("Time Over");
        });*/
        Debug.Log("----- Route: " + (APIManager.QRRoute + APIManager.QRAuth));
        requesting = true;

        //QRCodesPlugin.Instance.ResetHandlers();
        //QRCodesPlugin.Instance.QRCodeAdded += LoginQRCode;
        //QRCodesPlugin.Instance.StartQRTracking();

        WebSocket authChannel = APIManager.CreateWebSocketConnection(APIManager.QRRoute + APIManager.QRAuth, (WebSocket ws, string response) => {
            Debug.Log("RESPONSE");
            Debug.Log(response);
            requesting = false;
            ws.Close();

        }, null, (WebSocket ws) => {

            QRCodeReaderManager.DetectQRCodes(DetectionMode.OneShot, (List<QRCodeDecodeReply.Detection> list) => {
         
                if (list != null && list.Count > 0) {
                    try { 
                        UIManager.Instance.LoginMenu.QRCodeText.text = "Validating...";

                        foreach (QRCodeDecodeReply.Detection detection in list) {
                            if (!requesting)
                                break;
                            Debug.Log("Detection: " + detection.content.ToString());
                            string channel = detection.content.ToString();
                            if (channel.Contains('.')) {
                                channel = channel.Split('.')[1];
                                Debug.Log("----- Channel 1: " + channel);

                                if (channel[channel.Length - 1] != '=')
                                    channel += "=";

                                channel = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(channel));
                                Debug.Log("----- Channel 2: " + channel);
                                Debug.Log("----- Object2: " + JObject.Parse(channel).ToString());
                                channel = JObject.Parse(channel)["context"]["channel"].ToString();
                                Debug.Log("----- Channel 3: " + channel);

                                ws.Send("{ \"channel\": " + channel + ", \"confirmation\": " + false + " }");

                            }


                            Debug.Log(detection.content.ToString());

                        }

                        return;

                    } catch (Exception ex) {
                        Debug.Log("A: " + ex.Message, LogType.Exception);

                        ws.Close();

                    }

                } else
                    UIManager.Instance.LoginMenu.QRCodeText.text = "No QRCode Found";

                Debug.Log("----- NOP Founded xd");

            });

        });

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

        LoginWithCredentials(qrMessage["username"].ToString(), qrMessage["password"].ToString());

    }

    async public static void LoginWithCredentials(string username, string password) {
        requesting = true;
        IsLogged = await ValidateLogin(username, password);

        requesting = false;

    }

    async private static Task<bool> ValidateLogin(string username, string password) {
        GraphQL.Type queryOperation = new GraphQL.Type(
        "MemberLogin", new GraphQL.Type[] {new GraphQL.Type("loginCredentials", new GraphQL.Params[] {
            new GraphQL.Params("username", "\"" + username + "\""),
            new GraphQL.Params("password", "\"" + password + "\""),
        }) });


        await APIManager.ExecuteRequest("", queryOperation,
            (message, succeed) => {
                try {
                    if (succeed) {
                        JObject response = JObject.Parse(@message);
                        if (response.HasValues && response["data"] != null && response["data"]["MemberLogin"]["message"] == null) {

                            IsLogged = true;
                            OnLoggedStatusChange?.Invoke(IsLogged);
                            SaveUser(response);
                            requesting = false;

                        } else {
                            IsLogged = false;

                            UIManager.Instance.LoginMenu.ShowLoginErrorMessage("Invalid Credentials");

                        }

                    }

                } catch (Exception e) {
                    requesting = false;
                }

            },
            new GraphQL.Type[] {
                new GraphQL.Type("... on Member", new GraphQL.Type[] {
                    new GraphQL.Type("token"),
                    new GraphQL.Type("uuid"),
                    new GraphQL.Type("name"),
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

        return IsLogged;

    }

    #endregion

    #region Logged Account Persistence

    private static void SaveUser(JObject response) {
        if (RealmManager.CreateUpdateUser(response, response["data"]["MemberLogin"]["uuid"].Value<string>()))
            currentUserUUID = response["data"]["MemberLogin"]["uuid"].Value<string>();

        SetRelationshipInstitution(response);

    }



    private static void SetRelationshipInstitution(JObject response) {
        UserEntity currentUser = RealmManager.realm.Find<UserEntity>(currentUserUUID);

        foreach (var relationship in response["data"]["MemberLogin"]["MemberOf"])
            RealmManager.CreateUpdateUserMembership(currentUser, relationship);


    }

    #endregion



    public static bool Logout() {
        IsLogged = false;
        OnLoggedStatusChange?.Invoke(IsLogged);
        return true;

    }


}
