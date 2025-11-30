using UnityEngine;

namespace DoodleStudio95 {
	internal class NameGenerator {
		static string[] names1 = new string[] {
			"super",
			"awesome",
			"cute",
			"best",
			"bestest",
		};

		static string[] names2 = new string[] {
			"Drawing",
			"DRaWIng",
			"DraWING",
			"Sketch",
			"Sketcch",
			"Filename",
			"MySketch",
		};

		static string[] names3 = new string[] {
			"666",
			"898798",
			"_asdf",
			"v2final",
			"_1.1",
			"_---thisONE",
			"JPEG",
			"_FINAL",
			"---bestone",
		};

		internal static string GetName() {
			return 
				names1[Random.Range(0, names1.Length - 1)] +
				names2[Random.Range(0, names2.Length - 1)] +
				names3[Random.Range(0, names3.Length - 1)];
		} 
	}
}