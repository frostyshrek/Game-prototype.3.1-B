using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class npc : MonoBehaviour
{
    private bool hasExplainedQuest = false;
    private bool rewardGiven = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // If boss already beaten
        if (HasBeatenBoss())
        {
            if (!rewardGiven)
            {
                ShowDialogue(
                    "You have defeated the boss! As promised, you are granted a powerful buff."
                );

                GiveRewardToPlayer();
                rewardGiven = true;
            }
            else
            {
                ShowDialogue(
                    "You already received your reward. Use it well."
                );
            }

            return;
        }

        // Boss NOT beaten yet
        if (!hasExplainedQuest)
        {
            ShowDialogue(
                "Beyond this land lies a fearsome boss. Defeat it, and you shall be rewarded with a powerful buff."
            );

            hasExplainedQuest = true;
        }
        else
        {
            ShowDialogue(
                "You are not ready yet. Return after you have defeated the boss."
            );
        }
    }

    bool HasBeatenBoss()
    {
        // TODO:
        // Replace with your boss/quest system
        // Example:
        // return QuestManager.instance.bossDefeated;

        return false; // placeholder
    }

    void GiveRewardToPlayer()
    {
        // TODO:
        // Add buff, item, stat increase, etc.
        // Example:
        // playerStats.damage += 10;

        Debug.Log("Reward given to player (placeholder)");
    }

    void ShowDialogue(string text)
    {
        // TODO:
        // Hook into your dialogue UI system
        // Example:
        // DialogueManager.instance.Show(text);

        Debug.Log("NPC says: " + text);
    }
}