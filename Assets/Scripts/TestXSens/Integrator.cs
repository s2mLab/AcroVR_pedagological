using System;
using System.Text;
using Microsoft.Research.Oslo;
using System.Runtime.InteropServices;
using UnityEngine;

// =================================================================================================================================================================
/// <summary> Fonctions utilisées pour utiliser l'intégrateur RK8. </summary>

public class Integrator : MonoBehaviour
{
    /// <summary> Vecteur contenant l'état (q0 et q0dot) au temps t(frame - 2). </summary>
    public static double[] xTFrame0;
    /// <summary> Vecteur contenant l'état (q0 et q0dot) au temps t(frame). </summary>
    public static double[] xTFrame1;

    public static string[] msg;

    // =================================================================================================================================================================
    /// <summary> Initialisation des positions, des vitesses et des accélérations initiaux des articulations, pour les paramètres de décollage spécifiés. </summary>

    public static void InitMove(double[] q, double[] qdot)
    {
        msg = new string[12];
        for (int i = 0; i < msg.Length; i++)
            msg[i] = "";

        // Définir un nom racourci pour avoir accès à la structure Joints

        MainParameters.StrucJoints joints = MainParameters.Instance.joints;

        double[] q0 = new double[q.Length];
        double[] q0dot = new double[q.Length];
        for (int i = 0; i < q.Length; i++)
        {
            q0[i] = q[i];
            q0dot[i] = qdot[i];
        }

        double rotRadians = joints.takeOffParam.rotation * Math.PI / 180;
        double tilt = joints.takeOffParam.tilt;
        if (tilt == 90)                                 // La fonction Ode.RK547M donne une erreur fatale, si la valeur d'inclinaison est de 90 ou -90 degrés
            tilt = 90.001f;
        else if (tilt == -90)
            tilt = -90.01f;

        for (int i = 0; i < q0.Length; i++)
        {
            msg[0] += string.Format("{0}, ", q0[i]);
            msg[1] += string.Format("{0}, ", q0dot[i]);
        }

        q0[Math.Abs(joints.lagrangianModel.root_right) - 1] = 0;                                                                    // en m
        q0[Math.Abs(joints.lagrangianModel.root_foreward) - 1] = 0;                                                                 // en m
        q0[Math.Abs(joints.lagrangianModel.root_upward) - 1] = 0;                                                                   // en m			(Ajouter par Marcel 2021-01-11)
        q0[Math.Abs(joints.lagrangianModel.root_somersault) - 1] = rotRadians;                                                      // en radians
        q0[Math.Abs(joints.lagrangianModel.root_tilt) - 1] = tilt * Math.PI / 180;                                                  // en radians
        q0[Math.Abs(joints.lagrangianModel.root_twist) - 1] = 0;                                                                    // en radians

        q0dot[Math.Abs(joints.lagrangianModel.root_right) - 1] = 0;                                                                 // en m/s
        q0dot[Math.Abs(joints.lagrangianModel.root_foreward) - 1] = joints.takeOffParam.anteroposteriorSpeed;                       // en m/s
        q0dot[Math.Abs(joints.lagrangianModel.root_upward) - 1] = joints.takeOffParam.verticalSpeed;                                // en m/s
        q0dot[Math.Abs(joints.lagrangianModel.root_somersault) - 1] = joints.takeOffParam.somersaultSpeed * 2 * Math.PI;            // en radians/s
        q0dot[Math.Abs(joints.lagrangianModel.root_tilt) - 1] = 0;                                                                  // en radians/s
        q0dot[Math.Abs(joints.lagrangianModel.root_twist) - 1] = joints.takeOffParam.twistSpeed * 2 * Math.PI;                      // en radians/s
        foreach (int i in joints.lagrangianModel.q2)                                                                                // Vitesse hors-racine = 0
            q0dot[i - 1] = 0;

        for (int i = 0; i < q0.Length; i++)
        {
            msg[2] += string.Format("{0}, ", q0[i]);
            msg[3] += string.Format("{0}, ", q0dot[i]);
        }

        // correction of linear velocity to have CGdot = qdot

        double[] Q = new double[joints.lagrangianModel.nDDL];
        for (int i = 0; i < joints.lagrangianModel.nDDL; i++)
            Q[i] = q0[i];
        double[] tagX;
        double[] tagY;
        double[] tagZ;
        AnimationF.Instance.EvaluateTags(Q, out tagX, out tagY, out tagZ);

        double[] cg = new double[3];          // CG in initial posture
        cg[0] = tagX[0];
        cg[1] = tagY[0];
        cg[2] = tagZ[0];

        for (int i = 0; i < cg.Length; i++)
            msg[4] += string.Format("{0}, ", cg[i]);

        int[] rotation = new int[3] { joints.lagrangianModel.root_somersault, joints.lagrangianModel.root_tilt, joints.lagrangianModel.root_twist };
        int[] rotationS = Vector.Sign(rotation);
        for (int i = 0; i < rotation.Length; i++) rotation[i] = Math.Abs(rotation[i]);
        int[] translation = new int[3] { joints.lagrangianModel.root_right, joints.lagrangianModel.root_foreward, joints.lagrangianModel.root_upward };
        int[] translationS = Vector.Sign(translation);
        for (int i = 0; i < translation.Length; i++) translation[i] = Math.Abs(translation[i]);
        double[] u1 = new double[3];
        double[,] rot = new double[3, 1];
        for (int i = 0; i < 3; i++)
        {
            u1[i] = cg[i] - q0[translation[i] - 1] * translationS[i];
            rot[i, 0] = q0dot[rotation[i] - 1] * rotationS[i];
        }

        for (int i = 0; i < u1.Length; i++)
        {
            msg[5] += string.Format("{0}, ", u1[i]);
            msg[6] += string.Format("{0}, ", rot[i, 0]);
        }

        double[,] u = { { 0, -u1[2], u1[1] }, { u1[2], 0, -u1[0] }, { -u1[1], u1[0], 0 } };
        double[,] rotM = Matrix.Multiplication(u, rot);

        for (int i = 0; i < u.GetLength(0); i++)
            for (int j = 0; j < u.GetLength(1); j++)
                msg[7] += string.Format("{0}, ", u[i, j]);
        for (int i = 0; i < rotM.GetLength(0); i++)
            for (int j = 0; j < rotM.GetLength(1); j++)
                msg[8] += string.Format("{0}, ", rotM[i, j]);

        for (int i = 0; i < 3; i++)
        {
            q0dot[translation[i] - 1] = q0dot[translation[i] - 1] * translationS[i] + rotM[i, 0];
            q0dot[rotation[i] - 1] = q0dot[rotation[i] - 1] * rotationS[i];
        }

        for (int i = 0; i < q0.Length; i++)
        {
            msg[9] += string.Format("{0}, ", q0[i]);
            msg[10] += string.Format("{0}, ", q0dot[i]);
        }

        double hFeet = Math.Min(tagZ[joints.lagrangianModel.feet[0] - 1], tagZ[joints.lagrangianModel.feet[1] - 1]);
        double hHand = Math.Min(tagZ[joints.lagrangianModel.hand[0] - 1], tagZ[joints.lagrangianModel.hand[1] - 1]);
        if (joints.condition < 8 && Math.Cos(rotRadians) > 0)
            q0[Math.Abs(joints.lagrangianModel.root_upward) - 1] += joints.lagrangianModel.hauteurs[joints.condition] - hFeet;
        else                                                            // bars, vault and tumbling from hands
            q0[Math.Abs(joints.lagrangianModel.root_upward) - 1] += joints.lagrangianModel.hauteurs[joints.condition] - hHand;

        xTFrame0 = new double[joints.lagrangianModel.nDDL * 2];
        xTFrame1 = new double[joints.lagrangianModel.nDDL * 2];
        for (int i = 0; i < joints.lagrangianModel.nDDL; i++)
        {
            xTFrame0[i] = q0[i];
            xTFrame0[joints.lagrangianModel.nDDL + i] = q0dot[i];
        }

        string fileRK8Q0 = @"C:\Devel\AcroVR_RK8Q0Qdot0.txt";
        System.IO.File.Delete(fileRK8Q0);
        for (int i = 0; i < msg.Length; i++)
            System.IO.File.AppendAllText(fileRK8Q0, string.Format("{0}{1}", msg[i], System.Environment.NewLine));

    }

