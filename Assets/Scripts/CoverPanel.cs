using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverPanel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        CoverTimer.start_test += HidePanel;
    }

    private void OnDisable()
    {
        CoverTimer.start_test -= HidePanel;
    }
    void HidePanel()
    {
        this.gameObject.active = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
