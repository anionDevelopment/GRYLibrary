using System.Collections.Generic;

namespace GRYLibrary.Core.Misc
{
    public class NonPersistentInputHistoryList
    {
        private readonly IList<string> UserInputs = [];
        private int CurrentuserInputIndex = 0;
        public void EnterPressed(string input)
        {
            input = input.Trim();
            this.UserInputs.Remove(input);
            this.UserInputs.Add(input);
            this.ResetCurrentReadPosition();
        }
        public string UpPressed()
        {
            if (this.CurrentuserInputIndex > 0)
            {
                this.CurrentuserInputIndex -= 1;
            }
            return this.GetCurrentItem();
        }
        public string DownPressed()
        {
            if (this.CurrentuserInputIndex < this.UserInputs.Count)
            {
                this.CurrentuserInputIndex += 1;
            }
            return this.GetCurrentItem();
        }
        public void ResetCurrentReadPosition()
        {
            this.CurrentuserInputIndex = this.UserInputs.Count;
        }

        private string GetCurrentItem()
        {
            if (this.CurrentuserInputIndex == this.UserInputs.Count || this.UserInputs.Count == 0)
            {
                return string.Empty;
            }
            return this.UserInputs[this.CurrentuserInputIndex];
        }
    }
}