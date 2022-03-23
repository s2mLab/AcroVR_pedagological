using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XDA;

public class XSensData { 

	public List<XsMatrix[]> Data { get; protected set; } = new List<XsMatrix[]>();
	public List<long> TimeStamps { get; protected set; } = new List<long>();
	public long NbFrames { get; protected set; } = 0;

	public void Add(long timeStamps, XsMatrix[] data){
		Data.Add (data);
		TimeStamps.Add (timeStamps);
		NbFrames += 1;
	}

	public void Clear(){
		Data.Clear ();
		TimeStamps.Clear();
		NbFrames = 0;
	}
}
