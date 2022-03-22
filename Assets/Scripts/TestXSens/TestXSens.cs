//#define RK8
#define RK4_1
//#define USE_XSENS_DATA
//#define KALMAN
//#define USE_RK8_INPUT_DATA

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using Crosstales.FB;
using XDA;

// =================================================================================================================================================================
/// <summary> Script principal pour le fonctionnement du logiciel en mode TestXSens. </summary>

public class TestXSens : MonoBehaviour
{
    public static TestXSens Instance;

    public S2M_model modelS2M;
    public GameObject buttonTestXSens;
    public GameObject buttonXSensON;
    public Text textTestXSensStatus;
    public GameObject panelTestMessages;

    public AudioSource audioS;

    public Text textTestMessages;
    public static IntPtr modelInt;
    public static int nQ_modelInt = 0;
    public static int nMarkers_modelInt = 0;

    XSensData xSdataCalib;
    List<XsEuler[]> xEulerDataCalib = new List<XsEuler[]>();
    double[,,] eulerDataCalib;
    public XSensData xSdataTrial;
    List<XsEuler[]> xEulerDataTrial = new List<XsEuler[]>();
    public double[,,] eulerDataTrial;

    enum RunStateList { DisplayCalibMsg, PreCalib, Calib, PostCalib, Trial, PostTrial, Idle };
    RunStateList runState = RunStateList.Idle;      // État de fonctionnement du mode TestXSens

    System.Diagnostics.Stopwatch timeStamp = new System.Diagnostics.Stopwatch();

    public static float[] qHumans;
    public static float[] qdotHumans;
    public static float[] qddotHumans;

    // Vecteurs contenant les positions, vitesses et accelerations des articulations

#if RK8
	float[,] qint;
	float[,] qdint;
	float[,] qddint;
#else
    float[] qint0;
    float[] qdint0;
    float[] qddint0;
    float[] qint1;
    float[] qdint1;
    float[] qddint1;
    float[] qint2;
    float[] qdint2;
    float[] qddint2;
#endif

    double[,] qXSens;
    double[] qdotXSens;

    XsMatrix[] imusMatrixPrev;

    //bool initAnimationDone = false;
    bool calibrationDone = false;
    int numberFramesCalib = 50;
    float timeBegin;
    float timeEnd;
    float timeTrialDuration;
    public int nFrame = 0;
    int nFrameXSens = 0;
    //static bool resetOrientationDone = false;

    bool logDataInformation = true;             // Debug Marcel
    int nFramesCalib = 0;
    int nFramesTrial = 0;

    string[,] textDataFiles;
    int nDataFiles;
    int nMaxTextDataFiles = 10000;
    string[] idDataFiles;
    string[] nameDataFiles;
    string[] nameReadDataFiles;
    int[] indexDataFiles;
    int[] nTextDataFiles;
    int nDataFilesTypes = 6;                    // Fichiers de données = Calibration OrientationMatrix, Essai OrientationMatrix, Calibration OrientationEuler, Essai OrientationEuler,
                                                //                       Calibration Accélération & Essai Accélération

#if !USE_XSENS_DATA
    List<XsMatrix[]> imusMatrixCalib;
    List<XsMatrix[]> imusMatrixTrial;
#endif

    public static string[] MsgXInterface;
    public static int nMsgXInterface = 0;
    public static string[,] MsgXInterfaceFreeAcc;
    public static int[] nMsgXInterfaceFreeAcc;
    public static string[,] MsgXInterfaceCalAcc;
    public static int[] nMsgXInterfaceCalAcc;
    public static string[,] MsgXInterfaceMagField;
    public static int[] nMsgXInterfaceMagField;
    public static string[] msgS2M;
    public static int nMsgS2M = 0;
    string[,] msgEuler;
    int nMsgEuler = 0;
    string[,] msgKalmanQ;
    int nMsgKalmanQ = 0;
    string[,] msgRK8Q;
    int nMsgRK8Q = 0;
    public static string[,] msgJCS;
    public static int nMsgJCS = 0;
    public static string[] msgDebug;
    public static int nMsgDebug = 0;

    public static int nMsgSize = 10000;

#if USE_RK8_INPUT_DATA
	double[] dataRK8Q0;
	double[] dataRK8Qdot0;
	double[,] dataRK8Q;
	double[,] dataRK8Qdot;
	double[,] dataRK8Qddot;

	string[,] msgRK8InitQ0;
	int nMsgRK8InitQ0 = 0;
	string[,] msgRK8InputQ;
	int nMsgRK8InputQ = 0;
#endif

    // =================================================================================================================================================================
    /// <summary> Initialisation du script. </summary>

    void Start()
    {
        Instance = this;
        eulerDataCalib = null;
        eulerDataTrial = null;

        XSensInterface._numberIMUtoConnect = S2M_model.nbImuToConnect;

        imusMatrixPrev = new XsMatrix[XSensInterface._numberIMUtoConnect];
        for (int i = 0; i < XSensInterface._numberIMUtoConnect; i++)
            imusMatrixPrev[i] = new XsMatrix(3, 3);

        // Initialisation de la matrice contenant les textes à imprimer dans les fichiers de données, ainsi que les autres variables utilisées pour créer les fichiers de données

        nDataFiles = XSensInterface._numberIMUtoConnect * nDataFilesTypes;
        textDataFiles = new string[nMsgSize, nDataFiles];
        idDataFiles = new string[nDataFilesTypes];
        idDataFiles[0] = "CalibIMUs";
        idDataFiles[1] = "IMUs";
        idDataFiles[2] = "CalibEuler";
        idDataFiles[3] = "Euler";
        idDataFiles[4] = "CalibAcc";
        idDataFiles[5] = "Acc";
        nameDataFiles = new string[nDataFiles];
        nameReadDataFiles = new string[nDataFiles];
        nTextDataFiles = new int[nDataFiles];
        indexDataFiles = new int[nDataFilesTypes];
        for (int i = 0; i < indexDataFiles.Length; i++)
        {
            indexDataFiles[i] = i * XSensInterface._numberIMUtoConnect;
            for (int j = 0; j < XSensInterface._numberIMUtoConnect; j++)
            {
                int jj = indexDataFiles[i] + j;
                nameDataFiles[jj] = string.Format(@"C:\Devel\AcroVR_{0}{1}.txt", idDataFiles[i], j);
                nameReadDataFiles[jj] = string.Format(@"C:\Devel\AcroVRXSensDataFiles\AcroVR_{0}{1}.txt", idDataFiles[i], j);
                nTextDataFiles[jj] = 0;
                for (int k = 0; k < nMsgSize; k++)
                    textDataFiles[k, jj] = "";
            }
        }

        MsgXInterface = new string[nMsgSize];
        nMsgXInterface = 0;
        MsgXInterfaceFreeAcc = new string[nMsgSize, XSensInterface._numberIMUtoConnect];
        nMsgXInterfaceFreeAcc = new int[XSensInterface._numberIMUtoConnect];
        MsgXInterfaceCalAcc = new string[nMsgSize, XSensInterface._numberIMUtoConnect];
        nMsgXInterfaceCalAcc = new int[XSensInterface._numberIMUtoConnect];
        MsgXInterfaceMagField = new string[nMsgSize, XSensInterface._numberIMUtoConnect];
        nMsgXInterfaceMagField = new int[XSensInterface._numberIMUtoConnect];
        for (int i = 0; i < XSensInterface._numberIMUtoConnect; i++)
        {
            nMsgXInterfaceFreeAcc[i] = 0;
            nMsgXInterfaceCalAcc[i] = 0;
            nMsgXInterfaceMagField[i] = 0;
        }

        msgS2M = new string[nMsgSize];
        nMsgS2M = 0;
        msgEuler = new string[nMsgSize, XSensInterface._numberIMUtoConnect];
        nMsgEuler = 0;
        msgKalmanQ = new string[nMsgSize, 3];
        nMsgKalmanQ = 0;
        msgRK8Q = new string[nMsgSize, 2];
        nMsgRK8Q = 0;
        msgJCS = new string[nMsgSize, XSensInterface._numberIMUtoConnect];
        nMsgJCS = 0;
        msgDebug = new string[nMsgSize];
        nMsgDebug = 0;
        for (int i = 0; i < nMsgSize; i++)
        {
            msgS2M[i] = "";
            MsgXInterface[i] = "";
            for (int j = 0; j < XSensInterface._numberIMUtoConnect; j++)
            {
                MsgXInterfaceFreeAcc[i, j] = "";
                MsgXInterfaceCalAcc[i, j] = "";
                MsgXInterfaceMagField[i, j] = "";
                msgEuler[i, j] = "";
                msgJCS[i, j] = "";
            }
            for (int j = 0; j < 3; j++)
                msgKalmanQ[i, j] = "";
            for (int j = 0; j < 2; j++)
                msgRK8Q[i, j] = "";
            msgDebug[i] = "";
        }

#if RK8
		Debug.Log("RK8");
#else
#if RK4_1
        Debug.Log("RK4_1");
#else
        Debug.Log("RK4");
#endif
#endif

#if KALMAN
		Debug.Log("Kalman");
#else
        Debug.Log("TransEuler");
#endif

#if USE_XSENS_DATA
        Debug.Log("XSens");
#else
        Debug.Log("Fichiers");
        imusMatrixCalib = ReadXSensDataFiles("CalibIMUs");
        imusMatrixTrial = ReadXSensDataFiles("IMUs");
#endif

#if USE_RK8_INPUT_DATA
		Debug.Log("Fichiers entrées RK8");

		msgRK8InitQ0 = new string[nMsgSize, 2];
		nMsgRK8InitQ0 = 0;
		msgRK8InputQ = new string[nMsgSize, 3];
		nMsgRK8InputQ = 0;
		for (int i = 0; i < nMsgSize; i++)
		{
			for (int j = 0; j < 2; j++)
				msgRK8InitQ0[i, j] = "";
			for (int j = 0; j < 3; j++)
				msgRK8InputQ[i, j] = "";
		}

		ReadIntegratorInputDataFiles1(out dataRK8Q0, out dataRK8Qdot0);

		for (int i = 0; i < 2; i++)
			msgRK8InitQ0[nMsgRK8InitQ0, i] = string.Format("{0}, ", 0);
		for (int i = 0; i < dataRK8Q0.Length; i++)
		{
			msgRK8InitQ0[nMsgRK8InitQ0, 0] += string.Format("{0}, ", dataRK8Q0[i]);
			msgRK8InitQ0[nMsgRK8InitQ0, 1] += string.Format("{0}, ", dataRK8Qdot0[i]);
		}
		nMsgRK8InitQ0++;

		string fileRK8InitQ0 = @"C:\Devel\AcroVR_RK8InitQ0.txt";
		string fileRK8InitQdot0 = @"C:\Devel\AcroVR_RK8InitQdot0.txt";
		System.IO.File.Delete(fileRK8InitQ0);
		System.IO.File.Delete(fileRK8InitQdot0);
		for (int i = 0; i < nMsgRK8InitQ0; i++)
		{
			System.IO.File.AppendAllText(fileRK8InitQ0, string.Format("{0}{1}", msgRK8InitQ0[i, 0], System.Environment.NewLine));
			System.IO.File.AppendAllText(fileRK8InitQdot0, string.Format("{0}{1}", msgRK8InitQ0[i, 1], System.Environment.NewLine));
		}

		ReadIntegratorInputDataFiles2(out dataRK8Q, out dataRK8Qdot, out dataRK8Qddot);

		for (int i = 0; i < dataRK8Q.GetLength(0); i++)
		{
			for (int j = 0; j < 3; j++)
				msgRK8InputQ[nMsgRK8InputQ, j] = string.Format("{0}, ", i);
			for (int j = 0; j < dataRK8Q.GetLength(1); j++)
			{
				msgRK8InputQ[nMsgRK8InputQ, 0] += string.Format("{0}, ", dataRK8Q[i, j]);
				msgRK8InputQ[nMsgRK8InputQ, 1] += string.Format("{0}, ", dataRK8Qdot[i, j]);
				msgRK8InputQ[nMsgRK8InputQ, 2] += string.Format("{0}, ", dataRK8Qddot[i, j]);
			}
			nMsgRK8InputQ++;
		}

		string fileRK8InputQ = @"C:\Devel\AcroVR_RK8InputQ.txt";
		string fileRK8InputQdot = @"C:\Devel\AcroVR_RK8InputQdot.txt";
		string fileRK8InputQddot = @"C:\Devel\AcroVR_RK8InputQddot.txt";
		System.IO.File.Delete(fileRK8InputQ);
		System.IO.File.Delete(fileRK8InputQdot);
		System.IO.File.Delete(fileRK8InputQddot);
		for (int i = 0; i < nMsgRK8InputQ; i++)
		{
			System.IO.File.AppendAllText(fileRK8InputQ, string.Format("{0}{1}", msgRK8InputQ[i, 0], System.Environment.NewLine));
			System.IO.File.AppendAllText(fileRK8InputQdot, string.Format("{0}{1}", msgRK8InputQ[i, 1], System.Environment.NewLine));
			System.IO.File.AppendAllText(fileRK8InputQddot, string.Format("{0}{1}", msgRK8InputQ[i, 2], System.Environment.NewLine));
		}

#endif
    }

