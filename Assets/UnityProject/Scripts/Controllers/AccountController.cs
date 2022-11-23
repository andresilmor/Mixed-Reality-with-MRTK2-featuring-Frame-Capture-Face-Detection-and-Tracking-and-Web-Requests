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

public static class AccountController 
{
    public static string currentUserUUID { get; private set; }

    private static bool _isLogged = false;
    public static bool isLogged {
        get { return _isLogged; }   
        private set { 
            if (_isLogged == value) { return; }
            _isLogged = value;
            if (_isLogged) {
               foreach (MemberOf memberOf in AppCommandCenter.realm.Find<UserEntity>(currentUserUUID).MemberOf)
               {
                    NotificationsController.SetupMedicationAlerts(memberOf.Institution.UUID);

               }

            }
            //OnLoggedStatusChange(isLogged);
        }
    }

    //public delegate void OnVariableChangeDelegate(bool newValue);
    //public static event OnVariableChangeDelegate OnLoggedStatusChange;

    private static bool requesting = false;

    #region Login

    async public static Task<bool> Login()
    {
       
        AppCommandCenter.qrCodesManager.StartQRTracking();
        AppCommandCenter.qrCodesManager.QRCodeAdded += LoginQRCode;

        return true;
    }

    async private static void LoginQRCode(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> args)
    {
        if (requesting || AppCommandCenter.qrCodesManager.lastSeen?.Data == args.Data)
            return;

        AppCommandCenter.qrCodesManager.lastSeen = args;

        JObject qrMessage = JObject.Parse(@args.Data.Data.ToString());
        AppCommandCenter.qrCodesManager.StopQRTracking();
        AppCommandCenter.qrCodesManager.QRCodeAdded -= LoginQRCode;
        
        requesting = true;
        APIController.Field queryOperation = new APIController.Field(
        "memberLogin", new APIController.FieldParams[] {
            new APIController.FieldParams("username", "\"" + qrMessage["username"] + "\""),
            new APIController.FieldParams("password", "\"" + qrMessage["password"] + "\""),
        });

        await APIController.ExecuteQuery("Read", null, queryOperation,
            (message) => {
                try
                {
                    JObject response = JObject.Parse(@message);
                    if (response["data"] != null) {
                        isLogged = SaveUser(response);
                        requesting = false;

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

    private static bool SaveUser(JObject response)
    {
        UserEntity currentUser = AppCommandCenter.realm.Find<UserEntity>(response["data"]["memberLogin"]["uuid"].ToString());

        using (Realm realm = AppCommandCenter.realm) {
            using (Transaction transiction = realm.BeginWrite()) { 
                try { 
                    if (currentUser == null) {
                        currentUser = new UserEntity(
                                UUID: response["data"]["memberLogin"]["uuid"].ToString(),
                                token: response["data"]["memberLogin"]["token"].ToString()
                        );
                        realm.Add(currentUser);

                    } else {
                        currentUser.Token = response["data"]["memberLogin"]["token"].ToString();
                        realm.Add(currentUser, update: true);

                    }
                    transiction.Commit();
                    currentUserUUID = currentUser.UUID;

                } catch (Exception ex) {
                    transiction.Dispose();

                }
            }

        }

        return SetRelationshipInstitution(response);   

    }

    private static bool SetRelationshipInstitution(JObject response)
    {
        UserEntity currentUser = AppCommandCenter.realm.Find<UserEntity>(currentUserUUID);

        foreach (var relationship in response["data"]["memberLogin"]["memberOf"]) {
            InstitutionEntity institution = AppCommandCenter.realm.Find<InstitutionEntity>(relationship["institution"]["uuid"].ToString());

            using (var realm = AppCommandCenter.realm) {
                var transiction = realm.BeginWrite();
                Debug.Assert(institution == null);
                try { 
                    if (institution == null) {
                        institution = new InstitutionEntity(relationship["institution"]["uuid"].ToString());
                        AppCommandCenter.realm.Add(institution);

                    }

                    currentUser.MemberOf.Add(new MemberOf(relationship["role"].ToString(), institution));
                    AppCommandCenter.realm.Add(currentUser, update: true);
                    transiction.Commit();

                } catch (Exception ex) {
                    Debug.Log("Error: " + ex.Message);
                    transiction.Dispose();

                }

            }

        }

        return true;
    
    }

    #endregion



    public static bool Logout()
    {
        isLogged = false;
        return true;

    }


}
