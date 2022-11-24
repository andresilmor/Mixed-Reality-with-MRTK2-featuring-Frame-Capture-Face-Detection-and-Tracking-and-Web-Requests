using Realms;

public class PacientEntity : RealmObject
{

    [PrimaryKey]
    public string UUID { get; set; }    


    public PacientEntity() { }

    public PacientEntity(string uuid)
    {
        UUID = uuid;
    }
}