    // =================================================================================================================================================================
    /// <summary> Exécution de la fonction à chaque frame. </summary>

    void Update()
    {
        // Vérifier que la station XSens Awinda est prête et que tous les senseurs XSens sont prêt à être utiliser

        if (MainParameters.Instance.softwareRunMode != MainParameters.softwareRunModeType.TestXSensFromDataFiles && !XSensInterface._canUseTheSystem)
            return;

        // Afficher le message d'attente pour la calibration

        if (runState <= RunStateList.DisplayCalibMsg)
        {
            timeBegin = Time.time;
            if (MainParameters.Instance.softwareRunMode == MainParameters.softwareRunModeType.TestXSens)
                timeEnd = timeBegin + 5;
            else
            {
                XSensInterface.initReadXSensDataDone = true;
                timeEnd = timeBegin;
            }
            panelTestMessages.SetActive(true);
            textTestMessages.text = string.Format("Calibration dans {0:F0}s", timeEnd - timeBegin);
            runState = RunStateList.PreCalib;
        }

        // Attendre le début de la calibration

        else if (runState == RunStateList.PreCalib)
        {
            float timeElapsed = timeEnd - Time.time;
            textTestMessages.text = string.Format("Calibration dans {0:F0}s", timeElapsed);
            if (timeElapsed < 0)
            {
                timeBegin = Time.time;
                if (XSensInterface.initReadXSensDataDone)
                {
                    //if (MainParameters.Instance.softwareRunMode == MainParameters.softwareRunModeType.TestXSens)
                    //	XSensInterface.Instance.ResetOrientation();
                    timeEnd = timeBegin + 2;
                    textTestMessages.text = "Calibration ...";
                    calibrationDone = false;
                    nFrame = 0;
                    timeStamp.Reset();
                    timeStamp.Start();
                    XSensInterface.readXSensDataON = true;
                    runState = RunStateList.Calib;
                }
                else
                {
                    timeEnd = timeBegin + 2;
                    panelTestMessages.SetActive(true);
                    textTestMessages.text = "Erreur #1 avec la fonction callback de XSens";
                    runState = RunStateList.PostTrial;
                }
            }
        }

        // Calibration

        else if (runState == RunStateList.Calib)
        {
            //if (nMess < nMessSize)                                      // Debug Marcel
            //{
            //	mess[nMess] = string.Format("{0}: {1}, {2}, {3}, {4}", runState, XSensInterface.initReadXSensDataDone, XSensInterface.readXSensDataON, XSensInterface.nProcessFrame, nFrame);
            //	nMess++;
            //}
            if (timeEnd - Time.time < 0 || calibrationDone)
            {
                //timeBegin = Time.time;                                                                                                  // Debug Marcel (Début)
                //timeEnd = timeBegin + 2;
                //textTestMessages.text = "Essai terminé";
                //runState = RunStateList.PostTrial;

                timeBegin = Time.time;
                timeEnd = timeBegin + 2;
                textTestMessages.text = string.Format("Calib terminé; Début essai dans {0:F0}s", timeEnd - timeBegin);
                audioS.Play(0);
                nFrame = 0;
                if (MainParameters.Instance.softwareRunMode == MainParameters.softwareRunModeType.TestXSens)
                    XSensInterface.Instance.PrepareSystem();
                else
                    XSensInterface.initReadXSensDataDone = true;
                runState = RunStateList.PostCalib;                                                                                  // Debug Marcel (Fin)
            }
            else
                Calibration();
        }

        // Calibration terminé, attendre pour le début de l'essai

        else if (runState == RunStateList.PostCalib)
        {
            float timeElapsed = timeEnd - Time.time;
            textTestMessages.text = string.Format("Calib terminé; Début essai dans {0:F0}s", timeElapsed);
            if (timeElapsed < 0)
            {
                audioS.Play(0);
                timeBegin = Time.time;
                if (XSensInterface.initReadXSensDataDone)
                {
                    if (MainParameters.Instance.softwareRunMode == MainParameters.softwareRunModeType.TestXSens)
                        timeEnd = timeBegin + 1;
                    else
                        timeEnd = timeBegin + 100;
                    //initAnimationDone = false;
                    nFrame = 0;
                    nFrameXSens = 0;
                    AnimationF.Instance.EnableDisableAnimationOutline(true);
                    timeStamp.Reset();
                    timeStamp.Start();
                    panelTestMessages.SetActive(false);
                    XSensInterface.readXSensDataON = true;
                    runState = RunStateList.Trial;
                }
                else
                {
                    timeEnd = timeBegin + 2;
                    panelTestMessages.SetActive(true);
                    textTestMessages.text = "Erreur #2 avec la fonction callback de XSens";
                    runState = RunStateList.PostTrial;
                }
            }
        }

        // Message de calibration supprimer
        // Afficher la silhouette selon les déplacements des senseurs XSens
        // Afficher un message quand l'essai est terminé

        else if (runState == RunStateList.Trial)
        {
            //if (nMess < nMessSize)                                      // Debug Marcel
            //{
            //	mess[nMess] = string.Format("{0}: {1}, {2}, {3}, {4}", runState, XSensInterface.initReadXSensDataDone, XSensInterface.readXSensDataON, XSensInterface.nProcessFrame, nFrame);
            //	nMess++;
            //}
            float timeNow = Time.time;
            if (timeEnd - timeNow < 0)
            {
                if (XSensInterface.readXSensDataON)
                {
                    audioS.Play(0);
                    XSensInterface.readXSensDataON = false;
                    timeTrialDuration = timeNow - timeBegin;
                }
                else
                {
                    int i = 0;
                    while (i < 100 && ReadXSensData())                       // S'assurer d'avoir bien lu tous les frames disponibles, avant de terminer l'essai
                        i++;
                    nFramesTrial = nFrame + 1;
                    TerminateTrial();
                    timeBegin = Time.time;
                    timeEnd = timeBegin + 2;
                    panelTestMessages.SetActive(true);
                    if (i < 100)
                        textTestMessages.text = "Essai terminé";
                    else
                        textTestMessages.text = "Essai terminé (Beaucoup de frames en retard)";
                    runState = RunStateList.PostTrial;
                    Debug.Log(textTestMessages.text);
                }
            }
            else
                ReadXSensData();
        }

        // Fin affichage de la silhouette, attendre avant de supprimer le message de fin d'essai

        else if (runState == RunStateList.PostTrial && timeEnd - Time.time < 0)
        {
            AnimationF.Instance.textChrono.text = "";
            panelTestMessages.SetActive(false);
            runState = RunStateList.Idle;
        }
    }

