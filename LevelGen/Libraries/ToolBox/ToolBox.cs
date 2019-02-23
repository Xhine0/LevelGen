using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ToolBox.Syntax {
	[System.Serializable]
	public class STuple<A,B> : System.Tuple<A,B> {
		[SerializeField]
		private A a;
		[SerializeField]
		private B b;

		public STuple(A item1, B item2) : base(item1, item2) {
			a = item1;
			b = item2;
		}
	}

	public static class Syntax {
		public static void For(int origin, int limit, int step, System.Action<int> action) {
			for (int i = origin; i < limit; i += step)
				action(i);
		}

		public static void Switch<T>(T x, params (T, System.Action)[] cases) {
			foreach (var (e, func) in cases) {
				if (x.Equals(e)) {
					func();
					return;
				}
			}
		}
		public static void Switch<T>(T x, params (T, System.Action<T>)[] cases) {
			foreach (var (e, func) in cases) {
				if (x.Equals(e)) {
					func(e);
					return;
				}
			}
		}
		public static T Switch<T>(T x, params (T, System.Func<T>)[] cases) {
			foreach (var (e, func) in cases) {
				if (x.Equals(e)) {
					return func();
				}
			}
			return default;
		}
		public static O Switch<I, O>(I x, params (I, System.Func<O>)[] cases) {
			foreach (var (e, func) in cases) {
				if (x.Equals(e)) {
					return func();
				}
			}
			return default;
		}
		public static O Switch<I, O>(I x, params (I, System.Func<I, O>)[] cases) {
			foreach (var (e, func) in cases) {
				if (x.Equals(e)) {
					return func(e);
				}
			}
			return default;
		}
	}
}
namespace ToolBox.Data {
	/// <summary>
	/// Inclusive interval
	/// </summary>
	public class Interval {
		public float min, max;
		public Interval(float min, float max) {
			this.min = min;
			this.max = max;
		}

		public float Mid => (min + max) / 2;
		public float Length => max - min;
		public float Difference(Interval other) => Mathf.Abs(Length - other.Length);
		public Interval Intersection(Interval other) => new Interval(Mathf.Max(min, other.min), Mathf.Min(max, other.max));
		public bool Within(float value) => value >= min && value <= max;
		public bool Within(Interval other) => min >= other.min && max <= other.max;
		public bool Intersects(Interval other) => max >= other.min || min < other.max;
		public float Clamp(float value) => value < min ? min : value > max ? max : value;

		public (float, float) Tuple => (min, max);

		public override bool Equals(object obj) {
			var interval = obj as Interval;
			return interval != null &&
				   min == interval.min &&
				   max == interval.max;
		}

		public override int GetHashCode() {
			var hashCode = -897720056;
			hashCode = hashCode * -1521134295 + min.GetHashCode();
			hashCode = hashCode * -1521134295 + max.GetHashCode();
			return hashCode;
		}

		public override string ToString() => "<" + min + "," + max + ">";

		public void DrawLine(Vector3 dir, Vector3 offset, Color color) => Debug.DrawLine(offset + dir * min, offset + dir * max, color);
		public void DrawLine(Vector3 dir, Color color) => DrawLine(dir, Vector3.zero, color);

		internal Interval Operate(Interval other, System.Func<float, float, float> func) => new Interval(func(min, other.min), func (max, other.max));
		internal Interval Operate(float value, System.Func<float, float, float> func) => new Interval(func(min, value), func(max, value));

		public static Interval operator +(Interval interval1, Interval interval2) => interval1.Operate(interval2, (a, b) => a + b);
		public static Interval operator -(Interval interval1, Interval interval2) => interval1.Operate(interval2, (a, b) => a - b);
		public static Interval operator *(Interval interval1, Interval interval2) => interval1.Operate(interval2, (a, b) => a * b);

		public static Interval operator +(Interval interval, float value) => interval.Operate(value, (a, b) => a + b);
		public static Interval operator -(Interval interval, float value) => interval.Operate(value, (a, b) => a - b);
		public static Interval operator *(Interval interval, float factor) {
			float delta = (1 - factor) * interval.Length / 2;
			return new Interval(interval.min + delta, interval.max - delta);
		}

		public static bool operator ==(Interval interval1, Interval interval2) => EqualityComparer<Interval>.Default.Equals(interval1, interval2);
		public static bool operator !=(Interval interval1, Interval interval2) => !(interval1 == interval2);
	}

	public static class Compass {
		public static Vector2 NW => new Vector2(-1, 1);
		public static Vector2 N => new Vector2(0, 1);
		public static Vector2 NE => new Vector2(1, 1);
		public static Vector2 W => new Vector2(-1, 0);
		public static Vector2 C => new Vector2(0, 0);
		public static Vector2 E => new Vector2(1, 0);
		public static Vector2 SW => new Vector2(-1, -1);
		public static Vector2 S => new Vector2(0, -1);
		public static Vector2 SE => new Vector2(1, -1);
	}

