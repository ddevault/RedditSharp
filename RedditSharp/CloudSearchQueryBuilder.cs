using System;
using System.Collections.Generic;
using System.Text;

namespace RedditSharp
{
    public enum CloudSearchQueryBuilderDataType
    {
        String,
        Integer,
        DateTime
    }

    public enum CloudSearchQueryBuilderQueryType
    {
        Normal,
        Range
    }

    public interface ICloudSearchQueryBuilderItem
    {
        string Name { get; }
        CloudSearchQueryBuilderDataType Type { get; }
        string GetStringValue();
        string ToString();
    }

    public abstract class CloudSearchQueryBuilderItemBase<T> : ICloudSearchQueryBuilderItem
    {
        protected CloudSearchQueryBuilderItemBase(string name, T value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; private set; }
        public T Value { get; private set; }

        public abstract CloudSearchQueryBuilderDataType Type { get; }
        public abstract string GetStringValue();

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class DateTimeCloudSearchQueryBuilderItem : CloudSearchQueryBuilderItemBase<DateTime?>
    {
        public DateTimeCloudSearchQueryBuilderItem(string name, DateTime? value)
            : base(name, value)
        {
        }

        public override CloudSearchQueryBuilderDataType Type
        {
            get { return CloudSearchQueryBuilderDataType.DateTime; }
        }

        public override string GetStringValue()
        {
            if (Value == null)
                return null;

            return Value.Value.ToEpoch().ToString();
        }
    }

    public class IntegerCloudSearchQueryBuilderItem : CloudSearchQueryBuilderItemBase<int>
    {
        public IntegerCloudSearchQueryBuilderItem(string name, int value)
            : base(name, value)
        {
        }

        public override CloudSearchQueryBuilderDataType Type
        {
            get { return CloudSearchQueryBuilderDataType.Integer; }
        }

        public override string GetStringValue()
        {
            return Value.ToString();
        }
    }

    public class StringCloudSearchQueryBuilderItem : CloudSearchQueryBuilderItemBase<string>
    {
        public StringCloudSearchQueryBuilderItem(string name, string value)
            : base(name, value)
        {
        }

        public override CloudSearchQueryBuilderDataType Type
        {
            get { return CloudSearchQueryBuilderDataType.String; }
        }

        public override string GetStringValue()
        {
            return Value;
        }
    }

    public class CloudSearchQueryBuilder
    {
        private readonly List<KeyValuePair<ICloudSearchQueryBuilderItem, CloudSearchQueryBuilderQueryType>> _items =
            new List<KeyValuePair<ICloudSearchQueryBuilderItem, CloudSearchQueryBuilderQueryType>>();


        public void Add(string name, string from, string to)
        {
            _items.Add(new KeyValuePair<ICloudSearchQueryBuilderItem, CloudSearchQueryBuilderQueryType>(new StringCloudSearchQueryBuilderItem(name, from), CloudSearchQueryBuilderQueryType.Range));    
            Add(name, to);
        }

        public void Add(string name, string value)
        {
            _items.Add(new KeyValuePair<ICloudSearchQueryBuilderItem, CloudSearchQueryBuilderQueryType>(new StringCloudSearchQueryBuilderItem(name, value), CloudSearchQueryBuilderQueryType.Normal));
        }

        public void Add(string name, int from, int to)
        {
            _items.Add(new KeyValuePair<ICloudSearchQueryBuilderItem, CloudSearchQueryBuilderQueryType>(new IntegerCloudSearchQueryBuilderItem(name, from), CloudSearchQueryBuilderQueryType.Range));
            Add(name, to);
        }
        
        public void Add(string name, int value)
        {
            _items.Add(new KeyValuePair<ICloudSearchQueryBuilderItem, CloudSearchQueryBuilderQueryType>(new IntegerCloudSearchQueryBuilderItem(name, value), CloudSearchQueryBuilderQueryType.Normal));
        }

        public void Add(string name, DateTime? from, DateTime? to)
        {
            _items.Add(new KeyValuePair<ICloudSearchQueryBuilderItem, CloudSearchQueryBuilderQueryType>(new DateTimeCloudSearchQueryBuilderItem(name, from), CloudSearchQueryBuilderQueryType.Range));
            Add(name, to);
        }

        public void Add(string name, DateTime? value)
        {
            _items.Add(new KeyValuePair<ICloudSearchQueryBuilderItem, CloudSearchQueryBuilderQueryType>(new DateTimeCloudSearchQueryBuilderItem(name, value), CloudSearchQueryBuilderQueryType.Normal));
        }

        private static void AppendValue(StringBuilder builder, ICloudSearchQueryBuilderItem item)
        {
            if (item.Type == CloudSearchQueryBuilderDataType.String)
                builder.Append('\'');

            var value = item.GetStringValue();

            builder.Append(!string.IsNullOrEmpty(value) ? Uri.EscapeUriString(value) : null);

            if (item.Type == CloudSearchQueryBuilderDataType.String)
                builder.Append('\'');
        }

        public string BuildQuery()
        {
            var builder = new StringBuilder("(and ");

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];

                builder.Append(item.Key.Name);
                builder.Append(':');
                AppendValue(builder, item.Key);

                if (item.Value == CloudSearchQueryBuilderQueryType.Range)
                {
                    i++;
                    var to = _items[i];
                    builder.Append("..");
                    AppendValue(builder, to.Key);
                }

                if (i < _items.Count - 1)
                    builder.Append(' ');
            }

            builder.Append(')');

            return builder.ToString();
        }

        public override string ToString()
        {
            return BuildQuery();
        }
    }
}