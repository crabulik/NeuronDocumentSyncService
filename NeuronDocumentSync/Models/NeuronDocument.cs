using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NeuronDocumentSync.Enums;
using NeuronDocumentSync.Resources;

namespace NeuronDocumentSync.Models
{
    public class NeuronDocument
    {
        public int ID { get; set; }

        public string Name { get; set; }
        [Required(ErrorMessageResourceType = typeof (MainMessages), ErrorMessageResourceName = "rs_FileNameMustBeFilled")]
        public string FileName { get; set; }

        [RegularExpression(@"^(?("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))$",
            ErrorMessageResourceType = typeof(MainMessages), ErrorMessageResourceName = "rs_EmailIsNotValid")]
        public string DeliveryEMail { get; set; }

        [RegularExpression(@"^\s*(?:\+?(\d{1,3}))?[-. (]*(\d{3})[-. )]*(\d{3})[-. ]*(\d{4})(?: *x(\d+))?\s*$", ErrorMessageResourceType = typeof (MainMessages), ErrorMessageResourceName = "rs_PhoneNumberIsNotValid")]
        public string DeliveryPhone { get; set; }

        public DateTime CreatDate { get; set; }
        
        public ExportDocumentsType DocumentType { get; set; }

        public byte[] DocumentData { get; set; }

        public byte[] DocumentAdditionalData { get; set; }

        public string Errors { get; set; }
    }
}