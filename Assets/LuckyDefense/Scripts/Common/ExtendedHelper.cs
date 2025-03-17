using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data.Managers
{
    public static class ExtendedHelper
    {
        public static bool UseIngameExitBtn(this Define.ECONTENT_TYPE contentType)
        {
            switch (contentType)
            {
                case Define.ECONTENT_TYPE.MAIN:
                    return true;
                case Define.ECONTENT_TYPE.INGAME:
                default:
                    return false;
            }
        }

        public static void AddStatData(this List<StatData> statDatas, Define.EStatType statType, int statVal)
        {
            if (statType == Define.EStatType.NONE)
                return;
            if (statVal == 0)
                return;
            var stat = statDatas.Find(_ => _.stat == statType);
            if (stat == null)
            {
                stat = new StatData();
                stat.stat = statType;
                stat.val = 0;
                statDatas.Add(stat);
            }

            stat.val += statVal;
            //stat.val = stat.val.GetFixedStat();
        }

        public static Vector3 GetDirecton(this Define.EUNIT_DIRECTION direction)
        {
            switch (direction)
            {
                case Define.EUNIT_DIRECTION.UP:
                    return Vector3.forward;
                case Define.EUNIT_DIRECTION.DOWN:
                    return Vector3.back;
                case Define.EUNIT_DIRECTION.LEFT:
                    return Vector3.left;
                case Define.EUNIT_DIRECTION.RIGHT:
                    return Vector3.right;
                default:
                    return Vector3.zero;
            }
        }

        public static string GetLabelString(this Define.AssetLabel _label)
        {
            string label = "";
            switch (_label) 
            {
                case Define.AssetLabel.Default:
                    label = "default";
                    break;
                case Define.AssetLabel.Popup:
                    label = "Popup";
                    break;
                case Define.AssetLabel.Script:
                    label = "Script";
                    break;
                case Define.AssetLabel.Font:
                    label = "font";
                    break;
                case Define.AssetLabel.UI:
                    label = "ui";
                    break;
                case Define.AssetLabel.Material:
                    label = "Mat";
                    break;
                case Define.AssetLabel.Particle:
                    label = "Particle";
                    break;
                default:
                    break;
            }

            return label;
        }

        public static bool IsLoadLabel(this Define.AssetLabel label)
        {
            switch (label)
            {
                case Define.AssetLabel.Default:
                case Define.AssetLabel.Popup:
                case Define.AssetLabel.Script:
                    return true;
                
                default:
                    return false;
            }
        }

        public static bool IsFusionableRarity(this Define.EUNIT_RARITY rarity)
        {
            switch (rarity)
            {

                case Define.EUNIT_RARITY.Normal:
                case Define.EUNIT_RARITY.Rare:
                    return true;
                case Define.EUNIT_RARITY.Hero:
                    break;
                case Define.EUNIT_RARITY.Legend:
                    break;
                case Define.EUNIT_RARITY.Myth:
                    break;
                case Define.EUNIT_RARITY.Boss:
                    break;
                default:
                    break;
            }

            return false;
        }
    }
}
