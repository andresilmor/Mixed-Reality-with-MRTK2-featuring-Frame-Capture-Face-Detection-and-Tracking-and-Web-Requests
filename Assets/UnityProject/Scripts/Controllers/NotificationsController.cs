using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Realms;
using System.Linq;
using System.Threading.Tasks;

public static class NotificationsController {
    public static List<TimedEventHandler> timedEventsList { get; private set; }


    public static async Task SetupMedicationAlerts(string institutionUUID) {
        if (timedEventsList == null)
            timedEventsList = new List<TimedEventHandler>();

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

                        if ((response["data"]["medicationToTake"] as JArray).Count > 0) {
                            foreach (JToken medicationToTake in response["data"]["medicationToTake"])
                                RealmController.CreateUpdateMedicationToTake(medicationToTake, institutionUUID);

                            AppCommandCenter.Instance.StartCoroutine(CreateTimerMedication(institutionUUID));

                        }

                    }

                } catch (Exception e) {
                    Debug.LogException(e);

                }


            },
            new APIController.Field[] {
                new APIController.Field("atTime"),
                new APIController.Field("quantity"),
                new APIController.Field("timeMeasure"),
                new APIController.Field("intOfTime"),
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

    private static IEnumerator CreateTimerMedication(string institutionUUID) {
        foreach (MedicationToTakeEntity medicationToTake in RealmController.realm.All<MedicationToTakeEntity>().Filter(
            "Pacient.InstitutionInCare.UUID == '" + institutionUUID + "'"
            )) {
            if (medicationToTake.AtTime.HasValue) {
                TimedEventController.AddUpdateTimedEvent(medicationToTake.ID, new TimedEventHandler(Parser.NormalizeRealmDateTime(medicationToTake.AtTime.ToString()), () => { }));
                Debug.Log(TimedEventController.GetTimedEventTimeLeft(medicationToTake.ID));

                Debug.Log(TimedEventController.GetTimers(10).Count);

            }
            yield return null;

        }




    }

}
