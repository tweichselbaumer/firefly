using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
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
            _ConnectionInfo = new ConnectionInfo(host, user, new PasswordAuthenticationMethod(user, pwd));
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

        internal string GetAllText(string filename)
        {
            try
            {
                using (SftpClient client = new SftpClient(_ConnectionInfo))
                {
                    client.Connect();
                    return client.ReadAllText(filename);
                }
            }
            catch (Exception ex)
            {

            }
            return "";
        }
    }
}
