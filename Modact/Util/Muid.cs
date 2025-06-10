using System.Text;

namespace Modact
{
    public class Muid
    {
        //private string? _muid;
        //private readonly string BASE_CHAR = "123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        //public readonly DateTime BaseDate = new DateTime(1970, 1, 1);
        //public DateTime CurrentDate { get; set; } = DateTime.UtcNow;
        //public Guid Guid { get; set; } = Guid.NewGuid();
        //public bool IsSequence { get; set; } = true;

        //public Muid(bool isSequence = true)
        //{
        //    IsSequence = isSequence;
        //    RenewMuid();
        //}

        //public Muid(DateTime currentDate, bool isSequence = true)
        //{
        //    IsSequence = isSequence;
        //    CurrentDate = currentDate;
        //    RenewMuid();
        //}

        //public void RenewMuid()
        //{
        //    if (IsSequence)
        //    {
        //        RenewSequenceMuid();
        //    }
        //    else
        //    {
        //        RenewNonSequenceMuid();
        //    }
        //}

        //private void RenewSequenceMuid()
        //{
        //    TimeSpan allsecs = new TimeSpan(CurrentDate.Ticks - BaseDate.Ticks); //Timespan from basedate to now

        //    Guid guidOrigin = Guid; //A random new guid
        //    byte[] guidArray = guidOrigin.ToByteArray(); //Convert guid to bytes
        //    byte[] allsecsArray = BitConverter.GetBytes((long)allsecs.TotalMilliseconds); //Convert timespan milliseconds to bytes, for Intel/AMD CPU
        //    Array.Copy(allsecsArray, 0, guidArray, 10, 6); //Replace timespan into guid last 6 bytes
        //    Array.Reverse(guidArray); //Reverse bytes array let timespan bytes be first & correct its order

        //    _muid = GuidArrayToMuid(guidArray);
        //}

        //private void RenewNonSequenceMuid()
        //{
        //    Guid guidOrigin = Guid; //A random new guid
        //    byte[] guidArray = guidOrigin.ToByteArray(); //Convert guid to bytes

        //    _muid = GuidArrayToMuid(guidArray);
        //}

        //private string GuidArrayToMuid(byte[] guidArray)
        //{
        //    StringBuilder strOutput = new StringBuilder(26);
        //    int divideByteLength = 7; //Per 7 bytes parse to long int for calculate
        //    for (int i = 0; i < guidArray.Length; i += divideByteLength)
        //    {
        //        int copyLenth = divideByteLength;
        //        byte[] calByte = new byte[divideByteLength];
        //        byte[] longByte = new byte[8];
        //        if (i + divideByteLength > guidArray.Length) //Detect to copy the last bytes do not over the byte array length
        //        {
        //            copyLenth = guidArray.Length - i;
        //        }
        //        Array.Copy(guidArray, i, calByte, 0, copyLenth); //Copy the calculating bytes from GUID
        //        if (i + divideByteLength <= guidArray.Length) // Reverse bytes for Intel/AMD CPU
        //        {
        //            Array.Reverse(calByte);
        //        }
        //        Array.Copy(calByte, 0, longByte, 0, calByte.Length); //Copy to a 8 byte array for parse to long
        //        long cal = BitConverter.ToInt64(longByte, 0);

        //        StringBuilder a = new StringBuilder();
        //        while (cal > 0)
        //        {
        //            a.Append(BASE_CHAR[(int)(cal % BASE_CHAR.Length)]); //Math mod to get char
        //            cal /= BASE_CHAR.Length;
        //        }
        //        char[] charByte = a.ToString().ToCharArray();
        //        Array.Reverse(charByte); //Reverse order to get correct array
        //        string strByteOutput = new string(charByte);
        //        if (i + divideByteLength <= guidArray.Length) //Pad 0 if not full array
        //        {
        //            strByteOutput = strByteOutput.PadLeft(11, '0');
        //        }
        //        else
        //        {
        //            strByteOutput = strByteOutput.PadLeft(4, '0');
        //        }
        //        strOutput.Append(strByteOutput);
        //    }

        //    return strOutput.ToString();
        //}

        //public static string NewMuid(bool isSequence = true)
        //{
        //    return (new Muid(isSequence).ToString());
        //}

        //public static string NewMuid(DateTime currentDate)
        //{
        //    return (new Muid(currentDate, true).ToString());
        //}

        //public static List<string> NewMuid(int counts, bool isSequence = true)
        //{
        //    List<string> NewMuids = new List<string>();
        //    for (int i = 0; i < counts; i++)
        //    {
        //        NewMuids.Add(Muid.NewMuid(isSequence));
        //    }
        //    if (isSequence)
        //    {
        //        NewMuids.Sort();
        //    }
        //    return NewMuids;
        //}

        //public override string ToString()
        //{
        //    return _muid;
        //}
    }
}