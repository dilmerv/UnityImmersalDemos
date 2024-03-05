using UnityEngine;

public class ARContentComponents : MonoBehaviour
{
    [SerializeField] private GameObject uiComponent;
    [SerializeField] private GameObject arrowComponent;
    [SerializeField] private GameObject buttonComponent;

    public GameObject UI => uiComponent;

    public GameObject Arrow => arrowComponent;

    public GameObject Button => buttonComponent;
}