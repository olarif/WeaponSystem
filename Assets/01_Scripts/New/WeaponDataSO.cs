using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "newWeapon", menuName = "Weapon/WeaponSO", order = 0)]
public class WeaponDataSO : ScriptableObject
{
    public string weaponName;
    public List<InputBinding> bindings = new List<InputBinding>();
    
    [System.Serializable]
    public class InputBinding
    {
        public WeaponInputEvent eventType;
        
        [Tooltip("Time in seconds to hold the button before executing the action")]
        public float holdTime;

        [Tooltip("Time in seconds between each action execution when holding the button")]
        public float fireRate = 0.1f;
        
        public InputActionReference inputAction;
        
        [SerializeReference] 
        public List<WeaponActionData> actions = new List<WeaponActionData>();
    }
}