using UnityEngine; 
using System.Collections.Generic;

public class BeatNotePool : MonoBehaviour
{
    [SerializeField] private BeatNote notePrefab;
    [SerializeField] private int poolSize = 10;

    private List<BeatNote> pool = new List<BeatNote>();

    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            BeatNote n = Instantiate(notePrefab, transform);
            n.gameObject.SetActive(false);
            pool.Add(n);
        }
    }

    public BeatNote Get()
    {
        foreach (BeatNote n in pool)
        {
            if (!n.gameObject.activeSelf)
                return n;
        }
        
        BeatNote extra = Instantiate(notePrefab, transform);
        extra.gameObject.SetActive(false);
        pool.Add(extra);
        return extra;
    }
}