    // =================================================================================================================================================================
    /// <summary> Bouton Test XSens a été appuyer. </summary>

    public void ButtonTestXSens()
    {
        buttonTestXSens.SetActive(false);
        buttonXSensON.SetActive(true);
        MainParameters.Instance.softwareRunMode = MainParameters.softwareRunModeType.TestXSensFromDataFiles;
        textTestXSensStatus.text = string.Format("TestXSens{0}(Fichiers)", System.Environment.NewLine);
    }

    // =================================================================================================================================================================
    /// <summary> Bouton XSens ON a été appuyer. </summary>

    public void ButtonXSensON()
    {
        buttonXSensON.SetActive(false);
        MainParameters.Instance.softwareRunMode = MainParameters.softwareRunModeType.TestXSens;
        textTestXSensStatus.text = string.Format("TestXSens{0}(XSens)", System.Environment.NewLine);

        // Initialisation des angles Euler

        eulerDataCalib = null;
        eulerDataTrial = null;

        // Désactiver le bouton Load et activer le bouton Play

        MovementF.Instance.buttonLoad.interactable = false;
        MovementF.Instance.buttonLoadImage.color = Color.gray;
        AnimationF.Instance.dropDownPlayView.interactable = true;
        AnimationF.Instance.buttonPlay.interactable = true;
        AnimationF.Instance.buttonPlayImage.color = Color.white;
    }

    // =================================================================================================================================================================
    /// <summary> Exécuter le test. </summary>

    public void StartTest()
    {
        //if (!resetOrientationDone)
        //{
        // Effacer l'affichage de la silhouette précédente, s'il y en a une d'afficher à l'écran

        AnimationF.Instance.EnableDisableAnimationOutline(false);

        // On utilise le mode Gesticulation pour le moment

        //AnimationF.Instance.playMode = MainParameters.Instance.languages.Used.animatorPlayModeGesticulation;

        // Initialisation du message de calibration

        textTestMessages = panelTestMessages.GetComponentInChildren<Text>();

        // Initialisation du compteur de frames

        nFrame = 0;

        // Initialisation de l'accès aux senseurs XSens (Centrales ou IMU = Inertia Measurement Unit)

        if (MainParameters.Instance.softwareRunMode == MainParameters.softwareRunModeType.TestXSens)
        {
            xSdataCalib = new XSensData("");
            xSdataTrial = new XSensData("");
            xEulerDataCalib = new List<XsEuler[]>();
            xEulerDataTrial = new List<XsEuler[]>();
            XSensInterface.Instance.PrepareSystem();
            //XSensInterface.Instance.ResetOrientation();
        }
        //    resetOrientationDone = true;
        //}
        //else
        runState = RunStateList.DisplayCalibMsg;
    }

    // =================================================================================================================================================================
    /// <summary> Calibration. </summary>

    void Calibration()
    {
        // Lire et conserver les matrices de rotation pour tous les senseurs XSens, seulement si tous les senseurs ont rapporté des données

        XsMatrix[] imusMatrix = null;
        XSensInterface.Instance.GetXSensData(nFrame, out imusMatrix, out XsEuler[] imusEuler, out XsVector[] imusAcc);
        if (imusMatrix == null || imusEuler == null || imusAcc == null)
            return;
#if !USE_XSENS_DATA
        if (nFrame < imusMatrixCalib.Count)
            imusMatrix = imusMatrixCalib[nFrame];
        else
            imusMatrix = imusMatrixCalib[imusMatrixCalib.Count - 1];
#endif

        xSdataCalib.Add(timeStamp.ElapsedMilliseconds, imusMatrix);
        imusMatrix = RemoveZeros(imusMatrix);
        SaveTextDataFiles(nFrame, 0, imusMatrix, 2, imusEuler, 4, imusAcc);
        //xEulerDataCalib.Add(imusEuler);
        nFrame++;

        // Faire la calibration, c'est-à-dire modifier le modèle BioRBD selon les données lues

        if (nFrame >= numberFramesCalib)
        {
            XSensInterface.readXSensDataON = false;
            calibrationDone = true;
            nFramesCalib = nFrame;

            string pathToTemplate = string.Format(@"{0}\Model_Marcel.s2mMod", Application.streamingAssetsPath);
            string pathToModel = string.Format(@"{0}\Model.s2mMod", Application.streamingAssetsPath);
            modelS2M.createModelFromStaticXsens(xSdataCalib.getData(), pathToModel, pathToTemplate);
            modelS2M.LoadModel(new System.Text.StringBuilder(pathToModel));

            string pathToTemplateRK8 = string.Format(@"{0}\Modele_Marcel_RK8.s2mMod", Application.streamingAssetsPath);
            modelInt = MainParameters.c_biorbdModel(new System.Text.StringBuilder(pathToTemplateRK8));
            nQ_modelInt = MainParameters.c_nQ(modelInt);
            nMarkers_modelInt = MainParameters.c_nMarkers(modelInt);

            //AvatarManager.Instance.UpdateParent(modelS2M);

            // Initialisation du modèle Lagrangien

            LagrangianModelSimple lagrangianModelSimple = new LagrangianModelSimple();
            MainParameters.Instance.joints.lagrangianModel = lagrangianModelSimple.GetParameters;

            MainParameters.Instance.joints.condition = 1;
            MainParameters.Instance.joints.takeOffParam.rotation = 0;
            MainParameters.Instance.joints.takeOffParam.tilt = 0;
            MainParameters.Instance.joints.takeOffParam.anteroposteriorSpeed = 0;
            MainParameters.Instance.joints.takeOffParam.verticalSpeed = 4.905f;
            MainParameters.Instance.joints.takeOffParam.somersaultSpeed = 1;
            MainParameters.Instance.joints.takeOffParam.twistSpeed = 0;

#if RK8
			qint = new float[MainParameters.Instance.joints.lagrangianModel.nDDL, 3];
			qdint = new float[MainParameters.Instance.joints.lagrangianModel.nDDL, 3];
			qddint = new float[MainParameters.Instance.joints.lagrangianModel.nDDL, 3];
#else
            qint0 = new float[MainParameters.Instance.joints.lagrangianModel.nDDL];
            qdint0 = new float[MainParameters.Instance.joints.lagrangianModel.nDDL];
            qddint0 = new float[MainParameters.Instance.joints.lagrangianModel.nDDL];
            qint1 = new float[MainParameters.Instance.joints.lagrangianModel.nDDL];
            qdint1 = new float[MainParameters.Instance.joints.lagrangianModel.nDDL];
            qddint1 = new float[MainParameters.Instance.joints.lagrangianModel.nDDL];
            qint2 = new float[MainParameters.Instance.joints.lagrangianModel.nDDL];
            qdint2 = new float[MainParameters.Instance.joints.lagrangianModel.nDDL];
            qddint2 = new float[MainParameters.Instance.joints.lagrangianModel.nDDL];
#endif

            qXSens = new double[MainParameters.Instance.joints.lagrangianModel.nDDL, 5];
            qdotXSens = new double[MainParameters.Instance.joints.lagrangianModel.nDDL];
            for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.nDDL; i++)
            {   for (int j = 0; j < 5; j++)
                    qXSens[i, j] = 0;
                qdotXSens[i] = 0;
            }
        }
    }

    // =================================================================================================================================================================
    /// <summary> Lecture des données XSens. </summary>

    bool ReadXSensData()
    {
#if USE_RK8_INPUT_DATA
		if (nFrame >= dataRK8Qdot.GetLength(0))
			return false;
#endif

        // Lire et conserver les matrices de rotation pour tous les senseurs XSens, seulement si tous les senseurs ont rapporté des données
        // Lire aussi les angles Euler pour vérification seulement

        XsMatrix[] imusMatrix;
        XSensInterface.Instance.GetXSensData(nFrameXSens, out imusMatrix, out XsEuler[] imusEuler, out XsVector[] imusAcc);
        if (imusMatrix == null || imusEuler == null)
            return false;
#if !USE_XSENS_DATA
        if (runState == RunStateList.Trial)
        {
            if (nFrame < imusMatrixTrial.Count)
                imusMatrix = imusMatrixTrial[nFrame];
            else
                imusMatrix = imusMatrixTrial[imusMatrixCalib.Count - 1];
        }
#endif
        imusMatrix = RemoveZeros(imusMatrix);
        SaveTextDataFiles(nFrameXSens, 1, imusMatrix, 3, imusEuler, 5, imusAcc);

        double[] q;
        double[] qdot;
        double[] qddot;
        int nDDL;

        // Passer les données dans un filtre de Kalman étendu et en extraire les vecteurs de positions, vitesses et accélérations des articulations (DDL)

#if KALMAN
        modelS2M.KalmanReconsIMU(imusMatrix);

        // Extraire les données obtenues du filtre de Kalman

        nDDL = modelS2M.getQ().Length;
        q = new double[nDDL];
        q = modelS2M.getQ();
        qdot = new double[nDDL];
        qddot = new double[nDDL];
        //qdot = modelS2M.getQdot();
        //qddot = modelS2M.getQddot();
#else
        q = TransEuler(imusMatrix);
        nDDL = q.Length;
        qdot = new double[nDDL];
        qddot = new double[nDDL];
#endif

        for (int i = 0; i < nDDL; i++)
        {
            for (int j = 0; j < 4; j++)
                qXSens[i, j] = qXSens[i, j + 1];
            qXSens[i, 4] = q[i];
        }

        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize - 1)
        //{
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("nFrame = {0}, nFrameXSens = {1}", nFrame, nFrameXSens);
        //    TestXSens.nMsgDebug++;
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("q[3] = {0}, qXSens[3, ...] = {1}, {2}, {3}, {4}, {5}", q[3], qXSens[3, 0], qXSens[3, 1], qXSens[3, 2], qXSens[3, 3], qXSens[3, 4]);
        //    TestXSens.nMsgDebug++;
        //}

        if (nFrameXSens <= 3)
        {
            for (int i = 0; i < nDDL; i++)

            {
                qdot[i] = 0;
                qddot[i] = 0;
            }
            //qddot[0] = -9.8;
        }
        else if (nFrameXSens == 4)
        {
            for (int i = 0; i < nDDL; i++)
            {
                double qMinus = (qXSens[i, 0] + qXSens[i, 1]) / 2;
                double qPlus = (qXSens[i, 3] + qXSens[i, 4]) / 2;
                double dt = 0.03;
                qdot[i] = (qPlus - qMinus) / dt;
                qddot[i] = 0;

                //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize - 1)
                //{
                //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("qMinus = {0}, qPlus = {1}, dt = {2} , qdot = {3}", qMinus, qPlus, dt, qdot[i]);
                //    TestXSens.nMsgDebug++;
                //}
            }
            //qddot[0] = -9.8;
        }
        else
        {
            for (int i = 0; i < nDDL; i++)
            {
                double qMinus = (qXSens[i, 0] + qXSens[i, 1]) / 2;
                double qPlus = (qXSens[i, 3] + qXSens[i, 4]) / 2;
                double dt = 0.03;
                qdot[i] = (qPlus - qMinus) / dt;
                qddot[i] = (qdot[i] - qdotXSens[i]) / MainParameters.Instance.joints.lagrangianModel.dt;
            }
        }

        for (int i = 0; i < nDDL; i++)
            qdotXSens[i] = qdot[i];

        // Mouvement affiché en retard

        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize - 1)
        //{
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("q[3] = {0}, qdot[3] = {1}, , qddot[3] = {2}", q[3], qdot[3], qddot[3]);
        //    TestXSens.nMsgDebug++;
        //}

        if (nFrameXSens > 1)
        {
            if (MainParameters.Instance.debugDataFileIMUsEulerQ && nMsgKalmanQ < nMsgSize)
            {
                for (int i = 0; i < 3; i++)
                    msgKalmanQ[nMsgKalmanQ, i] = string.Format("{0}, ", nFrame);
                for (int i = 0; i < nDDL; i++)
                {
                    msgKalmanQ[nMsgKalmanQ, 0] += string.Format("{0}, ", q[i]);
                    msgKalmanQ[nMsgKalmanQ, 1] += string.Format("{0}, ", qdot[i]);
                    msgKalmanQ[nMsgKalmanQ, 2] += string.Format("{0}, ", qddot[i]);
                }
                nMsgKalmanQ++;
            }

            //AvatarManager.Instance.SetSegmentsRotations(qBioRBD);

            // Insérer les valeurs des vecteurs q, qdot et qddot dans les matrices utilisées par l'intégrateur

#if !RK4_1
            int j = nFrame < 2 ? nFrame : 2 - nFrame % 2;
#endif

#if RK8
    		for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.nDDL; i++)
    		{
#if !USE_RK8_INPUT_DATA
    			qint[i, j] = (float)q[i];
    			qdint[i, j] = (float)qdot[i];
    			qddint[i, j] = (float)qddot[i];
#else
    			qint[i, j] = (float)dataRK8Q[nFrame, i];
    			qdint[i, j] = (float)dataRK8Qdot[nFrame, i];
    			qddint[i, j] = (float)dataRK8Qddot[nFrame, i];
#endif
    		}
