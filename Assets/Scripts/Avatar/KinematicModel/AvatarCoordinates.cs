public class AvatarCoordinates
{
    public int TimeIndex { get; protected set; }
    public AvatarVector Q { get; protected set; }
    public AvatarVector QDot { get; protected set; }
    public AvatarVector QDDot { get; protected set; }
    public int Length { get { return Q.Length; } }

    public AvatarCoordinates(int _timeIndex, AvatarVector _q, AvatarVector _qdot, AvatarVector _qddot)
    {
        TimeIndex = _timeIndex;
        Q = _q;
        QDot = _qdot;
        QDDot = _qddot;
    }
}
