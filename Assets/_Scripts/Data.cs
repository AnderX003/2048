using System.Collections.Generic;
using UnityEngine;

namespace _Scripts
{
    public static class Data
    {
        public static int Score { get; set; }
        public static int MaxValue { get; set; }
        public static int CurrentTheme { get; set; }

        #region Colors

        private static Color32[][] Colors { get; } =
        {
            new[]
            {
                new Color32(239, 239, 239, 255),
                new Color32(177, 238, 221, 255),
                new Color32(99, 204, 234, 255),
                new Color32(179, 130, 227, 255),
                new Color32(57, 204, 143, 255),
                new Color32(255, 127, 30, 255),
                new Color32(0, 178, 161, 255),
                new Color32(255, 102, 154, 255),
                new Color32(134, 154, 223, 255),
                new Color32(95, 186, 98, 255),
                new Color32(255, 71, 71, 255),
                new Color32(255, 209, 25, 255),
                new Color32(94, 146, 161, 255),
                new Color32(71, 107, 184, 255),
                new Color32(254, 87, 34, 255)
            },
            new[]
            {
                new Color32(241, 241, 238, 255),
                new Color32(236, 254, 129, 255),
                new Color32(186, 241, 157, 255),
                new Color32(125, 232, 125, 255),
                new Color32(52, 203, 140, 255),
                new Color32(129, 181, 254, 255),
                new Color32(184, 138, 229, 255),
                new Color32(222, 160, 218, 255),
                new Color32(255, 128, 170, 255),
                new Color32(255, 92, 92, 255),
                new Color32(255, 139, 60, 255),
                new Color32(255, 219, 76, 255)
            },
            new[]
            {
                new Color32(241, 238, 240, 255),
                new Color32(254, 216, 224, 255),
                new Color32(223, 197, 234, 255),
                new Color32(184, 138, 229, 255),
                new Color32(241, 254, 164, 255),
                new Color32(251, 226, 122, 255),
                new Color32(255, 179, 136, 255),
                new Color32(255, 149, 76, 255),
                new Color32(252, 131, 135, 255),
                new Color32(255, 92, 92, 255),
                new Color32(148, 209, 151, 255),
                new Color32(159, 197, 254, 255)
            },
            new[]
            {
                new Color32(204, 204, 204, 255),
                new Color32(160, 223, 160, 255),
                new Color32(153, 220, 186, 255),
                new Color32(140, 217, 209, 255),
                new Color32(137, 210, 251, 255),
                new Color32(140, 183, 242, 255),
                new Color32(185, 161, 232, 255),
                new Color32(217, 173, 235, 255),
                new Color32(255, 153, 187, 255),
                new Color32(254, 114, 114, 255),
                new Color32(255, 149, 76, 255),
                new Color32(255, 214, 51, 255)
            },
        };

        public static Color32 GetColorByValue(int value)
        {
            int i = 0;
            for (; value != 2; value /= 2, i++) { }

            return Colors[CurrentTheme].Length > i + 1 ? Colors[CurrentTheme][i + 1] : Colors[CurrentTheme][0];
            /*try
            {
                return Colors[CurrentTheme][i + 1];
            }
            catch
            {
                return Colors[CurrentTheme][0];
            }*/
        }

