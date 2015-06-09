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
            SELECT
                    D.ID,
                    D.NAME,
                    D.FILENAME,
                    D.status,
                    D.MAIL,
                    D.PHONE,
                    D.DOCUMENTID,
                    D.PINCODE,
                    D.PINCODEENTERCOUNT,
                    D.IDENTIFIER,
                    D.MAILSTATUS,
                    D.SMSSTATUS,
                    D.CREATEDATE,
                    D.DOCUMENTTYPE
                FROM DOCUMENTS D
                WHERE
                D.mailstatus = 1 and D.smsstatus = 1 and not(D.documenttype is null) AND D.STATUS = 1
        ";

        public const string GetDocData = @"
            SELECT FIRST 1
                  D.data,
                  D.additional_data
                FROM DOCUMENTS D
            WHERE D.id = @PNr
        ";

        public const string InsertLog = @"
            INSERT INTO ""LOG"" (NAME, CREATEDATE, ""ACTION"", DOCUMENT_ID, INFO_MESSAGE) VALUES ( @PNAME, @PCREATEDATE, @PACTION, @PDOCUMENT_ID, @PINFO_MESSAGE);";

        public const string SetHandled = @"
            UPDATE DOCUMENTS
            SET STATUS = 2,
                DATA = NULL,
                ADDITIONAL_DATA = NULL
            WHERE (ID = @PNr)
        ";

        public const string DeleteDocument = @"
            EXECUTE BLOCK
                 (PNr integer = @PNR)
                as
                declare tmpID int;
                declare tmpName varchar(256);
                declare createDate timestamp;
                begin
                  SELECT  FIRST 1
                    D.id,
                    D.name,
                    D.createdate
                   FROM DOCUMENTS D
                     WHERE D.id = :PNr
                   into :tmpID, :tmpName, :createDate;

                   if(not(tmpID is null)) then
                   begin
                     INSERT INTO ""LOG"" (NAME, CREATEDATE, ""ACTION"", DOCUMENT_ID, INFO_MESSAGE)
                        VALUES ( :tmpName, :createDate, 3, :tmpID, '');

                     DELETE FROM DOCUMENTS D WHERE D.id = :tmpID;
                   end
                end
        ";

        public const string DeleteHandledDocuments = @"
            EXECUTE BLOCK
            as
            declare tmpID int;
            declare tmpName varchar(256);
            declare createDate timestamp;
            begin
              for
                  SELECT
                    D.id,
                    D.name,
                    D.createdate
                   FROM DOCUMENTS D
                     WHERE D.status = 2
                   into :tmpID, :tmpName, :createDate
               do
               begin
                 INSERT INTO ""LOG"" (NAME, CREATEDATE, ""ACTION"", DOCUMENT_ID, INFO_MESSAGE)
                    VALUES ( :tmpName, :createDate, 3, :tmpID, '');

                 DELETE FROM DOCUMENTS D WHERE D.id = :tmpID;
               end
            end
        ";
    }
}
