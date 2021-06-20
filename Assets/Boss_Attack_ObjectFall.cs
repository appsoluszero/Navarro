using UnityEngine;

public class Boss_Attack_ObjectFall : MonoBehaviour, IBossAttack
{
    [SerializeField] private int spawnPointCount = 5;
    [SerializeField] private Transform left, right;
    public struct fallingObject {
        public Vector3 spawnPos;
        public float timeInterval;
        public float timer;
        public bool isFalling;
        public fallingObject(Vector3 pos) {
            spawnPos = pos;
            timeInterval = 0;
            timer = 0f;
            isFalling = false;
        }
    }

    private fallingObject[] _object;
    [SerializeField] private GameObject objectList;

    void Start() {
        if(spawnPointCount < 2) spawnPointCount = 2;
        _object = new fallingObject[spawnPointCount];
        SpawnPointInitialize();
        TimeIntervalSetup();
    }

    public void AttackLogicRunner() {
        for(int i = 0 ; i < spawnPointCount ; ++i) {
            if(_object[i].timer >= _object[i].timeInterval && !_object[i].isFalling) {
                transform.GetChild(1).GetChild(i).rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
                transform.GetChild(1).GetChild(i).gameObject.SetActive(true);
                _object[i].timer = 0f;
                _object[i].isFalling = true;
            }
            if(!_object[i].isFalling) {
                _object[i].timer += Time.deltaTime;
                _object[i].timer = Mathf.Clamp(_object[i].timer, 0f, _object[i].timeInterval);
            }
        }
    }

    void SpawnPointInitialize() {
        float Xdistance = right.position.x - left.position.x;
        float Ydistance = right.position.y - left.position.y;
        float Xfraction = Xdistance / (float)(spawnPointCount-1);
        float Yfraction = Ydistance / (float)(spawnPointCount-1);
        for(int i = 0 ; i < spawnPointCount ; ++i) {
            _object[i] = new fallingObject(new Vector3(left.position.x + (i*Xfraction), left.position.y + (i*Yfraction), 0f));
            GameObject p = Instantiate(objectList, transform.GetChild(1));
            p.transform.position = _object[i].spawnPos;
            p.SetActive(false);
        } 
    }

    public void ReturnObjectToSpawn(GameObject p) {
        p.transform.position = _object[p.transform.GetSiblingIndex()].spawnPos;
        p.SetActive(false);
        _object[p.transform.GetSiblingIndex()].isFalling = false;
    }

    public void TimeIntervalSetup() {
        for(int i = 0 ; i < spawnPointCount ; ++i) {
            _object[i].timeInterval = UnityEngine.Random.Range(0.75f, 2.25f);
        } 
    }
}
