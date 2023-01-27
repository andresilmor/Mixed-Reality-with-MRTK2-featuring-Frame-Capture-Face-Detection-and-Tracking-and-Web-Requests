using System;
using ProtoBuf;

[ProtoContract]
public class PacientAndEmotionDetected {
    [ProtoMember(1)]
    public string uuid { get; set; }

    [ProtoMember(2)]
    public BodyBoxCenter bodyCenter { get; set; }

    [ProtoMember(3)]
    public FaceRect faceRect { get; set; }

    [ProtoMember(4)]
    public EmotionsDetected emotionsDetected { get; set; }

}