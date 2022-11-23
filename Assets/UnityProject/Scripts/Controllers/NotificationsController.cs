using BestHTTP.Logger;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NotificationsController 
{   

    async public static void SetupMedicationAlerts(string institutionUUID)
    {
        Debug.Log("User: " + AccountController.currentUserUUID);
        Debug.Log("Institution: " + institutionUUID);
        Debug.Log("Token: " + AppCommandCenter.realm.Find<UserEntity>(AccountController.currentUserUUID).Token);

        APIController.Field queryOperation = new APIController.Field(
                    "medicationToTake", new APIController.FieldParams[] {
                        new APIController.FieldParams("memberID", "\"" + AccountController.currentUserUUID + "\""),
                        new APIController.FieldParams("institutionID", "\"" + institutionUUID + "\""),
                    });

        await APIController.ExecuteQuery("Read", AppCommandCenter.realm.Find<UserEntity>(AccountController.currentUserUUID).Token.ToString().Trim(), queryOperation,
            (message) => {
                try {
                    JObject response = JObject.Parse(@message);
                    Debug.Log(response.ToString());
                } catch (Exception e) {
                    Debug.Log("Error: " + e.Message);

                }

            },
            new APIController.Field[] {
                new APIController.Field("atTime"),
                new APIController.Field("quantity"),
                new APIController.Field("medication", new APIController.Field[] {
                    new APIController.Field("uuid"),
                    new APIController.Field("name")
                }),
                new APIController.Field("pacient", new APIController.Field[] {
                    new APIController.Field("uuid")
                })
            }
        );


    }


}
