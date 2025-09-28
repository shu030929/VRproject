using UnityEngine;

public class GoalFireworks : MonoBehaviour
{
    [Header("Trigger")]
    public string requiredTag = "Ball";   // Ball 태그만 반응
    public bool fireOnce = true;          // 한 번만 터뜨릴지

    [Header("Effects")]
    public ParticleSystem fireworksPrefab; // 폭죽 프리팹 (필수)
    public AudioClip sfx;                  // (선택) 효과음
    public float yOffset = 0.5f;           // 접점에서 위로 살짝 올려 배치

    bool fired;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(requiredTag)) return;
        if (fireOnce && fired) return;
        if (fireworksPrefab == null) { Debug.LogWarning("Fireworks Prefab not assigned."); return; }

        fired = true;

        // 트리거는 접점이 없으므로 ClosestPoint로 위치 추정
        Vector3 p = GetComponent<Collider>().ClosestPoint(other.transform.position);
        if (p == other.transform.position)      // 드물게 동일하면 반대로 한 번 더
            p = other.ClosestPoint(transform.position);
        p += Vector3.up * yOffset;

        // 폭죽 생성 & 재생
        var fx = Instantiate(fireworksPrefab, p, Quaternion.identity);
        fx.Play();

        // (선택) 사운드
        if (sfx) AudioSource.PlayClipAtPoint(sfx, p);

        // 파티클 수명 끝나면 정리
        var main = fx.main;
        float life = main.duration + main.startLifetime.constantMax + 0.5f;
        Destroy(fx.gameObject, life);
    }
}
