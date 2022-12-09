using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSCP;
using System.IO;

namespace FtpClientCs
{
    class Program
    {
        private static bool HelpRequired(string param)
        {
            return param.ToLower() == "-h" || param.ToLower() == "--help" || param.ToLower() == "-help" || param.ToLower() == "help" || param.ToLower() == "/?";
        }
        private static void DisplayHelp()
        {
            int spacechar = 15;
            int tabchar = 3;
            Console.WriteLine("==========================================================");
            Console.WriteLine("DESCRIPTION :".PadRight(spacechar) + "FTP Client Support FTP/SFTP By BaoGa");
            Console.WriteLine("USAGE       :".PadRight(spacechar) + "FtpClientCs.exe PTC=SFTP FMD=F FH=hostexam.com FU=ftpuser FPW=ftppass  FP=22");
            Console.WriteLine("CODE EXIT   :".PadRight(spacechar) + "Close Application With Return Code Exit:");
            Console.WriteLine(" ".PadRight(spacechar + tabchar) + "200 : Success");
            Console.WriteLine(" ".PadRight(spacechar + tabchar) + "401 : FtpHost IsNull"); 
            Console.WriteLine(" ".PadRight(spacechar + tabchar) + "402 : FtpUser IsNull");
            Console.WriteLine(" ".PadRight(spacechar + tabchar) + "403 : FtpPass IsNull");
            Console.WriteLine(" ".PadRight(spacechar + tabchar) + "404 : FtpMethod IsNull");
            Console.WriteLine(" ".PadRight(spacechar + tabchar) + "405 : FtpMethod undefined");
            Console.WriteLine(" ".PadRight(spacechar + tabchar) + "406 : FileSource IsNull");
            Console.WriteLine(" ".PadRight(spacechar + tabchar) + "407 : FileTarget IsNull");
            Console.WriteLine(" ".PadRight(spacechar + tabchar) + "408 : File Not Found On Server With Method GET/MOVE/REMOVE/FILEEXISTS");
            Console.WriteLine(" ".PadRight(spacechar + tabchar) + "501 : Exception Message");
            Console.WriteLine("");
            Console.WriteLine("PTC :".PadRight(spacechar) + "Ftp Protocol Values FTP/SFTP Default FTP");
            Console.WriteLine("FH  :".PadRight(spacechar) + "Ftp Host Required");
            Console.WriteLine("FU  :".PadRight(spacechar) + "Ftp User Login Required");
            Console.WriteLine("FPW :".PadRight(spacechar) + "Ftp PassWord Required");
            Console.WriteLine("FP  :".PadRight(spacechar) + "Ftp Port Default FTP=21/SFTP=22");
            Console.WriteLine("FMD :".PadRight(spacechar) + "Ftp Mode Login With Mode Passive(F) Or Active(T) Default F");
            Console.WriteLine("FMT :".PadRight(spacechar) + "Ftp Method Required :");
            Console.WriteLine(" ".PadRight(spacechar + tabchar) + "PUT        : Upload File");
            Console.WriteLine(" ".PadRight(spacechar + tabchar) + "GET        : Download File");
            Console.WriteLine(" ".PadRight(spacechar + tabchar) + "MOVE       : Move File Or ReName File To Dirctory");
            Console.WriteLine(" ".PadRight(spacechar + tabchar) + "REMOVE     : Delete File");
            Console.WriteLine(" ".PadRight(spacechar + tabchar) + "FILEEXISTS : Check File Exists On Server");
            Console.WriteLine("FRS :".PadRight(spacechar) + "File ReMove Source Is ReMove File Source When Excute Success METHOD PUT/GET Value T/F Defaul F");
            Console.WriteLine("FS  :".PadRight(spacechar) + "File Source Is Path Of File Source");
            Console.WriteLine("FT  :".PadRight(spacechar) + "File Target Is Path Of File Target");
            Console.WriteLine("MS  :".PadRight(spacechar) + "Show Messagebox If Error Value Y/N Default N");

            Console.WriteLine("");
            Console.WriteLine("EXAMPLE :".PadRight(spacechar) + "FMT=PUT FRS=T FS=c:\\Exam.txt FT=/Exam.txt");
            Console.WriteLine(" ".PadRight(spacechar) + "FMT=GET FRS=F FS=/Exam.txt FT=c:\\Exam.txt");
            Console.WriteLine(" ".PadRight(spacechar) + "FMT=MOVE FS=/Exam.txt FT=/DirMove/Exam.txt");
            Console.WriteLine(" ".PadRight(spacechar) + "FMT=REMOVE FS=/Exam.txt");
            Console.WriteLine(" ".PadRight(spacechar) + "FMT=FILEEXISTS FS=/Exam.txt");
            Console.WriteLine("==========================================================");
        }
        static void Main(string[] args)
        {
            string MessShow = "N";
            if (args.Length == 0 || (args.Length == 1 && HelpRequired(args[0])))
            {
                DisplayHelp();
                Environment.Exit(200);
            }
            try
            {
                /*
                [Alias('PTC')][string]$FTPPROTOCOL,
                [Alias('FH')][string]$FTPHOST,
                [Alias('FU')][string]$FTPUSER,
                [Alias('FPW')][string]$FTPPASS,
                [Alias('FP')][int]$FTPPORT,
                [Alias('FMD')][string]$FTPMODE,
                [Alias('FMT')][string]$FTPMETHOD,
                [Alias('FRV')][string]$FILEREMOVE,
                [Alias('FS')][string]$FILESOURCE,
                [Alias('FT')][string]FILETARGET
                // suppose you have command line parameter "Input=InputFile"
                 */
                var param = new CommandLineParametersReader(args);
                //param.CaseSensitive = true;
                var FtpProtocol = param.Get("PTC", "FTP").ToUpper();
                var FtpHost = param.Get("FH");
                var FtpUser = param.Get("FU");
                var FtpPass = param.Get("FPW");
                var FtpPort = param.Get("FP");
                var FtpMode = param.Get("FMD").ToUpper();
                var FtpMethod = param.Get("FMT").ToUpper();
                var FtpRemove = param.Get("FRS", "F").ToUpper();
                var FileSource = param.Get("FS");
                var FileTarget = param.Get("FT");
                MessShow = param.Get("MS", "N");

                if (string.IsNullOrEmpty(FtpProtocol))
                {
                    FtpProtocol = "FTP";
                }
                if (string.IsNullOrEmpty(FtpPort))
                {
                    if (FtpProtocol == "FTP")
                    {
                        FtpPort = "21";
                    }
                    else if (FtpProtocol == "SFTP")
                    {
                        FtpPort = "22";
                    }
                }

                if (string.IsNullOrEmpty(FtpHost))
                {
                    Console.WriteLine("FtpHost IsNull");
                    if (MessShow == "Y")
                    {
                        MessageBox.Show("FtpHost IsNull", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    Environment.Exit(401);
                }
                if (string.IsNullOrEmpty(FtpUser))
                {
                    Console.WriteLine("FtpUser IsNull");
                    if (MessShow == "Y")
                    {
                        MessageBox.Show("FtpUser IsNull", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    Environment.Exit(402);
                }
                if (string.IsNullOrEmpty(FtpPass))
                {
                    Console.WriteLine("FtpPass IsNull");
                    if (MessShow == "Y")
                    {
                        MessageBox.Show("FtpPass IsNull", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    Environment.Exit(403);
                }
                if (string.IsNullOrEmpty(FtpMethod))
                {
                    Console.WriteLine("FtpMethod IsNull");
                    if (MessShow == "Y")
                    {
                        MessageBox.Show("FtpMethod IsNull", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    Environment.Exit(404);
                }
                // Setup session options
                SessionOptions sessionOptions = null;
                if (FtpMode == "T")
                {
                    if (FtpProtocol == "FTP")
                    {
                        sessionOptions = new SessionOptions
                        {
                            Protocol = Protocol.Ftp,
                            HostName = FtpHost,
                            UserName = FtpUser,
                            Password = FtpPass,
                            PortNumber = int.Parse(FtpPort),
                            FtpMode = WinSCP.FtpMode.Active,
                        };
                    }
                    else if (FtpProtocol == "SFTP")
                    {
                        sessionOptions = new SessionOptions
                        {
                            Protocol = Protocol.Sftp,
                            HostName = FtpHost,
                            UserName = FtpUser,
                            Password = FtpPass,
                            PortNumber = int.Parse(FtpPort),
                            GiveUpSecurityAndAcceptAnySshHostKey = true,
                            FtpMode = WinSCP.FtpMode.Active,
                            //SshHostKeyFingerprint = "ssh-rsa 2048 xxxxxxxxxxx...="
                        };
                    }
                }
                else
                {
                    if (FtpProtocol == "FTP")
                    {
                        sessionOptions = new SessionOptions
                        {
                            Protocol = Protocol.Ftp,
                            HostName = FtpHost,
                            UserName = FtpUser,
                            Password = FtpPass,
                            PortNumber = int.Parse(FtpPort),
                        };
                    }
                    else if (FtpProtocol == "SFTP")
                    {
                        sessionOptions = new SessionOptions
                        {
                            Protocol = Protocol.Sftp,
                            HostName = FtpHost,
                            UserName = FtpUser,
                            Password = FtpPass,
                            PortNumber = int.Parse(FtpPort),
                            GiveUpSecurityAndAcceptAnySshHostKey = true,
                            //SshHostKeyFingerprint = "ssh-rsa 2048 xxxxxxxxxxx...="
                        };
                    }
                }
                if (FtpMethod == "PUT")
                {
                    FtpPut(sessionOptions, FileSource, FileTarget, FtpRemove, MessShow);
                }
                else if (FtpMethod == "GET")
                {
                    FtpGet(sessionOptions, FileSource, FileTarget, FtpRemove, MessShow);
                }
                else if (FtpMethod == "FILEEXISTS")
                {
                    FtpFileExists(sessionOptions, FileSource, MessShow);
                }
                else if (FtpMethod == "MOVE")
                {
                    FtpMove(sessionOptions, FileSource, FileTarget, MessShow);
                }
                else if (FtpMethod == "REMOVE")
                {
                    FtpReMove(sessionOptions, FileSource, MessShow);
                }
                else
                {
                    Console.WriteLine("FtpMethod undefined");
                    if (MessShow == "Y")
                    {
                        MessageBox.Show("FtpMethod undefined", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    Environment.Exit(405);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (MessShow == "Y")
                {
                    MessageBox.Show(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                Environment.Exit(501);
            }
            Environment.Exit(200);
        }
        #region Funtions Action
        private static void FtpPut(SessionOptions SessionOpt, string FileSource, string FileTarget, string FtpRemove, string MessShow)
        {
            try
            {
                bool DelFlag = false;
                if (string.IsNullOrEmpty(FileSource))
                {
                    Console.WriteLine("FileSource IsNull");
                    if (MessShow == "Y")
                    {
                        MessageBox.Show("FileSource IsNull", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    Environment.Exit(406);
                }
                /*
                if (!File.Exists(Fromfile))
                {
                    MessageBox.Show("Fromfile Does Not Exists", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Environment.Exit(1);
                }
                */
                if (string.IsNullOrEmpty(FileTarget))
                {
                    Console.WriteLine("FileTarget IsNull");
                    if (MessShow == "Y")
                    {
                        MessageBox.Show("FileTarget IsNull", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    Environment.Exit(407);
                }
                if (FtpRemove == "T")
                {
                    DelFlag = true;
                }
                using (Session session = new Session())
                {
                    // Connect
                    session.Open(SessionOpt);

                    try
                    {
                        // Upload files
                        TransferOptions transferOptions = new TransferOptions();
                        transferOptions.TransferMode = TransferMode.Binary;

                        TransferOperationResult transferResult;
                        transferResult = session.PutFiles(FileSource, FileTarget, DelFlag, transferOptions);

                        // Throw on any error
                        transferResult.Check();

                        // Print results
                        foreach (TransferEventArgs transfer in transferResult.Transfers)
                        {
                            Console.WriteLine("PutFiles Success: " + transfer.FileName);
                            //Environment.Exit(2428);
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        if (MessShow == "Y")
                        {
                            MessageBox.Show(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        Environment.Exit(501);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (MessShow == "Y")
                {
                    MessageBox.Show(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                Environment.Exit(501);
            }
            Environment.Exit(200);
        }
        private static void FtpGet(SessionOptions SessionOpt, string FileSource, string FileTarget, string FtpRemove, string MessShow)
        {
            try
            {
                bool DelFlag = false;
                if (string.IsNullOrEmpty(FileSource))
                {
                    Console.WriteLine("FileSource IsNull");
                    if (MessShow == "Y")
                    {
                        MessageBox.Show("FileSource IsNull", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    Environment.Exit(406);
                }
                if (string.IsNullOrEmpty(FileTarget))
                {
                    Console.WriteLine("FileTarget IsNull");
                    if (MessShow == "Y")
                    {
                        MessageBox.Show("FileTarget IsNull", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    Environment.Exit(407);
                }
                if (FtpRemove == "T")
                {
                    DelFlag = true;
                }
                using (Session session = new Session())
                {
                    // Connect
                    session.Open(SessionOpt);

                    try
                    {
                        //Check FileExists
                        if (session.FileExists(FileSource))
                        {
                            // Download files
                            TransferOptions transferOptions = new TransferOptions();
                            transferOptions.TransferMode = TransferMode.Binary;

                            TransferOperationResult transferResult;
                            transferResult = session.GetFiles(FileSource, FileTarget, DelFlag, transferOptions);

                            // Throw on any error
                            transferResult.Check();

                            // Print results
                            foreach (TransferEventArgs transfer in transferResult.Transfers)
                            {
                                Console.WriteLine("GetFiles Success: " + transfer.FileName);
                                //Environment.Exit(2428);
                            }
                        }
                        else
                        {
                            Console.WriteLine("File Source On Ftp Server Does Not Exists");
                            if (MessShow == "Y")
                            {
                                MessageBox.Show("File Source On Ftp Server Does Not Exists", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            Environment.Exit(408);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        if (MessShow == "Y")
                        {
                            MessageBox.Show(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        Environment.Exit(501);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (MessShow == "Y")
                {
                    MessageBox.Show(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                Environment.Exit(501);
            }
            Environment.Exit(200);
        }
        private static void FtpFileExists(SessionOptions SessionOpt, string FileSource, string MessShow)
        {
            try
            {
                if (string.IsNullOrEmpty(FileSource))
                {
                    Console.WriteLine("FileSource IsNull");
                    if (MessShow == "Y")
                    {
                        MessageBox.Show("FileSource IsNull", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    Environment.Exit(406);
                }

                using (Session session = new Session())
                {
                    // Connect
                    session.Open(SessionOpt);
                    try
                    {
                        //Check FileExists
                        if (session.FileExists(FileSource))
                        {
                            //Environment.Exit(2428);
                            Console.WriteLine("FileExists Success: " + FileSource + " Status : True");
                        }
                        else
                        {
                            Console.WriteLine("File Source On Ftp Server Does Not Exists");
                            if (MessShow == "Y")
                            {
                                MessageBox.Show("File Source On Ftp Server Does Not Exists", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            Environment.Exit(408);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        if (MessShow == "Y")
                        {
                            MessageBox.Show(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        Environment.Exit(501);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (MessShow == "Y")
                {
                    MessageBox.Show(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                Environment.Exit(501);
            }
            Environment.Exit(200);
        }
        private static void FtpMove(SessionOptions SessionOpt, string FileSource, string FileTarget, string MessShow)
        {
            try
            {
                if (string.IsNullOrEmpty(FileSource))
                {
                    Console.WriteLine("FileSource IsNull");
                    if (MessShow == "Y")
                    {
                        MessageBox.Show("FileSource IsNull", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    Environment.Exit(406);
                }
                if (string.IsNullOrEmpty(FileTarget))
                {
                    Console.WriteLine("FileTarget IsNull");
                    if (MessShow == "Y")
                    {
                        MessageBox.Show("FileTarget IsNull", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    Environment.Exit(407);
                }
                
                using (Session session = new Session())
                {
                    // Connect
                    session.Open(SessionOpt);

                    try
                    {
                        // Check FileExists
                        if (session.FileExists(FileSource))
                        {
                            // Move File Or Rename File
                            session.MoveFile(FileSource, FileTarget);
                            Console.WriteLine("MoveFile Success: " + FileSource + " To " + FileTarget);
                        }
                        else
                        {
                            Console.WriteLine("File Source On Ftp Server Does Not Exists");
                            if (MessShow == "Y")
                            {
                                MessageBox.Show("File Source On Ftp Server Does Not Exists", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            Environment.Exit(408);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        if (MessShow == "Y")
                        {
                            MessageBox.Show(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        Environment.Exit(501);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (MessShow == "Y")
                {
                    MessageBox.Show(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                Environment.Exit(501);
            }
            Environment.Exit(200);
        }
        private static void FtpReMove(SessionOptions SessionOpt, string FileSource, string MessShow)
        {
            try
            {
                if (string.IsNullOrEmpty(FileSource))
                {
                    Console.WriteLine("FileSource IsNull");
                    if (MessShow == "Y")
                    {
                        MessageBox.Show("FileSource IsNull", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    Environment.Exit(406);
                }
                using (Session session = new Session())
                {
                    // Connect
                    session.Open(SessionOpt);

                    try
                    {
                        // Check FileExists
                        if (session.FileExists(FileSource))
                        {
                            // ReMoveFile
                            RemovalOperationResult removalResult = null;
                            removalResult = session.RemoveFiles(FileSource);

                            // Throw on any error
                            removalResult.Check();

                            // Print results
                            foreach (RemovalEventArgs removal in removalResult.Removals)
                            {
                                Console.WriteLine("RemoveFiles Success: " + removal.FileName);
                                //Environment.Exit(200);
                            }
                        }
                        else
                        {
                            Console.WriteLine("File Source On Ftp Server Does Not Exists");
                            if (MessShow == "Y")
                            {
                                MessageBox.Show("File Source On Ftp Server Does Not Exists", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            Environment.Exit(408);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        if (MessShow == "Y")
                        {
                            MessageBox.Show(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        Environment.Exit(501);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (MessShow == "Y")
                {
                    MessageBox.Show(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                Environment.Exit(501);
            }
            Environment.Exit(200);
        }
        #endregion
    }
}
