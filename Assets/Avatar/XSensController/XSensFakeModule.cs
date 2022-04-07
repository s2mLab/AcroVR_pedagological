using System.Timers;

public class XSensFakeModule : XSensModule
{
    Timer TimerHolder;

    public XSensFakeModule(AvatarManager _avatar) 
        : base(_avatar)
    {
    }
    public override bool SetupMaterial()
    {
        IsStationInitialized = true;
        return true;
    }
    public override int NbSensorsConnected()
    {
        return NbSensorsExpected;
    }

    public override bool SetupSensors()
    {
        IsSensorsConnected = NbSensorsConnected() == NbSensorsExpected;
        return IsSensorsConnected;
    }

    public override bool FinalizeSetup()
    {
        CurrentData = null;

        TimerHolder = new Timer();
        TimerHolder.Interval = 500; // In milliseconds
        TimerHolder.AutoReset = true;
        TimerHolder.Elapsed += new ElapsedEventHandler(HandlerDataAvailableCallback);
        TimerHolder.Start();

        return true;
    }

    public override void Disconnect()
    {
        TimerHolder.Stop();
    }

    void HandlerDataAvailableCallback(object sender, ElapsedEventArgs e)
    {
        // Note: If the IMU is resting on its largest surface with the 
        // connectic hole towards you. Then the axes points (using left hand)
        // X+ rightward
        // Y+ frontward
        // Z+ downward

        PreviousPacketCounter++;

        CurrentData = new AvatarData(PreviousPacketCounter, NbSensorsConnected());
        for (uint i = 0; i < NbSensorsConnected(); i++)
        {
            double[] _angles = new double[3];
            _angles[0] = 0 * ((i + 1) / 2.0);
            _angles[1] = 0 * ((i + 1) / 2.0);
            _angles[2] = 0 * ((i + 1) / 2.0);

            if (i == 1)
            {
                _angles[2] += PreviousPacketCounter / 5.0;
            }

            CurrentData.AddData(
                i, AvatarMatrixRotation.FromEulerXYZ(_angles)
            );
        }
    }
    
}
