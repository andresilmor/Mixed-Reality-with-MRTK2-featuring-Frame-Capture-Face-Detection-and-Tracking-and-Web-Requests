using System;
using ProtoBuf;

[ProtoContract]
public class BodyBoxCenter {
    [ProtoMember(1)]
    public uint x { get; set; }

    [ProtoMember(2)]
    public uint y { get; set; }

}