//using System;
//using System.IO;
//using System.Text;
//using UnityEngine;
//using System.Collections.Generic;
//using System.Runtime.InteropServices;
//using XDA;

//public class S2M_model : MonoBehaviour
//{

//	// Tous les pointeurs et nombre qui peuvent sauver du temps ou des appels

//	public static bool calibre = false;
//	bool m_isloaded = false;
//	bool m_isStaticDone = false;
//	public static IntPtr m_s2m; // Pointeur sur des modèles
//	public static int m_nQ = 0;
//	int m_nQdot = 0, m_nQddot = 0, m_nTags = 0;
//	IntPtr m_Q, m_Qdot, m_Qddot; // stocker ou recevoir des coordonnées généralisées

//	IntPtr m_mark; // Stocker ou recevoir des position de marqueurs

//	IntPtr m_kalman; // Pointeur sur un filtre de kalmanIMU
//	static public int nbImuToConnect = 4;
//	List<StringBuilder> m_nameIMUs = new List<StringBuilder>(); // Nom du IMU
//	List<StringBuilder> m_parentIMUs = new List<StringBuilder>(); // Nom du parent du IMU
//	List<int> m_positionIMUs = new List<int>(); // Numéro du marqueur où est le IMU
//	List<int> m_parentIMUsIdx = new List<int>(); // Numéro du marqueur où est le IMU
//	int m_nIMUs = 0;
//	IntPtr m_IMU; // stocker ou recevoir des matrices d'orientation des centrales inertielles
//	double[] Q;
//	double[] Qdot;
//	double[] Qddot;

//	public void Awake()
//	{
//		//Variables to create a new Biorbd or S2M model from a template

//		//m_nameIMUs.Add(new StringBuilder("Thorax_IMU"));
//		//m_nameIMUs.Add(new StringBuilder("RScapula_Acromion_IMU"));
//		//m_nameIMUs.Add(new StringBuilder("RHumerus_Delt_IMU"));

//		//m_parentIMUs.Add(new StringBuilder("Thorax"));
//		//m_parentIMUs.Add(new StringBuilder("RScapula"));
//		//m_parentIMUs.Add(new StringBuilder("RHumerus"));

//		//m_parentIMUsIdx.Add(0);
//		//m_parentIMUsIdx.Add(1);
//		//m_parentIMUsIdx.Add(2);

//		//m_positionIMUs.Add(0);
//		//m_positionIMUs.Add(1);
//		//m_positionIMUs.Add(2);

//		m_nameIMUs.Add(new StringBuilder("Pelvis_IMU"));
//        m_nameIMUs.Add(new StringBuilder("RightUpperArm_IMU"));
//		m_nameIMUs.Add(new StringBuilder("LeftUpperArm_IMU"));
//        m_nameIMUs.Add(new StringBuilder("Bedaine_IMU"));

//        m_parentIMUs.Add(new StringBuilder("Pelvis"));
//        m_parentIMUs.Add(new StringBuilder("RightArm"));
//		m_parentIMUs.Add(new StringBuilder("LeftArm"));
//        m_parentIMUs.Add(new StringBuilder("Thorax"));

//        m_parentIMUsIdx.Add(0);
//		m_parentIMUsIdx.Add(1);
//		m_parentIMUsIdx.Add(2);
//        m_parentIMUsIdx.Add(3);

//        m_positionIMUs.Add(0);
//		m_positionIMUs.Add(1);
//		m_positionIMUs.Add(2);
//        m_positionIMUs.Add(3);

//        //m_nameIMUs.Add(new StringBuilder("Thorax_IMU"));
//        //m_nameIMUs.Add(new StringBuilder("Scapula_Epine_IMU"));
//        //m_nameIMUs.Add(new StringBuilder("Scapula_Acromion_IMU"));
//        //m_nameIMUs.Add(new StringBuilder("Humerus_Delt_IMU"));
//        //m_nameIMUs.Add(new StringBuilder("Humerus_EPICm_IMU"));
//        //m_nameIMUs.Add(new StringBuilder("AvBras_Olecrane_IMU"));
//        //m_nameIMUs.Add(new StringBuilder("AvBras_Poignet_IMU"));

//        //m_parentIMUs.Add(new StringBuilder("Thorax"));
//        //m_parentIMUs.Add(new StringBuilder("Scapula"));
//        //m_parentIMUs.Add(new StringBuilder("Scapula"));
//        //m_parentIMUs.Add(new StringBuilder("Humerus"));
//        //m_parentIMUs.Add(new StringBuilder("Humerus"));
//        //m_parentIMUs.Add(new StringBuilder("AvBras"));
//        //m_parentIMUs.Add(new StringBuilder("Radius"));

//        //m_parentIMUsIdx.Add(0);
//        //m_parentIMUsIdx.Add(1);
//        //m_parentIMUsIdx.Add(1);
//        //m_parentIMUsIdx.Add(2);
//        //m_parentIMUsIdx.Add(2);
//        //m_parentIMUsIdx.Add(3);
//        //m_parentIMUsIdx.Add(4);

//        //m_positionIMUs.Add(6);
//        //m_positionIMUs.Add(12);
//        //m_positionIMUs.Add(14);
//        //m_positionIMUs.Add(18);
//        //m_positionIMUs.Add(20);
//        //m_positionIMUs.Add(24);
//        //m_positionIMUs.Add(26);

//        //m_nameIMUs.Add(new StringBuilder("Pelvis_IMU"));
//        //m_nameIMUs.Add(new StringBuilder("Head_IMU"));
//        //m_nameIMUs.Add(new StringBuilder("RightUpperLeg_IMU"));
//        //m_nameIMUs.Add(new StringBuilder("RightLowerLeg_IMU"));
//        //m_nameIMUs.Add(new StringBuilder("RightUpperArm_IMU"));
//        //m_nameIMUs.Add(new StringBuilder("LeftUpperArm_IMU"));

//        ////Name of the segment associated with the IMUs
//        //m_parentIMUs.Add(new StringBuilder("Pelvis"));
//        //m_parentIMUs.Add(new StringBuilder("Head"));
//        //m_parentIMUs.Add(new StringBuilder("RightThigh"));
//        //m_parentIMUs.Add(new StringBuilder("RightLeg"));
//        //m_parentIMUs.Add(new StringBuilder("RightArm"));
//        //m_parentIMUs.Add(new StringBuilder("LeftArm"));

//        ////Index of the parent
//        //m_parentIMUsIdx.Add(0);
//        //m_parentIMUsIdx.Add(2);
//        //m_parentIMUsIdx.Add(3);
//        //m_parentIMUsIdx.Add(4);
//        //m_parentIMUsIdx.Add(9);
//        //m_parentIMUsIdx.Add(12);

//        ////idex of the marker
//        //m_positionIMUs.Add(0);
//        //m_positionIMUs.Add(1);
//        //m_positionIMUs.Add(2);
//        //m_positionIMUs.Add(3);
//        //m_positionIMUs.Add(4);
//        //m_positionIMUs.Add(5);
//    }


//    public bool isLoaded() {
//		return m_isloaded;
//	}
//    List<double[]> reorientIMUtoBodyVerticalAxisXsens(IntPtr model, List<double[]> allIMUs)
//    {
//        // Recueillir les JCS à la position 0 du pelvis
//#if BIORBD
//        int nQTemplate = MainParameters.c_nQ(model);
//#else
//        int nQTemplate = c_nQ(model);
//#endif
//        double[] Q = new double[nQTemplate];
//        for (int i = 0; i < nQTemplate; ++i)
//            Q[i] = 0;

