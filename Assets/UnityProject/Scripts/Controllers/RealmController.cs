using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Realms;
using System;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using System.Linq;

public static class RealmController
{

    private static RealmConfiguration realmConfig = new RealmConfiguration
    {
        SchemaVersion = 1
    };
    public static Realm realm
    {
        get
        {
            realmConfig.ShouldDeleteIfMigrationNeeded = true;
            return Realm.GetInstance(realmConfig);
        }
        private set
        {
            realm = value;
        }
    }

    public static void BulldozeRealm()
    {
        using (var realm = RealmController.realm)
        {
            realm.Write(() => {
                realm.RemoveAll();

            });

            Debug.Log("The Realm was destroyed. No survivers.");

        }

    }

    /// <summary>
    /// Checks existance of UserEntity with the gived userUUID, if already existes just updates the content based on the data, if not, a new one is created.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="userUUID"></param>
    /// <returns>True: Updated/created and commited | False: Did not commit</returns>
    public static bool CreateUpdateUser(JObject data, string userUUID)
    {
        RealmObject userObject = RealmController.realm.Find<UserEntity>(userUUID);

        using (Realm realm = RealmController.realm)
        {
            using (Transaction transaction = realm.BeginWrite())
            {
                try
                {
                    if (userObject == null)
                    {
                        userObject = new UserEntity(
                                UUID: data["data"]["memberLogin"]["uuid"].Value<string>(),
                                token: data["data"]["memberLogin"]["token"].Value<string>()
                        );
                        realm.Add(userObject);

                    }
                    else
                    {
                        (userObject as UserEntity).Token = data["data"]["memberLogin"]["token"].Value<string>();
                        realm.Add(userObject, update: true);
                        Debug.Log("User Updtted");

                    }
                    transaction.Commit();
                    return true;

                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return false;

                }

            }

        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userObject"></param>
    /// <param name="relationship"></param>
    /// <returns></returns>
    public static bool CreateUpdateUserMembership(RealmObject userObject, JToken relationship)
    {
        InstitutionEntity institution = RealmController.realm.Find<InstitutionEntity>(relationship["institution"]["uuid"].Value<string>());

        using (Realm realm = RealmController.realm)
        {
            using (Transaction transaction = realm.BeginWrite()) {
                try
                {
                    if (institution == null)
                    {
                        institution = new InstitutionEntity(relationship["institution"]["uuid"].Value<string>());
                        RealmController.realm.Add(institution);

                    }

                    (userObject as UserEntity).MemberOf.Add(new MemberOf(relationship["role"].Value<string>(), institution));
                    RealmController.realm.Add(userObject, update: true);
                    transaction.Commit();
                    return true;

                }
                catch (Exception ex)
                {
                    Debug.Log("Error: " + ex.Message);
                    transaction.Rollback();
                    return false;

                }

            }

        }
    }

    public static bool CreateUpdateMedicationToTake(JToken data)
    {
        PacientEntity pacient = RealmController.realm.Find<PacientEntity>(data["pacient"]["uuid"].Value<string>());
        if (pacient is null)
            pacient = new PacientEntity(data["pacient"]["uuid"].Value<string>());

        MedicationEntity medication = RealmController.realm.Find<MedicationEntity>(data["medication"]["uuid"].Value<string>());
        if (medication is null)
            medication = new MedicationEntity(data["medication"]["uuid"].Value<string>(), data["medication"]["name"].Value<string>());

        MedicationToTakeEntity medicationToTake = null;
        medicationToTake = RealmController.realm.All<MedicationToTakeEntity>().Filter(
                "Medication.UUID == '" + medication.UUID + "' && Pacient.UUID == '" + pacient.UUID + "'"
                ).FirstOrDefault();

        if (medicationToTake is null)
        {
            if (data["atTime"].Type != JTokenType.Null)
                medicationToTake = new MedicationToTakeEntity(data["quantity"].Value<byte>(), DateTimeOffset.Parse(data["atTime"].Value<string>()), pacient, medication);
            else
                medicationToTake = new MedicationToTakeEntity(data["quantity"].Value<byte>(), pacient, medication);
        
        } else { 
            if (medicationToTake.atTime > DateTimeOffset.Parse(data["atTime"].Value<string>())) { 
                // TO DO (When we start to have mutations in the API XD
            
            }
        
        }

        Debug.Log(medicationToTake.Medication.Name);

        using (Realm realm = RealmController.realm)
        {
            using (Transaction transaction = realm.BeginWrite())
            {
                try {
                    realm.Add(pacient, update: true);
                    realm.Add(medication, update: true);
                    realm.Add(medicationToTake, update: true);

                    transaction.Commit();
                    return true;

                } catch (Exception ex) {
                    Debug.Log(ex.Message);
                    transaction.Rollback();
                    return false;

                }

            }

        }

    }

}
