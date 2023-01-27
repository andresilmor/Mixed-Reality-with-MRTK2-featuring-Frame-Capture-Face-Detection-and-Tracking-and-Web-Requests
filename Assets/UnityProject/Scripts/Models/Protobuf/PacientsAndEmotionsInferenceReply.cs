using System;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public class PacientsAndEmotionsInferenceReply {
    [ProtoMember(1)]
    public IEnumerable<PacientAndEmotionDetected> detections { get; set; }

}

