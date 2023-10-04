using UnityEngine;

public class DeactivateMiniMapForBigMap : MonoBehaviour
{
    [SerializeField] private GameObject _miniMap;
    [SerializeField] private GameObject _bigMap;
    // Start is called before the first frame update
    void Start()
    {
        _miniMap.SetActive(true);
        _bigMap.SetActive(false);
    }

    public void OnMap() 
    {
        Toggle();
    }
    // Update is called once per frame
    public void Toggle()
    {
        _miniMap.SetActive(!_miniMap.activeInHierarchy);
        _bigMap.SetActive(!_bigMap.activeInHierarchy);
    }
}
