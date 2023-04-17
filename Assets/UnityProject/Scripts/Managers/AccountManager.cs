using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Realms;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.MixedReality.SampleQRCodes;
using System.Security.Cryptography;
using System.Linq;
using Realms.Sync;
using UnityEditor;
using System.Diagnostics.Contracts;
using Microsoft.MixedReality.Toolkit.UI;

using Debug = MRDebug;

public static class AccountManager {
    public static string currentUserUUID { get; private set; }

    private static bool _isLogged = false;
    public static bool isLogged {
        get { return _isLogged; }
        private set {
            if (_isLogged == value) { return; }
            _isLogged = value;
            if (_isLogged) {
                foreach (MemberOf memberOf in RealmManager.realm.Find<UserEntity>(currentUserUUID).MemberOf) {
                    NotificationsManager.SetupMedicationAlerts(memberOf.Institution.UUID);

                }

            }
            //OnLoggedStatusChange(isLogged);
        }
    }

    //public delegate void OnVariableChangeDelegate(bool newValue);
    //public static event OnVariableChangeDelegate OnLoggedStatusChange;

    public static UIWindow loginWindow;

    private static bool requesting = false;

    #region Login

    async public static Task<bool> LoginQR() {
        if (loginWindow != null) {
            (loginWindow.components["BotButton"] as Interactable).enabled = false;
            loginWindow.UpdateContent("BotButtonText", "Looking for QR Code...");

        }

        QRCodeReaderManager.DetectQRCodes(null, 1.5f);
        Debug.Log("yO");

        //QRCodesManager.Instance.StartQRTracking();
        //QRCodesManager.Instance.QRCodeAdded += LoginQRCode;

        return true;
    }

    async private static void LoginQRCode(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> args) {
        //improve
        Debug.Log("Here.");
        if (requesting || QRCodesManager.Instance.lastSeen.Data.Data.Equals(args.Data.Data)) {
            Debug.Log("Old QRCode.");
            return;
        }


        JObject qrMessage = JObject.Parse(@args.Data.Data.ToString());
        QRCodesManager.Instance.lastSeen = args;


        QRCodesManager.Instance.StopQRTracking();
        QRCodesManager.Instance.QRCodeAdded -= LoginQRCode;
        requesting = true;

        APIManager.Field queryOperation = new APIManager.Field(
        "memberLogin", new APIManager.FieldParams[] {
            new APIManager.FieldParams("username", "\"" + qrMessage["username"] + "\""),
            new APIManager.FieldParams("password", "\"" + qrMessage["password"] + "\""),
        });
        Debug.Log(qrMessage.ToString());
        return;

        await APIManager.ExecuteRequest("", queryOperation,
            (message, succeed) => {
                try {
                    if (succeed) {
                        JObject response = JObject.Parse(@message);
                        //Debug.Log(response.ToString());
                        if (response.HasValues && response["Data"] != null) {
                            isLogged = SaveUser(response);
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
        isLogged = false;
        return true;

    }


}