	public static class Resources {
		public static Dictionary<string, Vector2Int> Compass9 { get; private set; } = new Dictionary<string, Vector2Int>() {
			{ "nw", new Vector2Int(-1,  1) }, { "n", new Vector2Int(0,  1) }, { "ne", new Vector2Int(1,  1) },
			{  "w", new Vector2Int(-1,  0) }, { "c", new Vector2Int(0,  0) }, {  "e", new Vector2Int(1,  0) },
			{ "sw", new Vector2Int(-1, -1) }, { "s", new Vector2Int(0, -1) }, { "se", new Vector2Int(1, -1) }
		};
		public static Dictionary<string, Vector2Int> Compass5 { get; private set; } = new Dictionary<string, Vector2Int>() {
												{ "n", new Vector2Int(0,  1) },
			{  "w", new Vector2Int(-1,  0) }, { "c", new Vector2Int(0,  0) }, {  "e", new Vector2Int(1,  0) },
												{ "s", new Vector2Int(0, -1) }
		};

		public static Dictionary<string, char> Symbols { get; } = new Dictionary<string, char>() {
			{ "left", '←' }, { "right", '→' }, { "up", '↑' }, { "down", '↓' }, { "leftRight", '↔' }, { "upDown", '↕' },
			{ "chevronLeft", '◄' }, { "chevronRight", '►' }, { "chevronUp", '▲' }, { "chevronDown", '▼' },
			{ "note", '♪' }, { "noteDouble", '♫' },
			{ "yes", '✓' }, { "no", '✗'},
			{ "true", '✓' }, { "false", '✗'},
			{ "dagger", '†' }, { "doubleDagger", '‡' }
		};
		
		public enum ColorEnum { Black, Blue, Clear, Cyan, Gray, Green, Grey, Magenta, Red, White, Yellow, Random }
	}
}
namespace ToolBox.Calculations {
	public static class Math {
		#region Basic Math Functions
		/**
		 * Uses Bhaskara I's approximation formula (converted for using radians), for a more efficient calculation of sin
		*/
		public static float ApproxSin(float r) {
			float pi = Mathf.PI;
			return (16 * r * (pi - r)) / (5 * pi * pi - 4 * r * (pi - r));
		}

		public static int IntDiv(int a, int b) {
			return (int)SafeDiv(a, b);
		}
		public static float SafeDiv(int a, int b) {
			return (float)a / b;
		}

		public static bool InInterval(float val, float min, float max) {
			return val >= min && val <= max;
		}

		public static float RoundFloat(float val, int floatPrecision) {
			return Mathf.Round(val * Mathf.Pow(10, floatPrecision)) / Mathf.Pow(10, floatPrecision);
		}

		public static int Sign(float val) => val == 0 ? 0 : (int)Mathf.Sign(val);
		public static Vector3 Sign(Vector3 v) => new Vector3(Sign(v.x), Sign(v.y), Sign(v.z));

		public static float Filter(float v, float f) {
			return f == 0 ? 0 : v;
		}
		public static Vector3 Filter(Vector3 v, Vector3 f) {
			return new Vector3(Filter(v.x, f.x), Filter(v.y, f.y), Filter(v.z, f.z));
		}
		public static Vector2 Filter(Vector2 v, Vector3 f) {
			return new Vector2(Filter(v.x, f.x), Filter(v.y, f.y));
		}

		public static float ClampMin(float val, float min) {
			if (val < min) {
				return min;
			}
			return val;
		}
		public static float ClampMax(float val, float max) {
			if (val < max) {
				return max;
			}
			return val;
		}
		public static float WrapClamp(float val, float min, float max) {
			if (val > max) { return min; }
			else if (val < min) { return max; }
			return val;
		}

		public static int AbsRound(float val) {
			return Sign(val) * Mathf.RoundToInt(Mathf.Abs(val));
		}
		public static int AbsFloor(float val) {
			return Sign(val) * Mathf.FloorToInt(Mathf.Abs(val));
		}
		public static int AbsCeil(float val) {
			return Sign(val) * Mathf.CeilToInt(Mathf.Abs(val));
		}

		public static float SignedMin(float a, float b) {
			if (Mathf.Abs(a) < Mathf.Abs(b)) return a;
			return b;
		}
		public static float SignedMax(float a, float b) {
			if (Mathf.Abs(a) > Mathf.Abs(b)) return a;
			return b;
		}

		public static float Average(float[] nums) {
			float sum = 0;
			for (int i = 0; i < nums.Length; i++) {
				sum += nums[i];
			}
			return sum / nums.Length;
		}
		public static float Average(Color col) {
			return Average(new float[] { col.r, col.g, col.b });
		}
		#endregion

		#region Compound Math Functions
		public static float EffectiveRadius(Vector3 center, Vector3 point, float radius) {
			return Mathf.Abs(Mathf.Cos(Mathf.Asin((point.y - center.y) / radius) * Mathf.PI)) * radius;
		}

