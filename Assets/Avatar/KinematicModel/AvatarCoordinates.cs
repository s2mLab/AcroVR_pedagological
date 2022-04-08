public class AvatarCoordinates
{
    public int TimeIndex { get; protected set; }
    public AvatarMatrixHomogenous[] Jcs { get; protected set; }
    public int Length { get { return Jcs.Length; } }

    public AvatarCoordinates(int _timeIndex, AvatarMatrixHomogenous[] _jcs)
    {
        TimeIndex = _timeIndex;
        Jcs = _jcs;
    }
}
