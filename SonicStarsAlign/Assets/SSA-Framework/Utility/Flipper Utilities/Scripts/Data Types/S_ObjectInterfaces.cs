using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ITriggerable
{
	public void TriggerObjectOn () {

	}

	public void TriggerObjectOff () {

	}

	public void TriggerObjectEachFrame( ) { 
	}

	public void StartTriggeredOn () {
	}
}

public enum TriggerTypes
{
	On,
	//Once,
	Off,
	Either,
	Reset,
	Frame,
	Start
}
