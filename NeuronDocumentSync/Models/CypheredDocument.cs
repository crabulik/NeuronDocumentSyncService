namespace NeuronDocumentSync.Models
{
    public class CypheredDocument
    {
        public string CypheredData { get; set; }

        public CypheredDocument(string cypheredData)
        {
            CypheredData = cypheredData;
        }
    }
}