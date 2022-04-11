using System.Timers;

public class XSensFakeModule : XSensModule
{
    Timer TimerHolder;
    protected bool IsLocked;  // Very basic mutex to manage the fact that the timer won't stop pushing data while debugging

    public XSensFakeModule(AvatarKinematicModel _avatar) 
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
        _currentData = null;
        _currentAvatarCoordinates = null;

        int _imgPerSecond = 20;
        TimerHolder = new Timer();
        TimerHolder.Interval = (int)(1/(double)_imgPerSecond * 1000); // In milliseconds
        TimerHolder.AutoReset = true;
        TimerHolder.Elapsed += new ElapsedEventHandler(HandlerDataAvailableCallback);
        TimerHolder.Start();

        return true;
    }

    public override void Disconnect()
    {
        TimerHolder.Stop();
    }

    protected bool RequestLock()
    {
        if (IsLocked) return false;
        IsLocked = true;
        return true;
    }

    protected void ReleaseLock()
    {
        IsLocked = false;
        return;
    }

    void HandlerDataAvailableCallback(object sender, ElapsedEventArgs e)
    {
        if (!RequestLock()) return;

        // Note: If the IMU is resting on its largest surface with the 
        // connectic hole towards you. Then the axes points (using left hand)
        // X+ rightward
        // Y+ frontward
        // Z+ downward
        try
        {
            PreviousPacketCounter++;
            SetNewFrame(PreviousPacketCounter);

            for (int i = 0; i < NbSensorsConnected(); i++)
            {
                double[] _angles = new double[3];
                _angles[0] = 0; //* ((i + 1) / 2.0);
                _angles[1] = 0; //* ((i + 1) / 2.0);
                _angles[2] = 0; // * ((i + 1) / 2.0);

                if (i == 1)
                {
                    _angles[2] += PreviousPacketCounter / 2.0;
                    //if (PreviousPacketCounter < 100)
                    //{
                    //    _angles[2] += PreviousPacketCounter / 25.0;
                    //}
                    //else
                    //{
                    //    _angles[2] += 100.0 / 5.0;
                    //    _angles[0] += PreviousPacketCounter / 25.0;
                    //}
                }

                _currentData.AddData(i, AvatarMatrixRotation.FromEulerXYZ(_angles));
            }
        }
        finally {
            ReleaseLock(); 
        }
    }
    
}
