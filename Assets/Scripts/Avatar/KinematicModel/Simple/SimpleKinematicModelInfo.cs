using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleKinematicModelInfo : KinematicModelInfo
{
    public int[] ParentIndex { get; private set; }
    public int NbSegments { get; private set; }
    public int NbSensors { get; private set; }

    public SimpleKinematicModelInfo(int[] _parentIndex, int _nbSegments, int _nbSensors)
    {
        ParentIndex = _parentIndex;
        NbSegments = _nbSegments;
        NbSensors = _nbSensors;
    }
}
