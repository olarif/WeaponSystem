using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private WeaponDataSO weaponData;
    private List<IInputComponent> _inputs;
    private List<IExecuteComponent> _executes;
    private List<IOnHitComponent> _onHitComponents;
    WeaponContext _context;
    
    [SerializeField] private Transform firePoint;
    private Animator animator;
    private AudioSource audioSource;

    private void Awake()
    {
        _context = new WeaponContext
        {
            FirePoint = firePoint,
            Animator = animator,
            AudioSource = audioSource,
        };
        
        _context.LineRenderer = GetComponent<LineRenderer>();

        //Instantiate the weapon data
        _onHitComponents = weaponData.onHitComponents
            .Select(so => Instantiate(so))
            .Cast<IOnHitComponent>()
            .ToList();
        
        _context.OnHitComponents = _onHitComponents;
        
        _inputs = weaponData.inputComponents.Select(so => Instantiate(so)).Cast<IInputComponent>().ToList();
        _executes = weaponData.executeComponents.Select(so => Instantiate(so)).Cast<IExecuteComponent>().ToList();

        // Initialize all components
        _inputs.ForEach(i => i.Initialize(_context));
        _executes.ForEach(e => e.Initialize(_context));
        _onHitComponents.ForEach(h => h.Initialize(_context));
    }
    
    private void Update()
    {
        if (_inputs.Any(input => input.CanExecute()))
        {
            _executes.ForEach(execute => execute.Execute());
        }
    }
}