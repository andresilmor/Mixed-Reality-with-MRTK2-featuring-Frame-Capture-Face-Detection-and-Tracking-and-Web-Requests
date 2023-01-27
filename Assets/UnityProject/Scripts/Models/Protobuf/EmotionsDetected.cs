using System;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public class EmotionsDetected {
    [ProtoMember(1)]
    public IDictionary<string,float> continuous { get; set; }

    [ProtoMember(2)]
    public IEnumerable<string> categorical { get; set; }

}