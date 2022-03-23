using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using Xsens;
using XDA;

// =================================================================================================================================================================
/// <summary> This is the actual module that interfaces XSens so it is usable by the avatar. </summary>

public class XSensModule
{
    // Xda related variables
    MyXda _xda = null;
    public List<MasterInfo> AllScannedStations { get; protected set; } = new List<MasterInfo>();
    public MasterInfo SelectedScanStation { get; protected set; }
    public XsDevice MeasuringDevice { get; protected set; } = null;
    XsDevicePtrArray _deviceIds;
    public XsDevicePtrArray DeviceIds {
        get
        {
            if (!IsConnected)
            {
                Debug.Log("No device is connected yet. Returning previous ids");
                return _deviceIds;
            }

            _deviceIds = MeasuringDevice.children();
            return _deviceIds;
        }
        protected set
        {
            _deviceIds = value;
        }
    } 
  

    Dictionary<XsDevice, MyMtCallback> DevicesCallbackDict = new Dictionary<XsDevice, MyMtCallback>();
    uint[] SensorIds;                             // Contient les identificateurs de chacun des senseurs XSens

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
    public int NbFramesToDropAtStart = 3;  // There is a lag at the start of the reading, this is the number of frame to drop to deal with this lag

    // Status related variables
    public bool IsConnected { get; protected set; } = false;
    public bool IsReady { get; protected set; } = false;
    public bool IsTrialTimerStarted { get; protected set; } = false;
    public int NbImusToConnect { get; protected set; }

    // Trial related variables
    public DateTime TimeSinceTrialStarted { get; protected set; }
    public int CurrentFrameIdx { get; protected set; } = 0;




    double beginTimeInMs;                           // Contient le temps du début du frame actuel, en millisecondes
    public static int processFreq = 100;            // Fréquence de traitement des données XSens par BioRBD (filtre de Kalman)
    //public static int processFreq = 50;             // Fréquence de traitement des données XSens par BioRBD (filtre de Kalman)
    
    //public static int updateRateXSens = 100;        // Fréquence de lecture/transmission des données XSens venant de la station XSens (Update rate)
    float processPeriodInMs;						// Durée de frame désirée, en millisecondes
	

	
	// Vecteurs circulaires qui sont utilisé pour transférer d'une façon sécuritaire les données XSens de la fonction Callback à d'autres fonctions
	// Un seul pointeur d'écriture (fonction Callback) est utilisé, tandis que plusieurs pointeurs de lecture (autres fonctions) peuvent être utilisé
	// Circulaire car une fois à la fin des vecteurs, on revient au début des vecteurs pour continuer à transférer les données
	// Sécuritaire car la fonction Callback modifie les données des vecteurs, tandis que les autres fonctions ne font que lire les données des vecteurs

	int[] listFrames;								// Contient les numéros de frame
	int[] listIdIMU;								// Contient les identificateurs des senseurs XSens
 	XsMatrix[] listMatrix;							// Contient les matrices de rotation
	XsEuler[] listEuler;							// Contient les angles Euler
    XsVector[] listAcc;                             // Contient les accélérations
	int nListWrite;                                 // Pointeur d'écriture des vecteurs circulaires
	int nListRead;									// Pointeur de lecture des vecteurs circulaires
	int nListSize = 10000;                           // Capacité de stockage des vecteurs circulaires

	public static int[] numCallbackByFrame;         // Contient le nombre d'appel à la fonction Callback à chaque frame
	public static int[] numIMUsByFrame;             // Contient le nombre de senseurs XSens lu à chaque frame
	public static double[] numMsByFrame;            // Contient la durée réelle de chaque frame, en millisecondes
	int nNumByFrameSize = 10000;                     // Capacité de stockage (nombre maximum de frames) des vecteurs précédents

	XsMatrix zerosMatrix = new XsMatrix(3, 3);      // Matrice de rotation contenant des zéros
    XsVector zerosAcc = new XsVector(3);            // Vecteur des accélérations contenant des zéros

