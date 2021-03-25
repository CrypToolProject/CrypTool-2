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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CrypTool.PluginBase.Utils
{
    using Datatypes;
    using System.Collections.Generic;
    using static Datatypes.Datatypes;
    namespace ObjectDeconstruct
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
            private static Func<T, List<object>, object> upcurry(Func<T, object> f) => (o, vs) => f(o);

            public RecLayerListbasedFormatting(params Func<T, List<object>, object>[] ex)
            {
                this.lenses = new List<Func<T, List<object>, object>>(ex);
                new List<String>(new string[]{ "ab"});
            }
            public List<object> DeconstructToList(T t, List<object> visited)
            {
                return lenses.ConvertAll(lens => lens.Invoke(t, visited));
            }
            public Dictionary<String, object> DeconstructToDict(T t, List<object> visited)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                var i = 1;
                foreach (var item in DeconstructToList(t, visited)) {
                    dict[""+i] = item;
                    i++;
                };
                return dict;
            }
            public string Format(string sep, string format, Dictionary<string, string> recursedDeconstruction)
            {
                StringBuilder builder = new StringBuilder();
                var split = format.Split(new string[] { sep }, StringSplitOptions.None);
                if (split.Length != recursedDeconstruction.Count + 1) throw new FormatException($"Format {format} has not the right number of blanks for deconstruction {recursedDeconstruction}");
                var i = 0; builder.Append(split[i]);
                for (i = 1; i < split.Length; i++)
                {
                    builder.Append(recursedDeconstruction[""+i]);
                    builder.Append(split[i]);
                }
                return builder.ToString();
            }
        }
        public class RecLayerListbased<T> : RecursiveLayer<T, string>
        {
            public RecLayerListbased(Func<object, List<object>, string> recursiveReceiver) : base(recursiveReceiver)
            {
                this.L1MainConvert = (subject, visited) => L2MakeRepr(subject, visited);

                this.L2MakeRepr = 
                    (subject, visited) =>
                    {
                        var stringpairs = L3MkSList(subject, visited);
                        return L2XSList2Repr(stringpairs, subject, visited);
                    };

                this.L2XSList2Repr = 
                    (slist, subject, visited) => 
                    {
                        StringBuilder b = new StringBuilder();
                        Type type = subject.GetType();
                        b.Append("[");
                        var i = 0;
                        foreach (var kv in slist)
                        {
                            b.Append($"{kv}");
                            if (i < slist.Count - 1) b.Append(", ");
                            i++;
                        }
                        b.Append("]");
                        return b.ToString();
                    };

                this.L3MkSList =
                    (subject, visited) =>
                    {
                        var internaldict = this.L4Deconstruct(subject, visited);

                        var result = new List<string>();
                        foreach (var kv in internaldict)
                        {
                            result.Add(this.RecurseOut(kv, visited));
                        }
                        return result;
                    };

                this.L4Deconstruct =
                    (subject, visited) =>
                    {
                        return Dynamic.MemberList(subject);
                    };

            }
            public Func<T, List<object>, string> L1MainConvert;
            public Func<T, List<object>, string> L2MakeRepr;
            public Func<List<String>, T, List<object>, string> L2XSList2Repr;
            public Func<T, List<object>, List<string>> L3MkSList;
            public Func<T, List<object>, List<object>> L4Deconstruct;

            public sealed override string Convert(T subject, List<object> visited) => L1MainConvert.Invoke(subject, visited);
            protected string MakeRepr(T subject, List<object> visited) => L2MakeRepr.Invoke(subject, visited);
            protected string Pairs2Repr (List<String> slist, T subject, List<object> visited) => L2XSList2Repr.Invoke(slist, subject, visited);
            protected List<string> MkPairs(T subject, List<object> visited) => L3MkSList.Invoke(subject, visited);
            protected List<object> Deconstruct(T subject, List<object> visited) => L4Deconstruct.Invoke(subject, visited);

            protected String RecurseOut(object sub, List<object> visited) => recursiveReceiver.Invoke(sub, visited);

            public override RecursiveLayer<T, string> CloneForRecursion(Func<object, List<object>, string> newRecursionReceiver)
            {
                var result = new RecLayerListbased<T>(newRecursionReceiver);
                result.L1MainConvert = L1MainConvert;
                result.L2MakeRepr = L2MakeRepr;
                result.L2XSList2Repr = L2XSList2Repr;
                result.L3MkSList = L3MkSList;
                result.L4Deconstruct = L4Deconstruct;
                return result;
            }
        }
        public class RecLayerDictbased<T> : RecursiveLayer<T, String>
        {
            public RecLayerDictbased(Func<object, List<object>, String> recursiveReceiver) : base(recursiveReceiver)
            {
                this.L1MainConvert = (subject, visited) => L2MakeRepr(subject, visited);

                this.L2MakeRepr = 
                    (subject, visited) =>
                    {
                        Dictionary<String, String> stringpairs = L3MkPairs(subject, visited);
                        return L2XPairs2Repr(stringpairs, subject, visited);
                    };

                this.L2XPairs2Repr = 
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
                        var i = 0;
                        foreach (var kv in stringpairs)
                        {
                            b.Append($"{kv.Key} : {kv.Value}");
                            if (i < stringpairs.Count - 1) b.Append(", ");
                            i++;
                        }
                        b.Append("}");
                        return b.ToString();
                    };

                this.L3MkPairs =
                    (subject, visited) =>
                    {
                        Dictionary<string, object> internaldict = this.L4Deconstruct(subject, visited);

                        Dictionary<string, string> result = new Dictionary<string, string>();
                        foreach (var kv in internaldict)
                        {
                            result[kv.Key] = this.RecurseOut(kv.Value, visited);
                        }
                        return result;
                    };

                this.L4Deconstruct =
                    (subject, visited) =>
                    {
                        return Dynamic.MemberDict(subject);
                    };
            }
            public Func<T, List<object>, string> L1MainConvert;
            public Func<T, List<object>, string> L2MakeRepr;
            public Func<Dictionary<String, String>, T, List<object>, string> L2XPairs2Repr;
            public Func<T, List<object>, Dictionary<string, string>> L3MkPairs;
            public Func<T, List<object>, Dictionary<String, object>> L4Deconstruct;

            public sealed override string Convert(T subject, List<object> visited) => L1MainConvert.Invoke(subject, visited);
            protected string MakeRepr(T subject, List<object> visited) => L2MakeRepr.Invoke(subject, visited);
            protected string Pairs2Repr (Dictionary<String, String> stringpairs, T subject, List<object> visited) => L2XPairs2Repr.Invoke(stringpairs, subject, visited);
            protected Dictionary<string, string> MkPairs(T subject, List<object> visited) => L3MkPairs.Invoke(subject, visited);
            protected Dictionary<String, object> Deconstruct(T subject, List<object> visited) => L4Deconstruct.Invoke(subject, visited);

            protected String RecurseOut(object sub, List<object> visited) => recursiveReceiver.Invoke(sub, visited);

            public override RecursiveLayer<T, string> CloneForRecursion(Func<object, List<object>, string> newRecursionReceiver)
            {
                var result = new RecLayerDictbased<T>(newRecursionReceiver);
                result.L1MainConvert = L1MainConvert;
                result.L2MakeRepr = L2MakeRepr;
                result.L2XPairs2Repr = L2XPairs2Repr;
                result.L3MkPairs = L3MkPairs;
                result.L4Deconstruct = L4Deconstruct;
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
//             private Func<object, List<object>, string> RecursionPoint;
//             public string Recurse(T subject, List<object> visited) => this.RecursionPoint(subject, visited);
            
            protected RecursiveLayer<T, String> deconstruction;

            public virtual Option<Type> TypeCoverageFor(object o) => o is T t ? Some(typeof(T)) : None<Type>();

            public virtual string FormatAlreadySeen(object knowObj, List<object> visited) => "{...}";
            public virtual string FormatNull() => "<null>";

            protected Func<object, List<object>, string> fallback;

            public string Convert(object subject, List<object> visited)
            {
                if (TypeCoverageFor(subject).IsNone)
                {
                    return this.fallback.Invoke(subject, visited);
                }
                if (visited.Contains(subject)) return FormatAlreadySeen(subject, visited);
                visited.Add(subject);
                if (subject == null) { return FormatNull(); }
                return deconstruction.Convert(subject as dynamic, visited);
            }
            public string Convert(object subject) { return Convert(subject, new List<object>()); }
            public virtual string ConvertLease(Func<object, List<object>, string> callback, object subject, List<object> visited)
            {
                var lease = deconstruction.CloneForRecursion(callback);
                return lease.Convert(subject as dynamic, visited);
            }

            public TypedDeconstruction(Func<object, List<object>, string> fallback, RecursiveLayer<T, String> deconstruction) {
                this.fallback = fallback;
                this.deconstruction = deconstruction ?? new RecLayerDictbased<T>(this.Convert);
            }
        }

        public class SeqToStringDeconstruction : TypedDeconstruction<System.Collections.IEnumerable>
        {
            public SeqToStringDeconstruction(Func<object, List<object>, string> fallback, Func<object, List<object>, string> recursiveReceiver) : base(fallback, ObjToSeqLayer(recursiveReceiver)) { }

            public static RecLayerListbased<System.Collections.IEnumerable> ObjToSeqLayer(Func<object, List<object>, string> recursiveReceiver)
                => new RecLayerListbased<System.Collections.IEnumerable>(recursiveReceiver)
                {
                    L4Deconstruct = (subject, vs) =>
                    {
                        var r = new List<Object>(); foreach (var i in subject) r.Add(i); return r;
                    }
                };

            public override Option<Type> TypeCoverageFor(object o)
            {
                var valueType = o.GetType();
                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(valueType)) return Some(typeof(System.Collections.IEnumerable));
                return None<Type>();
            }
        }

        // TODO: arrays won't be subject to "already visited" checking when wrapped
        public class ArrayToStringDeconstruction : SeqToStringDeconstruction
        {
            public ArrayToStringDeconstruction(Func<object, List<object>, string> fallback, Func<object, List<object>, string> recursiveReceiver) : base(fallback, recursiveReceiver) { }

            public override string ConvertLease(Func<object, List<object>, string> callback, object subject, List<object> visited)
            {
                var lease = deconstruction.CloneForRecursion(callback);
                if (this.TypeCoverageFor(subject).IsNone)
                {
                    return this.fallback(subject, visited);
                }
                var me = (Array)subject;
                return lease.Convert(me.Cast<Object>(), visited);
            }

            public override Option<Type> TypeCoverageFor(object o)
            {
                var valueType = o.GetType();
                if (valueType.IsArray) return Some<Type>(valueType);
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
                var toSort = new List<IToStringDeconstruction>(Converters);
                var paired = toSort.ConvertAll(c => (c, c.TypeCoverageFor(subject)));
                paired.Sort( (x,y) => AggregateStrConverter.CompareSpecialization(x,y,subject) );
                paired.RemoveAll(p => p.Item2.IsNone);
                var result = paired.ConvertAll(p => (p.Item1, p.Item2.Get()));
                if (result.Count == 0) return None<(IToStringDeconstruction, Type)>();
                return Some(result[paired.Count - 1]);
            }

            public Option<Type> TypeCoverageFor(Object o) => GetMostSpecialized(o).Map(some => some.Item2);
            public string Convert(object subject, List<object> visited)
            {
                if (subject == null) return this.GetNullRepr();

                var conv = GetMostSpecialized(subject);
                if (conv.IsNone) return subject.ToString();
                var (converter, tpeOpt) = conv.Get();
                return converter.ConvertLease(Convert, subject, visited);
            }

            private string GetNullRepr()
            {
                return "<null>";
            }

            public static int CompareSpecialization((IToStringDeconstruction c, Option<Type> t) x, (IToStringDeconstruction c, Option<Type> t) y, object o)
            {
                if (x.t.IsNone && y.t.IsNone) return 0;
                if (x.t.IsNone) return -1;
                if (y.t.IsNone) return  1;
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
                this.Converters = new List<IToStringDeconstruction>(converters);
                this.fallback = new TypedDeconstruction<Object>(null, new RecLayerDictbased<Object>(this.Convert) { 
                    L1MainConvert = this.Convert
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

            public AggregateStrConverterInc WithAll(IEnumerable<IToStringDeconstruction> cs) { foreach (var c in cs) this.Converters.Add(c); return this; }
            public AggregateStrConverterInc With(IToStringDeconstruction c) { this.Converters.Add(c); return this; }
            public AggregateStrConverterInc WithType<T>() => With(new TypedDeconstruction<T>(this.fallback.Convert, new RecLayerDictbased<T>(this.Convert)) );
            public AggregateStrConverterInc WithType<T>(Func<T, List<object>, string> conversion) =>
                With(new TypedDeconstruction<T>(fallback.Convert, new RecLayerDictbased<T>(this.Convert) {
                    L1MainConvert = (t, vs) => conversion.Invoke(t, vs)
                }));
            public AggregateStrConverterInc WithType<T>(Func<T, string> conversion) =>
                With(new TypedDeconstruction<T>(fallback.Convert, new RecLayerDictbased<T>(this.Convert) { 
                    L1MainConvert = (t, vs) => conversion.Invoke(t) 
                }));

            public AggregateStrConverterInc WithRFormat<T>(string sep, string format, params Func<T, object>[] lenses)
                => With(RFormat(sep, format, lenses));
            public AggregateStrConverterInc WithRFormat<T>(string format, params Func<T, object>[] lenses)
                => With(RFormat(format, lenses));
            public AggregateStrConverterInc WithRFormat<T>(string sep, string format, params Func<T, object, List<object>>[] lenses)
                => With(RFormat(sep, format, lenses));
            public AggregateStrConverterInc WithRFormat<T>(string format, params Func<T, object, List<object>>[] lenses)
                => With(RFormat(format, lenses));

            public IToStringDeconstruction RFormat<T>(string format, params Func<T, object>[] lenses) => RFormat("{}", format, lenses);
            public IToStringDeconstruction RFormat<T>(string sep, string format, params Func<T, object>[] lenses)
            {
                var extractor = new RecLayerListbasedFormatting<T>(lenses);
                var converter = new TypedDeconstruction<T>(fallback.Convert, new RecLayerDictbased<T>(this.Convert) { 
                    L4Deconstruct = (t, vs) => extractor.DeconstructToDict(t, vs),
                    L2XPairs2Repr = (dict, t, vs) => extractor.Format(sep, format, dict)
                    }
                );
                return converter;
            }
            public IToStringDeconstruction RFormat<T>(string format, params Func<T, object, List<object>>[] lenses) => RFormat("{}", format, lenses);
            public IToStringDeconstruction RFormat<T>(string sep, string format, params Func<T, object, List<object>>[] lenses)
            {
                var extractor = new RecLayerListbasedFormatting<T>(lenses);
                return new TypedDeconstruction<T>(fallback.Convert, new RecLayerDictbased<T>(this.Convert)
                {
                    L4Deconstruct = extractor.DeconstructToDict,
                    L2XPairs2Repr = (dict, t, vs) => extractor.Format(sep, format, dict),
                });
            }
        }

        public static class Dynamic
        {
            public static String ObjKVToStr(String name, Dictionary<String,String> kvdict)
            {
                StringBuilder b = new StringBuilder();
                b.Append(name + "{");
                var i = 0;
                foreach (var kv in kvdict)
                {
                    b.Append($"{kv.Key}:{kv.Value}");
                    if (i < kvdict.Count - 1) b.Append(", ");
                    i++;
                }
                b.Append("}");
                return b.ToString();
            }
            internal static List<object> MemberList<T>(T subject)
            {
                Type tpe = GetType(subject);
                List<object> result = new List<object>();
                foreach (var info in tpe.GetProperties())
                {
                    object value = info.GetValue(subject);
                    result.Add(value);
                }
                foreach (var info in tpe.GetFields())
                {
                    object value = info.GetValue(subject);
                    result.Add(info.Name);
                }
                return result;
            }
            public static Dictionary<String, Object> MemberDict(object o)
            {
                Type tpe = GetType(o);
                Dictionary<string, object> result = new Dictionary<string, object>();
                foreach (var info in tpe.GetProperties())
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
                foreach (var info in tpe.GetFields())
                {
                    object value = info.GetValue(o);
                    result.Add(info.Name, value);
                }
                return result;
            }
            public static Type GetType(object o) => o?.GetType();

        }
    }
}
