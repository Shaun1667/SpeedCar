using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 도로 모듈 프리팹에 붙이면, 모듈이 생성될 때(Start) 5개 차선 위치 중 일부에
/// 무작위로 트래픽 자동차를 올려놓습니다. 각 차량에는 자동으로
/// TrafficCarAutoSpeed가 붙어서, 생성되는 순간의 플레이어 속도를 한 번
/// 등록하고 그 속도로 계속 앞으로 이동합니다.
///
/// 사용법: 도로 모듈 프리팹(Root)에 이 컴포넌트를 붙이고 Traffic Prefabs에
/// 자동차 프리팹들을 등록하세요. Lane Positions는 PlayerController의 차선
/// 좌표(-2,-1,0,1,2)와 맞춰져 있는 게 기본값입니다.
/// </summary>
public class TrafficOnModule : MonoBehaviour
{
    [Tooltip("스폰할 트래픽 자동차 프리팹들. 여러 개 등록하면 매번 랜덤으로 하나를 고릅니다.")]
    public GameObject[] trafficPrefabs;

    [Tooltip("차선 x좌표들 (도로 폭에 맞게 조절). 기본은 5차선: -2, -1, 0, 1, 2")]
    public float[] lanePositions = { -2f, -1f, 0f, 1f, 2f };

    [Tooltip("모듈 기준, 차량이 놓일 z 위치의 최소값 (이 범위 안에서 차선마다 무작위로 정해집니다)")]
    public float minZOffset = -5f;

    [Tooltip("모듈 기준, 차량이 놓일 z 위치의 최대값")]
    public float maxZOffset = 5f;

    [Tooltip("모듈 로컬 기준, 차량이 놓일 y 위치 (도로 표면 높이에 맞게 조절)")]
    public float spawnYOffset = 0f;

    [Range(0f, 1f)]
    [Tooltip("차선 하나당 차량이 스폰될 확률")]
    public float spawnChancePerLane = 0.4f;

    [Tooltip("이 모듈 하나에 동시에 스폰될 수 있는 최대 차량 수 (모든 차선이 막히지 않도록 제한)")]
    public int maxCarsPerModule = 3;

    [Tooltip("스폰된 차량에 이 태그를 자동으로 붙일지 여부 (충돌/장애물 판정용)")]
    public bool tagAsTraffic = true;

    [Tooltip("tagAsTraffic이 켜져있을 때 사용할 태그 이름")]
    public string trafficTag = "Traffic";

    [Tooltip("스폰된 트래픽 차량들을 자식으로 넣어둘 부모 오브젝트. 비워두면 씬에서 " +
             "\"TrafficParent\"라는 이름의 오브젝트를 자동으로 찾습니다.")]
    public Transform trafficParent;

    [Tooltip("Traffic Parent를 자동으로 찾을 때 사용할 이름")]
    public string trafficParentName = "TrafficParent";

    [Tooltip("켜두면 이 모듈에는 트래픽을 스폰하지 않습니다. 게임 시작 직후 첫 모듈처럼 " +
             "안전 구간이 필요할 때 RoadModuleManager가 이 값을 true로 설정해줍니다.")]
    public bool skipTraffic = false;

    // 이 모듈이 스폰한 트래픽 차량들. 모듈이 삭제될 때 같이 정리하기 위해 기억해둡니다.
    readonly List<GameObject> spawnedCars = new List<GameObject>();

    void Awake()
    {
        if (trafficParent == null)
        {
            var found = GameObject.Find(trafficParentName);
            if (found != null) trafficParent = found.transform;
        }
    }

    void Start()
    {
        if (skipTraffic) return;
        if (trafficPrefabs == null || trafficPrefabs.Length == 0 || lanePositions == null || lanePositions.Length == 0)
            return;

        int[] laneOrder = ShuffledIndices(lanePositions.Length);
        int spawned = 0;

        foreach (int laneIndex in laneOrder)
        {
            if (spawned >= maxCarsPerModule) break;
            if (Random.value > spawnChancePerLane) continue;

            SpawnOnLane(lanePositions[laneIndex]);
            spawned++;
        }
    }

    void SpawnOnLane(float laneX)
    {
        GameObject prefab = trafficPrefabs[Random.Range(0, trafficPrefabs.Length)];

        // laneX는 PlayerController/ControllerCollider와 동일한 "절대 월드 x좌표"
        // 체계(-2,-1,0,1,2)이므로, 모듈의 로컬 좌표로 변환하지 않고 그대로 사용합니다.
        // (TransformPoint를 쓰면 모듈 오브젝트의 위치/회전/스케일에 따라 차선 x가
        // 밀릴 수 있어서, x는 절대값 그대로 두고 z(앞뒤 위치)만 모듈 기준으로 계산합니다)
        // z는 차선마다 minZOffset~maxZOffset 사이에서 무작위로 골라, 한 줄로
        // 나란히 서있지 않고 제각각 다른 간격으로 늘어서도록 합니다.
        float randomZ = Random.Range(minZOffset, maxZOffset);
        Vector3 worldPos = new Vector3(
            laneX,
            transform.position.y + spawnYOffset,
            transform.position.z + randomZ);

        // 부모를 같이 넘기며 Instantiate하면 위치가 부모 기준 로컬 좌표로 해석되는
        // 경우가 있어 혼동이 생기기 쉽습니다. 그래서 일단 부모 없이 생성해 월드
        // 좌표를 명시적으로(transform.position) 지정한 뒤, worldPositionStays=true로
        // 부모를 붙여서 위치가 절대 바뀌지 않도록 합니다.
        GameObject car = Instantiate(prefab);
        car.transform.SetPositionAndRotation(worldPos, transform.rotation);
        if (trafficParent != null)
            car.transform.SetParent(trafficParent, true);

        if (car.GetComponent<TrafficCarAutoSpeed>() == null)
            car.AddComponent<TrafficCarAutoSpeed>();

        if (car.GetComponent<LaneBumper>() == null)
        {
            var bumper = car.AddComponent<LaneBumper>();
            bumper.lanePositions = lanePositions;
        }

        if (tagAsTraffic)
            SetTagRecursive(car.transform, trafficTag);

        spawnedCars.Add(car);
    }

    // 이 모듈이 삭제될 때(RoadModuleManager가 오래된 모듈을 정리할 때 등),
    // 이 모듈 위에 스폰했던 트래픽 차량들도 함께 삭제합니다.
    // (차량들은 TrafficParent 밑에 있어서 모듈을 지워도 자동으로 같이 지워지지 않기 때문)
    void OnDestroy()
    {
        foreach (var car in spawnedCars)
        {
            if (car != null) Destroy(car);
        }
    }

    static void SetTagRecursive(Transform t, string tag)
    {
        t.gameObject.tag = tag;
        foreach (Transform child in t)
            SetTagRecursive(child, tag);
    }

    static int[] ShuffledIndices(int count)
    {
        int[] arr = new int[count];
        for (int i = 0; i < count; i++) arr[i] = i;
        for (int i = count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
        return arr;
    }
}
