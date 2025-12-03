using UnityEngine;
using System;

public class PlayerEnergy : MonoBehaviour
{
    public int maxEnergy = 100;
    public int CurrentEnergy { get; private set; }

    public event Action<int, int> OnEnergyChanged;

    private void Awake()
    {
        CurrentEnergy = maxEnergy;
        RaiseEnergyChanged();
    }

    private void RaiseEnergyChanged()
    {
        OnEnergyChanged?.Invoke(CurrentEnergy, maxEnergy);
    }

    public bool TrySpend(int amount)
    {
        if (CurrentEnergy < amount)
            return false;

        CurrentEnergy -= amount;
        RaiseEnergyChanged();
        return true;
    }

    public void RefillFull()
    {
        CurrentEnergy = maxEnergy;
        RaiseEnergyChanged();
    }
}
