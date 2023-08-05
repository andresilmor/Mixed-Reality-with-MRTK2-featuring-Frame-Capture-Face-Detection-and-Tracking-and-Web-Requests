using Realms;

public class InstitutionEntity : RealmObject {

    [PrimaryKey]
    public string UUID { get; set; }

    public InstitutionEntity() { }

    public InstitutionEntity(string uUID) {
        UUID = uUID;
    }
}
