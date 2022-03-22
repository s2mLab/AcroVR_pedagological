using System;
using System.Runtime.InteropServices;

public class DllManagement
{
	/// <summary> Accès à la fonction LoadLibrary, utilisée pour charger les librairies DLL en mémoire. </summary>
	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr LoadLibrary(string libname);
	/// <summary> Accès à la fonction FreeLibrary, utilisée pour supprimer les librairies DLL en mémoire. </summary>
	[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
	private static extern bool FreeLibrary(IntPtr hModule);

    // =================================================================================================================================================================
    // Load a DLL
    static public IntPtr LoadLib(string path)
	{
		IntPtr ptr = LoadLibrary(path);
		if (ptr == IntPtr.Zero)
		{
			int errorCode = Marshal.GetLastWin32Error();
			UnityEngine.Debug.LogError(string.Format("Failed to load library {1} (ErrorCode: {0})", errorCode, path));
		}
		return ptr;
	}

	// =================================================================================================================================================================
	// Unload a library
	static public void FreeLib(IntPtr handlesDLL)
	{
		FreeLibrary(handlesDLL);
	}
}