        public static Color32[,] UIColors { get; } =
        {
            {
                new Color32(45, 45, 45, 255), //Background
                new Color32(70, 70, 70, 255), //Unit_Empty
                new Color32(0,0,0,140), //Unit_Text
                new Color32(255, 127, 30, 255), //Btn_Achievemnts
                new Color32(57, 204, 143, 255), //Btn_GPGS
                new Color32(99, 204, 234, 255), //Btn_Leaderboards
                new Color32(255, 127, 30, 255), //Btn_Menu
                new Color32(255, 209, 25, 255), //Btn_Play
                new Color32(57, 204, 143, 255), //Btn_Replay
                new Color32(0,0,0,150), //Btn_Text_Icon
                new Color32(179, 130, 227, 255), //Btn_Themes
                new Color32(179, 130, 227, 255), //Btn_Undo
                new Color32(255, 209, 25, 255), //Buble_Best
                new Color32(45, 45, 45, 255), //Buble_Msgbox
                new Color32(0,0,0,128), //Buble_Msgbox_Background
                new Color32(70, 70, 70, 255), //Buble_Msgbox_Btn
                new Color32(99, 204, 234, 255), //Buble_Score
                new Color32(239, 239, 239, 255), //Text_Size_Btns
                new Color32(239, 239, 239, 255), //Text_2048
                new Color32(239, 239, 239, 255), //Text_Game_Over
                new Color32(239, 239, 239, 255), //Buble_Msgbox_Text
                new Color32(239, 239, 239, 255), //Buble_Msgbox_Btn_Text
            },
            {
                new Color32(47, 47, 44, 255), //Background
                new Color32(72, 72, 69, 255), //Unit_Empty
                new Color32(0,0,0,140), //Unit_Text
                new Color32(236, 254, 129, 255), //Btn_Achievemnts
                new Color32(186, 241, 157, 255), //Btn_GPGS
                new Color32(177, 238, 221, 255), //Btn_Leaderboards
                new Color32(236, 254, 129, 255), //Btn_Menu
                new Color32(186, 241, 157, 255), //Btn_Play
                new Color32(177, 238, 221, 255), //Btn_Replay
                new Color32(0,0,0,150), //Btn_Text_Icon
                new Color32(216, 200, 236, 255), //Btn_Themes
                new Color32(216, 200, 236, 255), //Btn_Undo
                new Color32(186, 241, 157, 255), //Buble_Best
                new Color32(47, 47, 44, 255), //Buble_Msgbox
                new Color32(0,0,0,128), //Buble_Msgbox_Background
                new Color32(72, 72, 69, 255), //Buble_Msgbox_Btn
                new Color32(177, 238, 221, 255), //Buble_Score
                new Color32(241, 241, 238, 255), //Text_Size_Btns
                new Color32(241, 241, 238, 255), //Text_2048
                new Color32(241, 241, 238, 255), //Text_Game_Over
                new Color32(241, 241, 238, 255), //Buble_Msgbox_Text
                new Color32(241, 241, 238, 255), //Buble_Msgbox_Btn_Text
            },
            {
                new Color32(34, 31, 33, 255), //Background
                new Color32(54, 45, 46, 255), //Unit_Empty
                new Color32(0,0,0,140), //Unit_Text
                new Color32(241, 254, 164, 255), //Btn_Achievemnts
                new Color32(251, 226, 122, 255), //Btn_GPGS
                new Color32(254, 216, 224, 255), //Btn_Leaderboards
                new Color32(255, 179, 136, 255), //Btn_Menu
                new Color32(254, 216, 224, 255), //Btn_Play
                new Color32(241, 254, 164, 255), //Btn_Replay
                new Color32(0,0,0,150), //Btn_Text_Icon
                new Color32(255, 179, 136, 255), //Btn_Themes
                new Color32(254, 216, 224, 255), //Btn_Undo
                new Color32(254, 216, 224, 255), //Buble_Best
                new Color32(34, 31, 33, 255), //Buble_Msgbox
                new Color32(0,0,0,128), //Buble_Msgbox_Background
                new Color32(54, 45, 46, 255), //Buble_Msgbox_Btn
                new Color32(251, 226, 122, 255), //Buble_Score
                new Color32(241, 238, 240, 255), //Text_Size_Btns
                new Color32(241, 238, 240, 255), //Text_2048
                new Color32(241, 238, 240, 255), //Text_Game_Over
                new Color32(241, 238, 240, 255), //Buble_Msgbox_Text
                new Color32(241, 238, 240, 255), //Buble_Msgbox_Btn_Text
            },
            {
                new Color32(255, 255, 255, 255), //Background
                new Color32(234, 234, 234, 255), //Unit_Empty
                new Color32(0,0,0,140), //Unit_Text
                new Color32(255, 214, 51, 255), //Btn_Achievemnts
                new Color32(254, 143, 114, 255), //Btn_GPGS
                new Color32(156, 217, 252, 255), //Btn_Leaderboards
                new Color32(255, 214, 51, 255), //Btn_Menu
                new Color32(179, 229, 204, 255), //Btn_Play
                new Color32(156, 217, 252, 255), //Btn_Replay
                new Color32(0,0,0,150), //Btn_Text_Icon
                new Color32(217, 173, 235, 255), //Btn_Themes
                new Color32(217, 173, 235, 255), //Btn_Undo
                new Color32(179, 229, 204, 255), //Buble_Best
                new Color32(220, 244, 255, 255), //Buble_Msgbox
                new Color32(255,255,255,128), //Buble_Msgbox_Background
                new Color32(195, 232, 253, 255), //Buble_Msgbox_Btn
                new Color32(254, 143, 114, 255), //Buble_Score
                new Color32(0,0,0,128), //Text_Size_Btns
                new Color32(0,0,0,102), //Text_2048
                new Color32(0,0,0,102), //Text_Game_Over
                new Color32(0,0,0,150), //Buble_Msgbox_Text
                new Color32(0,0,0,150), //Buble_Msgbox_Btn_Text
            },
        };

