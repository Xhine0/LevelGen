using UnityEngine;

public class RoomTrigger : MonoBehaviour {
	public LayerMask triggerMask;
	public Graph.Node exit;

	private LevelGenerator2D level;
	private static bool active = true;

	protected void Start() {
		GetComponent<Collider2D>().isTrigger = true;
		level = GameObject.FindGameObjectWithTag("Level").GetComponent<LevelGenerator2D>();
	}

	private void OnTriggerEnter2D(Collider2D collision) {
		GameObject obj = collision.gameObject;
		if (!triggerMask.Contains(obj.layer)) return;

		if (exit == null || !active) return;
		active = false;
		obj.transform.position = level.Transition(exit, obj);
	}
	private void OnTriggerExit2D(Collider2D collision) {
		if (!triggerMask.Contains(collision.gameObject.layer)) return;

		active = true;
	}
}
