using System;
using UnityEngine;

// =================================================================================================================================================================
/// <summary> Script utilisé pour faire bouger l'avatar. </summary>

public class AvatarManager3Segments : AvatarManager
{
	// Moving Joints
	public GameObject hips;
	public GameObject leftUpperLimb;
	public GameObject rightUpperLimb;
	public float[] AnglesAvatar { get; protected set; }

	protected new void Start()
	{
		base.Start();
		AnglesAvatar = new float[Model.NbQ];
	}

	protected override string BiomodPath()
    {
		return @"Assets/Avatar/Biorbd/model3Segments.bioMod";
	}

	void Update()
	{
        GetCurrentData();
		if (CurrentData == null)
        {
			return;
        }

		Debug.Log($"Time frame : {CurrentData.TimeIndex}");
		for (int i=0; i < CurrentData.NbSensorsSet; ++i)
        {
			var euler = CurrentData.OrientationEuler[i];
			Debug.Log($"\tSensor {i}: ({euler.x()}, {euler.y()}, {euler.z()}");
        }
        //SetSegmentsRotations(_data);
    }

	public override void SetSegmentsRotations(double[] q)
	{
		ConvertToAngles(q);

		PerformRotation(hips, new Vector3(AnglesAvatar[0], AnglesAvatar[1], AnglesAvatar[2]));
		PerformRotation(leftUpperLimb, new Vector3(AnglesAvatar[6], AnglesAvatar[7], AnglesAvatar[8]));
		PerformRotation(rightUpperLimb, new Vector3(AnglesAvatar[3], AnglesAvatar[4], AnglesAvatar[5]));
	}

	void ConvertToAngles(double[] q)
	{
		// qAvatar = [q0, -q1, -q2, q3, -q4, -q5, q6, -q7, -q8].
		AnglesAvatar[0] = (float)MathUtils.ToDegree(q[0]);
		AnglesAvatar[1] = (float)MathUtils.ToDegree(q[1]);
		AnglesAvatar[2] = (float)MathUtils.ToDegree(q[2]);
		AnglesAvatar[3] = (float)MathUtils.ToDegree(q[3]);
		AnglesAvatar[4] = (float)MathUtils.ToDegree(q[4]);
		AnglesAvatar[5] = (float)MathUtils.ToDegree(q[5]);
		AnglesAvatar[6] = (float)MathUtils.ToDegree(q[6]);
		AnglesAvatar[7] = (float)MathUtils.ToDegree(q[7]);
		AnglesAvatar[8] = (float)MathUtils.ToDegree(q[8]);
    }



	///// <summary>
	///// Writes the trial.
	///// </summary>
	//public void writeTrialToBinary(string filepath)
	//{
	//	DirectoryInfo dirpath = new System.IO.DirectoryInfo(Path.GetDirectoryName(filepath));
	//	if (dirpath.Exists == false)
	//	{
	//		dirpath.Create();

	//		// S'il n'existe pas encore, c'est qu'il y a une erreur, ne pas écrire rien
	//		dirpath = new System.IO.DirectoryInfo(Path.GetDirectoryName(filepath));
	//		if (dirpath.Exists == false)
	//		{
	//			Debug.Log("Le dossier n'a pu être créé, le fichier ne sera pas enregistré");
	//			return;
	//		}
	//	}

	//	double version = 1.0;
	//	long nbFrame = nFrame();
	//	long nCentrale = (long)XSensModule.nIMUtoConnect;
	//	long nElementsInMatrix = (long)9; // matrice 3x3

	//	// Enregistrer l'essai dans un fichier
	//	using (BinaryWriter writer = new BinaryWriter(File.Open(filepath, FileMode.Create)))
	//	{
	//		// Enregistrer des éléments pour permettre d'aider à réouvrir plus tard
	//		writer.Write(version);
	//		writer.Write(m_modelPath);
	//		writer.Write(nbFrame);
	//		writer.Write(nCentrale);
	//		writer.Write(nElementsInMatrix);

	//		for (int i = 0; i < nbFrame; ++i)
	//		{ // Pour chaque frame
	//			writer.Write(getTimeStamp(i)); // Enregistrer le time stamp
	//			for (int j = 0; j < nCentrale; ++j)
	//			{ // Pour chaque centrale
	//				for (uint k = 0; k < nElementsInMatrix; ++k)
	//				{ // Pour chaque élément de la matrice de rotation
	//					writer.Write(getData(i)[j].value(k % 3, k / 3));
	//				}
	//			}
	//		}
	//	}
	//}


	///// <summary>
	///// Reads a trial.
	///// </summary>
	//public void readTrialFromBinary(string path)
	//{
	//	Clear();

	//	// Enregistrer l'essai dans un fichier
	//	using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
	//	{
	//		// Lire des éléments pour permettre d'avoir les dimensions
	//		double version = reader.ReadDouble();
	//		m_modelPath = reader.ReadString();
	//		long nFrame = reader.ReadInt64();
	//		long nCentrale = reader.ReadInt64();
	//		long nElementsInMatrix = reader.ReadInt64();

	//		// Lire ce que l'on sait sur l'essai
	//		//string nomTrial = reader.ReadString();
	//		//string categorie = reader.ReadString();
	//		//string description = reader.ReadString ();

	//		if (version == 1.0)
	//		{
	//			for (int i = 0; i < nFrame; ++i)
	//			{ // Pour chaque frame
	//				long timeStamps = reader.ReadInt64(); // Lire un timestamp Int64 = long
	//				XsMatrix[] all_mat = new XsMatrix[nCentrale];
	//				for (int j = 0; j < nCentrale; ++j)
	//				{ // Pour chaque centrale
	//					XsMatrix mat = new XsMatrix(3, 3);
	//					for (uint k = 0; k < nElementsInMatrix; ++k)
	//					{ // Pour chaque élément de la matrice de rotation
	//						mat.setValue(k % 3, k / 3, reader.ReadDouble());
	//					}
	//					all_mat[j] = mat;
	//				}
	//				Add(timeStamps, all_mat);
	//			}
	//		}
	//	}
	//}

}
