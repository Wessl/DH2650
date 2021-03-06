using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitAnimatorPool : MonoBehaviour
{
    [SerializeField]
    private GameObject hitAnimatorPrefab;

    private Queue<GameObject> availableObjects = new Queue<GameObject>();

    public static HitAnimatorPool Instance { get; private set; }
    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        GrowPool();
    }

    private void GrowPool()
    {
        for (int i = 0; i < 10; i++)
        {
            var instanceToAdd = Instantiate(hitAnimatorPrefab);
            instanceToAdd.transform.SetParent(transform);
            AddToPool(instanceToAdd);
        }
    }

    public void AddToPool(GameObject instance)
    {
        instance.SetActive(false);
        availableObjects.Enqueue(instance);
    }

    public GameObject GetFromPool(Vector3 position)
    {
        if (availableObjects.Count == 0)
            GrowPool();

        var instance = availableObjects.Dequeue();
        instance.SetActive(true);
        instance.transform.position = position;
        return instance;
    }
    // Update is called once per frame
    void Update()
    {

    }
}