//        //System.IO.File.AppendAllText(@"C:\Devel\AcroVR_S2M_Debug.txt", string.Format("nQTemplate: {0}{1}", nQTemplate, System.Environment.NewLine));     // Debug Marcel
//        //System.IO.File.AppendAllText(@"C:\Devel\AcroVR_S2M_Debug.txt", string.Format("Q:{0}", System.Environment.NewLine));					// Début Debug Marcel
//        //string line1 = "";
//        //for (int i = 0; i < Q.Length; i++)
//        //	line1 += string.Format("{0}, ", Q[i]);
//        //System.IO.File.AppendAllText(@"C:\Devel\AcroVR_S2M_Debug.txt", string.Format("{0}{1}", line1, System.Environment.NewLine));			// Fin Debug Marcel

//        IntPtr ptr_Q = Marshal.AllocCoTaskMem(sizeof(double) * nQTemplate);
//        Marshal.Copy(Q, 0, ptr_Q, nQTemplate);
//        IntPtr ptrJCS = Marshal.AllocCoTaskMem(sizeof(double) * 4 * 4 * nbImuToConnect); //15= nombre de segment au total dans le model biorbd != nombre total d'imu
//#if BIORBD
//        MainParameters.c_globalJCS(model, ptr_Q, ptrJCS);
//#else
//        c_globalJCS(model, ptr_Q, ptrJCS);
//#endif
//        double[] pelvisJCS = new double[16];
//        Marshal.Copy(ptrJCS, pelvisJCS, 0, 16);
//        Marshal.FreeCoTaskMem(ptr_Q);

//        //System.IO.File.AppendAllText(@"C:\Devel\AcroVR_S2M_Debug.txt", string.Format("pelvisJCS:{0}", System.Environment.NewLine));             // Debug Marcel (Début)
//        //for (int i = 0; i < 4; i++)
//        //{
//        //    string line = string.Format("");
//        //    for (int j = 0; j < 4; j++)
//        //        line += string.Format("{0}, ", pelvisJCS[i * 4 + j]);
//        //    System.IO.File.AppendAllText(@"C:\Devel\AcroVR_S2M_Debug.txt", string.Format("{0}{1}", line, System.Environment.NewLine));
//        //}                                                                                                                                   // Debug Marcel (Fin)

//        // Récupérer le imu pelvis
//        double[] pelvisIMU = allIMUs[0];

//        System.IO.File.AppendAllText(@"C:\Devel\AcroVR_S2M_Debug.txt", string.Format("PelvisIMU:{0}", System.Environment.NewLine));             // Debug Marcel (Début)
//        for (int i = 0; i < pelvisIMU.Length / 4; i++)
//        {
//            string line = "";
//            for (int j = 0; j < 4; j++)
//                line += string.Format("{0}, ", pelvisIMU[i * 4 + j]);
//            System.IO.File.AppendAllText(@"C:\Devel\AcroVR_S2M_Debug.txt", string.Format("{0}{1}", line, System.Environment.NewLine));
//        }                                                                                                                                   // Debug Marcel (Fin)

//        // Trouver quel axe est à peu près déjà aligné avec l'axe vertical (colonne 3) du pelvis
//        double max = -1; // valeur maximale du dot product
//        int idxMax = -1;
//        for (int i = 0; i < 3; ++i)
//        {
//            double[] v1 = new double[3];
//            Array.Copy(pelvisIMU, i * 4, v1, 0, 3);
//            double[] v2 = new double[3];
//            Array.Copy(pelvisJCS, 8, v2, 0, 3);
//            double dotProd = Math.Abs(dot(v1, v2));
//            if (dotProd > max)
//            {
//                max = dotProd;
//                idxMax = i;
//            }
//        }

//        System.IO.File.AppendAllText(@"C:\Devel\AcroVR_S2M_Debug.txt", string.Format("idxMax, max: {0}, {1}{2}", idxMax, max, System.Environment.NewLine));     // Debug Marcel
//        pelvisJCS = new double[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };

//        System.IO.File.AppendAllText(@"C:\Devel\AcroVR_S2M_Debug.txt", string.Format("pelvisJCS:{0}", System.Environment.NewLine));             // Debug Marcel (Début)
//        for (int i = 0; i < 4; i++)
//        {
//            string line = string.Format("");
//            for (int j = 0; j < 4; j++)
//                line += string.Format("{0}, ", pelvisJCS[i * 4 + j]);
//            System.IO.File.AppendAllText(@"C:\Devel\AcroVR_S2M_Debug.txt", string.Format("{0}{1}", line, System.Environment.NewLine));
//        }                                                                                                                                   // Debug Marcel (Fin)

//        Marshal.Copy(ptrJCS, pelvisJCS, 0, 16);
//        idxMax = 2;

//        // Faire l'optimisation pour aligner les axes
//        // ptrJCS est trop grand (tous les segments) mais comme on s'intéresse au premier, c'est correct
//        IntPtr ptrPelvisIMU = Marshal.AllocCoTaskMem(sizeof(double) * 16);
//        Marshal.Copy(pelvisIMU, 0, ptrPelvisIMU, 16);
//        IntPtr ptrMatRot = Marshal.AllocCoTaskMem(sizeof(double) * 16);
//#if BIORBD
//        MainParameters.c_alignSpecificAxisWithParentVertical(ptrJCS, ptrPelvisIMU, idxMax, ptrMatRot);
//#else
//        c_alignSpecificAxisWithParentVertical(ptrJCS, ptrPelvisIMU, idxMax, ptrMatRot);
//#endif

//        Marshal.FreeCoTaskMem(ptrPelvisIMU);
//        Marshal.FreeCoTaskMem(ptrJCS);

//        // Appliquer la rotation à chaque segment
//        double[] matRot = new double[16];
//        Marshal.Copy(ptrMatRot, matRot, 0, 16);

//        System.IO.File.AppendAllText(@"C:\Devel\AcroVR_S2M_Debug.txt", string.Format("matRot:{0}", System.Environment.NewLine));             // Debug Marcel (Début)
//        for (int i = 0; i < matRot.Length / 4; i++)
//        {
//            string line = "";
//            for (int j = 0; j < 4; j++)
//                line += string.Format("{0}, ", matRot[i * 4 + j]);
//            System.IO.File.AppendAllText(@"C:\Devel\AcroVR_S2M_Debug.txt", string.Format("{0}{1}", line, System.Environment.NewLine));
//        }                                                                                                                                   // Debug Marcel (Fin)

//        List<double[]> allIMUsRotated = new List<double[]>();
//        IntPtr ptrM2 = Marshal.AllocCoTaskMem(sizeof(double) * 16);
//        IntPtr ptr_newRotation = Marshal.AllocCoTaskMem(sizeof(double) * 16);
//        foreach (double[] toRotate in allIMUs)
//        {
//            Marshal.Copy(toRotate, 0, ptrM2, 16);
//#if BIORBD
//            MainParameters.c_matrixMultiplication(ptrMatRot, ptrM2, ptr_newRotation);
//#else
//            c_matrixMultiplication(ptrMatRot, ptrM2, ptr_newRotation);
//#endif
//            double[] newOrientation = new double[16];
//            Marshal.Copy(ptr_newRotation, newOrientation, 0, 16);
//            allIMUsRotated.Add(newOrientation);
//        }
//        Marshal.FreeCoTaskMem(ptrM2);
//        Marshal.FreeCoTaskMem(ptr_newRotation);
//        Marshal.FreeCoTaskMem(ptrMatRot);

//        // Retourner l'orientation tournée
//        return allIMUsRotated;

//    }

