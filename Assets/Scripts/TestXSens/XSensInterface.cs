using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Xsens;
using XDA;

// =================================================================================================================================================================
/// <summary> Script qui fait l'interface avec les senseurs XSens. </summary>

public class XSensInterface : MonoBehaviour
{
	IntPtr[] handlesDLL = new IntPtr[3];
	public static XSensInterface Instance;

	private MyXda _xda;
	private XsDevice _measuringDevice = null;
	private Dictionary<XsDevice, MyMtCallback> _measuringMts = new Dictionary<XsDevice, MyMtCallback>();

	private List<MasterInfo> _allScannedStations = new List<MasterInfo>();
	private MasterInfo _selectedScanStation;

	public GameObject _PanelConnexion;
	public static int _numberIMUtoConnect = 1;		// Par défaut = 1 senseur, mais le nombre exact doit être initialiser dans un des scripts principaux du projet
	public static bool _canUseTheSystem = false;	// True si la connexion est active et correcte
	private bool _hasPrepared = false;

	public static bool readXSensDataON;             // Activation/Désactivation de la lecture des senseurs XSens et conservation des données dans les vecteurs circulaires
	public static bool initReadXSensDataDone = false; // Indique si la fonction Callback a été appellé au moins une fois, donc qu'elle est prête à recevoir des données XSens
	bool initFramesDone;                            // Indique si le nombre de frames d'initialisation a été exécutés
	int nInitFramesToDo = 3;                        // Nombre de frames d'initialisation à faire avant le début réel de la lecture des senseurs XSens
	DateTime timeBegin;								// Temps du début de la lecture des données XSens, utilisé pour mesurer la durée des frames
	bool initTimeBeginDone = false;                 // Indique si la variable timeBegon a été initialisé ou non
	double beginTimeInMs;							// Contient le temps du début du frame actuel, en millisecondes
	public static int processFreq = 100;            // Fréquence de traitement des données XSens par BioRBD (filtre de Kalman)
    //public static int processFreq = 50;             // Fréquence de traitement des données XSens par BioRBD (filtre de Kalman)
    public static int updateRateXSens = 120;        // Fréquence de lecture/transmission des données XSens venant de la station XSens (Update rate)
    //public static int updateRateXSens = 100;        // Fréquence de lecture/transmission des données XSens venant de la station XSens (Update rate)
    float processPeriodInMs;						// Durée de frame désirée, en millisecondes
	public static int nProcessFrame = 0;            // Numéro du frame actuel, utilisé lors de la lecture des senseurs XSens pour séquencer les données lues

	XsDevicePtrArray deviceIds;
	uint[] imuDeviceId;                             // Contient les identificateurs de chacun des senseurs XSens

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

    // =================================================================================================================================================================
    /// <summary> Initialisation du script. </summary>

    void Start()
	{
		//handlesDLL[0] = DllManagement.LoadLib(@"Assets\Avatar\XSens\bin\xstypes64.dll");
		//handlesDLL[1] = DllManagement.LoadLib(@"Assets\Avatar\XSens\bin\xsensdeviceapi64.dll");
		//handlesDLL[2] = DllManagement.LoadLib(@"Assets\Avatar\XSens\bin\xsensdeviceapi_csharp64.dll");

		Instance = this;
		processPeriodInMs = 1000 / processFreq;
		zerosMatrix.setZero();
        zerosAcc.setZero();

        // S'assurer que le séparateur décimal est bien un point et non une virgule

        System.Globalization.NumberFormatInfo nfi = new System.Globalization.NumberFormatInfo();
		nfi.NumberDecimalSeparator = ".";
		System.Globalization.CultureInfo ci = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
		ci.NumberFormat = nfi;
		System.Threading.Thread.CurrentThread.CurrentCulture = ci;

		_xda = new MyXda();
	}

	// =================================================================================================================================================================
	/// <summary> Initialisation de l'accès aux senseurs XSens. </summary>

