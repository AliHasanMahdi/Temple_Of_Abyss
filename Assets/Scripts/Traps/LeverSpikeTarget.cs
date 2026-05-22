using System.Collections;
using UnityEngine;


public class LeverSpikeTarget : MonoBehaviour
{
    [Header("Damage")]
    [Tooltip("Damage dealt to each enemy touched.")]
    public float damage = 999f;

    [Tooltip("Kill enemies instantly regardless of health?")]
    public bool instantKill = false;

    [Header("Movement")]
    [Tooltip("How far (in world-Y) the spikes rise.")]
    public float riseHeight = 1.5f;

    [Tooltip("Speed at which spikes rise and fall (units/sec).")]
    public float moveSpeed = 4f;

    // „Ÿ„Ÿ internal „Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ
    private Vector3 _downPos;
    private Vector3 _upPos;
    private bool _isUp = false;
    private bool _moving = false;

    void Start()
    {
        _downPos = transform.position;
        _upPos = new Vector3(transform.position.x,
                               transform.position.y + riseHeight,
                               transform.position.z);
    }

    // „Ÿ„Ÿ Called by AN_Button each time the lever is pulled „Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ
    public void Activate()
    {
        if (_moving) return;
        if (!_isUp)
            StartCoroutine(MoveToPos(_upPos, up: true));
        else
            StartCoroutine(MoveToPos(_downPos, up: false));
    }

    // „Ÿ„Ÿ Movement coroutine „Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ
    IEnumerator MoveToPos(Vector3 target, bool up)
    {
        _moving = true;

        while (transform.position != target)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        _isUp = up;
        _moving = false;
    }

    // „Ÿ„Ÿ Damage enemies on contact (works while rising AND while fully up) „Ÿ„Ÿ„Ÿ„Ÿ
    void OnTriggerEnter(Collider other)
    {

        if (!_isUp && !_moving) return;

        EnemyHealth eh = other.GetComponent<EnemyHealth>()
                      ?? other.GetComponentInParent<EnemyHealth>()
                      ?? other.GetComponentInChildren<EnemyHealth>();

        if (eh != null && !eh.IsDead())
        {
            float dmg = instantKill ? eh.maxHealth * 2f : damage;
            eh.TakeDamage(dmg);
            Debug.Log("[LeverSpikeTarget] Hit " + other.name + " for " + dmg + " dmg.");
        }
    }

    // „Ÿ„Ÿ Scene view helper „Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ
    void OnDrawGizmosSelected()
    {
        Vector3 up = Application.isPlaying ? _upPos
                   : new Vector3(transform.position.x,
                                 transform.position.y + riseHeight,
                                 transform.position.z);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(up, Vector3.one * 0.4f);
        Gizmos.DrawLine(transform.position, up);
    }
}
