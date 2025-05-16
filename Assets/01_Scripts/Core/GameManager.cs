using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public Transform playerSpawnPoint;
    private GameObject _playerInstance;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            //find the player prefab in the scene
            _playerInstance = GameObject.FindGameObjectWithTag("Player");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetGame()
    {
        CharacterController controller = _playerInstance.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false; // Temporarily disable to avoid collision issues
        }
        
        _playerInstance.transform.position = playerSpawnPoint.position;
        _playerInstance.transform.rotation = playerSpawnPoint.rotation;
        
        if (controller != null)
        {
            controller.enabled = true;
        }
    }
}