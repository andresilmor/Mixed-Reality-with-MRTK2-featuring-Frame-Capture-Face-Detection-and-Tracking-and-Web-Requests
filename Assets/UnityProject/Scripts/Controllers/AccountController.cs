using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Realms;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.MixedReality.SampleQRCodes;

public static class AccountController 
{
    public static RealmObject currentUser { get; private set; }

    public static bool isLogged { get; private set; }


    async public static Task<bool> Login()
    {
        AppCommandCenter.qrCodesManager.StartQRTracking();
        AppCommandCenter.qrCodesManager.QRCodeAdded += LoginQRCode;

        return true;
    }

    async private static void LoginQRCode(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> args)
    {
        JObject response = JObject.Parse(@args.Data.Data.ToString());
        AppCommandCenter.qrCodesManager.StopQRTracking();

        try
        {

            APIController.Field queryOperation = new APIController.Field(
            "memberLogin", new APIController.FieldParams[] {
                new APIController.FieldParams("username", "\"" + response["username"]  + "\""),
                new APIController.FieldParams("password", "\"" + response["password"] + "\""),
            });

            await APIController.ExecuteQuery("Read", queryOperation,
                (message) => {
                    try
                    {
                        JObject response = JObject.Parse(@message);
                        if (response["data"] != null) { 
                            AppCommandCenter.realm.Write(() => {
                                AppCommandCenter.realm.Add(new UserEntity(response["data"]["uuid"].ToString(), response["data"]["token"].ToString()));
                                
                        });

                        }

                        AppCommandCenter.qrCodesManager.QRCodeAdded -= LoginQRCode;
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message);
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
        catch (Exception error)
        {
            Debugger.AddText(error.Message);
         

        }

    }

    public static bool Logout()
    {
        return true;
    }


}
