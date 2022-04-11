public class AvatarCoordinates
{
    public int TimeIndex { get; protected set; }
    public AvatarVector Q { get; protected set; }
    public int Length { get { return Q.Length; } }

    public AvatarCoordinates(int _timeIndex, AvatarVector _q)
    {
        TimeIndex = _timeIndex;
        Q = _q;
    }
}