    // =================================================================================================================================================================
    /// <summary> Intégration RK8. </summary>

    public static void RK8(double[,] qint, double[,] qdint, double[,] qddint)
    {
        double[] t = new double[3] { 0, MainParameters.Instance.joints.lagrangianModel.dt / 2, MainParameters.Instance.joints.lagrangianModel.dt };
        double[] k_1 = ShortDynamicsRK8(0, xTFrame0, t, qint, qdint, qddint);

        //if (MainParameters.Instance.debugDataFileIMUsEulerQ &&	TestXSens.nMsgDebug < TestXSens.nMsgSize)
        //{
        //	TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("xTFrame0 = ");
        //	for (int i = 0; i < xTFrame0.Length; i++)
        //		TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0}, ", xTFrame0[i]);
        //	TestXSens.nMsgDebug++;
        //	TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("qint[0] = ");
        //	for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.nDDL; i++)
        //		TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0}, ", qint[i, 0]);
        //	TestXSens.nMsgDebug++;
        //	TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("qdint[0] = ");
        //	for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.nDDL; i++)
        //		TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0}, ", qdint[i, 0]);
        //	TestXSens.nMsgDebug++;
        //}

        double[] xT = new double[MainParameters.Instance.joints.lagrangianModel.nDDL * 2];
        for (int i = 0; i < xTFrame0.Length; i++)
            xT[i] = xTFrame0[i] + (MainParameters.Instance.joints.lagrangianModel.dt * 4.0f / 27.0f) * k_1[i];
        double[] k_2 = ShortDynamicsRK8(MainParameters.Instance.joints.lagrangianModel.dt * 4.0f / 27.0f, xT, t, qint, qdint, qddint);

        for (int i = 0; i < xTFrame0.Length; i++)
            xT[i] = xTFrame0[i] + (MainParameters.Instance.joints.lagrangianModel.dt / 18.0f) * (k_1[i] + 3.0f * k_2[i]);
        double[] k_3 = ShortDynamicsRK8(MainParameters.Instance.joints.lagrangianModel.dt * 2.0f / 9.0f, xT, t, qint, qdint, qddint);

        for (int i = 0; i < xTFrame0.Length; i++)
            xT[i] = xTFrame0[i] + (MainParameters.Instance.joints.lagrangianModel.dt / 12.0f) * (k_1[i] + 3.0f * k_3[i]);
        double[] k_4 = ShortDynamicsRK8(MainParameters.Instance.joints.lagrangianModel.dt * 1.0f / 3.0f, xT, t, qint, qdint, qddint);

        for (int i = 0; i < xTFrame0.Length; i++)
            xT[i] = xTFrame0[i] + (MainParameters.Instance.joints.lagrangianModel.dt / 8.0f) * (k_1[i] + 3.0f * k_4[i]);
        double[] k_5 = ShortDynamicsRK8(MainParameters.Instance.joints.lagrangianModel.dt * 1.0f / 2.0f, xT, t, qint, qdint, qddint);

        for (int i = 0; i < xTFrame0.Length; i++)
            xT[i] = xTFrame0[i] + (MainParameters.Instance.joints.lagrangianModel.dt / 54.0f) * (13.0f * k_1[i] - 27.0f * k_3[i] + 42.0f * k_4[i] + 8.0f * k_5[i]);
        double[] k_6 = ShortDynamicsRK8(MainParameters.Instance.joints.lagrangianModel.dt * 2.0f / 3.0f, xT, t, qint, qdint, qddint);

        for (int i = 0; i < xTFrame0.Length; i++)
            xT[i] = xTFrame0[i] + (MainParameters.Instance.joints.lagrangianModel.dt / 4320.0f) * (389.0f * k_1[i] - 54.0f * k_3[i] + 966.0f * k_4[i] - 824.0f * k_5[i] + 243.0f * k_6[i]);
        double[] k_7 = ShortDynamicsRK8(MainParameters.Instance.joints.lagrangianModel.dt * 1.0f / 6.0f, xT, t, qint, qdint, qddint);

        for (int i = 0; i < xTFrame0.Length; i++)
            xT[i] = xTFrame0[i] + (MainParameters.Instance.joints.lagrangianModel.dt / 20.0f) * (-234.0f * k_1[i] + 81.0f * k_3[i] - 1164.0f * k_4[i] + 656.0f * k_5[i] - 122.0f * k_6[i] + 800.0f * k_7[i]);
        double[] k_8 = ShortDynamicsRK8(MainParameters.Instance.joints.lagrangianModel.dt, xT, t, qint, qdint, qddint);

        for (int i = 0; i < xTFrame0.Length; i++)
            xT[i] = xTFrame0[i] + (MainParameters.Instance.joints.lagrangianModel.dt / 288.0f) * (-127.0f * k_1[i] + 18.0f * k_3[i] - 678.0f * k_4[i] + 456.0f * k_5[i] - 9.0f * k_6[i] + 576.0f * k_7[i] + 4.0f * k_8[i]);
        double[] k_9 = ShortDynamicsRK8(MainParameters.Instance.joints.lagrangianModel.dt * 5.0f / 6.0f, xT, t, qint, qdint, qddint);

        for (int i = 0; i < xTFrame0.Length; i++)
            xT[i] = xTFrame0[i] + (MainParameters.Instance.joints.lagrangianModel.dt / 820.0f) * (1481.0f * k_1[i] - 81.0f * k_3[i] + 7104.0f * k_4[i] - 3376.0f * k_5[i] + 72.0f * k_6[i] - 5040.0f * k_7[i] -
                                                    60.0f * k_8[i] + 720.0f * k_9[i]);
        double[] k_10 = ShortDynamicsRK8(MainParameters.Instance.joints.lagrangianModel.dt, xT, t, qint, qdint, qddint);

        for (int i = 0; i < xTFrame0.Length; i++)
            xTFrame1[i] = xTFrame0[i] + MainParameters.Instance.joints.lagrangianModel.dt / 840.0f * (41.0f * k_1[i] + 27.0f * k_4[i] + 272.0f * k_5[i] + 27.0f * k_6[i] + 216.0f * k_7[i] + 216.0f * k_9[i] +
                                                    41.0f * k_10[i]);
    }