		public static bool CheckFloatTolerance(float a, float b, float ratio, float maxVal) {
			return Mathf.Abs(a - b) / maxVal <= Mathf.Clamp01(ratio);
		}
		#endregion

		#region Vector Calculations
		/// <summary>
		/// Converts an angle (in degrees) to a periphery point on the unit circle
		/// </summary>
		/// <param name="angle">Angle in degrees</param>
		/// <returns>Corresponding periphery point on the unit circle</returns>
		public static Vector2 AngleToVector(float angle) {
			return Quaternion.AngleAxis(angle, Vector3.forward) * Vector2.right;
		}

		public static Vector2 RandomVector2(Vector2 min, Vector2 max) {
			return new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));
		}
		public static Vector3 RandomVector3(Vector3 min, Vector3 max) {
			return new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
		}

		public static Vector2 Closest(Vector2 target, params Vector2[] points) {
			Vector2 point = points[0];
			foreach (Vector2 p in points)
				if (Vector2.Distance(p, target) < Vector2.Distance(point, target))
					point = p;
			return point;
		}
		public static Vector3 Closest(Vector3 target, params Vector3[] points) {
			Vector3 point = points[0];
			foreach (Vector3 p in points)
				if (Vector3.Distance(p, target) < Vector3.Distance(point, target))
					point = p;
			return point;
		}
		#endregion
	}
	public static class Predicates {
		public static bool AllIsOfType<T>(object[] array) {
			foreach (object elem in array)
				if (elem == null || elem.GetType() != typeof(T)) return false;
			return true;
		}

		public static bool IsSubclassOfRawGeneric(System.Type generic, System.Type toCheck) {
			while (toCheck != null && toCheck != typeof(object)) {
				var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
				if (generic == cur) {
					return true;
				}
				toCheck = toCheck.BaseType;
			}
			return false;
		}

		public static int MatchCount(params bool[] vals) {
			int i = 0;
			foreach (bool v in vals) if (v) i++;
			return i;
		}
	}
	public static class Physics {
		#region Projections
		public static bool OnScreen(Vector3 p) {
			Vector2 vPos = Camera.main.WorldToViewportPoint(p);
			return vPos.x >= 0 && vPos.x <= 1 && vPos.y >= 0 && vPos.y <= 1;
		}
		#endregion

		#region Raycast Calculations

		#region Basic Raycasting (3D)
		public static bool CastRayHit(Vector3 start, Vector3 direction, float range, LayerMask hitMask, Color color) {
			Ray ray = new Ray(start, direction);
			if (UnityEngine.Physics.Raycast(ray, range, hitMask)) {
				Debug.DrawRay(start, direction * range, color);
				return UnityEngine.Physics.Raycast(ray, range, hitMask);
			}
			return false;
		}

		public static GameObject CastRay(Vector3 start, Vector3 direction, float range, LayerMask hitMask, Color color) {
			Ray ray = new Ray(start, direction);
			if (UnityEngine.Physics.Raycast(ray, range, hitMask)) {
				RaycastHit hitRay = UnityEngine.Physics.RaycastAll(ray, range, hitMask)[0];
				Debug.DrawRay(start, direction * range, color);

				return hitRay.transform.gameObject;
			}
			return null;
		}

		public static GameObject CastLine(Vector3 start, Vector3 end, float stopDistance, LayerMask hitMask, Color color) {
			return CastRay(start, GetRayDirection(start, end), GetRayRange(start, end, stopDistance), hitMask, color);
		}
		public static GameObject CastLine(Vector3 start, Vector3 end, LayerMask hitMask, Color color) {
			return CastLine(start, end, 0, hitMask, color);
		}

		public static bool CastLineHit(Vector3 start, Vector3 end, float stopDistance, LayerMask hitMask, Color color) {
			float distance = Vector3.Distance(start, end);
			end *= Mathf.Max(0, (distance - stopDistance) / distance);

			bool hit = UnityEngine.Physics.Linecast(start, end, hitMask);
			if (hit) {
				Debug.DrawLine(start, end);
			}
			return hit;
		}
		public static bool CastLineHit(Vector3 start, Vector3 end, LayerMask hitMask, Color color) {
			return CastLineHit(start, end, 0, hitMask, color);
		}

		public static GameObject CastSphere(Vector3 center, float radius, LayerMask hitMask) {
			RaycastHit hitRay = UnityEngine.Physics.SphereCastAll(center, radius, Vector3.one * radius, hitMask)[0];
			GameObject obj = hitRay.transform.gameObject;

			return hitMask == (hitMask | (1 << obj.layer)) ? obj : null;
		}
		public static GameObject[] CastSphereAll(Vector3 center, float radius, LayerMask hitMask) {
			RaycastHit[] hitRay = UnityEngine.Physics.SphereCastAll(center, radius, Vector3.one * radius, hitMask);
			GameObject[] objs = new GameObject[hitRay.Length];
			for (int i = 0; i < hitRay.Length; i++) {
				objs[i] = hitRay[i].transform.gameObject;
			}

			return objs;
		}
		#endregion

		#region Basic Raycasting (2D)
		public static bool CastRayHit2D(Vector3 start, Vector3 direction, float range, LayerMask hitMask, Color color) {
			bool hit = Physics2D.Raycast(start, direction, range, hitMask);
			if (hit) {
				Debug.DrawRay(start, direction * range, color);
				return hit;
			}
			return false;
		}

		public static GameObject CastRay2D(Vector2 start, Vector2 direction, float range, LayerMask hitMask, Color color) {
			if (Physics2D.Raycast(start, direction, range, hitMask)) {
				Debug.DrawRay(start, direction * range, color);
				RaycastHit2D[] hits = Physics2D.RaycastAll(start, direction, range, hitMask);

				return hits.Length == 0 ? null : hits[0].transform.gameObject;
			}
			return null;
		}
		public static GameObject CastRay2D(Ray ray, float range, LayerMask hitMask, Color color) {
			return CastRay2D(ray.origin, ray.direction, range, hitMask, color);
		}

		public static GameObject CastLine2D(Vector3 start, Vector3 end, float stopDistance, LayerMask hitMask, Color color) {
			return CastRay2D(start, GetRayDirection(start, end), GetRayRange(start, end, stopDistance), hitMask, color);
		}
		public static GameObject CastLine2D(Vector3 start, Vector3 end, LayerMask hitMask, Color color) {
			return CastLine2D(start, end, 0, hitMask, color);
		}

		public static bool CastLineHit2D(Vector3 start, Vector3 end, float stopDistance, LayerMask hitMask, Color color) {
			float distance = Vector3.Distance(start, end);
			end *= Mathf.Max(0, (distance - stopDistance) / distance);

			if (Physics2D.Linecast(start, end, hitMask)) {
				Debug.DrawLine(start, end);
				return true;
			}
			return false;
		}
		public static bool CastLineHit2D(Vector3 start, Vector3 end, LayerMask hitMask, Color color) {
			return CastLineHit2D(start, end, 0, hitMask, color);
		}
		#endregion

		#region Raycast Properties Calculation
		public static Vector3 GetRayDirection(Vector3 start, Vector3 end) {
			return (end - start).normalized;
		}
		public static float GetRayRange(Vector3 start, Vector3 end, float stopDistance) {
			return Vector3.Distance(start, end) - stopDistance;
		}
		#endregion

		#endregion

		#region Compound Raycasting

		private static bool VisibleCheck(GameObject obj, GameObject target, Vector3 originPos) {
			bool visible = obj == target;

			if (!visible)
				Debug.DrawLine(originPos, target.transform.position, Color.red);
			if (obj != null)
				Debug.DrawLine(originPos, obj.transform.position, Color.green);

			return visible;
		}

		#region Compound Raycasting 3D
		public static bool InLineOfSight(GameObject target, Vector3 originPos, float stopDistance, LayerMask mask) {
			GameObject obj = CastLine(originPos, target.transform.position, stopDistance, mask, Color.red);

			return VisibleCheck(obj, target, originPos);
		}
		public static bool InLineOfSight(GameObject target, Vector3 originPos, LayerMask mask) {
			return InLineOfSight(target, originPos, 0, mask);
		}
		#endregion

		#region Compound Raycasting 2D
		public static bool InLineOfSight2D(GameObject target, Vector2 originPos, float stopDistance, LayerMask mask) {
			GameObject obj = CastLine2D(originPos, target.transform.position, stopDistance, mask, Color.red);
			return VisibleCheck(obj, target, originPos);
		}
		public static bool InLineOfSight2D(GameObject target, Vector2 originPos, LayerMask mask) {
			return InLineOfSight2D(target, originPos, 0, mask);
		}

		public static bool BrushForPath2D(Vector2 lookFrom, Vector2 lookTo, out Vector2 p, float lookDistance, float stepAmount, LayerMask sightMask, GameObject target, HashSet<Vector2> visited) {
			if (stepAmount <= 0) stepAmount = 1;
			p = lookTo;
			if (InLineOfSight2D(target, lookFrom, sightMask)) return true;

			bool pFound = false;
			for (float y = -lookDistance / 2; y < lookDistance / 2; y += stepAmount) {
				for (float x = -lookDistance / 2; x < lookDistance / 2; x += stepAmount) {
					Vector2 point = (lookFrom + new Vector2(x, y)).Round();
					if (visited.Contains(point)) continue;
					visited.Add(point);

					GameObject hit = CastLine2D(lookFrom, point, sightMask, new Color(0,0,0,0));
					if (hit == null || hit == target) {
						Utility.Aesthetic.DrawPoint(point, 0.25f, Color.green, 0);
						p = pFound ? Math.Closest(lookTo, point, p) : point;
						pFound = true;
					}
				}
			}

			foreach (Vector2 v in visited)
				Utility.Aesthetic.DrawPoint(v, 0.25f, new Color(0.5f, 0.5f, 0.5f, 0.2f), 0);

			return pFound;
		}

		public static List<Transform> CastCollider2D(Transform transform, Vector2 offset, Vector2 sizeMultiplier) {
			Vector2 pos = transform.position.ToXY() + offset;
			Vector2 size = (transform.GetComponent<Collider2D>().bounds.size).Mult(sizeMultiplier);
			Utility.Aesthetic.DrawPoint(pos, size, Color.yellow);

			List<Transform> objs = new List<Transform>();
			foreach (RaycastHit2D hit in Physics2D.BoxCastAll(pos, size, 0, Vector2.zero)) {
				objs.Add(hit.transform);
			}
			return objs;
		}
		public static List<Transform> CastCollider2D(Transform transform) {
			return CastCollider2D(transform, Vector2.zero, Vector2.one);
		}

		public static bool CastColliderTop2D(Transform transform, LayerMask triggerMask) {
			Vector2 offset = Vector2.up * 0.1f;
			var (NW, _, NE, _, _, _, _, _, _) = GetColliderExtents(transform);
			Vector2 a = NW + offset, b = NE + offset;

			Debug.DrawLine(a, b, Color.yellow);

			foreach (RaycastHit2D hit in Physics2D.RaycastAll(a, Vector2.right, Vector3.Distance(a, b))) {
				if (triggerMask.Contains(hit.transform.gameObject))
					return true;
			}
			return false;
		}
		#endregion

		#endregion

		#region Physics Calculations
		public static (Vector2 NW, Vector2 N, Vector2 NE, 
					   Vector2 W,  Vector2 C, Vector2 E, 
			           Vector2 SW, Vector2 S, Vector2 SE) 
			GetColliderExtents(Transform obj, float sizeMultiplier) {
			Vector2 extents = obj.GetComponent<Collider2D>().bounds.extents.ToXY() * sizeMultiplier;
			Vector2[] result = new Vector2[9];
			int i = 0;
			Data.Resources.Compass9.Values.Perform((dir) => result[i++] = obj.position.ToXY() + extents * dir);
			return (result[0], result[1], result[2], result[3], result[4], result[5], result[6], result[7], result[8]);
		}
		public static (Vector2 NW, Vector2 N, Vector2 NE,
					   Vector2 W, Vector2 C, Vector2 E,
					   Vector2 SW, Vector2 S, Vector2 SE) 
			GetColliderExtents(Transform obj) => GetColliderExtents(obj, 1);

		public static float Accelerate(float speed, float baseSpeed, float accelTime) {
			if (accelTime <= 0) return 0;
			float result = speed + baseSpeed * (1 / accelTime) * Time.deltaTime * Math.Sign(speed);
			return Math.Sign(result) != Math.Sign(speed) ? 0 : result;
		}
		#endregion
	}
}
namespace ToolBox.Management {
	public static class Game {
		#region Scene Management
		public static void ReloadScene() {
			LoadScene(SceneManager.GetActiveScene().buildIndex);
		}
		public static void LoadScene(int buildIndex) {
			SceneManager.LoadScene(buildIndex);
		}
		public static void RemoveInactiveScenes() {
			for (int i = 0; i < SceneManager.sceneCount; i++)
				if (i != SceneManager.GetActiveScene().buildIndex)
					SceneManager.UnloadSceneAsync(i);
		}
		public static void DestroyObj(Object obj) {
			Object.Destroy(obj);
		}
		public static void DestroyObjImmediate(Object obj) {
			Object.DestroyImmediate(obj);
		}
		#endregion