    public XSensModule(int nbImusToConnect)
	{
		NbImusToConnect = nbImusToConnect;

        //processPeriodInMs = 1000 / processFreq;
        //zerosMatrix.setZero();
        //      zerosAcc.setZero();

        //// S'assurer que le séparateur décimal est bien un point et non une virgule

        //System.Globalization.NumberFormatInfo nfi = new System.Globalization.NumberFormatInfo();
        //nfi.NumberDecimalSeparator = ".";
        //System.Globalization.CultureInfo ci = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
        //ci.NumberFormat = nfi;
        //System.Threading.Thread.CurrentThread.CurrentCulture = ci;

        _xda = new MyXda();
    }

    // =================================================================================================================================================================
    /// <summary> Initialisation de l'accès aux senseurs XSens. </summary>

    public void PrepareSystem()
    {
        listFrames = new int[nListSize];
        listIdIMU = new int[nListSize];
        listMatrix = new XsMatrix[nListSize];
        listEuler = new XsEuler[nListSize];
        listAcc = new XsVector[nListSize];
        nListWrite = -1;
        nListRead = 0;

        numCallbackByFrame = new int[nNumByFrameSize];
        numIMUsByFrame = new int[nNumByFrameSize];
        numMsByFrame = new double[nNumByFrameSize];
        for (int i = 0; i < nNumByFrameSize; i++)
        {
            numCallbackByFrame[i] = 0;
            numIMUsByFrame[i] = 0;
            numMsByFrame[i] = 0;
        }

        // Prevents from connecting multiple times
        if (IsConnected) 
            return;

        // Find the station to connect to
        ScanForStations();
        if (AllScannedStations.Count == 0)
        {
            Debug.Log("No station were found. Stopping the preparing procedure.");
            return;
        }
        // Select the first one found, even though more are found
        SelectedScanStation = AllScannedStations[0];

        // Activer radio signal on devices
        if (!EnableRadioOnDevice())
        {
            Debug.Log("Could not enable the radio on the device. Stopping the preparing procedure");
            return;
        }

        SetupDevices();
    }

    //// =================================================================================================================================================================
    ///// <summary> Initialisation de l'orientation des senseurs XSens. </summary>

    //public void ResetOrientation()
    //{
    //       for (uint i = 0; i < DeviceIds.size(); i++)                     // Debug Marcel
    //       {
    //           XsDevice mtw = new XsDevice(DeviceIds.at(i));
    //           //mtw.resetOrientation(XsResetMethod.XRM_DefaultAlignment);
    //           mtw.resetOrientation(XsResetMethod.XRM_Alignment);
    //       }
    //       Debug.Log("ResetOrientation exécuté");
    //   }

    //   // =================================================================================================================================================================
    //   /// <summary> Lecture des matrices de rotation et des angles Euler disponible, selon les senseurs XSens lus par la fonction Callback, pour le prochain numéro de frame à lire. </summary>

    //   public void GetXSensData(int nFrame, out XsMatrix[] imusMatrix, out XsEuler[] imusEuler, out XsVector[] imusAcc)
    //{
    //	// Si l'initialisation de la lecture des senseurs XSens n'est pas terminé, alors aucune données XSens n'est disponible encore
    //	// Si la fonction Callback n'a pas encore mise à jour les vecteurs circulaire, alors aucune données XSens disponible
    //	// Si le numéro de frame actuellement traité par la fonction Callback est le même que le numéro de frame rendu à lire, alors aucune données XSens disponible

    //	if (!isReadingReady || nListWrite < 0 || nFrame >= CurrentFrameIdx || nFrame >= listFrames[nListWrite])
    //	{
    //		//if (TestXSens.nMsgXInterface < TestXSens.nMsgSize)																	// Debug Marcel (Début)
    //		//{
    //		//	TestXSens.MsgXInterface[TestXSens.nMsgXInterface] = string.Format("GetXSensData1: {0}, {1}, {2}, {3}", initReadXSensDataDone, nListWrite, CurrentFrameIdx, nFrame);
    //		//	TestXSens.nMsgXInterface++;
    //		//}																														// Debug Marcel (Fin)

