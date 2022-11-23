using Realms;

class MemberOf : EmbeddedObject
{

    public string Role { get; set; }

    public InstitutionEntity Institution { get; set; }


    public MemberOf() { }

    public MemberOf(string role, InstitutionEntity institution)
    {
        Role = role;
        Institution = institution;
    }

}
