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
        //atk 0フリドラ 1ふしょく 2いん 3よう 4はね
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
                    type = "ノーマル";
                    break;
                case 1:
                    type = "ほのお";
                    break;
                case 2:
                    type = "みず";
                    break;
                case 3:
                    type = "でんき";
                    break;
                case 4:
                    type = "くさ";
                    break;
                case 5:
                    type = "こおり";
                    break;
                case 6:
                    type = "かくとう";
                    break;
                case 7:
                    type = "どく";
                    break;
                case 8:
                    type = "じめん";
                    break;
                case 9:
                    type = "ひこう";
                    break;
                case 10:
                    type = "エスパー";
                    break;
                case 11:
                    type = "むし";
                    break;
                case 12:
                    type = "いわ";
                    break;
                case 13:
                    type = "ゴースト";
                    break;
                case 14:
                    type = "ドラゴン";
                    break;
                case 15:
                    type = "あく";
                    break;
                case 16:
                    type = "はがね";
                    break;
                case 17:
                    type = "フェアリー";
                    break;
            }
            return type;
        }
        public static int TypeToNum(string type)
        {
            int num = -1;
            switch (type)
            {
                case "ノーマル":
                    num = 0;
                    break;
                case "ほのお":
                    num = 1;
                    break;
                case "みず":
                    num = 2;
                    break;
                case "でんき":
                    num = 3;
                    break;
                case "くさ":
                    num = 4;
                    break;
                case "こおり":
                    num = 5;
                    break;
                case "かくとう":
                    num = 6;
                    break;
                case "どく":
                    num = 7;
                    break;
                case "じめん":
                    num = 8;
                    break;
                case "ひこう":
                    num = 9;
                    break;
                case "エスパー":
                    num = 10;
                    break;
                case "むし":
                    num = 11;
                    break;
                case "いわ":
                    num = 12;
                    break;
                case "ゴースト":
                    num = 13;
                    break;
                case "ドラゴン":
                    num = 14;
                    break;
                case "あく":
                    num = 15;
                    break;
                case "はがね":
                    num = 16;
                    break;
                case "フェアリー":
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
                case "ノーマル":
                    htmlString = "#aea886";
                    break;
                case "ほのお":
                    htmlString = "#f45c19";
                    break;
                case "みず":
                    htmlString = "#4a96d6";
                    break;
                case "でんき":
                    htmlString = "#eaa317";
                    break;
                case "くさ":
                    htmlString = "#28b25c";
                    break;
                case "こおり":
                    htmlString = "#45a9c0";
                    break;
                case "かくとう":
                    htmlString = "#9a3d3e";
                    break;
                case "どく":
                    htmlString = "#8f5b98";
                    break;
                case "じめん":
                    htmlString = "#916d3c";
                    break;
                case "ひこう":
                    htmlString = "#7e9ecf";
                    break;
                case "エスパー":
                    htmlString = "#d56d8b";
                    break;
                case "むし":
                    htmlString = "#989001";
                    break;
                case "いわ":
                    htmlString = "#878052";
                    break;
                case "ゴースト":
                    htmlString = "#555fa4";
                    break;
                case "ドラゴン":
                    htmlString = "#454ba6";
                    break;
                case "あく":
                    htmlString = "#7a0049";
                    break;
                case "はがね":
                    htmlString = "#9b9b9b";
                    break;
                case "フェアリー":
                    htmlString = "#ffbbff";
                    break;
            }
            ColorUtility.TryParseHtmlString(htmlString, out Color color);

            return color;
        }
    }
}