		#region Spawning
		public static GameObject CreateGameObject(Transform parent, string name, Vector3 localPos) {
			GameObject obj = new GameObject();
			Transform t = obj.transform;
			t.name = name;
			t.parent = parent;
			t.localPosition = localPos;
			t.rotation = new Quaternion(0, 0, 0, 0);

			return obj;
		}
		public static GameObject CreateGameObject(Transform parent, string name) {
			return CreateGameObject(parent, name, Vector3.zero);
		}
		#endregion

		#region Inheritance Management
		public static List<T> GetAllComponentsRecursive<T>(Transform parent) where T : Component {
			List<T> components = new List<T>();

			components.AddRange(parent.GetComponents<T>());
			foreach (Transform child in parent) {
				components.AddRange(GetAllComponentsRecursive<T>(child));
			}

			return components;
		}
		#endregion

		#region File Management
		/// <summary>
		/// [CURRENTLY NOT FUNCTIONAL] Creates all folders needed to create file
		/// </summary>
		/// <param name="path">Path of file to create folders for</param>
		private static void CreatePath(string path) {
			path = path.Replace("Assets/", "").Replace("Assets\\", "");
			string folderPath = "Assets/";

			foreach (string subPath in path.Split('/', '\\')) {
				if (subPath.Contains(".")) break;
				folderPath += subPath + "/";

				if (!File.Exists(folderPath))
					File.Create(folderPath);
			}
			if (folderPath.Length != 0) Debug.Log("Created path '" + folderPath + "'");
		}

