using UnityEngine;

public class TargetZone : MonoBehaviour
{
    public GameObject cube; // 떨어질 큐브
    public Vector3 startPosition = new Vector3(0, 10, 0); // 시작 높이 위치
    private Rigidbody cubeRb;

    void Start()
    {
        if (cube != null)
        {
            cubeRb = cube.GetComponent<Rigidbody>();
            if (cubeRb == null) cubeRb = cube.AddComponent<Rigidbody>();

            // 처음에는 공중에 고정 (중력X, 비활성)
            cubeRb.useGravity = false;
            cubeRb.isKinematic = true;
            cube.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball")) // Ball에 Tag="Ball" 꼭 붙여주세요
        {
            if (cube != null)
            {
                cube.SetActive(true);
                cube.transform.position = startPosition; // 하늘 위에 위치
                cubeRb.isKinematic = false;
                cubeRb.useGravity = true;
            }
        }
    }
}
