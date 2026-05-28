using UnityEngine;

public enum PlayerPowerUpType
{
    DoubleJump,
    SpeedBoost
}

public class PlayerPowerUps : MonoBehaviour
{
    [Header("Defaults")]
    public float defaultSpeedMultiplier = 1.75f;
    public float pickupMessageDuration = 2f;

    float speedBoostTimer;
    float speedBoostMultiplier = 1f;
    int doubleJumpUsesRemaining;

    public int DoubleJumpUsesRemaining => doubleJumpUsesRemaining;
    public float CurrentSpeedMultiplier => speedBoostTimer > 0f ? speedBoostMultiplier : 1f;

    void Update()
    {
        if (speedBoostTimer <= 0f)
            return;

        speedBoostTimer -= Time.deltaTime;
        if (speedBoostTimer > 0f)
            return;

        speedBoostTimer = 0f;
        speedBoostMultiplier = 1f;
    }

    public void EnablePowerUp(PlayerPowerUpType type, float durationSeconds = 0f, int uses = 0, float magnitude = 0f)
    {
        switch (type)
        {
            case PlayerPowerUpType.DoubleJump:
                EnableDoubleJump(uses);
                break;
            case PlayerPowerUpType.SpeedBoost:
                EnableSpeedBoost(durationSeconds, magnitude);
                break;
        }
    }

    public void EnableDoubleJump(int uses)
    {
        if (uses <= 0)
            return;

        doubleJumpUsesRemaining += uses;
        ShowMessage("Double jump +" + uses);
    }

    public void EnableSpeedBoost(float durationSeconds, float multiplier = 0f)
    {
        if (durationSeconds <= 0f)
            return;

        speedBoostTimer += durationSeconds;
        speedBoostMultiplier = Mathf.Max(speedBoostMultiplier, multiplier > 1f ? multiplier : defaultSpeedMultiplier);
        ShowMessage("Speed boost " + durationSeconds.ToString("0.#") + "s");
    }

    public bool TryConsumeDoubleJump()
    {
        if (doubleJumpUsesRemaining <= 0)
            return false;

        doubleJumpUsesRemaining--;
        return true;
    }

    void ShowMessage(string message)
    {
        HUDManager.Instance?.ShowTimedMessage(message, pickupMessageDuration);
    }
}
