using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Realms;
using System;
using Newtonsoft.Json.Linq;

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

        }

    }

    /// <summary>
    /// Checks existance of UserEntity with the gived userUUID, if already existes just updates the content based on the response, if not, a new one is created.
    /// </summary>
    /// <param name="response"></param>
    /// <param name="userUUID"></param>
    /// <returns>True: Updated/created and commited | False: Did not commit</returns>
    public static bool CreateUpdateUser(JObject response, string userUUID)
    {
        RealmObject userObject = RealmController.realm.Find<UserEntity>(userUUID);

        using (Realm realm = RealmController.realm)
        {
            using (Transaction transiction = realm.BeginWrite())
            {
                try
                {
                    if (userObject == null)
                    {
                        userObject = new UserEntity(
                                UUID: response["data"]["memberLogin"]["uuid"].ToString(),
                                token: response["data"]["memberLogin"]["token"].ToString()
                        );
                        realm.Add(userObject);
                        Debug.Log("User Added");

                    }
                    else
                    {
                        (userObject as UserEntity).Token = response["data"]["memberLogin"]["token"].ToString();
                        realm.Add(userObject, update: true);
                        Debug.Log("User Updtted");

                    }
                    transiction.Commit();
                    return true;

                }
                catch (Exception ex)
                {
                    transiction.Dispose();
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
        InstitutionEntity institution = RealmController.realm.Find<InstitutionEntity>(relationship["institution"]["uuid"].ToString());

        using (var realm = RealmController.realm)
        {
            var transiction = realm.BeginWrite();
            try
            {
                if (institution == null)
                {
                    institution = new InstitutionEntity(relationship["institution"]["uuid"].ToString());
                    RealmController.realm.Add(institution);

                }

                (userObject as UserEntity).MemberOf.Add(new MemberOf(relationship["role"].ToString(), institution));
                RealmController.realm.Add(userObject, update: true);
                transiction.Commit();
                return true;

            }
            catch (Exception ex)
            {
                Debug.Log("Error: " + ex.Message);
                transiction.Dispose();
                return false;

            }

        }
    }

}
