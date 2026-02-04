using UnityEngine;
using System.Collections;
using System;

public class S_DeactivateOnStart : MonoBehaviour
{

	public enumDeactivate _whatAction = enumDeactivate.Deactivate;
	public float _delayInSeconds;
	[SerializeField] bool _applyOnRespawn;

	public enum enumDeactivate
	{
		stopRendering,
		Destroy,
		Deactivate,
	}

	void Start () {
		StartCoroutine(Delay());

	}

	IEnumerator Delay() {
		yield return new WaitForSeconds(_delayInSeconds);
		Deactivate();
	}

	void Deactivate () {
		if(this == null || this != enabled) { return; }

		switch (_whatAction)
		{
			case enumDeactivate.Deactivate:
				gameObject.SetActive(false);
				break;
			case enumDeactivate.stopRendering:
				gameObject.GetComponent<MeshRenderer>().enabled = false;
				gameObject.GetComponent<Renderer>().enabled = false;
				break;
			case enumDeactivate.Destroy:
				Destroy(gameObject);
				break;
		}
	}

	void EventDeactivateOnReset ( object sender, EventArgs e ) {

		Deactivate();
	}


}