    //		imusMatrix = null;
    //		imusEuler = null;
    //           imusAcc = null;
    //           return;
    //	}

    //	// Lecture de toutes les données XSens disponible pour le numéro de frame rendu à lire
    //	// Utilisation d'une double boucle, pour prévoir les cas où les pointeurs sont rendu à la fin des vecteurs circulaires, car habituellement seulement une boucle est utilisé
    //	// Si plusieurs données XSens sont disponible pour un même senseur pour le numéro de frame à lire, alors seulement la dernière données XSens est lue, les données précédentes sont effacée
    //	// Si aucune donnée XSens est disponible pour un senseur, pour le numéro de frame à lire, alors on retourne une matrice contenant des zéros

    //	int nForLoop;
    //	imusMatrix = new XsMatrix[NbImusToConnect];
    //	imusEuler = new XsEuler[NbImusToConnect];
    //       imusAcc = new XsVector[NbImusToConnect];
    //       int[] idIMUs = new int[NbImusToConnect];
    //	for (int i = 0; i < NbImusToConnect; i++)
    //	{
    //		imusMatrix[i] = zerosMatrix;
    //		imusEuler[i] = new XsEuler();
    //           imusAcc[i] = zerosAcc;
    //           idIMUs[i] = 0;
    //	}
    //	int[] nForFirst = new int[2] { nListRead, 0 };
    //	int[] nForLast = new int[2] { nListWrite, 0 };
    //	if (nListRead < nListWrite)
    //		nForLoop = 1;
    //	else
    //	{
    //		nForLoop = 2;
    //		nForLast[0] = nListSize;
    //		nForLast[1] = nListWrite;
    //	}
    //	for (int i = 0; i < nForLoop; i++)
    //	{
    //		for (int j = nForFirst[i]; j < nForLast[i]; j++)
    //		{
    //			if (listFrames[j] == nFrame)
    //			{
    //				imusMatrix[listIdIMU[j]] = listMatrix[j];
    //				imusEuler[listIdIMU[j]] = listEuler[j];
    //                   imusAcc[listIdIMU[j]] = listAcc[j];
    //                   idIMUs[listIdIMU[j]] = 1;
    //				if (j + 1 < nListSize)
    //					nListRead = j + 1;
    //				else
    //					nListRead = 0;
    //			}
    //		}
    //	}

    //	// Calculé le nombre de senseurs XSens qui a été lu à chaque frame

    //	if (nFrame < nNumByFrameSize)
    //		numIMUsByFrame[nFrame] = idIMUs.Sum();

    //	//if (TestXSens.nMsgXInterface < TestXSens.nMsgSize)                                                               // Debug Marcel (Début)
    //	//{
    //	//	TestXSens.MsgXInterface[TestXSens.nMsgXInterface] = string.Format(string.Format("GetXSensData2: {0}, {1}, {2}, {3}",
    //	//																				nFrame, numIMUsByFrame[nFrame], _numberIMUtoConnect, XSensModule.readXSensDataON));
    //	//	TestXSens.nMsgXInterface++;
    //	//}                                                                                                                   // Debug Marcel (Fin)

    //	//if (TestXSens.nMsgXInterface < TestXSens.nMsgSize)                                                              // Debug Marcel (Début)
    //	//{
    //	//	TestXSens.MsgXInterface[TestXSens.nMsgXInterface] = string.Format("GetXSensData3: {0}, ", nFrame);
    //	//	for (int i = 0; i < _numberIMUtoConnect; i++)
    //	//		TestXSens.MsgXInterface[TestXSens.nMsgXInterface] += string.Format("{0}, ", idIMUs[i]);
    //	//	TestXSens.nMsgXInterface++;
    //	//}                                                                                                                   // Debug Marcel (Fin)
    //}

    //// =================================================================================================================================================================
    ///// <summary> Quand on quitte ou redémarre le logiciel, alors on désactive l'accès aux senseurs XSens et on initialise certains paramètres. </summary>

