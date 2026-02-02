using MVC;
using System.Collections;
using System.Collections.Generic;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class playerBuyUIView : BaseView
{
    Button EndBuy;
    playerBuyUIController controller;
    private void Awake()
    {
        var obj = transform.FindChildRecursive(nameof(EndBuy));
        if (obj != null)
        {
            EndBuy = obj.GetComponent<Button>();
        }

       

    }
    // Start is called before the first frame update
    void Start()
    {
        controller = this.GetComponent<playerBuyUIController>();
        if (controller == null)
        {
            controller = this.AddComponent<playerBuyUIController>();
        }
        controller.BindView(this);
        EndBuy.onClick.AddListener(controller.EndBuyOnClick);
    }
}