    // =================================================================================================================================================================
    /// <summary> Routine qui sera exécuter par le ODE (Ordinary Differential Equation). </summary>

    static double[] ShortDynamicsRK8(double ti, double[] x, double[] t, double[,] qdaa, double[,] qdotdaa, double[,] qddotdaa)
    {
        double[] q = new double[MainParameters.Instance.joints.lagrangianModel.nDDL];
        double[] qdot = new double[MainParameters.Instance.joints.lagrangianModel.nDDL];
        double[] qddot = new double[MainParameters.Instance.joints.lagrangianModel.nDDL];
        for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.nDDL; i++)
        {
            q[i] = x[i];
            qdot[i] = x[MainParameters.Instance.joints.lagrangianModel.nDDL + i];
        }

        double[] qda = Quintic.Interp1(ti, t, qdaa);
        double[] qdotda = Quintic.Interp1(ti, t, qdotdaa);
        double[] qddotda = Quintic.Interp1(ti, t, qddotdaa);

        for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.q2.Length; i++)
        {
            int ii = MainParameters.Instance.joints.lagrangianModel.q2[i] - 1;
            q[ii] = qda[ii];
            qdot[ii] = qdotda[ii];
        }

        for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.nDDL; i++)
            qddot[i] = qddotda[i];

        double[,] M11 = new double[MainParameters.Instance.joints.lagrangianModel.q1.Length, MainParameters.Instance.joints.lagrangianModel.q1.Length];
        double[,] M12 = new double[MainParameters.Instance.joints.lagrangianModel.q1.Length, MainParameters.Instance.joints.lagrangianModel.q2.Length];
        double[,] massMat = Vector.ToSquareMatrix(MassMatrix(q));                        // On obtient une matrice nDDL x nDDL
        for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.q1.Length; i++)
            foreach (int j in MainParameters.Instance.joints.lagrangianModel.q1)
                M11[i, j - 1] = massMat[i, j - 1];                                                      // M11 = [0...5, 0...5]

        for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.q1.Length; i++)
            for (int j = 0; j < MainParameters.Instance.joints.lagrangianModel.q2.Length; j++)
                M12[i, j] = massMat[i, MainParameters.Instance.joints.lagrangianModel.q2[j] - 1];       // M12 = [0...5, 6...nDDL -1]

        double[] N1 = NonlinearEffects(q, qdot);

        // Calcul "Matrix Left division" suivante: qddot(q1) = M11\(-N1-M12*qddot(q2));
        // On peut faire ce calcul en utilisant le calcul "Matrix inverse": qddot(q1) = inv(M11)*(-N1-M12*qddot(q2));

        double[,] mA = Matrix.Inverse(M11);

        double[] qddotb = new double[MainParameters.Instance.joints.lagrangianModel.q2.Length];
        for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.q2.Length; i++)
            qddotb[i] = qddot[MainParameters.Instance.joints.lagrangianModel.q2[i] - 1];
        double[,] mB = Matrix.Multiplication(M12, qddotb);

        double[,] n1mB = new double[mB.GetUpperBound(0) + 1, mB.GetUpperBound(1) + 1];
        for (int i = 0; i <= mB.GetUpperBound(0); i++)
            for (int j = 0; j <= mB.GetUpperBound(1); j++)
                n1mB[i, j] = -N1[i] - mB[i, j];

        double[,] mC = Matrix.Multiplication(mA, n1mB);

        for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.q1.Length; i++)
            qddot[MainParameters.Instance.joints.lagrangianModel.q1[i] - 1] = (double)mC[i, 0];

        double[] xdot = new double[MainParameters.Instance.joints.lagrangianModel.nDDL * 2];
        for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.nDDL; i++)
        {
            xdot[i] = qdot[i];
            xdot[MainParameters.Instance.joints.lagrangianModel.nDDL + i] = qddot[i];
        }

        return xdot;
    }

    // =================================================================================================================================================================
    /// <summary> Implémentation de la méthode RK45 pour la simulation en temps réel. </summary>
    // Référence: Fichier RK547M.cs dans Unity asset Oslo de Microsoft.
    //				Simple RK implementation with fixed time step. Not intended for practical use</summary>
    //				[Obsolete("Fixed step RK45 method is provided only as an example")]

    public static Microsoft.Research.Oslo.Vector RK4(
        Func<double, double[], double[], double[], double[], Microsoft.Research.Oslo.Vector> DynamicFunc,
        Microsoft.Research.Oslo.Vector x0, 
        double[] qFrame0, 
        double[] qdFrame0,
        double[] qddFrame0, 
        double[] qFrame1, 
        double[] qdFrame1, 
        double[] qddFrame1, 
        double[] qFrame2, 
        double[] qdFrame2,
        double[] qddFrame2
    )
    {
        Microsoft.Research.Oslo.Vector x = x0;
        double dt = MainParameters.Instance.joints.lagrangianModel.dt;
        double dt2 = dt / 2;

        //DynamicFunc = TestXSens.model.RootDynamics;
        Microsoft.Research.Oslo.Vector x1 = DynamicFunc(0, x, qFrame0, qdFrame0, qddFrame0);
        Microsoft.Research.Oslo.Vector x2 = DynamicFunc(0, x + x1 * dt2, qFrame1, qdFrame1, qddFrame1);
        Microsoft.Research.Oslo.Vector x3 = DynamicFunc(0, x + x2 * dt2, qFrame1, qdFrame1, qddFrame1);
        Microsoft.Research.Oslo.Vector x4 = DynamicFunc(0, x + x3 * dt, qFrame2, qdFrame2, qddFrame2);
        x = x + (dt / 6.0) * (x1 + 2.0 * x2 + 2.0 * x3 + x4);

        return x;  //retourne état suivant à l'instant t+dt         
    }

    //public static Vector RK4_1(Vector x0, double[] qddFrame0, double[] qddFrame1)           // Ancienne version 2021-04-15
    public static Microsoft.Research.Oslo.Vector RK4_1(double[] x0, double[] qFrame0, double[] qdFrame0, double[] qddFrame0, double[] qFrame1, double[] qdFrame1, double[] qddFrame1)
    {
        //double n_step = 5;
        double[] x = x0;
        //double dt = MainParameters.Instance.joints.lagrangianModel.dt / n_step;
        double dt = MainParameters.Instance.joints.lagrangianModel.dt;
        double dt2 = dt / 2;
        double[] k1, k2, k3, k4;
        //double t = 0;
        //double[] qddot_Approx = Quintic.Copy(qddFrame0);

        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize - 1)
        //{
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("nFrame = {0}, qddFrame0 = ", TestXSens.Instance.nFrame);
        //    for (int i = 0; i < qddFrame0.Length; i++)
        //        TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0},", qddFrame0[i]);
        //    TestXSens.nMsgDebug++;
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("nFrame = {0}, qddFrame1 = ", TestXSens.Instance.nFrame);
        //    for (int i = 0; i < qddFrame1.Length; i++)
        //        TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0},", qddFrame1[i]);
        //    TestXSens.nMsgDebug++;
        //}

        //for (int i_step = 0; i_step < n_step + 1; i_step++)
        //{

        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize)
        //{
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("nFrame = {0}, k1", TestXSens.Instance.nFrame);
        //    TestXSens.nMsgDebug++;
        //}

        //k1 = ShortDynamicsRK4_1(i_step / n_step, x, qddFrame0, qddFrame1);

        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize - 8)
        //{
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("nFrame = {0}", TestXSens.Instance.nFrame);
        //    TestXSens.nMsgDebug++;
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("x (length = {0}) = ", x.Length);
        //    for (int i = 0; i < x.Length; i++)
        //        TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0},", x[i]);
        //    TestXSens.nMsgDebug++;
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("qFrame0 (length = {0}) = ", qFrame0.Length);
        //    for (int i = 0; i < qFrame0.Length; i++)
        //        TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0},", qFrame0[i]);
        //    TestXSens.nMsgDebug++;
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("qdFrame0 (length = {0}) = ", qdFrame0.Length);
        //    for (int i = 0; i < qdFrame0.Length; i++)
        //        TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0},", qdFrame0[i]);
        //    TestXSens.nMsgDebug++;
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("qddFrame0 (length = {0}) = ", qddFrame0.Length);
        //    for (int i = 0; i < qddFrame0.Length; i++)
        //        TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0},", qddFrame0[i]);
        //    TestXSens.nMsgDebug++;
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("qFrame1 (length = {0}) = ", qFrame1.Length);
        //    for (int i = 0; i < qFrame1.Length; i++)
        //        TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0},", qFrame1[i]);
        //    TestXSens.nMsgDebug++;
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("qdFrame1 (length = {0}) = ", qdFrame1.Length);
        //    for (int i = 0; i < qdFrame1.Length; i++)
        //        TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0},", qdFrame1[i]);
        //    TestXSens.nMsgDebug++;
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("qddFrame1 (length = {0}) = ", qddFrame1.Length);
        //    for (int i = 0; i < qddFrame1.Length; i++)
        //        TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0},", qddFrame1[i]);
        //    TestXSens.nMsgDebug++;
        //}

        k1 = ShortDynamicsRK4_1(0, x, qFrame0, qdFrame0, qddFrame0, qFrame1, qdFrame1, qddFrame1);

        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize)
        //{
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("k1 (length = {0}) = ", k1.Length);
        //    for (int i = 0; i < k1.Length; i++)
        //        TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0},", k1[i]);
        //    TestXSens.nMsgDebug++;
        //}

        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize)
        //{
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("nFrame = {0}, k2", TestXSens.Instance.nFrame);
        //    TestXSens.nMsgDebug++;
        //}

        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize)
        //{
        //    double[] v = k1;
        //    //TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("nFrame = {0}, i_step = {1}, k1 = ", TestXSens.Instance.nFrame, i_step);
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("{0}, 1, ", TestXSens.Instance.nFrame);
        //    for (int i = 0; i < v.Length; i++)
        //        TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0}, ", v[i]);
        //    TestXSens.nMsgDebug++;
        //}

        //k2 = ShortDynamicsRK4_1(i_step / n_step, x + dt2 * k1, qddFrame0, qddFrame1);
        double[] x2 = new double[MainParameters.Instance.joints.lagrangianModel.nDDL * 2];
        for (int i = 0; i < x.Length; i++)
            x2[i] = x[i] + dt2 * k1[i];
        k2 = ShortDynamicsRK4_1(dt2, x2, qFrame0, qdFrame0, qddFrame0, qFrame1, qdFrame1, qddFrame1);

        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize)
        //{
        //    double[] v = k2;
        //    //TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("nFrame = {0}, i_step = {1}, k2 = ", TestXSens.Instance.nFrame, i_step);
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("{0}, 2, ", TestXSens.Instance.nFrame);
        //    for (int i = 0; i < v.Length; i++)
        //        TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0}, ", v[i]);
        //    TestXSens.nMsgDebug++;
        //}

        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize)
        //{
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("nFrame = {0}, k3", TestXSens.Instance.nFrame);
        //    TestXSens.nMsgDebug++;
        //}

        //k3 = ShortDynamicsRK4_1(i_step / n_step, x + dt2 * k2, qddFrame0, qddFrame1);
        double[] x3 = new double[MainParameters.Instance.joints.lagrangianModel.nDDL * 2];
        for (int i = 0; i < x.Length; i++)
            x3[i] = x[i] + dt2 * k2[i];
        k3 = ShortDynamicsRK4_1(dt2, x3, qFrame0, qdFrame0, qddFrame0, qFrame1, qdFrame1, qddFrame1);

        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize)
        //{
        //    double[] v = k3;
        //    //TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("nFrame = {0}, i_step = {1}, k3 = ", TestXSens.Instance.nFrame, i_step);
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("{0}, 3, ", TestXSens.Instance.nFrame);
        //    for (int i = 0; i < v.Length; i++)
        //        TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0}, ", v[i]);
        //    TestXSens.nMsgDebug++;
        //}

        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize)
        //{
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("nFrame = {0}, k4", TestXSens.Instance.nFrame);
        //    TestXSens.nMsgDebug++;
        //}

        //k4 = ShortDynamicsRK4_1(i_step / n_step, x + dt * k3, qddFrame0, qddFrame1);
        double[] x4 = new double[MainParameters.Instance.joints.lagrangianModel.nDDL * 2];
        for (int i = 0; i < x.Length; i++)
            x4[i] = x[i] + dt * k3[i];
        k4 = ShortDynamicsRK4_1(dt, x4, qFrame0, qdFrame0, qddFrame0, qFrame1, qdFrame1, qddFrame1);

        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize)
        //{
        //    double[] v = k4;
        //    //TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("nFrame = {0}, i_step = {1}, k4 = ", TestXSens.Instance.nFrame, i_step);
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("{0}, 4, ", TestXSens.Instance.nFrame);
        //    for (int i = 0; i < v.Length; i++)
        //        TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0}, ", v[i]);
        //    TestXSens.nMsgDebug++;
        //}

        double[] x5 = new double[MainParameters.Instance.joints.lagrangianModel.nDDL * 2];
        for (int i = 0; i < x.Length; i++)
            x5[i] = x[i] + (dt / 6) * (k1[i] + 2 * k2[i] + 2 * k3[i] + k4[i]);

        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize)
        //{
        //    double[] v = x5;
        //    //TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("nFrame = {0}, i_step = {1}, x = ", TestXSens.Instance.nFrame, i_step);
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("{0}, 5, ", TestXSens.Instance.nFrame);
        //    for (int i = 0; i < v.Length; i++)
        //        TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0}, ", v[i]);
        //    TestXSens.nMsgDebug++;
        //}

        return x5;  //retourne état suivant à l'instant t+dt         
    }

    // =================================================================================================================================================================
    /// <summary> Routine qui sera exécuter par le ODE (Ordinary Differential Equation). </summary>

    static double[] ShortDynamicsRK4_1(double t, double[] x, double[] qFrame0, double[] qdFrame0, double[] qddFrame0, double[] qFrame1, double[] qdFrame1, double[] qddFrame1)
    {
        double[] q = new double[MainParameters.Instance.joints.lagrangianModel.nDDL];
        double[] qdot = new double[MainParameters.Instance.joints.lagrangianModel.nDDL];
        double[] qddot = new double[MainParameters.Instance.joints.lagrangianModel.nDDL];
        for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.nDDL; i++)
        {
            q[i] = x[i];
            qdot[i] = x[MainParameters.Instance.joints.lagrangianModel.nDDL + i];
        }

        double[] qda = new double[qFrame0.Length];
        double[] qdotda = new double[qFrame0.Length];
        double[] qddotda = new double[qFrame0.Length];
        for (int i = 0; i < qFrame0.Length; i++)
        {
            qda[i] = (qFrame1[i] - qFrame0[i]) * t / MainParameters.Instance.joints.lagrangianModel.dt + qFrame0[i];
            qdotda[i] = (qdFrame1[i] - qdFrame0[i]) * t / MainParameters.Instance.joints.lagrangianModel.dt + qdFrame0[i];
            qddotda[i] = (qddFrame1[i] - qddFrame0[i]) * t / MainParameters.Instance.joints.lagrangianModel.dt + qddFrame0[i];
        }

        for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.q2.Length; i++)
        {
            int ii = MainParameters.Instance.joints.lagrangianModel.q2[i] - 1;
            q[ii] = qda[ii];
            qdot[ii] = qdotda[ii];
        }

        for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.nDDL; i++)
            qddot[i] = qddotda[i];

        double[,] M11 = new double[MainParameters.Instance.joints.lagrangianModel.q1.Length, MainParameters.Instance.joints.lagrangianModel.q1.Length];
        double[,] M12 = new double[MainParameters.Instance.joints.lagrangianModel.q1.Length, MainParameters.Instance.joints.lagrangianModel.q2.Length];
        double[,] massMat = Vector.ToSquareMatrix(MassMatrix(q));                        // On obtient une matrice nDDL x nDDL

        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize - massMat.GetUpperBound(1) - 1)
        //{
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("massMat (length = {0}, {1}) = ", massMat.GetUpperBound(0) + 1, massMat.GetUpperBound(1) + 1);
        //    for (int i = 0; i < massMat.GetUpperBound(0) + 1; i++)
        //    {
        //        for (int j = 0; j < massMat.GetUpperBound(1) + 1; j++)
        //            TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0},", massMat[i,j]);
        //        TestXSens.nMsgDebug++;
        //    }
        //}

        //for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.q1.Length; i++)
        //    foreach (int j in MainParameters.Instance.joints.lagrangianModel.q1)
        //        M11[i, j - 1] = massMat[i, j - 1];                                                      // M11 = [0...5, 0...5]

        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize - M11.GetUpperBound(1) - 1)
        //{
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("M11 (length = {0}, {1}) = ", M11.GetUpperBound(0) + 1, M11.GetUpperBound(1) + 1);
        //    for (int i = 0; i < M11.GetUpperBound(0) + 1; i++)
        //    {
        //        for (int j = 0; j < M11.GetUpperBound(1) + 1; j++)
        //            TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0},", M11[i, j]);
        //        TestXSens.nMsgDebug++;
        //    }
        //}

        //for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.q1.Length; i++)
        //    for (int j = 0; j < MainParameters.Instance.joints.lagrangianModel.q2.Length; j++)
        //        M12[i, j] = massMat[i, MainParameters.Instance.joints.lagrangianModel.q2[j] - 1];       // M12 = [0...5, 6...nDDL -1]

        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize - M12.GetUpperBound(1) - 1)
        //{
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("M12 (length = {0}, {1}) = ", M12.GetUpperBound(0) + 1, M12.GetUpperBound(1) + 1);
        //    for (int i = 0; i < M12.GetUpperBound(0) + 1; i++)
        //    {
        //        for (int j = 0; j < M12.GetUpperBound(1) + 1; j++)
        //            TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0},", M12[i, j]);
        //        TestXSens.nMsgDebug++;
        //    }
        //}

        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize)
        //{
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("q (length = {0}) = ", q.Length);
        //    for (int i = 0; i < q.Length; i++)
        //        TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0},", q[i]);
        //    TestXSens.nMsgDebug++;
        //}

        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize)
        //{
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("qdot (length = {0}) = ", qdot.Length);
        //    for (int i = 0; i < qdot.Length; i++)
        //        TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0},", qdot[i]);
        //    TestXSens.nMsgDebug++;
        //}

        double[] N1 = NonlinearEffects(q, qdot);

        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize)
        //{
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("N1 (length = {0}) = ", N1.Length);
        //    for (int i = 0; i < N1.Length; i++)
        //        TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0},", N1[i]);
        //    TestXSens.nMsgDebug++;
        //}

        // Calcul "Matrix Left division" suivante: qddot(q1) = M11\(-N1-M12*qddot(q2));
        // On peut faire ce calcul en utilisant le calcul "Matrix inverse": qddot(q1) = inv(M11)*(-N1-M12*qddot(q2));

        double[,] mA = Matrix.Inverse(M11);

        double[] qddotb = new double[MainParameters.Instance.joints.lagrangianModel.q2.Length];
        for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.q2.Length; i++)
            qddotb[i] = qddot[MainParameters.Instance.joints.lagrangianModel.q2[i] - 1];
        double[,] mB = Matrix.Multiplication(M12, qddotb);

        double[,] n1mB = new double[mB.GetUpperBound(0) + 1, mB.GetUpperBound(1) + 1];
        for (int i = 0; i <= mB.GetUpperBound(0); i++)
            for (int j = 0; j <= mB.GetUpperBound(1); j++)
                n1mB[i, j] = -N1[i] - mB[i, j];

        double[,] mC = Matrix.Multiplication(mA, n1mB);

        for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.q1.Length; i++)
            qddot[MainParameters.Instance.joints.lagrangianModel.q1[i] - 1] = (double)mC[i, 0];

        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize)
        //{
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("qddot (length = {0}) = ", qddot.Length);
        //    for (int i = 0; i < qddot.Length; i++)
        //        TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0},", qddot[i]);
        //    TestXSens.nMsgDebug++;
        //}

        double[] xdot = new double[MainParameters.Instance.joints.lagrangianModel.nDDL * 2];
        //for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.nDDL; i++)
        //{
        //    xdot[i] = qdot[i];
        //    xdot[MainParameters.Instance.joints.lagrangianModel.nDDL + i] = qddot[i];
        //}

        //if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize)
        //{
        //    TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("xdot (length = {0}) = ", xdot.Length);
        //    for (int i = 0; i < xdot.Length; i++)
        //        TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0},", xdot[i]);
        //    TestXSens.nMsgDebug++;
        //}

        return xdot;
    }

    // =================================================================================================================================================================
    /// <summary> Routine qui sera exécuter par le ODE (Ordinary Differential Equation). </summary>

    //public static Vector ShortDynamicsRK4_1(double t_ratio, Vector x, double[] qddFrame0, double[] qddFrame1)                   // Sérieux doutes sur la validiter du calcul de cette fonction
    //{
    //    int NDDL = MainParameters.Instance.joints.lagrangianModel.nDDL;             // Récupère le nombre de DDL du modèle
    //    int NROOT = MainParameters.Instance.joints.lagrangianModel.q1.Length;       // Pour le moment, la racine possède 6 ddl

    //    // Extraire les positions et vitesses des DDLs
    //    // Initialisation des accélérations des DDLs

    //    double[] q = new double[NDDL];
    //    double[] qdot = new double[NDDL];
    //    for (int i = 0; i < NDDL; i++)
    //    {
    //        q[i] = x[i];
    //        qdot[i] = x[i + NDDL];
    //    }

    //    double[] qddot = new double[NDDL];
    //    for (int i = 0; i < qddot.Length; i++)
    //        qddot[i] = (qddFrame1[i] - qddFrame0[i]) * t_ratio + qddFrame0[i];
    //    for (int i = 0; i < NROOT; i++)
    //        qddot[i] = 0;

    //    // Génère la matrice de masse

    //    double[,] massMat = Quintic.ConvertVectorInSquareMatrix(MassMatrix(q));                        // On obtient une matrice nDDL x nDDL
    //    double[,] matriceA = new double[NROOT, NROOT];
    //    matriceA = Quintic.ShrinkSquare(massMat, NROOT);                                         // On réduit la matrice de masse

    //    // Calcul de la dynamique inverse

    //    double[] taud = new double[NDDL];
    //    IntPtr ptr_Q = Marshal.AllocCoTaskMem(sizeof(double) * q.Length);
    //    IntPtr ptr_qdot = Marshal.AllocCoTaskMem(sizeof(double) * qdot.Length);
    //    IntPtr ptr_qddot = Marshal.AllocCoTaskMem(sizeof(double) * qddot.Length);
    //    IntPtr ptr_tau = Marshal.AllocCoTaskMem(sizeof(double) * taud.Length);

    //    Marshal.Copy(q, 0, ptr_Q, q.Length);
    //    Marshal.Copy(qdot, 0, ptr_qdot, qdot.Length);
    //    Marshal.Copy(qddot, 0, ptr_qddot, qddot.Length);

    //    MainParameters.c_inverseDynamics(TestXSens.modelInt, ptr_Q, ptr_qdot, ptr_qddot, ptr_tau);
    //    Marshal.Copy(ptr_tau, taud, 0, taud.Length);

    //    // Résoudre le système linéaire

    //    double[] matAGrandVecteur = new double[NROOT * NROOT];
    //    matAGrandVecteur = Quintic.ConvertSquareMatrixInVector(matriceA);                              // La nouvelle matrice doit être convertie en vecteur

    //    IntPtr ptr_matA = Marshal.AllocCoTaskMem(sizeof(double) * matAGrandVecteur.Length);
    //    IntPtr ptr_solX = Marshal.AllocCoTaskMem(sizeof(double) * NROOT);

    //    Marshal.Copy(matAGrandVecteur, 0, ptr_matA, matAGrandVecteur.Length);

    //    if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize - 3)
    //    {
    //        TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("NROOT, NDDL = {0}, {1}", NROOT, NDDL);
    //        TestXSens.nMsgDebug++;

    //        TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("matAGrandVecteur = ");
    //        for (int i = 0; i < matAGrandVecteur.Length; i++)
    //            TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0}, ", matAGrandVecteur[i]);
    //        TestXSens.nMsgDebug++;

    //        TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("taud = ");
    //        for (int i = 0; i < taud.Length; i++)
    //            TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0}, ", taud[i]);
    //        TestXSens.nMsgDebug++;
    //    }

    //    MainParameters.c_solveLinearSystem(ptr_matA, NROOT, NROOT, ptr_tau, ptr_solX);                  // Résouds l'équation Ax=b

    //    double[] solutionX = new double[NROOT];
    //    Marshal.Copy(ptr_solX, solutionX, 0, solutionX.Length);

    //    if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize)
    //    {
    //        TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("solutionX = ");
    //        for (int i = 0; i < solutionX.Length; i++)
    //            TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0}, ", solutionX[i]);
    //        TestXSens.nMsgDebug++;
    //    }

    //    for (int i = 0; i < NROOT; i++)
    //        qddot[i] = -solutionX[i];

    //    double[] qddot1 = new double[NDDL * 2];
    //    for (int i = 0; i < NDDL; i++)
    //    {
    //        qddot1[i] = qdot[i];
    //        qddot1[i + NDDL] = qddot[i];
    //    }

    //    // Désallocation des pointeurs

    //    Marshal.FreeCoTaskMem(ptr_Q);
    //    Marshal.FreeCoTaskMem(ptr_qdot);
    //    Marshal.FreeCoTaskMem(ptr_qddot);
    //    Marshal.FreeCoTaskMem(ptr_tau);
    //    Marshal.FreeCoTaskMem(ptr_matA);
    //    Marshal.FreeCoTaskMem(ptr_solX);

    //    return new Vector(qddot1);
    //}

    // =================================================================================================================================================================
    /// <summary> This function computes the generalized forces from given generalized states and velocities. </summary>

    static double[] NonlinearEffects(double[] q, double[] qdot)
	{
        int nMark = 0;
		double[] tau = new double[nMark];

		IntPtr ptr_Q = Marshal.AllocCoTaskMem(sizeof(double) * q.Length);
		IntPtr ptr_Qdot = Marshal.AllocCoTaskMem(sizeof(double) * qdot.Length);
		IntPtr ptr_Tau = Marshal.AllocCoTaskMem(sizeof(double) * tau.Length);

		Marshal.Copy(q, 0, ptr_Q, q.Length);
		Marshal.Copy(qdot, 0, ptr_Qdot, qdot.Length);
        Debug.Log("Fix this");
		//BiorbdModel.c_NonlinearEffects(TestXSens.model._ptr_model, ptr_Q, ptr_Qdot, ptr_Tau);
		Marshal.Copy(ptr_Tau, tau, 0, tau.Length);

		Marshal.FreeCoTaskMem(ptr_Q);
		Marshal.FreeCoTaskMem(ptr_Qdot);
		Marshal.FreeCoTaskMem(ptr_Tau);

		return tau;
	}

	// =================================================================================================================================================================
	/// <summary> Computes the joint space inertia matrix by using the Composite Rigid Body Algorithm.. </summary>

	static double[] MassMatrix(double[] q)
	{
		double[] massMatrix = new double[q.Length * q.Length];

		IntPtr ptr_Q = Marshal.AllocCoTaskMem(sizeof(double) * q.Length);
		IntPtr ptr_massMatrix = Marshal.AllocCoTaskMem(sizeof(double) * massMatrix.Length);

		Marshal.Copy(q, 0, ptr_Q, q.Length);
        Debug.Log("Fix this");
        //BiorbdModel.c_massMatrix(TestXSens.model._ptr_model, ptr_Q, ptr_massMatrix);
		Marshal.Copy(ptr_massMatrix, massMatrix, 0, massMatrix.Length);

		Marshal.FreeCoTaskMem(ptr_Q);
		Marshal.FreeCoTaskMem(ptr_massMatrix);

		return massMatrix;
	}

	// =================================================================================================================================================================
	/// <summary> Return all the markers at a given Q in the global reference frame. </summary>

	public static double[] Markers(double[] q)
	{
        int nMark = 0;
		double[] markPos = new double[nMark * 3];

		IntPtr ptr_Q = Marshal.AllocCoTaskMem(sizeof(double) * q.Length);
		IntPtr ptr_markPos = Marshal.AllocCoTaskMem(sizeof(double) * markPos.Length);

		Marshal.Copy(q, 0, ptr_Q, q.Length);

        Debug.Log("Fix this");
        //BiorbdModel.c_markers(TestXSens.model._ptr_model, ptr_Q, ptr_markPos, false, true);

		Marshal.Copy(ptr_markPos, markPos, 0, markPos.Length);

		Marshal.FreeCoTaskMem(ptr_Q);
		Marshal.FreeCoTaskMem(ptr_markPos);

		//if (MainParameters.Instance.debugDataFileIMUsEulerQ && TestXSens.nMsgDebug < TestXSens.nMsgSize)
		//{
		//	TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("Length = {0}, {1}, {2}", q.Length, TestXSens.nMarkers_modelInt, markPos.Length);
		//	TestXSens.nMsgDebug++;

		//	for (int i = 0; i < TestXSens.nMarkers_modelInt; i++)
		//	{
		//		TestXSens.msgDebug[TestXSens.nMsgDebug] = string.Format("markPos = ");
		//		for (int j = 0; j < 3; j++)
		//			TestXSens.msgDebug[TestXSens.nMsgDebug] += string.Format("{0}, ", markPos[i * 3 + j]);
		//		TestXSens.nMsgDebug++;
		//	}
		//}

		return markPos;
	}
}
