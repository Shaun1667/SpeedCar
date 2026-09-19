using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 도로 모듈을 이어 붙여서 끝없는 도로를 만드는 매니저입니다.
/// 게임 시작 시 모듈을 미리 여러 개(기본 2개) 만들어두고, 이후에는 각 모듈 안에 있는
/// "ModuleStart" 태그 트리거를 플레이어가 지날 때마다 ModuleStartTrigger가
/// SpawnNextModule()을 호출해서 새 모듈을 하나씩 앞쪽에 이어 붙입니다.
///
/// 사용법:
/// 1. 빈 GameObject를 만들어 이 스크립트를 붙이고, Module Prefabs에 도로 모듈
///    프리팹(들)을 등록합니다. (여러 개면 매번 랜덤으로 하나를 골라 생성합니다)
/// 2. Start Point를 지정하면 그 위치/방향에서부터 도로가 시작됩니다.
///    비워두면 이 오브젝트 자신의 위치에서 시작합니다.
/// 3. 각 모듈 프리팹에는 RoadModule.cs를 붙이고 End Point를 연결해두는 걸 추천합니다.
///    (연결 안 하면 Fallback Module Length만큼 떨어진 곳에 다음 모듈이 생성됩니다)
/// </summary>
public class RoadModuleManager : MonoBehaviour
{
    public static RoadModuleManager Instance { get; private set; }

    [Tooltip("생성할 도로 모듈 프리팹들. 여러 개 등록하면 매번 랜덤으로 하나를 고릅니다.")]
    public GameObject[] modulePrefabs;

    [Tooltip("첫 모듈이 생성될 위치/방향. 비워두면 이 오브젝트 자신의 위치를 사용합니다.")]
    public Transform startPoint;

    [Tooltip("게임 시작 시 미리 만들어둘 모듈 개수")]
    public int initialModuleCount = 2;

    [Tooltip("모듈 프리팹에 RoadModule(End Point)이 없을 때 사용할 기본 모듈 길이(m)")]
    public float fallbackModuleLength = 20f;

    [Tooltip("게임 시작 시 미리 만드는 모듈 중 맨 앞(첫 번째) 모듈에는 트래픽을 스폰하지 " +
             "않도록 할지 여부. 켜두면 시작하자마자 장애물과 만나지 않는 안전 구간이 생깁니다.")]
    public bool firstModuleIsSafe = true;

    [Tooltip("동시에 유지할 최대 모듈 개수. 새 모듈이 생성되어 이 개수를 넘으면, " +
             "가장 오래된(제일 뒤에 남은) 모듈과 그 위에 스폰된 트래픽 차량들을 자동으로 삭제합니다.")]
    public int maxActiveModules = 4;

    Vector3 nextSpawnPos;
    Quaternion nextSpawnRot;

    readonly Queue<GameObject> activeModules = new Queue<GameObject>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        nextSpawnPos = startPoint != null ? startPoint.position : transform.position;
        nextSpawnRot = startPoint != null ? startPoint.rotation : transform.rotation;

        for (int i = 0; i < initialModuleCount; i++)
        {
            bool isFirst = (i == 0) && firstModuleIsSafe;
            SpawnNextModule(isFirst);
        }
    }

    /// <summary>다음 도로 모듈을 현재 이어 붙일 위치에 생성하고, 이어 붙일 위치를 그 모듈의 끝으로 갱신합니다.</summary>
    public void SpawnNextModule() => SpawnNextModule(false);

    /// <param name="skipTraffic">true면 이 모듈에 TrafficOnModule이 있어도 트래픽을 스폰하지 않습니다.</param>
    public void SpawnNextModule(bool skipTraffic)
    {
        if (modulePrefabs == null || modulePrefabs.Length == 0)
        {
            Debug.LogWarning("[RoadModuleManager] Module Prefabs가 비어있습니다.");
            return;
        }

        GameObject prefab = modulePrefabs[Random.Range(0, modulePrefabs.Length)];
        GameObject instance = Instantiate(prefab, nextSpawnPos, nextSpawnRot);

        // Start()가 실행되기 전(같은 프레임 내)에 미리 설정해두면, TrafficOnModule이
        // 이 모듈에는 트래픽을 스폰하지 않고 건너뜁니다.
        if (skipTraffic)
        {
            var traffic = instance.GetComponent<TrafficOnModule>();
            if (traffic != null) traffic.skipTraffic = true;
        }

        var module = instance.GetComponent<RoadModule>();
        if (module != null && module.endPoint != null)
        {
            nextSpawnPos = module.endPoint.position;
            nextSpawnRot = module.endPoint.rotation;
        }
        else
        {
            // End Point가 없으면 고정 길이만큼 현재 방향(forward)으로 이어 붙임
            nextSpawnPos += nextSpawnRot * Vector3.forward * fallbackModuleLength;
        }
    }
}