//    public void WriteModelInFile()
//	{
//#if BIORBD
//		MainParameters.c_writeBiorbdModel(m_s2m, new StringBuilder(@"C:\Devel\Model_Debug2.s2mMod"));
//#else
//		c_writeS2mMusculoSkeletalModel(m_s2m, new StringBuilder(@"C:\Devel\Model_Debug2.s2mMod"));
//#endif
//	}

//	// Use this for initialization
//	public void LoadModel(StringBuilder pathToModel)
//	{
//		// Si un modèle est déjà loadé, le fermer et ouvrir le nouveau
//		if (m_isloaded)
//			OnDestroy();

//		if (pathToModel.ToString ().CompareTo ("") == 0)
//			return;

//		// Load et préparation du modèle musculoskelettique
//#if BIORBD
//		//m_s2m = MainParameters.c_biorbdModel(pathToModel);                      // Fonction #12
//		m_nQ = MainParameters.c_nQ(m_s2m);
//		m_nQdot = MainParameters.c_nQDot(m_s2m);
//		m_nQddot = MainParameters.c_nQDDot(m_s2m);
//		m_nTags = MainParameters.c_nMarkers(m_s2m);
//		MainParameters.c_writeBiorbdModel(m_s2m, new StringBuilder(@"C:\Devel\Model_Debug1.s2mMod"));			// Debug Marcel
//#else
//		//m_s2m = c_s2mMusculoSkeletalModel(pathToModel);
//		m_nQ = c_nQ (m_s2m);
//		m_nQdot = c_nQDot (m_s2m);
//		m_nQddot = c_nQDDot (m_s2m);
//		m_nTags = c_nTags (m_s2m);
//		c_writeS2mMusculoSkeletalModel(m_s2m, new StringBuilder(@"C:\Devel\Model_Debug1.s2mMod"));				// Debug Marcel
//#endif

//		// idem pour Kalman
//		// Initial Guess pour le filtre de kalman
//		IntPtr QinitialGuess = Marshal.AllocCoTaskMem(sizeof(double)*m_nQ);
//		double[] Q = new double[m_nQ];
//		Marshal.Copy(Q, 0, QinitialGuess, m_nQ);

//		if (TestXSens.nMsgS2M < TestXSens.nMsgSize - 2)																			// Debug Marcel (Début)
//		{
//			TestXSens.msgS2M[TestXSens.nMsgS2M] = string.Format("LoadModel #1: processFreq, m_nQ = {0}, {1}", XSensModule.processFreq, m_nQ);
//			TestXSens.nMsgS2M++;
//			TestXSens.msgS2M[TestXSens.nMsgS2M] = string.Format("LoadModel #2: Q = ");
//			for (int i = 0; i < m_nQ; i++)
//				TestXSens.msgS2M[TestXSens.nMsgS2M] += string.Format("{0}, ", Q[i]);
//			TestXSens.nMsgS2M++;
//		}                                                                                                                       // Debug Marcel (Fin)

//		// Préparer le filtre
//#if BIORBD
//		m_kalman = MainParameters.c_BiorbdKalmanReconsIMU(m_s2m, QinitialGuess, XSensModule.processFreq, 0.005f, 1E-5);              // Fonction #13
//		m_nIMUs = MainParameters.c_nIMUs(m_s2m);
//#else
//		m_kalman = c_s2mKalmanReconsIMU(m_s2m, QinitialGuess, XSensModule.processFreq, 0.005f, 1E-5);
//		m_nIMUs = c_nIMUs (m_s2m);
//#endif
//		//double[] kalm = new double[10];
//		//Marshal.Copy(m_kalman, kalm, 0, 10);

//		//if (TestXSens.nMsgS2M < TestXSens.nMsgSize)																				// Debug Marcel (Début)
//		//{
//		//	TestXSens.msgS2M[TestXSens.nMsgS2M] = string.Format("LoadModel #3: kalm = ");
//		//	for (int i = 0; i < 10; i++)
//		//		TestXSens.msgS2M[TestXSens.nMsgS2M] += string.Format("{0}, ", kalm[i]);
//		//	TestXSens.nMsgS2M++;
//		//}                                                                                                                       // Debug Marcel (Fin)

//		Marshal.FreeCoTaskMem (QinitialGuess);

//		// Allouer les pointeurs des coordonnées généralisées
//		m_Q = Marshal.AllocCoTaskMem(sizeof(double)*m_nQ);
//		m_Qdot = Marshal.AllocCoTaskMem(sizeof(double)*m_nQdot);
//		m_Qddot = Marshal.AllocCoTaskMem(sizeof(double)*m_nQddot);

//		// Allouer le pointeur sur les marqueurs
//		m_mark = Marshal.AllocCoTaskMem(sizeof(double)*3*m_nTags); // matrice XYZ * nMark

//		// Allouer le pointeur sur les centrales
//		m_IMU = Marshal.AllocCoTaskMem(sizeof(double)*3*3*m_nIMUs); // matrice 3*3*nIMU

//		// Finaliser
//		m_isloaded = true;
//	}

//	//public void createModelFromStatic(string pathToModel, string pathToTemplate, List<XsMatrix[]> statiqueTrial)
//	//{
//	//	// Trouver le nom et path du statique à générer
//	//	StringBuilder path = new StringBuilder(pathToModel);

//	//	// Load du modèle générique
//	//	string templatePath = pathToTemplate;
//	//	if (!System.IO.File.Exists (templatePath)) {
//	//		Debug.Log ("Template not found");
//	//		return;
//	//	}
//	//	IntPtr modelTemplate = c_s2mMusculoSkeletalModel(new StringBuilder(templatePath));

//	//	// moyenner les centrales
//	//	List<double[]> allIMUsMean = getIMUmean (modelTemplate, statiqueTrial);

//	//	// Trouver l'orientation du tronc (en comparant l'axe vertical de la première centrale avec celle du tronc)
//	//	List<double[]> allIMUsMeanReoriented = reorientIMUtoBodyVerticalAxis(modelTemplate, allIMUsMean);

//	//	// Remettre les IMUs dans le repère local par segment
//	//	List<double[]> allIMUsInLocal = computeIMUsInLocal(modelTemplate, allIMUsMeanReoriented);

//	//	// Include translation to IMU
//	//	List<double[]> allIMUsInLocalWithTrans = addLocalTags(modelTemplate, allIMUsInLocal);

//	//	// Ajouter les imus dans le modèle template
//	//	addIMUtoModel(modelTemplate, allIMUsInLocalWithTrans);

//	//	// Générer le s2mMod
//	//	c_writeS2mMusculoSkeletalModel(modelTemplate, path);

//	//	// Copier les dossiers de bones
//	//	CopyFilesRecursively(new System.IO.DirectoryInfo(Path.GetDirectoryName(pathToTemplate) + "/bones"), new System.IO.DirectoryInfo(Path.GetDirectoryName(pathToModel) + "/bones"));

//	//	// Finaliser
//	//	m_isStaticDone = true;
//	//}

//	public void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
//	{
//		target.Create ();
//		foreach (DirectoryInfo dir in source.GetDirectories())
//			CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
//		foreach (FileInfo file in source.GetFiles()) {
//			if (!File.Exists(Path.Combine (target.FullName, file.Name)))
//				file.CopyTo (Path.Combine (target.FullName, file.Name));
//		}
//	}

//	void addIMUtoModel(IntPtr model, List<double[]> allIMUs)
//	{
//		int idxIMU = 0; 
//		IntPtr imuRT = Marshal.AllocCoTaskMem(sizeof(double)*16); 
//		foreach (double[] imu in allIMUs) {
//			// stocker ou recevoir des matrices d'orientation des centrales inertielles
//			Marshal.Copy(imu, 0, imuRT, 16);

