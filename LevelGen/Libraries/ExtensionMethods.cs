using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ToolBox.Calculations;

public static class ExtensionMethods {
	#region Global Extensions
	public static bool Check<T>(object e, System.Func<T, bool> func) => e != null && func((T)e);
	public static bool Check(object e, System.Func<bool> func) => e != null && func();
	#endregion

	#region Arithemic Extensions
	public static bool InInterval(this float val, float min, float max) => val >= min && val <= max;
	#endregion

	#region MonoScript Extensions
	public static string Path(this ScriptableObject script) {
		return AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(script));
	}
	public static string Path(this MonoBehaviour script) {
		return AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(script));
	}
	public static string DirectoryPath(this MonoBehaviour script) {
		return AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(script)).Slice('/', -1);
	}
	#endregion

	#region Vector Calculations
	public static Vector2Int ToIntVector(this Vector2 v) {
		return new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
	}
	public static Vector3Int ToIntVector(this Vector3 v) {
		return new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
	}

	public static Vector2 ToFloatVector(this Vector2Int v) {
		return new Vector2(v.x, v.y);
	}
	public static Vector3 ToFloatVector(this Vector3Int v) {
		return new Vector3(v.x, v.y, v.z);
	}

	public static Vector2 Mult(this Vector2 u, Vector2 v) {
		return new Vector2(u.x * v.x, u.y * v.y);
	}
	public static Vector2 Mult(this Vector2 u, float x, float y) {
		return u.Mult(new Vector2(x, y));
	}
	public static Vector2Int Mult(this Vector2Int u, Vector2Int v) {
		return new Vector2Int(u.x * v.x, u.y * v.y);
	}

	/// <summary>
	/// Returns a vector where each element of the given vectors has been multiplied together.
	/// </summary>
	/// <param name="vectA"></param>
	/// <param name="vectB"></param>
	/// <returns>Vector3</returns>
	public static Vector3 Mult(this Vector3 vectA, Vector3 vectB) {
		return new Vector3(vectA.x * vectB.x, vectA.y * vectB.y, vectA.z * vectB.z);
	}
	public static Vector2 MultVectors(Vector2 vectA, Vector2 vectB) {
		return new Vector2(vectA.x * vectB.x, vectA.y * vectB.y);
	}

	/// <summary>
	/// Returns a vector with the value of the float val, divived by the vector vect.
	/// </summary>
	/// <param name="val">Value to divide</param>
	/// <param name="vect">Dividing vector</param>
	/// <returns>val / vect</returns>
	public static Vector3 DivVector(this float val, Vector3 vect) {
		return new Vector3(val / vect.x, val / vect.y, val / vect.z);
	}

	public static Vector3 Div(this Vector3 u, Vector3 v) {
		return new Vector3(u.x / v.x, u.y / v.y, u.z / v.z);
	}

	public static Vector2 Mod(this Vector2 v, float m) {
		return new Vector3(v.x % m, v.y % m);
	}
	public static Vector3 Mod(this Vector3 v, float m) {
		return new Vector3(v.x % m, v.y % m, v.z % m);
	}

	public static Vector2 Flip(this Vector2 v) {
		return new Vector2(v.y, v.x);
	}
	public static Vector2 Flip(this Vector3 v) {
		return new Vector3(v.z, v.y, v.x);
	}

	/// <summary>
	/// Rotate a point around a pivot
	/// </summary>
	/// <param name="point">Point to rotate</param>
	/// <param name="pivot">Point to rotate around</param>
	/// <param name="angles">Rotation angles in degrees</param>
	/// <returns>Rotated point</returns>
	public static Vector3 RotateAround(this Vector3 point, Vector3 pivot, Vector3 angles) {
		return Quaternion.Euler(angles) * (point - pivot) + pivot;
	}
	/// <summary>
	/// Rotate a point around a pivot
	/// </summary>
	/// <param name="point">Point to rotate</param>
	/// <param name="pivot">Point to rotate around</param>
	/// <param name="angles">Rotation angles in degrees</param>
	/// <returns>Rotated point</returns>
	public static Vector2 RotateAround(this Vector2 point, Vector2 pivot, Vector3 angles) => 
		new Vector3(point.x, point.y).RotateAround(pivot, angles);
	public static Vector2 RotateAround(this Vector2 point, Vector2 pivot, float angle) =>
		point.RotateAround(pivot, new Vector3(0, 0, angle));

	public static Vector2 SignedAngleVector(this Vector2 to, Vector2 from) {
		return Math.AngleToVector(Vector2.SignedAngle(from, to));
	}

	public static Vector2 Move(this Vector2 v, Vector2 dir, float distance) {
		return v + dir.normalized * distance;
	}
	public static Vector3 Move(this Vector3 v, Vector3 dir, float distance) {
		return v + dir.normalized * distance;
	}

	public static Vector2 Limit(this Vector2 v, float interval) {
		v = v.normalized;
		float angle = Vector2.Angle(Vector2.right, v);
		angle -= angle % interval;
		angle *= Mathf.Deg2Rad;

		return new Vector2(Mathf.Abs(Mathf.Cos(angle)) * Math.Sign(v.x), Mathf.Abs(Mathf.Sin(angle)) * Math.Sign(v.y)).normalized;
	}

	/// <summary>
	/// Checks if a vector is within a given interval (inclusive)
	/// </summary>
	/// <returns>If the vector is within interval</returns>
	public static bool InInterval(this Vector2 val, Vector2 a, Vector2 b) {
		Vector2 minVal = Vector2.Min(a, b), maxVal = Vector2.Max(a, b);
		return val.x >= minVal.x && val.x <= maxVal.x && val.y >= minVal.y && val.y <= maxVal.y;
	}
	/// <summary>
	/// Checks if the vector is within a given interval (inclusive)
	/// </summary>
	/// <param name="val">Vector to check</param>
	/// <param name="minVal">Min vector [inclusive]</param>
	/// <param name="maxVal">Max vector [inclusive]</param>
	/// <returns>If the vector is within interval</returns>
	public static bool InInterval(this Vector2Int val, Vector2Int minVal, Vector2Int maxVal) {
		return val.x >= minVal.x && val.x <= maxVal.x && val.y >= minVal.y && val.y <= maxVal.y;
	}

	/// <summary>
	/// Checks if the vector is within a given interval (inclusive)
	/// </summary>
	/// <param name="val">Vector to check</param>
	/// <param name="minVal">Min vector [inclusive]</param>
	/// <param name="maxVal">Max vector [inclusive]</param>
	/// <returns>If the vector is within interval</returns>
	public static bool InInterval(this Vector3 val, Vector3 minVal, Vector3 maxVal) {
		return val.x >= minVal.x && val.x <= maxVal.x && val.y >= minVal.y && val.y <= maxVal.y && val.z >= minVal.z && val.z <= maxVal.z;
	}

	public static Vector2Int Clamp(this Vector2Int val, Vector2Int min, Vector2Int max) {
		return new Vector2Int(
			Mathf.Clamp(val.x, min.x, max.x),
			Mathf.Clamp(val.y, min.y, max.y)
		);
	}
	public static Vector3Int Clamp(this Vector3Int val, Vector3Int min, Vector3Int max) {
		return new Vector3Int(
			Mathf.Clamp(val.x, min.x, max.x),
			Mathf.Clamp(val.y, min.y, max.y),
			Mathf.Clamp(val.z, min.z, max.z)
		);
	}
	public static Vector2 Clamp(this Vector2 val, Vector2 min, Vector2 max) {
		return new Vector2(
			Mathf.Clamp(val.x, min.x, max.x),
			Mathf.Clamp(val.y, min.y, max.y)
		);
	}
	public static Vector3 Clamp(this Vector3 val, Vector3 minVal, Vector3 maxVal) {
		return new Vector3(
			Mathf.Clamp(val.x, minVal.x, maxVal.x),
			Mathf.Clamp(val.y, minVal.y, maxVal.y),
			Mathf.Clamp(val.z, minVal.z, maxVal.z)
		);
	}
	public static Vector2 Clamp01(this Vector2 val) {
		return val.Clamp(Vector2.zero, Vector2.one);
	}
	public static Vector3 Clamp(this Vector3 val) {
		return val.Clamp(Vector3.zero, Vector3.one);
	}

	public static Vector3 ClampMin(this Vector3 val, Vector3 minVal) {
		if (val.x < minVal.x) { val.x = minVal.x; }
		if (val.y < minVal.y) { val.y = minVal.y; }
		if (val.z < minVal.z) { val.z = minVal.z; }

		return val;
	}
	public static Vector3 ClampMax(this Vector3 val, Vector3 maxVal) {
		if (val.x > maxVal.x) { val.x = maxVal.x; }
		if (val.y > maxVal.y) { val.y = maxVal.y; }
		if (val.z > maxVal.z) { val.z = maxVal.z; }

		return val;
	}

	public static Vector3 Clamp01(this Vector3 val) {
		return Clamp(val, Vector3.zero, Vector3.one);
	}

	public static Vector2 Abs(this Vector2 v) {
		return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
	}
	public static Vector3 Abs(this Vector3 v) {
		return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
	}

	public static Vector2Int Sign(this Vector2Int v) {
		return new Vector2Int(Math.Sign(v.x), Math.Sign(v.y));
	}
	public static Vector2Int Sign(this Vector2 v) {
		return new Vector2Int(Math.Sign(v.x), Math.Sign(v.y));
	}
	public static Vector3Int Sign(this Vector3Int v) {
		return new Vector3Int(Math.Sign(v.x), Math.Sign(v.y), Math.Sign(v.z));
	}
	public static Vector3Int Sign(this Vector3 v) {
		return new Vector3Int(Math.Sign(v.x), Math.Sign(v.y), Math.Sign(v.z));
	}

	public static Vector2Int Round(this Vector2 v) {
		return new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
	}
	public static Vector2Int RoundByFactor(this Vector2 v, int factor) {
		return (v / (factor * 1.0f)).Round() * factor;
	}
	public static Vector2Int Floor(this Vector2 vect) {
		return new Vector2Int(Mathf.FloorToInt(vect.x), Mathf.FloorToInt(vect.y));
	}
	public static Vector2Int Ceil(this Vector2 vect) {
		return new Vector2Int(Mathf.CeilToInt(vect.x), Mathf.CeilToInt(vect.y));
	}

	public static Vector3Int Round(this Vector3 v) {
		return new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
	}
	public static Vector3Int RoundByFactor(this Vector3 v, int factor) {
		return (v / (factor * 1.0f)).Round() * factor;
	}
	public static Vector3Int Floor(this Vector3 vect) {
		return new Vector3Int(Mathf.FloorToInt(vect.x), Mathf.FloorToInt(vect.y), Mathf.FloorToInt(vect.z));
	}
	public static Vector3Int Ceil(this Vector3 vect) {
		return new Vector3Int(Mathf.CeilToInt(vect.x), Mathf.CeilToInt(vect.y), Mathf.CeilToInt(vect.z));
	}

	public static Vector2Int AbsRound(this Vector2 v) {
		return new Vector2Int(Math.AbsRound(v.x), Math.AbsRound(v.y));
	}
	public static Vector2Int AbsFloor(this Vector2 v) {
		return new Vector2Int(Math.AbsFloor(v.x), Math.AbsFloor(v.y));
	}
	public static Vector2Int AbsCeil(this Vector2 v) {
		return new Vector2Int(Math.AbsCeil(v.x), Math.AbsCeil(v.y));
	}
	public static Vector3Int AbsRound(this Vector3 v) {
		return new Vector3Int(Math.AbsRound(v.x), Math.AbsRound(v.y), Math.AbsRound(v.z));
	}
	public static Vector3Int AbsFloor(this Vector3 v) {
		return new Vector3Int(Math.AbsFloor(v.x), Math.AbsFloor(v.y), Math.AbsFloor(v.z));
	}
	public static Vector3Int AbsCeil(this Vector3 v) {
		return new Vector3Int(Math.AbsCeil(v.x), Math.AbsCeil(v.y), Math.AbsCeil(v.z));
	}

	public static Vector3 NormalizeSign(this Vector3 vect) {
		return Math.Sign(vect).normalized;
	}
	public static Vector3 SafeNormalize(this Vector3 vect) {
		return (vect * vect.magnitude).normalized / vect.magnitude;
	}

	public static Vector3 Toggle(this Vector3 u, Vector3 v, bool toggleX, bool toggleY, bool toggleZ) {
		return new Vector3(
			toggleX ? u.x : v.x,
			toggleY ? u.y : v.y,
			toggleZ ? u.z : v.z
		);
	}

	/// <summary>
	/// For each component respectively, vect is set equal to the modifier, if the sign is the same, and modifier has a greater absolute value
	/// </summary>
	/// <param name="vect">Vector to be modified</param>
	/// <param name="modifier">The modifying vector</param>
	/// <returns></returns>
	public static Vector2 Boost(this Vector2 vect, Vector2 modifier) {
		Vector2 vectAbs = Abs(vect), modAbs = Abs(modifier);
		if (Math.Sign(vect.x) == Math.Sign(modifier.x) && modAbs.x > vectAbs.x) vect.x = modifier.x;
		if (Math.Sign(vect.y) == Math.Sign(modifier.y) && modAbs.y > vectAbs.y) vect.y = modifier.y;
		return vect;
	}

	public static bool CheckForZeroDir(this Vector3 u) {
		return u.x == 0 || u.y == 0 || u.z == 0;
	}

	public static bool CheckForNegativeDir(this Vector3 u) {
		return u.x < 0 || u.y < 0 || u.z < 0;
	}

	public static bool CheckForPositiveDir(this Vector3 u) {
		return u.x > 0 || u.y > 0 || u.z > 0;
	}

	public static Vector2 ToXY(this Vector3 v) {
		return new Vector2(v.x, v.y);
	}

	#endregion

	#region Transform Management
	public static Transform[] GetChildren(this Transform obj) {
		Transform[] children = new Transform[obj.childCount];
		for (int i = 0; i < children.Length; i++)
			children[i] = obj.GetChild(i);
		return children;
	}
	#endregion

	#region GameObject Management
	public static bool InScene(this GameObject obj) => GameObject.FindGameObjectWithTag(obj.tag) != null;

	public static void DestroyThis(this GameObject obj) {
		ToolBox.Management.Game.DestroyObj(obj);
	}

	public static GameObject SetLayersRecursive(this GameObject obj, int layer) {
		obj.layer = layer;
		foreach (Transform t in obj.transform)
			t.gameObject.SetLayersRecursive(layer);
		return obj;
	}
	public static List<T> GetComponentsRecursive<T>(this GameObject obj) where T : Component {
		List<T> components = new List<T>();
		foreach (T component in obj.GetComponents<T>())
			if (component != null) components.Add(component);
		foreach (Transform t in obj.transform)
			components.AddRange(t.gameObject.GetComponentsRecursive<T>());
		return components;
	}
	public static void RemoveComponent<T>(this GameObject obj) where T : Component {
		ToolBox.Management.Game.DestroyObj(obj.GetComponent<T>());
	}
	public static void RemoveComponent(this GameObject obj, System.Type type) {
		ToolBox.Management.Game.DestroyObj(obj.GetComponent(type));
	}
	public static void RemoveComponents<T>(this GameObject obj) where T : Component {
		foreach (T component in obj.GetComponents<T>())
			ToolBox.Management.Game.DestroyObj(component);
	}
	public static void RemoveComponentsRecursive<T>(this GameObject obj) where T : Component {
		foreach (T component in obj.GetComponentsRecursive<T>())
			ToolBox.Management.Game.DestroyObj(component);
	}

	public static void RemoveAllComponents(this GameObject obj, params System.Type[] exclude) {
		foreach (Component component in obj.GetComponents<Component>()) {
			if (!exclude.Contains(component.GetType()))
				obj.RemoveComponent(component.GetType());
		}
	}
	public static void RemoveAllComponentsRecursive(this GameObject obj, params System.Type[] exclude) {
		foreach (Component component in obj.GetComponentsRecursive<Component>()) {
			if (!exclude.Contains(component.GetType()))
				obj.RemoveComponent(component.GetType());
		}
	}

	public static GameObject Root(this GameObject obj) {
		return obj.transform == null ? obj : Root(obj.transform.parent.gameObject);
	}
	#endregion

	#region Collections Management
	public static I Head<I>(this I[] array) {
		return array[0];
	}
	public static I Head<I>(this List<I> list) {
		return list == null ? default : list[0];
	}
	public static I[] Tail<I>(this I[] array) {
		I[] tail = new I[array.Length - 1];
		for (int i = 1; i < array.Length; i++) {
			tail[i - 1] = array[i];
		}
		return tail;
	}
	public static I[] Tail<I>(this List<I> list) {
		return list.ToArray().Tail();
	}

	public static T Last<T>(this List<T> list) => list[list.Count - 1];
	public static T Last<T>(this T[] array) => array[array.Length - 1];

	public static (T, T[]) Step<T>(this T[] array) => (array.Head(), array.Tail());

	public static bool Safe<T>(this ICollection<T> collection, int i) {
		return i >= 0 && i < collection.Count;
	}

	public static T SafeGet<T>(this T[] array, int i) => array.Safe(i) ? array[i] : default;
	public static T SafeGet<T>(this IList<T> list, int i) => list.Safe(i) ? list[i] : default;
	public static V SafeGet<K, V>(this IDictionary<K, V> dictionary, K key) => dictionary.ContainsKey(key) ? dictionary[key] : default;

	public static void SafeSet<T>(this T[] array, int i, T val) {
		if (array.Safe(i)) array[i] = val;
	}
	public static void SafeSet<T>(this IList<T> list, int i, T val) {
		if (list.Safe(i)) list[i] = val;
	}
	public static void SafeSet<T>(this IList<T> list, T val) {
		SafeSet(list, list.IndexOf(val), val);
	}

	public static List<T> ToList<T>(this ICollection<T> collection) {
		List<T> set = new List<T>();
		collection.Perform(set.Add);
		return set;
	}
	public static HashSet<T> ToSet<T>(this ICollection<T> collection) {
		HashSet<T> set = new HashSet<T>();
		collection.Perform(set.Add);
		return set;
	}

	public static Dictionary<K, V> KeysToDictionary<K, V>(this ICollection<K> keys, System.Func<K, V> valueFunc) {
		Dictionary<K, V> result = new Dictionary<K, V>();
		keys.Perform((e) => result.SafeAdd(e, valueFunc(e)));
		return result;
	}
	public static Dictionary<K,V> ValuesToDictionary<K,V>(this ICollection<V> values, System.Func<V,K> keyFunc) {
		Dictionary<K, V> result = new Dictionary<K, V>();
		values.Perform((e) => result.SafeAdd(keyFunc(e), e));
		return result;
	}
	public static Dictionary<K,V> ToDictionary<K,V>(this IList<KeyValuePair<K,V>> pairList) {
		Dictionary<K, V> result = new Dictionary<K, V>();
		pairList.Perform((e) => result.Add(e.Key, e.Value));
		return result;
	}
	public static (K,V)[] ToTupleList<K,V>(this Dictionary<K,V> dictionary) {
		return dictionary.Perform((e) => (e.Key, e.Value));
	}

	public static int AmountOf<T>(this ICollection<T> collection, T match) => collection.Filter((e) => e.Equals(match)).Count;

	public static bool Any<T>(this ICollection<T> collection, System.Func<T, bool> predicate) {
		if (collection == null) return false;
		return collection.FirstMatch(predicate) != null;
	}

	public static T FirstMatch<T>(this ICollection<T> collection, System.Func<T, bool> predicate) {
		if (collection != null) {
			foreach (T elem in collection)
				if (predicate(elem)) return elem;
		}
		return default;
	}

	public static T BestMatch<T>(this ICollection<T> collection, System.Func<int, bool> predicate) where T : System.IComparable<T> {
		T result = default;
		bool set = false;
		foreach (T elem in collection)
			if (!set || predicate(elem.CompareTo(result))) { set = true; result = elem; }
		return result;
	}

	public static T[] Reverse<T>(this T[] array) {
		T[] result = new T[array.Length];
		for (int i = 0; i < array.Length; i++)
			result[i] = array[array.Length - 1 - i];
		return result;
	}

	public static T[] Concat<T>(this T[][] array) {
		List<T> result = new List<T>();
		array.Perform(result.AddRange);
		return result.ToArray();
	}
	public static T[] Concat<T>(this ICollection<T>[] array) {
		List<T> result = new List<T>();
		array.Perform(result.AddRange);
		return result.ToArray();
	}

	public static List<T> Map<T>(this ICollection<T> collection, System.Func<T, T> func) {
		List<T> result = new List<T>();
		foreach (T elem in collection)
			result.Add(func(elem));
		return result;
	}

	public static List<T> Filter<T>(this ICollection<T> collection, System.Func<T, bool> predicate) {
		List<T> result = new List<T>();
		collection.Perform((elem) => {
			if (predicate(elem)) result.Add(elem);
		});
		return result;
	}

	public static (T[], T[]) SplitFilter<T>(this ICollection<T> collection, System.Func<T, bool> predicate) {
		List<T> a = new List<T>(), b = new List<T>();
		collection.Perform((e) => {
			if (predicate(e)) a.Add(e);
			else b.Add(e);
		});
		return (a.ToArray(), b.ToArray());
	}

	public static void Perform(this ICollection<System.Action> collection) {
		collection.Perform((f) => f());
	}
	public static O[] Perform<I, O>(this ICollection<I> collection, System.Func<I, O> func) {
		O[] result = new O[collection.Count];
		int i = 0;
		foreach (I elem in collection)
			result[i++] = func(elem);
		return result;
	}
	public static void Perform<I>(this IEnumerable<I> collection, System.Func<I, EditorAction> func) {
		foreach (I elem in collection)
			if (func(elem) != EditorAction.Null) return;
	}
	public static void Perform<I>(this IEnumerable<I> collection, System.Func<int, I, EditorAction> func) {
		int i = 0;
		foreach (I elem in collection)
			if (func(i++, elem) != EditorAction.Null) return;
	}

	public static void Perform<I>(this IEnumerable<I> collection, System.Action<int, I> func) {
		int i = 0;
		foreach (I elem in collection)
			func(i++, elem);
	}
	public static void Perform<I>(this IEnumerable<I> collection, System.Action<I> func) {
		foreach (I elem in collection)
			func(elem);
	}

	public static void PerformWhile<T>(this IEnumerable<T> collection, System.Func<T, bool> func) {
		foreach (T elem in collection)
			if (!func(elem)) return;
	}
	public static void PerformIf<T>(this IEnumerable<T> collection, System.Func<T, bool> predicate, System.Action<T> func) {
		foreach (T elem in collection) {
			if (!predicate(elem)) return;
			func(elem);
		}
	}

	public static bool SafeAdd<T>(this ICollection<T> collection, T elem) {
		if (elem == null) return false;
		collection.Add(elem);
		return true;
	}
	public static bool SafeAdd<K, V>(this Dictionary<K, V> dictionary, K key, V value) {
		if (dictionary.ContainsKey(key)) return false;
		dictionary.Add(key, value); return true;
	}

	public static void Set<K, V>(this Dictionary<K, V> dictionary, K key, V value) {
		if (!dictionary.ContainsKey(key)) dictionary.Add(key, value);
		else dictionary[key] = value;
	}

	public static void AddAll<T>(this ICollection<T> collection, ICollection<T> collectionToAdd) {
		foreach (T elem in collectionToAdd)
			collection.Add(elem);
	}

	public static T[] Append<T>(this ICollection<T> collection, ICollection<T> other) {
		List<T> result = new List<T>(collection);
		result.AddRange(other);
		return result.ToArray();
	}
	public static T[] Append<T>(this ICollection<T> collection, T other) => collection.Append(new T[] { other });
	public static T[] Prepend<T>(this ICollection<T> collection, ICollection<T> other) => other.Append(collection);
	public static T[] Prepend<T>(this ICollection<T> collection, T other) => collection.Prepend(new T[] { other });

	public static string[] ToStringArray<T>(this ICollection<T> array, string filter) {
		string[] strArray = new string[array.Count];
		int i = 0;
		foreach (T elem in array) {
			string str = elem == null ? "<null>" : elem.ToString();
			strArray[i++] = filter.Length == 0 ? str : str.Replace(filter, "");
		}
		return strArray;
	}
	public static string[] ToStringArray<T>(this ICollection<T> array) {
		return ToStringArray(array, "");
	}
	public static string ArrayToString<T>(this ICollection<T> array) {
		string str = "[";
		foreach (T elem in array)
			str += (str.Length == 1 ? "" : ", ") + elem.ToString();
		return str + "]";
	}

	public static List<List<T>> To2DList<T>(this T[] array, int[] indices) {
		List<List<T>> list = new List<List<T>>();
		List<T> buffer = new List<T>();

		int n = 0;
		int j = 0;
		for (int i = 0; i < array.Length; i++) {
			if (i - n == indices[j] + 1) {
				list.Add(buffer);
				buffer.Clear();
				n += indices[j++];
			}
		}
		if (buffer.Count != 0) list.Add(buffer);

		return list;
	}

	public static (T[], int[]) Flatten<T>(this List<List<T>> list) {
		List<T> temp = new List<T>();
		int[] indices = new int[list.Count];

		for (int i = 0; i < list.Count; i++) {
			foreach (T elem in list[i])
				temp.Add(elem);
			indices[i] = list[i].Count;
		}

		return (temp.ToArray(), indices);
	}

	public static List<T> Fill<T>(this List<T> list, int count) {
		int startCount = list.Count;
		while (list.Count < count) {
			list.Add(default);
			if (list.Count == startCount) throw new System.Exception("Infinite loop detected");
		}
		return list;
	}



	public static List<T> Shuffle<T>(this List<T> list) {
		List<T> result = new List<T>();

		while (list.Count > 0) {
			result.Add(TakeRandom(list));
		}
		list = result;
		return list;
	}

	public static T GetRandom<T>(this List<T> list) {
		return list[Random.Range(0, list.Count)];
	}
	public static T GetRandom<T>(this T[] array) {
		return array[Random.Range(0, array.Length)];
	}

	public static T TakeRandom<T>(this List<T> list) {
		T elem = GetRandom(list);
		list.Remove(elem);

		return elem;
	}

	public static T Take<T>(this List<T> list, int i) {
		if (i < 0 || i >= list.Count) return default;
		int n = list.Count;
		T obj = list[i];
		list.RemoveAt(i);
		if (list.Count == n) throw new System.Exception("Take method didn't remove element from list");
		return obj;
	}

	public static object[] ToObjectArray<T>(this T[] typedArray) {
		object[] array = new object[typedArray.Length];
		for (int i = 0; i < typedArray.Length; i++) {
			array[i] = typedArray[i];
		}
		return array;
	}
	public static T[] TypeArray<T>(this object[] array) {
		if (array == null) { Debug.LogError("Can't type null-array"); return null; }
		T[] typedArray = new T[array.Length];
		for (int i = 0; i < array.Length; i++) {
			if (array[i] is T) typedArray[i] = (T)array[i];
			else { Debug.LogError("Couldn't type array"); return null; }
		}
		return typedArray;
	}
	public static T[] ExtractType<T>(this object[] array) {
		List<T> typedList = new List<T>();
		foreach (object elem in array)
			if (elem is T) typedList.Add((T)elem);
		return typedList.ToArray();
	}

	public static T[] Add<T>(this T[] array, params T[] elems) {
		T[] temp = (T[])array.Clone();
		int n = temp.Length;
		array = new T[n + elems.Length];

		for (int i = 0; i < n; i++)
			array[i] = temp[i];
		for (int i = 0; i < elems.Length; i++)
			array[i + n] = elems[i];
		return array;
	}
	public static T[] RemoveAt<T>(this T[] array, int index) {
		T[] temp = (T[])array.Clone();
		int n = temp.Length;

		array = new T[n - 1];

		int j = 0;
		for (int i = 0; i < n; i++) {
			if (i != index) {
				array[j++] = temp[i];
			}
		}
		return array;
	}
	public static T[] Move<T>(this T[] list, int index, int dir) {
		if (index + dir >= 0 && index + dir < list.Length) {
			T temp = list[index];
			list[index] = list[index + dir];
			list[index + dir] = temp;
		}

		return list;
	}
	public static IList<T> Move<T>(this IList<T> list, int index, int dir) {
		if (index + dir < 0 || index + dir >= list.Count) return list;

		T temp = list[index];
		list[index] = list[index + dir];
		list[index + dir] = temp;

		return list;
	}

	public static void RemoveAll<T>(this HashSet<T> set, ICollection<T> collection) {
		foreach (T elem in collection)
			set.Remove(elem);
	}

	public static List<int> RemoveAllNotIn<T1>(this List<T1> list, List<T1> filter) {
		List<int> indices = new List<int>();
		List<T1> temp = new List<T1>(list);
		for (int i = 0; i < temp.Count; i++) {
			if (!filter.Contains(temp[i])) {
				list.RemoveAt(i);
				indices.Add(i);
			}
		}
		return indices;
	}

	public static List<T> SubList<T>(this List<T> list, int index, int length) {
		List<T> result = new List<T>();
		for (int i = index; i < Mathf.Min(length, list.Count); i++)
			result.Add(list[i]);
		return result;
	}

	public static T[] SubArray<T>(this T[] data, int index, int length) {
		T[] result = new T[length];
		System.Array.Copy(data, index, result, 0, Mathf.Min(data.Length, length));
		return result;
	}
	public static T[] SubArray<T>(this T[] data, int index) => SubArray(data, index, data.Length);
	#endregion

	#region Maybe Management
	public static void Do<T>(this T? obj, System.Action<T> valueAction, System.Action nullAction) where T : struct {
		System.Action<T> tempAction = (_) => nullAction();
		(obj.HasValue ? valueAction : tempAction).Invoke(obj.GetValueOrDefault());
	}
	#endregion

	#region Tuple Management
	public static (B, A) Flip<A, B>(this (A, B) tuple) => (tuple.Item2, tuple.Item1);

	public static (A, B)[] Pack<A, B>(this (A[], B[]) input) {
		if (input.Item1.Length != input.Item2.Length) throw new System.ArgumentException("Both input arrays must be of the same length");

		List<(A,B)> result = new List<(A,B)>();
		for (int i = 0; i < input.Item1.Length; i++)
			result.Add((input.Item1[i], input.Item2[i]));
		return result.ToArray();
	}

	public static (A[], B[]) Unpack<A, B>(this (A, B)[] tuples) {
		List<A> a = new List<A>();
		List<B> b = new List<B>();
		foreach (var (one, two) in tuples) {
			a.Add(one);
			b.Add(two);
		}
		return (a.ToArray(), b.ToArray());
	}
	#endregion

	#region Collections Info
	public static bool AllIs<T>(this ICollection<T> collection, T val) {
		foreach (T elem in collection)
			if (!elem.Equals(val)) return false;
		return true;
	}

	public static T Max<T>(this ICollection<T> collection) where T : System.IComparable<T> => collection.BestMatch((result) => result > 0);
	public static T Min<T>(this ICollection<T> collection) where T : System.IComparable<T> => collection.BestMatch((result) => result < 0);

	public static bool ContainsAny<T>(this ICollection<T> collection, params T[] elems) {
		foreach (T elem in elems)
			if (collection.Contains(elem)) return true;
		return false;
	}
	public static bool ContainsAll<T>(this ICollection<T> collection, params T[] elems) {
		foreach (T elem in elems)
			if (!collection.Contains(elem)) return false;
		return true;
	}
	public static bool ContainsOnly<T>(this ICollection<T> collection, params T[] elems) {
		foreach (T o in collection)
			if (!elems.Contains(o)) return false;
		return true;
	}
	public static bool Contains<T>(this T[] array, T elem) {
		if (array == null) return false;
		foreach (T e in array)
			if (e.Equals(elem)) return true;
		return false;
	}

	public static bool Contains(this LayerMask mask, int layer) {
		return mask == (mask | (1 << layer));
	}
	public static bool Contains(this LayerMask mask, GameObject obj) {
		return obj != null && Contains(mask, obj.layer);
	}
	public static bool Contains<T>(this LayerMask mask, T component) where T : Component {
		return Contains(mask, component.gameObject);
	}

	public static bool ContainsKeys<K, V>(this Dictionary<K, V> dictionary, params K[] keys) {
		foreach (K key in keys)
			if (!dictionary.ContainsKey(key)) return false;
		return true;
	}
	public static bool ContainsValues<K, V>(this Dictionary<K, V> dictionary, params V[] values) {
		foreach (V value in values)
			if (!dictionary.ContainsValue(value)) return false;
		return true;
	}

	public static List<T> Intersection<T>(this List<T> a, List<T> b) {
		List<T> c = new List<T>();
		foreach (T elem in a)
			if (b.Contains(elem))
				c.Add(elem);
		return c;
	}
	public static List<T> IntersectionComplement<T>(this List<T> a, List<T> b) {
		List<T> c = new List<T>();
		foreach (T elem in a)
			if (!b.Contains(elem))
				c.Add(elem);
		return c;
	}

	public static List<T> ToList<T>(this T[] array) {
		List<T> list = new List<T>();

		foreach (T elem in array)
			list.Add(elem);

		return list;
	}


	public static List<int> GetIndices<T>(this List<T> source, List<T> target) {
		List<int> indices = new List<int>();
		foreach (T obj in source)
			indices.Add(target.IndexOf(obj));
		return indices;
	}

	public static int IndexOf<T>(this T[] array, T value) {
		if (value != null) {
			for (int i = 0; i < array.Length; i++) {
				if (value.Equals(array[i])) { return i; }
			}
		}
		return -1;
	}
	public static bool IsAnyElementTrue(this bool[] list) {
		for (int i = 0; i < list.Length; i++) { if (list[i]) { return true; } }
		return false;
	}
	public static bool IsAnyElementFalse(this bool[] list) {
		for (int i = 0; i < list.Length; i++) { if (!list[i]) { return true; } }
		return false;
	}

	public static int DictionaryIndexOf<K, V>(this Dictionary<K, V> dictionary, K key) {
		int i = -1;
		foreach (var item in dictionary) {
			i++;
			if (item.Key.Equals(key)) { return i; }
		}
		return i;
	}
	#endregion

	#region Text Formatting
	public static string Generate(this string str, int length, System.Func<char> func) {
		str = "";
		for (int i = 0; i < length; i++)
			str += func();
		return str;
	}

	public static string Filter(this string str, System.Func<char, bool> filter) {
		string result = "";
		foreach (char c in str.ToCharArray())
			if (filter(c)) result += c.ToString();
		return result;
	}

	public static string Capitalize(this string str) {
		return str[0].ToString().ToUpper() + str.Substring(1);
	}

	public static string Genitive(this string str, int i) {
		return i + " " + str + (i == 1 ? "" : "s");
	}

	public static string SubStringAt(this string str, char c0, char c1) {
		int i0 = str.IndexOf(c0) + 1;
		int i1 = str.IndexOf(c1);
		int n = i1 - i0;

		if (n <= 0) { return ""; }

		return str.Substring(i0, n);
	}
	public static string SubstringStartAt(this string str, char c0) {
		return str.Substring(str.IndexOf(c0) + 1);
	}
	public static string SubstringEndAt(this string str, char c1) {
		return str.Substring(0, str.IndexOf(c1) + 1);
	}
	public static string Slice(this string str, char c, int startI, int endI) {
		string[] split = str.Split(c);
		if (endI == 0) endI = split.Length;
		else endI = endI.Mod(split.Length);
		startI = startI.Mod(split.Length);
		
		string result = "";
		for (int i = startI; i < endI; i++)
			result += (result.Length == 0 ? "" : c.ToString()) + split[i];
		return result;
	}
	public static string Slice(this string str, char c, int endI) {
		return str.Slice(c, 0, endI);
	}

	public static string BaseName(this Object obj) {
		if (obj == null) return "";
		return obj.name.Replace("(Clone)", "");
	}

	public static int Mod(this int a, int b) {
		while (a < 0) a += b;
		return a % b;
	}

	public static int ToCharInt(this string str) {
		string num = "";

		foreach (char c in str.ToCharArray()) {
			num += ((int)c).ToString();
		}

		return int.Parse(num);
	}
	#endregion

	#region Color & Texture Formatting
	public static Color32 ToColor(this Color32 c) {
		return new Color(c.r / 255, c.g / 255, c.b / 255);
	}

	public static Color ToColor(ToolBox.Data.Resources.ColorEnum val) {
		switch (val) {
			case ToolBox.Data.Resources.ColorEnum.Black:
				return Color.black;
			case ToolBox.Data.Resources.ColorEnum.Blue:
				return Color.black;
			case ToolBox.Data.Resources.ColorEnum.Clear:
				return Color.clear;
			case ToolBox.Data.Resources.ColorEnum.Cyan:
				return Color.cyan;
			case ToolBox.Data.Resources.ColorEnum.Gray:
				return Color.gray;
			case ToolBox.Data.Resources.ColorEnum.Green:
				return Color.green;
			case ToolBox.Data.Resources.ColorEnum.Grey:
				return Color.grey;
			case ToolBox.Data.Resources.ColorEnum.Magenta:
				return Color.magenta;
			case ToolBox.Data.Resources.ColorEnum.Red:
				return Color.red;
			case ToolBox.Data.Resources.ColorEnum.White:
				return Color.white;
			case ToolBox.Data.Resources.ColorEnum.Yellow:
				return Color.yellow;
			default:
				return ToolBox.Utility.Aesthetic.RandomColor();
		}
	}

	public static Vector3 ToVector3(this Color c) => new Vector3(c.r, c.g, c.b);

	public static float Difference(this Color a, Color b) => (a.ToVector3() - b.ToVector3()).Abs().magnitude;

	public static Texture2D Tint(this Texture2D texture, Color color, float amount) {
		for (int y = 0; y < texture.height; y++)
			for (int x = 0; x < texture.width; x++)
				texture.SetPixel(x, y, texture.GetPixel(x, y).Blend(color, amount));
		texture.Apply();
		return texture;
	}
	public static Color Blend(this Color a, Color b, float ratio) {
		ratio = Mathf.Clamp01(ratio);
		return a * ratio + b * (1 - ratio);
	}
	public static Color Fade(this Color c, float factor) {
		return new Color(c.r, c.g, c.b, c.a * factor);
	}
	public static Color Inverse(this Color a) {
		return new Color(1 - a.r, 1 - a.g, 1 - a.b);
	}
	public static Color InverseAlpha(this Color c) {
		return new Color(c.r, c.g, c.b, 1 - c.a);
	}
	#endregion

	#region Image Processing
	/// <summary>
	/// Adds random noise to each color channel
	/// </summary>
	/// <param name="color">Color to change</param>
	/// <param name="minStrength">Minimum range</param>
	/// <param name="maxStrength">Maximum range</param>
	/// <returns>New color</returns>
	public static Color AddNoise(this Color color, float minStrength, float maxStrength) {
		return new Color(color.r + Random.Range(minStrength, maxStrength),
							color.g + Random.Range(minStrength, maxStrength),
							color.b + Random.Range(minStrength, maxStrength));
	}
	public static Color AddNoise(this Color color, float strength) {
		return AddNoise(color, -strength, strength);
	}
	#endregion
}
