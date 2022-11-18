using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Realms;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

public static class AccountController 
{
    public static RealmObject currentUser { get; private set; }

    async public static Task<bool> Login()
    {
        try
        {
            string username = "test";
            string password = "x";


            APIController.Field queryOperation = new APIController.Field(
            "memberLogin", new APIController.FieldParams[] {
                new APIController.FieldParams("username", "\"" + username + "\""),
                new APIController.FieldParams("password", "\"" + password + "\""),
            });

            await APIController.ExecuteQuery("Read", queryOperation,
                (message) => {
                    try
                    {
                        dynamic response = JObject.Parse(@message);
                        Debug.Log(JObject.Parse(@message)["data"]["memberLogin"]);

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
        } catch (Exception error)
        {
            Debugger.AddText(error.Message);    
            return false;

        }

        return true;
    }

    public static bool Logout()
    {
        return true;
    }


}