//			if (TestXSens.nMsgS2M < TestXSens.nMsgSize - 2)																			// Debug Marcel (Début)
//			{
//				TestXSens.msgS2M[TestXSens.nMsgS2M] = string.Format("addIMUtoModel #1: idxIMU: {0}", idxIMU);
//				TestXSens.nMsgS2M++;
//				TestXSens.msgS2M[TestXSens.nMsgS2M] = string.Format("addIMUtoModel #2: imu: ");
//				for (int i = 0; i < 16; i++)
//					TestXSens.msgS2M[TestXSens.nMsgS2M] += string.Format("{0}, ", imu[i]);
//				TestXSens.nMsgS2M++;
//			}                                                                                                                       // Debug Marcel (Fin)

//			// Ajouter le IMU dans le modèle
//#if BIORBD
//			MainParameters.c_addIMU(model, imuRT, m_nameIMUs[idxIMU], m_parentIMUs[idxIMU], true, true);                // Fonction #10
//#else
//			c_addIMU(model, imuRT, m_nameIMUs[idxIMU], m_parentIMUs[idxIMU], true, true);
//#endif

//			// Finaliser la boucle
//			idxIMU += 1; 
//		}
//		// Libérer la mémoire qu'il faut libérer
//		Marshal.FreeCoTaskMem (imuRT);
//	}

//	List<double[]> getIMUmean (IntPtr modelTemplate, List<XsMatrix[]> statiqueTrial)
//	{
//		// Get Tags in local coordinates
//#if BIORBD
//		int nTagsTemplate = MainParameters.c_nMarkers(modelTemplate);                           // Fonction #2
//#else
//		int nTagsTemplate = c_nTags (modelTemplate);
//#endif
//		double[] tagsInLocal = new double[3*nTagsTemplate];
//		IntPtr ptrTagsInLocal = Marshal.AllocCoTaskMem (sizeof(double) * 3*nTagsTemplate);
//#if BIORBD
//		MainParameters.c_markersInLocal(modelTemplate, ptrTagsInLocal);                         // Fonction #3
//#else
//		c_TagsInLocal(modelTemplate, ptrTagsInLocal);
//#endif
//		Marshal.Copy(ptrTagsInLocal, tagsInLocal, 0, 3*nTagsTemplate);
//		Marshal.FreeCoTaskMem (ptrTagsInLocal);

//		if (TestXSens.nMsgS2M < TestXSens.nMsgSize)                                                                             // Debug Marcel (Début)
//		{
//			TestXSens.msgS2M[TestXSens.nMsgS2M] = string.Format("getIMUmean #1: {0}", statiqueTrial.Count);
//			TestXSens.nMsgS2M++;
//		}                                                                                                                       // Debug Marcel (Fin)

//		// Faire la moyenne du statique
//		int nMatSize = 16;
//		List<double[]> allIMUsMean = new List<double[]>();
//		for (int nIMU = 0; nIMU < nbImuToConnect; nIMU++)
//		{
//            System.IO.File.AppendAllText(@"C:\Devel\AcroVR_S2M_Debug.txt", string.Format("getIMUmean: nIMU = {0}{1}", nIMU, System.Environment.NewLine));     // Debug Marcel

//            double[] oneIMUoverTime = new double[nMatSize * statiqueTrial.Count];
//#if BIORBD
//			int nFramesNonZero = 0;

//			// Boucle sur tous les frames

//			for (int nFrame = 0; nFrame < statiqueTrial.Count; nFrame++)
//			{
//				double sumIMU = 0;
//				double[] oneIMU = new double[nMatSize];

//				// Copier la partie rotation (3 x 3) de la matrice complète (4 x 4)

//				for (int nRow = 0; nRow < 3; nRow++)
//				{
//					int n = nRow * nMatSize / 4;
//					for (int nCol = 0; nCol < 3; nCol++)
//					{
//						oneIMU[n + nCol] = statiqueTrial[nFrame][nIMU].value((uint)nRow, (uint)nCol);
//						sumIMU += oneIMU[n + nCol];
//					}
//					oneIMU[n + 3] = 0;
//				}

//				// Vérifier que la matrice de rotation est non nulle, si oui alors copier la partie translation et ajouter au vecteur globale contenant tous les IMUs

//				if (sumIMU != 0)
//				{
//					int n = nMatSize * 3 / 4;                                           // Partie translation est la dernière ligne de la matrice complète (4 x 4)
//					for (int i = 0; i < 3; ++i)
//						oneIMU[n + i] = tagsInLocal[m_positionIMUs[nIMU] * 3 + i];
//					oneIMU[nMatSize - 1] = 1;                                           // Dernier élément est 1

//					for (int i = 0; i < nMatSize; i++)
//						oneIMUoverTime[nFramesNonZero * nMatSize + i] = oneIMU[i];
//					nFramesNonZero++;
//				}
//			}

//			// Faire la moyenne

//			IntPtr ptrIMU = Marshal.AllocCoTaskMem(sizeof(double) * nMatSize * nFramesNonZero);
//			IntPtr ptrIMU_mean = Marshal.AllocCoTaskMem(sizeof(double) * nMatSize);
//			Marshal.Copy(oneIMUoverTime, 0, ptrIMU, nMatSize * nFramesNonZero);
//			MainParameters.c_meanRT(ptrIMU, nFramesNonZero, ptrIMU_mean);                   // Fonction #4
//#else
//			for (int t = 0; t<statiqueTrial.Count; ++t)
//			{ // Pour tous les instants
//				uint cmpElement = 0;
//				// Copier la partie rotation
//				for (int i = 0; i < 12; ++i) { // Ne pas process la 4ième colonne
//                    if (i % 4 != 3) { // Si on est pas à la dernière ligne

//                        oneIMUoverTime [t * nMatSize + i] = statiqueTrial[t][nIMU].value (cmpElement / 3, cmpElement % 3); // Dispatch
//                        cmpElement += 1;
//					} else {
//						oneIMUoverTime [t * nMatSize + i] = 0;
//					}
//				}
//				// Copier la partie translation
//				for (int i =0; i<3; ++i){
//					oneIMUoverTime [t * nMatSize + 12 + i] = tagsInLocal [3 * m_positionIMUs[nIMU] + i];
//				}

//				// Dernier élément est un 1
//				oneIMUoverTime [t * nMatSize + 15] = 1;
//			}
//			// Faire la moyenne
//			IntPtr ptrIMU = Marshal.AllocCoTaskMem (sizeof(double) * nMatSize * statiqueTrial.Count);
//			IntPtr ptrIMU_mean = Marshal.AllocCoTaskMem (sizeof(double) * nMatSize);
//			Marshal.Copy(oneIMUoverTime, 0, ptrIMU, nMatSize * statiqueTrial.Count);
//			c_meanIMU(ptrIMU, statiqueTrial.Count, ptrIMU_mean);
//#endif

//			if (TestXSens.nMsgS2M < TestXSens.nMsgSize - statiqueTrial.Count)                                                      // Debug Marcel (Début)
//			{
//				for (int ii = 0; ii < statiqueTrial.Count; ii++)
//				{
//					TestXSens.msgS2M[TestXSens.nMsgS2M] = string.Format("getIMUmean #2: oneIMUoverTime({0}, {1}): ", nIMU, ii);
//					for (int jj = 0; jj < nMatSize; jj++)
//						TestXSens.msgS2M[TestXSens.nMsgS2M] += string.Format("{0}, ", oneIMUoverTime[ii * nMatSize + jj]);
//					TestXSens.nMsgS2M++;
//				}
//			}                                                                                                                       // Debug Marcel (Fin)

