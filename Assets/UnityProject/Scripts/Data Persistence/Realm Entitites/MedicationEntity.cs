using Realms;

public class MedicationEntity : RealmObject
{

    [PrimaryKey]
    public string UUID { get; set; }  

    public string Name { get; set; }    

    
    public MedicationEntity() { }

    public MedicationEntity(string uuid, string name)
    {
        UUID = uuid;
        Name = name;
    }   
}
