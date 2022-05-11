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

    static public AvatarCoordinates operator +(AvatarCoordinates _first, AvatarCoordinates _second)
    {
        return new AvatarCoordinates(
            _first.TimeIndex, _first.Q + _second.Q, _first.QDot + _second.QDot, _first.QDDot + _second.QDDot
        );
    }
    static public AvatarCoordinates operator *(double _scalar, AvatarCoordinates _old)
    {
        return new AvatarCoordinates(
            _old.TimeIndex, _scalar * _old.Q, _scalar * _old.QDot, _scalar * _old.QDDot
        );
    }
    static public AvatarCoordinates operator *(AvatarCoordinates _old, double _scalar)
    {
        return _scalar * _old;
    }
}
