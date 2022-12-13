using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Realms;
using System.Linq;
using System.Threading.Tasks;

public static class NotificationsManager {
    public static List<TimedEventHandler> timedEventsList { get; private set; }


    public static async Task SetupMedicationAlerts(string institutionUUID) {
        if (timedEventsList == null)
            timedEventsList = new List<TimedEventHandler>();

        APIManager.Field queryOperation = new APIManager.Field(
                    "medicationToTake", new APIManager.FieldParams[] {
                        new APIManager.FieldParams("memberID", "\"" + AccountManager.currentUserUUID + "\""),
                        new APIManager.FieldParams("institutionID", "\"" + institutionUUID + "\""),
                    });

        await APIManager.ExecuteRequest(RealmManager.realm.Find<UserEntity>(AccountManager.currentUserUUID).Token.ToString().Trim(), queryOperation,
            (message, succeed) => {
                try {
                    if (succeed) {
                        JObject response = JObject.Parse(@message);

                        if ((response["data"]["medicationToTake"] as JArray).Count > 0) {
                            foreach (JToken medicationToTake in response["data"]["medicationToTake"])
                                RealmManager.CreateUpdateMedicationToTake(medicationToTake, institutionUUID);

                            AppCommandCenter.Instance.StartCoroutine(CreateTimerMedication(institutionUUID));

                        }

                    }

                } catch (Exception e) {
                    Debug.LogException(e);

                }


            },
            new APIManager.Field[] {
                new APIManager.Field("atTime"),
                new APIManager.Field("quantity"),
                new APIManager.Field("timeMeasure"),
                new APIManager.Field("intOfTime"),
                new APIManager.Field("medication", new APIManager.Field[] {
                    new APIManager.Field("uuid"),
                    new APIManager.Field("name")
                }),
                new APIManager.Field("pacient", new APIManager.Field[] {
                    new APIManager.Field("uuid")
                })
            }
        );


    }

    private static IEnumerator CreateTimerMedication(string institutionUUID) {
        foreach (MedicationToTakeEntity medicationToTake in RealmManager.realm.All<MedicationToTakeEntity>().Filter(
            "Pacient.InstitutionInCare.UUID == '" + institutionUUID + "'"
            )) {
            if (medicationToTake.AtTime.HasValue) {
                TimedEventManager.AddUpdateTimedEvent(medicationToTake.ID, new TimedEventHandler(Parser.NormalizeRealmDateTime(medicationToTake.AtTime.ToString()), () => { }));
                Debug.Log(TimedEventManager.GetTimedEventTimeLeft(medicationToTake.ID));

                Debug.Log(TimedEventManager.GetTimers(10).Count);

            }
            yield return null;

        }




    }

}
