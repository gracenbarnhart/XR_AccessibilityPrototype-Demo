using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


[DefaultExecutionOrder(-1000)] 
public class VRSettingsUIController : MonoBehaviour
{
    private static VRSettingsUIController _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureExistsEarly()
    {
        
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            if (!ReferenceEquals(this, null)) Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (!gameObject.activeSelf) gameObject.SetActive(true);

        if (!enabled) enabled = true;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        if (!enabled) enabled = true;
    }

    private void OnDisable()
    {
        if (Application.isPlaying)
            StartCoroutine(ReenableNextFrame());
    }

    private IEnumerator ReenableNextFrame()
    {
        yield return null;
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        if (!enabled) enabled = true;
    }

    public Coroutine StartSafeCoroutine(IEnumerator routine)
    {
        if (!gameObject.activeInHierarchy)
        {
            return StartCoroutine(StartAfterReenable(routine));
        }
        if (!isActiveAndEnabled)
        {
            enabled = true; 
        }
        return StartCoroutine(routine);
    }

    private IEnumerator StartAfterReenable(IEnumerator routine)
    {
        gameObject.SetActive(true);
        enabled = true;
        yield return null;
        yield return StartCoroutine(routine);
    }
}
