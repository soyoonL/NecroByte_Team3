using UnityEngine;

public class Follow : MonoBehaviour
{
    public static Follow Instance;

    public Vector3 offset;

    [SerializeField] Camera cam;
    [SerializeField] float normalFOV = 60f;
    [SerializeField] float dodgeFOV = 68f;
    [SerializeField] float fovChangeSpeed = 15f;
    [SerializeField] PlayerController player;
    Transform target;

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
            this.enabled = false;
            return;
        }
    }

    void Start()
    {
        target = PlayerController.Instance.transform;
        player = PlayerController.Instance;

        offset = transform.position - target.transform.position;

        if (cam == null)
            cam = GetComponent<Camera>();

        cam.fieldOfView = normalFOV;
    }

    void LateUpdate()
    {
        transform.position = target.transform.position + offset;

        HandleFov();
    }

    void HandleFov()
    {
        if (cam == null) return;
        if (player == null) return;

        float targetFOV = player.isDodgingNow ? dodgeFOV : normalFOV;

        cam.fieldOfView = Mathf.Lerp(
            cam.fieldOfView,
            targetFOV,
            Time.deltaTime * fovChangeSpeed
        );
    }
}