#else
#if !RK4_1
            if (j <= 0)
            {
                qint0 = MathFunc.MatrixCopy(q);
                qdint0 = MathFunc.MatrixCopy(qdot);
                qddint0 = MathFunc.MatrixCopy(qddot);
            }
            else if (j == 1)
            {
                qint1 = MathFunc.MatrixCopy(q);
                qdint1 = MathFunc.MatrixCopy(qdot);
                qddint1 = MathFunc.MatrixCopy(qddot);
            }
            else
            {
                qint2 = MathFunc.MatrixCopy(q);
                qdint2 = MathFunc.MatrixCopy(qdot);
                qddint2 = MathFunc.MatrixCopy(qddot);
            }
#else
            if (nFrame <= 0)
            {
                qint0 = MathFunc.MatrixCopy(q);
                qdint0 = MathFunc.MatrixCopy(qdot);
                qddint0 = MathFunc.MatrixCopy(qddot);
            }
            else
            {
                qint1 = MathFunc.MatrixCopy(q);
                qdint1 = MathFunc.MatrixCopy(qdot);
                qddint1 = MathFunc.MatrixCopy(qddot);
            }
#endif
#endif

            if (nFrame <= 0)
            {
                if (q.Length != MainParameters.Instance.joints.lagrangianModel.nDDL || qdot.Length != MainParameters.Instance.joints.lagrangianModel.nDDL)
                {
                    Debug.Log(string.Format("Integrator: Dimensions de q (= {0}) ou qdot (= {1}) différent de lagrangianModel.nDDL (= {2}", q.Length, qdot.Length, MainParameters.Instance.joints.lagrangianModel.nDDL));
                    runState = RunStateList.PostTrial;
                    return true;
                }
                Integrator.InitMove(q, qdot);

                double[] newGravity = new double[3] { -9.81, 0, 0 };
                IntPtr ptrNewGravity = Marshal.AllocCoTaskMem(sizeof(double) * 3);
                Marshal.Copy(newGravity, 0, ptrNewGravity, 3);
                MainParameters.c_setGravity(modelInt, ptrNewGravity);
                Marshal.FreeCoTaskMem(ptrNewGravity);

                double[] gravity = new double[3];
                IntPtr ptrGravity = Marshal.AllocCoTaskMem(sizeof(double) * 3);
                MainParameters.c_getGravity(modelInt, ptrGravity);
                Marshal.Copy(ptrGravity, gravity, 0, 3);
                Marshal.FreeCoTaskMem(ptrGravity);
                Debug.Log(string.Format("Gravity = {0}, {1}, {2}", gravity[0], gravity[1], gravity[2]));

#if USE_RK8_INPUT_DATA
    			for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.nDDL; i++)
    			{
    				Integrator.xTFrame0[i] = dataRK8Q0[i];
    				Integrator.xTFrame0[MainParameters.Instance.joints.lagrangianModel.nDDL + i] = dataRK8Qdot0[i];
    			}
#endif

                //for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.nDDL; i++)
                //{
                //	Integrator.xTFrame0[i] = q[i];
                //	Integrator.xTFrame0[MainParameters.Instance.joints.lagrangianModel.nDDL + i] = qdot[i];
                //}
            }

            // Intégration à tous les 2 frames

