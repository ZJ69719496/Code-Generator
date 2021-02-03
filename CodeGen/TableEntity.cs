using System.Collections.Generic;
using System.Text;

namespace CodeGen
{
    class TableEntity
    {
        public TableEntity()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        private string preFix;
        public string PreFix
        {
            get { return preFix; }
        }

        /// <summary>
        /// 
        /// </summary>
        private string tableName;
        public string TableName
        {
            get
            {
                return tableName;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    tableName = value;

                    var names = tableName.Split('_');
                    if (names.Length > 0)
                    {
                        preFix = names[0];
                    }

                    if (!string.IsNullOrEmpty(PreFix))
                    {
                        value = value.TrimStart(PreFix.ToCharArray());
                    }

                    EntityName = StringUtils.ToEntityName(value);
                    entityName = StringUtils.LowFirst(EntityName);
                    ENTITYNAME = EntityName.ToUpper();
                    entityname = EntityName.ToLower();

                    PreEntityName = StringUtils.ToEntityName(tableName);
                    preEntityName = StringUtils.LowFirst(PreEntityName);
                    PREENTITYNAME = PreEntityName.ToUpper();
                    preentityname = PreEntityName.ToLower();
                }
                else
                {
                    tableName = "";
                    EntityName = "";
                    entityname = "";
                    ENTITYNAME = "";
                    entityname = "";

                    PreEntityName = "";
                    preEntityName = "";
                    PREENTITYNAME = "";
                    preentityname = "";
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string TableComment { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string entityName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ENTITYNAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string entityname { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PreEntityName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string preEntityName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PREENTITYNAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string preentityname { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private string _pkColumnName;
        public string pkColumnName
        {
            get
            {
                return _pkColumnName;
            }
            set
            {
                _pkColumnName = value;
                if (!string.IsNullOrEmpty(value))
                {
                    PkFieldName = StringUtils.ToEntityName(_pkColumnName);
                    pkFieldName = StringUtils.LowFirst(PkFieldName);
                    PKFIELDNAME = PkFieldName.ToUpper();
                    pkfieldname = PkFieldName.ToLower();
                }
                else
                {
                    PkFieldName = "";
                    pkFieldName = "";
                    PKFIELDNAME = "";
                    pkfieldname = "";
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string _pkColumnType;

        /// <summary>
        /// 
        /// </summary>
        public string pkColumnType
        {
            get
            {

                return _pkColumnType;
            }
            set
            {
                _pkColumnType = value;
                pkFieldType = getFieldType(_pkColumnType);
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
        /// 
        /// </summary>
        public string pkColumnComment { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string pkFieldType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int pkColumnTypeLength { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string pkFieldName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PkFieldName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string pkfieldname { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PKFIELDNAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, ColumnEntity> Cols { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
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
            sb.AppendLine(F(nameof(PreFix)) + PreFix);
            sb.AppendLine(F(nameof(TableName)) + TableName);
            sb.AppendLine(F(nameof(TableComment)) + TableComment);
            sb.AppendLine();
            sb.AppendLine(F(nameof(EntityName)) + EntityName);
            sb.AppendLine(F(nameof(entityName)) + entityName);
            sb.AppendLine(F(nameof(ENTITYNAME)) + ENTITYNAME);
            sb.AppendLine(F(nameof(entityname)) + entityname);
            sb.AppendLine();
            sb.AppendLine(F(nameof(PreEntityName)) + PreEntityName);
            sb.AppendLine(F(nameof(preEntityName)) + preEntityName);
            sb.AppendLine(F(nameof(PREENTITYNAME)) + PREENTITYNAME);
            sb.AppendLine(F(nameof(preentityname)) + preentityname);
            sb.AppendLine();
            sb.AppendLine(F(nameof(pkColumnName)) + pkColumnName);
            sb.AppendLine(F(nameof(pkColumnComment)) + pkColumnComment);
            sb.AppendLine(F(nameof(pkColumnType)) + pkColumnType);
            sb.AppendLine(F(nameof(pkFieldType)) + pkFieldType);
            sb.AppendLine(F(nameof(pkColumnTypeLength)) + pkColumnTypeLength);
            sb.AppendLine();
            sb.AppendLine(F(nameof(pkFieldName)) + pkFieldName);
            sb.AppendLine(F(nameof(PkFieldName)) + PkFieldName);
            sb.AppendLine(F(nameof(pkfieldname)) + pkfieldname);
            sb.AppendLine(F(nameof(PKFIELDNAME)) + PKFIELDNAME);
            sb.AppendLine();
            sb.AppendLine("Cols count  :  " + Cols.Count.ToString());
            return sb.ToString();
        }

    }


}