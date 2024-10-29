using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TextEditorLib
{
    public class FileManager
    {

        public string readTextFromFile(string fileName)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            string fileContent = File.ReadAllText(fileInfo.FullName);
            return fileContent;

        }

        public void saveTextToFile(string fileName, string text)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            StreamWriter writer = fileInfo.CreateText();
            writer.Write(text);

            writer.Flush();
            writer.Close();
        }
    }

    public class TextManager
    {
        public int SearchText(string srcText, string searchText)
        {
            if (string.IsNullOrEmpty(srcText) || string.IsNullOrEmpty(searchText))
            {
                return -1;
            }

            int index = srcText.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);

            return index;
        }

        public string CopyText(string srcText, int startIdx, int endIdx)
        {
            if (string.IsNullOrEmpty(srcText) || startIdx < 0 || endIdx >= srcText.Length || startIdx > endIdx)
            {
                return "";
            }

            int length = endIdx - startIdx + 1;
            return srcText.Substring(startIdx, length);
        }

        // insertIndex = -1 -> insert to end
        public string InsertText(string srcText, string insertText, int insertIndex)
        {
            if (string.IsNullOrEmpty(srcText))
            {
                return insertText;
            }

            if (insertIndex == -1) {
                insertIndex = srcText.Length-1;
            }

            if (insertIndex < 0 || insertIndex > srcText.Length)
            {
                return srcText;
            }

            return srcText.Insert(insertIndex, insertText);
        }

        public string CutText(string srcText, int startIdx, int endIdx)
        {
            if (string.IsNullOrEmpty(srcText) || startIdx < 0 || endIdx >= srcText.Length || startIdx > endIdx)
            {
                return srcText;
            }

            return srcText.Remove(startIdx, endIdx - startIdx + 1);
        }

        public string ReplaceText(string srcText, string oldText, string newText)
        {
            if (string.IsNullOrEmpty(srcText) || string.IsNullOrEmpty(oldText))
            {
                return srcText;
            }

            // Заменяем все вхождения oldText на newText
            return srcText.Replace(oldText, newText);
        }
    }

    public class TextBuffer
    {
        private FileManager fm;
        private Timer timer;

        public List<string> history = new List<string>();
        public List<string> redoDeque = new List<string>();

        private readonly int periodTime = 5000;
        private readonly int defaultBufferSize = 100;

        public TextBuffer()
        {
            this.fm = new FileManager();
        }

        public void Undo()
        {
            if (this.history.Count == 1) {
                return;
            }

            string last = this.history.Last();
            this.history.RemoveAt(this.history.Count - 1);
            
            if (this.redoDeque.Count > this.defaultBufferSize)
            {
                this.redoDeque.RemoveAt(0);
            }

            this.redoDeque.Add(last);
        }

        public void Redo()
        {
            if (this.redoDeque.Count == 0)
            {
                return;
            }

            string last = this.redoDeque.Last();
            this.redoDeque.RemoveAt(this.redoDeque.Count - 1);
            this.history.Add(last);
        }

        public void updateState(string newState)
        {
            if (this.history.Count > this.defaultBufferSize) {
                this.history.RemoveAt(0);
            }

            if (this.history.Count == 0 || this.history.Last() != newState)
            {
                this.history.Add(newState);
                this.redoDeque.Clear();
            }
        }

        public void ClearRedo()
        {
            if (this.redoDeque.Count > 0 )
            {
                this.redoDeque.Clear();
            }
        }

        public string getState()
        {
            return this.history.Last();
        }

        public void clearState()
        {
            this.history.Clear();
            this.redoDeque.Clear();
        }
        
        public void startAutoSave(string fileName)
        {
            this.timer = new Timer(state =>
            {
                this.fm.saveTextToFile(fileName, this.history.Last());
            }, null, periodTime, periodTime);

        }

        public void stopAutoSave()
        {
            this.timer.Dispose();
        }
    }

}
