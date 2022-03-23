using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AvatarManager : MonoBehaviour
{
	public BiorbdModel Model { get; protected set; }
	public XSensModule Module { get; protected set; }
	public abstract void SetSegmentsRotations(double[] q);

	void Start()
    {
		Model = new BiorbdModel(GetBiorbdModelPath());
		Module = new XSensModule(Model.NbImus);
	}

	void Update()
    {

    }

	protected abstract string GetBiorbdModelPath();

	protected void PerformRotation(GameObject segment, Vector3 target)
	{
		if (segment.transform.localEulerAngles != target)
        {
			segment.transform.localEulerAngles = target;
		}
	}
}
