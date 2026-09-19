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

    [Tooltip("모듈 로컬 기준, 차량이 놓일 z 위치 (도로 표면 높이에 맞게 조절)")]
    public float spawnZOffset = 0f;


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
        Vector3 worldPos = new Vector3(
            laneX,
            transform.position.y + spawnYOffset,
            transform.position.z + spawnZOffset);

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

        if (tagAsTraffic)
            SetTagRecursive(car.transform, trafficTag);
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
