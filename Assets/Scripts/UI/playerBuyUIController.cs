using Managers;
using MVC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(playerBuyUIView))]
    public class playerBuyUIController : BaseController
    {
        
        public void EndBuyOnClick()
        {
            GameCoreManager.Instance.EndPlayerTurn();
            UIManager.Instance.HideGameUI(ViewType.playerBuyUI.ToString());
        }
    }
}