//			// Dispatch de la moyenne
//			double[] imuMean = new double[nMatSize];
//			Marshal.Copy(ptrIMU_mean, imuMean, 0, nMatSize);

//			if (TestXSens.nMsgS2M < TestXSens.nMsgSize)                                                                             // Debug Marcel (Début)
//			{
//				TestXSens.msgS2M[TestXSens.nMsgS2M] = string.Format("getIMUmean #3: imuMean({0}): ", nIMU);
//				for (int jj = 0; jj < nMatSize; jj++)
//					TestXSens.msgS2M[TestXSens.nMsgS2M] += string.Format("{0}, ", imuMean[jj]);
//				TestXSens.nMsgS2M++;
//			}                                                                                                                       // Debug Marcel (Fin)

//			allIMUsMean.Add (imuMean);
//			Marshal.FreeCoTaskMem (ptrIMU);
//			Marshal.FreeCoTaskMem (ptrIMU_mean);
//		}
//        System.IO.File.AppendAllText(@"C:\Devel\AcroVR_S2M_Debug.txt", string.Format("getIMUmean: allIMUsMean.count = {0}{1}", allIMUsMean.Count, System.Environment.NewLine));     // Debug Marcel
//        return allIMUsMean;
//	}

//	List<double[]> addLocalTags (IntPtr modelTemplate, List<double[]> allIMUsInLocal)
//	{
//		// Get Tags in local coordinates
//#if BIORBD
//		int nTagsTemplate = MainParameters.c_nMarkers(modelTemplate);                       // Fonction #8
//#else
//		int nTagsTemplate = c_nTags (modelTemplate);
//#endif
//		double[] tagsInLocal = new double[3*nTagsTemplate];
//		IntPtr ptrTagsInLocal = Marshal.AllocCoTaskMem (sizeof(double) * 3*nTagsTemplate);
//#if BIORBD
//		MainParameters.c_markersInLocal(modelTemplate, ptrTagsInLocal);                     // Fonction #9
//#else
//		c_TagsInLocal(modelTemplate, ptrTagsInLocal);
//#endif
//		Marshal.Copy(ptrTagsInLocal, tagsInLocal, 0, 3*nTagsTemplate);
//		Marshal.FreeCoTaskMem(ptrTagsInLocal);

//		// Faire la moyenne du statique
//		List<double[]> imuOut = new List<double[]>();
//		for (int j = 0; j < allIMUsInLocal.Count; ++j) { // Pour tous les IMUs
//			double[] imu = new double[16];
//			for (int i = 0; i<16; ++i) { // Pour tous les éléments de la matrice de rotation 4x4
//				if (i < 12) // Copier la rotation
//					imu [i] = allIMUsInLocal [j] [i];
//				else if (i < 15) // Copier la translation
//					imu[i] = tagsInLocal [3 * m_positionIMUs[j] + i-12];
//				else // Dernier élément est un 1
//					imu[i] = 1;
//			}
//			imuOut.Add (imu);
//		}
//		return imuOut;
//	}

//	//public XsMatrix[] computeIMUsInLocal(XsMatrix[] IMUs)
//	//{
//	//	if (TestXSens.nMsgS2M < TestXSens.nMsgSize - 1)																			// Debug Marcel (Début)
//	//	{
//	//		TestXSens.msgS2M[TestXSens.nMsgS2M] = string.Format("ComputeIMUsInLocal #1");
//	//		TestXSens.nMsgS2M++;
//	//		TestXSens.msgS2M[TestXSens.nMsgS2M] = string.Format("ComputeIMUsInLocal: m_nIMUs = {0}, {1}, {2}, {3}", TestXSens.Instance.nFrame, m_nIMUs, IMUs[0].rows(), IMUs[0].cols());
//	//		TestXSens.nMsgS2M++;
//	//	}                                                                                                                       // Debug Marcel (Fin)

//	//	// Get Tags in local coordinates
//	//	int nTagsTemplate = c_nTags(m_s2m);
//	//	double[] tagsInLocal = new double[3 * nTagsTemplate];
//	//	IntPtr ptrTagsInLocal = Marshal.AllocCoTaskMem(sizeof(double) * 3 * nTagsTemplate);
//	//	c_TagsInLocal(m_s2m, ptrTagsInLocal);
//	//	Marshal.Copy(ptrTagsInLocal, tagsInLocal, 0, 3 * nTagsTemplate);
//	//	Marshal.FreeCoTaskMem(ptrTagsInLocal);

//	//	List<double[]> imus = new List<double[]>();
//	//	for (int i = 0; i < m_nIMUs; i++)
//	//	{
//	//		double[] oneIMU = new double[16];
//	//		for (uint row = 0; row < 4; row++)
//	//		{
//	//			for (uint col = 0; col < 4; col++)
//	//			{
//	//				uint n = row * 4 + col;
//	//				if (row < 3 & col < 3)
//	//					oneIMU[n] = IMUs[i].value(row, col);
//	//				else if (row < 3)
//	//					oneIMU[n] = 0;
//	//				else if (col < 3)
//	//					oneIMU[n] = tagsInLocal[m_positionIMUs[i] * 3 + col];
//	//				else
//	//					oneIMU[n] = 1;
//	//			}
//	//		}
//	//		imus.Add(oneIMU);
//	//	}

//	//	//List<double[]> imusOut = new List<double[]>();
//	//	//IntPtr ptrPelvis = Marshal.AllocCoTaskMem(sizeof(double) * 16);
//	//	//IntPtr ptrSegment = Marshal.AllocCoTaskMem(sizeof(double) * 16);
//	//	//IntPtr ptrSegmentOut = Marshal.AllocCoTaskMem(sizeof(double) * 16);
//	//	//Marshal.Copy(imus[0], 0, ptrPelvis, 16);
//	//	//for (int i = 0; i < imus.Count; ++i)
//	//	//{
//	//	//	Marshal.Copy(imus[i], 0, ptrSegment, 16);
//	//	//	c_matrixMultiplication(ptrPelvis, ptrSegment, ptrSegmentOut);
//	//	//	double[] segmentOut = new double[16];
//	//	//	Marshal.Copy(ptrSegmentOut, segmentOut, 0, 16);
//	//	//	imusOut.Add(segmentOut);
//	//	//}
//	//	//Marshal.FreeCoTaskMem(ptrPelvis);
//	//	//Marshal.FreeCoTaskMem(ptrSegment);
//	//	//Marshal.FreeCoTaskMem(ptrSegmentOut);

//	//	List<double[]> imusOut = computeIMUsInLocal(m_s2m, imus);

//	//	if (TestXSens.nMsgS2M < TestXSens.nMsgSize)																				// Debug Marcel (Début)
//	//	{
//	//		TestXSens.msgS2M[TestXSens.nMsgS2M] = string.Format("ComputeIMUsInLocal: imusOut.Count = {0}", imusOut.Count);
//	//		TestXSens.nMsgS2M++;
//	//	}                                                                                                                       // Debug Marcel (Fin)
//	//	XsMatrix[] IMUsOut = new XsMatrix[imusOut.Count];
//	//	for (int i = 0; i < imusOut.Count; ++i)
//	//	{
//	//		IMUsOut[i] = new XsMatrix(3, 3);
//	//		for (uint row = 0; row < 3; ++row)
//	//			for (uint col = 0; col < 3; ++col)
//	//			{
//	//				//System.IO.File.AppendAllText(@"C:\Devel\AcroVR_S2M_Debug.txt", string.Format("ComputeIMUsInLocal: imusOut = {0}, {1}, {2}, {3}{4}", i, row, col, imusOut[i][4 * col + row], System.Environment.NewLine)); // Debug Marcel
//	//				IMUsOut[i].setValue(row, col, imusOut[i][4 * row + col]);
//	//			}
//	//	}

