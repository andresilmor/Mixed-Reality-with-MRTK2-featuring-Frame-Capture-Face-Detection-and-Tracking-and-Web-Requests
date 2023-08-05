using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QRCodeDecodeReply {
  

    [SerializeField]
    public class Detection {
        public string content { get; set; } 
        public QRCodeBounds bounds { get; set; }
        public QRCodeSize size { get; set; }
    }

    [SerializeField]
    public class QRCodeSize {
        public float width { get; set; }
        public float height { get; set; }
    }

    [SerializeField]
    public class QRCodeBounds {
        public QRCodeBound TL { get; set; }
        public QRCodeBound TR { get; set; }
        public QRCodeBound BL { get; set; }
        public QRCodeBound BR { get; set; }
    }

    [SerializeField]
    public class QRCodeBound {
        public float x { get; set; } 
        public float y { get; set; } 
    }

    [SerializeField]
    public class DetectionsList {
        public List<Detection> qrCodes { get; set; }
    }


}