#if !RK4_1
            if (nFrame % 2 == 0)
            {
#endif
            if (nFrame > 0)
            {
#if RK8
				Integrator.RK8(qint, qdint, qddint);
				for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.nDDL; i++)
				{
					qint[i, 0] = qint[i, 2];
					qdint[i, 0] = qdint[i, 2];
					qddint[i, 0] = qddint[i, 2];

					//Integrator.xTFrame0[i] = q[i];
					//Integrator.xTFrame0[MainParameters.Instance.joints.lagrangianModel.nDDL + i] = qdot[i];
				}
#else
#if !RK4_1
                Integrator.xTFrame1 = Integrator.RK4(Integrator.xTFrame0, qint0, qdint0, qddint0, qint1, qdint1, qddint1, qint2, qdint2, qddint2);
                qint0 = MathFunc.MatrixCopy(qint2);
                qdint0 = MathFunc.MatrixCopy(qdint2);
                qddint0 = MathFunc.MatrixCopy(qddint2);
#else
                //Integrator.xTFrame1 = Integrator.RK4_1(Integrator.xTFrame0, qddint0, qddint1);        // Ancienne version 2021-04-15
                Integrator.xTFrame1 = Integrator.RK4_1(Integrator.xTFrame0, qint0, qdint0, qddint0, qint1, qdint1, qddint1);
                qint0 = MathFunc.MatrixCopy(qint1);
                qdint0 = MathFunc.MatrixCopy(qdint1);
                qddint0 = MathFunc.MatrixCopy(qddint1);
#endif
#endif
                foreach (int i in MainParameters.Instance.joints.lagrangianModel.q1)
                {
                    int ii = i - 1;
                    Integrator.xTFrame0[ii] = Integrator.xTFrame1[ii];
                    Integrator.xTFrame0[MainParameters.Instance.joints.lagrangianModel.nDDL + ii] = Integrator.xTFrame1[MainParameters.Instance.joints.lagrangianModel.nDDL + ii];
                }
                foreach (int i in MainParameters.Instance.joints.lagrangianModel.q2)
                {
                    int ii = i - 1;
#if !USE_RK8_INPUT_DATA
                    Integrator.xTFrame0[ii] = q[ii];
                    Integrator.xTFrame0[MainParameters.Instance.joints.lagrangianModel.nDDL + ii] = qdot[ii];
#else
                    Integrator.xTFrame0[ii] = (float)dataRK8Q[nFrame, ii];
                    Integrator.xTFrame0[MainParameters.Instance.joints.lagrangianModel.nDDL + ii] = (float)dataRK8Qdot[nFrame, ii];
#endif
                    //Integrator.xTFrame0[ii] = Integrator.xTFrame1[ii];
                    //Integrator.xTFrame0[MainParameters.Instance.joints.lagrangianModel.nDDL + ii] = Integrator.xTFrame1[MainParameters.Instance.joints.lagrangianModel.nDDL + ii];
                }
            }

            if (MainParameters.Instance.debugDataFileIMUsEulerQ && nMsgRK8Q < nMsgSize)
            {
                for (int i = 0; i < 2; i++)
                    msgRK8Q[nMsgRK8Q, i] = string.Format("{0}, ", nFrame);
                for (int i = 0; i < nDDL; i++)
                {
                    msgRK8Q[nMsgRK8Q, 0] += string.Format("{0}, ", Integrator.xTFrame0[i]);
                    msgRK8Q[nMsgRK8Q, 1] += string.Format("{0}, ", Integrator.xTFrame0[i + MainParameters.Instance.joints.lagrangianModel.nDDL]);
                }
                nMsgRK8Q++;
            }
#if !RK4_1
            }
