using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    public abstract class Parameter<T> where T : IComparable<T>
    {
        // class for holding parameters restricted to ranges

        public T UpperBound { get; }
        public T LowerBound { get; }

        // the soft bounds are there to provide basic distributional information on the parameter
        // and to provided sensible bounds if full range parameters are presented to the user

        private T _softUpperBound;
        public T SoftUpperBound
        {
            get => _softUpperBound;
            set
            {
                if (Comparer<T>.Default.Compare(value, UpperBound) <= 0 && Comparer<T>.Default.Compare(value, LowerBound) >= 0)
                {
                    _softUpperBound = value;
                }
            }
        }

        private T _softLowerBound;
        public T SoftLowerBound
        {
            get => _softLowerBound;
            set
            {
                if (Comparer<T>.Default.Compare(value, UpperBound) <= 0 && Comparer<T>.Default.Compare(value, LowerBound) >= 0)
                {
                    _softLowerBound = value;
                }
            }
        }

        private T _val;
        public T Value
        {
            get => _val;
            set
            {
                if (Comparer<T>.Default.Compare(value, UpperBound) <= 0 && Comparer<T>.Default.Compare(value, LowerBound) >= 0)
                {
                    _val = value;
                }
            }
        }

        public Parameter(T val, T ub, T lb)
        {
            if (Comparer<T>.Default.Compare(val, ub) > 0 || Comparer<T>.Default.Compare(val, lb) < 0)
            {
                throw new Exception("Value provided exceeds bounds");
            }

            UpperBound = ub;
            LowerBound = lb;

            Value = val;

            SoftUpperBound = ub;
            SoftLowerBound = lb;
        }

        public Parameter(T val, T ub, T lb, T sub, T slb) : this(val, ub, lb)
        {
            if (Comparer<T>.Default.Compare(sub, ub) > 0 || Comparer<T>.Default.Compare(sub, lb) < 0)
            {
                throw new Exception("Soft upper bound provided exceeds bounds");
            }

            if (Comparer<T>.Default.Compare(slb, ub) > 0 || Comparer<T>.Default.Compare(slb, lb) < 0)
            {
                throw new Exception("Soft lower bound provided exceeds bounds");
            }

            SoftUpperBound = sub;
            SoftLowerBound = slb;
        }


    }
}