//	//	return IMUsOut;
//	//}

//	List<double[]> computeIMUsInLocal(IntPtr model, List<double[]> imus)
//	{
//		if (TestXSens.nMsgS2M < TestXSens.nMsgSize)                                                                             // Debug Marcel (Début)
//		{
//			TestXSens.msgS2M[TestXSens.nMsgS2M] = string.Format("ComputeIMUsInLocal #2: {0}", TestXSens.Instance.nFrame);
//			TestXSens.nMsgS2M++;
//		}                                                                                                                       // Debug Marcel (Fin)

//		// Recueillir les JCS à la position 0
//#if BIORBD
//		int nQTemplate = MainParameters.c_nQ(model);                            // Fonction #5
//#else
//		int nQTemplate = c_nQ(model);
//#endif
//		double[] Q = new double[nQTemplate];
//		for (int i = 0; i < nQTemplate; ++i) {
//			Q [i] = 0;
//		}
//		IntPtr ptr_Q = Marshal.AllocCoTaskMem(sizeof(double)*nQTemplate);
//		Marshal.Copy(Q, 0, ptr_Q, nQTemplate);
//		IntPtr ptrJCS = Marshal.AllocCoTaskMem (sizeof(double) * 16 * 15);
//#if BIORBD
//		MainParameters.c_globalJCS(model, ptr_Q, ptrJCS);                       // Fonction #6
//#else
//		c_globalJCS(model, ptr_Q, ptrJCS); 
//#endif
//		double[] JCS = new double[16 * 15];
//		Marshal.Copy(ptrJCS, JCS, 0, 16 * 15);
//		Marshal.FreeCoTaskMem (ptr_Q);
//		Marshal.FreeCoTaskMem (ptrJCS);

//		if (TestXSens.nMsgS2M < TestXSens.nMsgSize)                                                                             // Debug Marcel (Début)
//		{
//			TestXSens.msgS2M[TestXSens.nMsgS2M] = string.Format("ComputeIMUsInLocal: nQTemplate = {0}", nQTemplate);
//			TestXSens.nMsgS2M++;
//		}                                                                                                                       // Debug Marcel (Fin)

//		// Variable de sortie
//		List<double[]> outIMU = new List<double[]>();

//		// Projeter dans les repères locaux
//		IntPtr ptr_imuToRebase = Marshal.AllocCoTaskMem (sizeof(double) * 16);
//		IntPtr ptr_parentSeg = Marshal.AllocCoTaskMem (sizeof(double) * 16);
//		IntPtr ptr_rotatedIMU = Marshal.AllocCoTaskMem (sizeof(double) * 16);
//		for (int i = 0; i<imus.Count; ++i)
//		{
//			if (TestXSens.nMsgS2M < TestXSens.nMsgSize)                                                                             // Debug Marcel (Début)
//			{
//				TestXSens.msgS2M[TestXSens.nMsgS2M] = string.Format("ComputeIMUsInLocal: m_parentIMUsIdx[{0}] = {1}", i, m_parentIMUsIdx[i]);
//				TestXSens.nMsgS2M++;
//			}                                                                                                                       // Debug Marcel (Fin)

//			if (TestXSens.nMsgS2M < TestXSens.nMsgSize - 4)                                                                         // Debug Marcel (Début)
//			{
//				TestXSens.msgS2M[TestXSens.nMsgS2M] = string.Format("ComputeIMUsInLocal: imus({0}):", i);
//				TestXSens.nMsgS2M++;
//				for (int j = 0; j < 4; j++)
//				{
//					TestXSens.msgS2M[TestXSens.nMsgS2M] = string.Format("");
//					for (int k = 0; k < 4; k++)
//						TestXSens.msgS2M[TestXSens.nMsgS2M] += string.Format("{0}, ", imus[i][k * 4 + j]);
//					TestXSens.nMsgS2M++;
//				}
//			}                                                                                                                       // Debug Marcel (Fin)

//			if (TestXSens.nMsgS2M < TestXSens.nMsgSize - 4)                                                                         // Debug Marcel (Début)
//			{
//				TestXSens.msgS2M[TestXSens.nMsgS2M] = string.Format("ComputeIMUsInLocal: JCS({0}):", i);
//				TestXSens.nMsgS2M++;
//				for (int j = 0; j < 4; j++)
//				{
//					TestXSens.msgS2M[TestXSens.nMsgS2M] = string.Format("");
//					for (int k = 0; k < 4; k++)
//						TestXSens.msgS2M[TestXSens.nMsgS2M] += string.Format("{0}, ", JCS[i * 16 + k * 4 + j]);
//					TestXSens.nMsgS2M++;
//				}
//			}                                                                                                                       // Debug Marcel (Fin)

//			Marshal.Copy(imus[i], 0, ptr_imuToRebase, 16);
//			Marshal.Copy(JCS, m_parentIMUsIdx[i]*16, ptr_parentSeg, 16);
//#if BIORBD
//			MainParameters.c_projectJCSinParentBaseCoordinate(ptr_parentSeg, ptr_imuToRebase, ptr_rotatedIMU);              // Fonction #7
//#else
//			c_projectJCSinParentBaseCoordinate(ptr_parentSeg, ptr_imuToRebase, ptr_rotatedIMU);
//#endif
//			double[] rotatedIMU = new double[16];
//			Marshal.Copy(ptr_rotatedIMU, rotatedIMU, 0, 16);

//			if (TestXSens.nMsgS2M < TestXSens.nMsgSize - 4)                                                                         // Debug Marcel (Début)
//			{
//				TestXSens.msgS2M[TestXSens.nMsgS2M] = string.Format("ComputeIMUsInLocal: rotatedIMU({0}):", i);
//				TestXSens.nMsgS2M++;
//				for (int j = 0; j < 4; j++)
//				{
//					TestXSens.msgS2M[TestXSens.nMsgS2M] = string.Format("");
//					for (int k = 0; k < 4; k++)
//						TestXSens.msgS2M[TestXSens.nMsgS2M] += string.Format("{0}, ", rotatedIMU[k * 4 + j]);
//					TestXSens.nMsgS2M++;
//				}
//			}                                                                                                                       // Debug Marcel (Fin)

//			outIMU.Add(rotatedIMU);
//		}
//		Marshal.FreeCoTaskMem (ptr_rotatedIMU);
//		Marshal.FreeCoTaskMem (ptr_imuToRebase);
//		Marshal.FreeCoTaskMem(ptr_parentSeg);

//		return outIMU;
//	}

//	/// <summary>
//	/// Dot product of the specified v1 and v2.
//	/// </summary>
//	/// <param name="v1">V1.</param>
//	/// <param name="v2">V2.</param>
//	double dot(double[] v1, double[] v2)
//	{
//		return v1 [0] * v2 [0] + v1 [1] * v2 [1] + v1 [2] * v2 [2];
//	}

//	public void OnDestroy()
//	{
//		// Libérer la mémoire sur les pointeurs unmanaged
//		Marshal.FreeCoTaskMem (m_Q);
//		Marshal.FreeCoTaskMem (m_Qdot);
//		Marshal.FreeCoTaskMem (m_Qddot);

//		Marshal.FreeCoTaskMem (m_mark);

//		Marshal.FreeCoTaskMem (m_IMU);

