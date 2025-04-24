using System.Collections;
using UnityEngine;

public class CollisionChecker : MonoBehaviour
{
    private IUpgradable upgradeReference;
    private bool IsUpgradeTag(GameObject obj) => obj.CompareTag("Upgrade");

    void OnTriggerEnter(Collider other)
    {
        if(IsUpgradeTag(other.gameObject))
        {
            StartCoroutine(SetupReference(other.gameObject));
        }
        
    }

    IEnumerator SetupReference(GameObject _object)
    {
        yield return new WaitForSeconds(1f);

        if(_object != null)
        {
            if (_object.TryGetComponent(out IUpgradable upgrade))
            {
                upgradeReference = upgrade;
                Debug.Log("Componente de Upgrade encontrado!");
            }
            else
            {
                Debug.Log("Componente de Upgrade não encontrado.");
            }
        }

        yield return null;
    }

    void OnTriggerStay(Collider other)
    {
        if (IsUpgradeTag(other.gameObject))
        {
            upgradeReference?.OnStackMoney();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsUpgradeTag(other.gameObject))
        {
            StopAllCoroutines();
            upgradeReference = null;
        }
    }
}
