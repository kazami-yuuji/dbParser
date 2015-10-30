using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Npgsql;

namespace dbParser
{
    public partial class MainForm : Form
    {
        private struct RecordBounds
        {
            public readonly int Begin;
            public readonly int End;

            public RecordBounds(int begin, int end)
            {
                Begin = begin;
                End = end;
            }
        }

        private const int RecordsMax = 3800377;
        private const int ThreadsNumber = 5;
        private int _threadsCounter; 
        private int _count;
        private bool _close;

        private readonly DbConnection _dbConnection;

        public MainForm()
        {
            InitializeComponent();
            RecordsProgressBar.Maximum = RecordsMax;
            _count = 0;
            _lockerEnd = new object();
            _close = false;
            _threadsCounter = 0;
            
            _lockerDb = new object();

            DbConnectionStringBuilder connectionStringBuilder = new NpgsqlConnectionStringBuilder();
            connectionStringBuilder.Add("Server", "localhost");
            connectionStringBuilder.Add("Port", "5432");
            connectionStringBuilder.Add("User Id", "postgres");
            connectionStringBuilder.Add("Password", "1");
            connectionStringBuilder.Add("Database", "project");
            _dbConnection = new NpgsqlConnection(connectionStringBuilder.ToString());
            _dbConnection.Open();
        }

        private void StartButton_Click(object sender, EventArgs eargs)
        {
            const int part = RecordsMax/ThreadsNumber;
            var begin = 1;
            var end = part;
            for (var i = 0; i < ThreadsNumber; i++)
            {
                var b = begin;
                var e = end;
                ThreadPool.QueueUserWorkItem(Parse, new RecordBounds(b, e));
                begin += part;
                end += part;
            }
            ThreadPool.QueueUserWorkItem(MeasureTime);
        }
        
        private readonly object _lockerEnd;

        private readonly object _lockerDb;

        private void MeasureTime(object stateInfo)
        {
            Interlocked.Increment(ref _threadsCounter);
            while (!_close)
            {
                var previousValue = _count;
                Thread.CurrentThread.Join(2000);
                var currentValue = _count;
                if (SpeedMeasureLabel.InvokeRequired)
                {
                    SpeedMeasureLabel.Invoke((MethodInvoker) delegate
                    {
                        SpeedMeasureLabel.Text = $"{((currentValue - previousValue) / (float)2) * 60} records per minute";
                    });
                }
            }
            Interlocked.Decrement(ref _threadsCounter);
        }

