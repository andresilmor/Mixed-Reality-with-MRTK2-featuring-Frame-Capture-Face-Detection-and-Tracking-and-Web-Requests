using System;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public class TestingProto {
    [ProtoMember(1)]
    public IEnumerable<string> nome { get; set; }

}
