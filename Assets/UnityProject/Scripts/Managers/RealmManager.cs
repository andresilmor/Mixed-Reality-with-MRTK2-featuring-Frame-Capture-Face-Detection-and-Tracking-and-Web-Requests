using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Realms;
using System;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using System.Linq;

using Debug = MRDebug;

public static class RealmManager {

    private static RealmConfiguration realmConfig = new RealmConfiguration {
        SchemaVersion = 1
    };
    public static Realm realm {
        get {
            realmConfig.ShouldDeleteIfMigrationNeeded = true;
            return Realm.GetInstance(realmConfig);
        }
        private set {
            realm = value;
        }
    }

    public static void BulldozeRealm() {
        using (var realm = RealmManager.realm) {
            realm.Write(() => {
                realm.RemoveAll();

            });

            Debug.Log("The Realm was destroyed. No survivers.");

        }

    }

    public static string FindActiveUser(bool save = false) {
        List<UserEntity> userObject = RealmManager.realm.All<UserEntity>().Where(user => user.Token != "").ToList();
        Debug.Log("User Count: " + userObject.Count);

        if (userObject.Count > 0 && save) {
            Debug.Log("WTF MEN!");
            Debug.Log("User Token Realm: " + userObject[0].Token);
            AccountManager.Token = userObject[0].Token;
            AccountManager.ActiveUserEmail = userObject[0].Email;
            return userObject[0].Token;

        }
        Debug.Log("Returning null");
        return null;
      

    }

    public static bool LogoutUser(string userEmail) {
        RealmObject userObject = RealmManager.realm.Find<UserEntity>(userEmail);

        using (Realm realm = RealmManager.realm) {
            using (Transaction transaction = realm.BeginWrite()) {
                try {
                    (userObject as UserEntity).Token = "";
                    realm.Add(userObject, update: true);
                  
                    transaction.Commit();
                    return true;

                } catch (Exception ex) {
                    transaction.Rollback();
                    return false;

                }

            }

        }

    }


    /// <summary>
    /// Checks existance of UserEntity with the gived userEmail, if already existes just updates the content based on the Data, if not, a new one is created.
    /// </summary>
    /// <param Name="data"></param>
    /// <param Name="userEmail"></param>
    /// <returns>True: Updated/created and commited | False: Did not commit</returns>
    public static bool CreateUpdateUser(JObject data, string userEmail) {
        RealmObject userObject = RealmManager.realm.Find<UserEntity>(userEmail);
        Debug.Log("User " + (userObject != null ? "Founded" : "Not Founded"));
        using (Realm realm = RealmManager.realm) {
            using (Transaction transaction = realm.BeginWrite()) {
                try {
                    if (userObject == null) {
                        userObject = new UserEntity(
                                email: userEmail,
                                UUID: data["data"]["MemberLogin"]["uuid"].Value<string>(),
                                token: data["data"]["MemberLogin"]["token"].Value<string>()
                        );
                        realm.Add(userObject);
                        Debug.Log("User Added");


                    } else {
                        (userObject as UserEntity).Token = data["data"]["MemberLogin"]["token"].Value<string>();
                        realm.Add(userObject, update: true);
                        Debug.Log("User Updated");

                    }
                    transaction.Commit();
                    return true;

                } catch (Exception ex) {
                    transaction.Rollback();
                    return false;

                }

            }

        }

    }

    public static bool CreateUpdateUser(string uuid, string token, string userEmail) {
        RealmObject userObject = RealmManager.realm.Find<UserEntity>(userEmail);
        Debug.Log("User " + (userObject != null ? "Founded" : "Not Founded"));
        using (Realm realm = RealmManager.realm) {
            using (Transaction transaction = realm.BeginWrite()) {
                try {
                    if (userObject == null) {
                        userObject = new UserEntity(
                                email: userEmail,
                                UUID: uuid,
                                token: token
                        );
                        realm.Add(userObject);
                        Debug.Log("User Added");

                    } else {
                        (userObject as UserEntity).Token = token;
                        realm.Add(userObject, update: true);
                        Debug.Log("User Updated");

                    }
                    transaction.Commit();
                    return true;

                } catch (Exception ex) {
                    transaction.Rollback();
                    return false;

                }

            }

        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param Name="userObject"></param>
    /// <param Name="relationship"></param>
    /// <returns></returns>
    public static bool CreateUpdateUserMembership(RealmObject userObject, JToken relationship) {
        InstitutionEntity institution = RealmManager.realm.Find<InstitutionEntity>(relationship["institution"]["uuid"].Value<string>());

        using (Realm realm = RealmManager.realm) {
            using (Transaction transaction = realm.BeginWrite()) {
                try {
                    if (institution == null) {
                        institution = new InstitutionEntity(relationship["institution"]["uuid"].Value<string>());
                        RealmManager.realm.Add(institution);

                    }

                    (userObject as UserEntity).MemberOf.Add(new MemberOf(relationship["role"].Value<string>(), institution));
                    RealmManager.realm.Add(userObject, update: true);
                    transaction.Commit();
                    return true;

                } catch (Exception ex) {
                    Debug.Log("Error: " + ex.Message);
                    transaction.Rollback();
                    return false;

                }

            }

        }
    }

    public static bool CreateUpdateMedicationToTake(JToken data, string institutionResponsible) {
        PacientEntity pacient = RealmManager.realm.Find<PacientEntity>(data["pacient"]["uuid"].Value<string>());
        if (pacient is null)
            pacient = new PacientEntity(data["pacient"]["uuid"].Value<string>(), RealmManager.realm.Find<InstitutionEntity>(institutionResponsible));

        MedicationEntity medication = RealmManager.realm.Find<MedicationEntity>(data["medication"]["uuid"].Value<string>());
        if (medication is null)
            medication = new MedicationEntity(data["medication"]["uuid"].Value<string>(), data["medication"]["Name"].Value<string>());

        MedicationToTakeEntity medicationToTake = null;
        medicationToTake = RealmManager.realm.All<MedicationToTakeEntity>().Filter(
                "Medication.UUID == '" + medication.UUID + "' && Pacient.UUID == '" + pacient.UUID + "'"
                ).FirstOrDefault();

        if (medicationToTake is null) {
            if (data["atTime"].Type != JTokenType.Null)
                medicationToTake = new MedicationToTakeEntity(data["quantity"].Value<byte>(), data["timeMeasure"].Value<string>(), data["intOfTime"].Value<int>(), DateTimeOffset.Parse(data["atTime"].Value<string>()), pacient, medication);
            else
                medicationToTake = new MedicationToTakeEntity(data["quantity"].Value<byte>(), data["timeMeasure"].Value<string>(), data["intOfTime"].Value<int>(), pacient, medication);

        } else {
            if (medicationToTake.AtTime > DateTimeOffset.Parse(data["atTime"].Value<string>())) {
                // TO DO (When we start to have mutations in the API XD

            }

        }

        using (Realm realm = RealmManager.realm) {
            using (Transaction transaction = realm.BeginWrite()) {
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
