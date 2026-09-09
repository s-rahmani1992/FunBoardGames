using System;
using System.Collections.Generic;

namespace FunBoardGames.Client
{
    public class ButtonData
    {
        public string Text;
        public Action Action;

        public ButtonData(string text, Action action)
        {
            Text = text;
            Action = action;
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
