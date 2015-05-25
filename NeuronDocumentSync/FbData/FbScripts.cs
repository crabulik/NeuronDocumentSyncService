using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuronDocumentSync.FbData
{
    internal class FbScripts
    {
        public const string GetUnhandledDocs = @"
            SELECT *
                FROM DOCUMENTS D
                WHERE
                D.mailstatus = 1 and D.smsstatus = 1 and not(D.documenttype is null)
        ";
    }
}
