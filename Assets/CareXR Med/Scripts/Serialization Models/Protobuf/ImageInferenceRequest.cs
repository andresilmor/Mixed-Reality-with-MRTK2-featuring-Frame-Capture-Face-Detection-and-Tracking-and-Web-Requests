using System;
using ProtoBuf;

[ProtoContract]
public class ImageInferenceRequest {
    [ProtoMember(1)]
    public byte[] image { get; set; }

    [ProtoMember(2)]
    public string[] filter { get; set; }

}
