using System;
using System.Collections.Generic;
using UnityEngine;
using Xsens;
using XDA;

// =================================================================================================================================================================
/// <summary> This is the actual module that interfaces XSens so it is usable by the avatar. </summary>

public class XSensModule
{
    // Xda related variables
    MyXda _xda = new MyXda();
    public List<MasterInfo> AllScannedStations { get; protected set; } = new List<MasterInfo>();
    public int SelectedScanStationIndex { get; protected set; } = 0; // Select the first station, even though more are found
    public XsDevice Device { get; protected set; } = null;
    public XsDevicePtrArray Sensors {
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
    protected List<uint> SensorIds = new List<uint>();
    Dictionary<XsDevice, MyMtCallback> DevicesCallbackDict = new Dictionary<XsDevice, MyMtCallback>();
    public int NbSensors { get { return (int)Sensors.size(); } }

    // Sensor configuration variables
    protected int _updateRate = 120; // Acquisition frequency driven by UpdateRate property
    public int UpdateRate
    {
        get => _updateRate;
        set { 
            if (value != 60 || value != 120) 
            {
                Debug.LogWarning("Wrong values for UpdateRate, only 60 or 120Hz is allowed. Default value (120Hz) is selected");
                _updateRate = 120;
            } else
            {
                _updateRate = value;
            }
        }
    }
    // Status related variables
    public bool IsStationInitialized { get; protected set; } = false;
    public bool IsSensorsConnected { get; protected set; } = false;

    // Trial related variables
    public bool IsRecording { get; protected set; } = false;
    DateTime TrialStartingTime;
    public XSensData CurrentData { get; protected set; } = null;
    public List<XSensData> Data { get; protected set; } = new List<XSensData>();
    
    public XSensModule()
	{
        // Make sure decimal separator is the point (for instance, on french computers)
        System.Globalization.NumberFormatInfo nfi = new System.Globalization.NumberFormatInfo();
        nfi.NumberDecimalSeparator = ".";
        System.Globalization.CultureInfo ci = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
        ci.NumberFormat = nfi;
        System.Threading.Thread.CurrentThread.CurrentCulture = ci;

        // processPeriodInMs = 1000 / processFreq;
    }

    public void Disconnect()
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

    public bool InitializeStationsAndDevice()
    {
        // Prevents from connecting multiple times
        if (IsSensorsConnected)
        {
            Debug.Log("System already connected. Nothing to do");
            return false;
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

    public bool SetupSensors(int _expectedNbSensors)
    {
        if (AllScannedStations.Count == 0)
        {
            Debug.Log("No station connected, cannot setup");
            return false;
        }

        // Setup each sensor
        SensorIds.Clear();
        for (uint i = 0; i < Sensors.size(); i++)
        {
            XsDevice mtw = new XsDevice(Sensors.at(i));
            SensorIds.Add(mtw.deviceId().toInt());

            // Connect signals
            MyMtCallback callback = new MyMtCallback();
            callback.DataAvailable += new EventHandler<DataAvailableArgs>(HandlerDataAvailableCallback);

            mtw.addCallbackHandler(callback);
            DevicesCallbackDict.Add(mtw, callback); // To be removed?
        }

        IsSensorsConnected = NbSensors == _expectedNbSensors;
        return IsSensorsConnected;
    }

    public void FinalizeSetup()
    {

        Device.setOptions(XsOption.XSO_Orientation | XsOption.XSO_Calibrate, XsOption.XSO_None);
        Device.gotoMeasurement();
    }


    public void StartTrial()
    {
        TrialStartingTime = DateTime.Now;
        IsRecording = true;
        Data.Clear();
    }

    public void StopTrial()
    {
        IsRecording = false;
    }


	// =================================================================================================================================================================
	/// <summary> Fonction "callback" utilisé pour lire les données des senseurs XSens à la fréquence d'échantillonnage spécifiée. </summary>

	void HandlerDataAvailableCallback(object sender, DataAvailableArgs e)
	{
        var rand = new System.Random();
        double[] _data = new double[NbSensors];
        for (int i = 0; i < NbSensors; i++)
        {
            _data[i] = rand.NextDouble();
        }

        CurrentData = new XSensData(
            DateTime.Now.Subtract(TrialStartingTime).TotalMilliseconds,
            (int)e.Packet.timeOfArrival().msTimeOfDay(), 
            e.Packet.orientationMatrix(),
            e.Packet.orientationEuler(),
            e.Packet.orientationQuaternion(),
            e.Packet.calibratedAcceleration()
        );

        if (IsRecording)
        {
            Data.Add(CurrentData);
        }
    }
}
﻿