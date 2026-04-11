using UnityEngine;

public class PickupInteractable : Interactable
{
    public enum RewardType
    {
        Score,
        Health
    }

    public RewardType rewardType = RewardType.Score;
    public int amount = 10;

    public override void Interact(GameObject interactor)
    {
        if (rewardType == RewardType.Score && HUDManager.Instance != null)
            HUDManager.Instance.AddScore(amount);

        if (rewardType == RewardType.Health)
        {
            PlayerHealth playerHealth = interactor.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.Heal(amount);
        }

        gameObject.SetActive(false);
    }
}
