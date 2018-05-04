using UnityEngine;
using System.Collections;

namespace UnityEngine.Analytics {
	public class AdaptiveStateDispatcher
	{
		public delegate void OnExitState(string id);
		public delegate void OnEnterState(string id);
	}
	
}