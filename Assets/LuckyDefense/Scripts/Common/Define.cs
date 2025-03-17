using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Data
{
    public class Define
    {
        public enum Scene
        {
            Unknown = 0,
            Login = 1,
            GameScene = 2,
            Loading = 3,
            Lobby = 4,
            ResourceRelease = 5,


            Debug = 999,
        }

        public enum ConstDefType
        {
            None = 0,
            GroupUnitCount = 1, //�׷� ���� ��
            CritDmg = 2, //�⺻ ũ��Ƽ�� ������
        }


        public enum EUnitType
        {
            None = 0,
            Player = 1,
            Monster = 2,
        }

        public enum EUNIT_RARITY
        {
            None = 0,
            Normal = 1, // �Ϲ�
            Rare = 2, // ����
            Hero = 3, // ����
            Legend = 4,
            Myth = 5, // ��ȭ

            Boss = 11, // ���� ����
        }

        public enum EGAME_LEVEL
        {
            Normal = 1,
            Hard = 2,
            Hell = 3,
            God = 4,
        }



        public enum EUNIT_STATE
        {
            IDLE = 0,
            RUN = 1,
            ATTACK = 2,
            DIE = 3,
            DESTROY = 4,
            [InspectorName("��ų1")]
            ACTIVE_SKILL = 5,
            [InspectorName("��ų2")]
            ACTIVE_SKILL2 = 6,
        }


        public enum DECIMALROUND
        {
            None,   //�Ҽ� ������ ����
            RoundUp, //�ø�
            RoundDown,//����
            Round,//�ݿø�
        }

        public enum Notation
        {
            None,   //��ǥ��
            Amount, //���� ǥ��
            IsUnits,//k, m�� ���� ������ ǥ���Ҷ� ���
            Percent,//�ۼ�Ʈ ǥ���Ҷ� ���
        }

        public enum EPOPUP_TYPE
        {
            None = 0,
            PopupNetWait = 1,
            PopupConfirm = 2,
            PopupYesNo = 3,
            PopupOption = 4,
            PopupPause = 5,
            PopupLock = 6,
            PopupPush = 7,
            PopupCountDown = 8,
            PopupInGameResult = 9,

            PopupGamble = 10,
            PopupMyth = 11,
        }

        public enum EFONT_TYPE
        {
            ENONE = 0,
            Default = 1,
        }

        public enum StringFileType
        {
            Normal = 0,
            ErrorStr = 1,
            BuildStr = 2,
        }

        public enum ECONTENT_TYPE
        {
            LOBBY,
            MAIN,
            INGAME,
        }

        public enum EUNIT_DIRECTION
        {
            UP,
            DOWN,
            LEFT,
            RIGHT,
        }

        [Flags]
        public enum EGAME_STATE
        {
            LOADING = 0,
            LOADING_COMPLETE = 1 << 1,
            ENTRY = 1 << 2,
            PLAY = 1 << 3,
            PAUSE = 1 << 4,
            RESULT = 1 << 5,
            COMMANDER = 1 << 6,
            MANAGE = 1 << 7,
            ENTRY_COMPLETE = 1 << 8,
        }

        public enum TimeType
        {
            UTC = 0,
            Local = 1,
            ServerUTC = 2,
        }

        public enum AssetLabel
        {
            Default = 0,
            Popup = 1,
            Script = 2,
            Font = 3,
            UI = 4,
            Material = 5,
            Particle = 6,
        }

        public enum Sound
        {
            Master = 0,
            Bgm = 1,
            AnotherBgm = 2,
            Effect = 3,
            Voice = 4,
            Max = 5,
        }

        public enum EDAMAGE_TYPE
        {
            NONE = 0,
            PHYSICAL = 1,
            MAGICAL = 2,
        }

        public enum ESEARCH_TARGET_TYPE
        {
            [InspectorName("����� ��")]
            NeareastEnemy = 0,
            [InspectorName("��ǥ ����")]
            TargetDesignation = 1,
            [InspectorName("HPLoss �Ʊ�")]
            LowestHPTeam = 2,
            [InspectorName("����� �Ʊ�")]
            NeareastTeam = 3,
        }

        public enum SkillType
        {
            None = 0,
            Passive = 1,
            NormalAttack = 2,
            Active1 = 3,
            Active2 = 4,
        }

        public enum EStatType
        {
            NONE = 0,
            ATK = 1,
            DEF = 2,
            LIFE = 3,
            ATK_SPEED = 4,
            MOVE_SPEED = 5,
            CRITICAL = 6,
        }

        public enum ItemType
        {
            None = 0,
            Gold = 1,
            Stone = 2,
        }

    }
}