#endif

            // Incrémenter le compteur de frames

            nFrame++;
        }
        nFrameXSens++;

        if (MainParameters.Instance.softwareRunMode == MainParameters.softwareRunModeType.TestXSens && runState == RunStateList.Trial)
            AnimationF.Instance.textChrono.text = string.Format("{0:0.0}", Time.time - timeBegin);

        return true;
    }

    // =================================================================================================================================================================
    /// <summary> Terminer correctement la fin de l'essai. </summary>

    public void TerminateTrial()
    {
        if (logDataInformation)
            DisplayLogData();

        if (MainParameters.Instance.debugDataFileIMUsEulerQ)
        {
            for (int i = 0; i < nDataFiles; i++)
            {
                System.IO.File.Delete(nameDataFiles[i]);
                for (int j = 0; j < nTextDataFiles[i]; j++)
                {
                    System.IO.File.AppendAllText(nameDataFiles[i], string.Format("{0}{1}", textDataFiles[j, i], System.Environment.NewLine));
                }
            }

            string fileN1 = string.Format(@"C:\Devel\AcroVR_XSensInterface.txt");
            System.IO.File.Delete(fileN1);
            for (int i = 0; i < nMsgXInterface; i++)
                System.IO.File.AppendAllText(fileN1, string.Format("{0}{1}", MsgXInterface[i], System.Environment.NewLine));

            for (int i = 0; i < XSensInterface._numberIMUtoConnect; i++)
            {
                string fileN = string.Format(@"C:\Devel\AcroVR_FreeAcceleration{0}.txt", i);
                System.IO.File.Delete(fileN);
                for (int j = 0; j < nMsgXInterfaceFreeAcc[i]; j++)
                    System.IO.File.AppendAllText(fileN, string.Format("{0}{1}", MsgXInterfaceFreeAcc[j, i], System.Environment.NewLine));
                fileN = string.Format(@"C:\Devel\AcroVR_CalibratedAcceleration{0}.txt", i);
                System.IO.File.Delete(fileN);
                for (int j = 0; j < nMsgXInterfaceCalAcc[i]; j++)
                    System.IO.File.AppendAllText(fileN, string.Format("{0}{1}", MsgXInterfaceCalAcc[j, i], System.Environment.NewLine));
                fileN = string.Format(@"C:\Devel\AcroVR_MagneticField{0}.txt", i);
                System.IO.File.Delete(fileN);
                for (int j = 0; j < nMsgXInterfaceMagField[i]; j++)
                    System.IO.File.AppendAllText(fileN, string.Format("{0}{1}", MsgXInterfaceMagField[j, i], System.Environment.NewLine));
            }

            string fileN2 = string.Format(@"C:\Devel\AcroVR_S2M.txt");
            System.IO.File.Delete(fileN2);
            for (int i = 0; i < nMsgS2M; i++)
                System.IO.File.AppendAllText(fileN2, string.Format("{0}{1}", msgS2M[i], System.Environment.NewLine));

            for (int i = 0; i < nMsgEuler; i++)
            {
                for (int j = 0; j < XSensInterface._numberIMUtoConnect; j++)
                {
                    string fileN = string.Format(@"C:\Devel\AcroVR_IMUsEuler{0}.txt", j);
                    if (i <= 0)
                        System.IO.File.Delete(fileN);
                    System.IO.File.AppendAllText(fileN, string.Format("{0}{1}", msgEuler[i, j], System.Environment.NewLine));
                }
            }

            string fileKalmanQ = @"C:\Devel\AcroVR_KalmanQ.txt";
            string fileKalmanQdot = @"C:\Devel\AcroVR_KalmanQdot.txt";
            string fileKalmanQddot = @"C:\Devel\AcroVR_KalmanQddot.txt";
            System.IO.File.Delete(fileKalmanQ);
            System.IO.File.Delete(fileKalmanQdot);
            System.IO.File.Delete(fileKalmanQddot);
            for (int i = 0; i < nMsgKalmanQ; i++)
            {
                System.IO.File.AppendAllText(fileKalmanQ, string.Format("{0}{1}", msgKalmanQ[i, 0], System.Environment.NewLine));
                System.IO.File.AppendAllText(fileKalmanQdot, string.Format("{0}{1}", msgKalmanQ[i, 1], System.Environment.NewLine));
                System.IO.File.AppendAllText(fileKalmanQddot, string.Format("{0}{1}", msgKalmanQ[i, 2], System.Environment.NewLine));
            }

            string fileRK8Q = @"C:\Devel\AcroVR_RK8Q.txt";
            string fileRK8Qdot = @"C:\Devel\AcroVR_RK8Qdot.txt";
            System.IO.File.Delete(fileRK8Q);
            System.IO.File.Delete(fileRK8Qdot);
            for (int i = 0; i < nMsgRK8Q; i++)
            {
                System.IO.File.AppendAllText(fileRK8Q, string.Format("{0}{1}", msgRK8Q[i, 0], System.Environment.NewLine));
                System.IO.File.AppendAllText(fileRK8Qdot, string.Format("{0}{1}", msgRK8Q[i, 1], System.Environment.NewLine));
            }

            for (int i = 0; i < nMsgJCS; i++)
            {
                for (int j = 0; j < XSensInterface._numberIMUtoConnect; j++)
                {
                    string fileN = string.Format(@"C:\Devel\AcroVR_JCS{0}.txt", j);
                    if (i <= 0)
                        System.IO.File.Delete(fileN);
                    //System.IO.File.AppendAllText(fileN, string.Format("{0}{1}", msgJCS[i, j], System.Environment.NewLine));
                }
            }

            for (int i = 0; i < nMsgDebug; i++)
            {
                string fileN = string.Format(@"C:\Devel\AcroVR_Debug.txt");
                if (i <= 0)
                    System.IO.File.Delete(fileN);
                System.IO.File.AppendAllText(fileN, string.Format("{0}{1}", msgDebug[i], System.Environment.NewLine));
            }
        }
        modelS2M.WriteModelInFile();

        // Convertir les données des angles Euler sous un nouveau format

        if (xEulerDataCalib.Count > 0 && xEulerDataTrial.Count > 0)
        {
            eulerDataCalib = ConvertEulerData(xEulerDataCalib);
            eulerDataTrial = ConvertEulerData(xEulerDataTrial);
        }

        // Activer le bouton Save

        MovementF.Instance.buttonSave.interactable = true;
        MovementF.Instance.buttonSaveImage.color = Color.white;
    }

    // =================================================================================================================================================================
    /// <summary> Obtenir les transformations en angles Euler. </summary>

    double[] TransEuler(XsMatrix[] imusMatrix)
    {
        double[,] imu0 = new double[3, 3];
        double[,] imu0_T = new double[3, 3];
        for (uint nRow = 0; nRow < 3; nRow++)
            for (uint nCol = 0; nCol < 3; nCol++)
            {
                imu0[nRow, nCol] = imusMatrix[0].value(nCol, nRow);
                imu0_T[nRow, nCol] = imusMatrix[0].value(nRow, nCol);
            }

        double[,] imu1 = new double[3, 3];
        for (uint nRow = 0; nRow < 3; nRow++)
            for (uint nCol = 0; nCol < 3; nCol++)
                imu1[nRow, nCol] = imusMatrix[1].value(nCol, nRow);

        double[,] imu2 = new double[3, 3];
        for (uint nRow = 0; nRow < 3; nRow++)
            for (uint nCol = 0; nCol < 3; nCol++)
                imu2[nRow, nCol] = imusMatrix[2].value(nCol, nRow);

        IntPtr ptr_Rot0 = Marshal.AllocCoTaskMem(sizeof(double) * 9);
        IntPtr ptr_Rot1 = Marshal.AllocCoTaskMem(sizeof(double) * 9);
        IntPtr ptr_Rot2 = Marshal.AllocCoTaskMem(sizeof(double) * 9);
        IntPtr ptr_EulerTransfo0 = Marshal.AllocCoTaskMem(sizeof(double) * 3);
        IntPtr ptr_EulerTransfo1 = Marshal.AllocCoTaskMem(sizeof(double) * 3);
        IntPtr ptr_EulerTransfo2 = Marshal.AllocCoTaskMem(sizeof(double) * 3);

        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize - 12)
        //{
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("imu0: nFrame = {0}", nFrame);
        //    TestXSens.nMsgDebug++;
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("{0}, {1}, {2}", imu0[0, 0], imu0[0, 1], imu0[0, 2]);
        //    TestXSens.nMsgDebug++;
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("{0}, {1}, {2}", imu0[1, 0], imu0[1, 1], imu0[1, 2]);
        //    TestXSens.nMsgDebug++;
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("{0}, {1}, {2}", imu0[2, 0], imu0[2, 1], imu0[2, 2]);
        //    TestXSens.nMsgDebug++;

        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("imu1: nFrame = {0}", nFrame);
        //    TestXSens.nMsgDebug++;
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("{0}, {1}, {2}", imu1[0, 0], imu1[0, 1], imu1[0, 2]);
        //    TestXSens.nMsgDebug++;
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("{0}, {1}, {2}", imu1[1, 0], imu1[1, 1], imu1[1, 2]);
        //    TestXSens.nMsgDebug++;
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("{0}, {1}, {2}", imu1[2, 0], imu1[2, 1], imu1[2, 2]);
        //    TestXSens.nMsgDebug++;

        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("imu2: nFrame = {0}", nFrame);
        //    TestXSens.nMsgDebug++;
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("{0}, {1}, {2}", imu2[0, 0], imu2[0, 1], imu2[0, 2]);
        //    TestXSens.nMsgDebug++;
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("{0}, {1}, {2}", imu2[1, 0], imu2[1, 1], imu2[1, 2]);
        //    TestXSens.nMsgDebug++;
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("{0}, {1}, {2}", imu2[2, 0], imu2[2, 1], imu2[2, 2]);
        //    TestXSens.nMsgDebug++;
        //}

        MainParameters.c_rotation(imu0[0, 0], imu0[0, 1], imu0[0, 2], imu0[1, 0], imu0[1, 1], imu0[1, 2], imu0[2, 0], imu0[2, 1], imu0[2, 2], ptr_Rot0);
        MainParameters.c_rotationToEulerAngles(ptr_Rot0, new System.Text.StringBuilder("xyz"), ptr_EulerTransfo0);

        double[,] rotMat1 = MathFunc.MatrixMultiply(imu1, imu0_T);
        MainParameters.c_rotation(rotMat1[0, 0], rotMat1[0, 1], rotMat1[0, 2], rotMat1[1, 0], rotMat1[1, 1], rotMat1[1, 2], rotMat1[2, 0], rotMat1[2, 1], rotMat1[2, 2], ptr_Rot1);
        MainParameters.c_rotationToEulerAngles(ptr_Rot1, new System.Text.StringBuilder("xyz"), ptr_EulerTransfo1);

        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize - 4)
        //{
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("rotMat1: nFrame = {0}", nFrame);
        //    TestXSens.nMsgDebug++;
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("{0}, {1}, {2}", rotMat1[0, 0], rotMat1[0, 1], rotMat1[0, 2]);
        //    TestXSens.nMsgDebug++;
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("{0}, {1}, {2}", rotMat1[1, 0], rotMat1[1, 1], rotMat1[1, 2]);
        //    TestXSens.nMsgDebug++;
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("{0}, {1}, {2}", rotMat1[2, 0], rotMat1[2, 1], rotMat1[2, 2]);
        //    TestXSens.nMsgDebug++;
        //}

        double[,] rotMat2 = MathFunc.MatrixMultiply(imu2, imu0_T);
        MainParameters.c_rotation(rotMat2[0, 0], rotMat2[0, 1], rotMat2[0, 2], rotMat2[1, 0], rotMat2[1, 1], rotMat2[1, 2], rotMat2[2, 0], rotMat2[2, 1], rotMat2[2, 2], ptr_Rot2);
        MainParameters.c_rotationToEulerAngles(ptr_Rot2, new System.Text.StringBuilder("xyz"), ptr_EulerTransfo2);

        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize - 4)
        //{
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("rotMat2: nFrame = {0}", nFrame);
        //    TestXSens.nMsgDebug++;
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("{0}, {1}, {2}", rotMat2[0, 0], rotMat2[0, 1], rotMat2[0, 2]);
        //    TestXSens.nMsgDebug++;
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("{0}, {1}, {2}", rotMat2[1, 0], rotMat2[1, 1], rotMat2[1, 2]);
        //    TestXSens.nMsgDebug++;
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("{0}, {1}, {2}", rotMat2[2, 0], rotMat2[2, 1], rotMat2[2, 2]);
        //    TestXSens.nMsgDebug++;
        //}

        double[] eulerTransfo0 = new double[3];
        double[] eulerTransfo1 = new double[3];
        double[] eulerTransfo2 = new double[3];
        Marshal.Copy(ptr_EulerTransfo0, eulerTransfo0, 0, 3);
        Marshal.Copy(ptr_EulerTransfo1, eulerTransfo1, 0, 3);
        Marshal.Copy(ptr_EulerTransfo2, eulerTransfo2, 0, 3);

        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize - 2)
        //{
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("eulerTransfo0: nFrame = {0}", nFrame);
        //    TestXSens.nMsgDebug++;
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("{0}, {1}, {2}", eulerTransfo0[0], eulerTransfo0[1], eulerTransfo0[2]);
        //    TestXSens.nMsgDebug++;

        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("eulerTransfo1: nFrame = {0}", nFrame);
        //    TestXSens.nMsgDebug++;
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("{0}, {1}, {2}", eulerTransfo1[0], eulerTransfo1[1], eulerTransfo1[2]);
        //    TestXSens.nMsgDebug++;

        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("eulerTransfo2: nFrame = {0}", nFrame);
        //    TestXSens.nMsgDebug++;
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("{0}, {1}, {2}", eulerTransfo2[0], eulerTransfo2[1], eulerTransfo2[2]);
        //    TestXSens.nMsgDebug++;
        //}

        double[] Q_out = new double[12];
        for (int i = 0; i < 3; i++)
        {
            Q_out[i] = 0;
            Q_out[i + 3] = eulerTransfo0[i];
            Q_out[i + 6] = eulerTransfo1[i];
            Q_out[i + 9] = eulerTransfo2[i];
        }

        Marshal.FreeCoTaskMem(ptr_EulerTransfo0);
        Marshal.FreeCoTaskMem(ptr_EulerTransfo1);
        Marshal.FreeCoTaskMem(ptr_EulerTransfo2);
        Marshal.FreeCoTaskMem(ptr_Rot0);
        Marshal.FreeCoTaskMem(ptr_Rot1);
        Marshal.FreeCoTaskMem(ptr_Rot2);

        return Q_out;
    }

    // =================================================================================================================================================================
    /// <summary> Afficher les données log sur la console ou dans un fichier output_log.txt, si on est en mode "Build". </summary>

    void DisplayLogData()
    {
        int sumNumCallback = 0;
        int minNumCallback = 9999;
        int maxNumCallback = -1;
        int sumNumIMUs = 0;
        int minNumIMUs = 9999;
        int maxNumIMUs = -1;
        double sumNumMs = 0;
        double minNumMs = 9999;
        double maxNumMs = -1;

        if (MainParameters.Instance.softwareRunMode == MainParameters.softwareRunModeType.TestXSens)
        {
            for (int i = 0; i < XSensInterface.nProcessFrame; i++)
            {
                sumNumCallback += XSensInterface.numCallbackByFrame[i];
                if (XSensInterface.numCallbackByFrame[i] < minNumCallback)
                    minNumCallback = XSensInterface.numCallbackByFrame[i];
                if (XSensInterface.numCallbackByFrame[i] > maxNumCallback)
                    maxNumCallback = XSensInterface.numCallbackByFrame[i];
                sumNumIMUs += XSensInterface.numIMUsByFrame[i];
                if (XSensInterface.numIMUsByFrame[i] < minNumIMUs)
                    minNumIMUs = XSensInterface.numIMUsByFrame[i];
                if (XSensInterface.numIMUsByFrame[i] > maxNumIMUs)
                    maxNumIMUs = XSensInterface.numIMUsByFrame[i];
                sumNumMs += XSensInterface.numMsByFrame[i];
                if (XSensInterface.numMsByFrame[i] < minNumMs)
                    minNumMs = XSensInterface.numMsByFrame[i];
                if (XSensInterface.numMsByFrame[i] > maxNumMs)
                    maxNumMs = XSensInterface.numMsByFrame[i];
            }

            Debug.Log(string.Format("XSens Update rate = {0} hz", XSensInterface.updateRateXSens));
            Debug.Log(string.Format("Temps lecture     = {0:0.00}", timeTrialDuration));
            Debug.Log(string.Format("Frames lus        = {0}, {1} ({2})", XSensInterface.nProcessFrame + 1, nFramesTrial, nFramesCalib));
            Debug.Log(string.Format("Callback          = {0}, {1:0.0}, {2}, {3}", sumNumCallback, sumNumCallback / XSensInterface.nProcessFrame, minNumCallback, maxNumCallback));
            Debug.Log(string.Format("Senseurs XSens    = {0:0.0}, {1}, {2}", sumNumIMUs / XSensInterface.nProcessFrame, minNumIMUs, maxNumIMUs));
            Debug.Log(string.Format("Durée frames      = {0:0.00}, {1}, {2}", sumNumMs / XSensInterface.nProcessFrame, Math.Round(minNumMs), Math.Round(maxNumMs)));
        }
        else
            Debug.Log(string.Format("Frames lus        = {0} ({1})", nFramesTrial, nFramesCalib));
    }


    // =================================================================================================================================================================
    /// <summary> Enlever les zéros. </summary>

    XsMatrix[] RemoveZeros(XsMatrix[] imusMatrix)
    {
        XsMatrix[] newImusMatrix;
        newImusMatrix = new XsMatrix[imusMatrix.Length];

        for (int nImus = 0; nImus < imusMatrix.Length; nImus++)
        {
            newImusMatrix[nImus] = new XsMatrix(3, 3);

            bool allZeros = true;
            uint nRow = 0;
            while (allZeros && nRow < 3)
            {
                uint nCol = 0;
                while (nCol < 3 && imusMatrix[nImus].value(nRow, nCol) == 0)
                    nCol++;
                if (nCol < 3)
                    allZeros = false;
                nRow++;
            }

            if (allZeros && nFrame >= 1)
                newImusMatrix[nImus] = imusMatrixPrev[nImus];
            else
            {
                imusMatrixPrev[nImus] = imusMatrix[nImus];
                newImusMatrix[nImus] = imusMatrix[nImus];
            }
        }

        return newImusMatrix;
    }

    // =================================================================================================================================================================
    /// <summary> Conserver les données spécifiées sous forme de texte, qui peut être imprimé aisément dans un fichier. </summary>

    void SaveTextDataFiles(int nF, int indexImus, XsMatrix[] imus, int indexEuler, XsEuler[] euler, int indexAcc, XsVector[] acc)
	{
		if (MainParameters.Instance.debugDataFileIMUsEulerQ)
		{
			for (int i = 0; i < XSensInterface._numberIMUtoConnect; i++)
			{
				int ii = i + indexDataFiles[indexImus];
				if (nTextDataFiles[ii] < nMaxTextDataFiles)
				{
					textDataFiles[nTextDataFiles[ii], ii] = string.Format("{0}, {1}, ", i, nF);
					for (uint nRow = 0; nRow < 3; nRow++)
						for (uint nCol = 0; nCol < 3; nCol++)
							textDataFiles[nTextDataFiles[ii], ii] += string.Format("{0}, ", imus[i].value(nRow, nCol));
					nTextDataFiles[ii]++;
				}

                ii = i + indexDataFiles[indexEuler];
                if (nTextDataFiles[ii] < nMaxTextDataFiles)
                {
                    textDataFiles[nTextDataFiles[ii], ii] = string.Format("{0}, {1}, {2}, {3}, {4},", i, nF, euler[i].yaw(), euler[i].pitch(), euler[i].roll());
                    nTextDataFiles[ii]++;
                }

                ii = i + indexDataFiles[indexAcc];
                if (nTextDataFiles[ii] < nMaxTextDataFiles)
                {
                    textDataFiles[nTextDataFiles[ii], ii] = string.Format("{0}, {1}, ", i, nF);
                    for (uint n = 0; n < 3; n++)
                        textDataFiles[nTextDataFiles[ii], ii] += string.Format("{0}, ", acc[i].value(n));
                    nTextDataFiles[ii]++;
                }
            }
        }
	}

	// =================================================================================================================================================================
	/// <summary> Lire les données XSens spécifiées à partir de fichiers de données. </summary>

	List<XsMatrix[]> ReadXSensDataFiles(string dataType)
	{
		List<XsMatrix[]> listOfIMUsMatrix = new List<XsMatrix[]>();

		int nDataType = idDataFiles.Length - 1;
		while (nDataType >= 0 && !idDataFiles[nDataType].Contains(dataType))
			nDataType--;
		//Debug.Log(string.Format("nDataType = {0}", nDataType));

		// Lecture de tous les fichiers de données et on place toutes les lignes dans une matrice de chaîne de caractères

		string[,] fileLines = new string[nMaxTextDataFiles, XSensInterface._numberIMUtoConnect];
		int nLines = 0;
		for (int i = 0; i < XSensInterface._numberIMUtoConnect; i++)
		{
			int ii = indexDataFiles[nDataType] + i;
			string[] fileL = System.IO.File.ReadAllLines(nameReadDataFiles[ii]);
			for (int j = 0; j < fileL.Length; j++)
				fileLines[j, i] = fileL[j];
			if (i <= 0)
				nLines = fileL.Length;
		}

		// Création de la liste des matrices IMU via la matrice de chaîne de caractères

		string[] values;
		for (int i = 0; i < nLines; i++)
		{
			XsMatrix[] imusMatrix = new XsMatrix[XSensInterface._numberIMUtoConnect];
			for (int j = 0; j < XSensInterface._numberIMUtoConnect; j++)
			{
				imusMatrix[j] = new XsMatrix(3, 3);
				values = System.Text.RegularExpressions.Regex.Split(fileLines[i, j], string.Format(","));
				for (uint k = 0; k < 9; k++)
					imusMatrix[j].setValue(k / 3, k % 3, double.Parse(values[k + 2]));

				//Debug.Log(string.Format("i, j = {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}", i, j, imusMatrix[j].value(0,0), imusMatrix[j].value(0, 1), imusMatrix[j].value(0, 2),
				//							imusMatrix[j].value(1, 0), imusMatrix[j].value(1, 1), imusMatrix[j].value(1, 2), imusMatrix[j].value(2, 0), imusMatrix[j].value(2, 1), imusMatrix[j].value(2, 2)));
			}
			listOfIMUsMatrix.Add(imusMatrix);
		}

		return listOfIMUsMatrix;
	}

	// =================================================================================================================================================================

	void ReadIntegratorInputDataFiles1(out double[] q, out double[] qdot)
	{
		int nDDL = 12;
		string[] name = new string[2] { "Q0", "Qdot0" };
		q = new double[nDDL];
		qdot = new double[nDDL];
		for (int i = 0; i < 2; i++)
		{
			string[] fileL = System.IO.File.ReadAllLines(string.Format(@"C:\Devel\AcroVRXSensDataFiles\RK8\2020-05-04\AcroVR_RK8Init{0}.txt", name[i]));
			string[] values = System.Text.RegularExpressions.Regex.Split(fileL[0], string.Format(","));
			if (i <= 0)
			{
				for (int j = 0; j < nDDL; j++)
					q[j] = double.Parse(values[j + 1]);
			}
			else
			{
				for (int j = 0; j < nDDL; j++)
					qdot[j] = double.Parse(values[j + 1]);
			}
		}
	}

	// =================================================================================================================================================================

	void ReadIntegratorInputDataFiles2(out double[,] q, out double[,] qdot, out double[,] qddot)
	{
		int nDDL = 12;
		string[] name = new string[3] { "Q", "Qdot", "Qddot" };

		string[] fileL = System.IO.File.ReadAllLines(@"C:\Devel\AcroVRXSensDataFiles\RK8\2020-05-04\AcroVR_RK8InputQ.txt");
		q = new double[fileL.Length, nDDL];
		for (int i = 0; i < fileL.Length; i++)
		{
			string[] values = System.Text.RegularExpressions.Regex.Split(fileL[i], string.Format(","));
			for (int j = 0; j < nDDL; j++)
				q[i, j] = double.Parse(values[j + 1]);
		}

		fileL = System.IO.File.ReadAllLines(@"C:\Devel\AcroVRXSensDataFiles\RK8\2020-05-04\AcroVR_RK8InputQdot.txt");
		qdot = new double[fileL.Length, nDDL];
		for (int i = 0; i < fileL.Length; i++)
		{
			string[] values = System.Text.RegularExpressions.Regex.Split(fileL[i], string.Format(","));
			for (int j = 0; j < nDDL; j++)
				qdot[i, j] = double.Parse(values[j + 1]);
		}

		fileL = System.IO.File.ReadAllLines(@"C:\Devel\AcroVRXSensDataFiles\RK8\2020-05-04\AcroVR_RK8InputQddot.txt");
		qddot = new double[fileL.Length, nDDL];
		for (int i = 0; i < fileL.Length; i++)
		{
			string[] values = System.Text.RegularExpressions.Regex.Split(fileL[i], string.Format(","));
			for (int j = 0; j < nDDL; j++)
				qddot[i, j] = double.Parse(values[j + 1]);
		}
	}

	// =================================================================================================================================================================
	/// <summary> Sélection et chargement des fichiers de données. </summary>

	public void SelectAndLoadDataFiles()
	{
		// Sélection d'un fichier de données dans le répertoire des fichiers de simulation XSens, par défaut

		ExtensionFilter[] extensions = new[]
		{
			new ExtensionFilter("Fichiers Données (*.xSd;*.txt)", new string[2] { "xSd", "txt" }),
			new ExtensionFilter(MainParameters.Instance.languages.Used.movementLoadDataFileAllFiles, "*" ),
		};
#if UNITY_STANDALONE_OSX
		Debug.Log("Ne fonctionne pas sur Mac !!!");
		return;
#elif UNITY_EDITOR
		string dirSimulationFilesXSens = string.Format(@"{0}/../Installer/SimulationFilesXSens", UnityEngine.Application.dataPath);
		if (!System.IO.Directory.Exists(dirSimulationFilesXSens))
		{
			dirSimulationFilesXSens = string.Format(@"{0}\Tekphy\AcroVR\SimulationFilesXSens", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
			if (!System.IO.Directory.Exists(dirSimulationFilesXSens))
			{
				dirSimulationFilesXSens = string.Format(@"{0}\Tekphy\AcroVR\SimulationFilesXSens", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
				if (!System.IO.Directory.Exists(dirSimulationFilesXSens))
					dirSimulationFilesXSens = "";
			}

		}
#else
		string dirSimulationFilesXSens = string.Format(@"{0}\Tekphy\AcroVR\SimulationFilesXSens", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
		if (!System.IO.Directory.Exists(dirSimulationFilesXSens))
		{
			dirSimulationFilesXSens = string.Format(@"{0}\Tekphy\AcroVR\SimulationFilesXSens", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
			if (!System.IO.Directory.Exists(dirSimulationFilesXSens))
				dirSimulationFilesXSens = "";
		}

#endif
		string fileName = FileBrowser.OpenSingleFile(string.Format("{0} XSens", MainParameters.Instance.languages.Used.movementLoadDataFileTitle), dirSimulationFilesXSens, extensions);
		if (fileName.Length <= 0)
			return;

		// Initialisation des noms de tous les fichiers de simulation XSens utilisées, selon le fichier de simulation XSens sélectionné

		int index = fileName.ToLower().IndexOf("_rotationcalib.xsd");
		if (index < 0)
		{
			index = fileName.ToLower().IndexOf("_rotationtrial.xsd");
			if (index < 0)
			{
				index = fileName.ToLower().IndexOf("_eulercalib.txt");
				if (index < 0)
					index = fileName.ToLower().IndexOf("_eulertrial.txt");
			}
		}

		if (index < 0)
			return;
		string fileNameRotationCalib = string.Format(@"{0}_RotationCalib.xSd", fileName.Substring(0, index));
		string fileNameRotationTrial = string.Format(@"{0}_RotationTrial.xSd", fileName.Substring(0, index));
		string fileNameEulerCalib = string.Format(@"{0}_EulerCalib.txt", fileName.Substring(0, index));
		string fileNameEulerTrial = string.Format(@"{0}_EulerTrial.txt", fileName.Substring(0, index));

		// Lecture des fichiers de simulation XSens

		xSdataCalib = new XSensData("");
		xSdataTrial = new XSensData("");
		xSdataCalib.readTrialFromBinary(fileNameRotationCalib);
		xSdataTrial.readTrialFromBinary(fileNameRotationTrial);
		eulerDataCalib = null;
		eulerDataCalib = DataFileManager.Instance.ReadEulerDataFromFile(fileNameEulerCalib);
		eulerDataTrial = null;
		eulerDataTrial = DataFileManager.Instance.ReadEulerDataFromFile(fileNameEulerTrial);

		// Afficher le nom du fichier à l'écran

		MovementF.Instance.textFileName.text = System.IO.Path.GetFileName(fileName.Substring(0, index));

		// Activer certains boutons de la section animation

		AnimationF.Instance.dropDownPlayView.interactable = true;
		AnimationF.Instance.buttonPlay.interactable = true;
		AnimationF.Instance.buttonPlayImage.color = Color.white;
	}

	// =================================================================================================================================================================
	/// <summary> Consever les données dans des fichiers de données. </summary>

	public void SaveDataFiles()
	{
		// Utilisation d'un répertoire de données par défaut, alors si ce répertoire n'existe pas, il faut le créer

#if UNITY_STANDALONE_OSX
		string dirSimulationFilesXSens = string.Format("{0}/Documents/AcroVR", Environment.GetFolderPath(Environment.SpecialFolder.Personal));
#else
		string dirSimulationFilesXSens = Environment.ExpandEnvironmentVariables(@"%UserProfile%\Documents\AcroVR");
#endif
		if (!System.IO.Directory.Exists(dirSimulationFilesXSens))
		{
			try
			{
				System.IO.Directory.CreateDirectory(dirSimulationFilesXSens);
			}
			catch
			{
				dirSimulationFilesXSens = "";
			}
		}

		// Sélection d'un ancien fichier de données qui sera modifié ou d'un nouveau fichier de données qui sera créé

		string fileName = FileBrowser.SaveFile("Créer/modifier un Fichier de Simulation XSens", dirSimulationFilesXSens, "DefaultFile", "txt");
		if (fileName.Length <= 0)
			return;

		// Initialisation des noms de tous les fichiers de simulation XSens utilisées

		int index = fileName.ToLower().IndexOf("_rotation");
		if (index < 0)
		{
			index = fileName.ToLower().IndexOf("_euler");
			if (index < 0)
				index = fileName.ToLower().IndexOf(".");
		}
		string fileNameRotationCalib = string.Format(@"{0}_RotationCalib.xSd", fileName.Substring(0, index));
		string fileNameRotationTrial = string.Format(@"{0}_RotationTrial.xSd", fileName.Substring(0, index));
		string fileNameEulerCalib = string.Format(@"{0}_EulerCalib.txt", fileName.Substring(0, index));
		string fileNameEulerTrial = string.Format(@"{0}_EulerTrial.txt", fileName.Substring(0, index));

		// Conserver les fichiers de données, en affichant un message d'attente à l'écran

		StartCoroutine(SaveDataWithWaitMessage(fileName, fileNameRotationCalib, fileNameRotationTrial, fileNameEulerCalib, fileNameEulerTrial));
	}

	// =================================================================================================================================================================
	/// <summary> Conserver les fichiers de données, en affichant un message d'attente à l'écran. </summary>

	System.Collections.IEnumerator SaveDataWithWaitMessage(string fileName, string fileNameRotationCalib, string fileNameRotationTrial, string fileNameEulerCalib, string fileNameEulerTrial)
	{
		// Afficher le message d'attente et terminer la fonction appelante

		panelTestMessages.SetActive(true);
		textTestMessages.text = "Attendre svp, création des fichers ...";
		yield return new WaitForSeconds(0.5f);

		// Conserver les fichiers de données

		xSdataCalib.writeTrialToBinary(fileNameRotationCalib);
		xSdataTrial.writeTrialToBinary(fileNameRotationTrial);
		DataFileManager.Instance.WriteEulerDataInFile(fileNameEulerCalib, eulerDataCalib);
		DataFileManager.Instance.WriteEulerDataInFile(fileNameEulerTrial, eulerDataTrial);

		// Afficher le nom du fichier à l'écran

		MovementF.Instance.textFileName.text = System.IO.Path.GetFileName(fileName);

		// Supprimer le message d'attente

		panelTestMessages.SetActive(false);
		yield return 0;
	}

	// =================================================================================================================================================================
	/// <summary> Convertir les données des angles Euler sous un nouveau format. </summary>

	double[,,] ConvertEulerData(List<XsEuler[]> xEulerData)
	{
		double[,,] eulerData = new double[xEulerData.Count, XSensInterface._numberIMUtoConnect, 3];
		int frame = 0;
		foreach (XsEuler[] xIMUsEuler in xEulerData)
		{
			int IMU = 0;
			foreach (XsEuler xIMUEuler in xIMUsEuler)
			{
				eulerData[frame, IMU, 0] = xIMUEuler.yaw();
				eulerData[frame, IMU, 1] = xIMUEuler.pitch();
				eulerData[frame, IMU, 2] = xIMUEuler.roll();
				IMU++;
			}
			frame++;
		}
		return eulerData;
	}

	double[,] ConvertEulerData(XsEuler[] xIMUsEuler)
	{
		double[,] eulerData = new double[XSensInterface._numberIMUtoConnect, 3];
		int IMU = 0;
		foreach (XsEuler xIMUEuler in xIMUsEuler)
		{
			eulerData[IMU, 0] = xIMUEuler.yaw();
			eulerData[IMU, 1] = xIMUEuler.pitch();
			eulerData[IMU, 2] = xIMUEuler.roll();
			IMU++;
		}
		return eulerData;
	}
}
