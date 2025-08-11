using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Ensures the VRSettingsUI GameObject is ALWAYS active at runtime,
/// and provides a safe way to start coroutines even if something
/// temporarily disables the object.
///
/// Put this on the *VRSettingsUI* GameObject.
[DefaultExecutionOrder(-1000)] // run very early
public class VRSettingsUIController : MonoBehaviour
{
    private static VRSettingsUIController _instance;

    // Runs before any scene loads (belt-and-suspenders)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureExistsEarly()
    {
        // If a disabled VRSettingsUI is already in the scene, we’ll wake it in Awake().
        // If you ever move to a prefab workflow, you can instantiate it here instead.
    }

    private void Awake()
    {
        // Singleton-ish: keep one alive and persistent
        if (_instance != null && _instance != this)
        {
            // If a second copy appears, prefer the already-persisting one.
            if (!ReferenceEquals(this, null)) Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // If someone left it disabled in the scene, turn it on now.
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        // Also make sure our component is enabled
        if (!enabled) enabled = true;

        // Keep re‑enabling after scene changes, just in case
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
        // If a new scene deactivates UI roots, turn ourselves back on.
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        if (!enabled) enabled = true;
    }

    private void OnDisable()
    {
        // If something disables this object at runtime, flip it back on next frame.
        // (Avoids the "Coroutine couldn't be started; GameObject is inactive" error.)
        if (Application.isPlaying)
            StartCoroutine(ReenableNextFrame());
    }

    private IEnumerator ReenableNextFrame()
    {
        // Wait one frame to avoid Unity warnings about changing active state during callbacks.
        yield return null;
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        if (!enabled) enabled = true;
    }

    /// Use this instead of StartCoroutine(...) inside this component.
    public Coroutine StartSafeCoroutine(IEnumerator routine)
    {
        if (!gameObject.activeInHierarchy)
        {
            // Reactivate and start next frame
            return StartCoroutine(StartAfterReenable(routine));
        }
        if (!isActiveAndEnabled)
        {
            enabled = true; // make sure component is enabled
        }
        return StartCoroutine(routine);
    }

    private IEnumerator StartAfterReenable(IEnumerator routine)
    {
        // Turn ourselves back on and wait a frame, then start.
        gameObject.SetActive(true);
        enabled = true;
        yield return null;
        yield return StartCoroutine(routine);
    }
}