        private void Parse(object bounds)
        {
            if (!(bounds is RecordBounds))
                throw new ApplicationException();
            Interlocked.Increment(ref _threadsCounter);
            var recordBounds = (RecordBounds)bounds;
            var i = recordBounds.Begin;
            while (i < recordBounds.End && !_close)
            {
                try
                {
                    var xml = new StreamReader(
                        // ReSharper disable once AssignNullToNotNullAttribute
                        WebRequest.Create($"http://ieeexplore.ieee.org/gateway/ipsSearch.jsp?an={i}")
                            .GetResponse()
                            .GetResponseStream()).ReadToEnd();
                    var xmlReader = XmlReader.Create(new StringReader(xml));
                    
                    if (!xmlReader.ReadToFollowing("Error"))
                    {
                        xmlReader.Close();

                        Interlocked.Increment(ref _count);

                        string title = string.Empty,
                            authors = string.Empty,
                            pubtitle = string.Empty,
                            pubtype = string.Empty,
                            publisher = string.Empty,
                            description = string.Empty,
                            mdurl = string.Empty,
                            pdf = string.Empty;

                        List<string> controlledTerms = new List<string>(),
                            uncontrolledTerms = new List<string>(),
                            thesaurusTerms = new List<string>();

                        var xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(xml);

                        var node = xmlDoc.SelectSingleNode("//root/document/title");
                        if (node != null)
                            title = node.InnerText.Trim();

                        node = xmlDoc.SelectSingleNode("//root/document/authors");
                        if (node != null)
                            authors = node.InnerText.Trim();

                        node = xmlDoc.SelectSingleNode("//root/document/pubtitle");
                        if (node != null)
                            pubtitle = node.InnerText.Trim();

                        node = xmlDoc.SelectSingleNode("//root/document/pubtype");
                        if (node != null)
                            pubtype = node.InnerText.Trim();

                        node = xmlDoc.SelectSingleNode("//root/document/publisher");
                        if (node != null)
                            publisher = node.InnerText.Trim();

                        node = xmlDoc.SelectSingleNode("//root/document/abstract");
                        if (node != null)
                            description = node.InnerText.Trim();

                        node = xmlDoc.SelectSingleNode("//root/document/mdurl");
                        if (node != null)
                            mdurl = node.InnerText.Trim();

                        node = xmlDoc.SelectSingleNode("//root/document/pdf");
                        if (node != null)
                            pdf = node.InnerText.Trim();

                        var xmlNodesList = xmlDoc.SelectNodes("//root/document/controlledterms");
                        if (xmlNodesList != null)
                        {
                            controlledTerms.AddRange(from XmlNode xmlNode in xmlNodesList
                                select xmlNode.ChildNodes
                                into childNodes
                                from XmlNode childNode in childNodes
                                select childNode.InnerText.Trim());
                        }

                        xmlNodesList = xmlDoc.SelectNodes("//root/document/uncontrolledterms");
                        if (xmlNodesList != null)
                        {
                            uncontrolledTerms.AddRange(from XmlNode xmlNode in xmlNodesList
                                select xmlNode.ChildNodes
                                into childNodes
                                from XmlNode childNode in childNodes
                                select childNode.InnerText.Trim());
                        }

                        xmlNodesList = xmlDoc.SelectNodes("//root/document/thesaurusterms");
                        if (xmlNodesList != null)
                        {
                            thesaurusTerms.AddRange(from XmlNode xmlNode in xmlNodesList
                                                       select xmlNode.ChildNodes
                                into childNodes
                                                       from XmlNode childNode in childNodes
                                                       select childNode.InnerText.Trim());
                        }

                        var sqlCommand = _dbConnection.CreateCommand();
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.CommandText = "insert_article";

                        var titleParameter = sqlCommand.CreateParameter();
                        titleParameter.DbType = DbType.String;
                        titleParameter.Value = title;
                        sqlCommand.Parameters.Add(titleParameter);

                        var authorsParameter = sqlCommand.CreateParameter();
                        authorsParameter.DbType = DbType.Object;
                        authorsParameter.Value = (from element in authors.Trim().Split(';') select element.Trim()).ToArray();
                        sqlCommand.Parameters.Add(authorsParameter);

                        var pubtitleParameter = sqlCommand.CreateParameter();
                        pubtitleParameter.DbType = DbType.String;
                        pubtitleParameter.Value = pubtitle;
                        sqlCommand.Parameters.Add(pubtitleParameter);

                        var pubtypeParameter = sqlCommand.CreateParameter();
                        pubtypeParameter.DbType = DbType.String;
                        pubtypeParameter.Value = pubtype;
                        sqlCommand.Parameters.Add(pubtypeParameter);

                        var publisherParameter = sqlCommand.CreateParameter();
                        publisherParameter.DbType = DbType.String;
                        publisherParameter.Value = publisher;
                        sqlCommand.Parameters.Add(publisherParameter);

                        var descriptionParameter = sqlCommand.CreateParameter();
                        descriptionParameter.DbType = DbType.String;
                        descriptionParameter.Value = description;
                        sqlCommand.Parameters.Add(descriptionParameter);

                        var mdurlParameter = sqlCommand.CreateParameter();
                        mdurlParameter.DbType = DbType.String;
                        mdurlParameter.Value = mdurl;
                        sqlCommand.Parameters.Add(mdurlParameter);

                        var pdfParameter = sqlCommand.CreateParameter();
                        pdfParameter.DbType = DbType.String;
                        pdfParameter.Value = pdf;
                        sqlCommand.Parameters.Add(pdfParameter);

                        var controlledtermsParameter = sqlCommand.CreateParameter();
                        controlledtermsParameter.DbType = DbType.Object;
                        controlledtermsParameter.Value = controlledTerms.ToArray();
                        sqlCommand.Parameters.Add(controlledtermsParameter);

                        var uncontrolledtermsParameter = sqlCommand.CreateParameter();
                        uncontrolledtermsParameter.DbType = DbType.Object;
                        uncontrolledtermsParameter.Value = uncontrolledTerms.ToArray();
                        sqlCommand.Parameters.Add(uncontrolledtermsParameter);

                        var thesaurustermsParameter = sqlCommand.CreateParameter();
                        thesaurustermsParameter.DbType = DbType.Object;
                        thesaurustermsParameter.Value = thesaurusTerms.ToArray();
                        sqlCommand.Parameters.Add(thesaurustermsParameter);

                        lock (_lockerDb)
                        {
                            sqlCommand.ExecuteNonQuery();
                        }

                        RecordsLabel.Invoke((MethodInvoker)delegate
                        {
                            RecordsLabel.Text = $"{_count}";
                        });
                        RecordsProgressBar.Invoke((MethodInvoker)delegate
                        {
                            RecordsProgressBar.PerformStep();
                        });
                    }
                    i++;
                }
                catch (WebException ex)
                {
                    DialogResult result;
                    lock (_lockerEnd)
                    {
                        result =
                            MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                    }
                    switch (result)
                    {
                        case DialogResult.Cancel:
                            return;
                        case DialogResult.Retry:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            Interlocked.Decrement(ref _threadsCounter);
        }

        private void ThreadEndWaiting(object stateInfo)
        {
            while (_threadsCounter != 0)
            {

            }
            Invoke((MethodInvoker) Close);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _close = true;
            if (_threadsCounter != 0)
            {
                ThreadPool.QueueUserWorkItem(ThreadEndWaiting);
                e.Cancel = true;
            } else if (_dbConnection.State == ConnectionState.Open)
                _dbConnection.Close();
        }
    }
}
