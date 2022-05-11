using UnityEngine;
using System.Timers;
using System.Text;
using System.Runtime.InteropServices;

public class BiorbdKinematicModel : KalmanInterface
{
    public AvatarVector CurrentQ { get; protected set; }
    Timer TimerHolder;

    public BiorbdKinematicModel(KinematicModelInfo _kinematicModelInfo) : 
        base(_kinematicModelInfo)
    {
        
	}

    protected override void Initialize()
    {
        Debug.Log("Initialize must be call with path for Biorbd");
    }

    protected override void Initialize(string _path)
    {
        base.Initialize(_path);
        CurrentQ = new AvatarVector(NbQ);

        TimerHolder = new Timer();
        TimerHolder.Interval = 500; // In milliseconds
        TimerHolder.AutoReset = true;
        TimerHolder.Elapsed += new ElapsedEventHandler(ChangeQ);
        TimerHolder.Start();

        void ChangeQ(object sender, ElapsedEventArgs e)
        {
            CurrentQ.Set(0, CurrentQ.Get(0) + 0.01);
        }
    }
}
