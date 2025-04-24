using UnityEngine;

public interface IUpgradable
{
    void OnStackMoney();
    void OnUpgrade();
    void IncreasePrice();
    void OnChangePrice();
}