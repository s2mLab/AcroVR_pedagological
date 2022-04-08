using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiorbdCoordinates
{
    protected BiorbdInterface KinematicModelHandler;
    public AvatarVector Q { get; protected set; }
    public AvatarVector QDot { get; protected set; }
    public AvatarVector QDDot { get; protected set; }
    public AvatarMatrixHomogenous[] Jcs { get; protected set; }

    public BiorbdCoordinates(BiorbdInterface _model, int _nbQ, int _nbQDot, int _nbQDDot)
    {
        KinematicModelHandler = _model;
        Q = new AvatarVector(_nbQ);
        QDot = new AvatarVector(_nbQDot);
        QDDot = new AvatarVector(_nbQDDot);
    }

    public void SetData(AvatarVector _q, AvatarVector _qDot, AvatarVector _qDDot)
    {
        Q = _q;
        QDot = _qDot;
        QDDot = _qDDot;
        Jcs = KinematicModelHandler.GlobalJcs(Q);
    }
}
