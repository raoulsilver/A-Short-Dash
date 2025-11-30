using UnityEngine;
using UnityEngine.Rendering;

namespace DoodleStudio95 {
[RequireComponent(typeof(SpriteRenderer)), ExecuteInEditMode()]
public class ShadowCastingSprite : MonoBehaviour {
	
	public ShadowCastingMode castShadows = ShadowCastingMode.TwoSided;
	public bool receiveShadows = true;

	public void SetMode() {
		GetComponent<SpriteRenderer>().shadowCastingMode = castShadows;
		GetComponent<SpriteRenderer>().receiveShadows = receiveShadows;
	}
	void OnEnable() {
		SetMode();
	}
	void OnDisable() {
		GetComponent<SpriteRenderer>().shadowCastingMode = ShadowCastingMode.Off;
		GetComponent<SpriteRenderer>().receiveShadows = false;
	}
}
}