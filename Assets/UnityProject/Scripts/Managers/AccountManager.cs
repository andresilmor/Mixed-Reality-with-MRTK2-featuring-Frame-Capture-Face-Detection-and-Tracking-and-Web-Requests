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
using static APIManager;

public static class AccountManager {
    public static string currentUserUUID { get; private set; }

    private static bool _isLogged = false;
    public static bool IsLogged {
        get { return _isLogged; }
        private set {
            if (_isLogged == value) { return; }
            _isLogged = value;
            if (_isLogged) {
                foreach (MemberOf memberOf in RealmManager.realm.Find<UserEntity>(currentUserUUID).MemberOf) {
                    NotificationsManager.SetupMedicationAlerts(memberOf.Institution.UUID);

                }

            }
            //OnLoggedStatusChange(IsLogged);
        }
    }



    public delegate void OnVariableChangeDelegate(bool newValue);
    public static event OnVariableChangeDelegate OnLoggedStatusChange;

    public static UIWindow loginWindow;

    private static bool requesting = false;
    private static int _qrTrackingCounter = 0;
    private static QRInfo _lastQR = null;

    #region Login

    public static bool LoginQR() {
        /*
        if (loginWindow != null) {
            (loginWindow.components["BotButton"] as Interactable).enabled = false;
            loginWindow.UpdateContent("BotButtonText", "Looking for QR Code...");

        }

        /*
        Debug.Log("Starting");
        QRCodeReaderManager.DetectQRCodes((List<QRCodeReaderManager.QRCodeDetected> results) => {
            Debug.Log("Invoked");
        }, 1.5f, () => {
            Debug.Log("Time Over");
        });*/
        Debug.Log("Yo");


        QRCodesManager.Instance.ResetHandlers();
        QRCodesManager.Instance.QRCodeAdded += LoginQRCode;
        QRCodesManager.Instance.StartQRTracking();

        return true;
    }

    async private static void LoginQRCode(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> code) {
        Debug.Log("Hello There.");
        QRInfo newQR = new QRInfo(code.Data);

        Debug.Log(code.Data.Data.ToString());
        Debug.Log("Here.");

        if (_lastQR != null && _lastQR.Id == newQR.Id)
            return;

        _lastQR = newQR;

        //if (_qrTrackingCounter++ != 2)
        //    return;


        requesting = true;
        Debug.Log("Here. 0");
        QRCodesManager.Instance.StopQRTracking();
        QRCodesManager.Instance.QRCodeAdded -= LoginQRCode;
        
        Debug.Log("Here. 0.1");
        Debug.Log(newQR.Data.ToString());

        Debug.Log("Here. 1"); // Vai até aqui se ler json, se nao der Reiniciar os oculos
        JObject qrMessage = JObject.Parse(@newQR.Data.ToString());

        IsLogged = await ValidateLogin(qrMessage["username"].ToString(), qrMessage["password"].ToString());
        OnLoggedStatusChange?.Invoke(true);

        requesting = false;
        _qrTrackingCounter = 0;
        
    }

    async public static Task<bool> ValidateLogin(string username, string password) {
        APIManager.Field queryOperation = new APIManager.Field(
        "MemberLogin", new APIManager.Field[] {new APIManager.Field("loginCredentials", new APIManager.FieldParams[] {
            new APIManager.FieldParams("username", "\"" + username + "\""),
            new APIManager.FieldParams("password", "\"" + password + "\""),
        }) });


        await APIManager.ExecuteRequest("", queryOperation,
            (message, succeed) => {
                try {
                    if (succeed) {
                        JObject response = JObject.Parse(@message);
                        //Debug.Log(response.ToString());
                        if (response.HasValues && response["Data"] != null) {
                            IsLogged = SaveUser(response);
                            requesting = false;
                            UIManager.Instance.CloseWindow(AccountManager.loginWindow.stacker);



                        } else {
                            Debug.Log("Response empty");

                        }

                    }

                } catch (Exception e) {
                    Debug.Log("Error: " + e.Message);
                    requesting = false;

                }

            },
            new APIManager.Field[] {
                new APIManager.Field("token"),
                new APIManager.Field("uuid"),
                new APIManager.Field("Name"),
                new APIManager.Field("memberOf", new APIManager.Field[] {
                    new APIManager.Field("role"),
                    new APIManager.Field("institution", new APIManager.Field[] {
                    new APIManager.Field("uuid")
                })

            })

        });

        return false;

    }

    #endregion

    #region Logged Account Persistence

    private static bool SaveUser(JObject response) {
        if (RealmManager.CreateUpdateUser(response, response["Data"]["memberLogin"]["uuid"].Value<string>()))
            currentUserUUID = response["Data"]["memberLogin"]["uuid"].Value<string>();

        return SetRelationshipInstitution(response);

    }



    private static bool SetRelationshipInstitution(JObject response) {
        UserEntity currentUser = RealmManager.realm.Find<UserEntity>(currentUserUUID);

        foreach (var relationship in response["Data"]["memberLogin"]["memberOf"])
            RealmManager.CreateUpdateUserMembership(currentUser, relationship);

        return true;

    }

    #endregion



    public static bool Logout() {
        IsLogged = false;
        return true;

    }


}
