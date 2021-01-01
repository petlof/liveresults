#region MIT License
/*
 * Copyright © 2008 Jonathan Mark Porter.
 * H2Sharp is a wrapper for the H2 Database Engine. http://h2sharp.googlecode.com
 * 
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */
#endregion

using System;
using System.Collections.Generic;
using org.h2.jdbcx;

namespace System.Data.H2
{
    /// <summary>
    /// A class that holds useful methods for manipulating the database.
    /// </summary>
    public static class H2Database
    {
        /// <summary>
        /// Delete the database files. The database must be closed before calling this tool.
        /// </summary>
        /// <param name="dir">the directory</param>
        /// <param name="db">the database name (null for all databases)</param>
        /// <param name="quiet">don't print progress information</param>
        public static void DeleteDbFiles(String dir, String db, bool quiet)
        {
            org.h2.tools.DeleteDbFiles.execute(dir, db, quiet);
        }
        /// <summary>
        /// Backs up a H2 database by creating a .zip file from the database files.
        /// </summary>
        /// <param name="zipFileName">the name of the target backup file (including path)</param>
        /// <param name="directory">the source directory name</param>
        /// <param name="db">the source database name (null if there is only one database)</param>
        /// <param name="quiet">don't print progress information</param>
        public static void Backup(String zipFileName, String directory, String db, bool quiet)
        {
            org.h2.tools.Backup.execute(zipFileName, directory, db, quiet);
        }
       
        /// <summary>
        /// Changes the password for a database. The passwords must be supplied as char arrays and are cleaned in this method. 
        /// </summary>
        /// <param name="dir">the directory (. for the current directory)</param>
        /// <param name="db">the database name (null for all databases)</param>
        /// <param name="cipher">the cipher (AES, XTEA)</param>
        /// <param name="decryptPassword">the decryption password as a char array</param>
        /// <param name="encryptPassword">the encryption password as a char array</param>
        /// <param name="quiet">don't print progress information</param>
        public static void ChangeFileEncryption(String dir, String db, String cipher, char[] decryptPassword, char[] encryptPassword, bool quiet)
        {
            org.h2.tools.ChangeFileEncryption.execute(dir, db, cipher, decryptPassword, encryptPassword, quiet);
        }
        /// <summary>
        /// Dumps the database. 
        /// </summary>
        /// <param name="dir">the directory</param>
        /// <param name="db">the database name (null for all databases)</param>
        /// <remarks>Dumps the contents of a database file to a human readable text file. This text file can be used to recover most of the data. This tool does not open the database and can be used even if the database files are corrupted. A database can get corrupted if there is a bug in the database engine or file system software, or if an application writes into the database file that doesn't understand the the file format, or if there is a hardware problem.</remarks>
        public static void Recover(String dir, String db)
        {
            org.h2.tools.Recover.execute(dir, db);
        }
       
    }
}