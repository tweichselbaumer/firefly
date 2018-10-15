using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FireFly.Data.Storage
{
    public class RemoteDataStore
    {
        private ConnectionInfo _ConnectionInfo;

        public RemoteDataStore(string host, string user, string pwd)
        {
            if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pwd))
            {
                _ConnectionInfo = new ConnectionInfo(host, user, new PasswordAuthenticationMethod(user, pwd));
                _ConnectionInfo.Timeout = new TimeSpan(0, 0, 0, 0, 500);
            }
        }

        public void DownloadFile(string remoteFileName, string localFileName, Action<double> downloadAction)
        {
            using (FileStream stream = File.Open(localFileName, FileMode.CreateNew))
            {
                try
                {
                    using (SftpClient client = new SftpClient(_ConnectionInfo))
                    {
                        client.Connect();
                        SftpFileAttributes attributes = client.GetAttributes(remoteFileName);
                        client.DownloadFile(remoteFileName, stream, delegate (ulong i)
                        {
                            downloadAction?.Invoke(i / (double)attributes.Size);
                        });
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public string ExecuteCommands(List<string> commands, string expactString)
        {
            string result = "";

            try
            {
                using (SshClient sshclient = new SshClient(_ConnectionInfo))
                {
                    sshclient.Connect();

                    using (ShellStream stream = sshclient.CreateShellStream("session", 512, 96, 800, 600, 8191))
                    {
                        stream.ErrorOccurred += (e, s) =>
                        {
                            Debug.WriteLine(s.Exception);
                        };

                        result += stream.Expect(new Regex(expactString));

                        foreach (string command in commands)
                        {
                            stream.Write(string.Format("{0}\n", command));
                            result += stream.Expect(new Regex(expactString));
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return result;
        }

        public List<string> GetAllFileNames(string path)
        {
            List<string> result = new List<string>();
            try
            {
                using (SftpClient client = new SftpClient(_ConnectionInfo))
                {
                    client.Connect();
                    result.AddRange(client.ListDirectory(path).Where(c => c.IsRegularFile).Select(d => d.Name));
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        public void UploadContentToFile(string remoteFileName, string content)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(content);
                    writer.Flush();
                    stream.Position = 0;
                    try
                    {
                        using (SftpClient client = new SftpClient(_ConnectionInfo))
                        {
                            client.Connect();
                            client.UploadFile(stream, remoteFileName);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public void UploadFile(string remoteFileName, string localFileName, Action<double> downloadAction = null)
        {
            using (FileStream stream = File.Open(localFileName, FileMode.Open))
            {
                try
                {
                    using (SftpClient client = new SftpClient(_ConnectionInfo))
                    {
                        client.Connect();
                        long length = new FileInfo(localFileName).Length;
                        client.UploadFile(stream, remoteFileName, delegate (ulong i)
                        {
                            downloadAction?.Invoke(i / (double)length);
                        });
                    }
                }
                catch (Exception)
                {
                }
            }
        }
    }
}