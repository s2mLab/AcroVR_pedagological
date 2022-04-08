using System;
using System.Collections.Generic;
using UnityEngine;
using Xsens;
using XDA;

// =================================================================================================================================================================
/// <summary> This is the actual module that interfaces XSens so it is usable by the avatar. </summary>

public class XSensModule : AvatarControllerModule
{
    public XSensModule(AvatarKinematicModel _avatar) 
        : base(_avatar)
    {
        UpdateRate = (int)120.0;
    }

    // Xda related variables
    MyXda _xda = new MyXda();

    // Devices
    protected List<MasterInfo> AllScannedStations = new List<MasterInfo>();
    protected int SelectedScanStationIndex = 0; // Select the first station, even though more are found
    protected XsDevice Device = null;
    public bool IsStationInitialized { get; protected set; } = false;

    // Sensors (IMUs)
    protected XsDevicePtrArray Sensors {
        get
        {
            if (Device is null)
            {
                Debug.Log("No device is connected yet.");
                return null;
            }

            return Device.children();
        }
    }
    public override int NbSensorsConnected() 
    { 
        return (int)Sensors.size(); 
    } 
    protected Dictionary<uint, uint> SensorsMap = new Dictionary<uint, uint>();
    Dictionary<XsDevice, MyMtCallback> DevicesCallbackDict = new Dictionary<XsDevice, MyMtCallback>();

    // Sensor configuration variables
    public int UpdateRate
    {
        get => (int)AcquisitionFrequency;
        set {
            if (value != 60 && value != 120)
            {
                Debug.LogWarning($"Wrong values for UpdateRate (yours : {(value - 120) == 0}), only 60 or 120Hz is allowed. Default value (120Hz) is selected");
                AcquisitionFrequency = 120;
            } else
            {
                AcquisitionFrequency = value;
            }
        }
    }
    
    // Trial related variables
    protected int PreviousPacketCounter = -1;
    
    public override void Disconnect()
    {
        // Closing devices so they can be reuse by another software
        if (Device != null)
        {
            if (Device.isRecording())
                Device.stopRecording();
            Device.gotoConfig();
            Device.disableRadio();
            Device.clearCallbackHandlers();
        }

        if (_xda != null)
        {
            _xda.Dispose();
        }
        _xda = new MyXda();  // Reset it in the case a new one is created

        IsStationInitialized = false;
        IsSensorsConnected = false;
    }

    public override bool SetupMaterial()
    {
        // Prevents from connecting multiple times
        if (IsStationInitialized)
        {
            Debug.Log("System already connected. Nothing to do");
            return true;
        }

        // Find the station to connect to
        ScanForStations();
        if (AllScannedStations.Count == 0)
        {
            Debug.Log("No station were found. Stopping the preparing procedure.");
            return false;
        }
        
        // Setup the measuring device
        Device = _xda.getDevice(AllScannedStations[SelectedScanStationIndex].DeviceId);
        Device.setUpdateRate(UpdateRate);

        // Activer radio signal on devices
        if (!EnableRadioOnDevice())
        {
            Debug.Log("Could not enable the radio on the device. Stopping the preparing procedure");
            return false;
        }

        IsStationInitialized = true;

        return true;
    }

    protected void ScanForStations()
    {
        _xda.scanPorts();
        if (_xda._DetectedDevices.Count > 0)
        {
            foreach (XsPortInfo portInfo in _xda._DetectedDevices)
            {
                if (portInfo.deviceId().isWirelessMaster())
                {
                    _xda.openPort(portInfo);
                    MasterInfo ai = new MasterInfo(portInfo.deviceId());
                    ai.ComPort = portInfo.portName();
                    ai.BaudRate = portInfo.baudrate();
                    AllScannedStations.Add(ai);
                }
            }
        }
    }

    protected bool EnableRadioOnDevice()
    {
        if (Device != null)
        {
            if (Device.isRadioEnabled())
                Device.disableRadio();

            // Test all channel until one works
            for (int i = 12; i < 26; ++i)
                if (Device.enableRadio(i))
                    return true;
        }
        // If we get here, the radio could not be enabled
        return false;
    }

    public override bool SetupSensors()
    {
        if (AllScannedStations.Count == 0)
        {
            Debug.Log("No station connected, cannot setup");
            return false;
        }

        // Setup each sensor
        SensorsMap.Clear();
        Device.clearCallbackHandlers();
        DevicesCallbackDict.Clear();
        try
        {
            for (uint i = 0; i < Sensors.size(); i++)
            {
                XsDevice mtw = new XsDevice(Sensors.at(i));
                SensorsMap.Add(mtw.deviceId().toInt(), i);

                // Connect signals
                MyMtCallback callback = new MyMtCallback();
                callback.DataAvailable += new EventHandler<DataAvailableArgs>(HandlerDataAvailableCallback);

                mtw.addCallbackHandler(callback);
                DevicesCallbackDict.Add(mtw, callback);
            }
        }
        catch (Exception)
        {
            // If any exception is thrown, the process has failed, but probably not for good. 
            // So allow to continue
            Debug.Log("Unhandled exception in XSens initialization.");
            return false;
        }

        IsSensorsConnected = NbSensorsConnected() == NbSensorsExpected;
        return IsSensorsConnected;
    }

    public override bool FinalizeSetup()
    {

        Device.setOptions(XsOption.XSO_Orientation | XsOption.XSO_Calibrate, XsOption.XSO_None);
        Device.gotoMeasurement();

        return base.FinalizeSetup();
    }


    public override void StartTrial()
    {
        base.StartTrial();
        PreviousPacketCounter = -1;
    }

    protected override void SetNewFrame(int FrameNumber)
    {
        base.SetNewFrame(FrameNumber);
        PreviousPacketCounter = FrameNumber;
    }

    void HandlerDataAvailableCallback(object sender, DataAvailableArgs e)
	{
        // If a new time frame comes, save then reinitialize the Data holder
        int _currentPacketCounter = (int)e.Packet.packetCounter();
        if (PreviousPacketCounter != _currentPacketCounter)
        {
            SetNewFrame(_currentPacketCounter);   
        }

        // Store the data into the current Data holder
        int _imuIndex  = (int)(SensorsMap[e.Device.deviceId().toInt()]);
        _currentData.AddData(
            _imuIndex,
            new AvatarMatrixRotation(e.Packet.orientationMatrix())
        );
    }
}

