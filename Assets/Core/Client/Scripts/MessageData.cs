using System;
using System.Collections.Generic;

namespace FunBoardGames.Client
{
    public enum ButtonColor
    {
        Blue,
        Green,
        Red
    }

    public class ButtonData
    {
        public string Text;
        public Action Action;
        public ButtonColor Color;

        public ButtonData(string text, Action action, ButtonColor color = ButtonColor.Green)
        {
            Text = text;
            Action = action;
            Color = color;
        }
    }

    public class MessageData
    {
        public string Title;
        public string Message;
        public List<ButtonData> Buttons;

        public MessageData(string title, string message, List<ButtonData> buttons)
        {
            Title = title;
            Message = message;
            Buttons = buttons;
        }
    }
}
