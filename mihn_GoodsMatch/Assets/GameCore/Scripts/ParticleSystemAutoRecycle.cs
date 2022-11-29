using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemAutoRecycle : MonoBehaviour
{
	void OnParticleSystemStopped()
	{
		gameObject.Recycle();
	}
}