	public void PrepareSystem()
	{
		// Initialisation des paramètres relié aux vecteurs circulaires et quelques autres paramètres

		readXSensDataON = false;
		initReadXSensDataDone = false;
		initFramesDone = false;
		initTimeBeginDone = false;
		beginTimeInMs = 0;
		nProcessFrame = 0;

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

		if (_hasPrepared) // S'assurer de ne le faire qu'une fois
			return;

		// Trouver le dock sur lequel se brancher
		if (!ScanSystem())
		{
			Debug.Log("Aucune station n'a été détecté.");
			return;
		}

		// Activer le signal radio
		if (!EnableSystem())
		{
			Debug.Log("Une erreur est apparue durant l'activation de la station.");
			return;
		}

		// Démarrer la connexion avec les centrales

		_hasPrepared = true;

		imuDeviceId = new uint[_numberIMUtoConnect];
		for (int i = 0; i < _numberIMUtoConnect; i++)
			imuDeviceId[i] = 0;

		StartCoroutine(MeasureSystem());
	}

	// =================================================================================================================================================================
	/// <summary> Initialisation de l'orientation des senseurs XSens. </summary>

	public void ResetOrientation()
	{
        for (uint i = 0; i < deviceIds.size(); i++)                     // Debug Marcel
        {
            XsDevice mtw = new XsDevice(deviceIds.at(i));
            //mtw.resetOrientation(XsResetMethod.XRM_DefaultAlignment);
            mtw.resetOrientation(XsResetMethod.XRM_Alignment);
        }
        Debug.Log("ResetOrientation exécuté");
    }

    // =================================================================================================================================================================
    /// <summary> Lecture des matrices de rotation et des angles Euler disponible, selon les senseurs XSens lus par la fonction Callback, pour le prochain numéro de frame à lire. </summary>

    public void GetXSensData(int nFrame, out XsMatrix[] imusMatrix, out XsEuler[] imusEuler, out XsVector[] imusAcc)
	{
		// Si l'initialisation de la lecture des senseurs XSens n'est pas terminé, alors aucune données XSens n'est disponible encore
		// Si la fonction Callback n'a pas encore mise à jour les vecteurs circulaire, alors aucune données XSens disponible
		// Si le numéro de frame actuellement traité par la fonction Callback est le même que le numéro de frame rendu à lire, alors aucune données XSens disponible

		if (!initReadXSensDataDone || nListWrite < 0 || nFrame >= nProcessFrame || nFrame >= listFrames[nListWrite])
		{
			//if (TestXSens.nMsgXInterface < TestXSens.nMsgSize)																	// Debug Marcel (Début)
			//{
			//	TestXSens.MsgXInterface[TestXSens.nMsgXInterface] = string.Format("GetXSensData1: {0}, {1}, {2}, {3}", initReadXSensDataDone, nListWrite, nProcessFrame, nFrame);
			//	TestXSens.nMsgXInterface++;
			//}																														// Debug Marcel (Fin)

			imusMatrix = null;
			imusEuler = null;
            imusAcc = null;
            return;
		}

		// Lecture de toutes les données XSens disponible pour le numéro de frame rendu à lire
		// Utilisation d'une double boucle, pour prévoir les cas où les pointeurs sont rendu à la fin des vecteurs circulaires, car habituellement seulement une boucle est utilisé
		// Si plusieurs données XSens sont disponible pour un même senseur pour le numéro de frame à lire, alors seulement la dernière données XSens est lue, les données précédentes sont effacée
		// Si aucune donnée XSens est disponible pour un senseur, pour le numéro de frame à lire, alors on retourne une matrice contenant des zéros

		int nForLoop;
		imusMatrix = new XsMatrix[_numberIMUtoConnect];
		imusEuler = new XsEuler[_numberIMUtoConnect];
        imusAcc = new XsVector[_numberIMUtoConnect];
        int[] idIMUs = new int[_numberIMUtoConnect];
		for (int i = 0; i < _numberIMUtoConnect; i++)
		{
			imusMatrix[i] = zerosMatrix;
			imusEuler[i] = new XsEuler();
            imusAcc[i] = zerosAcc;
            idIMUs[i] = 0;
		}
		int[] nForFirst = new int[2] { nListRead, 0 };
		int[] nForLast = new int[2] { nListWrite, 0 };
		if (nListRead < nListWrite)
			nForLoop = 1;
		else
		{
			nForLoop = 2;
			nForLast[0] = nListSize;
			nForLast[1] = nListWrite;
		}
		for (int i = 0; i < nForLoop; i++)
		{
			for (int j = nForFirst[i]; j < nForLast[i]; j++)
			{
				if (listFrames[j] == nFrame)
				{
					imusMatrix[listIdIMU[j]] = listMatrix[j];
					imusEuler[listIdIMU[j]] = listEuler[j];
                    imusAcc[listIdIMU[j]] = listAcc[j];
                    idIMUs[listIdIMU[j]] = 1;
					if (j + 1 < nListSize)
						nListRead = j + 1;
					else
						nListRead = 0;
				}
			}
		}

		// Calculé le nombre de senseurs XSens qui a été lu à chaque frame

		if (nFrame < nNumByFrameSize)
			numIMUsByFrame[nFrame] = idIMUs.Sum();

		//if (TestXSens.nMsgXInterface < TestXSens.nMsgSize)                                                               // Debug Marcel (Début)
		//{
		//	TestXSens.MsgXInterface[TestXSens.nMsgXInterface] = string.Format(string.Format("GetXSensData2: {0}, {1}, {2}, {3}",
		//																				nFrame, numIMUsByFrame[nFrame], _numberIMUtoConnect, XSensInterface.readXSensDataON));
		//	TestXSens.nMsgXInterface++;
		//}                                                                                                                   // Debug Marcel (Fin)

		//if (TestXSens.nMsgXInterface < TestXSens.nMsgSize)                                                              // Debug Marcel (Début)
		//{
		//	TestXSens.MsgXInterface[TestXSens.nMsgXInterface] = string.Format("GetXSensData3: {0}, ", nFrame);
		//	for (int i = 0; i < _numberIMUtoConnect; i++)
		//		TestXSens.MsgXInterface[TestXSens.nMsgXInterface] += string.Format("{0}, ", idIMUs[i]);
		//	TestXSens.nMsgXInterface++;
		//}                                                                                                                   // Debug Marcel (Fin)
	}

