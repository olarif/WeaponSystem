using System;
using UnityEngine;
using TMPro;

public class ConsoleManager : MonoBehaviour
{
    public static ConsoleManager Instance { get; private set; }
    
    [SerializeField] private GameObject[] subMenus;
    
    private bool _isConsoleOpen = false;
    private PlayerController _playerController;

    [SerializeField] private GameObject consoleUI;
    [SerializeField] private GameObject mainMenu;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        _playerController = FindFirstObjectByType<PlayerController>();
        
        _isConsoleOpen = false;
        CloseConsole();
    }

    public void ToggleConsole()
    {
        _isConsoleOpen = !_isConsoleOpen;
        if (_isConsoleOpen)
        {
            OpenConsole();
        }
        else
        {
            CloseConsole();
        }
    }

    private void OpenConsole()
    {
        consoleUI.SetActive(true);
        ReturnToMenu();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _isConsoleOpen = true;
        
        if (_playerController != null)
        {
            _playerController.TakeAwayControl();
        }
    }

    private void CloseConsole()
    {
        consoleUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _isConsoleOpen = false;
        
        if (_playerController != null)
        {
            _playerController.ReturnControl();
        }
    }

    public void ReturnToMenu()
    {
        mainMenu.SetActive(true);
        CloseAllSubMenus();
    }
    
    private void CloseAllSubMenus()
    {
        foreach (var menu in subMenus)
        {
            if (menu != null)
            {
                menu.SetActive(false);
            }
        }
    }

    public void OpenSubMenu(int menuIndex)
    {
        if (menuIndex < 0 || menuIndex >= subMenus.Length)
        {
            Debug.LogError("Invalid menu index: " + menuIndex);
            return;
        }

        CloseAllSubMenus();
        mainMenu.SetActive(false);
        
        if (subMenus[menuIndex] != null)
        {
            subMenus[menuIndex].SetActive(true);
        }
    }
}
