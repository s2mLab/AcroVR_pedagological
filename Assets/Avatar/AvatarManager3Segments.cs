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

	protected double[][] ZeroPosition;

	protected new void Start()
	{
		base.Start();
		ZeroPosition = GetOrientationFromAvatar();
	}

	protected double[][] GetOrientationFromAvatar()
    {
		double[] _hipsOrientation = {
			//hips.transform.localEulerAngles[0],
			//hips.transform.localEulerAngles[1],
			//hips.transform.localEulerAngles[2]
			0, 0, 0
		};
		double[] _leftUpperLimbOrientation = {
			//leftUpperLimb.transform.localEulerAngles[0],
			//leftUpperLimb.transform.localEulerAngles[1],
			//leftUpperLimb.transform.localEulerAngles[2]
			0, 0, 0
		};
		double[] _rightUpperLimbOrientation = {
			//rightUpperLimb.transform.localEulerAngles[0],
			//rightUpperLimb.transform.localEulerAngles[1],
			//rightUpperLimb.transform.localEulerAngles[2]
			0, 0, 0
		};
		
		double[][] result = { _hipsOrientation, _leftUpperLimbOrientation, _rightUpperLimbOrientation };
		return MapToInternal(result);
	}

	protected override string BiomodPath()
    {
		return @"Assets/Avatar/Biorbd/model3Segments.bioMod";
	}

	void Update()
	{
        GetCurrentData();
		if (CurrentData == null || !CurrentData.AllSensorsSet)
        {
			return;
        }

		AvatarMatrixRotation[] _data = ApplyZeroMatrix(CurrentData);
		if (_data is null)
        {
			return;
        }

		double[][] _dataVector = DispatchToAngleVector(_data);
		SetSegmentsRotations(_dataVector);
    }

	public override bool SetZeroMatrix(XSensData _zero)
	{
		if (!base.SetZeroMatrix(_zero))
        {
			return false;
        }

		// Put the body as it is at the beginning 
		for (int i = 0; i < 3; i++)
        {
			ZeroMatrix[i] *= AvatarMatrixRotation.FromEulerYXZ(ZeroPosition[i]);
		}
		return true;
	}

	public override void SetSegmentsRotations(double[][] q)
	{
		double[][] AnglesAvatar = MapToAvatar(q);
		ApplyRotation(hips, AnglesAvatar[0]);
		ApplyRotation(leftUpperLimb, AnglesAvatar[1]);
		ApplyRotation(rightUpperLimb, AnglesAvatar[2]);
	}

	static public double[][] MapToInternal(double[][] _angles)
    {
		double[][] _result = new double[3][];

		// Hips
		{
			double[] tp = { _angles[0][0], _angles[0][1], _angles[0][2] };
			_result[0] = MathUtils.ToRadian(tp);
		}

		// Left arm
		{
			double[] tp = { _angles[1][0], _angles[1][1], _angles[1][2] };
			_result[1] = MathUtils.ToRadian(tp);
		}

		// Right arm
		{
			double[] tp = { _angles[2][0], _angles[2][1], _angles[2][2] };
			_result[2] = MathUtils.ToRadian(tp);
		}

		return _result;
    }

	static public double[][] MapToAvatar(double[][] _angles)
	{
		double[][] _anglesDegree = MathUtils.ToDegree(_angles);

		double[][] _result = new double[3][];
		// Hips
		_result[0] = new double[3];
		_result[0][0] = (float)_anglesDegree[0][0];
		_result[0][1] = (float)_anglesDegree[0][1];
		_result[0][2] = (float)_anglesDegree[0][2];

		// Left arm
		_result[1] = new double[3];
		_result[1][0] = (float)_anglesDegree[1][0];
		_result[1][1] = (float)_anglesDegree[1][1];
		_result[1][2] = (float)_anglesDegree[1][2];

		// Right arm
		_result[2] = new double[3];
		_result[2][0] = (float)_anglesDegree[2][0];
		_result[2][1] = (float)_anglesDegree[2][1];
		_result[2][2] = (float)_anglesDegree[2][2];
		
		return _result;
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
