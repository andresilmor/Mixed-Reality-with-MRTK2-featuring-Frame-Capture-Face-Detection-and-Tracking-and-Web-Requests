using BestHTTP.Logger;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Realms;

public static class NotificationsController 
{   

    async public static void SetupMedicationAlerts(string institutionUUID)
    {

        APIController.Field queryOperation = new APIController.Field(
                    "medicationToTake", new APIController.FieldParams[] {
                        new APIController.FieldParams("memberID", "\"" + AccountController.currentUserUUID + "\""),
                        new APIController.FieldParams("institutionID", "\"" + institutionUUID + "\""),
                    });

        await APIController.ExecuteRequest("Read", RealmController.realm.Find<UserEntity>(AccountController.currentUserUUID).Token.ToString().Trim(), queryOperation,
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
