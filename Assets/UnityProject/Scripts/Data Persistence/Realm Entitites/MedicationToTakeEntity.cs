using Realms;
using System;


public class MedicationToTakeEntity : RealmObject {
    [PrimaryKey]
    public string ID { get; set; } = Guid.NewGuid().ToString();

    public byte Quantity { get; set; }
    public int IntOfTime { get; set; }
    public string TimeMeasure { get; set; }

    public bool TimeOut { get; set; }

    public DateTimeOffset? AtTime { get; set; }

    public PacientEntity Pacient { get; set; }

    public MedicationEntity Medication { get; set; }

    public MedicationToTakeEntity() { }

    public MedicationToTakeEntity(byte quantity, string timeMeasure, int intOfTime, DateTimeOffset atTime, PacientEntity pacient, MedicationEntity medication) {
        Quantity = quantity;
        IntOfTime = intOfTime;
        TimeMeasure = timeMeasure;
        this.AtTime = atTime;
        Pacient = pacient;
        Medication = medication;

        TimeOut = false;

    }

    public MedicationToTakeEntity(byte quantity, string timeMeasure, int intOfTime, PacientEntity pacient, MedicationEntity medication) {
        AtTime = null;
        Quantity = quantity;
        IntOfTime = intOfTime;
        TimeMeasure = timeMeasure;
        Pacient = pacient;
        Medication = medication;

        TimeOut = false;

    }

}
