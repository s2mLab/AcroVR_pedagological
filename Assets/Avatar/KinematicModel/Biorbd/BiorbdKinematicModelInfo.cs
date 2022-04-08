using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiorbdKinematicModelInfo : KinematicModelInfo
{
    public string BiorbdPath { get; protected set; }
    public BiorbdNode[] SensorNodes { get; protected set; }
    public BiorbdKinematicModelInfo(
        string _biorbdPath,
        BiorbdNode[] _sensorNodes
    )
    {
        BiorbdPath = _biorbdPath;
        SensorNodes = _sensorNodes;
    }
}
