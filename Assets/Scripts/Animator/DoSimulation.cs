using System;
using System.Linq;
using Microsoft.Research.Oslo;
using System.Runtime.InteropServices;

// =================================================================================================================================================================
/// <summary> Exécution des calculs de simulation. </summary>

public class DoSimulation
{
	// Déclaration des pointeurs

	static IntPtr ptr_massMatrix;
	static IntPtr ptr_tau;
	static IntPtr ptr_Q;
	static IntPtr ptr_V;
	static IntPtr ptr_qddot2;
	static IntPtr ptr_matA;
	static IntPtr ptr_solX;

	public static bool modeRT = false;

	static double[] qd;
	static double[] qdotd;
	static double[] qddotd;

	//public static int GetSimulation(out double[,] qOut)
	//{
	//	// Affichage d'un message dans la boîte des messages

	//	AnimationF.Instance.DisplayNewMessage(false, true, string.Format(" {0} = {1:0.0} s", MainParameters.Instance.languages.Used.displayMsgSimulationTime, MainParameters.Instance.joints.duration));

	//	// Définir un nom racourci pour avoir accès à la structure Joints

	//	MainParameters.StrucJoints joints = MainParameters.Instance.joints;

	//	// Init_Move

	//	#region Init_Move

	//	double[] q0 = new double[joints.lagrangianModel.nDDL];
	//	double[] q0dot = new double[joints.lagrangianModel.nDDL];
	//	double[] q0dotdot = new double[joints.lagrangianModel.nDDL];
	//	Trajectory trajectory = new Trajectory(joints.lagrangianModel, 0, joints.lagrangianModel.q2, out q0, out q0dot, out q0dotdot);
	//	trajectory.ToString();                  // Pour enlever un warning lors de la compilation

	//	int[] rotation = new int[3] { joints.lagrangianModel.root_somersault, joints.lagrangianModel.root_tilt, joints.lagrangianModel.root_twist };
	//	int[] rotationS = Vector.Sign(rotation);
	//	for (int i = 0; i < rotation.Length; i++) rotation[i] = Math.Abs(rotation[i]);

	//	int[] translation = new int[3] { joints.lagrangianModel.root_right, joints.lagrangianModel.root_foreward, joints.lagrangianModel.root_upward };
	//	int[] translationS = Vector.Sign(translation);
	//	for (int i = 0; i < translation.Length; i++) translation[i] = Math.Abs(translation[i]);

	//	double rotRadians = joints.takeOffParam.rotation * (double)Math.PI / 180;

	//	double tilt = joints.takeOffParam.tilt;
	//	if (tilt == 90)                                 // La fonction Ode.RK547M donne une erreur fatale, si la valeur d'inclinaison est de 90 ou -90 degrés
	//		tilt = 90.001f;
	//	else if (tilt == -90)
	//		tilt = -90.01f;
	//	q0[Math.Abs(joints.lagrangianModel.root_tilt) - 1] = tilt * (double)Math.PI / 180;                                           // en radians
	//	q0[Math.Abs(joints.lagrangianModel.root_somersault) - 1] = rotRadians;                                                      // en radians
	//	q0dot[Math.Abs(joints.lagrangianModel.root_foreward) - 1] = joints.takeOffParam.anteroposteriorSpeed;                       // en m/s
	//	q0dot[Math.Abs(joints.lagrangianModel.root_upward) - 1] = joints.takeOffParam.verticalSpeed;                                // en m/s
	//	q0dot[Math.Abs(joints.lagrangianModel.root_somersault) - 1] = joints.takeOffParam.somersaultSpeed * 2 * (double)Math.PI;     // en radians/s
	//	q0dot[Math.Abs(joints.lagrangianModel.root_twist) - 1] = joints.takeOffParam.twistSpeed * 2 * (double)Math.PI;               // en radians/s

	//	// correction of linear velocity to have CGdot = qdot

	//	double[] Q = new double[joints.lagrangianModel.nDDL];
	//	for (int i = 0; i < joints.lagrangianModel.nDDL; i++)
	//		Q[i] = q0[i];
	//	double[] tagX;
	//	double[] tagY;
	//	double[] tagZ;
	//	AnimationF.Instance.EvaluateTags(Q, out tagX, out tagY, out tagZ);

	//	double[] cg = new double[3];          // CG in initial posture
	//	cg[0] = tagX[tagX.Length - 1];
	//	cg[1] = tagY[tagX.Length - 1];
	//	cg[2] = tagZ[tagX.Length - 1];

