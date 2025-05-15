using System;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private WeaponDataSO weaponData;
    [SerializeField] private Transform weaponHolder;

    private void Awake()
    {
        // Instantiate the weapon prefab
        GameObject weaponPrefab = Instantiate(weaponData.weaponPrefab, weaponHolder.position, weaponHolder.rotation, weaponHolder);
        weaponPrefab.transform.localPosition = Vector3.zero;
    }
}
