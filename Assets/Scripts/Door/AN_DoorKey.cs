using UnityEngine;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class AN_DoorKey : MonoBehaviour
{
    public static bool AnyCollectPromptVisible { get; private set; }
    public static int LastCollectFrame { get; private set; } = -1;

    [Tooltip("True - red key object, false - blue key")]
    public bool isRedKey = true;
    public float collectDistance = 3.5f;
    public float hidePromptDistance = 4.5f;

    private AN_HeroInteractive hero;
    private Collider keyCollider;
    private static readonly List<AN_DoorKey> activeKeys = new List<AN_DoorKey>();
    private static AN_DoorKey promptKey;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetPromptState()
    {
        AnyCollectPromptVisible = false;
        LastCollectFrame = -1;
        activeKeys.Clear();
        promptKey = null;
    }

    public static bool IsAnyCollectableKeyNearby()
    {
        for (int i = activeKeys.Count - 1; i >= 0; i--)
        {
            AN_DoorKey key = activeKeys[i];
            if (key == null)
            {
                activeKeys.RemoveAt(i);
                continue;
            }

            if (key.CanCollect())
            {
                return true;
            }
        }

        return false;
    }

    private void Awake()
    {
        if (!activeKeys.Contains(this))
        {
            activeKeys.Add(this);
        }
    }

    private void Start()
    {
        hero = Object.FindAnyObjectByType<AN_HeroInteractive>();
        keyCollider = GetComponentInChildren<Collider>();

        if (hero == null)
        {
            Debug.LogError($"AN_DoorKey: No AN_HeroInteractive found in the scene! {gameObject.name} won't work.");
        }
    }

    private void Update()
    {
        RefreshCollectPrompt();

        if (promptKey == this && WasCollectPressed())
        {
            Collect();
        }
    }

    private void Collect()
    {
        hero.AddKey(isRedKey);

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddKey(isRedKey);
        }

        LastCollectFrame = Time.frameCount;
        activeKeys.Remove(this);
        if (promptKey == this)
        {
            promptKey = null;
        }

        RefreshCollectPrompt();
        Destroy(gameObject);
    }

    private float GetDistanceToPlayer()
    {
        EnsureHero();
        if (hero == null)
        {
            return float.MaxValue;
        }

        Vector3 keyPosition = keyCollider != null
            ? keyCollider.ClosestPoint(hero.transform.position)
            : transform.position;

        return Vector3.Distance(keyPosition, hero.transform.position);
    }

    private bool CanCollect()
    {
        return GetDistanceToPlayer() <= collectDistance;
    }

    private bool CanKeepPrompt()
    {
        return GetDistanceToPlayer() <= hidePromptDistance;
    }

    private static void RefreshCollectPrompt()
    {
        AN_DoorKey nearestKey = null;
        float nearestDistance = float.MaxValue;

        for (int i = activeKeys.Count - 1; i >= 0; i--)
        {
            AN_DoorKey key = activeKeys[i];
            if (key == null)
            {
                activeKeys.RemoveAt(i);
                continue;
            }

            bool isCurrentPromptKey = key == promptKey;
            bool inRange = isCurrentPromptKey ? key.CanKeepPrompt() : key.CanCollect();
            if (!inRange)
            {
                continue;
            }

            float distance = key.GetDistanceToPlayer();
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestKey = key;
            }
        }

        promptKey = nearestKey;
        AnyCollectPromptVisible = promptKey != null;

        if (HUDManager.Instance == null)
        {
            return;
        }

        if (AnyCollectPromptVisible)
        {
            HUDManager.Instance.ShowInteractPrompt("Press E to collect");
        }
        else
        {
            HUDManager.Instance.HideInteractPrompt();
        }
    }

    private void EnsureHero()
    {
        if (hero == null)
        {
            hero = Object.FindAnyObjectByType<AN_HeroInteractive>();
        }
    }

    private bool WasCollectPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.E);
#else
        return false;
#endif
    }

    private void OnDisable()
    {
        activeKeys.Remove(this);
        if (promptKey == this)
        {
            promptKey = null;
        }

        RefreshCollectPrompt();
    }

    private void OnDestroy()
    {
        activeKeys.Remove(this);
        if (promptKey == this)
        {
            promptKey = null;
        }

        RefreshCollectPrompt();
    }
}