		public static void WriteFile(string path, Texture2D tex, bool debug) {
			//CreatePath(path);
			byte[] bytes = tex.EncodeToPNG();
			File.WriteAllBytes(path, bytes);
			if (debug) Debug.Log("Texture saved to path '" + path + "'");
		}
		public static void WriteFile(string path, Texture2D tex) {
			WriteFile(path, tex, false);
		}
		public static void WriteFile(string path, string text) {
			//CreatePath(path);
			File.WriteAllText(path, text);
		}

		public static string ChangeFileExtension(string path, string extension) {
			return path.Split('.')[0] + "." + extension;
		}

		/// <summary>
		/// Return names of all files in directory (not recursive)
		/// </summary>
		/// <param name="path">Directory path</param>
		/// <param name="extensions">File extension filter (leave blank to include all extensions)</param>
		/// <returns></returns>
		public static List<string> FilesInDirectory(string path, params string[] extensions) {
			List<string> fileNames = new List<string>();
			foreach (string file in Directory.GetFiles(path))
				if (extensions.Contains(file.Split('.')[1])) fileNames.Add(file);
			return fileNames;
		}
		#endregion

		#region Mouse Managemet
		public static T ClickedObj<T>(LayerMask hitMask) where T : Component {
			Ray ray = Camera.main.ViewportPointToRay(Camera.main.ScreenToViewportPoint(Input.mousePosition));
			GameObject obj = Calculations.Physics.CastRay2D(ray, 1, hitMask, Color.magenta);
			return obj?.GetComponent<T>();
		}
		#endregion
	}
	public static class Collections {
		public static bool Transfer<T>(T elem, List<T> from, List<T> to) {
			if (!from.Contains(elem)) return false;

			to.Add(elem);
			from.Remove(elem);
			return true;
		}
	}
}
namespace ToolBox.Utility {
	public static class Text {
		public static char RandomAlphaNumericChar() {
			System.Random rnd = new System.Random();
			return (char)rnd.Next(48, 90);
		}
		public static string RandomAlphaNumericString(int n) {
			System.Random rnd = new System.Random();
			System.Text.StringBuilder str = new System.Text.StringBuilder();
			for (int i = 0; i < n; ++i) str.Append((char)rnd.Next(48, 90));
			return str.ToString();
		}
		public static string FreshId(string id) {
			string name = "";
			string num = "";
			bool readNum = true;
			foreach (char c in id.ToCharArray().Reverse()) {
				if (!char.IsDigit(c)) readNum = false;
				if (readNum) num = c + num;
				else name = c + name;
			}
			return num.Length == 0 ? name + "1" : name + (int.Parse(num) + 1);
		}
	}