	//	double[] u1 = new double[3];
	//	double[,] rot = new double[3,1];
	//	for (int i = 0; i < 3; i++)
	//	{
	//		u1[i] = cg[i] - q0[translation[i] - 1] * translationS[i];
	//		rot[i,0] = q0dot[rotation[i] - 1] * rotationS[i];
	//	}
	//	double[,] u = { { 0, -u1[2], u1[1] }, { u1[2], 0, -u1[0] }, { -u1[1], u1[0], 0 } };
	//	double[,] rotM = Matrix.Multiplication(u, rot);
	//	for (int i = 0; i < 3; i++)
	//	{
	//		q0dot[translation[i] - 1] = q0dot[translation[i] - 1] * translationS[i] + rotM[i, 0];
	//		q0dot[translation[i] - 1] = q0dot[translation[i] - 1] * translationS[i];
	//	}

	//	double hFeet = Math.Min(tagZ[joints.lagrangianModel.feet[0] - 1], tagZ[joints.lagrangianModel.feet[1] - 1]);
	//	double hHand = Math.Min(tagZ[joints.lagrangianModel.hand[0] - 1], tagZ[joints.lagrangianModel.hand[1] - 1]);

	//	if (joints.condition < 8 && Math.Cos(rotRadians) > 0)
	//		q0[Math.Abs(joints.lagrangianModel.root_upward) - 1] += joints.lagrangianModel.hauteurs[joints.condition] - hFeet;
	//	else															// bars, vault and tumbling from hands
	//		q0[Math.Abs(joints.lagrangianModel.root_upward) - 1] += joints.lagrangianModel.hauteurs[joints.condition] - hHand;
	//	#endregion

	//	// Sim_Airborn

	//	#region Sim_Airborn

	//	AnimationF.xTFrame0 = new double[joints.lagrangianModel.nDDL * 2];
	//	for (int i = 0; i < joints.lagrangianModel.nDDL; i++)
	//	{
	//		AnimationF.xTFrame0[i] = q0[i];
	//		AnimationF.xTFrame0[joints.lagrangianModel.nDDL + i] = q0dot[i];
	//	}

	//	Options options = new Options();
	//	options.InitialStep = joints.lagrangianModel.dt;

	//	// Extraire les données obtenues du Runge-Kutta et conserver seulement les points interpolés aux frames désirés, selon la durée et le dt utilisé

	//	DoSimulation.modeRT = false;
	//	var sol = Ode.RK547M(0, joints.duration + joints.lagrangianModel.dt, new Microsoft.Research.Oslo.Vector(AnimationF.xTFrame0), ShortDynamics, options);
	//	var points = sol.SolveFromToStep(0, joints.duration + joints.lagrangianModel.dt, joints.lagrangianModel.dt).ToArray();

	//	double[,] q = new double[joints.lagrangianModel.nDDL, points.GetUpperBound(0) + 1];
 //       for (int i = 0; i < joints.lagrangianModel.nDDL; i++)
	//		for (int j = 0; j <= points.GetUpperBound(0); j++)
	//			q[i,j] = points[j].X[i];
	//	#endregion

	//	// Vérifier s'il y a un contact avec le sol

	//	int index = 0;
	//	for (int i = 0; i <= q.GetUpperBound(1); i++)
	//	{
	//		index++;
	//		double[] qq = new double[joints.lagrangianModel.nDDL];
	//		for (int j = 0; j < joints.lagrangianModel.nDDL; j++)
	//			qq[j] = q[j, i];
	//		AnimationF.Instance.EvaluateTags(qq, out tagX, out tagY, out tagZ);
	//		if (joints.condition > 0 && tagZ.Min() < -0.05f && AnimationF.Instance.playMode != MainParameters.Instance.languages.Used.animatorPlayModeGesticulation)
	//			break;
	//	}

	//	// Copier les q dans une autre matrice qOut, mais contient seulement les données jusqu'au contact avec le sol
	//	// Utiliser seulement pour calculer la dimension du volume utilisé pour l'animation

	//	qOut = new double[MainParameters.Instance.joints.lagrangianModel.nDDL, index];
 //       for (int i = 0; i < index; i++)
 //           for (int j = 0; j < MainParameters.Instance.joints.lagrangianModel.nDDL; j++)
 //               qOut[j, i] = (double)q[j, i];

	//	return points.GetUpperBound(0) + 1;
	//}

}
