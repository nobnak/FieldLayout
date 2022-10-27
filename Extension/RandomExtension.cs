using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nobnak.FieldLayout.Extensions {

	public static class RandomExtension {
		public const int DEFAULT_SEED = 31;

		static RandomExtension() {
			SetSeed(DEFAULT_SEED);
		}

		static Random.State prevState;

		#region properties
		public static Random.State CurrState { get; set; }
		#endregion

		#region stack
		private static void Pop() {
			Random.state = prevState;
		}
		private static Random.State Push() {
			var prevState = Random.state;
			Random.state = CurrState;
			return prevState;
		}
		#endregion

		#region interfaces
		public static Random.State SetSeed(int seed) {
			Push();
			Random.InitState(seed);
			CurrState = Random.state;
			Pop();
			return CurrState;
		}


		public static float Value {
			get {
				Push();
				var v = Random.value;
				Pop();
				return v;
			}
		}
		public static int Range(int min, int max) {
			Push();
			var i = Random.Range(min, max);
			Pop();
			return i;
		}
		#endregion
	}
}
