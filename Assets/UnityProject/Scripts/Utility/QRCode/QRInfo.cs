using System;
using Microsoft.MixedReality.QR;
namespace QRTracking {
    public class QRInfo {
        public QRInfo(Microsoft.MixedReality.QR.QRCode code) {
            Id = code.Id;
            SpatialGraphNodeId = code.SpatialGraphNodeId;
            Version = code.Version;
            PhysicalSideLength = code.PhysicalSideLength;
            Data = code.Data;
            SystemRelativeLastDetectedTime = code.SystemRelativeLastDetectedTime;
            LastDetectedTime = code.LastDetectedTime;
        }

        public Guid Id { get; }
        public Guid SpatialGraphNodeId { get; }
        public QRVersion Version { get; }
        public float PhysicalSideLength { get; }
        public string Data { get; }
        public TimeSpan SystemRelativeLastDetectedTime { get; }
        public DateTimeOffset LastDetectedTime { get; }

    }
}