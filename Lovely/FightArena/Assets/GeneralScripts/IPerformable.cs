using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPerformable 
{  
	Mind Performer{ get;}
	IEnumerator Perform ();
}