        #endregion

        #region Layouts

        public static Dictionary<int, float> CellSizes { get; } = new Dictionary<int, float>
        {
            [3] = 322f,
            [4] = 240f,
            [5] = 191.18f,
            [6] = 158.86f,
            [7] = 135.89f,
            [8] = 118.73f
        };

        public static Dictionary<int, int> CellTextSizes { get; } = new Dictionary<int, int>
        {
            [3] = 115,
            [4] = 86,
            [5] = 68,
            [6] = 59,
            [7] = 48,
            [8] = 42
        };

        public static float[,,] CurrentLayout { get; set; }

        private static float[,,] layout_3 { get; } =
        {
            {{-352f, -352f}, {-352f, 0f}, {-352f, 352f}},
            {{0f, -352f}, {0f, 0f}, {0f, 352f}},
            {{352f, -352f}, {352f, 0f}, {352f, 352f}}
        };

        private static float[,,] layout_4 { get; } =
        {
            {{-393f, -393f}, {-393f, -131f}, {-393f, 131f}, {-393f, 393f}},
            {{-131f, -393f}, {-131f, -131f}, {-131f, 131f}, {-131f, 393f}},
            {{131f, -393f}, {131f, -131f}, {131f, 131f}, {131f, 393f}},
            {{393f, -393f}, {393f, -131f}, {393f, 131f}, {393f, 393f}}
        };

        private static float[,,] layout_5 { get; } =
        {
            {
                {-417.41f, -417.41f}, {-417.41f, -208.71f}, {-417.41f, 0f}, {-417.41f, 208.71f}, {-417.41f, 417.41f}
            },
            {
                {-208.71f, -417.41f}, {-208.71f, -208.71f}, {-208.71f, 0f}, {-208.71f, 208.71f}, {-208.71f, 417.41f}
            },
            {
                {0f, -417.41f}, {0f, -208.71f}, {0f, 0f}, {0, 208.71f}, {0f, 417.41f}
            },
            {
                {208.71f, -417.41f}, {208.71f, -208.71f}, {208.71f, 0f}, {208.71f, 208.71f}, {208.71f, 417.41f}
            },
            {
                {417.41f, -417.41f}, {417.41f, -208.71f}, {417.41f, 0f}, {417.41f, 208.71f}, {417.41f, 417.41f}
            }
        };

        private static float[,,] layout_6 { get; } =
        {
            {
                {-433.57f, -433.57f}, {-433.57f, -260.14f}, {-433.57f, -86.71f}, {-433.57f, 86.71f}, {-433.57f, 260.14f}, {-433.57f, 433.57f}
            },
            {
                {-260.14f, -433.57f}, {-260.14f, -260.14f}, {-260.14f, -86.71f}, {-260.14f, 86.71f}, {-260.14f, 260.14f}, {-260.14f, 433.57f}
            },
            {
                {-86.71f, -433.57f}, {-86.71f, -260.14f}, {-86.71f, -86.71f}, {-86.71f, 86.71f}, {-86.71f, 260.14f}, {-86.71f, 433.57f}
            },
            {
                {86.71f, -433.57f}, {86.71f, -260.14f}, {86.71f, -86.71f}, {86.71f, 86.71f}, {86.71f, 260.14f}, {86.71f, 433.57f}
            },
            {
                {260.14f, -433.57f}, {260.14f, -260.14f}, {260.14f, -86.71f}, {260.14f, 86.71f}, {260.14f, 260.14f}, {260.14f, 433.57f}
            },
            {
                {433.57f, -433.57f}, {433.57f, -260.14f}, {433.57f, -86.71f}, {433.57f, 86.71f}, {433.57f, 260.14f}, {433.57f, 433.57f}
            }
        };