//		// Libérer la mémoire des fonctions c
//		if (m_isloaded) {
//#if BIORBD
//			MainParameters.c_deleteBiorbdKalmanReconsIMU(m_kalman);
//			MainParameters.c_deleteBiorbdModel(m_s2m);
//#else
//			c_deleteS2mKalmanReconsIMU(m_kalman);
//			c_deleteS2mMusculoSkeletalModel(m_s2m);
//#endif
//		}
//	}

//	public double[] KalmanReconsIMU (XsMatrix[] IMUs)
//	{
//		// Préparer les données d'entrée (orientation des IMUs)

//		double[] imu = new double[9 * m_nIMUs]; // Matrice 3*3*nIMUs
//        for (int i = 0; i < m_nIMUs; ++i)
//            for (uint row = 0; row < 3; ++row)
//                for (uint col = 0; col < 3; ++col)
//                    if (IMUs[i] == null)
//                        imu[9 * i + 3 * row +col] = 0;
//                    else
//                        imu[9 * i + 3 * row + col] = IMUs[i].value(col, row);
//		Marshal.Copy(imu, 0, m_IMU, 9 * m_nIMUs);

//		if (TestXSens.nMsgS2M < TestXSens.nMsgSize - m_nIMUs)                                                                  // Debug Marcel (Début)
//		{
//			for (int i = 0; i < m_nIMUs; i++)
//			{
//				TestXSens.msgS2M[TestXSens.nMsgS2M] = string.Format("KalmanReconsIMU1: imu({0}): ", i);
//				for (int j = 0; j < 9; j++)
//					TestXSens.msgS2M[TestXSens.nMsgS2M] += string.Format("{0}, ", imu[i * 9 + j]);
//				TestXSens.nMsgS2M++;
//			}
//		}                                                                                                                       // Debug Marcel (Fin)

//#if BIORBD
//		int nQTemplate = MainParameters.c_nQ(m_s2m);                            // Fonction #14
//#else
//		int nQTemplate = c_nQ(m_s2m);
//#endif
//		if (TestXSens.nMsgS2M < TestXSens.nMsgSize)                                                                             // Debug Marcel (Début)
//		{
//			TestXSens.msgS2M[TestXSens.nMsgS2M] = string.Format("KalmanReconsIMU2: nQTemplate, nbImuToConnect, m_nIMUs = {0}, {1}, {2}: ", nQTemplate, nbImuToConnect, m_nIMUs);
//			TestXSens.nMsgS2M++;
//		}                                                                                                                       // Debug Marcel (Fin)

//        //		double[] Q1 = new double[nQTemplate];
//        //		for (int i = 0; i < nQTemplate; ++i)
//        //			Q1[i] = 0;
//        //		IntPtr ptr_Q = Marshal.AllocCoTaskMem(sizeof(double) * nQTemplate);
//        //		Marshal.Copy(Q1, 0, ptr_Q, nQTemplate);
//        //		IntPtr ptrJCS = Marshal.AllocCoTaskMem(sizeof(double) * 16 * m_nIMUs);
//        //		double[] jcs = new double[16 * m_nIMUs];
//        //#if BIORBD
//        //		MainParameters.c_globalJCS(m_s2m, ptr_Q, ptrJCS);                   // Fonction #15
//        //		Marshal.Copy(ptrJCS, jcs, 0, 16 * m_nIMUs);
//        //		//double[] jcs1 = new double[16 * m_nIMUs];
//        //		//Marshal.Copy(ptrJCS, jcs1, 0, 16 * m_nIMUs);
//        //		//for (int i = 0; i < m_nIMUs; i++)
//        //		//{
//        //		//	int ii = i * 16;
//        //		//	jcs[ii + 1] = jcs1[ii + 4];
//        //		//	jcs[ii + 2] = jcs1[ii + 8];
//        //		//	jcs[ii + 4] = jcs1[ii + 1];
//        //		//	jcs[ii + 6] = jcs1[ii + 9];
//        //		//	jcs[ii + 8] = jcs1[ii + 2];
//        //		//	jcs[ii + 9] = jcs1[ii + 6];
//        //		//}
//        //#else
//        //		c_globalJCS(m_s2m, ptr_Q, ptrJCS);
//        //		Marshal.Copy(ptrJCS, jcs, 0, 16 * m_nIMUs);
//        //#endif
//        //		Marshal.FreeCoTaskMem(ptr_Q);
//        //		Marshal.FreeCoTaskMem(ptrJCS);

//        //if (TestXSens.nMsgS2M < TestXSens.nMsgSize - m_nIMUs)																	// Debug Marcel (Début)
//        //{
//        //	for (int i = 0; i < m_nIMUs; i++)
//        //	{
//        //		TestXSens.msgS2M[TestXSens.nMsgS2M] = string.Format("KalmanReconsIMU3: JCS({0}): ", i);
//        //		for (int j = 0; j < 16; j++)
//        //			TestXSens.msgS2M[TestXSens.nMsgS2M] += string.Format("{0}, ", jcs[i * 16 + j]);
//        //		TestXSens.nMsgS2M++;
//        //	}
//        //}                                                                                                                       // Debug Marcel (Fin)

//        // Appeler la fonction de reconstruction

//        //IntPtr m_s2mTemp;
//#if BIORBD
//        //m_s2mTemp = MainParameters.c_biorbdModel(new System.Text.StringBuilder(string.Format(@"{0}\ModelTestBenjamin.s2mMod", UnityEngine.Application.streamingAssetsPath)));
//        //MainParameters.c_BiorbdKalmanReconsIMUstep(m_s2mTemp, m_kalman, m_IMU, m_Q, m_Qdot, m_Qddot);
//        //MainParameters.c_deleteBiorbdModel(m_s2mTemp);

//        MainParameters.c_BiorbdKalmanReconsIMUstep(m_s2m, m_kalman, m_IMU, m_Q, m_Qdot, m_Qddot);                   // Fonction #16
//#else
//        //m_s2mTemp = c_s2mMusculoSkeletalModel(new System.Text.StringBuilder(string.Format(@"{0}\ModelTestBenjamin.s2mMod", UnityEngine.Application.streamingAssetsPath)));
//        //c_s2mKalmanReconsIMUstep(m_s2mTemp, m_kalman, m_IMU, m_Q, m_Qdot, m_Qddot);
//        //c_deleteS2mMusculoSkeletalModel(m_s2mTemp);

//        //string fileN2 = string.Format(@"C:\Devel\AcroVR_S2M.txt");
//        //System.IO.File.Delete(fileN2);
//        //for (int i = 0; i < TestXSens.nMsgS2M; i++)
//        //    System.IO.File.AppendAllText(fileN2, string.Format("{0}{1}", TestXSens.msgS2M[i], System.Environment.NewLine));

//        c_s2mKalmanReconsIMUstep(m_s2m, m_kalman, m_IMU, m_Q, m_Qdot, m_Qddot);
//#endif

//		// Récupérer les données de sortie
//		Q = new double[m_nQ];
//        Qdot = new double[m_nQdot];
//        Qddot = new double[m_nQddot];
//		Marshal.Copy(m_Q, Q, 0, m_nQ);
//        Marshal.Copy(m_Qdot, Qdot, 0, m_nQdot);
//        Marshal.Copy(m_Qddot, Qddot, 0, m_nQddot);

