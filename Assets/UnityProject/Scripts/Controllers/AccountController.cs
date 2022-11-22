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
    public static RealmObject currentUser { get; private set; }

    private static bool _isLogged = false;
    public static bool isLogged {
        get { return _isLogged; }   
        private set { 
            if (_isLogged == value) { return; }
            _isLogged = value;
            if (OnLoggedStatusChange != null)
                OnLoggedStatusChange(isLogged);
        }
    }

    public delegate void OnVariableChangeDelegate(bool newValue);
    public static event OnVariableChangeDelegate OnLoggedStatusChange;


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
        try
        {

            requesting = true;
            APIController.Field queryOperation = new APIController.Field(
            "memberLogin", new APIController.FieldParams[] {
                new APIController.FieldParams("username", "\"" + qrMessage["username"] + "\""),
                new APIController.FieldParams("password", "\"" + qrMessage["password"] + "\""),
            });

            await APIController.ExecuteQuery("Read", queryOperation,
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

        } catch (Exception error) {
            Debug.Log("Error: " + error.Message);
        
        }

    }

    #endregion

    #region Logged Account Persistence

    private static bool SaveUser(JObject response)
    {
        currentUser = AppCommandCenter.realm.Find<UserEntity>(response["data"]["memberLogin"]["uuid"].ToString());
        if (currentUser == null) {
            AppCommandCenter.realm.Write(() =>
            {
                currentUser = new UserEntity(
                    UUID: response["data"]["memberLogin"]["uuid"].ToString(),
                    token: response["data"]["memberLogin"]["token"].ToString()
                );

                AppCommandCenter.realm.Add(currentUser);

            });

        } else {
            AppCommandCenter.realm.Write(() =>
            {
                (currentUser as UserEntity).Token = response["data"]["memberLogin"]["token"].ToString();

            });

        }

        return SetRelationshipInstitution(response);   

    }

    private static bool SetRelationshipInstitution(JObject response)
    {
        foreach (var relationship in response["data"]["memberLogin"]["memberOf"]) {
            InstitutionEntity institution = AppCommandCenter.realm.Find<InstitutionEntity>(relationship["institution"]["uuid"].ToString());
            if (institution == null) {
                AppCommandCenter.realm.Write(() =>  {
                    institution = new InstitutionEntity(relationship["institution"]["uuid"].ToString());
                    AppCommandCenter.realm.Add(institution);

                });

            }

            AppCommandCenter.realm.Write(() => {
                (currentUser as UserEntity).MemberOf.Add(new MemberOf(relationship["role"].ToString(), institution));

            });

            foreach (var contact in (currentUser as UserEntity).MemberOf) //Why I did this???
            {
                
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
