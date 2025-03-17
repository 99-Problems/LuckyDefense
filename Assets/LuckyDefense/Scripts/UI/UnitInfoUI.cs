using Data;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UnitInfoUI : MonoBehaviour
{
    public int passiveStringID = 13;
    public int activeStringID = 14;
    public GTMPro nameText;
    public GTMPro atkText;
    public GTMPro atkSpeedtext;
    public GTMPro titleText;
    public GTMPro descText;
    public GTMPro skillTypeText;
    public Toggle passiveToggle;
    public Toggle activeToggle;
    public Toggle active2Toggle;
    private Define.SkillType skillType;
    private UnitSkillInfoScript passive;
    private UnitSkillInfoScript active;
    private UnitSkillInfoScript active2;

    private void Start()
    {
        passiveToggle.OnValueChangedAsObservable().DistinctUntilChanged().Where(_ => _)
            .Subscribe(_1 =>
            {
                if (_1 == false)
                    return;

                skillType = Define.SkillType.Passive;
                UpdateUI();
            });
        activeToggle.OnValueChangedAsObservable().DistinctUntilChanged().Where(_ => _)
            .Subscribe(_1 =>
            {
                if (_1 == false)
                    return;
                skillType = Define.SkillType.Active1;
                UpdateUI();
            });
        active2Toggle.OnValueChangedAsObservable().DistinctUntilChanged().Where(_ => _)
            .Subscribe(_1 =>
            {
                if (_1 == false)
                    return;

                skillType = Define.SkillType.Active2;
                UpdateUI();
            });

        gameObject.SetActive(false);
    }
    public void Init(UnitLogic _unit)
    {
        if(_unit == null)
        {
            gameObject.SetActive(false);
            return;
        }
        nameText.SetStringID(_unit.Info.nameID);
        atkText.SetText(_unit.stat.atk);
        atkSpeedtext.SetText(_unit.stat.atk_speed);
        passiveToggle.gameObject.SetActive(_unit.Info.passiveSkillID > 0);
        activeToggle.gameObject.SetActive(_unit.Info.activeSkillID > 0);
        active2Toggle.gameObject.SetActive(_unit.Info.active2SkillID > 0);

        if (_unit.Info.passiveSkillID > 0)
            passive = Managers.Data.GetUnitSkillInfoScript(_ => _.skillID == _unit.Info.passiveSkillID);
        if (_unit.Info.activeSkillID > 0)
            active = Managers.Data.GetUnitSkillInfoScript(_ => _.skillID == _unit.Info.activeSkillID);
        if (_unit.Info.active2SkillID > 0)
            active2 = Managers.Data.GetUnitSkillInfoScript(_ => _.skillID == _unit.Info.active2SkillID);

        
        UpdateUI();
    }

    public void UpdateUI()
    {
        titleText.gameObject.SetActive(true);
        descText.gameObject.SetActive(true);
        
        switch (skillType)
        {
            case Define.SkillType.Passive:
                if (passive != null)
                {
                    titleText.SetStringID(passive.nameID);
                    descText.SetStringID(passive.descID);
                    skillTypeText.SetText(Managers.String.GetString(passiveStringID));
                    return;
                }
                break;
            case Define.SkillType.NormalAttack:
                break;
            case Define.SkillType.Active1:
                if (active != null)
                {
                    titleText.SetStringID(active.nameID);
                    descText.SetStringID(active.descID);
                    skillTypeText.SetText(Managers.String.GetString(activeStringID));
                    return;
                }
                break;
            case Define.SkillType.Active2:
                if (active2 != null) 
                {
                    titleText.SetStringID(active2.nameID);
                    descText.SetStringID(active2.descID);
                    skillTypeText.SetText(Managers.String.GetString(activeStringID));
                    return;
                }
                break;
            default:
                break;
        }

        titleText.gameObject.SetActive(false);
        descText.gameObject.SetActive(false);
    }
}
