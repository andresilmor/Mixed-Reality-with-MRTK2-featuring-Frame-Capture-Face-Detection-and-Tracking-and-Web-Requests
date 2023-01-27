using System;
using ProtoBuf;

[ProtoContract]
public class FaceRect {
    [ProtoMember(1)]
    public uint x1 { get; set; }

    [ProtoMember(2)]
    public uint y1 { get; set; }

    [ProtoMember(3)]
    public uint x2 { get; set; }

    [ProtoMember(4)]
    public uint y2 { get; set; }

}
