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

namespace CrypTool.LFSR.Utils
{
    public class Option<T>
    {
        internal readonly T value;
        public bool IsNone { get; }
        public bool IsSome { get; }
        internal Option(T value, bool isNone)
        {
            this.value = value;
            IsNone = isNone;
            IsSome = !isNone;
        }
        public M Match<M>(Func<T, M> Some, Func<M> None)
        {
            if (!IsNone)
            {
                return Some(value);
            }
            else
            {
                return None();
            }
        }
        public void Match(Action<T> Some, Action None)
        {
            if (!IsNone)
            {
                Some(value);
            }
            else
            {
                None();
            }
        }
        public T IfNone(Func<T> None)
        {
            return Match(value => value, None);
        }
        public T IfNone(T None)
        {
            return Match(value => value, () => None);
        }

        public Option<T2> Map<T2>(Func<T, T2> f)
        {
            if (IsSome)
            {
                return Option<T2>.Some(f.Invoke(value));
            }
            return Option<T2>.None();
        }
        internal static T checkNull(T value)
        {

            if (value == null)
            {
                throw new Exception("Option cannot be Some(null)");
            }
            return value;
        }

        // --- static API
        public static Option<T> Some(T value)
        {
            return new Option<T>(checkNull(value), false);
        }
        public static Option<T> None()
        {
            return new Option<T>(default(T), true);
        }

        public T Get()
        {
            if (IsNone)
            {
                throw new Exception("Get() invoked on None");
            }
            return value;
        }
        public string ToStringWithR(Func<T, string> rts)
        {
            return Match(some => $"Some({rts.Invoke(some)})", () => "None");
        }
        public override string ToString()
        {
            return ToStringWithR((some) => some.ToString());
        }
    }
    public class Either<L, R>
    {
        internal readonly Option<R> right;
        internal readonly Option<L> left;
        public bool IsLeft => left.IsSome;
        public bool IsRight => right.IsSome;

        public string ToStringRecursive(Func<dynamic, string> toString)
        {
            return Match(right => $"Right({toString.Invoke(right)})", left => left.ToString());
        }
        public override string ToString()
        {
            return ToStringRecursive((r) => r.ToString());
        }

        private Either(Option<L> left, Option<R> right)
        {
            if (!(left.IsSome ^ right.IsSome))
            {
                throw new Exception("Either cannot be both Left and Right");
            }
            this.left = left;
            this.right = right;
        }
        public void Match(Action<R> R, Action<L> L)
        {
            if (IsRight)
            {
                R.Invoke(right.Get());
                return;
            }
            L.Invoke(left.Get());
        }
        public Ret Match<Ret>(Func<R, Ret> R, Func<L, Ret> L)
        {
            return right.Map(R).IfNone(() => L.Invoke(left.Get()));
        }

        public static Either<L, R> Left(L left)
        {
            return new Either<L, R>(Option<L>.Some(left), Option<R>.None());
        }

        public static Either<L, R> Right(R right)
        {
            return new Either<L, R>(Option<L>.None(), Option<R>.Some(right));
        }

        public Either<L, R2> Map<R2>(Func<R, R2> f)
        {
            return new Either<L, R2>(left, right.Map(f));
        }

        public Either<L2, R> MapLeft<L2>(Func<L, L2> f)
        {
            return new Either<L2, R>(left.Map(f), right);
        }

        public Option<R> ToOption()
        {
            return right;
        }

        public R IfLeft(R rightVal)
        {
            return right.IfNone(rightVal);
        }

        public L IfRight(L leftVal)
        {
            return left.IfNone(leftVal);
        }

        // This method is just so it looks nicer
        public R OrIfError(R rightVal)
        {
            return IfLeft(rightVal);
        }

        public L GetLeft()
        {
            if (IsRight)
            {
                throw new Exception("GetLeft called on Right Either");
            }
            return left.Get();
        }
        public R GetRight()
        {
            if (IsLeft)
            {
                throw new Exception("GetLeft called on Right Either");
            }
            return right.Get();
        }
    }

    // Wrapped values with event infrastructure
    public interface IStoredValue<T>
    {
        T Value { get; set; }
    }
    public class Box<T> : IStoredValue<T>
    {
        public Action<T> OnChange { get; set; } = (val) => { };
        public Action<T, T> OnChangeFromTo { get; set; } = (old, newVal) => { };
        private T _value;

        public T Value
        {
            get => _value; set => SetValue(value);
        }
        public Box(T initial) { _value = initial; }
        private void SetValue(T newVal) { T oldVal = Value; _value = newVal; OnChange(newVal); OnChangeFromTo(oldVal, newVal); }
    }
    public class HistoryBox<T> : IStoredValue<T>
    {
        public Action<T> OnChange { get; set; } = (val) => { };
        public List<T> History = new List<T>();

        public void Record(T val)
        {
            Value = val;
        }

        public T Value { get { if (History.Count == 0) { throw new Exception("No value recorded in this history"); } return History[History.Count - 1]; } set => SetValue(value); }
        public T Last => Value;
        public HistoryBox() { History = new List<T>(); }
        public HistoryBox(T initial)
        {
            History = new List<T>
                {
                    initial
                };
        }
        private void SetValue(T newVal) { History.Add(newVal); OnChange(newVal); }
    }

    public static class Datatypes
    {
        public static Option<T> OptionNullable<T>(T value)
        {
            return value == null ? Option<T>.None() : Option<T>.Some(value);
        }

        public static Option<T> Some<T>(T value)
        {
            return Option<T>.Some(value);
        }

        public static Option<T> None<T>()
        {
            return Option<T>.None();
        }

        public static Either<L, R> Left<L, R>(L left)
        {
            return Either<L, R>.Left(left);
        }

        public static Either<L, R> Right<L, R>(R right)
        {
            return Either<L, R>.Right(right);
        }

        public static List<T> Sequence<T>(params T[] list)
        {
            return new List<T>(list);
        }

        public static Option<V> GetOpt<K, V>(this Dictionary<K, V> dict, K key)
        {
            bool hadVal = dict.TryGetValue(key, out V val);
            return hadVal ? Some(val) : None<V>();
        }
        public static V GetOr<K, V>(this Dictionary<K, V> dict, K key, V def)
        {
            return dict.GetOpt(key).IfNone(def);
        }

    }
}

