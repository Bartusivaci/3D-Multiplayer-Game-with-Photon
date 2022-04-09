using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController instance;
    void Awake()
    {
        instance = this;
    }


    public TMP_Text overHeatedMessage;
    public Slider weaponHeatSlider;

    public GameObject deathScreen;
    public TMP_Text deathText;

    public TMP_Text healthLabel;




    


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
