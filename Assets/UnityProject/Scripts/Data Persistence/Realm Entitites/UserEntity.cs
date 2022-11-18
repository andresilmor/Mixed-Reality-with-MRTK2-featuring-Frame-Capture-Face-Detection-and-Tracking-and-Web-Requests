using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Realms;

public class UserEntity : RealmObject
{
    [PrimaryKey]
    public string UserUUID
    {
        get => (string)UserUUID;
        set => UserUUID = (string)value;    
    }

    public string Role
    {
        get => (string)Role;
        set => Role = (string)value;
    }

    public string Token
    {
        get => (string)Token;
        set => Token = (string)value;   
    }

    public UserEntity() { }

    public UserEntity(string userUUID, string token)
    {
        UserUUID = userUUID;
        Token = token;
    }   
}
