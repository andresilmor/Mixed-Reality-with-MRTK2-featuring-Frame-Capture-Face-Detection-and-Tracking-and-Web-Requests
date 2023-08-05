using System;
using ProtoBuf;

[ProtoContract]
public class ProtoImage
{
    [ProtoMember(1)]
    public byte[] image { get; set; }    

}
