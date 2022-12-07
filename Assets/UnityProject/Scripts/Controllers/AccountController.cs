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

public static class AccountController {
    public static string currentUserUUID { get; private set; }

    private static bool _isLogged = false;
    public static bool isLogged {
        get { return _isLogged; }
        private set {
            if (_isLogged == value) { return; }
            _isLogged = value;
            if (_isLogged) {
                foreach (MemberOf memberOf in RealmController.realm.Find<UserEntity>(currentUserUUID).MemberOf) {
                    NotificationsController.SetupMedicationAlerts(memberOf.Institution.UUID);

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

        QRCodesManager.Instance.StartQRTracking();
        QRCodesManager.Instance.QRCodeAdded += LoginQRCode;

        return true;
    }

    async private static void LoginQRCode(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> args) {
        //improve
        if (requesting || QRCodesManager.Instance.lastSeen?.Data == args.Data) {
            Debug.LogWarning("Old QRCode.");
            return;
        }


        JObject qrMessage = JObject.Parse(@args.Data.Data.ToString());
        QRCodesManager.Instance.lastSeen = args;


        QRCodesManager.Instance.StopQRTracking();
        QRCodesManager.Instance.QRCodeAdded -= LoginQRCode;
        requesting = true;

        APIController.Field queryOperation = new APIController.Field(
        "memberLogin", new APIController.FieldParams[] {
            new APIController.FieldParams("username", "\"" + qrMessage["username"] + "\""),
            new APIController.FieldParams("password", "\"" + qrMessage["password"] + "\""),
        });

        await APIController.ExecuteRequest(null, queryOperation,
            (message, succeed) => {
                try {
                    if (succeed) {
                        JObject response = JObject.Parse(@message);
                        //Debug.Log(response.ToString());
                        if (response.HasValues && response["data"] != null) {
                            isLogged = SaveUser(response);
                            requesting = false;
                            UIController.Instance.CloseWindow(AccountController.loginWindow.stacker);

                        } else {
                            Debug.LogWarning("Response empty");

                        }

                    }

                } catch (Exception e) {
                    Debug.Log("Error: " + e.Message);
                    requesting = false;

                }

            },
            new APIController.Field[] {
                new APIController.Field("token"),
                new APIController.Field("uuid"),
                new APIController.Field("name"),
                new APIController.Field("memberOf", new APIController.Field[] {
                    new APIController.Field("role"),
                    new APIController.Field("institution", new APIController.Field[] {
                    new APIController.Field("uuid")
                })

            })

        });

    }

    #endregion

    #region Logged Account Persistence

    private static bool SaveUser(JObject response) {
        if (RealmController.CreateUpdateUser(response, response["data"]["memberLogin"]["uuid"].Value<string>()))
            currentUserUUID = response["data"]["memberLogin"]["uuid"].Value<string>();

        return SetRelationshipInstitution(response);

    }



    private static bool SetRelationshipInstitution(JObject response) {
        UserEntity currentUser = RealmController.realm.Find<UserEntity>(currentUserUUID);

        foreach (var relationship in response["data"]["memberLogin"]["memberOf"])
            RealmController.CreateUpdateUserMembership(currentUser, relationship);

        return true;

    }

    #endregion



    public static bool Logout() {
        isLogged = false;
        return true;

    }


}
