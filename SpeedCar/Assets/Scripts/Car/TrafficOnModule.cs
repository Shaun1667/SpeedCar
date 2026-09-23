using UnityEngine;

/// <summary>
/// 도로 모듈 프리팹에 붙이면, 모듈이 생성될 때(Start) 5개 차선 위치 중 일부에
/// 무작위로 트래픽 자동차를 올려놓습니다. 각 차량에는 자동으로
/// TrafficCarAutoSpeed(생성 시점 플레이어 속도로 전진)와 LaneBumper(플레이어/다른
/// 차량과 부딪히면 옆 차선으로 튕겨나가거나 맵 밖으로 날아감)가 붙습니다.
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

    [Tooltip("모듈 기준, 차량이 놓일 z 위치의 최소값 (이 범위 안에서 차선마다 무작위로 정해집니다). " +
             "RoadModuleManager가 모듈 길이를 알려주는 경우, 실제로는 모듈 경계(+ Edge Margin) " +
             "안쪽으로 한 번 더 제한됩니다.")]
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

    [Tooltip("모듈의 앞/뒤 경계에서 이 거리(m) 안쪽으로는 차를 스폰하지 않습니다. " +
             "Module Length를 알고 있을 때만 적용됩니다.")]
    public float edgeMargin = 0.5f;

    // RoadModuleManager가 이 모듈을 생성한 직후 Start()가 실행되기 전에 SetModuleLength로
    // 알려주는, 이 모듈의 실제 앞뒤 길이(m). 모르면 -1(미설정)로 남아있고, 그럴 땐 기존처럼
    // minZOffset~maxZOffset 범위를 그대로 사용합니다.
    float moduleLength = -1f;

    /// <summary>이 모듈의 실제 길이를 알려줍니다. RoadModuleManager가 모듈을 생성한 직후
    /// (Start()가 실행되기 전) 호출해서, 트래픽이 모듈 경계를 벗어나 옆 모듈 영역까지
    /// 스폰되지 않도록 합니다. (경계를 넘어가면, 이 모듈이 삭제될 때 사실은 옆 모듈에
    /// 속한 것처럼 보이는 차까지 같이 사라져버리는 문제가 생깁니다)</summary>
    public void SetModuleLength(float length)
    {
        moduleLength = length;
    }

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

        // moduleLength를 알고 있다면(RoadModuleManager가 알려준 경우), randomZ가 이 모듈의
        // 실제 앞/뒤 경계를 넘어가지 않도록 한 번 더 제한합니다. 이걸 안 하면 min/maxZOffset
        // 값에 따라 차가 옆 모듈 영역까지 넘어가 스폰될 수 있는데, 그 차는 여전히 "이
        // 모듈이 스폰한 차"로 기억되기 때문에, 나중에 이 모듈이 삭제될 때 옆(특히 앞쪽)
        // 모듈에 속한 것처럼 보이던 차까지 같이 사라져버리는 원인이 됩니다.
        if (moduleLength > 0f)
        {
            float safeMin = Mathf.Min(edgeMargin, moduleLength * 0.5f);
            float safeMax = Mathf.Max(moduleLength - edgeMargin, safeMin);
            randomZ = Mathf.Clamp(randomZ, safeMin, safeMax);
        }

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

        // 주의: 이 차는 더 이상 "이 모듈이 스폰했다"는 이유로 모듈과 함께 삭제되지
        // 않습니다. 모듈이 먼저 삭제되어도 차는 화면에 그대로 남아있고, 대신
        // TrafficCarAutoSpeed가 매 프레임 플레이어와의 z거리를 확인해서 스스로
        // 사라질 시점을 판단합니다. (모듈 삭제 시점과 무관하게 판단하기 위함)
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
