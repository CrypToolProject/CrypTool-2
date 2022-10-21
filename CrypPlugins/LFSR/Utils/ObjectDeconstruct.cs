/*
   Copyright 2019 Simon Leischnig, based on the work of Soeren Rinne

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using static CrypTool.LFSR.Utils.Datatypes;

namespace CrypTool.LFSR.Utils
{

    //
    public interface ToStringRecursive
    {
        string Convert(object o, List<object> visited);
        string Recurse(object o, List<object> visited);
    }

    public abstract class RecursiveLayer<TIn, TOut>
    {
        protected Func<object, List<object>, TOut> recursiveReceiver;
        public RecursiveLayer(Func<object, List<object>, TOut> recursiveReceiver) { this.recursiveReceiver = recursiveReceiver; }
        public abstract TOut Convert(TIn subject, List<object> visited);
        public abstract RecursiveLayer<TIn, TOut> CloneForRecursion(Func<object, List<object>, TOut> newRecursionReceiver);
    }
    //TODO: finish this up to simpled Recursive Layer
    public class RecLayerListbasedFormatting<T>
    {
        protected List<Func<T, List<object>, object>> lenses;

        public RecLayerListbasedFormatting(params Func<T, object>[] ex) : this(new List<Func<T, object>>(ex).ConvertAll(f => upcurry(f)).ToArray()) { }
        private static Func<T, List<object>, object> upcurry(Func<T, object> f)
        {
            return (o, vs) => f(o);
        }

        public RecLayerListbasedFormatting(params Func<T, List<object>, object>[] ex)
        {
            lenses = new List<Func<T, List<object>, object>>(ex);
            new List<string>(new string[] { "ab" });
        }
        public List<object> DeconstructToList(T t, List<object> visited)
        {
            return lenses.ConvertAll(lens => lens.Invoke(t, visited));
        }
        public Dictionary<string, object> DeconstructToDict(T t, List<object> visited)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            int i = 1;
            foreach (object item in DeconstructToList(t, visited))
            {
                dict["" + i] = item;
                i++;
            };
            return dict;
        }
        public string Format(string sep, string format, Dictionary<string, string> recursedDeconstruction)
        {
            StringBuilder builder = new StringBuilder();
            string[] split = format.Split(new string[] { sep }, StringSplitOptions.None);
            if (split.Length != recursedDeconstruction.Count + 1)
            {
                throw new FormatException($"Format {format} has not the right number of blanks for deconstruction {recursedDeconstruction}");
            }

            int i = 0; builder.Append(split[i]);
            for (i = 1; i < split.Length; i++)
            {
                builder.Append(recursedDeconstruction["" + i]);
                builder.Append(split[i]);
            }
            return builder.ToString();
        }
    }
    public class RecLayerListbased<T> : RecursiveLayer<T, string>
    {
        public RecLayerListbased(Func<object, List<object>, string> recursiveReceiver) : base(recursiveReceiver)
        {
            L1MainConvert = (subject, visited) => L2MakeRepr(subject, visited);

            L2MakeRepr =
                (subject, visited) =>
                {
                    List<string> stringpairs = L3MkSList(subject, visited);
                    return L2XSList2Repr(stringpairs, subject, visited);
                };

            L2XSList2Repr =
                (slist, subject, visited) =>
                {
                    StringBuilder b = new StringBuilder();
                    Type type = subject.GetType();
                    b.Append("[");
                    int i = 0;
                    foreach (string kv in slist)
                    {
                        b.Append($"{kv}");
                        if (i < slist.Count - 1)
                        {
                            b.Append(", ");
                        }

                        i++;
                    }
                    b.Append("]");
                    return b.ToString();
                };

            L3MkSList =
                (subject, visited) =>
                {
                    List<object> internaldict = L4Deconstruct(subject, visited);

                    List<string> result = new List<string>();
                    foreach (object kv in internaldict)
                    {
                        result.Add(RecurseOut(kv, visited));
                    }
                    return result;
                };

            L4Deconstruct =
                (subject, visited) =>
                {
                    return Dynamic.MemberList(subject);
                };

        }
        public Func<T, List<object>, string> L1MainConvert;
        public Func<T, List<object>, string> L2MakeRepr;
        public Func<List<string>, T, List<object>, string> L2XSList2Repr;
        public Func<T, List<object>, List<string>> L3MkSList;
        public Func<T, List<object>, List<object>> L4Deconstruct;

        public sealed override string Convert(T subject, List<object> visited)
        {
            return L1MainConvert.Invoke(subject, visited);
        }

        protected string MakeRepr(T subject, List<object> visited)
        {
            return L2MakeRepr.Invoke(subject, visited);
        }

        protected string Pairs2Repr(List<string> slist, T subject, List<object> visited)
        {
            return L2XSList2Repr.Invoke(slist, subject, visited);
        }

        protected List<string> MkPairs(T subject, List<object> visited)
        {
            return L3MkSList.Invoke(subject, visited);
        }

        protected List<object> Deconstruct(T subject, List<object> visited)
        {
            return L4Deconstruct.Invoke(subject, visited);
        }

        protected string RecurseOut(object sub, List<object> visited)
        {
            return recursiveReceiver.Invoke(sub, visited);
        }

        public override RecursiveLayer<T, string> CloneForRecursion(Func<object, List<object>, string> newRecursionReceiver)
        {
            RecLayerListbased<T> result = new RecLayerListbased<T>(newRecursionReceiver)
            {
                L1MainConvert = L1MainConvert,
                L2MakeRepr = L2MakeRepr,
                L2XSList2Repr = L2XSList2Repr,
                L3MkSList = L3MkSList,
                L4Deconstruct = L4Deconstruct
            };
            return result;
        }
    }
    public class RecLayerDictbased<T> : RecursiveLayer<T, string>
    {
        public RecLayerDictbased(Func<object, List<object>, string> recursiveReceiver) : base(recursiveReceiver)
        {
            L1MainConvert = (subject, visited) => L2MakeRepr(subject, visited);

            L2MakeRepr =
                (subject, visited) =>
                {
                    Dictionary<string, string> stringpairs = L3MkPairs(subject, visited);
                    return L2XPairs2Repr(stringpairs, subject, visited);
                };

            L2XPairs2Repr =
                (stringpairs, subject, visited) =>
                {
                    StringBuilder b = new StringBuilder();
                    Type type = subject.GetType();
                    string shortname = type.Name;
                    int lastDot = shortname.LastIndexOf(".");
                    if (lastDot > -1)
                    {
                        shortname = type.Name.Substring(lastDot + 1);
                    }
                    b.Append(shortname + "{");
                    int i = 0;
                    foreach (KeyValuePair<string, string> kv in stringpairs)
                    {
                        b.Append($"{kv.Key} : {kv.Value}");
                        if (i < stringpairs.Count - 1)
                        {
                            b.Append(", ");
                        }

                        i++;
                    }
                    b.Append("}");
                    return b.ToString();
                };

            L3MkPairs =
                (subject, visited) =>
                {
                    Dictionary<string, object> internaldict = L4Deconstruct(subject, visited);

                    Dictionary<string, string> result = new Dictionary<string, string>();
                    foreach (KeyValuePair<string, object> kv in internaldict)
                    {
                        result[kv.Key] = RecurseOut(kv.Value, visited);
                    }
                    return result;
                };

            L4Deconstruct =
                (subject, visited) =>
                {
                    return Dynamic.MemberDict(subject);
                };
        }
        public Func<T, List<object>, string> L1MainConvert;
        public Func<T, List<object>, string> L2MakeRepr;
        public Func<Dictionary<string, string>, T, List<object>, string> L2XPairs2Repr;
        public Func<T, List<object>, Dictionary<string, string>> L3MkPairs;
        public Func<T, List<object>, Dictionary<string, object>> L4Deconstruct;

        public sealed override string Convert(T subject, List<object> visited)
        {
            return L1MainConvert.Invoke(subject, visited);
        }

        protected string MakeRepr(T subject, List<object> visited)
        {
            return L2MakeRepr.Invoke(subject, visited);
        }

        protected string Pairs2Repr(Dictionary<string, string> stringpairs, T subject, List<object> visited)
        {
            return L2XPairs2Repr.Invoke(stringpairs, subject, visited);
        }

        protected Dictionary<string, string> MkPairs(T subject, List<object> visited)
        {
            return L3MkPairs.Invoke(subject, visited);
        }

        protected Dictionary<string, object> Deconstruct(T subject, List<object> visited)
        {
            return L4Deconstruct.Invoke(subject, visited);
        }

        protected string RecurseOut(object sub, List<object> visited)
        {
            return recursiveReceiver.Invoke(sub, visited);
        }

        public override RecursiveLayer<T, string> CloneForRecursion(Func<object, List<object>, string> newRecursionReceiver)
        {
            RecLayerDictbased<T> result = new RecLayerDictbased<T>(newRecursionReceiver)
            {
                L1MainConvert = L1MainConvert,
                L2MakeRepr = L2MakeRepr,
                L2XPairs2Repr = L2XPairs2Repr,
                L3MkPairs = L3MkPairs,
                L4Deconstruct = L4Deconstruct
            };
            return result;
        }
    }
    public interface IToStringDeconstruction
    {
        Option<Type> TypeCoverageFor(object o);
        string Convert(object subject, List<object> visited);
        string Convert(object subject);
        string ConvertLease(Func<object, List<object>, string> callback, object subject, List<object> visited);
    }
    public class TypedDeconstruction<T> : IToStringDeconstruction
    {
        protected RecursiveLayer<T, string> deconstruction;

        public virtual Option<Type> TypeCoverageFor(object o)
        {
            return o is T t ? Some(typeof(T)) : None<Type>();
        }

        public virtual string FormatAlreadySeen(object knowObj, List<object> visited)
        {
            return "{...}";
        }

        public virtual string FormatNull()
        {
            return "<null>";
        }

        protected Func<object, List<object>, string> fallback;

        public string Convert(object subject, List<object> visited)
        {
            if (TypeCoverageFor(subject).IsNone)
            {
                return fallback.Invoke(subject, visited);
            }
            if (visited.Contains(subject))
            {
                return FormatAlreadySeen(subject, visited);
            }

            visited.Add(subject);
            if (subject == null) { return FormatNull(); }
            return deconstruction.Convert(subject as dynamic, visited);
        }
        public string Convert(object subject) { return Convert(subject, new List<object>()); }
        public virtual string ConvertLease(Func<object, List<object>, string> callback, object subject, List<object> visited)
        {
            RecursiveLayer<T, string> lease = deconstruction.CloneForRecursion(callback);
            return lease.Convert(subject as dynamic, visited);
        }

        public TypedDeconstruction(Func<object, List<object>, string> fallback, RecursiveLayer<T, string> deconstruction)
        {
            this.fallback = fallback;
            this.deconstruction = deconstruction ?? new RecLayerDictbased<T>(Convert);
        }
    }

    public class SeqToStringDeconstruction : TypedDeconstruction<System.Collections.IEnumerable>
    {
        public SeqToStringDeconstruction(Func<object, List<object>, string> fallback, Func<object, List<object>, string> recursiveReceiver) : base(fallback, ObjToSeqLayer(recursiveReceiver)) { }

        public static RecLayerListbased<System.Collections.IEnumerable> ObjToSeqLayer(Func<object, List<object>, string> recursiveReceiver)
        {
            return new RecLayerListbased<System.Collections.IEnumerable>(recursiveReceiver)
            {
                L4Deconstruct = (subject, vs) =>
                {
                    List<object> r = new List<object>(); foreach (object i in subject)
                    {
                        r.Add(i);
                    }

                    return r;
                }
            };
        }

        public override Option<Type> TypeCoverageFor(object o)
        {
            Type valueType = o.GetType();
            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(valueType))
            {
                return Some(typeof(System.Collections.IEnumerable));
            }

            return None<Type>();
        }
    }

    // TODO: arrays won't be subject to "already visited" checking when wrapped
    public class ArrayToStringDeconstruction : SeqToStringDeconstruction
    {
        public ArrayToStringDeconstruction(Func<object, List<object>, string> fallback, Func<object, List<object>, string> recursiveReceiver) : base(fallback, recursiveReceiver) { }

        public override string ConvertLease(Func<object, List<object>, string> callback, object subject, List<object> visited)
        {
            RecursiveLayer<System.Collections.IEnumerable, string> lease = deconstruction.CloneForRecursion(callback);
            if (TypeCoverageFor(subject).IsNone)
            {
                return fallback(subject, visited);
            }
            Array me = (Array)subject;
            return lease.Convert(me.Cast<object>(), visited);
        }

        public override Option<Type> TypeCoverageFor(object o)
        {
            Type valueType = o.GetType();
            if (valueType.IsArray)
            {
                return Some<Type>(valueType);
            }

            return None<Type>();
        }
    }

    // Centerpieces : Build and aggregate ToString models that are nestable and know which one is responsible
    public class AggregateStrConverter
    {
        public IToStringDeconstruction fallback;

        public List<IToStringDeconstruction> Converters { get; } = new List<IToStringDeconstruction>() { };
        public Option<(IToStringDeconstruction, Type)> GetMostSpecialized(object subject)
        {
            List<IToStringDeconstruction> toSort = new List<IToStringDeconstruction>(Converters);
            List<(IToStringDeconstruction c, Option<Type>)> paired = toSort.ConvertAll(c => (c, c.TypeCoverageFor(subject)));
            paired.Sort((x, y) => AggregateStrConverter.CompareSpecialization(x, y, subject));
            paired.RemoveAll(p => p.Item2.IsNone);
            List<(IToStringDeconstruction, Type)> result = paired.ConvertAll(p => (p.Item1, p.Item2.Get()));
            if (result.Count == 0)
            {
                return None<(IToStringDeconstruction, Type)>();
            }

            return Some(result[paired.Count - 1]);
        }

        public Option<Type> TypeCoverageFor(object o)
        {
            return GetMostSpecialized(o).Map(some => some.Item2);
        }

        public string Convert(object subject, List<object> visited)
        {
            if (subject == null)
            {
                return GetNullRepr();
            }

            Option<(IToStringDeconstruction, Type)> conv = GetMostSpecialized(subject);
            if (conv.IsNone)
            {
                return subject.ToString();
            }

            (IToStringDeconstruction converter, Type tpeOpt) = conv.Get();
            return converter.ConvertLease(Convert, subject, visited);
        }

        private string GetNullRepr()
        {
            return "<null>";
        }

        public static int CompareSpecialization((IToStringDeconstruction c, Option<Type> t) x, (IToStringDeconstruction c, Option<Type> t) y, object o)
        {
            if (x.t.IsNone && y.t.IsNone)
            {
                return 0;
            }

            if (x.t.IsNone)
            {
                return -1;
            }

            if (y.t.IsNone)
            {
                return 1;
            }

            if (x.t.Get().Equals(y.t.Get()))
            {
                return 0;
            }
            if (y.t.Get().IsAssignableFrom(x.t.Get()))
            {
                return 1;
            }
            return -1;
        }

        //TODO: cannot be used recursively right now. But adding the converters into one list should be ok too.
        public string ConvertLease(Func<object, List<object>, string> callback, object subject, List<object> visited)
        {
            return Convert(subject, visited);
        }
        public string Convert(object subject)
        {
            return Convert(subject, new List<object>());
        }

        public AggregateStrConverter() : this(Sequence<IToStringDeconstruction>()) { }
        public AggregateStrConverter(IEnumerable<IToStringDeconstruction> converters) : base()
        {
            Converters = new List<IToStringDeconstruction>(converters);
            fallback = new TypedDeconstruction<object>(null, new RecLayerDictbased<object>(Convert)
            {
                L1MainConvert = Convert
            });
        }
    }
    public class AggregateStrConverterInc : AggregateStrConverter
    {
        // Fields/interface)

        public AggregateStrConverterInc()
        {
            BuildMe();
        }
        protected virtual void BuildMe() { }

        public AggregateStrConverterInc WithAll(IEnumerable<IToStringDeconstruction> cs) { foreach (IToStringDeconstruction c in cs) { Converters.Add(c); } return this; }
        public AggregateStrConverterInc With(IToStringDeconstruction c) { Converters.Add(c); return this; }
        public AggregateStrConverterInc WithType<T>()
        {
            return With(new TypedDeconstruction<T>(fallback.Convert, new RecLayerDictbased<T>(Convert)));
        }

        public AggregateStrConverterInc WithType<T>(Func<T, List<object>, string> conversion)
        {
            return With(new TypedDeconstruction<T>(fallback.Convert, new RecLayerDictbased<T>(Convert)
            {
                L1MainConvert = (t, vs) => conversion.Invoke(t, vs)
            }));
        }

        public AggregateStrConverterInc WithType<T>(Func<T, string> conversion)
        {
            return With(new TypedDeconstruction<T>(fallback.Convert, new RecLayerDictbased<T>(Convert)
            {
                L1MainConvert = (t, vs) => conversion.Invoke(t)
            }));
        }

        public AggregateStrConverterInc WithRFormat<T>(string sep, string format, params Func<T, object>[] lenses)
        {
            return With(RFormat(sep, format, lenses));
        }

        public AggregateStrConverterInc WithRFormat<T>(string format, params Func<T, object>[] lenses)
        {
            return With(RFormat(format, lenses));
        }

        public AggregateStrConverterInc WithRFormat<T>(string sep, string format, params Func<T, object, List<object>>[] lenses)
        {
            return With(RFormat(sep, format, lenses));
        }

        public AggregateStrConverterInc WithRFormat<T>(string format, params Func<T, object, List<object>>[] lenses)
        {
            return With(RFormat(format, lenses));
        }

        public IToStringDeconstruction RFormat<T>(string format, params Func<T, object>[] lenses)
        {
            return RFormat("{}", format, lenses);
        }

        public IToStringDeconstruction RFormat<T>(string sep, string format, params Func<T, object>[] lenses)
        {
            RecLayerListbasedFormatting<T> extractor = new RecLayerListbasedFormatting<T>(lenses);
            TypedDeconstruction<T> converter = new TypedDeconstruction<T>(fallback.Convert, new RecLayerDictbased<T>(Convert)
            {
                L4Deconstruct = (t, vs) => extractor.DeconstructToDict(t, vs),
                L2XPairs2Repr = (dict, t, vs) => extractor.Format(sep, format, dict)
            }
            );
            return converter;
        }
        public IToStringDeconstruction RFormat<T>(string format, params Func<T, object, List<object>>[] lenses)
        {
            return RFormat("{}", format, lenses);
        }

        public IToStringDeconstruction RFormat<T>(string sep, string format, params Func<T, object, List<object>>[] lenses)
        {
            RecLayerListbasedFormatting<T> extractor = new RecLayerListbasedFormatting<T>(lenses);
            return new TypedDeconstruction<T>(fallback.Convert, new RecLayerDictbased<T>(Convert)
            {
                L4Deconstruct = extractor.DeconstructToDict,
                L2XPairs2Repr = (dict, t, vs) => extractor.Format(sep, format, dict),
            });
        }
    }

    public static class Dynamic
    {
        public static string ObjKVToStr(string name, Dictionary<string, string> kvdict)
        {
            StringBuilder b = new StringBuilder();
            b.Append(name + "{");
            int i = 0;
            foreach (KeyValuePair<string, string> kv in kvdict)
            {
                b.Append($"{kv.Key}:{kv.Value}");
                if (i < kvdict.Count - 1)
                {
                    b.Append(", ");
                }

                i++;
            }
            b.Append("}");
            return b.ToString();
        }
        internal static List<object> MemberList<T>(T subject)
        {
            Type tpe = GetType(subject);
            List<object> result = new List<object>();
            foreach (PropertyInfo info in tpe.GetProperties())
            {
                object value = info.GetValue(subject);
                result.Add(value);
            }
            foreach (FieldInfo info in tpe.GetFields())
            {
                object value = info.GetValue(subject);
                result.Add(info.Name);
            }
            return result;
        }
        public static Dictionary<string, object> MemberDict(object o)
        {
            Type tpe = GetType(o);
            Dictionary<string, object> result = new Dictionary<string, object>();
            foreach (PropertyInfo info in tpe.GetProperties())
            {
                object value = info.GetValue(o);
                int lastidxofdot = info.Name.LastIndexOf(".");
                string shortname = info.Name;
                if (lastidxofdot > -1)
                {
                    shortname = info.Name.Substring(info.Name.LastIndexOf(".") + 1);
                }
                result.Add(shortname, value);
            }
            foreach (FieldInfo info in tpe.GetFields())
            {
                object value = info.GetValue(o);
                result.Add(info.Name, value);
            }
            return result;
        }
        public static Type GetType(object o)
        {
            return o?.GetType();
        }
    }
}

