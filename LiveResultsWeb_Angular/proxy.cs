using System;
using System.Configuration;
using System.Net;
using System.Web;

namespace AngularWeb
{
    public class proxy : IHttpHandler
    {
        /// <summary>
        /// You will need to configure this handler in the Web.config file of your 
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpHandler Members

        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var reqUrl = context.Request.Url.PathAndQuery;
            var req = WebRequest.Create(ConfigurationManager.AppSettings["proxyUrl"] + reqUrl);

            using (var resp = req.GetResponse())
            {
                context.Response.ContentType = resp.ContentType;

                var respStream = resp.GetResponseStream();
                int read = 0;
                int toRead = (int)resp.ContentLength;
                var buf = new byte[8192];
                while (read < toRead)
                {
                    int nr = respStream.Read(buf, 0, buf.Length);
                    context.Response.OutputStream.Write(buf, 0, nr);
                    read += nr;
                }
            }

        }

        #endregion
    }
}
