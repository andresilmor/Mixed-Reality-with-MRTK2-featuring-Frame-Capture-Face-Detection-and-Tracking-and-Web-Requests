using Realms;
using System.Collections.Generic;

class UserEntity : RealmObject
{
    [PrimaryKey]
    public string UUID { set; get; }

    public string Role { set; get; }

    public string Token { set; get; }

    public IList<MemberOf> MemberOf { get; }
  

    public UserEntity() { }

    public UserEntity(string UUID, string token)
    {
        this.UUID = UUID;
        Token = token;
    }   
}
