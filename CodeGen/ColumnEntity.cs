using System.Text;

namespace CodeGen
{
    /// <summary>
    /// 列实体
    /// </summary>
    public class ColumnEntity
    {
        private string _columnName;
        /// <summary>
        /// 列名称
        /// </summary>
        public string ColumnName
        {
            get
            {
                return _columnName;
            }
            set
            {
                _columnName = value;
                if (!string.IsNullOrEmpty(value))
                {
                    FieldName = StringUtils.ToEntityName(_columnName);
                    fieldName = StringUtils.LowFirst(FieldName);
                    FIELDNAME = FieldName.ToUpper();
                    fieldname = FieldName.ToLower();
                }
                else
                {
                    FieldName = "";
                    fieldName = "";
                    FIELDNAME = "";
                    fieldname = "";
                }
            }
        }

        /// <summary>
        /// 字段名称 小写开头
        /// </summary>
        public string fieldName { get; private set; }

        /// <summary>
        /// 字段名称大写开头
        /// </summary>
        public string FieldName { get; private set; }

        /// <summary>
        /// 全小写
        /// </summary>
        public string fieldname { get; private set; }

        /// <summary>
        /// 全大写
        /// </summary>
        public string FIELDNAME { get; private set; }


        private string _columnType;
        /// <summary>
        /// 列数据库类型
        /// </summary>
        public string ColumnType
        {
            get
            {
                return _columnType;
            }
            set
            {
                _columnType = value;
                fieldType = getFieldType(_columnType);
            }
        }

        /// <summary>
        /// 根据数据库类型 映射C# 类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string getFieldType(string _columnType)
        {
            string reval = string.Empty;
            switch (_columnType.ToLower())
            {
                case "int":
                    reval = "int";
                    break;
                case "text":
                    reval = "string";
                    break;
                case "bigint":
                    reval = "int";
                    break;
                case "binary":
                    reval = "byte[]";
                    break;
                case "bit":
                    reval = "bool";
                    break;
                case "char":
                    reval = "string";
                    break;
                case "datetime":
                    reval = "System.DateTime";
                    break;
                case "decimal":
                    reval = "System.Decimal";
                    break;
                case "float":
                    reval = "System.Double";
                    break;
                case "image":
                    reval = "byte[]";
                    break;
                case "money":
                    reval = "System.Decimal";
                    break;
                case "nchar":
                    reval = "string";
                    break;
                case "ntext":
                    reval = "string";
                    break;
                case "numeric":
                    reval = "System.Decimal";
                    break;
                case "nvarchar":
                    reval = "string";
                    break;
                case "real":
                    reval = "System.Single";
                    break;
                case "smalldatetime":
                    reval = "System.Datetime";
                    break;
                case "smallint":
                    reval = "int16";
                    break;
                case "smallmoney":
                    reval = "System.Decimal";
                    break;
                case "timestamp":
                    reval = "System.Datetime";
                    break;
                case "tinyint":
                    reval = "byte";
                    break;
                case "uniqueidentifier":
                    reval = "System.Guid";
                    break;
                case "varbinary":
                    reval = "byte[]";
                    break;
                case "varchar":
                    reval = "string";
                    break;
                case "variant":
                    reval = "object";
                    break;
                default:
                    reval = "string";
                    break;
            }
            return reval;
        }

        /// <summary>
        /// 字段类型
        /// </summary>
        public string fieldType { get; private set; }

        /// <summary>
        /// 列类型长度
        /// </summary>
        public int columnTypeLength { get; set; }

        /// <summary>
        /// 列备注
        /// </summary>
        public string columnComment { get; set; }

        /// <summary>
        /// 是否主键
        /// </summary>
        public bool IsPk { get; set; }

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsNull { get; set; }


        private string F(string str)
        {
            return "{{$" + str + "}}  :  ";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(F(nameof(ColumnName) ) + ColumnName);
            sb.AppendLine(F(nameof(columnComment) ) + columnComment);
            sb.AppendLine(F(nameof(ColumnType) ) + ColumnType);
            sb.AppendLine(F(nameof(fieldType) ) + fieldType);
            sb.AppendLine(F(nameof(columnTypeLength) ) + columnTypeLength);
            sb.AppendLine();
            sb.AppendLine(F(nameof(fieldName) ) + fieldName);
            sb.AppendLine(F(nameof(FieldName) ) + FieldName);
            sb.AppendLine(F(nameof(fieldname) ) + fieldname);
            sb.AppendLine(F(nameof(FIELDNAME) ) + FIELDNAME);
            sb.AppendLine();
            sb.AppendLine(F(nameof(IsPk) ) + IsPk);
            sb.AppendLine(F(nameof(IsNull) ) + IsNull);
            return sb.ToString();
        }
    }
}