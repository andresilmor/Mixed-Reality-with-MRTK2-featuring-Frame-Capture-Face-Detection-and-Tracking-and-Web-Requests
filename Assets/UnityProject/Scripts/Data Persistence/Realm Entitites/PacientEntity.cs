using Realms;

#if ENABLE_WINMD_SUPPORT
using Debug = MRDebug;
#else
using Debug = UnityEngine.Debug;
#endif

public class PacientEntity : RealmObject {

    [PrimaryKey]
    public string UUID { get; set; }

    public InstitutionEntity InstitutionInCare { get; set; }


    public PacientEntity() { }

    public PacientEntity(string uuid) {
        UUID = uuid;
    }

    public PacientEntity(string uuid, InstitutionEntity institutionInCare) {
        UUID = uuid;
        InstitutionInCare = institutionInCare;
    }
}
