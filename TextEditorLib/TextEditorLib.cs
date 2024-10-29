namespace TextEditorLib
{
    public class TextManager
    {
        private string someStr;
        public TextManager(string someStr)
        {
            this.someStr = someStr;
        }

        public string getSomeStr()
        {
            return this.someStr;
        }
    }
}
