using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "newWeapon", menuName = "Weapon/WeaponSO", order = 0)]
public class WeaponDataSO : ScriptableObject
{
    public string weaponName;
    public string weaponDescription;
    public GameObject rightHandModel;
    public GameObject leftHandModel;
    public GameObject worldPickupPrefab;
    
    public enum Hand { Right, Left, Both }
    public Hand defaultHand = Hand.Right;
    
    [Tooltip("Local position offset after parenting under the hand")]
    public Vector3 modelPositionOffset = Vector3.zero;
    [Tooltip("Local euler-rotation after parenting under the hand")]
    public Vector3 modelRotationOffset = Vector3.zero;
    
    public List<InputBinding> bindings = new List<InputBinding>();
    
    [System.Serializable]
    public class InputBinding
    {
        public WeaponInputEvent eventType;
        
        public Hand fireHand = Hand.Right;
        
        [Tooltip("Time in seconds to hold the button before executing the action")]
        public float holdTime;

        [Tooltip("Time in seconds between each action execution when holding the button")]
        public float fireRate = 0.1f;
        
        public InputActionReference inputAction;
        
        [SerializeReference] 
        public List<WeaponActionData> actions = new List<WeaponActionData>();
    }
}