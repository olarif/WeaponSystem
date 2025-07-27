using System.Collections;
using UnityEngine;
using TMPro;

public class InteractionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _promptText;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _fadeSpeed = 5f;

    private Coroutine _fadeCoroutine;
    private Camera _playerCamera;

    private void Awake()
    {
        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        _canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void ShowPrompt(string text, Camera camera)
    {
        _playerCamera = camera;
        _promptText.text = text;
        
        gameObject.SetActive(true);
        
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }
        
        _fadeCoroutine = StartCoroutine(FadeToAlpha(1f));
    }

    public void HidePrompt()
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }
        
        _fadeCoroutine = StartCoroutine(FadeToAlpha(0f));
    }

    private IEnumerator FadeToAlpha(float targetAlpha)
    {
        while (Mathf.Abs(_canvasGroup.alpha - targetAlpha) > 0.01f)
        {
            _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, targetAlpha, _fadeSpeed * Time.deltaTime);
            yield return null;
        }
        
        _canvasGroup.alpha = targetAlpha;
        
        if (targetAlpha == 0f)
            gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_playerCamera == null && gameObject.activeSelf)
            FaceCamera();
    }
    
    private void FaceCamera()
    {
        if (_playerCamera == null) return;

        Vector3 dir = transform.position - _playerCamera.transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        transform.rotation = lookRotation;
    }
}
