using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TypeUtil
{
    public class Type
    {
        static int[][] typePer = new int[][]{ 
            new int[] {100,100,100,100,100,100,100,100,100,100,100,100, 50,  0,100,100,100,100},
            new int[] {100, 50, 50,100,200,200,100,100,100,100,100,200, 50,100, 50,100,200,100},
            new int[] {100,200, 50,100, 50,100,100,100,200,100,100,100,200,100, 50,100,100,100},
            new int[] {100,100,200, 50, 50,100,100,100,  0,200,100,100,100,100, 50,100,100,100},
            new int[] {100, 50,200,100, 50,100,100, 50,200, 50,100, 50,200,100, 50,100, 50,100},
            new int[] {100, 50, 50,100,200, 50,100,100,200,200,100,100,100,100,200,100, 50,100},
            new int[] {200,100,100,100,100,200,100, 50,100, 50, 50, 50,200,  0,100,200,200, 50},
            new int[] {100,100,200,100,200, 50,100, 50, 50,100,100,100, 50, 50,100,100,  0,200},
            new int[] {100,200,100,200, 50,100,100,200,100,  0,100, 50,200,100,100,100,200,100},
            new int[] {100,100,100, 50,200,100,200,100,100,100,100,200, 50,100,100,100, 50,100},
            new int[] {100,100,100,100,100,100,200,200,100,100, 50,100,100,100,100,  0,100,100},
            new int[] {100, 50,100,100,200,100, 50, 50,100, 50,200,100,100, 50,100,200, 50,200},
            new int[] {100,200,100,100,100,200, 50,100, 50,200,100,200,100,100,100,100, 50,100},
            new int[] {  0,100,100,100, 50,100,100,100,100,100,200,100,100,200,100, 50,100,100},
            new int[] {100,100,100,100,100,100,100,100,100,100,100,100,100,100,200,100, 50,  0},
            new int[] {100,100,100,100,100,100, 50,100,100,100,200,100,100,200,100, 50,100,100},
            new int[] {100, 50, 50, 50,100,200,100,100,100,100,100,100,200,100,100,100, 50,200},
            new int[] {100,100,100,100, 50,100,200, 50,100,100,100,100,100,100,200,200, 50,100}
        };
        static int[][] skillPer = new int[][]{
            new int[] {100, 50,200,100,200, 50,100,100,200,200,100,100,100,100,200,100, 50,100},
            new int[] {100,100,200,100,200, 50,100, 50, 50,100,100,100, 50, 50,100,100,200,200},
            new int[] {  0,100,100,100, 50,100,100,100,100,100,200,100,100,200,100, 50,100,  0},
            new int[] {100,100,100,100, 50,100,200, 50,100,100,100,100,100,200,200,200, 50,100},
            new int[] {100,200,100,200, 50,100,100,200,100,100,100, 50,200,100,100,100,200,100}
        };
        public static int Per(string atk,string def1,string def2)
        {
            int atkNum = TypeToNum(atk);
            int def1Num = TypeToNum(def1);
            int def2Num = TypeToNum(def2);

            if (def2Num == -1)
            {
                return typePer[atkNum][def1Num];
            }
            else
            {
                return typePer[atkNum][def1Num] * typePer[atkNum][def2Num] / 100;
            }
        }
        //atk 0�t���h�� 1�ӂ��傭 2���� 3�悤 4�͂�
        public static int PerSkill(int atk, string def1, string def2)
        {
            int atkNum = atk;
            int def1Num = TypeToNum(def1);
            int def2Num = TypeToNum(def2);


            if (def2Num == -1)
            {
                return skillPer[atkNum][def1Num];
            }
            else
            {
                return skillPer[atkNum][def1Num] * skillPer[atkNum][def2Num] / 100;
            }
        }
        public static int Per(int atkNum, string def1, string def2)
        {
            int def1Num = TypeToNum(def1);
            int def2Num = TypeToNum(def2);
            if (def2Num == -1)
            {
                return typePer[atkNum][def1Num];
            }
            else
            {
                return typePer[atkNum][def1Num] * typePer[atkNum][def2Num] / 100;
            }
        }
        public static string NumToType(int i)
        {
            string type = "";
            switch (i)
            {
                case 0:
                    type = "�m�[�}��";
                    break;
                case 1:
                    type = "�ق̂�";
                    break;
                case 2:
                    type = "�݂�";
                    break;
                case 3:
                    type = "�ł�";
                    break;
                case 4:
                    type = "����";
                    break;
                case 5:
                    type = "������";
                    break;
                case 6:
                    type = "�����Ƃ�";
                    break;
                case 7:
                    type = "�ǂ�";
                    break;
                case 8:
                    type = "���߂�";
                    break;
                case 9:
                    type = "�Ђ���";
                    break;
                case 10:
                    type = "�G�X�p�[";
                    break;
                case 11:
                    type = "�ނ�";
                    break;
                case 12:
                    type = "����";
                    break;
                case 13:
                    type = "�S�[�X�g";
                    break;
                case 14:
                    type = "�h���S��";
                    break;
                case 15:
                    type = "����";
                    break;
                case 16:
                    type = "�͂���";
                    break;
                case 17:
                    type = "�t�F�A���[";
                    break;
            }
            return type;
        }
        public static int TypeToNum(string type)
        {
            int num = -1;
            switch (type)
            {
                case "�m�[�}��":
                    num = 0;
                    break;
                case "�ق̂�":
                    num = 1;
                    break;
                case "�݂�":
                    num = 2;
                    break;
                case "�ł�":
                    num = 3;
                    break;
                case "����":
                    num = 4;
                    break;
                case "������":
                    num = 5;
                    break;
                case "�����Ƃ�":
                    num = 6;
                    break;
                case "�ǂ�":
                    num = 7;
                    break;
                case "���߂�":
                    num = 8;
                    break;
                case "�Ђ���":
                    num = 9;
                    break;
                case "�G�X�p�[":
                    num = 10;
                    break;
                case "�ނ�":
                    num = 11;
                    break;
                case "����":
                    num = 12;
                    break;
                case "�S�[�X�g":
                    num = 13;
                    break;
                case "�h���S��":
                    num = 14;
                    break;
                case "����":
                    num = 15;
                    break;
                case "�͂���":
                    num = 16;
                    break;
                case "�t�F�A���[":
                    num = 17;
                    break;
            }

            return num;
        }
        public static Color TypeToColor(string type)
        {
            string htmlString = "";
            switch (type)
            {
                case "�m�[�}��":
                    htmlString = "#aea886";
                    break;
                case "�ق̂�":
                    htmlString = "#f45c19";
                    break;
                case "�݂�":
                    htmlString = "#4a96d6";
                    break;
                case "�ł�":
                    htmlString = "#eaa317";
                    break;
                case "����":
                    htmlString = "#28b25c";
                    break;
                case "������":
                    htmlString = "#45a9c0";
                    break;
                case "�����Ƃ�":
                    htmlString = "#9a3d3e";
                    break;
                case "�ǂ�":
                    htmlString = "#8f5b98";
                    break;
                case "���߂�":
                    htmlString = "#916d3c";
                    break;
                case "�Ђ���":
                    htmlString = "#7e9ecf";
                    break;
                case "�G�X�p�[":
                    htmlString = "#d56d8b";
                    break;
                case "�ނ�":
                    htmlString = "#989001";
                    break;
                case "����":
                    htmlString = "#878052";
                    break;
                case "�S�[�X�g":
                    htmlString = "#555fa4";
                    break;
                case "�h���S��":
                    htmlString = "#454ba6";
                    break;
                case "����":
                    htmlString = "#7a0049";
                    break;
                case "�͂���":
                    htmlString = "#9b9b9b";
                    break;
                case "�t�F�A���[":
                    htmlString = "#ffbbff";
                    break;
            }
            ColorUtility.TryParseHtmlString(htmlString, out Color color);

            return color;
        }
    }
}