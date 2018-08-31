using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireFly.Data.Storage
{
    public class RemoteDataStore
    {
        ConnectionInfo _ConnectionInfo;
        public RemoteDataStore(string host, string user, string pwd)
        {
            if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pwd))
            {
                _ConnectionInfo = new ConnectionInfo(host, user, new PasswordAuthenticationMethod(user, pwd));
                _ConnectionInfo.Timeout = new TimeSpan(0, 0, 1);
            }
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

        internal void DownloadFile(string filename, string tempFileName, Action<double> downloadAction)
        {
            using (FileStream stream = File.Open(tempFileName, FileMode.OpenOrCreate))
            {
                try
                {
                    using (SftpClient client = new SftpClient(_ConnectionInfo))
                    {
                        client.Connect();
                        SftpFileAttributes attributes = client.GetAttributes(filename);
                        client.DownloadFile(filename, stream, delegate (ulong i)
                        {
                            downloadAction((double)i / (double)attributes.Size);
                        });
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}
