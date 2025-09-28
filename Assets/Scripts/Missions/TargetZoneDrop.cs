using UnityEngine;

public class TargetZoneDrop : MonoBehaviour
{
    [Header("Trigger")]
    public string requiredTag = "Ball";   // Ball 태그만 반응

    [Header("Cube To Drop")]
    public Rigidbody cubeRb;              // 떨어질 큐브의 Rigidbody
    public float dropHeight = 10f;        // 현재 위치에서 위로 얼마나 올려 둘지
    public bool dropOnce = true;          // 한 번만 떨어뜨릴지

    [Header("Effects (optional)")]
    public ParticleSystem hitFx;          // 성공 이펙트가 있으면 연결
    public AudioSource hitSfx;            // 성공 사운드가 있으면 연결

    Vector3 basePos;                      // 큐브의 원래 위치
    bool dropped;

    void Awake()
    {
        if (cubeRb != null)
        {
            basePos = cubeRb.transform.position;

            // 시작 상태: 공중 대기 & 고정(보이기 싫으면 SetActive(false)로 바꿔도 됨)
            cubeRb.transform.position = basePos + Vector3.up * dropHeight;
            cubeRb.useGravity = false;
            cubeRb.isKinematic = true;
            // cubeRb.gameObject.SetActive(false); // 원한다면 주석 해제
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(requiredTag)) return;
        if (dropOnce && dropped) return;
        if (cubeRb == null) return;

        dropped = true;

        // 필요 시 보이게
        if (!cubeRb.gameObject.activeSelf) cubeRb.gameObject.SetActive(true);

        // 중력 켜고 낙하 시작
        cubeRb.isKinematic = false;
        cubeRb.useGravity = true;

        // 효과
        if (hitFx) hitFx.Play();
        if (hitSfx) hitSfx.Play();
    }

    // (선택) 다시 올려두고 재시도할 때 호출
    public void ResetDrop()
    {
        if (cubeRb == null) return;
        cubeRb.linearVelocity = Vector3.zero;
        cubeRb.angularVelocity = Vector3.zero;
        cubeRb.transform.position = basePos + Vector3.up * dropHeight;
        cubeRb.useGravity = false;
        cubeRb.isKinematic = true;
        dropped = false;
    }
}