        private static float[,,] layout_7 { get; } =
        {
            {
                {-445.05f, -445.05f}, {-445.05f, -296.7f}, {-445.05f, -148.35f}, {-445.05f, 0f}, {-445.05f, 148.35f}, {-445.05f, 296.7f}, {-445.05f, 445.05f}
            },
            {
                {-296.7f, -445.05f}, {-296.7f, -296.7f}, {-296.7f, -148.35f}, {-296.7f, 0f}, {-296.7f, 148.35f}, {-296.7f, 296.7f}, {-296.7f, 445.05f}
            },
            {
                {-148.35f, -445.05f}, {-148.35f, -296.7f}, {-148.35f, -148.35f}, {-148.35f, 0f}, {-148.35f, 148.35f}, {-148.35f, 296.7f}, {-148.35f, 445.05f}
            },
            {
                {0f, -445.05f}, {0f, -296.7f}, {0f, -148.35f}, {0f, 0f}, {0f, 148.35f}, {0f, 296.7f}, {0f, 445.05f}
            },
            {
                {148.35f, -445.05f}, {148.35f, -296.7f}, {148.35f, -148.35f}, {148.35f, 0f}, {148.35f, 148.35f}, {148.35f, 296.7f}, {148.35f, 445.05f}
            },
            {
                {296.7f, -445.05f}, {296.7f, -296.7f}, {296.7f, -148.35f}, {296.7f, 0f}, {296.7f, 148.35f}, {296.7f, 296.7f}, {296.7f, 445.05f}
            },
            {
                {445.05f, -445.05f}, {445.05f, -296.7f}, {445.05f, -148.35f}, {445.05f, 0f}, {445.05f, 148.35f}, {445.05f, 296.7f}, {445.05f, 445.05f}
            }
        };

        private static float[,,] layout_8 { get; } =
        {
            {
                {-453.64f, -453.64f}, {-453.64f, -324.03f}, {-453.64f, -194.42f}, {-453.64f, -64.8f}, {-453.64f, 64.8f}, {-453.64f, 194.42f}, {-453.64f, 324.03f}, {-453.64f, 453.64f}
            },
            {
                {-324.03f, -453.64f}, {-324.03f, -324.03f}, {-324.03f, -194.42f}, {-324.03f, -64.8f}, {-324.03f, 64.8f}, {-324.03f, 194.42f}, {-324.03f, 324.03f}, {-324.03f, 453.64f}
            },
            {
                {-194.42f, -453.64f}, {-194.42f, -324.03f}, {-194.42f, -194.42f}, {-194.42f, -64.8f}, {-194.42f, 64.8f}, {-194.42f, 194.42f}, {-194.42f, 324.03f}, {-194.42f, 453.64f}
            },
            {
                {-64.8f, -453.64f}, {-64.8f, -324.03f}, {-64.8f, -194.42f}, {-64.8f, -64.8f}, {-64.8f, 64.8f}, {-64.8f, 194.42f}, {-64.8f, 324.03f}, {-64.8f, 453.64f}
            },
            {
                {64.8f, -453.64f}, {64.8f, -324.03f}, {64.8f, -194.42f}, {64.8f, -64.8f}, {64.8f, 64.8f}, {64.8f, 194.42f}, {64.8f, 324.03f}, {64.8f, 453.64f}
            },
            {
                {194.42f, -453.64f}, {194.42f, -324.03f}, {194.42f, -194.42f}, {194.42f, -64.8f}, {194.42f, 64.8f}, {194.42f, 194.42f}, {194.42f, 324.03f}, {194.42f, 453.64f}
            },
            {
                {324.03f, -453.64f}, {324.03f, -324.03f}, {324.03f, -194.42f}, {324.03f, -64.8f}, {324.03f, 64.8f}, {324.03f, 194.42f}, {324.03f, 324.03f}, {324.03f, 453.64f}
            },
            {
                {453.64f, -453.64f}, {453.64f, -324.03f}, {453.64f, -194.42f}, {453.64f, -64.8f}, {453.64f, 64.8f}, {453.64f, 194.42f}, {453.64f, 324.03f}, {453.64f, 453.64f}
            }
        };

        public static Dictionary<int, float[,,]> layouts { get; } = new Dictionary<int, float[,,]>
        {
            [3] = layout_3,
            [4] = layout_4,
            [5] = layout_5,
            [6] = layout_6,
            [7] = layout_7,
            [8] = layout_8
        };

        #endregion
    }
}