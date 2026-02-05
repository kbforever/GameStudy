using Managers;
using MVC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UI
{
    [RequireComponent(typeof(playerRollUIView))]
    public class playerRollController : BaseController
    {
        
        public void OnRollClick()
        {
            Debug.LogWarning("¿ªÊ¼Ò¡É¸");
            UIManager.Instance.HideGameUI(ViewType.playerRollUI.ToString());
            var diceResult = GameCoreManager.Instance.RollDiceAndMove(MoveEndCallBack);
        }
        
        void MoveEndCallBack()
        {
            UIManager.Instance.ShowGameUI(ViewType.playerBuyUI.ToString());

        }

    }
}

