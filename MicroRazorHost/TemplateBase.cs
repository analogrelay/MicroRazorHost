using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace MicroRazorHost
{
    public abstract class TemplateBase
    {
        private StringBuilder _output;
        private StringWriter _writer;

        public StringWriter Writer { get { return _writer; } }

        public TemplateBase()
        {
            _output = new StringBuilder();
            _writer = new StringWriter(_output);
        }

        public string Run()
        {
            Reset();
            Execute();
            _writer.Flush();
            string result = _output.ToString();
            Reset();
            return result;
        }

        public abstract void Execute();

        protected virtual void Write(object val)
        {
            WriteTo(Writer, val);
        }

        protected virtual void WriteLiteral(object val)
        {
            WriteLiteralTo(Writer, val);
        }

        protected virtual void WriteTo(TextWriter writer, object val)
        {
            WriteLiteralTo(writer, WebUtility.HtmlEncode(val.ToString()));
        }

        protected virtual void WriteLiteralTo(TextWriter writer, object val)
        {
            Writer.Write(val);
        }

        public virtual void WriteAttribute(string name, PositionTagged<string> prefix, PositionTagged<string> suffix, params AttributeValue[] values)
        {
            WriteAttributeTo(Writer, name, prefix, suffix, values);
        }

        public virtual void WriteAttributeTo(TextWriter writer, string name, PositionTagged<string> prefix, PositionTagged<string> suffix, params AttributeValue[] values)
        {
            bool first = true;
            bool wroteSomething = false;
            if (values.Length == 0)
            {
                // Explicitly empty attribute, so write the prefix and suffix
                WritePositionTaggedLiteral(writer, prefix);
                WritePositionTaggedLiteral(writer, suffix);
            }
            else
            {
                for (int i = 0; i < values.Length; i++)
                {
                    AttributeValue attrVal = values[i];
                    PositionTagged<object> val = attrVal.Value;
                    PositionTagged<string> next = i == values.Length - 1 ?
                        suffix : // End of the list, grab the suffix
                        values[i + 1].Prefix; // Still in the list, grab the next prefix

                    bool? boolVal = null;
                    if (val.Value is bool)
                    {
                        boolVal = (bool)val.Value;
                    }

                    if (val.Value != null && (boolVal == null || boolVal.Value))
                    {
                        string valStr = val.Value as string;
                        if (valStr == null)
                        {
                            valStr = val.Value.ToString();
                        }
                        if (boolVal != null)
                        {
                            Debug.Assert(boolVal.Value);
                            valStr = name;
                        }

                        if (first)
                        {
                            WritePositionTaggedLiteral(writer, prefix);
                            first = false;
                        }
                        else
                        {
                            WritePositionTaggedLiteral(writer, attrVal.Prefix);
                        }

                        // Calculate length of the source span by the position of the next value (or suffix)
                        int sourceLength = next.Position - attrVal.Value.Position;

                        if (attrVal.Literal)
                        {
                            WriteLiteralTo(writer, valStr);
                        }
                        else
                        {
                            WriteTo(writer, valStr); // Write value
                        }
                        wroteSomething = true;
                    }
                }
                if (wroteSomething)
                {
                    WritePositionTaggedLiteral(writer, suffix);
                }
            }
        }

        private void WritePositionTaggedLiteral(TextWriter writer, string value, int position)
        {
            WriteLiteralTo(writer, value);
        }

        private void WritePositionTaggedLiteral(TextWriter writer, PositionTagged<string> value)
        {
            WritePositionTaggedLiteral(writer, value.Value, value.Position);
        }

        private void Reset()
        {
            _writer.Close();
            _output.Length = 0;
            _writer = new StringWriter(_output);
        }
    }
}
