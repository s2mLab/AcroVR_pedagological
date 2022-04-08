using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiorbdKinematicModelInfo : KinematicModelInfo
{
    public BiorbdNode[] SensorNodes { get; protected set; }
    public BiorbdKinematicModelInfo(BiorbdNode[] _sensorNodes)
    {
        SensorNodes = _sensorNodes;
    }
}
