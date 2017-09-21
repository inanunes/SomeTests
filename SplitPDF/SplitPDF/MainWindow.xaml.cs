using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Forms;

namespace SplitPDF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }



        private static void SplitPDF(string basePDFPath, string outputPath, bool isMonthly)
        {
            PdfReader reader = new PdfReader(basePDFPath);

            FileInfo file = new FileInfo(basePDFPath);
            string pdfFileName = file.Name.Substring(0, file.Name.LastIndexOf(".")) + " - ";

            int pagesOnSplited = isMonthly ? 2 : 1;

            for (int pageNumber = 1; pageNumber <= reader.NumberOfPages; pageNumber += pagesOnSplited)
            {
                string employeeName = ExtractEmployeeNameFromPDFPage(basePDFPath, pageNumber);
                string newPdfFileName = string.Format(pdfFileName + "{0}", employeeName);

                CreateAndSaveEmployeePDF(basePDFPath, outputPath, pageNumber, isMonthly, newPdfFileName);
            }
        }

        private static string ExtractEmployeeNameFromPDFPage(string path, int pageNumber)
        {
            string pdfTxt = GetTextFromPDFPage(path, pageNumber);
            string name = GetEmployeeNameOnText(pdfTxt);
            return name;
        }

        private static string GetTextFromPDFPage(string path, int pageNumber)
        {
            string pdfTxt;

            using (PdfReader reader = new PdfReader(path))
            {
                pdfTxt = PdfTextExtractor.GetTextFromPage(reader, pageNumber);
            }


            return pdfTxt;
        }

        private static string GetEmployeeNameOnText(string pdfTxt)
        {
            string baseStringLeft = "Funcionário:";
            string baseStringRight = "Matrícula:";
            int Start, End;

            if (pdfTxt.Contains(baseStringLeft) && pdfTxt.Contains(baseStringRight))
            {
                Start = pdfTxt.IndexOf(baseStringLeft, 0) + baseStringLeft.Length;
                End = pdfTxt.IndexOf(baseStringRight, Start);
                return pdfTxt.Substring(Start, End - Start).Trim();
            }
            else
            {
                return "";
            }
        }

        private static void CreateAndSaveEmployeePDF(string pdfFilePath, string outputPath, int startPage, bool isMonthly, string pdfFileName)
        {
            using (PdfReader reader = new PdfReader(pdfFilePath))
            {
                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                }

                Document document = new Document();
                PdfCopy copy = new PdfCopy(document, new FileStream(outputPath + "\\" + pdfFileName + ".pdf", FileMode.Create));
                document.Open();

                copy.AddPage(copy.GetImportedPage(reader, startPage));

                if (isMonthly)
                {
                    copy.AddPage(copy.GetImportedPage(reader, startPage + 1));
                }

                document.Close();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            // Create OpenFileDialog 
           OpenFileDialog dlg = new OpenFileDialog();
            
            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".pdf";
            dlg.Filter = "PDF Files (*.pdf)|*.pdf";


            // Display OpenFileDialog by calling ShowDialog method 
            DialogResult result = dlg.ShowDialog();
            string filename = dlg.FileName;

            // Get the selected file name and display in a TextBox 
            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(filename))
            {
                txtBasePdf.Text = filename;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    txtOutputPath.Text = fbd.SelectedPath;
                }
            }

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                SplitPDF(txtBasePdf.Text, txtOutputPath.Text, false);
                elpReport.Fill = new SolidColorBrush(Colors.Green);
            }
            catch (Exception ex)
            {
                elpReport.Fill = new SolidColorBrush(Colors.Red);
            }
          
        }
    }
}
