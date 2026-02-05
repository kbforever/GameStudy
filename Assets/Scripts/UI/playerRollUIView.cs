using MVC;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class playerRollUIView : BaseView
    {
        Button RollBtn;
        const string rollName = "Roll";
        playerRollController controller;


        private void Awake()
        {
            if (RollBtn == null)
            {
                Transform tran = transform.FindChildRecursive(rollName);
                if (tran != null)
                {
                    RollBtn = tran.GetComponent<Button>();
                }
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            InitializeView();
        }

        public override void InitializeView()
        {
            base.InitializeView();
            controller = GetComponent<playerRollController>();
            if (controller == null)
            {
                controller = transform.AddComponent<playerRollController>();    
            }
            controller.BindView(this);

            if (RollBtn != null)
            {
                RollBtn.onClick.AddListener(controller.OnRollClick);
            }
        }

    }
}