	public static class Conversions {
		#region Math Conversions
		public static Data.Interval ToInterval(float value, float delta) => new Data.Interval(value - delta, value + delta);
		#endregion

		#region Vector Conversions
		public static Vector2 BoolsToVector(bool x, bool y) {
			return new Vector2(x ? 1 : 0, y ? 1 : 0);
		}
		public static Vector3 BoolsToVector(bool x, bool y, bool z) {
			return new Vector3(x ? 1 : 0, y ? 1 : 0, z ? 1 : 0);
		}

		public static string BoolToSymbol(bool state) {
			if (state) { return Data.Resources.Symbols["yes"].ToString(); }
			return Data.Resources.Symbols["no"].ToString();
		}

		public static string ColorToRGB(Color col) {
			return "(" + col.r * 255 + ", " + col.g * 255 + ", " + col.b * 255 + ")";
		}
		public static string ColorToRGBA(Color col) {
			return "(" + col.r * 255 + ", " + col.g * 255 + ", " + col.b * 255 + ", " + col.a + ")";
		}

		public static Dictionary<T, int> CountElements<T>(ICollection<T> collection) {
			Dictionary<T, int> countTable = new Dictionary<T, int>();
			foreach (T elem in collection) {
				if (countTable.ContainsKey(elem)) countTable[elem]++;
				else countTable.Add(elem, 1);
			}
			return countTable;
		}
		public static Dictionary<K, int> CombineCountTables<K>(Dictionary<K, int> a, Dictionary<K, int> b) {
			foreach (K key in b.Keys) {
				if (a.ContainsKey(key)) a[key] += b[key];
				else a.Add(key, b[key]);
			}
			return a;
		}
		#endregion

		#region Color & Texture Conversions
		public static Color RGBToColor(int r, int g, int b, int a) {
			float d = 255.0f;
			r = (int)Mathf.Clamp(r, 0, d);
			g = (int)Mathf.Clamp(g, 0, d);
			b = (int)Mathf.Clamp(b, 0, d);

			return new Color(r / d, g / d, b / d, a / d);
		}
		public static Color RGBToColor(int r, int g, int b) {
			return RGBToColor(r, g, b, 255);
		}
		public static Color RGBToColor(int c) {
			return RGBToColor(c, c, c);
		}

