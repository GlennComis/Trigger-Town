using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BeatManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private EnemyController enemy;
    [SerializeField] private BeatNotePool notePool;

    [Header("Lane Points")]
    [SerializeField] private RectTransform spawnPoint;
    [SerializeField] private RectTransform despawnPoint;

    [Header("Timing")]
    [SerializeField] private float travelTime = 5f;
    [SerializeField] private float hitWindow = 0.25f;

    [Header("Testing")]
    [SerializeField] private float spawnInterval = 1f;

    private Queue<BeatNote> activeNotes = new Queue<BeatNote>();

    private void OnEnable()
    {
        player.OnShootRequested += HandlePlayerShootRequest;
    }

    private void OnDisable()
    {
        player.OnShootRequested -= HandlePlayerShootRequest;
    }

    private void Start()
    {
        InvokeRepeating(nameof(SpawnNote), 0f, spawnInterval);
    }

    private void Update()
    {
        HandleLateNotes();
    }

    private void SpawnNote()
    {
        BeatNote note = notePool.Get();

        note.Activate(
            spawnPoint.localPosition,
            travelTime,
            despawnPoint.localPosition.x,
            ReturnNoteToPool
        );

        activeNotes.Enqueue(note);
    }

    private void ReturnNoteToPool(BeatNote note)
    {
        activeNotes = new Queue<BeatNote>(activeNotes.Where(n => n != note));
        note.Deactivate();
    }

    private BeatNote GetCurrentNote()
    {
        foreach (BeatNote n in activeNotes)
            if (!n.isJudged)
                return n;

        return null;
    }

    private void HandlePlayerShootRequest()
    {
        BeatNote current = GetCurrentNote();

        if (current != null && current.IsInTimeWindow(hitWindow))
        {
            current.isJudged = true;

            player.Shoot();
            enemy.TakeDamage(player.weaponDamage);

            Debug.Log("Hit");
        }
        else
        {
            if (current != null)
                current.isJudged = true;

            enemy.Shoot();
            player.TakeDamage(player.weaponDamage);

            Debug.Log("Miss");
        }
    }

    private void HandleLateNotes()
    {
        BeatNote current = GetCurrentNote();
        if (current == null) return;

        if (!current.isJudged &&
            !current.hasTriggeredLate &&
            current.HasPassedCenter() &&
            !current.IsInTimeWindow(hitWindow))
        {
            current.hasTriggeredLate = true;
            current.isJudged = true;

            enemy.Shoot();
            player.TakeDamage(player.weaponDamage);

            Debug.Log("Late");
        }
    }
}
