using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnedBlock {
    public Transform obj;
    public Vector2 pos;
    public string group = "default";
    public string id;
    public bool isAssigned;

	public SpawnedBlock(Transform obj, Vector2 pos, string id, string group) {
        this.obj = obj;
        this.pos = pos;
        this.id = id;
        this.group = group;

        isAssigned = false;
    }
    public SpawnedBlock(Transform obj, Vector2 pos, string id)
    {
        this.obj = obj;
        this.pos = pos;
        this.id = id;
        group = "default";

        isAssigned = false;
    }

    public bool SameGroup(SpawnedBlock b) {
        return group == b.group && id == b.id;
    }

    public void AssignGroup(Transform handlerObj)
    {
        if (!isAssigned)
        {
            GameObject[] allActiveObjs = Object.FindObjectsOfType<GameObject>();

            for (int i = 0; i < allActiveObjs.Length; i++)
            {
                if (allActiveObjs[i].Equals(obj))
                {
                    allActiveObjs[i].transform.parent = handlerObj;
                    isAssigned = true;
                }
            }
        }
    }
}