	// =================================================================================================================================================================
	/// <summary> Quand on quitte ou redémarre le logiciel, alors on désactive l'accès aux senseurs XSens et on initialise certains paramètres. </summary>

	public void OnDestroy()
	{
		// Inutile, mais c'est pour s'en rappeler si on déplacer la fermeture
		_hasPrepared = false;
		_canUseTheSystem = false;

		if (_measuringDevice != null)
		{
			if (_measuringDevice.isRecording ())
				_measuringDevice.stopRecording ();
			_measuringDevice.gotoConfig ();
			_measuringDevice.disableRadio();
			_measuringDevice.clearCallbackHandlers ();
		}

		_measuringMts.Clear();

		if (_xda != null)
		{
			_xda.Dispose ();
			_xda = null;
		}

		//foreach (IntPtr handle in handlesDLL)
		//{
		//	DllManagement.FreeLib(handle);
		//}
	}

	// =================================================================================================================================================================
	/// <summary> Recherche une station XSens connectée et on l'initialise, si on trouve une station. </summary>

	protected bool ScanSystem()
	{
		if (_hasPrepared)
			return true;

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
					_allScannedStations.Add(ai);
					break;
				}
			}

			if (_allScannedStations.Count > 0)
			{ 
				// Sélectionner le premier par défaut (obligatoirement)
				_selectedScanStation = _allScannedStations [0];
				return true;
			}
		}
		return false; // Si on se rend ici, c'est qu'on a trouvé aucun dock
	}

	// =================================================================================================================================================================
	/// <summary> Initialisation de la connection sans fils entre les senseurs et la station XSens. </summary>

	protected bool EnableSystem()
	{
		if (_selectedScanStation != null)
		{
			XsDevice device = _xda.getDevice (_selectedScanStation.DeviceId);
			if (device.isRadioEnabled ())
				device.disableRadio ();

			// Tester tous les channels radio jusqu'à ce qu'un fonctionne
			for (int i = 12; i < 26; ++i)
				if (device.enableRadio (i))
					return true;
		}
		return false; // Si on arrive ici, c'est qu'aucun ne fonctionne
	}

	// =================================================================================================================================================================
	/// <summary>
	/// Configuration et détection du nombre de senseus XSens spécifié, activation de la fonction "callback" utilisé pour faire la lecture des données des senseurs XSens.
	/// </summary>

	IEnumerator MeasureSystem()
	{
		_measuringDevice = _xda.getDevice(_selectedScanStation.DeviceId);
		_measuringDevice.setUpdateRate(updateRateXSens);

		// Faire une boucle jusqu'à ce que tous les senseurs XSens soient connectés

		_PanelConnexion.SetActive(true);
		Text textToUpdate = _PanelConnexion.GetComponentInChildren<Text> ();

		//XsDevicePtrArray deviceIds;
		string stringToUse = textToUpdate.text;
		do
		{
			deviceIds = _measuringDevice.children();
			textToUpdate.text = string.Format(stringToUse, deviceIds.size(), _numberIMUtoConnect);
			yield return 0;
		} while (deviceIds.size() != _numberIMUtoConnect);

		_measuringDevice.setOptions(XsOption.XSO_Orientation | XsOption.XSO_Calibrate, XsOption.XSO_None);
		_measuringDevice.gotoMeasurement();

		_PanelConnexion.SetActive(false);

		// Activer l'accès à chacun des senseurs XSens

		for (uint i = 0; i < deviceIds.size(); i++)
		{
			XsDevice mtw = new XsDevice(deviceIds.at(i));

			MyMtCallback callback = new MyMtCallback();

			imuDeviceId[i] = mtw.deviceId().toInt();

			// connect signals
			callback.DataAvailable += new EventHandler<DataAvailableArgs>(_callbackHandler_DataAvailable);

			mtw.addCallbackHandler(callback);
			_measuringMts.Add(mtw, callback);                   // Pour une raison inconnu de moi, cette instruction doit être là, sinon Unity donne une erreur à l'exécution,
		}                                                       //  pourtant je crois qu'elle est inutile. Marcel 2020-07-15
		Debug.Log(string.Format("Nombre de senseurs connectés: {0}", deviceIds.size()));

		// On est prêt

		_canUseTheSystem = true;
	}

	// =================================================================================================================================================================
	/// <summary> Fonction "callback" utilisé pour lire les données des senseurs XSens à la fréquence d'échantillonnage spécifiée. </summary>

	void _callbackHandler_DataAvailable(object sender, DataAvailableArgs e)
	{
		// Initialisation (lecture des frames d'initialisation) pas faite encore ou lecture des données XSens désactivée
		// Une phase d'initialisation a été ajouté, car il a été observé que le ou les premiers frames sont souvent exécuté avec peu de lecture de senseurs XSens, surtout en mode "Build"
		// Hypothèse: Certaines parties du logiciel, comme le pilote XSens, doivent être chargé en mémoire au début, donc crée un ralentissement de la lecture des senseurs XSens

		if ((!initReadXSensDataDone || !readXSensDataON) && initFramesDone)
		{
			initReadXSensDataDone = true;
			return;
		}

		// Initialisation du temps qui indique le début de la lecture des données XSens

		if ((initReadXSensDataDone || !initFramesDone) && !initTimeBeginDone)
		{
			//if (TestXSens.nMsgXInterface < TestXSens.nMsgSize)													// Debug Marcel (Début)
			//{
			//	TestXSens.MsgXInterface[TestXSens.nMsgXInterface] = string.Format("Callback1: {0}, {1}, {2}, {3}, {4}",
			//																		initReadXSensDataDone, initFramesDone, initTimeBeginDone, nListWrite, nProcessFrame);
			//	TestXSens.nMsgXInterface++;
			//}																										// Debug Marcel (Fin)

			initTimeBeginDone = true;
			timeBegin = DateTime.Now;
		}

		// Calcul de la durée d'un frame, mise à jour du compteur de frame en conséquence

		double elapsedTimeInMs = DateTime.Now.Subtract(timeBegin).TotalMilliseconds;
		if (elapsedTimeInMs >= (nProcessFrame + 1) * processPeriodInMs)
		{
			if (nProcessFrame < nNumByFrameSize)
			{
				numMsByFrame[nProcessFrame] = elapsedTimeInMs - beginTimeInMs;
				beginTimeInMs = elapsedTimeInMs;
			}
			nProcessFrame++;
			if (!initFramesDone && nProcessFrame >= nInitFramesToDo)
			{
				//if (TestXSens.nMsgXInterface < TestXSens.nMsgSize)												// Debug Marcel (Début)
				//{
				//	TestXSens.MsgXInterface[TestXSens.nMsgXInterface] = string.Format("Callback2: {0}, {1}, {2}", initReadXSensDataDone, nListWrite, nProcessFrame);
				//	TestXSens.nMsgXInterface++;
				//}																									// Debug Marcel (Fin)

				beginTimeInMs = 0;
				initTimeBeginDone = false;
				nListWrite = -1;
				nProcessFrame = 0;
				initFramesDone = true;
			}
		}

		// Identifier le senseurs XSens qui vient d'être lue, si c'est un senseur invalide ou inconnu, alors on conserve aucune données dans les vecteurs circulaires

		int nIMU = 0;
		while (nIMU < _numberIMUtoConnect && e.Device.deviceId().toInt() != imuDeviceId[nIMU])
			nIMU++;
		if (nIMU >= _numberIMUtoConnect)
		{
			//if (TestXSens.nMsgXInterface < TestXSens.nMsgSize - 1)												// Debug Marcel (Début)
			//{
			//	TestXSens.MsgXInterface[TestXSens.nMsgXInterface] = string.Format("Callback3: {0}, {1}, {2}, {3}, {4}", initReadXSensDataDone, nListWrite, nProcessFrame, nIMU, _numberIMUtoConnect);
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
        //    TestXSens.MsgXInterfaceFreeAcc[TestXSens.nMsgXInterfaceFreeAcc[nIMU], nIMU] = string.Format(CultureInfo.InvariantCulture, "{0}, ", nProcessFrame);
        //    for (uint i = 0; i < e.Packet.freeAcceleration().size(); i++)
        //        TestXSens.MsgXInterfaceFreeAcc[TestXSens.nMsgXInterfaceFreeAcc[nIMU], nIMU] += string.Format(CultureInfo.InvariantCulture, "{0}, ", e.Packet.freeAcceleration().value(i));
        //    TestXSens.nMsgXInterfaceFreeAcc[nIMU]++;
        //}
        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgXInterfaceCalAcc[nIMU] < TestXSens.nMsgSize)
        //{
        //    TestXSens.MsgXInterfaceCalAcc[TestXSens.nMsgXInterfaceCalAcc[nIMU], nIMU] = string.Format(CultureInfo.InvariantCulture, "{0}, ", nProcessFrame);
        //    for (uint i = 0; i < e.Packet.calibratedData().m_acc.size(); i++)
        //        TestXSens.MsgXInterfaceCalAcc[TestXSens.nMsgXInterfaceCalAcc[nIMU], nIMU] += string.Format(CultureInfo.InvariantCulture, "{0}, ", e.Packet.calibratedData().m_acc.value(i));
        //    TestXSens.nMsgXInterfaceCalAcc[nIMU]++;
        //}
        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgXInterfaceMagField[nIMU] < TestXSens.nMsgSize)
        //{
        //    TestXSens.MsgXInterfaceMagField[TestXSens.nMsgXInterfaceMagField[nIMU], nIMU] = string.Format(CultureInfo.InvariantCulture, "{0}, ", nProcessFrame);
        //    for (uint i = 0; i < e.Packet.calibratedMagneticField().size(); i++)
        //        TestXSens.MsgXInterfaceMagField[TestXSens.nMsgXInterfaceMagField[nIMU], nIMU] += string.Format(CultureInfo.InvariantCulture, "{0}, ", e.Packet.calibratedMagneticField().value(i));
        //    TestXSens.nMsgXInterfaceMagField[nIMU]++;
        //}

        listFrames[nListWrite] = nProcessFrame;
		listIdIMU[nListWrite] = nIMU;
		listMatrix[nListWrite] = e.Packet.orientationMatrix();
        listEuler[nListWrite] = e.Packet.orientationEuler();
        listAcc[nListWrite] = e.Packet.calibratedAcceleration();

        //if (TestXSens.nMsgXInterface < TestXSens.nMsgSize)                                                      // Debug Marcel (Début)
        //{
        //	TestXSens.MsgXInterface[TestXSens.nMsgXInterface] = string.Format("Callback5: {0}; {1}; {2}; ", nListWrite, nProcessFrame, nIMU);
        //	for (uint nRow = 0; nRow < 3; nRow++)
        //		for (uint nCol = 0; nCol < 3; nCol++)
        //			TestXSens.MsgXInterface[TestXSens.nMsgXInterface] += string.Format("{0}; ", listMatrix[nListWrite].value(nRow, nCol));
        //	TestXSens.nMsgXInterface++;
        //}                                                                                                           // Debug Marcel (Fin)


        // Calculé le nombre de fois que la fonction Callback a été appellé à chaque frame

        if (nProcessFrame < nNumByFrameSize)
			numCallbackByFrame[nProcessFrame]++;
	}
}
﻿