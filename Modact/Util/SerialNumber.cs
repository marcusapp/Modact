using Modact.Data.Models;
using Modact.Data.DAL;
using Dapper;
using Microsoft.IdentityModel.Tokens;
using System.Data;

namespace Modact
{
    public class SerialNumber
    {
        private readonly DbHelper _dbHelper;
        private readonly string _snId;

        public SerialNumber(DbHelper dbHelper, string snId) 
        {
            _dbHelper = dbHelper;
            _snId = snId;
        }

        public string PickNextNumber(string? datetimeValue)
        {
            return GetNextNumber(datetimeValue, false);
        }

        public string ViewNextNumber(string? datetimeValue)
        {
            return GetNextNumber(datetimeValue, true);
        }

        string GetNextNumber(string? datetimeValue, bool isViewOnly)
        {
            var snList = (new Dao<DTO_modm_serialnumber>(_dbHelper)).GetList(new { sn_id = _snId, is_void = false }).ToList();

            if (snList.Count > 0)
            {
                DTO_modm_serialnumber snConfig = snList[0];

                string prefix = varFix(snConfig.sn_prefix ?? string.Empty);
                string postfix = varFix(snConfig.sn_postfix ?? string.Empty);

                string resetByDatetime = snConfig.reset_by_datetime ?? string.Empty;
                var currentTime = DateTime.Now;
                if (string.IsNullOrEmpty(datetimeValue))
                {
                    switch (snConfig.reset_by_datetime.ToUpper())
                    {
                        case "Y":
                            datetimeValue = currentTime.ToString("yyyy");
                            break;
                        case "M":
                            datetimeValue = currentTime.ToString("yyyyMM");
                            break;
                        case "D":
                            datetimeValue = currentTime.ToString("yyyyMMdd");
                            break;
                        case "H":
                            datetimeValue = currentTime.ToString("yyyyMMddHH");
                            break;
                        case "I":
                            datetimeValue = currentTime.ToString("yyyyMMddHHmm");
                            break;
                        case "S":
                            datetimeValue = currentTime.ToString("yyyyMMddHHmmss");
                            break;
                    }
                }

                var parameters = new DynamicParameters();
                parameters.Add("@sn_next", dbType: DbType.Decimal, direction: ParameterDirection.Output);
                parameters.Add("@sn_id", _snId, DbType.String, ParameterDirection.Input);
                parameters.Add("@datetime_value", datetimeValue, DbType.String, ParameterDirection.Input);
                parameters.Add("@is_view_only", isViewOnly, DbType.Boolean, ParameterDirection.Input);


                _dbHelper.Connection().Execute("sp_sn_next_number", parameters, _dbHelper.Transaction(), commandType: CommandType.StoredProcedure);
                decimal number = parameters.Get<decimal>("@sn_next");
                string nextNumber = string.Empty;
                if (string.IsNullOrEmpty(snConfig.padding_char))
                {
                    nextNumber = number.ToString();
                }
                else
                {
                    nextNumber = FormatNumber(number, snConfig.padding_digit ?? 0, snConfig.padding_char[0]);
                }

                return prefix + nextNumber + postfix;
            }

            throw new Exception("SN ID not found or inactive: " + _snId);
        }

        string varFix(string raw)
        {
            DateTime currentTime = DateTime.Now;
            //year
            raw = raw.Replace("{YYYY}", currentTime.ToString("yyyy"));
            raw = raw.Replace("{YYY}", currentTime.ToString("yyy"));
            raw = raw.Replace("{YY}", currentTime.ToString("yy"));
            raw = raw.Replace("{yyyy}", currentTime.ToString("yyyy"));
            raw = raw.Replace("{yyy}", currentTime.ToString("yyy"));
            raw = raw.Replace("{yy}", currentTime.ToString("yy"));

            //month
            raw = raw.Replace("{MMMM}", currentTime.ToString("MMMM"));
            raw = raw.Replace("{MMM}", currentTime.ToString("MMM"));
            raw = raw.Replace("{MM}", currentTime.ToString("MM"));
            raw = raw.Replace("{mmmm}", currentTime.ToString("MMMM"));
            raw = raw.Replace("{mmm}", currentTime.ToString("MMM"));
            raw = raw.Replace("{mm}", currentTime.ToString("MM"));

            //day
            raw = raw.Replace("{DD}", currentTime.ToString("dd"));
            raw = raw.Replace("{dd}", currentTime.ToString("dd"));

            //weekday
            raw = raw.Replace("{WD}", currentTime.ToString("ddd"));
            raw = raw.Replace("{WWD}", currentTime.ToString("dddd"));
            raw = raw.Replace("{wd}", currentTime.ToString("ddd"));
            raw = raw.Replace("{wwd}", currentTime.ToString("dddd"));

            //hour
            raw = raw.Replace("{HH}", currentTime.ToString("HH"));
            raw = raw.Replace("{hh}", currentTime.ToString("HH"));
            raw = raw.Replace("{II}", currentTime.ToString("hh"));
            raw = raw.Replace("{ii}", currentTime.ToString("hh"));
            raw = raw.Replace("{TT}", currentTime.ToString("tt"));
            raw = raw.Replace("{tt}", currentTime.ToString("tt"));

            //minute
            raw = raw.Replace("{MI}", currentTime.ToString("mm"));
            raw = raw.Replace("{mi}", currentTime.ToString("mm"));

            //second
            raw = raw.Replace("{SS}", currentTime.ToString("ss"));
            raw = raw.Replace("{ss}", currentTime.ToString("ss"));

            //millisecond
            raw = raw.Replace("{FF}", currentTime.ToString("ff"));
            raw = raw.Replace("{ff}", currentTime.ToString("ff"));
            raw = raw.Replace("{FFF}", currentTime.ToString("fff"));
            raw = raw.Replace("{fff}", currentTime.ToString("fff"));
            raw = raw.Replace("{FFFF}", currentTime.ToString("ffff"));
            raw = raw.Replace("{ffff}", currentTime.ToString("ffff"));
            raw = raw.Replace("{FFFFF}", currentTime.ToString("fffff"));
            raw = raw.Replace("{fffff}", currentTime.ToString("fffff"));
            raw = raw.Replace("{FFFFFF}", currentTime.ToString("ffffff"));
            raw = raw.Replace("{ffffff}", currentTime.ToString("ffffff"));
            raw = raw.Replace("{FFFFFFF}", currentTime.ToString("fffffff"));
            raw = raw.Replace("{fffffff}", currentTime.ToString("fffffff"));

            return raw;
        }

        string FormatNumber(decimal number, int digit, char fillChar)
        {
            if (number < (decimal)Math.Pow(10, digit))
            {
                string numberStr = number.ToString();
                int fillCharCount = digit - numberStr.Length;
                return new string(fillChar, fillCharCount) + numberStr;
            }
            else
            {
                return number.ToString();
            }
        }
    }
}
