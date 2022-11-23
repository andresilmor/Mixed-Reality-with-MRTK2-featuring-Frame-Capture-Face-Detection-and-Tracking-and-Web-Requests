using Realms;
using System;
using MongoDB.Bson;


public class MedicationToTakeEntity 
{
    [PrimaryKey]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

    public byte Quantity { get; set; }

    public DateTime atTime { get; set; }

    public PacientEntity Pacient { get; set; }

    public MedicationEntity Medication { get; set; }

    public MedicationToTakeEntity() { }

    public MedicationToTakeEntity(byte quantity, DateTime atTime, PacientEntity pacient, MedicationEntity medication)
    {
        Quantity = quantity;
        this.atTime = atTime;
        Pacient = pacient;
        Medication = medication;
    }


}
