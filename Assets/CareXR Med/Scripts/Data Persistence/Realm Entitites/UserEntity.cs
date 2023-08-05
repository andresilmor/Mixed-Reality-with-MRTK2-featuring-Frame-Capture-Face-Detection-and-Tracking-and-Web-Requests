using Realms;
using System.Collections.Generic;

class UserEntity : RealmObject {
    [PrimaryKey]
    public string Email { set; get; }

    public string UUID { set; get; }

    public string CurrentRole { set; get; }

    public string Token { set; get; }

    public IList<MemberOf> MemberOf { get; }


    public UserEntity() { }

    public UserEntity(string email, string UUID, string token) {
        this.Email = email;
        this.UUID = UUID;
        this.Token = token;
    }
}
