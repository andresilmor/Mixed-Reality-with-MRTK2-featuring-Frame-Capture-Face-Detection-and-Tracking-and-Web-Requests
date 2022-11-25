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

        await APIController.ExecuteRequest(RealmController.realm.Find<UserEntity>(AccountController.currentUserUUID).Token.ToString().Trim(), queryOperation,
            (message, succeed) => {
                try {
                    if (succeed) { 
                        JObject response = JObject.Parse(@message);

                        foreach (JToken medicationToTake in response["data"]["medicationToTake"])
                            RealmController.CreateUpdateMedicationToTake(medicationToTake);

                    }

                } catch (Exception e) {
                    Debug.LogException(e);

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

    private static void CreateTimeEventHandler()
    {



    }

}
