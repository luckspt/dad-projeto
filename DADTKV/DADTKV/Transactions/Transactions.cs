namespace DADTKV.Transactions
{
    public class Transactions
    {
        public List<DadInt> TxSubmit(string clientId, List<string> toRead, List<DadInt> toWrite)
        {
            try
            {
                // Return the list of values read
                return new List<DadInt>();
            }
            catch
            {
                return new List<DadInt>() { DadInt.CreateAborted() };
            }
        }

        public bool Status()
        {
            return true;
        }
    }
}