    public void OnDestroy()
    {
        //	// Inutile, mais c'est pour s'en rappeler si on déplacer la fermeture
        //	IsReady = false;
        //	IsConnected = false;

        //	if (_measuringDevice != null)
        //	{
        //		if (_measuringDevice.isRecording ())
        //			_measuringDevice.stopRecording ();
        //		_measuringDevice.gotoConfig ();
        //		_measuringDevice.disableRadio();
        //		_measuringDevice.clearCallbackHandlers ();
        //	}

        //	_measuringMts.Clear();

        if (_xda != null)
        {
            _xda.Dispose();
            _xda = null;
        }

        //	//foreach (IntPtr handle in handlesDLL)
        //	//{
        //	//	DllManagement.FreeLib(handle);
        //	//}
    }

    // =================================================================================================================================================================
    /// <summary> Recherche une station XSens connectée et on l'initialise, si on trouve une station. </summary>

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
        if (SelectedScanStation != null)
        {
            XsDevice device = _xda.getDevice(SelectedScanStation.DeviceId);
            if (device.isRadioEnabled())
                device.disableRadio();

            // Test all channel until one works
            for (int i = 12; i < 26; ++i)
                if (device.enableRadio(i))
                    return true;
        }
        return false; // If we get here, something went wrong
    }

    public void SetupDevices()
    {
        if (AllScannedStations.Count == 0)
        {
            Debug.Log("No station found, cannot setup");
            return;
        }

        // Setup the measuring device
        MeasuringDevice = _xda.getDevice(SelectedScanStation.DeviceId);
        MeasuringDevice.setUpdateRate(UpdateRate);

        // Setup each sensor
        SensorIds = new uint[NbImusToConnect];
        for (uint i = 0; i < DeviceIds.size(); i++)
        {
            XsDevice mtw = new XsDevice(DeviceIds.at(i));
            MyMtCallback callback = new MyMtCallback();
            SensorIds[i] = mtw.deviceId().toInt();

            // Connect signals
            callback.DataAvailable += new EventHandler<DataAvailableArgs>(_callbackHandler_DataAvailable);

            mtw.addCallbackHandler(callback);
            DevicesCallbackDict.Add(mtw, callback);
        }

        Debug.Log(string.Format("{0} were successfully setup", DeviceIds.size()));
        IsConnected = true;
    }


	// =================================================================================================================================================================
	/// <summary> Fonction "callback" utilisé pour lire les données des senseurs XSens à la fréquence d'échantillonnage spécifiée. </summary>

	void _callbackHandler_DataAvailable(object sender, DataAvailableArgs e)
	{
        // Initialisation (lecture des frames d'initialisation) pas faite encore ou lecture des données XSens désactivée
        // Une phase d'initialisation a été ajouté, car il a été observé que le ou les premiers frames sont souvent exécuté avec peu de lecture de senseurs XSens, surtout en mode "Build"
        // Hypothèse: Certaines parties du logiciel, comme le pilote XSens, doivent être chargé en mémoire au début, donc crée un ralentissement de la lecture des senseurs XSens
        // NbFramesToDropAtStart;


        // Start time counter
        if (!IsTrialTimerStarted)
		{
            IsTrialTimerStarted = true;
			TimeSinceTrialStarted = DateTime.Now;
		}

		// Calcul de la durée d'un frame, mise à jour du compteur de frame en conséquence

		double elapsedTimeInMs = DateTime.Now.Subtract(TimeSinceTrialStarted).TotalMilliseconds;
		if (elapsedTimeInMs >= (CurrentFrameIdx + 1) * processPeriodInMs)
		{
			if (CurrentFrameIdx < nNumByFrameSize)
			{
				numMsByFrame[CurrentFrameIdx] = elapsedTimeInMs - beginTimeInMs;
				beginTimeInMs = elapsedTimeInMs;
			}
			CurrentFrameIdx++;
			if (CurrentFrameIdx >= NbFramesToDropAtStart)
			{
				beginTimeInMs = 0;
                IsTrialTimerStarted = false;  // Stop the timer
				nListWrite = -1;
				CurrentFrameIdx = 0;
			}
		}

		// Identifier le senseurs XSens qui vient d'être lue, si c'est un senseur invalide ou inconnu, alors on conserve aucune données dans les vecteurs circulaires

		int nIMU = 0;
		while (nIMU < NbImusToConnect && e.Device.deviceId().toInt() != SensorIds[nIMU])
			nIMU++;
		if (nIMU >= NbImusToConnect)
		{
			//if (TestXSens.nMsgXInterface < TestXSens.nMsgSize - 1)												// Debug Marcel (Début)
			//{
			//	TestXSens.MsgXInterface[TestXSens.nMsgXInterface] = string.Format("Callback3: {0}, {1}, {2}, {3}, {4}", initReadXSensDataDone, nListWrite, CurrentFrameIdx, nIMU, _numberIMUtoConnect);
			//	TestXSens.nMsgXInterface++;
			//	TestXSens.MsgXInterface[TestXSens.nMsgXInterface] = string.Format("Callback4: {0}, ", e.Device.deviceId().toInt());
			//	for (int i = 0; i < _numberIMUtoConnect; i++)
			//		TestXSens.MsgXInterface[TestXSens.nMsgXInterface] += string.Format("{0}, ", imuDeviceId[i]);
			//	TestXSens.nMsgXInterface ++;
			//}																										// Debug Marcel (Fin)

			return;
		}

		// Lire la matrice de rotation et les angles Euler, et conserver dans les vecteurs circulaires, ainsi que le numéro du frame actuel et l'identificateur du senseur lu

		nListWrite++;
		if (nListWrite >= nListSize)
			nListWrite = 0;

        //if (TestXSens.nMsgXInterface < TestXSens.nMsgSize - 8)												        // Debug Marcel (Début)
        //{
        //    TestXSens.MsgXInterface[TestXSens.nMsgXInterface] = string.Format(
        //        "Contains values: Orientation = {0}, FreeAcceleration = {1}, CalibratedData = {2}, CalibratedAcceleration = {3}, CalibratedGyroscopeData = {4}, CalibratedMagneticField = {5}, Velocity = {6}",
        //        e.Packet.containsOrientation(), e.Packet.containsFreeAcceleration(), e.Packet.containsCalibratedData(), e.Packet.containsCalibratedAcceleration(),
        //        e.Packet.containsCalibratedGyroscopeData(), e.Packet.containsCalibratedMagneticField(), e.Packet.containsVelocity());
        //    TestXSens.nMsgXInterface++;

        //    TestXSens.MsgXInterface[TestXSens.nMsgXInterface] = string.Format(
        //        "Size values: Orientation (Rows, Cols) = {0}, {1}, FreeAcceleration = {2}, CalibratedData.m_acc = {3}, CalibratedAcceleration = {4}, CalibratedGyroscopeData = {5}, CalibratedMagneticField = {6}, Velocity = {7}",
        //        e.Packet.orientationMatrix().rows(), e.Packet.orientationMatrix().cols(), e.Packet.freeAcceleration().size(), e.Packet.calibratedData().m_acc.size(),
        //        e.Packet.calibratedAcceleration().size(), e.Packet.calibratedGyroscopeData().size(), e.Packet.calibratedMagneticField().size(), e.Packet.velocity().size());
        //    TestXSens.nMsgXInterface++;

        //    TestXSens.MsgXInterface[TestXSens.nMsgXInterface] = string.Format("FreeAcceleration.value = {0}, {1}, {2}",
        //        e.Packet.freeAcceleration().value(0), e.Packet.freeAcceleration().value(1), e.Packet.freeAcceleration().value(2));
        //    TestXSens.nMsgXInterface++;

        //    TestXSens.MsgXInterface[TestXSens.nMsgXInterface] = string.Format("CalibrateData.m_acc.value = {0}, {1}, {2}",
        //        e.Packet.calibratedData().m_acc.value(0), e.Packet.calibratedData().m_acc.value(1), e.Packet.calibratedData().m_acc.value(2));
        //    TestXSens.nMsgXInterface++;

        //    TestXSens.MsgXInterface[TestXSens.nMsgXInterface] = string.Format("CalibratedAcceleration.value = {0}, {1}, {2}",
        //        e.Packet.calibratedAcceleration().value(0), e.Packet.calibratedAcceleration().value(1), e.Packet.calibratedAcceleration().value(2));
        //    TestXSens.nMsgXInterface++;
        //}                                                                                                       // Debug Marcel (Fin)

        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgXInterfaceFreeAcc[nIMU] < TestXSens.nMsgSize)
        //{
        //    TestXSens.MsgXInterfaceFreeAcc[TestXSens.nMsgXInterfaceFreeAcc[nIMU], nIMU] = string.Format(CultureInfo.InvariantCulture, "{0}, ", CurrentFrameIdx);
        //    for (uint i = 0; i < e.Packet.freeAcceleration().size(); i++)
        //        TestXSens.MsgXInterfaceFreeAcc[TestXSens.nMsgXInterfaceFreeAcc[nIMU], nIMU] += string.Format(CultureInfo.InvariantCulture, "{0}, ", e.Packet.freeAcceleration().value(i));
        //    TestXSens.nMsgXInterfaceFreeAcc[nIMU]++;
        //}
        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgXInterfaceCalAcc[nIMU] < TestXSens.nMsgSize)
        //{
        //    TestXSens.MsgXInterfaceCalAcc[TestXSens.nMsgXInterfaceCalAcc[nIMU], nIMU] = string.Format(CultureInfo.InvariantCulture, "{0}, ", CurrentFrameIdx);
        //    for (uint i = 0; i < e.Packet.calibratedData().m_acc.size(); i++)
        //        TestXSens.MsgXInterfaceCalAcc[TestXSens.nMsgXInterfaceCalAcc[nIMU], nIMU] += string.Format(CultureInfo.InvariantCulture, "{0}, ", e.Packet.calibratedData().m_acc.value(i));
        //    TestXSens.nMsgXInterfaceCalAcc[nIMU]++;
        //}
        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgXInterfaceMagField[nIMU] < TestXSens.nMsgSize)
        //{
        //    TestXSens.MsgXInterfaceMagField[TestXSens.nMsgXInterfaceMagField[nIMU], nIMU] = string.Format(CultureInfo.InvariantCulture, "{0}, ", CurrentFrameIdx);
        //    for (uint i = 0; i < e.Packet.calibratedMagneticField().size(); i++)
        //        TestXSens.MsgXInterfaceMagField[TestXSens.nMsgXInterfaceMagField[nIMU], nIMU] += string.Format(CultureInfo.InvariantCulture, "{0}, ", e.Packet.calibratedMagneticField().value(i));
        //    TestXSens.nMsgXInterfaceMagField[nIMU]++;
        //}

        listFrames[nListWrite] = CurrentFrameIdx;
		listIdIMU[nListWrite] = nIMU;
		listMatrix[nListWrite] = e.Packet.orientationMatrix();
        listEuler[nListWrite] = e.Packet.orientationEuler();
        listAcc[nListWrite] = e.Packet.calibratedAcceleration();

        //if (TestXSens.nMsgXInterface < TestXSens.nMsgSize)                                                      // Debug Marcel (Début)
        //{
        //	TestXSens.MsgXInterface[TestXSens.nMsgXInterface] = string.Format("Callback5: {0}; {1}; {2}; ", nListWrite, CurrentFrameIdx, nIMU);
        //	for (uint nRow = 0; nRow < 3; nRow++)
        //		for (uint nCol = 0; nCol < 3; nCol++)
        //			TestXSens.MsgXInterface[TestXSens.nMsgXInterface] += string.Format("{0}; ", listMatrix[nListWrite].value(nRow, nCol));
        //	TestXSens.nMsgXInterface++;
        //}                                                                                                           // Debug Marcel (Fin)


        // Calculé le nombre de fois que la fonction Callback a été appellé à chaque frame

        if (CurrentFrameIdx < nNumByFrameSize)
			numCallbackByFrame[CurrentFrameIdx]++;
	}
}
﻿