//		if (TestXSens.nMsgS2M < TestXSens.nMsgSize - m_nQ * 3)																		// Debug Marcel (Début)
//		{
//			TestXSens.msgS2M[TestXSens.nMsgS2M] = string.Format("KalmanReconsIMU4: Q: ");
//			for (int i = 0; i < m_nQ; i++)
//				TestXSens.msgS2M[TestXSens.nMsgS2M] += string.Format("{0}, ", Q[i]);
//			TestXSens.nMsgS2M++;
//			TestXSens.msgS2M[TestXSens.nMsgS2M] = string.Format("KalmanReconsIMU5: Qdot: ");
//			for (int i = 0; i < m_nQ; i++)
//				TestXSens.msgS2M[TestXSens.nMsgS2M] += string.Format("{0}, ", Qdot[i]);
//			TestXSens.nMsgS2M++;
//			TestXSens.msgS2M[TestXSens.nMsgS2M] = string.Format("KalmanReconsIMU6: Qddot: ");
//			for (int i = 0; i < m_nQ; i++)
//				TestXSens.msgS2M[TestXSens.nMsgS2M] += string.Format("{0}, ", Qddot[i]);
//			TestXSens.nMsgS2M++;
//		}                                                                                                                       // Debug Marcel (Fin)

//		return Q;
//	}

//    public double[] getQ()
//    {
//        return Q;
//    }

//    public double[] getQdot()
//    {
//        return Qdot;
//    }

//    public double[] getQddot()
//    {
//        return Qddot;
//    }

// //   public double[] Tags()
//	//{
//	//	// Préparer les données d'entrée (coordonnées généralisées)
//	//	double[] Q = new double[m_nQ];
//	//	for (int i = 0; i < m_nQ; ++i) {
//	//		Q [i] = 0;
//	//	}
//	//	Marshal.Copy(Q, 0, m_Q, m_nQ);

//	//	// Appel de la fonction Tags
//	//	c_Tags(m_s2m, m_Q, m_mark);

//	//	// Parsing du résultat
//	//	double[] mark = new double[3 * m_nTags];
//	//	Marshal.Copy(m_mark, mark, 0, 3*m_nTags);    // Copy result to array.

//	//	return mark;
//	//}

//	// Accessor
//	public bool isStaticDone()
//	{
//		return m_isStaticDone;
//	}

//	//public double rsh()
//	//{
//	//	// m_Q est un pointeur qu'on a utilisé lors du dernier appel de Kalman, il possède donc le dernier Q en liste
//	//	double[] QNoScap = new double[m_nQ];
//	//	Marshal.Copy(m_Q, QNoScap, 0, m_nQ);
//	//	for (int i = 6; i<9; ++i) // Retirer la scapula
//	//		QNoScap [i] = 0; 
//	//	IntPtr ptrQNoScap = Marshal.AllocCoTaskMem (sizeof(double) * m_nQ);
//	//	Marshal.Copy(QNoScap, 0, ptrQNoScap, m_nQ);

//	//	// Trouver GL quand scap est là et quand il n'y est pas
//	//	double[] allJCS = new double[16*3];
//	//	double[] allJCSnoScap = new double[16*3];
//	//	IntPtr ptrJCS = Marshal.AllocCoTaskMem (sizeof(double) * 4 * 4 * nbImuToConnect);
//	//	c_globalJCS (m_s2m, m_Q, ptrJCS);
//	//	Marshal.Copy(ptrJCS, allJCS,  0, 16*3); // Thorax est le 1er segment, Scapula 2ième, Humerus 3ième
//	//	c_globalJCS (m_s2m, ptrQNoScap, ptrJCS);
//	//	Marshal.Copy(ptrJCS, allJCSnoScap,  0, 16*3); // Thorax est le 1er segment, Scapula 2ième, Humerus 3ième
//	//	Marshal.FreeCoTaskMem (ptrQNoScap);

//	//	// Calculer TH
//	//	double[] th_mat = new double[16];
//	//	double[] thNoScap_mat = new double[16];
//	//	IntPtr ptrThoraxJCS = Marshal.AllocCoTaskMem (sizeof(double) * 4 * 4);
//	//	IntPtr ptrHumJCS = Marshal.AllocCoTaskMem (sizeof(double) * 4 * 4);
//	//	Marshal.Copy(allJCS, 16*0, ptrThoraxJCS, 16); // 0 est le thorax
//	//	Marshal.Copy(allJCS, 16*2, ptrHumJCS, 16); // 2 est l'humérus
//	//	c_projectJCSinParentBaseCoordinate(ptrThoraxJCS, ptrHumJCS, ptrJCS);
//	//	Marshal.Copy(ptrJCS, th_mat,  0, 16); 
//	//	Marshal.Copy(allJCSnoScap, 16*2, ptrHumJCS, 16); // 2 est l'humérus
//	//	c_projectJCSinParentBaseCoordinate(ptrThoraxJCS, ptrHumJCS, ptrJCS);
//	//	Marshal.Copy(ptrJCS, thNoScap_mat,  0, 16); 
//	//	Marshal.FreeCoTaskMem (ptrJCS);
//	//	Marshal.FreeCoTaskMem (ptrHumJCS);
//	//	Marshal.FreeCoTaskMem (ptrThoraxJCS);

//	//	double th = extractElevationFromMatriceRotation (th_mat);
//	//	double th_noScap = extractElevationFromMatriceRotation (thNoScap_mat);

//	//	double rsh = (th-th_noScap) / th_noScap;
//	//	return rsh;
//	//}

//	/// <summary>
//	/// Extracts the elevation angle (Y) from matrix of rotation assuming ZYZ sequence.
//	/// </summary>
//	/// <returns>The elevation angle from matrix of rotation.</returns>
//	/// <param name="matrice">Matrix of rotation.</param>
//	//double extractElevationFromMatriceRotation(double[] matrice)
//	//{
//	//	// Arccos de l'élément 3,3 de la matrice de rotation est l'élévation dans une séquence zyz
//	//	double[] angles = new double[3];
//	//	IntPtr ptrAngles = Marshal.AllocCoTaskMem (sizeof(double) * 3);

//	//	// Appliquer la rotation à chaque segment
//	//	IntPtr ptrMatrice = Marshal.AllocCoTaskMem (sizeof(double) * 4 * 4);
//	//	Marshal.Copy(matrice, 0, ptrMatrice, 16);
//	//	StringBuilder sequence = new StringBuilder("zyz");

//	//	// Appeler la fonction principale
//	//	c_transformMatrixToCardan(ptrMatrice, sequence, ptrAngles);

//	//	Marshal.Copy(ptrAngles, angles,  0, 3);

//	//	return angles [2];
//	//}

//	//public double[] getCardanTransformationFromParentToSegment(int segIdx)
//	//{
//	//	double[] rtInCard = new double[6]; // 3 translations, 3 rotations

//	//	// Local coordinate system
//	//	double[] lcs = new double[16];
//	//	IntPtr ptrLcs = Marshal.AllocCoTaskMem (sizeof(double) * 16);
//	//	c_localJCS (m_s2m, segIdx, ptrLcs);
//	//	Marshal.Copy(ptrLcs, lcs,  0, 16); 
//	//	rtInCard [0] = lcs [12]; // Prendre les translations
//	//	rtInCard [1] = lcs [13];
//	//	rtInCard [2] = lcs [14];

//	//	// Convertir en angle de cardan
//	//	double[] cardan = new double[3];
//	//	IntPtr ptrCardanOut = Marshal.AllocCoTaskMem (sizeof(double) * 3);
//	//	c_transformMatrixToCardan (ptrLcs, new StringBuilder("xyz"), ptrCardanOut);
//	//	Marshal.Copy(ptrCardanOut, cardan,  0, 3); 
//	//	rtInCard [3] = cardan [0]; // Prendre les rotations
//	//	rtInCard [4] = cardan [1];
//	//	rtInCard [5] = cardan [2];

//	//	// Retourner la réponse
//	//	return rtInCard; // Transfomer en système main gauche
//	//}
//}
