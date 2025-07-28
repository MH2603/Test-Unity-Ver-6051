using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TrackerStatus
{
    NotInit,
    Active,
    Deactivated
}

public class TestTrackerHandler : MonoBehaviour
{
    public Button ToggleBtn;
    public TextMeshProUGUI StatusText;
    public GameObject TrackerContainerPrefab;

    private TrackerStatus _status;
    private GameObject _trackerContainer;
    
    private void Start()
    {
        ToggleBtn.onClick.AddListener(ToggleBtn_OnClick);
    }

    private void Update()
    {
        StatusText.text = _status.ToString();
    }

    private void ToggleBtn_OnClick()
    {
        if (_status == TrackerStatus.NotInit)
        {
            CreateTrackerContainer();
            _status = TrackerStatus.Active;
            Debug.Log("Tracker initialized and activated.");
        }
        else if (_status == TrackerStatus.Active)
        {
            _trackerContainer.SetActive(false);
            _status = TrackerStatus.Deactivated;
            Debug.Log("Tracker deactivated.");
        }
        else if (_status == TrackerStatus.Deactivated)
        {
            _trackerContainer.SetActive(true);
            _status = TrackerStatus.Active;
            Debug.Log("Tracker reactivated.");
        }
    }
    
    private void CreateTrackerContainer()
    {
        
        if (TrackerContainerPrefab == null)
        {
            Debug.LogError("TrackerContainerPrefab is not assigned.");
            return;
        }

        _trackerContainer = Instantiate(TrackerContainerPrefab, transform.position, Quaternion.identity);
        _trackerContainer.transform.SetParent(transform);
        Debug.Log("Tracker container created and parented to " + gameObject.name);

    }
}
