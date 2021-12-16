/*
   Copyright 2008 Timm Korte, University of Siegen

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
using System.Windows;
using System.Windows.Media.Animation;

namespace CrypTool.PRESENT
{
    public class CosDoubleAnimation : DoubleAnimationBase
    {
        public static DependencyProperty FromProperty = DependencyProperty.Register("From", typeof(double), typeof(CosDoubleAnimation));
        public static DependencyProperty ToProperty = DependencyProperty.Register("To", typeof(double), typeof(CosDoubleAnimation));
        public static DependencyProperty ScaleProperty = DependencyProperty.Register("Scale", typeof(double), typeof(CosDoubleAnimation));

        protected override Freezable CreateInstanceCore()
        {
            return new CosDoubleAnimation();
        }

        protected override double GetCurrentValueCore(double defaultOriginValue, double defaultDestinationValue, AnimationClock animationClock)
        {
            double timeFraction = animationClock.CurrentProgress.Value;
            double rad = From + timeFraction * (To - From);
            return Math.Cos(rad) * Scale;
        }

        public double From
        {
            get => (double)base.GetValue(FromProperty);
            set => base.SetValue(FromProperty, value);
        }

        public double To
        {
            get => (double)base.GetValue(ToProperty);
            set => base.SetValue(ToProperty, value);
        }

        public double Scale
        {
            get => (double)base.GetValue(ScaleProperty);
            set => base.SetValue(ScaleProperty, value);
        }
    }

    public class RoundCosDoubleAnimation : DoubleAnimationBase
    {
        public static DependencyProperty FromProperty = DependencyProperty.Register("From", typeof(double), typeof(RoundCosDoubleAnimation));
        public static DependencyProperty ToProperty = DependencyProperty.Register("To", typeof(double), typeof(RoundCosDoubleAnimation));
        public static DependencyProperty ScaleProperty = DependencyProperty.Register("Scale", typeof(double), typeof(RoundCosDoubleAnimation));

        protected override Freezable CreateInstanceCore()
        {
            return new RoundCosDoubleAnimation();
        }

        protected override double GetCurrentValueCore(double defaultOriginValue, double defaultDestinationValue, AnimationClock animationClock)
        {
            double timeFraction = animationClock.CurrentProgress.Value;
            double rad = From + timeFraction * (To - From);
            double newpos = Math.Cos(rad);
            return (newpos - Math.Cos(From)) * Scale;
        }

        public double From {
            get => (double)base.GetValue(FromProperty);
            set => base.SetValue(FromProperty, value);
        }

        public double To {
            get => (double)base.GetValue(ToProperty);
            set => base.SetValue(ToProperty, value);
        }

        public double Scale {
            get => (double)base.GetValue(ScaleProperty);
            set => base.SetValue(ScaleProperty, value);
        }
    }

    public class RoundSinDoubleAnimation : DoubleAnimationBase
    {
        public static DependencyProperty FromProperty = DependencyProperty.Register("From", typeof(double), typeof(RoundSinDoubleAnimation));
        public static DependencyProperty ToProperty = DependencyProperty.Register("To", typeof(double), typeof(RoundSinDoubleAnimation));
        public static DependencyProperty ScaleProperty = DependencyProperty.Register("Scale", typeof(double), typeof(RoundSinDoubleAnimation));

        protected override Freezable CreateInstanceCore()
        {
            return new RoundSinDoubleAnimation();
        }

        protected override double GetCurrentValueCore(double defaultOriginValue, double defaultDestinationValue, AnimationClock animationClock)
        {
            double timeFraction = animationClock.CurrentProgress.Value;
            double rad = From + timeFraction * (To - From);
            double newpos = Math.Sin(rad);
            return (newpos - Math.Sin(From)) * Scale;
        }

        public double From {
            get => (double)base.GetValue(FromProperty);
            set => base.SetValue(FromProperty, value);
        }

        public double To {
            get => (double)base.GetValue(ToProperty);
            set => base.SetValue(ToProperty, value);
        }

        public double Scale {
            get => (double)base.GetValue(ScaleProperty);
            set => base.SetValue(ScaleProperty, value);
        }
    }
}
