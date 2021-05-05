using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TonguePool : MonoBehaviour
{
    [SerializeField]
    private GameObject tonguePrefab;

    public Queue<GameObject> availableObjects = new Queue<GameObject>();

    public static TonguePool Instance { get; private set; }
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
            var instanceToAdd = Instantiate(tonguePrefab);
            instanceToAdd.transform.SetParent(transform);
            AddToPool(instanceToAdd);
        }
    }

    public void AddToPool(GameObject instance)
    {
        instance.SetActive(false);
        availableObjects.Enqueue(instance);
    }

    public GameObject GetFromPool()
    {
        if (availableObjects.Count == 0)
            GrowPool();

        var instance = availableObjects.Dequeue();
        instance.SetActive(true);
        return instance;
    }
    // Update is called once per frame
    void Update()
    {

    }
}
