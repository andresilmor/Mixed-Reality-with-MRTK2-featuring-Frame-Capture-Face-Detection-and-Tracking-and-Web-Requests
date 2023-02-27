using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PersonAndEmotionsInferenceReply {
    [SerializeField]
    public class BodyCenter {
        public int x { get; set; }
        public int y { get; set; }
    }

    [SerializeField]
    public class Continuous {
        public double Valence { get; set; }
        public double Arousal { get; set; }
        public double Dominance { get; set; }
    }

    [SerializeField]
    public class Detection {
        public string uuid { get; set; }
        public BodyCenter bodyCenter { get; set; }
        public FaceRect faceRect { get; set; }
        public EmotionsDetected emotionsDetected { get; set; }
    }

    [SerializeField]
    public class EmotionsDetected {
        public Continuous continuous { get; set; }
        public List<string> categorical { get; set; }
    }

    [SerializeField]
    public class FaceRect : BoxRect {
        public FaceRect(int x1, int y1, int x2, int y2) : base(x1, y1, x2, y2) {
        }

    }

    [SerializeField]
    public class DetectionsList {
        public List<Detection> detections { get; set; }
    }


}


