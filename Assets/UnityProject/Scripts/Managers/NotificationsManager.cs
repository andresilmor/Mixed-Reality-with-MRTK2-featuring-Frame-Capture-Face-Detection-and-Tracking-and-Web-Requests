using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Realms;
using System.Linq;
using System.Threading.Tasks;

using Debug = MRDebug;

public static class NotificationsManager {
    public static List<TimedEventHandler> timedEventsList { get; private set; }


    public static async Task SetupMedicationAlerts(string institutionUUID) {
        if (timedEventsList == null)
            timedEventsList = new List<TimedEventHandler>();

        GraphQL.Type queryOperation = new GraphQL.Type(
                    "medicationToTake", new GraphQL.Params[] {
                        new GraphQL.Params("memberID", "\"" + AccountManager.ActiveUserEmail + "\""),
                        new GraphQL.Params("institutionID", "\"" + institutionUUID + "\""),
                    });

        await APIManager.ExecuteRequest(RealmManager.realm.Find<UserEntity>(AccountManager.ActiveUserEmail).Token.ToString().Trim(), queryOperation,
            (message, succeed) => {
                try {
                    if (succeed) {
                        JObject response = JObject.Parse(@message);

                        if (response.HasValues && (response["Data"]["medicationToTake"] as JArray).Count > 0) {
                            foreach (JToken medicationToTake in response["Data"]["medicationToTake"])
                                RealmManager.CreateUpdateMedicationToTake(medicationToTake, institutionUUID);

                            AppCommandCenter.Instance.StartCoroutine(CreateTimerMedication(institutionUUID));

                        }

                    }

                } catch (Exception e) {
                    Debug.Log(e.Message);

                }


            },
            new GraphQL.Type[] {
                new GraphQL.Type("atTime"),
                new GraphQL.Type("quantity"),
                new GraphQL.Type("timeMeasure"),
                new GraphQL.Type("intOfTime"),
                new GraphQL.Type("medication", new GraphQL.Type[] {
                    new GraphQL.Type("uuid"),
                    new GraphQL.Type("Name")
                }),
                new GraphQL.Type("pacient", new GraphQL.Type[] {
                    new GraphQL.Type("uuid")
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
                Debug.Log(TimedEventManager.GetTimedEventTimeLeft(medicationToTake.ID).ToString());

                Debug.Log(TimedEventManager.GetTimers(10).Count.ToString());

            }
            yield return null;

        }

    }

}
