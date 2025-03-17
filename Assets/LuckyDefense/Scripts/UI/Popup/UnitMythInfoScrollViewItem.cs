using Data;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;


public class UnitMythInfoScrollViewItem : BaseScrollViewItem<UnitInfoScript>
{
    public GTMPro unitNameText;
    public GTMPro textOwn;
    public Image imgFrame;
    public List<Sprite> frameSprites = new List<Sprite>();
    public GameObject ownObj;

    public bool isOwnUnit;

    private UnitInfoScript info;
    private InGamePlayerInfo player;

    private void Start()
    {
        
    }

    public override void Init(UnitInfoScript _info, int _index)
    {
        if (_info == null)
            return;

        info =_info;
        if(player == null)
        {
            var data = Managers.Scene.CurrentScene as IGameData;
            player = data.PlayInfo.Player;
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        isOwnUnit = player.GetUnitFromID(info.unitID) != null;
        textOwn.SetText(isOwnUnit ? "보유중" : "미보유");
        textOwn.SetColor(isOwnUnit ? Color.yellow : Color.gray);
        ownObj.SetActive(isOwnUnit);
        var spriteIndex = (int)info.unitRarity - 1;
        if(spriteIndex >= 0 && frameSprites.Count > spriteIndex)
            imgFrame.sprite = frameSprites[spriteIndex];

        unitNameText.SetText(Managers.String.GetString(info.nameID));
    }

    public void Select(int unitID)
    {

    }
}
