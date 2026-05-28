using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PowerUpPickup : MonoBehaviour
{
    public PlayerPowerUpType powerUpType = PlayerPowerUpType.SpeedBoost;
    public float durationSeconds = 5f;
    public int uses = 1;
    public float magnitude = 0f;
    public bool disableInsteadOfDestroy;

    bool collected;

    void Reset()
    {
        Collider pickupCollider = GetComponent<Collider>();
        if (pickupCollider != null)
            pickupCollider.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        TryCollect(other != null ? other.gameObject : null);
    }

    public bool TryCollect(GameObject collector)
    {
        if (collected || collector == null)
            return false;

        PlayerPowerUps powerUps = collector.GetComponentInParent<PlayerPowerUps>();
        if (powerUps == null)
            return false;

        collected = true;
        powerUps.EnablePowerUp(powerUpType, durationSeconds, uses, magnitude);

        if (disableInsteadOfDestroy)
            gameObject.SetActive(false);
        else
            Destroy(gameObject);

        return true;
    }
}
