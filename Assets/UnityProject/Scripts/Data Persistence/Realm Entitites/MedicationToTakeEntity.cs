using Realms;
using System;


public class MedicationToTakeEntity : RealmObject
{
    [PrimaryKey]
    public string ID { get; set; } = Guid.NewGuid().ToString();

    public byte Quantity { get; set; }

    public DateTimeOffset atTime { get; set; }

    public PacientEntity Pacient { get; set; }

    public MedicationEntity Medication { get; set; }

    public MedicationToTakeEntity() { }

    public MedicationToTakeEntity(byte quantity, DateTimeOffset atTime, PacientEntity pacient, MedicationEntity medication)
    {
        Quantity = quantity;
        this.atTime = atTime;
        Pacient = pacient;
        Medication = medication;
    }

    public MedicationToTakeEntity(byte quantity, PacientEntity pacient, MedicationEntity medication)
    {
        Quantity = quantity;
        Pacient = pacient;
        Medication = medication;
    }

}
