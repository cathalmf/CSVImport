using CsvHelper;
using CSVImporter.Models;
using CSVImporter.MSSQLDatabaseConnection;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace CSVImporter.ViewModels
{
    internal class Presenter : ObservableObject
    {
        #region Properties & Fields

        private readonly BackgroundWorker Worker;

        private ObservableCollection<string> logList = new ObservableCollection<string>();
        public IEnumerable<string> LogList
        {
            get
            {
                return logList;
            }
        }

        private int filePercentage;
        public int FilePercentage
        {
            get
            {
                return filePercentage;
            }
            set
            {
                filePercentage = value;
                RaisePropertyChangedEvent("FilePercentage");
            }
        }

        private string importFilePath;
        public string ImportFilePath
        {
            get
            {
                return importFilePath;
            }
            set
            {
                importFilePath = value;
                RaisePropertyChangedEvent("ImportFilePath");
            }
        }

        private bool enableImport;
        public bool EnableImport
        {
            get
            {
                return enableImport;
            }
            set
            {
                enableImport = value;
                RaisePropertyChangedEvent("EnableImport");
            }
        }

        private int ValidLines;
        private int InvalidLines;
        private StringBuilder InvalidLineDetails;

        #endregion

        #region ctor

        public Presenter()
        {            
            Worker = new BackgroundWorker();
            Worker.WorkerReportsProgress = true;
            Worker.DoWork += Worker_DoWork;
            Worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            InvalidLineDetails = new StringBuilder();
        }

        #endregion

        #region Event Methods

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Log("Complete");
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            FilePercentage = 0;

            string TempFileName = Path.GetTempFileName();

            using(StreamWriter ValidatedSW = new StreamWriter(TempFileName))
            using (StreamReader OriginalSR = new StreamReader(ImportFilePath))
            using (CsvReader CsvReaderStream = new CsvReader(OriginalSR))
            {
                Log("Validating CSV");

                string[] fields = CsvReaderStream.Parser.Read();

                do
                {
                    bool ValidLine = ValidateCSV.Validate(fields);

                    if (ValidLine)
                    {
                        ValidatedSW.Write(CsvReaderStream.Parser.RawRecord);
                        ValidLines++;
                    }
                    else
                    {
                        InvalidLines++;
                        InvalidLineDetails.Append(CsvReaderStream.Parser.RawRecord);
                    }

                    FilePercentage = (int)(((decimal)OriginalSR.BaseStream.Position / OriginalSR.BaseStream.Length) * 100);

                    fields = CsvReaderStream.Parser.Read();

                } while (fields != null);

                Log("Completed Validation");
            }

            if (ValidLines > 0)
            {
                FilePercentage = 0;
                Log("Inserting Into Database. This may take a few minutes.");

                DatabaseConnection.SqlBulkInsert(TempFileName);

                FilePercentage = 100;
                Log("Database Insert Complete");
                File.Delete(TempFileName);
            }
            else
            {
                Log("No valid lines to import");
            }

            Log(string.Format("Invalid Lines: {0}", InvalidLines));
            Log(InvalidLineDetails.ToString());
        }

        #endregion

        #region Commands

        public ICommand ProcessFileCmd
        {
            get 
            { 
                return new Command(ProcessFile); 
            }
        }

        private void ProcessFile()
        {
            logList.Clear();
            Worker.RunWorkerAsync();
        }

        public ICommand BrowseFileCmd
        {
            get
            {
                return new Command(GetImportPath);
            }
        }

        private void GetImportPath()
        {
            var OpenFileDlg = new OpenFileDialog();
            OpenFileDlg.Filter = "CSV File | *.csv";
            var DlgResult = OpenFileDlg.ShowDialog();
            if (DlgResult.GetValueOrDefault())
            {
                ImportFilePath = OpenFileDlg.FileName;
                EnableImport = true;
            }
        }

        #endregion

        #region Log

        private void Log(string line)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                logList.Add(line);
            });
        }

        #endregion
    }
}
