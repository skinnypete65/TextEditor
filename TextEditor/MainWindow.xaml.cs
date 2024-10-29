using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TextEditorLib;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection.Emit;
using System.Runtime.Remoting.Contexts;

namespace TextEditor
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private FileManager fm;
        private TextManager tm;
        private TextBuffer tb;

        private string textFileFilter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
        private string curFileName = "";

        private readonly int periodTime = 1;
        private DispatcherTimer textCheckTimer;

        public MainWindow()
        {
            InitializeComponent();

            this.fm = new FileManager();
            this.tm = new TextManager();
            this.tb = new TextBuffer();

            this.Closed += onWindowClosed;

            this.textCheckTimer = new DispatcherTimer();
            this.textCheckTimer.Interval = TimeSpan.FromSeconds(periodTime);
            this.textCheckTimer.Tick += CheckTextChanges;
            this.textCheckTimer.Start();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = this.textFileFilter;

            if (openFileDialog.ShowDialog() == true)
            {
                notOpenFileText.Visibility = Visibility.Hidden;
                richTextBox.Visibility = Visibility.Visible;

                this.curFileName = openFileDialog.FileName;
                string fileContent = this.fm.readTextFromFile(openFileDialog.FileName);

                this.tb.clearState();
                this.tb.updateState(fileContent);
                this.tb.startAutoSave(this.curFileName);

                richTextBox.Document.Blocks.Clear();
                richTextBox.AppendText(this.tb.getState());
            }
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            if (this.curFileName == "") {
                return;
            }
            TextRange textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            this.fm.saveTextToFile(this.curFileName, textRange.Text);
        }

        private void onWindowClosed(object sender, EventArgs e)
        {
            this.tb.stopAutoSave();
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            this.tb.Undo();
            updateText();   
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            this.tb.Redo();

            updateText();
        }
        private void CheckTextChanges(object sender, EventArgs e)
        {
            // Получаем текущий текст из RichTextBox
            TextRange textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            string currentText = textRange.Text;
            this.tb.updateState(currentText);
        }


        public void HighlightWordAtIndex(int startIndex, int length)
        {
            // Проверяем, что индекс и длина корректны
            if (startIndex < 0 || length <= 0)
                return;

            // Находим позицию начала текста
            TextPointer startPointer = richTextBox.Document.ContentStart;
            TextPointer startHighlight = GetTextPointerAtOffset(startPointer, startIndex);
            TextPointer endHighlight = GetTextPointerAtOffset(startPointer, startIndex + length);

            if (startHighlight != null && endHighlight != null)
            {
                // Создаем диапазон для выделения
                TextRange wordRange = new TextRange(startHighlight, endHighlight);

                // Устанавливаем цвет фона для выделения
                wordRange.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow);
            }
        }

        private TextPointer GetTextPointerAtOffset(TextPointer startPointer, int offset)
        {
            TextPointer current = startPointer;
            int currentIndex = 0;

            while (current != null)
            {
                if (current.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textInRun = current.GetTextInRun(LogicalDirection.Forward);
                    if (currentIndex + textInRun.Length >= offset)
                        return current.GetPositionAtOffset(offset - currentIndex);

                    currentIndex += textInRun.Length;
                }
                current = current.GetNextContextPosition(LogicalDirection.Forward);
            }
            return null;
        }

        private void ClearHighlights()
        {
            TextRange textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            textRange.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent); // Сброс цвета фона
        }

        private void Find_Click(object sender, RoutedEventArgs e)
        {
            FindWindow findWindow = new FindWindow();
            if (findWindow.ShowDialog() == true)
            {
                string searchText = findWindow.SearchText;

                if (!string.IsNullOrEmpty(searchText))
                {
                    TextRange textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                    string currentText = textRange.Text;
                    int idx = this.tm.SearchText(currentText, searchText);
                    HighlightWordAtIndex(idx, searchText.Length);
                }
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ClearHighlights();
        }

        public void ReplaceText(string findText, string replaceText)
        {
            TextRange textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            string content = textRange.Text;

            // Заменяем текст и перезаписываем содержимое RichTextBox
            string newState = this.tm.ReplaceText(content, findText, replaceText);
            this.tb.updateState(newState);
            updateText();
        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {
            ReplaceWindow replaceWindow = new ReplaceWindow(this);
            replaceWindow.ShowDialog();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            TextRange textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            string content = textRange.Text;

            if (!richTextBox.Selection.IsEmpty)
            {
                TextRange selectionRange = new TextRange(richTextBox.Selection.Start, richTextBox.Selection.End);
                string selectedText = selectionRange.Text;

                int startIdx = content.IndexOf(selectedText);
                int endIdx = startIdx + selectedText.Length;

                string copyText = this.tm.CopyText(content, startIdx, endIdx);

           
                Clipboard.SetText(copyText);
            }
        }

        private void Cut_Click(object sender, RoutedEventArgs e) 
        {
            TextRange textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            string content = textRange.Text;

            if (!richTextBox.Selection.IsEmpty)
            {
                TextRange selectionRange = new TextRange(richTextBox.Selection.Start, richTextBox.Selection.End);
                string selectedText = selectionRange.Text;

                int startIdx = content.IndexOf(selectedText);
                int endIdx = startIdx + selectedText.Length;

                string copyText = this.tm.CopyText(content, startIdx, endIdx);
                string cutText = this.tm.CutText(content, startIdx, endIdx);

                this.tb.updateState(cutText);
                Clipboard.SetText(copyText);

                updateText();
            }
        }

        private void Insert_Click(object sender, RoutedEventArgs e)
        {
            TextRange textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            string content = textRange.Text;

            TextPointer caretPosition = richTextBox.CaretPosition;
            int caretIdx = new TextRange(richTextBox.Document.ContentStart, caretPosition).Text.Length;

            string clipboardText = Clipboard.GetText();
            string newText = this.tm.InsertText(content, clipboardText, caretIdx);

            this.tb.updateState(newText);

            updateText();  
            
        }

        private void updateText()
        {
            // Сохраняем текущую позицию курсора
            TextPointer currentCaretPosition = richTextBox.CaretPosition;

            // Получаем относительный индекс позиции курсора до обновления текста
            TextRange fullTextRangeBefore = new TextRange(richTextBox.Document.ContentStart, currentCaretPosition);
            int caretIndex = fullTextRangeBefore.Text.Length;

            // Очищаем текст и обновляем содержимое
            richTextBox.Document.Blocks.Clear();
            richTextBox.AppendText(this.tb.getState());

            // Восстанавливаем позицию курсора
            TextPointer newCaretPosition = richTextBox.Document.ContentStart;
            newCaretPosition = newCaretPosition.GetPositionAtOffset(caretIndex, LogicalDirection.Forward);

            // Устанавливаем курсор на нужную позицию, если она существует
            if (newCaretPosition != null)
            {
                richTextBox.CaretPosition = newCaretPosition;
            }

            // Прокручиваем RichTextBox, чтобы курсор был видим
            richTextBox.Focus();
        }
    }
   
}