		public static Texture2D ColorToTexture(int width, int height, Color color) {
			Texture2D tex = new Texture2D(width, height);
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					tex.SetPixel(x, y, color);
				}
			}
			tex.Apply();

			return tex;
		}
		public static Texture2D ColorToTexture(Vector2 size, Color color) {
			return ColorToTexture(Mathf.RoundToInt(size.x), Mathf.RoundToInt(size.y), color);
		}
		public static Texture2D ColorToTexture(int size, Color color) {
			return ColorToTexture(size, size, color);
		}
		#endregion
	}
	public static class Aesthetic {
		public static Color RandomColor() {
			return new Color(Random.value, Random.value, Random.value, 1);
		}

		public static float ColorLuminance(Color col) {
			return col.r * 0.3f + col.g * 0.59f + col.b * 0.11f;
		}

		public static Texture2D MakeTex(int width, int height, Color col) {
			Color[] pix = new Color[width * height];
			for (int i = 0; i < pix.Length; ++i) {
				pix[i] = col;
			}
			Texture2D result = new Texture2D(width, height);
			result.SetPixels(pix);
			result.Apply();
			return result;
		}

		public static Texture2D AddColorToTexture(Texture2D tex, Color addCol) {
			Color[] pixels = tex.GetPixels();
			for (int i = 0; i < pixels.Length; i++) {
				int x = i % tex.width;
				int y = i / tex.width;
				tex.SetPixel(x, y, AddColors(pixels[i], addCol));
			}
			tex.Apply();

			return tex;
		}

		public static Color AddColors(Color colA, Color colB) {
			float r = Mathf.Clamp01(colA.r + colB.r);
			float g = Mathf.Clamp01(colA.g + colB.g);
			float b = Mathf.Clamp01(colA.b + colB.b);
			return new Color(r, g, b);
		}

		#region Rendering
		/// <summary>
		/// Takes a photo of a given object.
		/// All references to subjects and lights on stage will be configured to not interfere rest of scene
		/// </summary>
		/// <param name="template">Template of photo subject (will be spawned onto stage, if necessary)</param>
		/// <param name="width">Photo width in pixels</param>
		/// <param name="height">Photo height in pixels</param>
		/// <param name="cam">Target camera</param>
		/// <param name="lights">Included lights</param>
		/// <returns></returns>
		public static Texture2D TakePhotoOfObject(Transform template, int width, int height, Camera cam, params Light[] lights) {
			GameObject photoStage = GameObject.Find("PhotoStage");
			if (photoStage == null) Management.Game.CreateGameObject(null, "PhotoStage");

			if (cam == null) {
				Debug.LogError("Couldn't take photo (camera is null)");
				return null;
			}
			GameObject obj = photoStage.transform.Find(template.name)?.gameObject;
			if (obj == null) {
				if (template == null) {
					Debug.LogError("Couldn't spawn photo subject (template is null)");
					return null;
				}
				Debug.Log("Photo subject doesn't exist. Spawning...");
				obj = (Object.Instantiate(template, Vector3.zero, Quaternion.Euler(Vector3.zero)) as Transform).gameObject;
				obj.transform.parent = photoStage.transform;
				obj.transform.name = template.name;
				obj.SetLayersRecursive(LayerMask.NameToLayer("IgnoreCamera"));
				obj.RemoveAllComponentsRecursive(typeof(Transform), typeof(MeshFilter), typeof(Renderer));
			}
			foreach (Renderer renderer in obj.GetComponentsRecursive<Renderer>()) renderer.enabled = true;

			if (cam.orthographic) cam.orthographicSize = Mathf.Max(obj.transform.lossyScale.x, obj.transform.lossyScale.y) / 2;

			cam.cullingMask = LayerMask.GetMask("IgnoreCamera");
			foreach (Light light in lights) light.cullingMask = LayerMask.GetMask("IgnoreCamera");

			cam.transform.LookAt(obj.transform);

			RenderTexture render = new RenderTexture(width, height, 0);
			cam.targetTexture = render;
			cam.Render();

			Texture2D tex = new Texture2D(width, height);
			RenderTexture.active = render;
			tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			tex.Apply();

			//Debug.Log("Took photo of " + obj.name);
			cam.targetTexture = null;
			RenderTexture.active = null;
			foreach (Renderer renderer in obj.GetComponentsRecursive<Renderer>()) renderer.enabled = false;
			render.Release();   // Garbage collection
			return tex;
		}
		public static void UnStagePhotoSubject(string name) {
			GameObject photoStage = GameObject.Find("PhotoStage");
			if (photoStage != null) {
				foreach (Transform t in photoStage.transform) {
					if (t.name == name) {
						Management.Game.DestroyObjImmediate(t.gameObject);
						break;
					}
				}
			}
		}
		#endregion

		#region Gizmos
		public static void DrawPoint(Vector3 point, float size, Color colorX, Color colorY, Color colorZ, float duration) {
			size = Calculations.Math.ClampMin(size, 0.1f);
			size /= 2;

			Debug.DrawLine(point + Vector3.left * size, point + Vector3.right * size, colorX, duration);
			Debug.DrawLine(point + Vector3.up * size, point + Vector3.down * size, colorY, duration);
			Debug.DrawLine(point + Vector3.forward * size, point + Vector3.back * size, colorZ, duration);
		}
		public static void DrawPoint(Vector3 point, float size, Color color, float duration) {
			DrawPoint(point, size, color, color, color, duration);
		}
		public static void DrawPoint(Vector3 point, float size) {
			DrawPoint(point, size, Color.red, Color.green, Color.blue, 0);
		}
		public static void DrawPoint(Vector3 point, Color color) {
			DrawPoint(point, 1, color, 0);
		}
		public static void DrawPoint(Vector3 point) {
			DrawPoint(point, 1);
		}
		
		public static void DrawPoint(Vector2 point, Vector2 size, Color color) {
			Vector2 extentX = Vector2.right * size.x / 2, extentY = Vector2.up * size.y / 2;
			Debug.DrawLine(point + extentY, point - extentY, color);
			Debug.DrawLine(point - extentX, point + extentX, color);
		}

		public static void DrawArrow(Vector2 a, Vector2 b, Vector2 headSize) {
			headSize = headSize.Mult(new Vector2(-1, 1));
			float angle = Vector2.Angle(Vector2.right, b - a);

			Gizmos.DrawLine(a, b);
			Gizmos.DrawLine(b, b + headSize.RotateAround(Vector2.zero, new Vector3(0, 0, angle)));
			Gizmos.DrawLine(b, b + headSize.RotateAround(Vector2.zero, new Vector3(0, 0, angle + 90)));
		}
		public static void DrawArrow(Vector2 a, Vector2 b, float headSize) {
			DrawArrow(a, b, Vector2.one * headSize);
		}

		public static void DrawShape(Vector3[] points, Vector3 offset, Vector3 anchor, Color color) {
			for (int i = 0; i < points.Length; i++) {
				points[i] -= anchor;
				Debug.DrawLine(points[i] + offset, points[i], color);
			}

			for (int i = 0; i < points.Length - 1; i++) {
				Vector3 A = points[i];
				Vector3 B = points[i + 1];

				Debug.DrawLine(A, B, color);
				Debug.DrawLine(A + offset, B + offset, color);
			}
			Debug.DrawLine(points[points.Length - 1], points[0], color);
			Debug.DrawLine(points[points.Length - 1] + offset, points[0] + offset, color);
		}
		public static void DrawShape(Vector3[] points, Vector3 offset, Vector3 anchor) {
			DrawShape(points, offset, anchor, Color.red);
		}
		public static void DrawShape(Vector3[] points, Vector3 offset) {
			DrawShape(points, offset, Vector3.zero);
		}
		public static void DrawShape(Vector3[] points) {
			DrawShape(points, Vector3.zero, Vector3.zero);
		}
		public static void DrawShape(Vector2[] points, Color color) {
			Vector3[] temp = new Vector3[points.Length];
			for (int i = 0; i < points.Length; i++) {
				Vector2 p = points[i];
				temp[i] = new Vector3(p.x, p.y, 0);
			}
			DrawShape(temp, Vector3.zero, Vector3.zero, color);
		}

		#endregion

		#region Text Formatting
		public static string Indent(int n) {
			string str = "";
			for (int i = 0; i < n; i++)
				str += " ";
			return str;
		}

		public static string FixName(string str) {
			string name = "";
			foreach (char c in str.ToCharArray()) {
				if (c == '_') name += " ";
				else {
					if (char.IsUpper(c) && name.Length != 0) name += " ";
					name += name.Length == 0 ? c : char.ToLower(c);
				}
			}
			return name;
		}

		
		#endregion
	}
}
namespace ToolBox.Algorithms {
	public static class Pathfinding {
		public static int[][] GetPaths((int, int)[] edges, int origin, int[] visited) {
			// Add current node to the explored path
			int[] visited_ = visited.Append(origin);
			// Get all nodes adjacent to the origin
			int[] adj = edges.Filter((e) => e.Item1 == origin).Perform((e) => e.Item2);
			// Undirected implementation: int[] adj = edges.Perform((e) => e.Item1 == origin ? e.Item2 : e.Item2 == origin ? e.Item1 : -1).Filter((i) => i != -1).ToArray();
			// Stop if explored path form a cycle, or there are no more adjacent nodes
			return visited.Contains(origin) || adj.Length == 0 ? new int[][] { visited_ } 
											   : adj.Perform((next) => GetPaths(edges, next, visited_)).Concat();
		}
		public static int[][] GetPaths(
(int, int)[] edges, int origin) => GetPaths(edges, origin, new int[0]);
		public static bool InCycle(int[][] paths, int origin) => paths.Any((c) => c.AmountOf(origin) > 1);
		public static bool InCycle((int, int)[] edges, int origin) => InCycle(GetPaths(edges, origin), origin);
	}
}